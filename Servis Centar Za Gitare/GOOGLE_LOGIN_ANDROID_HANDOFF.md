# Google Login Handoff za Android

Ovaj dokument objasnjava kako Android aplikacija treba implementirati Google login za ovaj backend.

Vazno: trenutni backend vec ima Google login za MVC web flow (`AddGoogle` u `Program.cs`), ali to je browser redirect + Identity cookie flow. Android aplikacija ne treba i ne moze dobro koristiti taj MVC cookie flow. Za Android treba napraviti native Google Sign-In flow i zatim backend endpoint koji izdaje nase JWT access/refresh tokene.

## Kratki odgovor o credentials

Mozemo koristiti isti Google Cloud projekt i isti OAuth consent screen.

Ne treba dijeliti isti credential kao da je sve jedan client. Trebaju postojati odvojeni OAuth client IDs po platformi:

- Web OAuth client: koristi backend/MVC Google login. Ima `ClientId` i `ClientSecret`. Secret ostaje samo na backendu.
- Android OAuth client: koristi Android aplikacija. Vezan je uz package name i SHA-1/SHA-256 potpis aplikacije. Nema client secret za cuvanje u aplikaciji.

Android aplikaciji se ne smije dati web `ClientSecret`.

Za backend-verifikaciju Google ID tokena Android najcesce konfigurira Google Sign-In/Credential Manager sa server/web client ID-jem kao `serverClientId`, dobije Google `idToken`, posalje ga backendu, a backend verificira token i izda nase JWT tokene.

## Ciljani flow

```text
Android app
  -> Google Credential Manager / Sign in with Google
  -> dobije Google ID token
  -> POST /api/auth/google sa { idToken }

Backend
  -> verificira Google ID token
  -> pronade ili kreira AppUser
  -> po potrebi linka Stranka profil
  -> izda nas TokenResponseDto

Android app
  -> sprema accessToken + refreshToken
  -> dalje koristi Authorization: Bearer <accessToken>
```

Nakon Google login-a Android vise ne koristi Google token za nase API pozive. Za nase API pozive koristi samo backend JWT token.

## Sto Android treba implementirati

Preporuceni moderni pristup je Android Credential Manager + Sign in with Google.

Android treba:

1. Konfigurirati Sign in with Google u Google Cloud/Auth Platform.
2. U aplikaciji dodati Google/Credential Manager dependencies.
3. Zatraziti Google credential.
4. Iz credentiala izvaditi Google ID token.
5. Poslati ID token backendu:

```http
POST /api/auth/google
Content-Type: application/json

{
  "idToken": "GOOGLE_ID_TOKEN"
}
```

6. Backend vraca isti oblik responsea kao email/password login:

```json
{
  "tokenType": "Bearer",
  "accessToken": "eyJhbGciOiJIUzI1NiIs...",
  "accessTokenExpiresAtUtc": "2026-06-22T12:30:00Z",
  "refreshToken": "base64-refresh-token",
  "refreshTokenExpiresAtUtc": "2026-07-06T12:00:00Z"
}
```

7. Android spremi nase tokene i dalje radi isto kao kod obicnog login-a.

## Backend endpoint

Backend sada ima:

- `POST /api/auth/login`
- `POST /api/auth/google`
- `POST /api/auth/refresh`
- `POST /api/auth/revoke`
- `GET /api/auth/me`

Request DTO:

```csharp
public class GoogleLoginRequestDto
{
    [Required]
    public string IdToken { get; set; } = string.Empty;
}
```

Backend flow:

1. Primi `idToken`.
2. Verificira potpis, issuer, expiry i audience.
3. Iz tokena uzme:
   - `sub` kao stabilni Google user id.
   - `email`.
   - `email_verified`.
   - `given_name`, `family_name`, `name`, `picture` ako treba.
4. Ako email nije verified, odbiti login.
5. Pronaci postojeceg `AppUser` po emailu ili Google login provider keyu.
6. Ako korisnik ne postoji, kreirati `AppUser`.
7. Dodati default role `User`, osim ako aplikacija ima drugu poslovnu logiku.
8. Opcionalno povezati/kreirati `Stranka` kroz `CustomerAccountLinker`.
9. Izdati `TokenResponseDto` koristeci postojece `JwtTokenService`.
10. Spremiti refresh token kao i kod `/api/auth/login`.

Backend ne smije vjerovati obicnom user id-u koji Android posalje. Mora verificirati Google ID token.

## Credentials setup

U Google Cloud/Auth Platform:

1. Koristiti isti Google Cloud projekt ako vec postoji za web Google login.
2. OAuth consent screen moze biti isti.
3. Zadrzati postojeci Web OAuth client za backend/MVC.
4. Dodati novi Android OAuth client za Android aplikaciju:
   - package name, npr. `com.example.guitarservice`
   - SHA-1 debug keystore fingerprint za debug build
   - kasnije SHA-1/SHA-256 release signing fingerprint za production/release
5. Backend konfiguracija treba znati koji audience/client ID prihvaca.

Prakticna preporuka:

- `Authentication:Google:ClientId` i `ClientSecret` ostaju za MVC web flow.
- Dodati npr. `Authentication:Google:ServerClientId` ili `Authentication:Google:AllowedClientIds`.
- Backend verifikacija neka dopusti samo nase stvarne client ID-jeve.

Ne stavljati `ClientSecret` u Android app.

## Koji client ID ide gdje?

Najbitnije pravilo:

- Android app koristi client ID potreban Google Sign-In/Credential Manager flowu.
- Ako Android trazi ID token za backend, obicno koristi server/web client ID kao `serverClientId`.
- Backend provjerava `aud` claim Google ID tokena i prihvaca samo client ID-jeve koje mi kontroliramo.

Ako bude vise Android buildova:

- debug app moze imati debug Android OAuth client.
- release app treba release Android OAuth client.
- backend moze imati allowed audience listu ako tokeni dolaze s vise client ID-jeva.

## Android pseudo-code

Shape za Android sloj:

```kotlin
data class GoogleLoginRequest(
    val idToken: String
)

data class TokenResponse(
    val tokenType: String,
    val accessToken: String,
    val accessTokenExpiresAtUtc: String,
    val refreshToken: String,
    val refreshTokenExpiresAtUtc: String
)
```

API:

```kotlin
interface AuthApi {
    @POST("api/auth/google")
    suspend fun googleLogin(@Body request: GoogleLoginRequest): TokenResponse
}
```

Client flow:

```kotlin
val googleIdToken: String = signInWithGoogleAndGetIdToken()
val tokens = authApi.googleLogin(GoogleLoginRequest(googleIdToken))
tokenStore.save(tokens.accessToken, tokens.refreshToken)
```

Nakon toga svi API pozivi idu preko naseg JWT-a:

```http
Authorization: Bearer <accessToken>
```

## Backend response i role

Google login treba vratiti isti `TokenResponseDto` kao obicni login. Android app ne treba posebnu logiku nakon toga.

Nakon login-a Android neka pozove:

```http
GET /api/auth/me
Authorization: Bearer <accessToken>
```

Na temelju `roles` iz responsea odlucuje navigaciju:

- `User`: Moj servis screens.
- `Manager`: operational screens.
- `Admin`: all screens.

## Security notes

- Ne slati Google `sub`, email ili ime kao dokaz login-a. To se moze lazirati.
- Slati samo Google `idToken`, a backend ga mora verificirati.
- Ne spremati Google ID token kao dugorocnu aplikacijsku sesiju.
- Ne koristiti Google access token za nase API-je.
- Ne stavljati Google web `ClientSecret` u Android aplikaciju.
- Refresh token iz naseg backend-a spremiti u encrypted storage.
- Za lokalni development preko HTTP-a je OK samo dok se testira na LAN-u; production mora ici preko HTTPS-a.

## Current backend status

Postojeci Google web login:

- `Program.cs` cita `Authentication:Google:ClientId`
- `Program.cs` cita `Authentication:Google:ClientSecret`
- ako oba postoje, dodaje `.AddGoogle(...)`
- Identity external login UI je u `Areas/Identity/Pages/Account`

To pokriva web/browser login.

Android API Google login je dodan u `Controllers/Api/AuthApiController.cs` kao `POST /api/auth/google`. Endpoint trazi `Authentication:Google:AllowedClientIds` konfiguraciju da bi znao koje Google token audiences smije prihvatiti.

## Official docs used

- Google backend auth docs: send ID token to backend, do not trust plain user IDs, verify signature/audience/issuer/expiry.
- Android Sign in with Google docs: use Credential Manager/Sign in with Google, Google ID token, Google Auth Platform setup.
