# Design Handoff za Android aplikaciju

Ovaj dokument opisuje kako prenijeti postojeci web dizajn aplikacije "Guitar Service Center" u Android aplikaciju. Companion dokument za API je `API_HANDOFF_ANDROID.md`.

## Vizualni smjer

Aplikacija treba izgledati kao premium repair studio command center: tamna, mirna, tehnicki uredna, s toplim zlatnim akcentima i hladnim cyan akcentima. Ne raditi genericki bijeli CRUD UI. Postojeci web dizajn koristi glass panele, status chipove, kartice i visoki kontrast.

Kljucni dojam:

- Dark mode first.
- Premium servis / studio / workshop, ne studentska admin tablica.
- Informacije su guste, ali uredne.
- Kartice su glavni pattern, tablice koristiti samo kad je stvarno potrebno.
- Status i prioritet moraju biti vidljivi odmah.

## Design tokeni

Izvorni web tokeni su u `wwwroot/css/site.css`.

### Boje

Koristiti ove vrijednosti kao Android theme/design tokens:

```text
background0 = #05070D
background1 = #0A1020
background2 = #121A2D
surface = rgba(12, 18, 30, 0.72)
surfaceStrong = rgba(16, 24, 39, 0.88)
stroke = rgba(255, 255, 255, 0.10)
strokeStrong = rgba(255, 255, 255, 0.16)
textPrimary = #EDF2FF
textMuted = #A8B2C7
textSoft = #6F7B96
accentGold = #D7B36A
accentCyan = #76D1FF
accentViolet = #8A7CFF
success = #6FD6A6
warning = #FFB45D
danger = #FF6B81
```

Android ne mora doslovno koristiti CSS `rgba` ako framework ne podrzava alpha tokene elegantno. Bitno je zadrzati tamnu podlogu, translucent-looking kartice i jasne rubove.

### Tipografija

Web koristi:

- Body: `Manrope`
- Headings/brand/large numbers: `Space Grotesk`

Na Androidu:

- Ako se dodaju Google Fonts, koristiti Manrope za body i Space Grotesk za naslove.
- Ako se ne dodaju fontovi, koristiti sistemski sans, ali zadrzati hijerarhiju: veliki kompaktni naslovi, citljiv body, mala uppercase labela za sekcije.

Tipografski pattern:

- Screen title: 28-34sp, semibold/bold.
- Section label/kicker: 11-12sp, uppercase, letter spacing blago povecan.
- Card title: 18-20sp, semibold.
- Body: 14-16sp.
- Metadata label: 11-12sp, uppercase.
- Metadata value: 14-15sp.

## Layout pravila za Android

### Generalno

- Root background: tamni gradient ili solid `background1`.
- Content padding: 16dp na mobitelu.
- Kartice: 12-20dp padding, 12-18dp radius.
- Razmak izmedju sekcija: 16-20dp.
- Min touch target: 44-48dp.
- Koristiti LazyColumn za liste.
- Ne kopirati desktop topbar doslovno; na Androidu koristiti bottom navigation ili navigation drawer ovisno o ulozi.

### Navigacija

Predlozeni role-based navigation:

`User`:

- Dashboard / Moj servis
- Gitare
- Servisni nalozi
- Profil / Logout

`Manager`:

- Dashboard
- Repairs
- Customers
- Guitars
- Technicians

`Admin`:

- Dashboard
- Repairs
- Customers
- Guitars
- More

Admin `More` moze sadrzavati:

- Technicians
- Employees
- Offices
- Users
- Lookup data
- Logout

Na Androidu ne prikazivati rute za koje korisnik nema rolu. Role dolaze iz `GET /api/auth/me`.

## Ekrani

### Login ekran

Svrha: email/password login preko `POST /api/auth/login`.

Layout:

- Tamna pozadina.
- Brand blok: `G` mark + `Guitar Service Center`.
- Subtitle: "Premium repair studio operations".
- Login kartica sa email i password poljima.
- Primary button: "Login".
- Error state: crveni/danger panel ako credentials nisu dobri.

Stil:

- Card surface tamna/translucent.
- Primary button zlatni gradient ili `accentGold`.
- Input border `stroke`, focus `accentCyan`.

Ne koristiti cookie login u Android aplikaciji. Samo JWT login.

### Dashboard

Web referenca: `Views/Home/Index.cshtml`.

Sadrzaj:

- Hero/header panel sa nazivom poslovnice i adresom.
- KPI cards:
  - Customers
  - Guitars
  - Repairs
  - Technicians
- Recent repairs kao queue kartice.
- Technicians & clients compact chips.

Android prijedlog:

- Top screen header: title + subtitle.
- Horizontal/2-column KPI cards, ovisno o sirini.
- Recent repairs kao vertikalna lista kartica.
- Quick actions: "Start repair", "Open queue", "Browse customers".

### Moj servis / korisnicki portal

Web referenca: `Views/MojServis/Index.cshtml`.

Ovo je najvazniji ekran za obicnog korisnika.

Sadrzaj:

- Header: "Moj servis".
- Kratki opis: pregled gitara i servisnih naloga.
- Lista gitara korisnika.
- Svaka gitara prikazuje:
  - Brand + type
  - Serial number
  - Received date
  - Active status
  - Issue
  - Planned close
- Ispod gitare prikazati servisne naloge te gitare.

Android prijedlog:

- Jedna kartica po gitari.
- U kartici prikazati najnoviji servis kao dominantan status chip.
- Prosirivi dio ili detail screen za povijest servisnih naloga.
- Empty state ako nema gitara: "Trenutno nemate evidentiranih gitara u servisu."

API:

- `GET /api/me/guitars`
- `GET /api/me/service-orders`

### Repairs queue

Web referenca: `Views/Repairs/Index.cshtml`.

Svrha: radni queue za servisne naloge.

Kartica repaira treba prikazati:

- Repair type kao mala section labela.
- Customer name kao glavni naslov.
- Description / issue.
- Status chip.
- Guitar.
- Technician ili "Unassigned".
- Opened date.
- Planned close.
- Actions: details, edit, delete ako Admin.

Special states:

- Missing skill coverage: danger/red styling. Poruka: "No technician has the required skill..."
- Needs technician: warning/orange styling. Poruka: "Qualified technician exists. Assign one..."
- Normal: standard dark card.

Na Androidu animacije nisu obavezne. Dovoljno je jasna boja ruba, status chip i warning/danger copy.

### Customers

Web referenca: `Views/Customers/Index.cshtml`.

Sadrzaj kartice:

- Initials badge.
- Full name.
- Email.
- Broj gitara kao info/status chip.
- Phone.
- Registration date.
- Note.
- Actions: details/edit/delete.

Android:

- List card s initials avatarom.
- Tap otvara detail screen.
- Long press ili overflow menu za edit/delete.
- Delete disabled ako customer ima guitars ili repairs.

### Guitars

Kartica treba prikazati:

- Brand + type.
- Serial number.
- Number of strings.
- Received date.
- Customer/owner.

Android:

- Koristiti guitar kao entity card.
- Ako dolazi iz korisnickog portala, fokus na status servisa.
- Ako dolazi iz admin/manager dijela, fokus na vlasnika i actions.

### Technicians / Employees / Offices

Ovi ekrani su admin/manager operational screens.

Pattern:

- Hero/header title.
- Filter/sort controls.
- Entity cards.
- Detail screen s definition list layoutom.
- Primary create action kao floating action button ili top action.

Technician kartica treba posebno prikazati skills kao chips:

- Guitar type
- Repair type

### Lookup screens

Lookup podaci:

- Brands
- Guitar types
- Repair statuses
- Repair types

Ako Android aplikacija ima admin tools, lookup screens mogu biti jednostavne liste s create/edit/delete. Ako je scope samo za korisnika, lookup screens nisu potrebni osim za dropdown podatke u formama.

## Komponente

### Hero / screen header

Web koristi `.hero-panel`, `.hero-kicker`, `.hero-title`, `.hero-subtitle`.

Android pattern:

- Surface/card na vrhu ekrana.
- Mala uppercase labela u `accentGold`.
- Veliki title.
- Muted subtitle.
- Optional action row.

### Entity card

Koristiti za customers, guitars, technicians, offices.

Style:

- Dark translucent surface.
- 1dp stroke `stroke`.
- 12-18dp radius.
- Primary title.
- Muted subtitle.
- Chip row.
- Action row ili overflow menu.

### Queue card

Koristiti za repairs/service orders.

Style:

- Vise informacija od entity carda.
- Status chip u gornjem desnom dijelu.
- Metadata row: guitar, technician, opened, planned close.
- Danger/warning variants za problematicne repair stateove.

### Status chip

Mapiranje:

```text
success: completed / zavrsen
warning: ceka dijelove / needs technician
info: received / in progress / normal active
danger: otkazan / impossible missing skill
```

Boje:

- success background alpha od `success`, text svijetlo zelen.
- warning background alpha od `warning`, text svijetlo narancast.
- info background alpha od `accentCyan`, text svijetlo cyan.
- danger background alpha od `danger`, text svijetlo crven.

### Buttons

Primary:

- Gold fill/gradient.
- Dark text.
- Koristiti za main action: login, create, save, start repair.

Secondary:

- Transparent/dark surface.
- Light text.
- Stroke.

Danger:

- Danger fill or outlined danger.
- Koristiti samo za delete/destructive actions.

### Forms

Web referenca: `Areas/Identity/Pages/Account/Register.cshtml` i `.gc-form` styles.

Android:

- Inputi imaju tamnu pozadinu, stroke border.
- Focus border cyan.
- Validation error kao red/danger helper text ili small danger panel.
- Form card s max width nije relevantan na mobile; koristiti full width.
- Date/time picker treba biti native Android picker ili Compose Material date/time picker, ne custom web clone.

### List controls

Web referenca: `Views/Shared/_ListControls.cshtml`.

Android:

- Sort kao dropdown/menu.
- Amount/page size vjerojatno nije potreban na mobile ako se koristi infinite scroll.
- Customer filter kao search field/autocomplete.
- Asc/Desc kao segmented control.
- "Showing X of Y" kao muted caption.

## Screen stateovi

Svaki list screen treba imati:

- Loading state: skeleton kartice ili centered progress.
- Empty state: kratka korisna poruka.
- Error state: danger panel + retry.
- Unauthorized state: ocisti tokene i vrati na login.
- Forbidden state (`403`): prikazi "Nemate ovlasti za ovaj dio aplikacije."

## Android implementation notes

Preporuka za moderni Android:

- Jetpack Compose.
- Material 3 kao baza, ali custom dark color scheme prema tokenima gore.
- Retrofit/OkHttp za API.
- OkHttp interceptor za `Authorization: Bearer`.
- Authenticator/interceptor za refresh token flow.
- Encrypted storage za refresh token.

Ako se koristi XML/Views umjesto Compose, isti tokeni i patterni vrijede.

## Compose theme sketch

```kotlin
private val DarkColors = darkColorScheme(
    background = Color(0xFF0A1020),
    surface = Color(0xFF0C121E),
    primary = Color(0xFFD7B36A),
    secondary = Color(0xFF76D1FF),
    tertiary = Color(0xFF8A7CFF),
    error = Color(0xFFFF6B81),
    onBackground = Color(0xFFEDF2FF),
    onSurface = Color(0xFFEDF2FF),
    onPrimary = Color(0xFF081018)
)
```

Cards should still manually use subtle borders and slightly different dark surfaces, because Material defaults alone will look flatter than the current web UI.

## Copy and language

The web app mixes English operational UI and Croatian customer portal text. For Android, pick one language strategy:

- If target users are Croatian customers: use Croatian for customer-facing screens (`Moj servis`, `Gitare`, `Servisni nalozi`).
- If target users are staff/admin: English operational labels are acceptable because the current dashboard uses English (`Repairs`, `Customers`, `Technicians`).

Do not mix languages inside the same screen unless matching existing domain terms.

## Backend/API relation

Use `API_HANDOFF_ANDROID.md` for endpoint and auth details. Design-wise:

- `User` app should prioritize `/api/me/guitars` and `/api/me/service-orders`.
- `Manager/Admin` app should prioritize dashboard, repairs queue, customers, guitars, technicians.
- After login, call `/api/auth/me` and use roles to choose the initial navigation.

## Files that define current web design

- Global layout/navigation: `Views/Shared/_Layout.cshtml`
- Global CSS/design tokens: `wwwroot/css/site.css`
- Dashboard: `Views/Home/Index.cshtml`
- Customer portal: `Views/MojServis/Index.cshtml`
- Repairs queue: `Views/Repairs/Index.cshtml`
- Customers list: `Views/Customers/Index.cshtml`
- Shared list controls: `Views/Shared/_ListControls.cshtml`
- Identity form styling example: `Areas/Identity/Pages/Account/Register.cshtml`

The Android app should preserve the visual language, but not the desktop layout. Translate top navigation and wide grids into mobile-first navigation, LazyColumn lists, bottom nav, and detail screens.
