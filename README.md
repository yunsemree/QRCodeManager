# QR Manager

Windows masaüstü uygulaması. Eser (ürün/envanter) bilgilerini QR koda dönüştürür, QR okur ve geçmiş kayıtlarını yönetir.

**Yayımcı:** Yunus Emre Teke  
**Sürüm:** 1.0.0  
**Platform:** Windows x64

---

## Özellikler

- **QR Oluştur** — Ürün, materyal, sahip, konum ve seri no alanlarından QR üretimi
- **QR Oku** — Görüntü dosyasından QR çözümleme ve yapılandırılmış metin çıktısı
- **Geçmiş** — Oluşturulan/okunan kayıtların listelenmesi
- **Ayarlar** — Açık/koyu tema (anında uygulanır), hata düzeltme seviyesi, dışa aktarma formatı
- **Modern arayüz** — Material Design, özel başlık çubuğu, sidebar navigasyon
- **Kurulum paketi** — Inno Setup ile self-contained dağıtım (.NET Runtime gerekmez)

---

## QR İçerik Formatı

QR kodu düz metin olarak saklanır:

```
📦 Eser Bilgisi
Ürün      : Laptop
Materyal  : Alüminyum
Sahibi    : Yunus Emre Teke
Konumu    : Sivas Halk Eğitim Merkezi
Seri No   : ABC123456
```

---

## Teknoloji Yığını

| Katman | Teknoloji |
|--------|-----------|
| UI | WPF (.NET 8), MVVM (CommunityToolkit.Mvvm), Material Design |
| Uygulama | Clean Architecture, servis/repository desenleri |
| Veritabanı | EF Core 8 + SQLite |
| QR | QRCoder (üretim), ZXing.Net (okuma) |
| Kurulum | Inno Setup 6 |

---

## Proje Yapısı

```
qr-wpf/
├── src/
│   ├── QRCodeManager.Domain/          # Domain modelleri ve enumlar
│   ├── QRCodeManager.Application/     # Arayüzler, DTO'lar, sabitler
│   ├── QRCodeManager.Infrastructure/  # EF Core, QR servisleri, repository
│   ├── QRCodeManager.WPF/            # WPF arayüzü (Views, ViewModels)
│   └── QRCodeManager.Tests/          # Birim testleri
├── scripts/
│   └── publish-release.ps1           # Release publish + installer scripti
├── tools/
│   └── IconGenerator/                # PNG → ICO dönüştürücü
├── Installer/
│   └── ApplicationInstaller.iss        # Inno Setup tanımı
├── publish/                          # Publish çıktısı (üretilir)
└── dist/                             # Setup dosyası (üretilir)
```

---

## Gereksinimler

### Geliştirme

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Windows 10/11
- Visual Studio 2022 veya VS Code (önerilir)

### Release / Installer

- [Inno Setup 6](https://jrsoftware.org/isdl.php) (`-BuildInstaller` için)
- EF Core CLI (publish script otomatik kullanır):

```powershell
dotnet tool install --global dotnet-ef
```

---

## Hızlı Başlangıç

```powershell
# Depoyu klonladıktan sonra proje kökünde:
dotnet restore QRCodeManager.sln
dotnet build QRCodeManager.sln
dotnet test QRCodeManager.sln
```

Visual Studio ile çalıştırmak için startup projesi: `QRCodeManager.WPF`

---

## Release Publish ve Kurulum

```powershell
# Test + publish
.\scripts\publish-release.ps1

# Testleri atla
.\scripts\publish-release.ps1 -SkipTests

# Publish + installer birlikte
.\scripts\publish-release.ps1 -BuildInstaller
```

### Çıktılar

| Dosya | Açıklama |
|-------|----------|
| `publish\QRCodeManager.exe` | Self-contained uygulama (win-x64) |
| `publish\InitialDatabase.db` | İlk çalıştırma veritabanı şablonu |
| `dist\QRManager-Setup-1.0.0.exe` | Windows kurulum dosyası |

Kurulum dizini: `C:\Program Files\QR Manager\`

---

## Kullanıcı Verileri

Yazılabilir veriler kurulum dizininde değil, kullanıcı profilinde tutulur:

```
%LOCALAPPDATA%\QR Manager\
├── database\database.db    # SQLite veritabanı
├── settings.json           # Uygulama ayarları
├── logs\application.log    # Uygulama logları
└── qr-images\              # Kaydedilen QR görselleri
```

İlk açılışta veritabanı, kurulum dizinindeki `InitialDatabase.db` dosyasından kopyalanır.

---

## Mimari

Clean Architecture katman sınırları:

```
WPF (Presentation)
    ↓
Application (Use cases, interfaces)
    ↓
Domain (Entities, business rules)
    ↑
Infrastructure (EF Core, QR, dosya sistemi)
```

- **Domain** — dış bağımlılık yok
- **Application** — yalnızca Domain'e bağımlı
- **Infrastructure** — Application arayüzlerini uygular
- **WPF** — DI ile servisleri kullanır, ince ViewModel katmanı

---

## Testler

```powershell
dotnet test QRCodeManager.sln
```

Test kapsamı: JSON servisi, QR üretim/okuma, repository, veri yolu sağlayıcı, form servisi.

---

## Sorun Giderme

| Sorun | Çözüm |
|-------|-------|
| Uygulama kurulum sonrası açılmıyor | `%LOCALAPPDATA%\QR Manager\logs\startup-crash.log` dosyasını kontrol edin |
| Installer derlenmiyor | Inno Setup 6 kurulu olduğundan emin olun |
| `InitialDatabase.db` bulunamadı | Önce `.\scripts\publish-release.ps1` çalıştırın |
| ICO hatası (Inno Setup) | Publish script geçerli ICO'yu otomatik üretir |

---

## Lisans

Bu proje Yunus Emre Teke tarafından geliştirilmiştir.
