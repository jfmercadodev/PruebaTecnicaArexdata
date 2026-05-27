Feature: Problem details and controllers
  Controllers must expose full HTTP contract with RFC 7807 error payloads.

  Scenario: BDD-012 Return 422 for domain invariant violation
    Given products controller is available
    When client sends create product request with sale price 40 and cost 55
    Then response status code is 422
    And response content type is "application/problem+json"
    And response body title contains "Invalid price"

  Scenario: BDD-013 Return 404 for missing product
    Given products controller is available
    When client requests product detail for unknown id
    Then response status code is 404
    And response body title contains "Product not found"

  Scenario: BDD-014 Return 400 for validation error
    Given products controller is available
    When client sends create product request with empty name and empty sku
    Then response status code is 400
    And response body title contains "Validation failed"

  Scenario: BDD-015 Expose controller endpoints without minimal APIs
    Given application starts
    When route table is inspected
    Then controller route "api/products" exists
    And no product endpoint depends on Minimal API handlers

  Scenario: BDD-019 Propagate correlation id through response and errors
    Given products controller is available
    When client sends request with header "X-Correlation-Id"
    Then response contains same "X-Correlation-Id" header
    And problem details body exposes same correlation id when request fails

  Scenario: BDD-023 Show technical detail in Development error boundary
    Given interactive page throws while rendering
    And application runs in "Development"
    When user stays on current screen
    Then boundary shows technical exception detail

  Scenario: BDD-024 Show friendly message in Production error boundary
    Given interactive page throws while rendering
    And application runs in "Production"
    When user stays on current screen
    Then boundary shows friendly message and correlation id

  Scenario: BDD-025 Read API problem title in UI and expose detail only in Development
    Given API operation fails with problem title "Product not found"
    And problem detail text exists in backend exception
    When Blazor page renders error banner
    Then UI shows problem title
    And UI shows problem detail only in "Development"
