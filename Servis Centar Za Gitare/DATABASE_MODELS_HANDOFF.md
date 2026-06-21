# Database Models Handoff

This file explains how the backend database is built. It is meant for the next AI/developer who needs to understand the data model, generate Android models, or extend the API.

The EF Core database context is `Data/AppDbContext.cs`. The domain model classes are in `models/`. The app uses PostgreSQL through Npgsql and ASP.NET Core Identity through `IdentityDbContext<AppUser>`.

## High-level domain

The system models a guitar service center:

- Customers bring guitars to the service center.
- Guitars belong to customers.
- Repairs/service orders connect a guitar, customer, optional technician, office, status, and repair type.
- Technicians are specialized employees.
- Technician skills define which guitar types and repair types a technician can work on.
- Offices contain customers, employees, and repair orders.
- Lookup tables store brands, guitar types, repair statuses, and repair types.
- ASP.NET Core Identity stores login users and roles.
- Refresh tokens store JWT refresh-token sessions for API clients.

## Main entity map

```text
AspNetUsers/AppUser 1 -- 0..1 Stranke
AspNetUsers/AppUser 1 -- many RefreshTokens

Poslovnice 1 -- many Stranke
Poslovnice 1 -- many Zaposlenici
Poslovnice 1 -- many Nalozi

Stranke 1 -- many Gitare
Stranke 1 -- many Nalozi

Gitare many -- 1 Marke
Gitare many -- 1 TipoveGitara
Gitare 1 -- many Nalozi

Nalozi many -- 1 Gitare
Nalozi many -- 1 Stranke
Nalozi many -- 0..1 Tehnicari/ZapTehnicar
Nalozi many -- 0..1 Poslovnice
Nalozi many -- 1 StatusiNaloga
Nalozi many -- 1 VrstePopravke
Nalozi 1 -- many NalogDatoteke

Zaposlenici includes ZapTehnicar by discriminator
ZapTehnicar 1 -- many Znanja

Znanja many -- 1 ZapTehnicar
Znanja many -- 1 TipoveGitara
Znanja many -- 1 VrstePopravke
```

## Identity and auth tables

Because `AppDbContext` inherits from `IdentityDbContext<AppUser>`, EF creates standard ASP.NET Core Identity tables:

- `AspNetUsers`
- `AspNetRoles`
- `AspNetUserRoles`
- `AspNetUserClaims`
- `AspNetRoleClaims`
- `AspNetUserLogins`
- `AspNetUserTokens`

### AppUser

File: `models/AppUser.cs`

Extends `IdentityUser`.

Extra navigation properties:

- `Stranka? Stranka` - optional one-to-one customer profile.
- `ICollection<RefreshToken> RefreshTokens` - API refresh-token sessions.

Seeded users are created in `Data/IdentitySeed.cs`:

- `admin@test.com` / role `Admin`
- `manager@test.com` / role `Manager`
- `user@test.com` / role `User`

## RefreshToken

File: `models/RefreshToken.cs`

Table: `RefreshTokens`

Fields:

- `Id: long`
- `TokenHash: string` - unique hash of the refresh token, not the raw token.
- `UserId: string` - FK to `AspNetUsers.Id`.
- `CreatedAtUtc: DateTime`
- `ExpiresAtUtc: DateTime`
- `RevokedAtUtc: DateTime?`
- `ReplacedByTokenHash: string?`
- `IsActive` computed property in C# only, not a DB column.
- `User: AppUser`

Relationships:

- `RefreshToken.UserId -> AspNetUsers.Id`
- Delete behavior: cascade when the user is deleted.
- Unique index on `TokenHash`.

Purpose:

- Used by `/api/auth/login` and `/api/auth/refresh`.
- Refresh tokens are rotated. A used token is revoked and points to the replacement token hash.

## Stranka / Customer

File: `models/Stranka.cs`

Table: `Stranke`

Fields:

- `Id: long`
- `PoslovnicaId: long?`
- `AppUserId: string?`
- `Ime: string`, max 80, required.
- `Prezime: string`, max 80, required.
- `Email: string`, max 120, required.
- `BrojTelefona: string`, max 30, required.
- `Adresa: string`, max 200, required.
- `DatumRegistracije: string`, max 30, required.
- `Napomena: string`, max 1000.

Navigation:

- `Poslovnica? Poslovnica`
- `AppUser? AppUser`
- `ICollection<Gitara> Gitare`
- `ICollection<Nalog> Nalozi`

Relationships:

- Many customers can belong to one office.
- One customer can be linked to one Identity user.
- `AppUserId` has a unique index, so one user can link to only one customer.
- If linked user is deleted, `AppUserId` is set null.
- If office is deleted, `PoslovnicaId` is set null.

Android/API meaning:

- A `User` role account normally sees data through its linked `Stranka`.
- `/api/me/guitars` and `/api/me/service-orders` use this link.

## Gitara / Guitar

File: `models/Gitara.cs`

Table: `Gitare`

Fields:

- `Id: long`
- `SerijskiBroj: string`, max 64, required.
- `MarkaId: int`, required.
- `BrojZica: string`, max 4, required.
- `TipGitareId: int`, required.
- `DatumZaprimanja: DateTime`
- `KupacId: long`, required.

Navigation:

- `Marka Marka`
- `TipGitare TipGitare`
- `Stranka Kupac`
- `ICollection<Nalog> Nalozi`

Relationships:

- Many guitars belong to one customer (`KupacId -> Stranke.Id`).
- Many guitars have one brand (`MarkaId -> Marke.Id`).
- Many guitars have one guitar type (`TipGitareId -> TipoveGitara.Id`).
- Delete behavior for brand, guitar type, and customer is restrict.

Meaning:

- A guitar is the instrument being serviced.
- It is always owned by a customer.

## Nalog / Repair or Service Order

File: `models/Nalog.cs`

Table: `Nalozi`

Fields:

- `Id: long`
- `GitaraId: long`, required.
- `StrankaId: long`, required.
- `TehnicarId: long?`
- `PoslovnicaId: long?`
- `OpisKvara: string`, max 1000, min 3, required.
- `DatumOtvaranja: DateTime`
- `DatumZatvaranja: DateTime`
- `StatusNalogaId: int`, required.
- `VrstaPopravkaId: int`, required.

Navigation:

- `Gitara Gitara`
- `Stranka Stranka`
- `ZapTehnicar? Tehnicar`
- `Poslovnica? Poslovnica`
- `StatusNaloga StatusNaloga`
- `VrstaPopravka VrstaPopravka`
- `ICollection<NalogDatoteka> Datoteke`

Relationships:

- Repair must have a guitar.
- Repair must have a customer.
- Repair can optionally have a technician.
- Repair can optionally belong to an office.
- Repair must have a status.
- Repair must have a repair type.
- Repair can have many uploaded files.

Delete behavior:

- Guitar/customer/status/repair type deletion is restricted while repairs reference them.
- Technician deletion sets `TehnicarId` null.
- Office deletion sets `PoslovnicaId` null.
- Repair file deletion cascades when the repair is deleted.

Important business rule:

- `ApiControllerBase.ValidateRepairAsync` checks whether a technician has the required `Znanje` for the guitar type + repair type combination.
- If no technician has the required skill, the repair is forced toward the received/default workflow and technician/office can be cleared.

## NalogDatoteka / Repair Attachment

File: `models/NalogDatoteka.cs`

Table: `NalogDatoteke`

Fields:

- `Id: long`
- `NalogId: long`, required.
- `OriginalniNaziv: string`, max 255, required.
- `SpremljeniNaziv: string`, max 255, required.
- `RelativnaPutanja: string`, max 500, required.
- `TipSadrzaja: string`, max 120, required.
- `VelicinaBajtova: long`
- `DatumUploada: DateTime`

Navigation:

- `Nalog Nalog`

Relationships:

- Many files belong to one repair.
- Delete behavior: cascade when the repair is deleted.

Meaning:

- Stores metadata for uploaded repair images/files.
- Actual files live under `wwwroot/uploads/...`, not inside the database.

## Poslovnica / Office

File: `models/Poslovnica.cs`

Table: `Poslovnice`

Fields:

- `Id: long`
- `Ime: string`, max 120, required.
- `Adresa: string`, max 200, required.

Navigation:

- `ICollection<Zaposlenik> Zaposlenici`
- `ICollection<Nalog> Nalozi`
- `ICollection<Stranka> Stranke`

Not mapped:

- `Tehnicari`
- `Menadzeri`
- `Gitare`

Meaning:

- Represents a physical service-center office/location.
- Employees, customers, and repair orders can optionally point to an office.

## Zaposlenik / Employee

File: `models/Zaposlenik.cs`

Table: `Zaposlenici`

Fields:

- `Id: long`
- `PoslovnicaId: long?`
- `Ime: string`, max 80, required.
- `Prezime: string`, max 80, required.
- `Email: string`, max 120, required.
- `BrojTelefona: string`, max 30, required.
- `Adresa: string`, max 200, required.
- `DatumZaposlenja: string`, max 30, required.
- `Placa: double`, range 0-1000000.
- `TipZaposlenika: string` discriminator column generated by EF.

Navigation:

- `Poslovnica? Poslovnica`
- `ICollection<Nalog> Nalozi`

Inheritance:

- `ZapTehnicar` inherits from `Zaposlenik`.
- EF uses table-per-hierarchy inheritance with discriminator:
  - `Zaposlenik`
  - `ZapTehnicar`

Relationships:

- Many employees can belong to one office.
- Office deletion sets `PoslovnicaId` null.

## ZapTehnicar / Technician

File: `models/ZapTehnicar.cs`

Table: `Zaposlenici`, same table as `Zaposlenik` because of inheritance.

Extra navigation:

- `ICollection<Znanje> Znanja`

Meaning:

- A technician is an employee with skill records.
- Repairs can reference a technician via `Nalog.TehnicarId`.

## Znanje / Technician Skill

File: `models/Znanje.cs`

Table: `Znanja`

Fields:

- `TehnicarId: long`
- `TipGitareId: int`
- `VrstaPopravkaId: int`

Primary key:

```text
(TehnicarId, TipGitareId, VrstaPopravkaId)
```

Navigation:

- `ZapTehnicar Tehnicar`
- `TipGitare TipGitare`
- `VrstaPopravka VrstaPopravka`

Relationships:

- Many skill rows can belong to one technician.
- Many skill rows can reference one guitar type.
- Many skill rows can reference one repair type.

Delete behavior:

- Technician deletion cascades to skills.
- Guitar type and repair type deletion are restricted while referenced by skills.

Meaning:

- Defines exactly what a technician is qualified to do.
- Example: technician 7 can work on electric guitars and pickup replacement.

## Lookup tables

Lookup tables are seeded in `AppDbContext.OnModelCreating`.

### Marka / Brand

File: `models/Marka.cs`

Table: `Marke`

Fields:

- `Id: int`
- `Naziv: string`, max 80, required.

Navigation:

- `ICollection<Gitara> Gitare`

Seeded values:

- Fender
- Gibson
- Yamaha
- Ibanez
- Taylor
- Martin
- PRS
- Epiphone
- Jackson
- Gretsch
- ESP
- Schecter
- Squier
- Takamine
- Charvel

### TipGitare / Guitar Type

File: `models/TipGitare.cs`

Table: `TipoveGitara`

Fields:

- `Id: int`
- `Naziv: string`, max 80, required.

Navigation:

- `ICollection<Gitara> Gitare`
- `ICollection<Znanje> Znanja`

Seeded values:

- `1` - Aukusticna
- `2` - Elektricna
- `3` - Klasicna
- `4` - BasGitara

### StatusNaloga / Repair Status

File: `models/StatusNaloga.cs`

Table: `StatusiNaloga`

Fields:

- `Id: int`
- `Naziv: string`, max 80, required.

Navigation:

- `ICollection<Nalog> Nalozi`

Seeded values:

- `1` - Zaprimljen
- `2` - U Obradi
- `3` - Ceka Dijelove
- `4` - Zavrsen
- `5` - Otkazan

### VrstaPopravka / Repair Type

File: `models/VrstaPopravka.cs`

Table: `VrstePopravke`

Fields:

- `Id: int`
- `Naziv: string`, max 80, required.

Navigation:

- `ICollection<Nalog> Nalozi`
- `ICollection<Znanje> Znanja`

Seeded values:

- `1` - Zamjena Zica
- `2` - Podesavanje Vrata
- `3` - Podesavanje Intonacije
- `4` - Zamjena Pragova
- `5` - Brusenje Pragova
- `6` - Popravak Elektronike
- `7` - Zamjena Pickupa
- `8` - Zamjena Masinica
- `9` - Popravak Kobilice
- `10` - Ciscenje Potenciometara

## Delete behavior summary

```text
Stranka -> AppUser: set AppUserId null when user is deleted
Stranka -> Poslovnica: set PoslovnicaId null when office is deleted
Zaposlenik -> Poslovnica: set PoslovnicaId null when office is deleted
Nalog -> Poslovnica: set PoslovnicaId null when office is deleted
Nalog -> Tehnicar: set TehnicarId null when technician is deleted
NalogDatoteka -> Nalog: cascade delete files when repair is deleted
Znanje -> Tehnicar: cascade delete skills when technician is deleted
RefreshToken -> AppUser: cascade delete tokens when user is deleted

Gitara -> Marka: restrict
Gitara -> TipGitare: restrict
Gitara -> Stranka/Kupac: restrict
Nalog -> Gitara: restrict
Nalog -> Stranka: restrict
Nalog -> StatusNaloga: restrict
Nalog -> VrstaPopravka: restrict
Znanje -> TipGitare: restrict
Znanje -> VrstaPopravka: restrict
```

## Android model guidance

For Android, do not mirror EF entities directly unless building an admin tool. Use API DTOs from:

- `Dtos/AuthDtos.cs`
- `Dtos/ApiDtos.cs`
- `Dtos/MyDtos.cs`

Important client-facing DTOs:

- `TokenResponseDto`
- `CurrentUserDto`
- `MyGuitarDto`
- `MyServiceOrderDto`
- `CustomerDto`
- `GuitarDto`
- `RepairDto`
- `OfficeDto`
- `EmployeeDto`
- `TechnicianDto`
- `LookupDto`

The database has EF navigation properties and circular relationships. Android models should be flat DTO models matching JSON responses, not full nested database entities.

## Files to inspect when changing schema

- `Data/AppDbContext.cs` - DbSets, relations, delete behaviors, seed data.
- `models/*.cs` - EF entity classes.
- `Migrations/*.cs` - actual migration history.
- `Migrations/AppDbContextModelSnapshot.cs` - current EF schema snapshot.
- `Dtos/*.cs` - API shape exposed to clients.

If a model changes, update:

1. Entity class in `models/`.
2. Relationship/seed data in `AppDbContext` if needed.
3. EF migration.
4. DTO and mapper if API response/request shape changes.
5. Android models if the API shape changed.
