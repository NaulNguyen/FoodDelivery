# FoodDelivery 🍔🚀

A scalable, robust backend API for a Food Delivery platform built with **.NET 10** following **Clean Architecture** principles.

## Features ✨

- **Authentication & Authorization**: Role-based access control (Admin, Customer, Driver, RestaurantOwner) using ASP.NET Identity & JWT with Refresh Token support.
- **Restaurant Management**: Endpoints for restaurant owners to create, update, manage their profile and menus.
- **Menu Management**: Manage menu items including prices and detailed descriptions (supports soft deletion).
- **Order System**: 
  - Customers can place orders for multiple items.
  - Restaurant Owners can update order statuses (Pending, Preparing, Delivering, Completed).
  - Includes Pagination, transaction safety (`IDbContextTransaction`) to ensure reliable order history.
- **Clean Architecture Elements**:
  - Encapsulated business logic.
  - `Result<T>` pattern for consistent error handling and responses without throwing unnecessary exceptions.
  - Entity Framework Core with Fluent configurations, query filters for soft delete logic.
  - DTO mapping handled efficiently via `AutoMapper`.

## Tech Stack 🛠️

- **Framework**: .NET 10 (C# 14)
- **Architecture**: Clean Architecture (API, Application, Domain, Infrastructure)
- **Database**: MySQL / Entity Framework Core 9.x
- **Identity & Security**: Microsoft.AspNetCore.Identity + JWT Bearer Tokens
- **Mapping**: AutoMapper
- **Patterns Used**: Generic Result Pattern, Repository/Services, Pagination Pattern

## Project Structure 📁

- `FoodDelivery.Domain`: Contains core entities (`User`, `Restaurant`, `Order`, `MenuItem`, etc.) and system constants (Roles, Order statuses).
- `FoodDelivery.Application`: Defines contracts (`Interfaces`), Data Transfer Objects (`DTOs`), common utilities like `Result<T>` and `PaginatedList<T>`, and AutoMapper configurations.
- `FoodDelivery.Infrastructure`: Implementations for the interfaces (Services), DB Context (`FoodDeliveryDbContext`), Identity configuration, and EF Core migrations.
- `FoodDelivery.API`: The entry point via ASP.NET Core Controllers, dependency injection registration, and global exception handling middleware.

## Getting Started 🚀

### Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- MySQL Server (or modify the connection string & provider in `Infrastructure` to suit your database)

### Setup & Installation

1. **Clone the repository:**
   ```bash
   git clone https://github.com/yourusername/FoodDelivery.git
   cd FoodDelivery
   ```

2. **Database Configuration:**
   Open `FoodDelivery.API/appsettings.json` and configure your database connection string and JWT Secrets:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=localhost;Database=FoodDeliveryDB;User=root;Password=your_password;"
   },
   "Jwt": {
     "Issuer": "your_issuer",
     "Audience": "your_audience",
     "Key": "your_super_secret_key_needs_to_be_long_enough"
   }
   ```

3. **Apply EF Core Migrations:**
   Ensure your database server is running and apply the migrations to construct the schema.
   ```bash
   cd FoodDelivery.Infrastructure
   dotnet ef database update --startup-project ../FoodDelivery.API
   ```

4. **Run the API:**
   ```bash
   cd ../FoodDelivery.API
   dotnet run
   ```

The API will now be running on either `https://localhost:xxxx` or `http://localhost:xxxx`. You can access the Swagger UI at `/swagger` for endpoint documentation.

## Role Responsibilities 👥

- **Customer**: Browse menus, place orders, view order history.
- **RestaurantOwner**: Register a restaurant, manage menu items, receive and update status on orders belonging to their restaurant.
- **Driver**: (Extensible) Accept pending orders, deliver and finalize the flow.
- **Admin**: Complete overview / management of the system operations.

## Roadmap & Enhancements 🛤️

- [ ] Integrate FluentValidation for clean DTO property validation.
- [ ] Add real-time notifications for order status changes via SignalR.
- [ ] Implement caching (Redis) for frequently fetched menus.
- [ ] Add Unit & Integration tests for Core layers.

