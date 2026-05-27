Feature: Product update
  Product update must keep aggregate consistent and publish one representative event.

  Scenario: BDD-005 Update price and stock successfully
    Given product "MKB-001" exists with sale price 120, cost 70 and stock 15
    When client updates product "MKB-001" with sale price 135, cost 80 and stock delta -2
    Then product has sale price 135, cost 80 and stock 13
    And single domain event "ProductUpdated" is pending publication

  Scenario: BDD-006 Roll back invalid composite update
    Given product "MKB-001" exists with sale price 120, cost 70 and stock 15
    When composite update changes stock delta -4 and sale price 60 with cost 80
    Then request fails with domain error "InvalidPriceException"
    And product keeps sale price 120, cost 70 and stock 15
    And no partial state is persisted

  Scenario: BDD-007 Reject stock below zero
    Given product "MKB-001" exists with stock 2
    When client adjusts stock by -3
    Then request fails with domain error "InvalidStockException"
    And product stock remains 2

  Scenario: BDD-022 Render edit form with original snapshot
    Given product "MKB-001" exists with sale price 120, cost 70 and stock 15
    When user opens page "/products/{id}/edit"
    Then page shows original sale, cost and stock snapshot
    And save submit stays disabled until user changes editable values
