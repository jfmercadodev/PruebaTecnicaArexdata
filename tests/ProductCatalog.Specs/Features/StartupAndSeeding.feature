Feature: Startup and seeding
  Containerized startup must prepare SQL Server and seed catalog safely.

  Scenario: BDD-016 Start app against SQL Server container
    Given docker compose starts services "web" and "sqlserver"
    When SQL Server healthcheck becomes healthy
    Then web application connects using configured connection string
    And pending migrations are applied

  Scenario: BDD-017 Seed catalog when database is empty
    Given SQL Server database is empty
    When application startup runs seeder
    Then at least 20 products are created
    And seeded products include varied sku, price, cost and stock values

  Scenario: BDD-018 Avoid duplicate seeding on restart
    Given seeded database already contains products
    When web container restarts
    Then seeder does not duplicate products
