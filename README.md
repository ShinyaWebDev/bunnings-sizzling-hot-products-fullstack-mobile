# Bunnings Sizzling Hot Products

Professional full stack solution for the Bunnings Sizzling Hot Products take-home assignment.

## Project Overview

This project calculates "sizzling hot products" for a supplied order dataset. A sizzling hot product is the top selling product for each day and across a 3 day reporting period.

The solution is intentionally split into a clear backend and frontend boundary:

- **Backend:** .NET 10 ASP.NET Core Web API
- **Frontend:** React Native / Expo mobile app
- **Tests:** xUnit test project for backend business rules

The ASP.NET backend owns all business rules, date handling, aggregation, deduplication, cancellation handling, and top-product calculations.

The React Native frontend is intentionally presentation-focused. It fetches calculated results from the API and renders them in a clean mobile view with no business logic duplicated in the app.

## Tech Stack

### Backend

- **.NET 10**
- **ASP.NET Core**
- **C#**

This stack provides a strongly typed, maintainable backend suited to clean architecture, SOLID principles, and explicit business-rule modelling.

### Frontend

- **React Native**
- **Expo**

React Native and Expo provide a fast, cross-platform mobile development workflow with minimal setup and dependencies.

### Testing

- **xUnit**

xUnit is an industry standard testing framework for .NET and is well suited to focused unit tests around deterministic business rules.

## Project Structure

```text
bunnings-sizzling-hot-products-fullstack-mobile/
├── backend/
│   ├── Bunnings.SizzlingHotProducts.Api/        ← .NET 10 Web API
│   └── Bunnings.SizzlingHotProducts.Tests/      ← xUnit test project
└── frontend/
    └── sizzling-hot-products-mobile/            ← React Native / Expo app
```

- `backend/Bunnings.SizzlingHotProducts.Api/` contains the ASP.NET Core Web API, repository implementation, domain logic, API controllers, and JSON-backed data access.
- `backend/Bunnings.SizzlingHotProducts.Tests/` contains xUnit tests covering the backend business rules without relying on the file system.
- `frontend/sizzling-hot-products-mobile/` contains the Expo React Native app that calls the backend API and presents the calculated sizzling hot products.

## Getting Started

### Prerequisites

- .NET 10 SDK
- Node.js
- Expo Go app on a mobile device, or an iOS/Android simulator

### Running the Backend

```bash
cd backend
dotnet run --project Bunnings.SizzlingHotProducts.Api/Bunnings.SizzlingHotProducts.Api.csproj
```

The API runs at:

```text
http://localhost:5291
```

Swagger UI is available at:

```text
http://localhost:5291/swagger
```

### Running the Frontend

```bash
cd frontend/sizzling-hot-products-mobile
npm install
npm start
```

Then choose one of the available Expo options from the terminal menu:

- iOS simulator
- Android emulator
- Expo Go on a real device by scanning the QR code

### Configure Backend URL

The API base URL is centralised in:

```text
frontend/sizzling-hot-products-mobile/src/config/apiConfig.ts
```

```ts
export const API_BASE_URL = "http://localhost:5291";
```

Use the appropriate base URL for the target runtime:

- iOS simulator: `http://localhost:5291`
- Android emulator: `http://10.0.2.2:5291`
- Real device: `http://192.168.x.x:5291` using your local network IP

For real device testing, run the backend with:

```bash
dotnet run --urls http://0.0.0.0:5291
```

### Running Tests

```bash
cd backend
dotnet test Bunnings.SizzlingHotProducts.Api/Bunnings.SizzlingHotProducts.Api.sln
```

Expected result:

```text
total: 11, failed: 0, succeeded: 11
```

## Architecture & Design Decisions

### Backend Layers

The backend separates data organisation from business-rule calculation.

#### Data Organisation Layer

- `GetValidCompletedOrders` removes cancelled orders upfront before any grouping.
- `GroupByDate` groups orders once into a dictionary, giving an `O(n)` organisation step instead of repeatedly scanning orders for each period.
- `GetOrdersForPeriod` is a pure slice operation and contains no business logic.

#### Business Logic Layer

- `CalculateTopProduct` handles deduplication, sale counting, and tiebreaking.
- Deduplication is performed with a `HashSet`.
- The sale key format is `customerId|productId|date`, which captures the required deduplication rules.

### Repository Pattern

- `IOrderRepository` defines the order data dependency and supports Dependency Inversion.
- `JsonOrderRepository` reads from JSON files in production.
- `FakeOrderRepository` is used by unit tests so business-rule tests do not need file system access.

### Single Responsibility

- Each method has one clear reason to change.
- Data organisation is separate from business logic.
- The frontend is purely presentational and contains zero duplicated business logic.

### API Contract

The backend returns calculated results in the following shape:

```json
[
  {
    "type": "single",
    "period": "21/04/2026",
    "productName": "Ezy Storage 37L Flexi Laundry Basket - White"
  },
  {
    "type": "range",
    "period": "21/04/2026 - 23/04/2026",
    "productName": "Ezy Storage 37L Flexi Laundry Basket - White"
  }
]
```

## Business Rules

- A product sale is counted once per order regardless of quantity.
- The same customer buying the same product on the same day across multiple orders counts as 1 sale.
- Cancelled orders are credited against the original order and removed from totals.
- Tiebreaker: alphabetical by product name, first wins.

## Assumptions

- Today's date is assumed to be `23/04/2026`.
- `orders.json` is pre-filtered to the target date range, `21/04/2026` to `23/04/2026`, plus any cancellations referencing orders within that range regardless of when the cancellation occurred.
- Deduplication is per day, not per period. The same customer buying the same product on different days counts as separate sales.
- Cancellations are credited against the original order's date, not the cancellation date.

## Expected Outcomes

| Date or Period          | Top Sizzling Hot Product                           |
| ----------------------- | -------------------------------------------------- |
| 21/04/2026              | Ezy Storage 37L Flexi Laundry Basket - White       |
| 22/04/2026              | Ezy Storage 37L Flexi Laundry Basket - White       |
| 23/04/2026              | Arlec 160W Crystalline Solar Foldable Charging Kit |
| 21/04/2026 - 23/04/2026 | Ezy Storage 37L Flexi Laundry Basket - White       |

## Future Improvements

- For larger datasets, daily counts could be cached and summed for range calculations rather than re-processing.
- Cancellation lookup could be extended to fetch cancellations outside the date window by `orderId` for production use.
- Today's date is currently hardcoded and could be driven by configuration or the system date.
- The backend URL in the frontend is hardcoded in `apiConfig.ts` and could be driven by environment variables for different environments.
