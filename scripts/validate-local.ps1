$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

function Invoke-Step {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Label,
        [Parameter(Mandatory = $true)]
        [scriptblock]$Action
    )

    Write-Host ">> $Label" -ForegroundColor Cyan
    & $Action

    if (-not $?) {
        throw "Command failed: $Label"
    }
}

Invoke-Step "dotnet build ProductCatalog.slnx" {
    dotnet build ProductCatalog.slnx
}

Invoke-Step "dotnet test arch" {
    dotnet test tests/ProductCatalog.ArchTests/ProductCatalog.ArchTests.csproj --no-build --no-restore
}

Invoke-Step "dotnet test unit" {
    dotnet test tests/ProductCatalog.UnitTests/ProductCatalog.UnitTests.csproj --no-build --no-restore
}

Invoke-Step "dotnet test integration" {
    dotnet test tests/ProductCatalog.IntegrationTests/ProductCatalog.IntegrationTests.csproj --no-build --no-restore -m:1
}

Invoke-Step "dotnet test specs" {
    dotnet test tests/ProductCatalog.Specs/ProductCatalog.Specs.csproj --no-build --no-restore -m:1
}

Invoke-Step "dotnet test solution" {
    dotnet test ProductCatalog.slnx --no-build --no-restore -m:1
}

Write-Host "Local validation finished OK." -ForegroundColor Green
