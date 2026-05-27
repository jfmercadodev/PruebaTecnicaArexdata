Feature: Product listing
  Listing must support filters, sorting, paging and cache visibility.

  Scenario: BDD-008 List products with paging and sorting
    Given catalog contains 25 products
    When client requests page 2 with page size 10 sorted by "Name" ascending
    Then response contains 10 products
    And response metadata shows page number 2 and page size 10
    And products are sorted by "Name" ascending

  Scenario: BDD-009 Filter products by name or sku
    Given catalog contains product "Mechanical Keyboard" with sku "MKB-001"
    And catalog contains product "Gaming Headset" with sku "GHS-002"
    When client filters listing by term "MKB"
    Then response contains only product "Mechanical Keyboard"

  Scenario: BDD-010 Show cache source metadata
    Given query cache contains result for listing key "products:p1:s10:name:asc:mkb"
    When client requests listing with same query key
    Then response metadata contains cache hit true
    And response metadata contains source "Cache"

  Scenario: BDD-011 Invalidate cache after write
    Given cached listing exists for key "products:p1:s10:name:asc:all"
    When client updates product "MKB-001"
    Then cached listing key "products:p1:s10:name:asc:all" is invalidated after commit

  Scenario: BDD-020 Render catalog page with list controls and metadata
    Given seeded catalog exists
    When user opens page "/products"
    Then page shows search input and paged table
    And page shows metadata source "Database" or "Cache"
    And page shows elapsed response time in ms
