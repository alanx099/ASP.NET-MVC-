# Sitemap

This document lists controllers, actions, routes (custom and default), views and brief purpose.

- **Home**
  - URL: `/` (default) - Controller: `HomeController` - Action: `Index` - View: [Views/Home/Index.cshtml](Views/Home/Index.cshtml) - Purpose: Landing page
  - URL: `/Home/Error` - Controller: `HomeController` - Action: `Error` - View: [Views/Home/Error.cshtml](Views/Home/Error.cshtml) - Purpose: Error page

- **Repairs (Nalozi)**
  - URL: `/servisni-nalozi` (CUSTOM) - Controller: `RepairsController` - Action: `Index` - View: [Views/Repairs/Index.cshtml](Views/Repairs/Index.cshtml) - Purpose: List all repair orders (uses EF via repository)
  - URL: `/Repairs/Details/{id}` (default) - Controller: `RepairsController` - Action: `Details` - View: [Views/Repairs/Details.cshtml](Views/Repairs/Details.cshtml) - Purpose: Show repair order details
  - URL: `/servisni-nalozi/novi` (CUSTOM) - Controller: `RepairsController` - Action: `Create` (GET/POST) - View: [Views/Repairs/Create.cshtml](Views/Repairs/Create.cshtml) - Purpose: Create new repair order (loads lookups from DB)
  - URL: `/servisni-nalozi/uredi/{id}` (CUSTOM) - Controller: `RepairsController` - Action: `Edit` (GET/POST) - View: [Views/Repairs/Edit.cshtml](Views/Repairs/Edit.cshtml) - Purpose: Edit existing repair order

- **Guitars (Gitare)**
  - URL: `/gitare` (CUSTOM) - Controller: `GuitarsController` - Action: `Index` - View: [Views/Guitars/Index.cshtml](Views/Guitars/Index.cshtml) - Purpose: List guitars
  - URL: `/Guitars/Details/{id}` (default) - Controller: `GuitarsController` - Action: `Details` - View: [Views/Guitars/Details.cshtml](Views/Guitars/Details.cshtml) - Purpose: Show guitar details
  - URL: `/gitare/nova` (CUSTOM) - Controller: `GuitarsController` - Action: `Create` (GET/POST) - View: [Views/Guitars/Create.cshtml](Views/Guitars/Create.cshtml) - Purpose: Create new guitar (uses lookup tables for brand/type/customers)
  - URL: `/gitare/uredi/{id}` (CUSTOM) - Controller: `GuitarsController` - Action: `Edit` (GET/POST) - View: [Views/Guitars/Edit.cshtml](Views/Guitars/Edit.cshtml) - Purpose: Edit guitar

- **Customers (Stranke)**
  - URL: `/Customers` (default) - Controller: `CustomersController` - Action: `Index` - View: [Views/Customers/Index.cshtml](Views/Customers/Index.cshtml) - Purpose: List customers
  - URL: `/Customers/Details/{id}` (default) - Controller: `CustomersController` - Action: `Details` - View: [Views/Customers/Details.cshtml](Views/Customers/Details.cshtml) - Purpose: Show customer details, guitars and repairs
  - URL: `/customers/nova` (CUSTOM) - Controller: `CustomersController` - Action: `Create` (GET/POST) - View: [Views/Customers/Create.cshtml](Views/Customers/Create.cshtml) - Purpose: Create new customer
  - URL: `/customers/uredi/{id}` (CUSTOM) - Controller: `CustomersController` - Action: `Edit` (GET/POST) - View: [Views/Customers/Edit.cshtml](Views/Customers/Edit.cshtml) - Purpose: Edit customer

- **Technicians**
  - URL: `/Technicians` (default) - Controller: `TechniciansController` - Action: `Index` - View: [Views/Technicians/Index.cshtml](Views/Technicians/Index.cshtml) - Purpose: List technicians
  - URL: `/Technicians/Details/{id}` (default) - Controller: `TechniciansController` - Action: `Details` - View: [Views/Technicians/Details.cshtml](Views/Technicians/Details.cshtml) - Purpose: Technician details

- **Notes**
  - Default route pattern: `{controller=Home}/{action=Index}/{id?}` (see Program.cs)
  - Custom routes counted for assignment: `/servisni-nalozi`, `/servisni-nalozi/novi`, `/servisni-nalozi/uredi/{id}`, `/gitare/nova` (also `/gitare/uredi/{id}` and `/gitare` and `/customers/nova` are custom)

For each Create/Edit form, lookup values come from DB lookup/reference tables (e.g., `Marke`, `TipoveGitara`, `StatusiNaloga`, `VrstePopravke`) and are presented with readable names in dropdowns.
