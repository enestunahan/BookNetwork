# Authorization (Yetkilendirme) Rehberi

Bu dokümanda rollerin, politikaların ve özel (custom) yetkilendirme kurallarının projede nasıl kurulduğunu, nasıl kullanılacağını ve unutulmaması gereken noktaları bulabilirsin.

---

## 1. Temel Kavramlar — Authentication vs Authorization

- **Authentication (kimlik doğrulama):** "Sen kimsin?" sorusunu yanıtlar. Projede JWT Bearer ile yapılıyor: [AuthenticationExtensions.cs](../src/Presentation/BookNetwork.API/Extensions/AuthenticationExtensions.cs).
- **Authorization (yetkilendirme):** "Bu kullanıcı bu işlemi yapabilir mi?" sorusunu yanıtlar. Rol, politika (policy), claim ve custom requirement'larla şekillenir.

Boru hattı: `UseAuthentication()` → `UseAuthorization()`. Önce kim olduğun bulunur, sonra ne yapabileceğine bakılır.

---

## 2. Roller ve Hiyerarşi

Bu projede 4 rol var (`Admin` > `Editor` > `User` > `Viewer`):

| Rol    | Yetki özeti                                                                 |
| ------ | --------------------------------------------------------------------------- |
| Admin  | Her şeyi yapabilir. Admin rolünü sadece Admin atayabilir.                   |
| Editor | Admin'in yapabildiği işlemlerin neredeyse tamamı, **Admin rol ataması dışında**. |
| User   | Üyelere açık alanları kullanabilir, yönetim işlemi yapamaz.                 |
| Viewer | Sadece okuma (read-only). Edit / insert / delete yapamaz.                   |

**Önemli:** ASP.NET Core'da "rol hiyerarşisi" diye hazır bir şey yok. Hiyerarşiyi biz **policy** olarak tanımlıyoruz — alt seviye policy, üst rolleri de kabul eden bir liste içerir.

Sabitler:

- Rol adları: [AppRoles.cs](../src/Core/BookNetwork.Application/Common/Authorization/AppRoles.cs)
- Policy adları: [AuthPolicies.cs](../src/Presentation/BookNetwork.API/Authorization/AuthPolicies.cs)

String literal kullanma — bu sabitleri kullan ki rename'ler güvenli olsun.

---

## 3. Policy-Based Authorization

Her policy, "bu endpointi kim çağırabilir" sorusuna cevap veren bir kurallar bütünü. Tanımlar [AuthorizationExtensions.cs](../src/Presentation/BookNetwork.API/Extensions/AuthorizationExtensions.cs) içinde:

| Policy       | Kabul edilen roller                  | Ekstra kural            |
| ------------ | ------------------------------------ | ----------------------- |
| `MinAdmin`   | Admin                                | —                       |
| `MinEditor`  | Admin, Editor                        | —                       |
| `MinUser`    | Admin, Editor, User                  | —                       |
| `MinViewer`  | Admin, Editor, User, Viewer          | —                       |
| `Adult`      | Admin, Editor, User                  | Yaş ≥ 18                |

"Minimum X rolü" felsefesi: `MinUser`, "en az User yetkisi gerekir" demek — yani User'dan yukarısı (Admin, Editor, User) girer, Viewer giremez.

### Kullanımı

```csharp
[Authorize(Policy = AuthPolicies.MinEditor)]
public class AdminBooksController : ControllerBase { ... }
```

- Controller seviyesinde policy koyarsan tüm action'lar için geçerli olur.
- Action seviyesinde daha katı bir policy koyabilirsin (örnek: controller `MinEditor`, delete action'ı `MinAdmin`). En katı kural kazanır.
- `[AllowAnonymous]` ile bir endpointi policy'den muaf tutabilirsin.

---

## 4. Custom Requirement: Minimum Yaş

Policy'ler sadece rollerle sınırlı değil. **Requirement + Handler** çiftiyle istediğin iş kuralını koyabilirsin:

- [MinimumAgeRequirement.cs](../src/Presentation/BookNetwork.API/Authorization/MinimumAgeRequirement.cs) — kuralın parametrelerini tutar (`int minimumAge`).
- [MinimumAgeHandler.cs](../src/Presentation/BookNetwork.API/Authorization/MinimumAgeHandler.cs) — kuralın çalışma mantığı. JWT'deki `ClaimTypes.DateOfBirth` claim'ini okuyup yaşı hesaplıyor ve yeterliyse `context.Succeed(requirement)` çağırıyor.

Yaş bilgisi şu zincir ile token'a giriyor:

1. [AppUser.BirthDate](../src/Core/BookNetwork.Domain/Entities/Identity/AppUser.cs) — kullanıcıda saklanır.
2. [JwtTokenService.BuildClaimsAsync](../src/Infrastructure/BookNetwork.Infrastructure/Security/JwtTokenService.cs) — login'de token üretilirken `DateOfBirth` claim'i olarak ekleniyor.
3. Handler token'daki claim'den yaşı çıkarıp kontrol ediyor.

### Yeni custom kural eklemek istersen

1. `IAuthorizationRequirement` implement eden bir record/class yaz.
2. `AuthorizationHandler<TRequirement>` implement eden handler yaz.
3. [AuthorizationExtensions.cs](../src/Presentation/BookNetwork.API/Extensions/AuthorizationExtensions.cs) içinde:
   - Handler'ı `services.AddSingleton<IAuthorizationHandler, YourHandler>()` ile kaydet.
   - Yeni policy'de `policy.AddRequirements(new YourRequirement(...))` ile bağla.

---

## 5. Business Kuralı: Editor, Admin Rolü Atayamaz

Bu sadece policy ile çözülemez çünkü request'in **body'sine** bakmak gerekiyor ("atamak istediği rol `Admin` mi?"). Bu yüzden iş kuralı handler'da uygulanıyor:

[AssignRoleToUserCommandHandler.cs](../src/Core/BookNetwork.Application/Features/Roles/Commands/AssignRoleToUser/AssignRoleToUserCommandHandler.cs)

Kurallar:

- Admin dışındaki kullanıcı `Admin` rolü atayamaz.
- Admin dışındaki kullanıcı, zaten Admin olan bir kullanıcının rollerini değiştiremez (privilege escalation koruması).

Aktif kullanıcıyı öğrenmek için Application katmanında `IHttpContextAccessor`'ı import etmek yerine abstraction kullandık:

- [ICurrentUserService.cs](../src/Core/BookNetwork.Application/Common/Security/ICurrentUserService.cs) — Application katmanındaki soyutlama.
- [CurrentUserService.cs](../src/Infrastructure/BookNetwork.Infrastructure/Security/CurrentUserService.cs) — Infrastructure tarafındaki gerçek implementasyon.

Clean Architecture açısından Application katmanı HTTP'yi bilmek zorunda değil — sadece "kim şu anda giriş yapmış" soyutlamasını biliyor.

---

## 6. Seed Edilen Kullanıcılar

[IdentitySeeder.cs](../src/Infrastructure/BookNetwork.Persistence/Seed/IdentitySeeder.cs) uygulamanın her başlangıcında idempotent çalışır. Şifre: **`Password123!`** (hepsi aynı).

| UserName       | Rol    | Doğum Tarihi | 18+ mı? |
| -------------- | ------ | ------------ | ------- |
| `admin`        | Admin  | 1990-01-01   | ✅      |
| `enes.editor`  | Editor | 1992-05-10   | ✅      |
| `enes.user`    | User   | 2000-03-15   | ✅      |
| `kucuk.user`   | User   | 2014-08-05   | ❌      |
| `enes.viewer`  | Viewer | 1995-02-20   | ✅      |

`kucuk.user` kasıtlı olarak 18 yaşın altında — `Adult` policy'sini test ederken kullan.

---

## 7. Örnek Endpointler

[ExamplesController.cs](../src/Presentation/BookNetwork.API/Controllers/ExamplesController.cs)

### `GET /api/examples/members-only`
- **Policy:** `MinUser`
- Admin / Editor / User erişebilir. Viewer ve anonim kullanıcılar 403 / 401 alır.

### `GET /api/examples/adults-only`
- **Policy:** `Adult` (MinUser + 18 yaş kuralı)
- `enes.user` ile çalışır, `kucuk.user` ile **403 Forbidden** döner.

### Manuel test (curl)

```bash
# Token al
TOKEN=$(curl -s -X POST https://localhost:5001/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"userNameOrEmail":"kucuk.user","password":"Password123!"}' \
  | jq -r '.accessToken')

# Members-only — 200 OK (User rolü yeterli)
curl -H "Authorization: Bearer $TOKEN" https://localhost:5001/api/examples/members-only

# Adults-only — 403 Forbidden (yaş < 18)
curl -H "Authorization: Bearer $TOKEN" https://localhost:5001/api/examples/adults-only
```

`enes.user` ile login yaparsan ikisi de çalışır.

---

## 8. Best Practices (bu projede uyguladıklarım)

1. **String literal yerine sabit kullan.** `[Authorize(Roles = "Admin")]` yerine `[Authorize(Policy = AuthPolicies.MinAdmin)]`.
2. **Her policy için tek sorumluluk.** `MinUser` rol kontrolü yapar, `Adult` yaş kontrolü ekler. Karmaşıklaşacaksa bölmek yerine yeni requirement yaz.
3. **Controller seviyesinde baseline, action seviyesinde sıkılaştırma.** Mesela `RolesController` için varsayılan `MinEditor`, ama `GetRoles` / `CreateRole` `MinAdmin`.
4. **Business kuralları handler'da, teknik kurallar policy'de.** "Body'deki rol ne?" gibi domain kuralları Application katmanına düşer; "bu kullanıcı oturum açmış mı + rolü var mı" policy'ye düşer.
5. **Privilege escalation'a karşı savun.** Editor Admin atayamasın + Editor Admin'i düşüremesin. İki yönlü kontrol.
6. **HTTP detaylarını Application'dan uzak tut.** `IHttpContextAccessor`'ı Infrastructure'da sarmala, Application sadece `ICurrentUserService` görsün.
7. **403 vs 401 farkını hatırla.** Token yoksa 401 Unauthorized, token var ama yetki yoksa 403 Forbidden.
8. **Yaş/tarih gibi gizlenebilir alanları token içine koymaktan kaçın** (gerçek sistemde). Bu projede demo amacıyla `DateOfBirth` claim'i eklendi — production'da birthDate'i DB'den çekip pipeline'da tutman daha iyi olabilir.

---

## 9. Unutmamanız Gerekenler / Gotcha'lar

- **`EnsureCreatedAsync` mevcut şemayı değiştirmez.** `AppUser`'a `BirthDate` kolonunu ekledim. Zaten DB oluşmuşsa sütun eklenmez. Çözüm:
  - Dev ortamında DB'yi sil (`DROP DATABASE BookNetworkDb; CREATE DATABASE BookNetworkDb;`) ve uygulamayı tekrar çalıştır.
  - Ya da (tercih edileni) EF migration'a geç: [Program.cs](../src/Presentation/BookNetwork.API/Program.cs) içinde `EnsureCreatedAsync` yerine `MigrateAsync` kullan ve `dotnet ef migrations add AddBirthDate` çalıştır.
- **`ClockSkew = TimeSpan.Zero`** (authentication'da) — token süresi dolunca hemen geçersiz olur, 5 dakika grace period yok. Unit testlerde sürpriz yapabilir.
- **Role / policy değişikliklerinden sonra eski token'lar eski claim'leri taşır.** Kullanıcının rolünü değiştirdiysen yeni login (veya refresh) gerekiyor, yoksa eski token'daki iddialar geçerli sanılır.
- **`AddAuthorization()` ile `AddAuthorizationPolicies()` aynı anda çağrılmasın.** `AddAuthorizationPolicies()` zaten `AddAuthorization`'ı çağırıyor — tek satır yeter.
- **Claim isimleri case-sensitive.** `.NET` `ClaimTypes.Role` aslında uzun bir URI. `RoleClaimType`'ı [AuthenticationExtensions.cs](../src/Presentation/BookNetwork.API/Extensions/AuthenticationExtensions.cs) içinde set ettiğimiz için `[Authorize(Roles=...)]` doğru çalışıyor.
- **`RequireRole` OR mantığında çalışır.** `p.RequireRole("A","B")` = A **veya** B. AND istersen `RequireRole("A").RequireRole("B")` şeklinde zincirle.
- **Policy'ler her request'te değerlendirilir.** Ağır bir iş koyarsan (DB çağrısı gibi) her istek yavaşlar; onun yerine claim olarak token'a işle.
- **`[Authorize]`'ı unutursan endpoint anonim erişime açık olur.** Default olarak her şeyi korumak istersen `FallbackPolicy` kullanabilirsin:
  ```csharp
  options.FallbackPolicy = new AuthorizationPolicyBuilder()
      .RequireAuthenticatedUser()
      .Build();
  ```
  Sonra `[AllowAnonymous]` ile muaf tutarsın.
- **`Admin` atama kuralı ek olarak `AssignClaim` endpoint'i için de geçerli değil.** Eğer claim üzerinden yetki veriyorsan oraya da benzer bir business kuralı eklemek iyi olur — şu an sadece `AssignRoleToUserCommandHandler` korunuyor.
- **Refresh token rotation.** `RefreshTokenLoginCommandHandler`'da kontrol et: refresh token her kullanımda yenileniyor mu? Aksi halde ele geçirilme riski artar.

---

## 10. İlerisi İçin Öneriler

- **Claim-based fine-grained yetki.** Bir kullanıcıya özel hak vermek için rol yerine claim kullan (örn. `books:delete`). Policy bu claim'i şart koşar. `AssignClaimToUserCommandHandler` zaten var — UI'dan yönetilebilir hale getirilebilir.
- **Endpoint Authorization (Endpoints tablosu).** Kod içinde `[Authorize]` yerine DB'den endpoint-role eşlemesi okuyan bir middleware. [Endpoint.cs](../src/Core/BookNetwork.Domain/Entities/Identity/Endpoint.cs) bu yöne hazırlanmış gibi görünüyor.
- **Audit logging.** Rol atamaları, yetki değişiklikleri loglansın ki retrospektif yapılabilsin.
- **Unit test.** `MinimumAgeHandler`'ı saf fonksiyon gibi test etmek kolay (fake `ClaimsPrincipal` üret, assert et). `AssignRoleToUserCommandHandler` business kuralları için de test yaz.
