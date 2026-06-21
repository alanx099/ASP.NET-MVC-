# API Handoff za Android aplikaciju

Ovaj backend je ASP.NET Core MVC/API aplikacija za servis gitara. Web dio i dalje koristi ASP.NET Core Identity cookie login, ali API sada podrzava JWT Bearer access token + refresh token flow. Android aplikacija se treba spajati preko JSON REST API-ja i za autorizirane pozive slati `Authorization: Bearer <accessToken>`.

## Backend URL

Lokalni backend se pokrece prema `Properties/launchSettings.json`:

- HTTPS: `https://localhost:6969`
- HTTP: `http://localhost:6970`

Za Android emulator `localhost` znaci sam emulator, ne racunalo. Zato Android klijent obicno treba koristiti:

- `http://10.0.2.2:6970` za Android Studio emulator
- `http://<LAN-IP-racunala>:6970` za fizicki uredaj na istoj mrezi

Za lokalni razvoj je najjednostavnije koristiti HTTP endpoint `6970`. Ako se koristi HTTPS u emulatoru, treba rijesiti trust za dev certificate.

## Autorizacija

Auth endpointi su u `Controllers/Api/AuthApiController.cs`.

### Login

`POST /api/auth/login`

Request:

```json
{
  "email": "admin@test.com",
  "password": "Admin123!"
}
```

Response:

```json
{
  "tokenType": "Bearer",
  "accessToken": "eyJhbGciOiJIUzI1NiIs...",
  "accessTokenExpiresAtUtc": "2026-06-21T19:30:00Z",
  "refreshToken": "base64-refresh-token",
  "refreshTokenExpiresAtUtc": "2026-07-05T19:00:00Z"
}
```

Seedani test korisnici su u `Data/IdentitySeed.cs`:

- `admin@test.com` / `Admin123!` / role: `Admin`
- `manager@test.com` / `Manager123!` / role: `Manager`
- `user@test.com` / `User123!` / role: `User`

### Slanje autoriziranog API poziva

Za svaki zasticeni endpoint Android mora dodati header:

```http
Authorization: Bearer <accessToken>
Accept: application/json
Content-Type: application/json
```

Primjer:

```http
GET /api/auth/me
Authorization: Bearer eyJhbGciOiJIUzI1NiIs...
```

### Refresh access tokena

Access token traje kratko, refresh token traje duze. Ako API vrati `401 Unauthorized`, Android treba probati refresh.

`POST /api/auth/refresh`

Request:

```json
{
  "refreshToken": "base64-refresh-token"
}
```

Response je isti oblik kao login response. Backend rotira refresh token: nakon uspjesnog refresh poziva stari refresh token vise ne treba koristiti, nego spremiti novi `refreshToken` iz responsea.

### Logout / revoke refresh tokena

`POST /api/auth/revoke`

Request:

```json
{
  "refreshToken": "base64-refresh-token"
}
```

Response: `204 No Content`

Android nakon toga treba obrisati lokalno spremljeni access token i refresh token.

### Trenutni korisnik

`GET /api/auth/me`

Response:

```json
{
  "id": "identity-user-id",
  "email": "admin@test.com",
  "roles": ["Admin"]
}
```

## Kako spremati tokene na Androidu

Preporuka:

- `accessToken`: drzati u memoriji dok app radi, moze i u sigurnom storageu ako je potrebno prezivjeti restart.
- `refreshToken`: spremiti u Android Keystore-backed storage, npr. EncryptedSharedPreferences ili DataStore uz encryption wrapper.
- Ne spremati tokene u obicni SharedPreferences bez enkripcije.
- Ne logirati tokene u Logcat.

Predlozeni klijentski flow:

1. App starta i cita spremljeni refresh token.
2. Ako refresh token postoji, pozove `/api/auth/refresh`.
3. Ako refresh uspije, spremi novi access + refresh token.
4. Ako refresh ne uspije, prikaze login.
5. Za svaki API poziv doda `Authorization: Bearer <accessToken>`.
6. Ako dobije `401`, jednom pokusa refresh i ponovi originalni request.
7. Ako refresh opet ne uspije, cisti tokene i vraca korisnika na login.

## Role i pristup endpointima

Backend koristi ASP.NET Core role autorizaciju.

- `Admin`: puni pristup, ukljucujuci delete endpointe.
- `Manager`: pristup vecini CRUD endpointa, ali delete endpointi su uglavnom samo `Admin`.
- `User`: pristup osobnom API-ju pod `/api/me`.

Osnovna pravila iz controllera:

- Svi controlleri koji nasljeduju `ApiControllerBase` traze `Admin` ili `Manager`.
- Vecina `DELETE` akcija dodatno trazi samo `Admin`.
- `MeApiController` trazi `User` ili `Admin`.
- Auth login/refresh/revoke su anonymous.

## Endpointi za Android

### Auth

- `POST /api/auth/login`
- `POST /api/auth/refresh`
- `POST /api/auth/revoke`
- `GET /api/auth/me`

### Osobni korisnicki API

Za klijentsku Android aplikaciju obicnog korisnika ovo je vjerojatno najbitniji dio:

- `GET /api/me/guitars`
- `GET /api/me/service-orders`

Ovi endpointi vracaju gitare i servisne naloge povezane s trenutno prijavljenim Identity korisnikom.

### Admin/manager API

Ovi endpointi su za administratorski ili managerski dio aplikacije:

- `GET /api/customers?query=&officeId=&take=50`
- `GET /api/customers/{id}`
- `POST /api/customers`
- `PUT /api/customers/{id}`
- `DELETE /api/customers/{id}` - samo `Admin`

- `GET /api/guitars?query=&customerId=&take=50`
- `GET /api/guitars/{id}`
- `POST /api/guitars`
- `PUT /api/guitars/{id}`
- `DELETE /api/guitars/{id}` - samo `Admin`

- `GET /api/repairs?query=&customerId=&technicianId=&statusId=&take=50`
- `GET /api/repairs/{id}`
- `POST /api/repairs`
- `PUT /api/repairs/{id}`
- `DELETE /api/repairs/{id}` - samo `Admin`

- `GET /api/offices?query=&take=50`
- `GET /api/offices/{id}`
- `POST /api/offices`
- `PUT /api/offices/{id}`
- `DELETE /api/offices/{id}` - samo `Admin`

- `GET /api/employees?query=&officeId=&take=50`
- `GET /api/employees/{id}`
- `POST /api/employees`
- `PUT /api/employees/{id}`
- `DELETE /api/employees/{id}` - samo `Admin`

- `GET /api/technicians?query=&officeId=&take=50`
- `GET /api/technicians/{id}`
- `POST /api/technicians`
- `PUT /api/technicians/{id}`
- `DELETE /api/technicians/{id}` - samo `Admin`

- `GET /api/knowledge?technicianId=&guitarTypeId=&repairTypeId=&take=50`
- `GET /api/knowledge/{technicianId}/{guitarTypeId}/{repairTypeId}`
- `POST /api/knowledge`
- `PUT /api/knowledge/{technicianId}/{guitarTypeId}/{repairTypeId}`
- `DELETE /api/knowledge/{technicianId}/{guitarTypeId}/{repairTypeId}` - samo `Admin`

### Lookup endpointi

Lookup endpointi sluze za dropdown/spinner podatke:

- `GET /api/brands`
- `GET /api/guitar-types`
- `GET /api/repair-statuses`
- `GET /api/repair-types`

Imaju i CRUD varijante (`GET /{id}`, `POST`, `PUT /{id}`, `DELETE /{id}`), ali delete je samo `Admin`.

## DTO napomene

DTO klase su u:

- `Dtos/AuthDtos.cs`
- `Dtos/ApiDtos.cs`
- `Dtos/MyDtos.cs`

ASP.NET Core JSON serializer po defaultu vraca camelCase property imena. Zato C# `AccessToken` dolazi kao JSON `accessToken`, `DatumOtvaranja` kao `datumOtvaranja`, itd.

Datumi se salju kao ISO-8601 stringovi. Android ih treba parsirati kao `Instant`, `OffsetDateTime` ili `LocalDateTime`, ovisno o ekranu. Auth expiry vrijednosti su UTC (`accessTokenExpiresAtUtc`, `refreshTokenExpiresAtUtc`).

## Primjeri modela za Android

Kotlin primjer za login response:

```kotlin
data class TokenResponse(
    val tokenType: String,
    val accessToken: String,
    val accessTokenExpiresAtUtc: String,
    val refreshToken: String,
    val refreshTokenExpiresAtUtc: String
)
```

Kotlin primjer za trenutnog korisnika:

```kotlin
data class CurrentUser(
    val id: String,
    val email: String,
    val roles: List<String>
)
```

Kotlin primjer za osobni servisni nalog:

```kotlin
data class MyServiceOrder(
    val id: Long,
    val gitaraId: Long,
    val gitara: String,
    val vrstaPopravka: String,
    val status: String,
    val opisKvara: String,
    val datumOtvaranja: String,
    val datumZatvaranja: String?
)
```

## Backend implementacijske reference

Ako sljedeci AI treba provjeriti ili prosiriti auth:

- JWT konfiguracija: `Program.cs`
- JWT opcije: `Options/JwtOptions.cs`
- Token service: `Services/JwtTokenService.cs`
- Auth controller: `Controllers/Api/AuthApiController.cs`
- Refresh token model: `models/RefreshToken.cs`
- Refresh token EF konfiguracija: `Data/AppDbContext.cs`
- Refresh token migracija: `Migrations/20260621204208_AddRefreshTokens.cs`

Bitno: API sada podrzava Bearer tokene, ali MVC web stranice i dalje mogu koristiti Identity cookie. To je namjerno. Android aplikacija ne treba koristiti cookie auth; treba koristiti samo `/api/auth/login`, `accessToken`, `refreshToken` i Bearer header.
