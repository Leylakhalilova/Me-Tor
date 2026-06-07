# MiniNotepad (Terminal Text Editor / Terminal Metin Editörü)

[![.NET 10](https://img.shields.io/badge/.NET-10.0-blue.svg)](https://dotnet.microsoft.com/)
[![Platform](https://img.shields.io/badge/Platform-Windows%20%7C%20Linux%20%7C%20macOS-lightgrey.svg)]()
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

An interactive, terminal-based text editor built using C# and .NET 10. Designed with a custom interactive file explorer, multi-level undo/redo operations, custom search & replace algorithms, dynamic text selection, and a responsive status bar. 

C# ve .NET 10 ile geliştirilmiş, tamamen komut satırı (terminal/konsol) üzerinden çalışan etkileşimli bir metin editörüdür. Özel interaktif dosya gezgini, çok seviyeli geri alma/yineleme (undo), özelleştirilmiş arama ve değiştirme algoritmaları, dinamik metin seçimi ve duyarlı bir durum çubuğu ile donatılmıştır.

---

## 🌐 Language / Dil
- [English](#-english)
- [Türkçe](#-türkçe)

---

# 🇬🇧 English

## ✨ Features
- **Interactive File Explorer:** Browse folders, change drives (Windows), open and save files using dedicated keyboard shortcuts without leaving the terminal.
- **Dynamic Status Bar:** Real-time feedback displaying the current mode (EDIT/SELECT), active filename, modification status (`*`), cursor coordinates (Line/Column), and search match counter (e.g., `[Match: 2/5]`).
- **Bilingual Confirmation Dialogs:** Save prompt handles both English (`y`/`n`) and Turkish (`e`/`h`) input keys.
- **High-Performance Buffer:** Text is managed as a `List<StringBuilder>`, enabling $O(1)$ character insertion/deletion and efficient line splitting/merging.
- **Visual Selection Mode:** VIM-like selection using `CTRL + Arrows` (character-based) and `CTRL + SHIFT + Arrows` (word-based). Selected text is highlighted in vibrant yellow.
- **Search & Replace:** Support for partial matching (`CTRL + F`), whole-word matching with match navigation (`CTRL + G`), and global text replace (`CTRL + R`).
- **Smart Scrolling:** Seamless vertical and horizontal scrolling tracking the cursor positioning.
- **Automated Bundling Tool:** A built-in script compiles optimized self-contained single-file releases for both Windows and Linux, structures submission files, and generates concatenated source dumps.

---

## 📁 Repository Structure
```
me-tor/ (Root Directory)
├── src/
│   └── MiniNotepad/          # C# Source Code (.NET 10)
│       ├── Core/             # EditorBuffer, Cursor, UndoManager
│       ├── IO/               # Interactive File Explorer
│       ├── UI/               # Console Renderer, Shortcut Toolbox
│       └── Utils/            # Search & Text-matching Algorithms
├── docs/                     # Academic Reports & Documentations
│   ├── 25-26 bahar prolab proje 1.pdf # Course assignment guidelines PDF
│   ├── Rapor.tex             # IEEE Format Project Report (LaTeX)
│   ├── sunum.md              # Presentation Slides Outline
│   └── *.md                  # Code component details & walkthroughs
├── scripts/                  # Automated Development Scripts
│   └── bundle.sh             # Compiles & packages university submission folder
├── .gitignore                # Ignored compiler artifacts & local binaries
├── README.md                 # Bilingual documentation
└── sndnd.txt                 # Developer fix logs & bug notes
```

---

## 💻 Installation & Execution

### Prerequisites
You need the **.NET 10 SDK** installed on your system.
- [Download .NET 10](https://dotnet.microsoft.com/download)

---

### 🍎 macOS / 🐧 Linux
#### 1. Running in Development Mode
```bash
# Navigate to the project root and run:
dotnet run --project src/MiniNotepad
```
#### 2. Building/Publishing
```bash
# Build debug binary:
dotnet build src/MiniNotepad

# Publish self-contained executable for Linux:
dotnet publish src/MiniNotepad/MiniNotepad.csproj -c Release -r linux-x64 --self-contained true -p:PublishSingleFile=true -p:PublishReadyToRun=true -o ./publish

# Publish self-contained executable for macOS (Apple Silicon):
dotnet publish src/MiniNotepad/MiniNotepad.csproj -c Release -r osx-arm64 --self-contained true -p:PublishSingleFile=true -o ./publish
```

---

### 🪟 Windows
#### 1. Running in Development Mode
Open Command Prompt (cmd) or PowerShell in the root directory and run:
```powershell
dotnet run --project src/MiniNotepad
```
#### 2. Building/Publishing
```powershell
# Build debug binary:
dotnet build src/MiniNotepad

# Publish self-contained single-file executable for Windows x64:
dotnet publish src/MiniNotepad/MiniNotepad.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:PublishReadyToRun=true -o ./publish
```

---

## ⌨️ Shortcuts & Operations

| Shortcut | Description / Action |
|----------|----------------------|
| `CTRL + O` | Open File (launches Interactive File Explorer) |
| `CTRL + S` / `CTRL + T` | Save File (Saves changes to current file or opens Save As dialog) |
| `CTRL + A` | Save As (Opens File Explorer to select path/filename) |
| `CTRL + F` | Search Text (Partial match, highlights occurrences in blue) |
| `CTRL + G` | Match Finder (Whole word, navigates occurrences using [ARROW] keys) |
| `CTRL + R` | Replace Text (Search & replace all occurrences globally) |
| `CTRL + Z` | Undo (LIFO Stack based, supports undoing edits, splits, and selection cuts) |
| `CTRL + C` | Copy Selected Text |
| `CTRL + X` | Cut Selected Text |
| `CTRL + V` | Paste Text |
| `CTRL + Arrows` | Start/Modify Text Selection (Character-by-character) |
| `CTRL + SHIFT + Arrows` | Start/Modify Text Selection (Word-by-word) |
| `CTRL + Backspace` | Delete Word (Deletes preceding word) |
| `Backspace` | Delete Character (Deletes preceding character / merges lines) |
| `ESC` | Exit Program (Asks confirmation if buffer is modified) |

---

# 🇹🇷 Türkçe

## ✨ Özellikler
- **İnteraktif Dosya Gezgini:** Terminali kapatmadan klasörler arası geçiş yapın, sürücü değiştirin (Windows), dosyaları açın ve kısayollar yardımıyla kaydedin.
- **Dinamik Durum Çubuğu:** Aktif çalışma modunu (DÜZENLEME/SEÇİM), aktif dosya adını, kaydedilme durumunu (`*`), imlecin ekran koordinatlarını (Satır/Sütun) ve arama sonuç göstergesini (örn: `[Eşleşme: 2/5]`) anlık takip edin.
- **İki Dilli Onay Kutuları:** Kaydedilmemiş değişiklik uyarılarında hem Türkçe (`e`/`h`) hem de İngilizce (`y`/`n`) klavye girdilerini tanır.
- **Yüksek Performanslı Bellek Modeli:** Metin, bellekte `List<StringBuilder>` yapısında tutulur. Bu sayede karakter ekleme/silme ve satır birleştirme/bölme işlemleri amorti edilmiş $O(1)$ karmaşıklığında çalışır.
- **Görsel Seçim Modu:** `CTRL + Yön Tuşları` ile karakter bazlı, `CTRL + SHIFT + Yön Tuşları` ile kelime bazlı seçim yapılabilir. Seçili metin canlı sarı renk arka planla vurgulanır.
- **Arama & Değiştirme:** Kısmi eşleşmeli hızlı arama (`CTRL + F`), tam kelime eşleşmeli ve sonuçlar arası gezinmeli gelişmiş arama (`CTRL + G`) ve küresel yer değiştirme (`CTRL + R`) desteği.
- **Akıllı Ekran Kaydırma:** İmleç konumunu takip ederek ekranı hem dikey hem de yatay olarak akıllıca kaydırır.
- **Otomatik Paketleme Scripti:** Windows ve Linux için tek dosya halinde çalışabilen binary'lerini derler, öğrenci numaralı teslim klasör yapısını kurar ve kaynak kodlarını tek dosyada birleştirir.

---

## 📁 Proje Klasör Yapısı
```
me-tor/ (Kök Dizin)
├── src/
│   └── MiniNotepad/          # C# Kaynak Kodları (.NET 10)
│       ├── Core/             # EditorBuffer, Cursor, UndoManager
│       ├── IO/               # İnteraktif Dosya Gezgini
│       ├── UI/               # Terminal Arayüzü ve Kısayol Menüsü
│       └── Utils/            # Arama Algoritmaları
├── docs/                     # Akademik Raporlar ve Dökümanlar
│   ├── 25-26 bahar prolab proje 1.pdf # Proje isterleri dökümanı
│   ├── Rapor.tex             # IEEE Formatında Proje Raporu (LaTeX)
│   ├── sunum.md              # Sunum Taslağı ve İçerikleri
│   └── *.md                  # Kod mimarisi açıklama dökümanları
├── scripts/                  # Otomasyon Araçları
│   └── bundle.sh             # Derleme ve teslimat paketleme scripti
├── .gitignore                # Git dışı bırakılan derleme/çalıştırma çıktıları
├── README.md                 # Çift dilli kullanım kılavuzu
└── sndnd.txt                 # Geliştirici hata/düzeltme notları
```

---

## 💻 Kurulum ve Çalıştırma

### Gereksinimler
Sisteminizde **.NET 10 SDK** kurulu olmalıdır.
- [.NET 10 SDK İndir](https://dotnet.microsoft.com/download)

---

### 🍎 macOS / 🐧 Linux
#### 1. Geliştirme Modunda Çalıştırma
```bash
# Proje kök dizininde terminalden şu komutu çalıştırın:
dotnet run --project src/MiniNotepad
```
#### 2. Derleme / Paketleme
```bash
# Hata ayıklama sürümünü derle:
dotnet build src/MiniNotepad

# Linux için tek dosya çalıştırılabilir sürüm üret:
dotnet publish src/MiniNotepad/MiniNotepad.csproj -c Release -r linux-x64 --self-contained true -p:PublishSingleFile=true -p:PublishReadyToRun=true -o ./publish

# macOS (M1/M2/M3 Apple Silicon) için tek dosya sürüm üret:
dotnet publish src/MiniNotepad/MiniNotepad.csproj -c Release -r osx-arm64 --self-contained true -p:PublishSingleFile=true -o ./publish
```

---

### 🪟 Windows
#### 1. Geliştirme Modunda Çalıştırma
Proje kök dizininde Komut İstemi (cmd) veya PowerShell açıp şu komutu çalıştırın:
```powershell
dotnet run --project src/MiniNotepad
```
#### 2. Derleme / Paketleme
```powershell
# Hata ayıklama sürümünü derle:
dotnet build src/MiniNotepad

# Windows x64 için tek dosya halinde çalışan bağımsız exe üret:
dotnet publish src/MiniNotepad/MiniNotepad.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:PublishReadyToRun=true -o ./publish
```

---

## ⌨️ Kısayollar ve Kontroller

| Kısayol | İşlem Açıklaması |
|---------|------------------|
| `CTRL + O` | Dosya Aç (İnteraktif Dosya Gezgini) |
| `CTRL + S` / `CTRL + T` | Dosyayı Kaydet (Mevcut dosyaya yazar veya yeni dosya ise Farklı Kaydet açar) |
| `CTRL + A` | Farklı Kaydet (Dosya konumu ve adı seçtirir) |
| `CTRL + F` | Metin Bul (Kısmi arama yapar, eşleşmeleri mavi renkle vurgular) |
| `CTRL + G` | Eşleşme Bulucu (Tam eşleşmeli arama yapar, [OKLAR] ile sonuçlar arası gezdirir) |
| `CTRL + R` | Metni Değiştir (Tüm eşleşmeleri bulur ve yeni kelimeyle değiştirir) |
| `CTRL + Z` | Geri Al (Undo - Metin eklemeleri, silmeleri ve kesimleri geri alır) |
| `CTRL + C` | Seçili Metni Kopyala |
| `CTRL + X` | Seçili Metni Kes |
| `CTRL + V` | Metni Yapıştır |
| `CTRL + Yön Tuşları` | Karakter bazlı metin seçimi başlatır/genişletir (Sarı vurgu) |
| `CTRL + SHIFT + Yön Tuşları` | Kelime bazlı metin seçimi başlatır/genişletir |
| `CTRL + Backspace` | Kelime bazlı silme (İmlecin solundaki kelimeyi siler) |
| `Backspace` | Karakter silme (İmlecin solundaki karakteri siler / satır birleştirir) |
| `ESC` | Programdan Çıkış (Değişiklik varsa kaydetme onayı sorar) |

---

## 📦 Submission Packaging / Üniversite Teslimi Otomasyonu

University homework submissions typically require a specific folder format (`240201092-240201016`), pre-built self-contained binaries, and a merged code file (`Kodlar.txt`).
You can automatically generate all of these requirements by running:

Üniversite ödev teslimlerinde istenen öğrenci numaralı klasör yapısını (`240201092-240201016`), bağımsız çalışabilen Linux/Windows binary'lerini ve birleştirilmiş kaynak kod dosyalarını (`Kodlar.txt`) tek bir komutla oluşturabilirsiniz:

```bash
./scripts/bundle.sh
```
