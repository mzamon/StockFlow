# StockFlow

**Inventory and Procurement Management System**

StockFlow is an ASP.NET Core MVC application for managing inventory, suppliers, purchasing, sales, stock movements, reporting, and staff access. It is designed for a small or medium-sized South African business and uses South African Rand (ZAR) formatting throughout the interface.

Copyright © 2026 mzamon. All rights reserved.

## Features

- Secure sign-in, registration, password validation, lockout protection, and sign-out.
- Role-based access control using ASP.NET Core Identity.
- Product catalogue with SKU, barcode, category, pricing, stock quantities, reorder levels, and active status.
- Product search, category filtering, and low-stock filtering.
- Supplier contact and procurement information.
- Purchase orders with approval controls and partial or full receiving.
- Sales recording with stock availability validation and immediate stock reduction.
- Stock movement history for opening stock, purchases, sales, and adjustments.
- Audit logging for important operational and account actions.
- Admin user management, account activation, and role assignment.
- Dashboard with stock health, inventory value, pending approvals, and recent activity.
- Reports with stock valuation, low-stock recommendations, recent sales totals, and CSV product export.
- Responsive desktop, tablet, and mobile layout.

## Technology

| Area | Technology |
|---|---|
| Application | ASP.NET Core MVC |
| Language | C# |
| Target framework | .NET 8 |
| Database | SQL Server / SQL Server LocalDB |
| ORM | Entity Framework Core 8 |
| Authentication | ASP.NET Core Identity |
| Frontend | Razor views, HTML, CSS, and vanilla JavaScript |
| UI fonts | DM Sans and Manrope |

## Requirements

- Windows with Visual Studio 2022, or the .NET 8 SDK.
- Visual Studio's **ASP.NET and web development** workload when using Visual Studio.
- SQL Server LocalDB, included with many Visual Studio installations, or another SQL Server instance.
- Internet access on first frontend load if the Google Fonts import is used.

The project currently targets `.NET 8`. A compatible .NET 8 runtime and SDK should be installed even if a newer SDK is also available.

## Quick start

### Visual Studio

1. Open `StockFlow.sln`.
2. Set `src/StockFlow` as the startup project if Visual Studio has not selected it automatically.
3. Check the connection string in `src/StockFlow/appsettings.json`.
4. Restore NuGet packages.
5. Press **F5** or select **Debug > Start Without Debugging**.
6. The first startup creates the database schema and initializes the roles, administrator account, and demonstration inventory.
7. The application opens at the sign-in page. Authenticate before accessing the dashboard or any operational module.

### Command line

From the solution directory:

```powershell
dotnet restore .\src\StockFlow\StockFlow.csproj
dotnet build .\StockFlow.sln
dotnet run --project .\src\StockFlow\StockFlow.csproj
```

The application uses the launch profile in `src/StockFlow/Properties/launchSettings.json` when started through Visual Studio or `dotnet run`.

## Administrator login

The first application startup creates the bootstrap administrator if the account does not already exist:

| Field | Value |
|---|---|
| Email | `admin@admin.com` |
| Password | `Password123!` |
| Role | `Admin` |

Change this password before using the application outside a local development environment. Do not publish, reuse, or expose the bootstrap credentials in a production deployment.

## Authentication and account flow

- Anonymous visitors are redirected to `/Account/Login`.
- Users cannot access the dashboard or protected modules without signing in.
- New users can select **Create an account** from the sign-in page.
- Registration creates an active `Viewer` account.
- Registration does not automatically sign the user in.
- After successful registration, the user is returned to the sign-in page and must log in.
- A user with an inactive account cannot sign in.
- Users are redirected to the originally requested local URL after successful login when one is available.
- Logout ends the session and returns the user to the sign-in page.

## Roles and permissions

| Role | Access |
|---|---|
| **Admin** | Full application access, dashboard demonstration data, user management, role assignment, account activation, audit log, inventory, suppliers, purchasing, receiving, sales, reports, and stock adjustments. |
| **Manager** | Inventory, suppliers, purchasing, approval, receiving, sales, reports, stock adjustments, and audit log. |
| **StockClerk** | Inventory viewing and editing, stock adjustments, receiving, sales, and stock movement history. |
| **Procurement** | Inventory maintenance, suppliers, purchase orders, and reports. |
| **Viewer** | Read-only inventory, suppliers, purchase orders, reports, and stock movement history. |

Only administrators can manage users and assign roles. New registrations always start as `Viewer`.

## Demonstration data

Demonstration data is intentionally limited to a maximum of **four sample products**:

1. A4 Copy Paper
2. Black Ballpoint Pens
3. USB-C Cable 1m
4. Wireless Mouse

The demonstration inventory is installed in the shared local database for the seeded administrator. It is not copied into a separate account workspace because this application uses one shared business database rather than multi-tenant data isolation.

Non-admin users receive an empty dashboard view and do not see administrator demonstration metrics or sample activity. Registered users can create and manage business records according to their assigned role.

On startup, the seed process also removes the old sample products with SKUs `SKU00005` through `SKU00008` if they are present. This keeps the demonstration catalogue at no more than four products.

## Business rules

- Purchase orders with a total of **R10,000 or more** require Manager or Admin approval before receiving.
- A purchase receipt cannot exceed the remaining ordered quantity.
- Partial receiving is supported.
- A sale cannot exceed the product's current quantity on hand.
- A sale immediately reduces inventory and creates a stock movement.
- A stock adjustment cannot create negative stock.
- Product SKUs must be unique.
- Barcodes must be unique when supplied.
- Purchase order numbers and sale numbers must be unique.
- Decimal prices and totals are stored with two decimal places.
- The reorder recommendation uses the transparent formula:

  ```text
  max((2 × reorder level) − quantity on hand, 1)
  ```

  This is a simple operational heuristic, not a statistical or machine-learning forecast.

## Database configuration

The default connection string uses SQL Server LocalDB:

```text
Server=(localdb)\mssqllocaldb;Database=StockFlowDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True
```

To use another SQL Server instance, update the `DefaultConnection` value in:

```text
src/StockFlow/appsettings.json
```

The application calls `EnsureCreated()` during startup. This is convenient for a fresh local demonstration database, but it is not a production migration strategy.

### Resetting the local database

If the entity model changes and the existing LocalDB schema is no longer compatible:

1. Stop the application.
2. Delete the local `StockFlowDb` database using SQL Server Object Explorer or SQL Server Management Studio.
3. Start the application again.
4. The schema and demonstration data will be recreated.

For production, replace `EnsureCreated()` with versioned EF Core migrations and a controlled deployment process.

## Project structure

```text
StockFlow/
├── StockFlow.sln
├── README.md
└── src/StockFlow/
    ├── Controllers/       MVC endpoints and workflows
    ├── Data/              EF Core context and seed initialization
    ├── Models/            Entities and view models
    ├── Services/          Shared application services
    ├── Views/             Razor views organized by controller
    ├── wwwroot/css/       Responsive application styling
    ├── wwwroot/js/        Client-side interactions
    ├── appsettings.json   Database connection and logging configuration
    ├── Program.cs         Dependency injection and middleware pipeline
    └── StockFlow.csproj   Project and package configuration
```

## CSV export

The Reports page and Inventory page provide product CSV export. Exported values are quoted and embedded quotation marks are escaped according to standard CSV rules.

The export includes:

- SKU
- Name
- Category
- Cost price
- Selling price
- Quantity on hand
- Reorder level
- Barcode
- Active status

## Security notes

The application includes:

- ASP.NET Core Identity password hashing.
- Strong password requirements.
- Account lockout after repeated failed login attempts.
- Role-based authorization.
- Antiforgery validation on POST actions.
- Local return URL validation after login.
- Protected admin and operational endpoints.
- Audit records for important changes.

Before production deployment:

- Replace the bootstrap administrator password.
- Store connection strings and secrets in a secrets manager or environment variables.
- Use HTTPS with a valid certificate.
- Replace `EnsureCreated()` with EF Core migrations.
- Configure email confirmation, password reset delivery, and account recovery.
- Review cookie, security-header, and session settings.
- Add automated unit, integration, and authorization tests.
- Configure database backups, monitoring, and concurrency handling.
- Remove or replace demonstration data.

## Validation and build

Build the solution with:

```powershell
dotnet build .\StockFlow.sln
```

The current solution has been formatted and verified with a clean build. The project includes application-level validation, but a dedicated automated test project should be added before production use.

## Copyright

Copyright © 2026 mzamon. All rights reserved.
