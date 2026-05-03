# Bunnings Sizzling Hot Products API

## Project Overview

This project is an ASP.NET Core Web API that calculates the top "sizzling hot" Bunnings product for recent sales activity.

The application reads order and product data from JSON input files, applies the required business rules, and returns the top product for each of the last three days plus the combined three-day period.

The problem it solves is identifying which Bunnings product is performing best over the target reporting window while handling real-world ordering concerns such as cancellations, duplicate product entries, and deterministic tie-breaking.

## Tech Stack

| Technology               | Purpose                                                                                       |
| ------------------------ | --------------------------------------------------------------------------------------------- |
| .NET 10                  | Provides the runtime and framework for the API.                                               |
| ASP.NET Core Minimal API | Keeps the HTTP surface small and focused for a single read-only endpoint.                     |
| C#                       | Used for strongly typed domain models, service logic, and clear business rule implementation. |
| System.Text.Json         | Reads `orders.json` and `products.json` using the built-in .NET JSON serializer.              |
| Dependency Injection     | Registers services such as `JsonOrderRepository` and `SizzlingHotProductService` cleanly.     |
| Swagger / Swashbuckle    | Provides interactive API documentation for local testing and inspection.                      |

## Architecture & Design Decisions

The application is intentionally small, but the core logic is separated into clear responsibilities inside `SizzlingHotProductService`.

### Data Organisation Layer

The data organisation layer prepares the order data before the business calculation runs.

- `GetValidCompletedOrders` removes cancelled orders and returns only completed orders that should contribute to the result.
- `GroupByDate` groups the valid completed orders by their order date.
- `GetOrdersForPeriod` retrieves all orders that fall within a requested date range.

This keeps filtering and retrieval concerns separate from product ranking logic.

### Business Logic Layer

`CalculateTopProduct` contains the core business calculation.

It counts valid product sales, applies per-day deduplication, resolves the winning product, and applies the alphabetical tie-breaker when multiple products have the same count.

### Helpers

`ParseDate` and `FormatDate` centralise date parsing and formatting.

This avoids scattering date format assumptions across the service and makes the expected `dd/MM/yyyy` format explicit.

### Grouping Performance

Orders are grouped by date once upfront using `GroupByDate`.

This is more efficient than scanning the entire order list separately for each reporting period. The upfront grouping is an `O(n)` operation, after which each period can work from the grouped structure. Without this step, the application would risk repeated scans approaching `O(n x periods)` as more reporting windows are added.

### SOLID Principles

The implementation follows the Single Responsibility Principle by separating input reading, data preparation, business calculation, and result formatting concerns.

- `JsonOrderRepository` is responsible for reading JSON input files.
- `SizzlingHotProductService` is responsible for organising orders and calculating results.
- Model classes represent the shape of orders, products, order entries, and API results.

## Business Rules

### Cancellations

Cancellations are pre-processed before orders are grouped by date.

`GetValidCompletedOrders` first collects cancelled order IDs, then excludes completed orders whose `orderId` appears in that cancelled set. This ensures cancelled sales do not contribute to daily or period-level results.

### Deduplication

Deduplication is based on this sale key:

```text
customerId|productId|date
```

This means the same customer buying the same product multiple times on the same day is counted once for that product on that day.

Within a single order, duplicate product entries are also collapsed by product ID before counting.

### Tiebreaker

If multiple products have the same sales count, the winner is selected alphabetically by product name.

This makes the result deterministic and avoids inconsistent output when counts are tied.

## Assumptions

- Today's date is assumed to be `23/04/2026`.
- `orders.json` is assumed to be pre-filtered to the target date range, plus any cancellations referencing orders within that range regardless of when the cancellation occurred.
- Deduplication is per day, not per period.
- Cancellations are credited against the original order's date, not the cancellation date.

## Getting Started

### Prerequisites

- .NET 10 SDK
- A terminal or IDE that can run .NET projects

### Install

Restore the project dependencies:

```bash
dotnet restore
```

### Run the API

Start the API from the project directory:

```bash
dotnet run
```

The API exposes the following endpoint:

```text
GET /api/sizzling-hot-products
```

Swagger UI is enabled and can be used to inspect and call the endpoint locally.

### Run Unit Tests

Run tests from the solution directory:

```bash
dotnet test
```

The solution includes a dedicated `Bunnings.SizzlingHotProducts.Tests` project. `dotnet test` will discover and run all tests automatically.

## Expected Outcomes

| Type     | Date or Period            | Top Sizzling Hot Product                           |
| -------- | ------------------------- | -------------------------------------------------- |
| `single` | `21/04/2026`              | Ezy Storage 37L Flexi Laundry Basket - White       |
| `single` | `22/04/2026`              | Ezy Storage 37L Flexi Laundry Basket - White       |
| `single` | `23/04/2026`              | Arlec 160W Crystalline Solar Foldable Charging Kit |
| `range`  | `21/04/2026 - 23/04/2026` | Ezy Storage 37L Flexi Laundry Basket - White       |

## Future Improvements

- For larger datasets, daily counts could be cached and summed for range calculations rather than re-processing the underlying orders.
- Cancellation lookup could be extended to fetch cancellations outside the date window by `orderId` for production use.
- A dedicated unit test project could be added to cover cancellation handling, deduplication, date range selection, and alphabetical tie-breaking.
