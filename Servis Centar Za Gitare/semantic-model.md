# Semantic Model (EF Core Code First)

## Persistent Entities
- Poslovnica
- Zaposlenik (base type)
- ZapTehnicar (derived from Zaposlenik)
- Stranka
- Gitara
- Nalog
- Znanje
- Marka (lookup table)
- TipGitare (lookup table)
- StatusNaloga (lookup table)
- VrstaPopravka (lookup table)

## Primary Keys
- Poslovnica: `Id`
- Zaposlenik: `Id`
- Stranka: `Id`
- Gitara: `Id`
- Nalog: `Id`
- Znanje: composite key (`TehnicarId`, `TipGitareId`, `VrstaPopravkaId`)
- Marka: `Id`
- TipGitare: `Id`
- StatusNaloga: `Id`
- VrstaPopravka: `Id`

## Important Properties
- Poslovnica: `Ime`, `Adresa`
- Zaposlenik: `Ime`, `Prezime`, `Email`, `BrojTelefona`, `Adresa`, `DatumZaposlenja`, `Placa`
- Stranka: `Ime`, `Prezime`, `Email`, `BrojTelefona`, `Adresa`, `DatumRegistracije`, `Napomena`
- Gitara: `SerijskiBroj`, `MarkaId`, `BrojZica`, `TipGitareId`, `DatumZaprimanja`, `KupacId`
- Nalog: `GitaraId`, `StrankaId`, `TehnicarId`, `PoslovnicaId`, `OpisKvara`, `DatumOtvaranja`, `DatumZatvaranja`, `StatusNalogaId`, `VrstaPopravkaId`
- Znanje: `TehnicarId`, `TipGitareId`, `VrstaPopravkaId`
- Marka: `Id`, `Naziv`
- TipGitare: `Id`, `Naziv`
- StatusNaloga: `Id`, `Naziv`
- VrstaPopravka: `Id`, `Naziv`

## Relationships
- Poslovnica (1) -> (many) Stranka
  - `Stranka.PoslovnicaId` (nullable FK)
- Poslovnica (1) -> (many) Zaposlenik
  - `Zaposlenik.PoslovnicaId` (nullable FK)
- Poslovnica (1) -> (many) Nalog
  - `Nalog.PoslovnicaId` (nullable FK)
- Stranka (1) -> (many) Gitara
  - `Gitara.KupacId` (required FK)
- Marka (1) -> (many) Gitara
  - `Gitara.MarkaId` (required FK)
- TipGitare (1) -> (many) Gitara
  - `Gitara.TipGitareId` (required FK)
- Gitara (1) -> (many) Nalog
  - `Nalog.GitaraId` (required FK)
- Stranka (1) -> (many) Nalog
  - `Nalog.StrankaId` (required FK)
- ZapTehnicar (1) -> (many) Nalog
  - `Nalog.TehnicarId` (required FK)
- StatusNaloga (1) -> (many) Nalog
  - `Nalog.StatusNalogaId` (required FK)
- VrstaPopravka (1) -> (many) Nalog
  - `Nalog.VrstaPopravkaId` (required FK)
- ZapTehnicar (1) -> (many) Znanje
  - `Znanje.TehnicarId` (required FK)
- TipGitare (1) -> (many) Znanje
  - `Znanje.TipGitareId` (required FK)
- VrstaPopravka (1) -> (many) Znanje
  - `Znanje.VrstaPopravkaId` (required FK)

## Enum Conversion to Lookup Tables
All four enums have been converted to database lookup tables for improved maintainability and flexibility:

- **Marka** (formerly MarkeEnum): 15 guitar brands - converted for business catalog management
- **TipGitare** (formerly TipGitareEnum): 4 guitar types - converted because it's part of Znanje composite key
- **StatusNaloga** (formerly StatusNalogaEnum): 5 workflow states - converted for workflow flexibility
- **VrstaPopravka** (formerly VrstaPopravkaEnum): 10 repair service types - converted for service menu management

Each lookup table is seeded with initial data preserving the original enum integer values as IDs, ensuring stable references.

## Assumptions During EF Conversion
- All four enum types (MarkeEnum, TipGitareEnum, StatusNalogaEnum, VrstaPopravkaEnum) have been refactored into lookup tables.
- `Poslovnica` was made persistent by adding `Id` and nullable relationships so existing mock construction remains valid.
- `Znanje` is modeled as an entity with a composite key (`TehnicarId`, `TipGitareId`, `VrstaPopravkaId`) to maintain skill uniqueness per technician.
- Existing repository/mock logic was preserved; EF registration and model mapping were added without deleting current application logic.
