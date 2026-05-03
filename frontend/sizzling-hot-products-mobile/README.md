# Sizzling Hot Products Mobile

Expo React Native mobile UI for the Sizzling Hot Products take-home assignment.

The ASP.NET backend owns the business rules, date handling, aggregation, and top-product calculations. This app is intentionally presentation-focused: it fetches the calculated results and renders them in a clean mobile view.

## Run the Expo app

1. Install dependencies:

   ```bash
   npm install
   ```

2. Start Expo:

   ```bash
   npm start
   ```

3. Open the app in Expo Go, an iOS simulator, an Android emulator, or the web target from the Expo terminal menu.

Start the ASP.NET backend separately with its normal local development command:

```bash
dotnet run
```

## Configure the backend URL

The API base URL is centralized in `src/config/apiConfig.ts`:

```ts
export const API_BASE_URL = "http://localhost:5291";
```

Update the host and port to match the running ASP.NET backend.

- iOS simulator can usually use `http://localhost:5291`.
- Android emulator may need `http://10.0.2.2:5291`.
- A real device running Expo Go should use your computer's local network IP, for example `http://192.168.x.x:5291`.

For real-device testing, the backend may also need to listen on your local network:

```bash
dotnet run --urls http://0.0.0.0:5291
```

The app expects the backend to return this presentation-ready shape from the configured endpoint:

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

## Implementation notes

- Uses Expo Router with `app/index.tsx` as the main screen.
- Adds `app/about.tsx` as a small Engineering Notes screen.
- Uses native `fetch` instead of Axios to keep dependencies minimal and because the app only needs a straightforward GET request.
- Keeps API access in `src/api/sizzlingHotApi.ts`.
- Keeps TypeScript response types in `src/types/sizzlingHot.ts`.
- Renders loading, error, refresh, and result states with simple reusable components.
- Uses the backend-provided `type` field to distinguish daily results from the 3-day period result.
- Does not implement business rule calculations in React Native.
- Uses `assets/images/bunnings-logo.jpg` in the home header and `assets/images/flame.png` as a small title/card accent.

## Engineering Notes screen

The About screen explains the main scope decisions: the ASP.NET backend owns the calculations, the Expo app stays presentation-focused, native `fetch` avoids unnecessary dependencies, and TypeScript keeps the data contract maintainable.

## Backend responsibility

The mobile frontend does not calculate sizzling hot products. It trusts the ASP.NET service layer to apply the assignment rules and return calculated results for:

- 21/04/2026
- 22/04/2026
- 23/04/2026
- 21/04/2026 - 23/04/2026

This keeps the React Native code easy to review, test, and explain in an interview.
