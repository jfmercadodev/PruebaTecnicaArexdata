Feature: Product creation
  Product creation must preserve domain invariants, idempotency and event consistency.

  Scenario: BDD-001 Create valid product
    Given product catalog is empty
    When client creates product with name "Mechanical Keyboard", sku "mkb--001", sale price 120, cost 70 and stock 15
    Then product is stored with normalized sku "MKB-001"
    And product margin percent is 41.6667
    And single domain event "ProductCreated" is pending publication

  Scenario: BDD-002 Reject sale price below cost
    Given product catalog is empty
    When client creates product with name "Gaming Mouse", sku "gm-001", sale price 40, cost 55 and stock 10
    Then request fails with domain error "InvalidPriceException"
    And no product is stored

  Scenario: BDD-003 Reject duplicate sku
    Given product exists with sku "MKB-001"
    When client creates product with name "Keyboard Clone", sku "mkb-001", sale price 100, cost 50 and stock 3
    Then request fails with conflict error
    And response status code is 409

  Scenario: BDD-004 Reuse idempotent response
    Given idempotency window is active
    And request id "7d27434f-8e25-4e12-9004-b0dfcf4c0f00" already created product "MKB-001"
    When client repeats create product command with same request id
    Then previous response is returned
    And create handler is not executed again

  Scenario: BDD-021 Block invalid create form until SKU validation passes
    Given product exists with sku "MKB-001"
    When user opens page "/products/new"
    And user types sku "mkb-001"
    And debounce window of 350 ms completes
    Then page shows validation message "SKU already exists."
    And create submit remains disabled until form becomes valid
