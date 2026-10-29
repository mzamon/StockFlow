# StockFlow — Inventory & Procurement Management System

StockFlow is an ASP.NET Core MVC application built for a small or medium-sized South African business. It uses C#, Entity Framework Core, SQL Server, Razor views, HTML, CSS and vanilla JavaScript.

## Included functionality

- Identity sign-in, registration, lockout protection, role-based authorization and administrator user management.
- Product catalogue with SKU/barcode uniqueness, categories, cost/selling price, stock levels, active status and low-stock filters.
- Supplier contact management.
- Purchase orders with R10,000 approval threshold, manager approval and partial/full receiving.
- Sales recording that validates stock and updates quantity immediately.
- Stock movement history for opening stock, receipts, sales and adjustments.
- Audit log for key product, purchase, sale and account actions.
- Dashboard, stock valuation, low-stock reorder recommendations, 30-day sales summary and CSV export.
- Responsive layout for desktop, tablet and mobile.

## Requirements

- Visual Studio 2022 (17.8 or newer recommended) with the **ASP.NET and web development** workload.
- .NET 8 SDK.
- SQL Server LocalDB (installed with Visual Studio) or SQL Server instance.

## Run in Visual Studio

1. Extract the ZIP.
2. Open `StockFlow.sln` in Visual Studio.
3. Confirm the `StockFlow` project is the startup project.
4. Review `src/StockFlow/appsettings.json`. The default uses LocalDB: `Server=(localdb)\mssqllocaldb;Database=StockFlowDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True`.
5. Restore NuGet packages and press **F5**. On first startup, the application creates its database schema and seeds the administrator plus demonstration inventory.
6. Sign in with the bootstrap credentials below, then change the password before exposing the app to a network.

### Bootstrap administrator

- Email: `admin@admin.com`
- Password: `Password123!`

The administrator and demonstration inventory are created only when their records are absent. A normal account registration creates a Viewer account and does **not** create or copy demonstration inventory. In a shared multi-user database, inventory is a shared business workspace; it is not isolated per account. The registration page is intended for staff accounts, not separate tenant businesses.

## SQL Server setup / schema updates

The project calls `EnsureCreated()` for a simple first-run experience. This is suitable for a fresh local demo database. If you change entity definitions after the database has already been created, delete the local `StockFlowDb` database and restart for a fresh schema, or replace the bootstrap with EF Core migrations for production. Do not use `EnsureCreated()` as a production migration strategy.

## Roles

- **Admin**: full access, account status and role management, audit log.
- **Manager**: products, stock, purchases and approval, suppliers, reports, audit log.
- **StockClerk**: products, stock adjustments, receiving, sales and stock movements.
- **Procurement**: products, suppliers and purchase orders.
- **Viewer**: read-only catalogue, suppliers, purchases, reports and stock movement history.

Registration defaults to Viewer. An administrator can assign a more privileged role through User management.

## Business rules and limitations

- Purchase orders with a line total of R10,000 or more require Manager/Admin approval before receiving.
- Receipts cannot exceed the ordered quantity; partial receiving is supported.
- Sales cannot exceed current on-hand stock.
- Negative stock adjustments are rejected if they would make the balance negative.
- The reorder suggestion is a transparent threshold heuristic (`max(2 × reorder level − on hand, 1)`), not machine-learning forecasting.
- Barcode fields and CSV export are supported; scanning hardware is browser/device dependent and not emulated.
- This is a professional starter/demo application, not yet production-hardened. Before real deployment, use a secrets manager, enforce HTTPS, implement email confirmation/reset email delivery, use EF migrations, add integration tests, review CSRF/security headers, and verify backups and concurrency handling.

## Project structure

```text
StockFlow/
├── StockFlow.sln
├── README.md
└── src/StockFlow/
    ├── Controllers/       MVC endpoints and business workflows
    ├── Data/              EF Core context and initial seed
    ├── Models/            Entities and view models
    ├── Services/           Shared services
    ├── Views/              Razor UI, organized by controller
    ├── wwwroot/css/        Responsive application styles
    ├── wwwroot/js/         Small vanilla-JS interactions
    ├── appsettings.json    Local SQL Server connection
    ├── Program.cs          Dependency injection and middleware
    └── StockFlow.csproj
```

## Module mapping

| Module | Evidence demonstrated |
|---|---|
| PROG6221 / PROG6212 | C#, MVC controllers, validation, business rules |
| DATA6211 / DATA6222 / ADDB6311 | Relational entities, keys, indexes, relationships, reporting queries |
| BUIS5111 | Stock, supplier, purchasing and sales workflows |
| SAND6221 | Layered structure, domain models, role workflows |
| ISEC6321 | Identity, password policy, lockout, authorization, anti-forgery tokens |
| SQAT6322 | Validation boundaries and recommended test seams; automated test project still needs to be added |
| IPMA6212 | Organized solution, README, roles, scope and acceptance rules |

## Verification note

The build environment used to package this source did not include the .NET SDK, so the solution has not been compiled or executed here. Open it in Visual Studio, restore packages and resolve any environment-specific issues before treating it as verified. Add automated unit/integration tests before submission or production use.
