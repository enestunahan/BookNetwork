# Authentication & Authorization Kurulumu

Bu dökümanda BookNetwork projesine entegre ettiğimiz **ASP.NET Core Identity + JWT + Refresh Token** yapısı anlatılıyor. Clean Architecture (Domain / Application / Infrastructure / Persistence / API) katmanlarına nasıl yerleştiğini katman katman inceleyeceğiz.

---

## 1. Mimari Genel Bakış

```
Domain
 └─ Entities/Identity/  (AppUser, AppRole, Endpoint)

Application
 ├─ Common/Security/    (JwtTokenOptions, TokenDto, ITokenService)
 ├─ Common/Exceptions/  (AuthenticationFailedException, BusinessException, NotFoundException)
 └─ Features/           (Auth, Roles, Users — CQRS/MediatR)

Infrastructure
 ├─ Security/JwtTokenService.cs
 └─ InfrastructureServiceRegistration.cs   (DI)

Persistence
 ├─ Contexts/BookNetworkDbContext.cs       (artık IdentityDbContext<AppUser, AppRole, string>)
 ├─ Configurations/Identity/               (AppUser, Endpoint config)
 └─ PersistenceServiceRegistration.cs      (AddIdentityCore + EF Stores)

API
 ├─ Extensions/AuthenticationExtensions.cs (JWT Bearer setup)
 ├─ Middlewares/ExceptionHandlingMiddleware.cs
 └─ Controllers/ (AuthController, Admin/RolesController, Admin/UsersController)
```

**Akış özeti:**
1. `AppUser` / `AppRole` Domain katmanında — temiz tutuyoruz, Identity'den türüyorlar.
2. `UserManager<AppUser>` ve `RoleManager<AppRole>` servislerini MediatR handler'ları kullanıyor — yani Application katmanı Identity abstraction'ı üzerinden iş mantığını yürütüyor.
3. JWT üretimi `Infrastructure` katmanında `JwtTokenService` tarafından yapılıyor.
4. Token doğrulaması **ayrı bir middleware yazmıyoruz** — bunu ASP.NET Core'un built-in `UseAuthentication()` + `JwtBearer` middleware'i hallediyor (best practice bu). Biz sadece pipeline'a ekliyoruz.
5. Authorization `[Authorize]` attribute'u ile controller/action seviyesinde.

---

## 2. Eklenen / Değişen Dosyalar

### Domain
- `src/Core/BookNetwork.Domain/Entities/Identity/AppUser.cs`  
  `IdentityUser<string>` türetiyor. `NameSurname`, `RefreshToken`, `RefreshTokenEndDate` alanları eklendi.
- `src/Core/BookNetwork.Domain/Entities/Identity/AppRole.cs`  
  `IdentityRole<string>` türetiyor. `ICollection<Endpoint>` navigation var → many-to-many ilişkisi.
- `src/Core/BookNetwork.Domain/Entities/Identity/Endpoint.cs`  
  Yeni entity. Role-based endpoint yetkilendirmesi için (Code, ActionType, HttpType, Definition, Menu + Roles nav).
- `BookNetwork.Domain.csproj` → `Microsoft.Extensions.Identity.Stores` paketi eklendi (IdentityUser/IdentityRole için).

### Application
- `Common/Security/TokenOptions.cs` → `JwtTokenOptions` (Identity'nin kendi `TokenOptions` sınıfıyla çakışmasın diye isim değişti).
- `Common/Security/TokenDto.cs` — AccessToken + expiration + RefreshToken döndürüyor.
- `Common/Security/ITokenService.cs` — Infrastructure'ın implement edeceği contract.
- `Common/Exceptions/` — 401/400/404 ayrımı için özel exception'lar.
- `Features/Auth/Commands/` → `Register`, `Login`, `RefreshTokenLogin`, `Logout` (MediatR).
- `Features/Roles/` → `CreateRole`, `GetRoles`, `AssignRoleToUser`.
- `Features/Users/` → `GetUsers`, `GetUserClaims`, `AssignClaimToUser`.
- `BookNetwork.Application.csproj` → `Microsoft.Extensions.Identity.Core` eklendi (UserManager/RoleManager için).

### Persistence
- `Contexts/BookNetworkDbContext.cs` → `DbContext` yerine `IdentityDbContext<AppUser, AppRole, string>`. `DbSet<Endpoint>` eklendi. `base.OnModelCreating(modelBuilder)` mutlaka çağrılmalı (Identity tabloları bu sayede oluşuyor).
- `Configurations/Identity/AppUserConfiguration.cs` — `NameSurname` max length, `RefreshToken` length.
- `Configurations/Identity/EndpointConfiguration.cs` — kolon length'leri, unique `Code` index, `Endpoints ↔ Roles` many-to-many join tablosu (`RoleEndpoints`).
- `PersistenceServiceRegistration.cs` → `AddIdentityCore<AppUser>().AddRoles<AppRole>().AddEntityFrameworkStores<BookNetworkDbContext>()`.
- `BookNetwork.Persistence.csproj` → `Microsoft.AspNetCore.Identity.EntityFrameworkCore` eklendi.

### Infrastructure
- `Security/JwtTokenService.cs` — JWT üretimi + rastgele refresh token. Claim'lere user id, email, username, roller ve user claim'leri basıyor.
- `InfrastructureServiceRegistration.cs` → `AddInfrastructure(...)`; `JwtTokenOptions` bağlıyor + `ITokenService` register ediyor.
- `BookNetwork.Infrastructure.csproj` → `Microsoft.IdentityModel.Tokens`, `System.IdentityModel.Tokens.Jwt` eklendi.

### API
- `Extensions/AuthenticationExtensions.cs` → `AddJwtAuthentication` extension'ı (Issuer/Audience/Key doğrulaması, `ClockSkew = 0`).
- `Middlewares/ExceptionHandlingMiddleware.cs` — custom exception → HTTP status eşlemesi (401/400/404/500).
- `Controllers/AuthController.cs` — `register`, `login`, `refresh-token-login`, `logout`.
- `Controllers/Admin/RolesController.cs` — `[Authorize(Roles = "Admin")]` korumalı; role CRUD + kullanıcıya rol atama.
- `Controllers/Admin/UsersController.cs` — kullanıcı listeleme, oluşturma, claim listeleme/ekleme.
- `Controllers/Admin/AdminBooksController.cs` → `[Authorize(Roles = "Admin")]` eklendi.
- `Program.cs` → `AddInfrastructure`, `AddJwtAuthentication`, `UseAuthentication`, `UseAuthorization`, `UseMiddleware<ExceptionHandlingMiddleware>` pipeline'a dahil.
- `BookNetwork.API.csproj` → `Microsoft.AspNetCore.Authentication.JwtBearer` paketi + `BookNetwork.Infrastructure` project reference.
- `appsettings.json` → `TokenOptions` bölümü.

---

## 3. Best Practice Kararları

### 3.1. `AddIdentityCore` vs `AddIdentity`
JWT tabanlı API'de **`AddIdentityCore`** kullanıyoruz. `AddIdentity` cookie authentication handler'ları da register eder — bizim gibi API'ler için gereksiz ve default scheme karmaşası yaratır.

### 3.2. Token Validation'ı Middleware Üzerinden
Yeni bir middleware yazmadık. ASP.NET Core'un `JwtBearer` handler'ı zaten `UseAuthentication()` ile pipeline'a giriyor ve her request için `Authorization: Bearer <token>` header'ını doğruluyor. Yeniden yazmak **anti-pattern** olur.

### 3.3. Refresh Token'ı DB'de Tut
Refresh token `AppUser.RefreshToken` kolonunda. Her yeni `Login` veya başarılı `RefreshTokenLogin` sonrası **rotate ediyoruz** (yeni refresh token üretip eskiyi geçersiz kılıyoruz). Çalındığında süre dolana kadar kullanılabilir — bu yüzden refresh süresi kısa (7 gün) tutulmalı.

### 3.4. Access Token Süresi Kısa
`AccessTokenExpirationMinutes: 15` → token çalınsa bile saldırı penceresi dar. Uzun ömürlü state `RefreshToken`'da.

### 3.5. `ClockSkew = TimeSpan.Zero`
Varsayılan 5 dk grace period kapatıldı — expiration tam saniyesinde geçerli olsun.

### 3.6. Lockout
Identity lockout default açık: 5 başarısız denemede 5 dk kilit → brute force koruması.

### 3.7. Custom Exception → HTTP Status
`ExceptionHandlingMiddleware` → Application'da fırlatılan `AuthenticationFailedException` vb. HTTP response'a temiz JSON olarak dönüyor.

### 3.8. Claim'ler Token İçinde
JWT'yi her istekte doğrulayacağımız için kullanıcının rollerini ve claim'lerini token içine gömdük → her istekte DB'ye gitmek zorunda kalmıyoruz. **Dezavantaj:** rol/claim değişince token expire olana kadar yansımaz. Access token süresi kısa tutulduğu için pratikte sorun olmaz.

---

## 4. Configuration — appsettings.json

```json
"TokenOptions": {
  "Issuer": "BookNetwork",
  "Audience": "BookNetwork.Client",
  "SecurityKey": "CHANGE_ME_MINIMUM_32_CHAR_SUPER_SECRET_KEY_!!",
  "AccessTokenExpirationMinutes": 15,
  "RefreshTokenExpirationDays": 7
}
```

> **ÖNEMLİ:** `SecurityKey` üretimde **ASLA** repo'da olmamalı. User Secrets / Environment Variables / Azure Key Vault kullan:
> ```bash
> dotnet user-secrets init --project src/Presentation/BookNetwork.API
> dotnet user-secrets set "TokenOptions:SecurityKey" "$(openssl rand -base64 48)"
> ```
> Minimum 32 karakter olmalı (HMAC-SHA256 için).

---

## 5. Database Migration

Mevcut `Migrations/` klasöründeki migration Identity tabloları yokken oluşturuldu. Identity tabloları (`AspNetUsers`, `AspNetRoles`, vs.) ve `Endpoints`, `RoleEndpoints` eklendiği için yeni migration şart.

**İki seçenek:**

**A) Dev ortamı sıfırla (en kolay):**
```bash
# PostgreSQL'de DB'yi sil, Program.cs zaten EnsureCreated ile yeniden oluşturur
dropdb -U postgres BookNetworkDb
dotnet run --project src/Presentation/BookNetwork.API
```

**B) Yeni migration (doğru yol):**
```bash
# Eski migration'ları sil
rm src/Infrastructure/BookNetwork.Persistence/Migrations/*.cs

# Yeni migration
dotnet ef migrations add InitialIdentity \
  --project src/Infrastructure/BookNetwork.Persistence \
  --startup-project src/Presentation/BookNetwork.API

dotnet ef database update \
  --project src/Infrastructure/BookNetwork.Persistence \
  --startup-project src/Presentation/BookNetwork.API
```

> Program.cs'deki `EnsureCreatedAsync()` **migration kullanmıyor**; migration'a geçmek istiyorsan onu `Database.MigrateAsync()` ile değiştirmelisin.

---

## 6. API Endpoint'leri

### Public
| Method | Route | Açıklama |
|--------|-------|----------|
| POST | `/api/auth/register` | Yeni kullanıcı oluştur |
| POST | `/api/auth/login` | Access + Refresh token al |
| POST | `/api/auth/refresh-token-login` | Refresh token ile yeni access token |

### Authenticated (Bearer token gerekli)
| Method | Route | Açıklama |
|--------|-------|----------|
| POST | `/api/auth/logout` | Refresh token'ı iptal et |

### Admin only (`[Authorize(Roles = "Admin")]`)
| Method | Route | Açıklama |
|--------|-------|----------|
| GET | `/api/admin/roles` | Tüm rolleri listele |
| POST | `/api/admin/roles` | Yeni rol oluştur |
| POST | `/api/admin/roles/assign-to-user` | Kullanıcıya rol ata |
| GET | `/api/admin/users` | Tüm kullanıcıları listele |
| POST | `/api/admin/users` | Admin üzerinden kullanıcı oluştur |
| GET | `/api/admin/users/{userId}/claims` | Kullanıcının claim'leri |
| POST | `/api/admin/users/{userId}/claims` | Kullanıcıya claim ata |
| GET | `/api/adminBooks/books` | Admin kitap listesi |

### Örnek Request Body'ler

**Register:**
```json
POST /api/auth/register
{
  "nameSurname": "Enes Çömez",
  "userName": "enes",
  "email": "enes@example.com",
  "password": "Test123",
  "passwordConfirm": "Test123"
}
```

**Login:**
```json
POST /api/auth/login
{ "userNameOrEmail": "enes", "password": "Test123" }
```
Response:
```json
{
  "accessToken": "eyJhbGc...",
  "accessTokenExpiration": "2026-04-19T15:30:00Z",
  "refreshToken": "a83f...",
  "refreshTokenExpiration": "2026-04-26T15:15:00Z"
}
```

**Refresh:**
```json
POST /api/auth/refresh-token-login
{ "refreshToken": "a83f..." }
```

**Authenticated requests:**
```
Authorization: Bearer eyJhbGc...
```

---

## 7. Senin Unuttuklarını Hatırlatıyorum / Bunları da Yapabiliriz 👇

Bunlar henüz yapılmadı; proje büyüdükçe ihtiyaç olabilir. Kısaca başlıklar hâlinde:

1. **İlk Admin kullanıcısını seed'le**  
   Şu an `[Authorize(Roles = "Admin")]` endpoint'lerini kimse çağıramaz çünkü henüz Admin rolünde kullanıcı yok. Best practice: `Persistence/Seed/IdentitySeeder.cs` ekleyip startup'ta "Admin" rolünü ve ilk admin kullanıcısını configuration'dan oluşturmak.

2. **Migration'a geç**  
   `EnsureCreatedAsync` → `MigrateAsync`. Üretim için migration şart.

3. **SecurityKey'i User Secrets'a taşı**  
   appsettings.json'daki key placeholder. User Secrets ya da environment variable'a taşı. Minimum 32 karakter olmalı.

4. **FluentValidation**  
   `RegisterCommand`, `LoginCommand` vb. için input validasyonu. MediatR pipeline behavior olarak ekleyebiliriz (`ValidationBehavior<TRequest,TResponse>`).

5. **Global MediatR pipeline behavior'ları**  
   Logging + validation + transaction behavior → cross-cutting.

6. **Endpoint → Role ilişkisini fiilen devreye sok**  
   `Endpoint` entity'sini oluşturduk ama şu an sadece role name üzerinden yetki veriyoruz (`[Authorize(Roles = "Admin")]`). "Hangi role hangi endpoint'e erişebilir" kontrolünü runtime'da yapmak için:
   - `/api/admin/endpoints` CRUD controller
   - Custom `IAuthorizationHandler` veya `ActionFilter` yazıp her request'te `Endpoint.Code == action.Code && role ∈ endpoint.Roles` kontrolü yapmak
   - Uygulama açılışında reflection ile `[HttpGet/Post]` action'ları DB'ye sync etme
   Bu **ciddi bir iş** — şimdilik kapsam dışı bıraktım.

7. **Swagger/OpenAPI'ye Bearer token desteği**  
   Şu an `AddOpenApi` var ama Swagger UI'da "Authorize" butonu için `SwaggerGen` + security definition konfigürasyonu gerek. Veya `Scalar` / `NSwag` kullan.

8. **Refresh token'ı hash'le**  
   Şu an DB'de düz saklanıyor. En iyi güvenlik için SHA256 hash'ini saklamak gerekir (DB sızarsa token'lar kullanılamaz).

9. **HTTPS-only refresh token (cookie)**  
   Daha ileri: refresh token'ı response body yerine `HttpOnly Secure SameSite=Strict` cookie'de dön. XSS'e karşı koruma.

10. **Rate limiting / CORS**  
    `AddRateLimiter` + `AddCors` konfigürasyonu. Özellikle `/login` brute-force'a karşı.

11. **Audit log**  
    `IdentityUserClaim`, `IdentityUserRole` tablolarına kim ne zaman ne ekledi bilgisini tutmak için `Created*` / `Modified*` kolonları + `SaveChangesInterceptor`.

12. **Email confirmation / password reset**  
    `AddDefaultTokenProviders()` + email sender service.

13. **DTO ↔ Command mapping**  
    Controller'da `RegisterCommand`'ı doğrudan `[FromBody]` aldık. Üretimde DTO ayrı tutup `Mapster`/`AutoMapper` ile dönüştürmek yaygın.

14. **BaseController + global ProblemDetails**  
    `ExceptionHandlingMiddleware` basit JSON dönüyor; `ProblemDetails` RFC 7807 standardına geçmek daha temiz olur.

15. **Integration test'ler**  
    `WebApplicationFactory` + Testcontainers (PostgreSQL) ile auth flow test'leri.

---

## 8. Hızlı Duman Testi

```bash
# API'yi çalıştır
dotnet run --project src/Presentation/BookNetwork.API

# Kayıt
curl -X POST http://localhost:5000/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{"nameSurname":"Test","userName":"test","email":"t@t.com","password":"Test123","passwordConfirm":"Test123"}'

# Login
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"userNameOrEmail":"test","password":"Test123"}'

# Korunan endpoint (Admin rolü olmadığı için 403 dönmeli)
curl http://localhost:5000/api/admin/users \
  -H "Authorization: Bearer <accessToken>"
```

---

## 9. Özet

- `AppUser`, `AppRole`, `Endpoint` → Domain.
- Identity Core + EF Stores → Persistence.
- JWT üretimi + options binding → Infrastructure.
- MediatR handler'ları (auth/user/role) → Application.
- Controllers + JWT Bearer middleware + exception middleware → API.
- Token doğrulaması `UseAuthentication()` tarafından, custom middleware **gerekmedi**.
- Admin endpoint'leri `[Authorize(Roles = "Admin")]` ile korundu.
