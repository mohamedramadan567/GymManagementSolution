# 🏋️ Gym Management Solution

A layered ASP.NET Core MVC application for managing gym operations — members, trainers, subscription plans, memberships, training sessions, and bookings — built with **.NET 9**, **Entity Framework Core**, and a clean **N-Layer architecture**.

---

## 📐 Architecture

The solution follows a classic **3-layer architecture**, splitting responsibilities across three separate class libraries/projects so each layer only depends on the one beneath it:

```
GymManagement (PL)  →  GymManagement.BLL  →  GymManagement.DAL
   Presentation           Business Logic         Data Access
```

| Layer | Project | Responsibility |
|---|---|---|
| **Presentation (PL)** | `GymManagement` | ASP.NET Core MVC — Controllers, Views, Identity/auth pipeline, static assets |
| **Business Logic (BLL)** | `GymManagement.BLL` | Services, ViewModels, AutoMapper profiles, cross-cutting `Result` type |
| **Data Access (DAL)** | `GymManagement.DAL` | EF Core `DbContext`, entity models, Fluent API configurations, migrations, repositories |

This separation keeps the UI unaware of EF Core, keeps business rules out of controllers, and makes the data layer swappable/testable in isolation.

```
GymManagementSolution/
├── GymManagement/              # Presentation Layer (ASP.NET Core MVC)
│   ├── Controllers/            # Account, Members, Plans, Trainers, Sessions, Memberships, Home
│   ├── Views/
│   ├── Program.cs              # App bootstrap, DI registration, middleware pipeline
│   └── ProgramExtensions.cs    # Migration + database seeding on startup
│
├── GymManagement.BLL/           # Business Logic Layer
│   ├── Services/
│   │   ├── Interfaces/         # IMemberService, IPlanService, ITrainerService, ISessionService, IMembershipService, IAnalyticsService
│   │   ├── Classes/             # Concrete service implementations
│   │   └── Attachment/          # File upload/delete abstraction (IAttachmentService)
│   ├── ViewModels/               # Per-feature DTOs (Member, Plan, Trainer, Session, Membership, Analytics, Account)
│   ├── Common/                  # Result / ResultKind — unified outcome type
│   └── MappingProfile.cs         # AutoMapper entity <-> ViewModel mappings
│
└── GymManagement.DAL/            # Data Access Layer
    ├── Data/
    │   ├── DbContexts/            # GymDbContext (EF Core + ASP.NET Identity)
    │   ├── Models/                 # Domain entities + Enums
    │   ├── Configurations/         # IEntityTypeConfiguration<T> Fluent API per entity
    │   ├── Migrations/             # EF Core migrations
    │   └── DataSeeding/             # Seed data for domain + Identity (roles/users)
    └── Repositories/
        ├── Interfaces/              # IGenericRepository<T>, IUnitOfWork, feature-specific repos
        └── Classes/                  # GenericRepository<T>, UnitOfWork, feature-specific repos
```

---

## 🧩 Design Patterns & Practices

- **Generic Repository Pattern** — `IGenericRepository<TEntity>` / `GenericRepository<TEntity>` provides common CRUD (`GetAllAsync`, `GetByIdAsync`, `FirstOrDefaultAsync`, `AnyAsync`, `CountAsync`, `Add`, `Update`, `Delete`) for any entity deriving from `BaseEntity`, avoiding repeated boilerplate per entity.
- **Unit of Work Pattern** — `IUnitOfWork` wraps the `DbContext`, lazily creates/caches a generic repository per entity type via `GetRepository<TEntity>()`, exposes dedicated repositories for more complex query needs (`ISessionRepository`, `IMembershipRepository`), and centralizes `SaveChangesAsync` so multiple repository operations commit as a single transaction.
- **Result Pattern** — Instead of throwing exceptions or returning bare booleans/nulls, services return a `Result` / `Result<T>` record (`OK`, `Fail`, `NotFound`, `Validation`) carrying a `ResultKind` (`OK`, `NotFound`, `Conflict`, `ValidationFailed`, `Forbidden`). This gives controllers a consistent, explicit way to branch on outcomes (e.g., map to `404`, `409`, or validation errors) without exception-driven control flow.
- **Dependency Injection** — Every service, repository, and the `UnitOfWork` itself are registered as scoped services in `Program.cs`, keeping components loosely coupled and easily testable/mockable.
- **Object Mapping (AutoMapper)** — A single `MappingProfile` maps domain entities to feature-specific ViewModels (e.g., `Member` ↔ `MemberViewModel`, `CreateMemberViewModel`), keeping persistence models out of the Views.
- **Table-Per-Hierarchy Inheritance** — `GymUser` is an abstract base class (`Name`, `Email`, `Phone`, `DateOfBirth`, `Gender`, owned `Address`) inherited by both `Member` and `Trainer`, avoiding duplication of shared person-level fields.
- **Owned Entity Types** — `Address` is modeled as an EF Core `[Owned]` value object embedded directly into `GymUser`-derived tables rather than a separate table.
- **Fluent API Configuration per Entity** — Each entity has its own `IEntityTypeConfiguration<T>` class under `Data/Configurations`, keeping `GymDbContext` free of inline configuration clutter.
- **Auditable Base Entity** — All domain entities inherit `BaseEntity` (`Id`, `CreatedAt`, `UpdatedAt`), giving consistent identity and audit timestamps across the model (e.g., `CreatedAt` doubles as `JoinDate`/`HireDate`/`BookingDate` depending on context).
- **Database Seeding on Startup** — `ProgramExtensions.MigrateAndSeedDatabaseAsync` automatically applies pending EF Core migrations and seeds both domain data (`GymDataSeeding`) and ASP.NET Identity roles/users (`IdentityDataSeeding`) at application boot.
- **ASP.NET Core Identity** — Authentication/authorization is handled via `ApplicationUser : IdentityUser` with custom lockout and unique-email policies configured in `Program.cs`.
- **Server-Side Search/Filtering** — `IMemberService` / `ITrainerService` (or their repositories) expose a search method that applies a case-insensitive partial match (`LIKE`/`Contains`) across `Name`, `Email`, and `Phone` at the query level (EF Core translates it to SQL), rather than filtering in memory. Search is submit-based — the user enters a term and clicks **Search** to trigger the query (not live/AJAX-as-you-type).

---

## 🗂️ Domain Model

| Entity | Notes |
|---|---|
| `GymUser` *(abstract)* | Shared base for `Member` and `Trainer` (name, contact info, DOB, gender, address) |
| `Member` | Gym member; has one `HealthRecord`, many `MemberShip`s and `Booking`s |
| `Trainer` | Has a `Specialty` (enum) and many `Session`s |
| `Plan` | Subscription plan (price, duration, active flag) |
| `MemberShip` | Links a `Member` to a `Plan`; computed `Status`/`IsActive` based on `EndDate` |
| `Session` | Training session run by a `Trainer` under a `Category`, with a member `Booking` list |
| `Booking` | Join entity between `Member` and `Session`, tracks attendance |
| `Category` | Session category/grouping |
| `HealthRecord` | Member's height/weight/blood type/notes |
| `ApplicationUser` | Identity user (login/auth), separate from the domain `Member`/`Trainer` concept |

---

## 🛠️ Tech Stack

- **.NET 9** / **ASP.NET Core MVC**
- **Entity Framework Core 9** (SQL Server provider)
- **ASP.NET Core Identity** for authentication & role-based authorization
- **AutoMapper**
- Razor Views + HTML/CSS/vanilla JS front end

---

## 🌐 Live Demo

A hosted version of the app is available for anyone who wants to try it out without running it locally:

🔗 **[https://gold-gym-management.runasp.net/](https://gold-gym-management.runasp.net/)**

Demo accounts:

| Role | Email | Password |
|---|---|---|
| SuperAdmin | `mohamedramadan@gmail.com` | `P@ssw0rd` |
| Admin | `ramyramadan@gmail.com` | `P@ssw0rd` |

> ⚠️ This is a demo/educational deployment — please don't use real personal data when testing, and expect the database to be reset periodically.

---

## 🚀 Getting Started

### Prerequisites
- [.NET 9 SDK](https://dotnet.microsoft.com/download)
- SQL Server (LocalDB or full instance)

### Setup
```bash
git clone https://github.com/mohamedramadan567/GymManagementSolution.git
cd GymManagementSolution
```

1. Configure your connection string in `GymManagement/appsettings.json`:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=GymManagementDb;Trusted_Connection=True;"
   }
   ```
2. Run the app — migrations and seed data (roles, an initial admin, sample domain data) are applied automatically on startup:
   ```bash
   dotnet run --project GymManagement
   ```

---

## 📋 Core Features

- Member registration & profile management (with photo upload)
- Trainer management by specialty (General Fitness, Yoga, Boxing, CrossFit)
- Server-side search & filtering for Members and Trainers by Name, Email, or Phone (case-insensitive partial match, submit-based via a Search button)
- Subscription plan management and membership tracking (active/expired)
- Session scheduling per trainer/category with member bookings and attendance
- Member health record tracking
- Analytics dashboard
- Role-based authentication via ASP.NET Core Identity

---

## 📝 Notes

This project is under active development — some services are mid-refactor toward the `Result` pattern (see in-code TODO comments), and analytics/reporting features are still evolving.
