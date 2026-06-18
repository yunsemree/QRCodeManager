# QR Manager - Release publish ve installer hazırlık scripti.
#
# Kullanım:
#   .\scripts\publish-release.ps1
#   .\scripts\publish-release.ps1 -BuildInstaller
#   .\scripts\publish-release.ps1 -SkipTests
#
# Çıktılar:
#   publish\QRCodeManager.exe          (self-contained win-x64)
#   publish\InitialDatabase.db         (ilk çalıştırma şablonu)
#   publish\database\database.db       (installer ilk kurulum kaynağı)
#   dist\QRManager-Setup-{version}.exe ( -BuildInstaller ile)

param(
    [switch]$SkipTests,
    [switch]$BuildInstaller,
    [ValidateSet("Release", "Debug")]
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $PSScriptRoot
$solution = Join-Path $root "QRCodeManager.sln"
$wpfProject = Join-Path $root "src\QRCodeManager.WPF\QRCodeManager.WPF.csproj"
$infraProject = Join-Path $root "src\QRCodeManager.Infrastructure\QRCodeManager.Infrastructure.csproj"
$assetsDir = Join-Path $root "src\QRCodeManager.WPF\Assets"
$iconPng = Join-Path $assetsDir "app-icon.png"
$iconIco = Join-Path $assetsDir "app-icon.ico"
$databaseSourceDir = Join-Path $root "src\QRCodeManager.WPF\Database"
$initialDbSource = Join-Path $databaseSourceDir "InitialDatabase.db"
$publishDir = Join-Path $root "publish"
$publishDatabaseDir = Join-Path $publishDir "database"
$publishDatabaseFile = Join-Path $publishDatabaseDir "database.db"
$distDir = Join-Path $root "dist"
$installerScript = Join-Path $root "Installer\ApplicationInstaller.iss"
$iconGeneratorProject = Join-Path $root "tools\IconGenerator\IconGenerator.csproj"

function Write-Step([string]$Message) {
    Write-Host ">> $Message" -ForegroundColor Cyan
}

function Assert-FileExists([string]$Path, [string]$Description) {
    if (-not (Test-Path $Path)) {
        throw "$Description bulunamadi: $Path"
    }
}

function Invoke-DotNet([string[]]$Arguments) {
    & dotnet @Arguments
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet $($Arguments -join ' ') basarisiz (exit code: $LASTEXITCODE)."
    }
}

function Get-ProjectProperty([string]$ProjectPath, [string]$PropertyName) {
    [xml]$projectXml = Get-Content $ProjectPath
    $values = @($projectXml.Project.PropertyGroup.$PropertyName | Where-Object { $_ })
    if ($values.Count -eq 0) {
        throw "csproj icinde '$PropertyName' bulunamadi: $ProjectPath"
    }
    return [string]$values[0]
}

function Ensure-AppIcon {
    Assert-FileExists $iconPng "Uygulama ikonu (PNG)"

    Write-Step "Gecerli app-icon.ico olusturuluyor (16/32/48/256)..."
    if (Test-Path $iconIco) {
        Remove-Item $iconIco -Force
    }

    Invoke-DotNet @(
        "run",
        "--project", $iconGeneratorProject,
        "--",
        $iconPng,
        $iconIco
    )

    Assert-FileExists $iconIco "Uygulama ikonu (ICO)"
}

function Ensure-InitialDatabase {
    Write-Step "Baslangic veritabani olusturuluyor (EF migration)..."
    New-Item -ItemType Directory -Force -Path $databaseSourceDir | Out-Null

    if (Test-Path $initialDbSource) {
        Remove-Item $initialDbSource -Force
    }

    $initialDbFullPath = [System.IO.Path]::GetFullPath($initialDbSource)

    Invoke-DotNet @("build", $infraProject, "-c", $Configuration)
    Invoke-DotNet @(
        "ef", "database", "update",
        "--project", $infraProject,
        "--connection", "Data Source=$initialDbFullPath",
        "--no-build"
    )

    Assert-FileExists $initialDbFullPath "InitialDatabase.db"
}

function Publish-Application {
    Write-Step "Publish baslatiliyor ($Configuration, self-contained win-x64)..."

    if (Test-Path $publishDir) {
        Remove-Item $publishDir -Recurse -Force
    }

    Invoke-DotNet @(
        "publish", $wpfProject,
        "-c", $Configuration,
        "-r", "win-x64",
        "--self-contained", "true",
        "-p:PublishSingleFile=false",
        "-p:PublishReadyToRun=true",
        "-o", $publishDir
    )
}

function Ensure-PublishDatabase {
    Write-Step "Publish veritabani yapisi dogrulaniyor..."

    if (-not (Test-Path $publishDatabaseFile)) {
        New-Item -ItemType Directory -Force -Path $publishDatabaseDir | Out-Null
        Copy-Item $initialDbSource $publishDatabaseFile -Force
    }

    Assert-FileExists $publishDatabaseFile "publish\database\database.db"
}

function Test-PublishOutput([string]$ExeName, [string]$Version, [string]$Publisher) {
    $exePath = Join-Path $publishDir $ExeName

    Assert-FileExists $exePath $ExeName
    Assert-FileExists (Join-Path $publishDir "InitialDatabase.db") "InitialDatabase.db"
    Assert-FileExists $publishDatabaseFile "publish\database\database.db"

    $exeInfo = Get-Item $exePath
    $totalSizeMb = [math]::Round(((Get-ChildItem $publishDir -Recurse -File | Measure-Object Length -Sum).Sum / 1MB), 1)

    Write-Host ""
    Write-Host "Publish tamamlandi." -ForegroundColor Green
    Write-Host "  Urun     : QR Manager v$Version"
    Write-Host "  Yayimci  : $Publisher"
    Write-Host "  Exe      : $exePath ($([math]::Round($exeInfo.Length / 1MB, 1)) MB)"
    Write-Host "  Toplam   : $totalSizeMb MB"
    Write-Host "  DB sablon: $publishDir\InitialDatabase.db"
    Write-Host "  Installer: $publishDatabaseFile"
    Write-Host ""
}

function Build-Installer([string]$Version) {
    $iscc = @(
        "${env:ProgramFiles(x86)}\Inno Setup 6\ISCC.exe",
        "$env:ProgramFiles\Inno Setup 6\ISCC.exe"
    ) | Where-Object { Test-Path $_ } | Select-Object -First 1

    if (-not $iscc) {
        throw "Inno Setup bulunamadi. Kurulum: https://jrsoftware.org/isdl.php"
    }

    Write-Step "Installer derleniyor (v$Version)..."
    & $iscc "/DMyAppVersion=$Version" $installerScript
    if ($LASTEXITCODE -ne 0) {
        throw "Installer derlemesi basarisiz."
    }

    $setupFile = Join-Path $distDir "QRManager-Setup-$Version.exe"
    Assert-FileExists $setupFile "Installer ciktisi"

    $setupSizeMb = [math]::Round((Get-Item $setupFile).Length / 1MB, 1)
    Write-Host "Installer olusturuldu: $setupFile ($setupSizeMb MB)" -ForegroundColor Green
}

# --- Ana akis ---

$exeName = "$(Get-ProjectProperty $wpfProject 'AssemblyName').exe"
$version = Get-ProjectProperty $wpfProject "Version"
$publisher = Get-ProjectProperty $wpfProject "Company"
$product = Get-ProjectProperty $wpfProject "Product"

Write-Host ""
Write-Host "QR Manager Publish" -ForegroundColor White
Write-Host "  Surum    : $version"
Write-Host "  Yayimci  : $publisher"
Write-Host "  Mod      : $Configuration"
Write-Host ""

Ensure-AppIcon
Ensure-InitialDatabase

if (-not $SkipTests) {
    Write-Step "Testler calistiriliyor..."
    Invoke-DotNet @("test", $solution, "-c", $Configuration)
}

Publish-Application
Ensure-PublishDatabase
Test-PublishOutput -ExeName $exeName -Version $version -Publisher $publisher

if ($BuildInstaller) {
    Build-Installer -Version $version
}
else {
    Write-Host "Installer icin: .\scripts\publish-release.ps1 -BuildInstaller" -ForegroundColor Yellow
}
