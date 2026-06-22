# Admin/Manager Android Handoff

Ovaj dokument objasnjava kako treba izgledati Android aplikacija kad je prijavljen `Admin` ili `Manager`.

Najbitnije: `Admin` i `Manager` nisu obicni korisnici servisa. Njima nije bitan ekran "moje gitare" ili "moji nalozi". Oni upravljaju cijelim servisnim centrom. Njihov prvi ekran mora biti operativni dashboard za sve kupce, sve gitare, sve servisne naloge, sve tehnicare i stanje posla.

Za korisnicki portal pogledaj `User` flow u `DESIGN_HANDOFF_ANDROID.md`. Za staff pogled koristi ovaj dokument.

## Role split

Nakon login-a Android mora pozvati:

```http
GET /api/auth/me
Authorization: Bearer <accessToken>
```

Ako `roles` sadrzi `Admin` ili `Manager`, aplikacija treba otvoriti staff/admin iskustvo.

Ako `roles` sadrzi samo `User`, aplikacija treba otvoriti korisnicki "Moj servis" portal.

## Staff mental model

Admin/Manager app je "service center control room".

Primarni zadaci:

- Vidjeti sto se trenutno dogadja u cijelom servisu.
- Vidjeti otvorene servisne naloge.
- Vidjeti koji nalozi trebaju tehnicara.
- Vidjeti gdje nema tehnicara s potrebnim znanjem.
- Otvoriti detalj kupca, gitare ili naloga.
- Kreirati i urediti kupce, gitare, naloge i tehnicare.
- Brzo filtrirati po statusu, kupcu, tehnicaru, poslovnici ili tipu popravka.

Sto ne raditi:

- Ne prikazivati `/api/me/guitars` kao glavni staff ekran.
- Ne prikazivati "moji servisni nalozi" kao glavni staff ekran.
- Ne graditi staff home kao osobni profil korisnika.

## Recommended staff navigation

Za `Manager`:

- Dashboard
- Repairs
- Customers
- Guitars
- Technicians

Za `Admin`:

- Dashboard
- Repairs
- Customers
- Guitars
- More

Admin `More`:

- Technicians
- Employees
- Offices
- Users ako postoji API/UI
- Lookup data
- Logout

Ako app koristi bottom navigation, neka glavni tabovi budu operativni. Ako ima previse stavki, koristiti bottom nav + More screen ili navigation drawer.

## Staff dashboard

Prvi ekran za `Admin`/`Manager` treba biti dashboard, ne lista njegovih privatnih naloga.

Dashboard treba prikazati:

- Ukupan broj kupaca.
- Ukupan broj gitara.
- Ukupan broj servisnih naloga.
- Broj otvorenih servisnih naloga.
- Broj tehnicara.
- Najnoviji servisni nalozi.
- Nalozi bez dodijeljenog tehnicara.
- Nalozi gdje nema tehnicara s potrebnim znanjem.
- Brze akcije: create repair, add customer, add guitar.

Visual pattern:

- Top hero/header: "Service center overview" ili "Dashboard".
- KPI cards u 2-column gridu na mobitelu.
- Priority queue ispod KPI-ja.
- Status chips za naloge.
- Warning/danger cards za probleme.

## Current API reality

Trenutno postoji puno CRUD API endpointa, ali nema poseban `/api/dashboard` endpoint za staff summary.

Android moze odmah koristiti postojece endpointove:

```http
GET /api/customers?take=200
GET /api/guitars?take=200
GET /api/repairs?take=200
GET /api/technicians?take=200
GET /api/offices?take=200
GET /api/repair-statuses
GET /api/repair-types
GET /api/guitar-types
```

Za dashboard Android moze privremeno:

- dohvatiti `/api/repairs?take=200`
- izbrojati otvorene statuse lokalno
- prikazati zadnjih 5-10 po `datumOtvaranja`
- dohvatiti customers/guitars/technicians count iz duljine responsea

Ovo je OK za mali projekt/lab, ali nije optimalno za produkciju.

## Recommended backend improvement

Najbolje bi bilo dodati jedan endpoint:

```http
GET /api/dashboard
Authorization: Bearer <accessToken>
Roles: Admin, Manager
```

Predlozeni response:

```json
{
  "totalCustomers": 42,
  "totalGuitars": 77,
  "totalRepairs": 108,
  "openRepairs": 12,
  "totalTechnicians": 6,
  "unassignedRepairs": 3,
  "missingSkillRepairs": 1,
  "recentRepairs": [],
  "repairsByStatus": [
    { "statusId": 1, "status": "Zaprimljen", "count": 4 },
    { "statusId": 2, "status": "U Obradi", "count": 5 }
  ]
}
```

Dok taj endpoint ne postoji, Android neka koristi postojece list endpointove.

## Repairs screen

Ovo je najvazniji staff ekran.

Endpoint:

```http
GET /api/repairs?query=&customerId=&guitarId=&technicianId=&statusId=&repairTypeId=&take=50
```

Screen treba:

- Prikazati sve servisne naloge, ne samo naloge trenutnog korisnika.
- Default sort: newest first.
- Prikazati status chip.
- Prikazati kupca.
- Prikazati gitaru.
- Prikazati tehnicara ili `Unassigned`.
- Prikazati datum otvaranja i planirano zatvaranje.
- Omoguciti filtere:
  - status
  - customer
  - guitar
  - technician
  - repair type
  - text search

Card content:

```text
[Status chip] [Repair type]
Customer name
Guitar brand + serial number
Issue/description
Technician: name or Unassigned
Opened: date
Planned close: date
```

Actions:

- Details
- Edit
- Delete only for Admin

For `Manager`, hide delete action or show disabled with explanation.

## Priority repair states

Staff mora odmah vidjeti probleme:

### Unassigned repair

Ako je `tehnicar == null`, oznaciti card kao warning.

Tekst:

```text
Needs technician assignment
```

### Missing skill coverage

Ako backend vrati ili app izracuna da nema tehnicara s potrebnim `Znanje` za `tipGitare + vrstaPopravka`, oznaciti danger.

Tekst:

```text
No qualified technician available
```

Trenutno API ne vraca direktno `missingSkillCoverage` flag u `RepairDto`. Web MVC to racuna u controller/viewmodelu. Za Android bi bilo dobro prosiriti `RepairDto` ili dodati dashboard endpoint koji vraca taj signal.

## Customers screen

Endpoint:

```http
GET /api/customers?query=&officeId=&take=50
```

Staff koristi ovaj ekran kao CRM/contact directory.

Prikaz:

- Customer name.
- Email.
- Phone.
- Guitar count ako ga DTO daje.
- Office.
- Notes.

Actions:

- Details
- Edit
- Delete only Admin, i samo ako customer nema guitars/repairs.
- Add customer.

Ne tretirati customer screen kao "moj profil".

## Guitars screen

Endpoint:

```http
GET /api/guitars?query=&customerId=&take=50
```

Prikaz:

- Brand.
- Guitar type.
- Serial number.
- Number of strings.
- Received date.
- Owner/customer.

Actions:

- Details
- Edit
- Delete only Admin
- Add guitar

Glavna staff vrijednost: brzo naci instrument i vidjeti tko je vlasnik i koji servisni nalozi postoje.

## Technicians screen

Endpoint:

```http
GET /api/technicians?query=&officeId=&take=50
```

Prikaz:

- Technician name.
- Email/phone.
- Office.
- Skill chips:
  - guitar type
  - repair type

Actions:

- Details
- Edit skills
- Delete only Admin
- Add technician

Glavna staff vrijednost: vidjeti kapacitet servisa i tko moze raditi koji tip popravka.

## Offices / Employees / Lookup data

Ovo je sekundarno za staff, ali bitno za Admin.

Admin moze imati `More` ekran:

- Offices
- Employees
- Brands
- Guitar types
- Repair statuses
- Repair types

Manager moze vidjeti neke od tih podataka read-only ako app to zeli, ali delete/create lookupa treba tretirati kao Admin-only.

## Create/Edit flows

Staff mora moci kreirati i urediti:

- Customer
- Guitar
- Repair
- Technician

Najbitniji create flow je `Create repair`.

Create repair treba imati:

- Customer picker/autocomplete.
- Guitar picker filtriran po customeru.
- Repair type dropdown.
- Status dropdown.
- Technician picker.
- Office picker ako treba.
- Opening date.
- Optional planned close date.
- Description.

Ako odabrani technician nema potrebni skill, prikazati validation error koji backend vrati.

## Role permissions

Backend pravila:

- `Admin` i `Manager` mogu koristiti glavne CRUD GET/POST/PUT endpointove koji nasljeduju `ApiControllerBase`.
- Vecina `DELETE` endpointa je samo `Admin`.
- `User` koristi `/api/me/...`.

Android pravila:

- Ako je `Manager`, ne prikazivati destructive delete kao normalnu akciju.
- Ako delete ipak postoji u UI-u, backend ce vratiti `403`; UI treba prikazati "Nemate ovlasti za brisanje."
- Ako je `Admin`, prikazati delete uz confirm dialog.

## Empty/error states for staff

Staff empty states trebaju biti operativni:

- No repairs: "No repair orders found for current filters."
- No customers: "No customers match this search."
- No technicians: "No technicians found. Add one before assigning repairs."
- 403: "Nemate ovlasti za ovu akciju."
- 401: clear tokens and return to login.

## Difference from User portal

User portal:

- `/api/me/guitars`
- `/api/me/service-orders`
- Shows only linked customer data.
- Personal service status.

Admin/Manager portal:

- `/api/repairs`
- `/api/customers`
- `/api/guitars`
- `/api/technicians`
- `/api/offices`
- Shows whole service center state.
- Operational queue and management.

Do not reuse User portal as the main staff experience.
