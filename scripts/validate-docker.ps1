$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

if (Test-Path Variable:PSNativeCommandUseErrorActionPreference) {
    $PSNativeCommandUseErrorActionPreference = $false
}

function Get-DockerCliPath {
    $dockerCommand = Get-Command docker -ErrorAction SilentlyContinue
    if ($dockerCommand) {
        return $dockerCommand.Source
    }

    $knownPaths = @(
        "C:\Program Files\Docker\Docker\resources\bin\docker.exe",
        "C:\Program Files\Docker\Docker\resources\docker.exe"
    )

    foreach ($path in $knownPaths) {
        if (Test-Path $path) {
            return $path
        }
    }

    throw "Docker CLI not found. Install Docker Desktop first."
}

function Get-DockerServiceStatus {
    $service = Get-Service com.docker.service -ErrorAction SilentlyContinue
    if (-not $service) {
        return "missing"
    }

    return $service.Status.ToString()
}

function Test-IsElevated {
    $identity = [Security.Principal.WindowsIdentity]::GetCurrent()
    $principal = [Security.Principal.WindowsPrincipal]::new($identity)
    return $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
}

$dockerCli = Get-DockerCliPath

$null = & $dockerCli info *> $null
if ($LASTEXITCODE -ne 0) {
    $serviceStatus = Get-DockerServiceStatus
    $isElevated = Test-IsElevated
    throw "Docker daemon not reachable. Service com.docker.service status: $serviceStatus. Elevated shell: $isElevated. Start Docker Desktop or service with admin privileges, then retry. CLI: $dockerCli"
}

$appPort = if ($env:APP_PORT) { $env:APP_PORT } else { "8080" }
$appUrl = "http://localhost:$appPort/products"
$defaultSqlServerImage = "mcr.microsoft.com/mssql/server:2022-latest"
$localSqlServerImage = "productcatalog-sqlserver-local:2022"

if (-not $env:SQL_SERVER_IMAGE) {
    $null = & $dockerCli image inspect $defaultSqlServerImage *> $null
    if ($LASTEXITCODE -eq 0) {
        $null = & $dockerCli image tag $defaultSqlServerImage $localSqlServerImage 2>$null
        if ($LASTEXITCODE -eq 0) {
            $env:SQL_SERVER_IMAGE = $localSqlServerImage
            $env:SQL_SERVER_PULL_POLICY = "never"
            Write-Host ">> using local SQL Server image alias $localSqlServerImage" -ForegroundColor Cyan
        }
    }
}

Write-Host ">> docker compose up -d --build" -ForegroundColor Cyan
$previousErrorActionPreference = $ErrorActionPreference
$ErrorActionPreference = "Continue"
try {
    $composeOutput = & $dockerCli compose up -d --build 2>&1
    $composeExitCode = $LASTEXITCODE
}
finally {
    $ErrorActionPreference = $previousErrorActionPreference
}
$composeOutput | Write-Host

if ($composeExitCode -ne 0) {
    $composeText = ($composeOutput | Out-String)

    if ($composeText -match "input/output error" -or $composeText -match "unable to get image") {
        throw "docker compose up failed due Docker Desktop image/blob I/O error while pulling SQL Server image. Compose file is valid; fix local Docker image store and retry."
    }

    throw "docker compose up failed."
}

$deadline = (Get-Date).AddMinutes(2)
$healthy = $false

while ((Get-Date) -lt $deadline) {
    try {
        $response = Invoke-WebRequest -Uri $appUrl -UseBasicParsing -TimeoutSec 10
        if ($response.StatusCode -eq 200) {
            $healthy = $true
            break
        }
    }
    catch {
        Start-Sleep -Seconds 5
    }
}

Write-Host ">> docker compose ps" -ForegroundColor Cyan
& $dockerCli compose ps

if (-not $healthy) {
    throw "App not reachable at $appUrl after compose startup."
}

Write-Host "Docker validation finished OK. App reachable at $appUrl" -ForegroundColor Green
