# 🚀 Order Service — Technical Test (Lead-Level Implementation)

## 📌 Overview

This project is an end-to-end implementation of an **Order Management Service**.

The system provides:

- Creating orders  
- Retrieving all orders  
- Filtering orders by status  
- Retrieving order details  
- Updating order status with strict transition rules  
- Calculating monthly profit  
- Full validation, logging, status normalization, and structured error handling  

---

## 🏗 Architecture

The solution is split into clear layers:

```text
Order.WebAPI         → REST API, routing, middleware, controllers
Order.Service        → business logic, domain rules, status handling
Order.Data           → EF Core DbContext, entities, repositories
Order.Model          → DTOs, request models
Order.Service.Tests  → unit tests
```

### Design principles

- Separation of concerns between Web/API, business logic, and data access
- Dependency Injection for all services and repositories
- Domain-level exceptions for domain errors
- FluentValidation for request validation
- Centralized status normalization and transition rules
- Consistent, typed API responses
- Repositories using `AsNoTracking()` for read-only queries
- Unit tests running against a real EF Core pipeline (SQLite In-Memory)

---

## 📦 Features

### 1. Order Creation

Create a new order with one or more items.

- Validates:
  - reseller/customer IDs
  - non-empty item list
  - positive quantities
- Returns full order details (including totals and status).

### 2. Get All Orders

Retrieve all existing orders:

```http
GET /orders
```

Supports filtering by status:

```http
GET /orders?status=in_progress
```

The status is normalized via a dedicated status normalizer, so inputs like:

- `in_progress`
- `In Progress`
- `INPROGRESS`
- `in progress`

are all mapped to the canonical `"In Progress"` status.

### 3. Get Order by ID

```http
GET /orders/{orderId}
```

Returns:

- order header information
- status
- items
- total cost and total price

### 4. Update Order Status

```http
PUT /orders/{orderId}/status
```

Request body:

```json
{
  "status": "In Progress"
}
```

Status transitions are validated by a centralized rule set:

| Current Status | Allowed Next Statuses     |
|----------------|---------------------------|
| Created        | In Progress, Failed       |
| In Progress    | Completed, Failed         |
| Failed         | In Progress               |
| Completed      | (no further transitions)  |

Invalid transitions throw a domain exception and result in a structured 409 Conflict response.

### 5. Monthly Profit

```http
GET /orders/profit-by-month
```

Returns a list of monthly profit aggregates, e.g.:

```json
[
  {
    "year": 2024,
    "month": 2,
    "profit": 123.45
  }
]
```

---

## 🧠 Status Management (Lead-Level Design)

### Centralized Status Definitions

`OrderStatuses` defines the canonical statuses in a single place, ensuring no magic strings are spread across the codebase.

### Status Normalizer

`OrderStatusNormalizer` is responsible for:

- mapping aliases / different formats to canonical values
- throwing clear errors for unknown statuses

This keeps input handling robust and predictable.

### Status Transition Rules

`OrderStatusTransitions` encapsulates the valid transitions and throws `InvalidOrderStatusTransitionException` when the rules are violated.

This makes the domain rules explicit, testable, and easy to extend.

---

## ⚙️ Error Handling

A custom `ErrorHandlingMiddleware` converts exceptions into **RFC 7807**-style `application/problem+json` responses.

Example mappings:

- `OrderNotFoundException` → HTTP 404 + problem JSON  
- `InvalidOrderStatusTransitionException` → HTTP 409 + problem JSON  
- `ArgumentException` (invalid input) → HTTP 400  
- Any other unhandled exception → HTTP 500 with generic message  

This gives clients a consistent, machine-readable error contract.

---

## ✅ Validation

Validation is implemented using **FluentValidation** for request models such as:

- `CreateOrderRequest`
- `UpdateOrderStatusRequest`

Typical rules include:

- required IDs
- non-empty items collection
- positive quantities

Invalid requests result in a standard validation response with detailed error messages.

---

## 🧪 Testing

Unit tests are implemented in the `Order.Service.Tests` project.

Characteristics:

- EF Core with SQLite In-Memory is used for realistic behavior  
- Real `OrderContext` and `OrderRepository` are used  
- Tests cover:
  - retrieving orders
  - totals (cost/price) calculations
  - retrieving by ID
  - item counts
  - reference data (statuses, products, services)

Run tests with:

```bash
dotnet test
```

---

## 🛠 Technologies

- .NET (ASP.NET Core Web API)
- Entity Framework Core
- SQLite In-Memory for tests
- FluentValidation
- NUnit
- Dependency Injection
- Structured logging
- ProblemDetails (application/problem+json)

---

## 📁 Project Structure

```text
src/
  Order.WebAPI/
    Controllers/
    Middleware/

  Order.Service/
    Interfaces/
    Implementation/
    Exceptions/
    Status/          # status constants, normalizer, transitions

  Order.Data/
    Context/
    Entities/
    Repositories/

  Order.Model/
    DTOs/
    Requests/

tests/
  Order.Service.Tests/
```

---

## ▶️ Running the Application

1. Restore and build:

```bash
dotnet restore
dotnet build
```

2. Run the API (from the WebAPI project):

```bash
dotnet run
```

3. The API will typically be available at:

```text
https://localhost:{port}/orders
```

You can then use Postman, curl, or a browser (for GET endpoints) to interact with the service.

---

## 🏁 Conclusion

This implementation:

- Covers the full flow from data to API  
- Encapsulates business rules in the domain layer  
- Uses proper validation and error handling  
- Implements realistic status management with normalization and transitions  
- Is fully testable and structured for maintainability  


Ganjali Imanov © 2025 
