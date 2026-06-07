# MiniNotepad (Terminal Metin Editörü)

Bu proje, Kocaeli Üniversitesi Bilgisayar Mühendisliği Programlama Laboratuvarı-II kapsamında geliştirilmiş, C# ve .NET 10 tabanlı, tamamen komut satırı (terminal/konsol) üzerinden çalışan interaktif bir **Metin Editörü** uygulamasıdır. Klasik Windows Not Defteri'nin ve Notepad++'ın terminal versiyonu olarak tasarlanmıştır.

## 🚀 Yeni Eklenen Özellikler & İyileştirmeler
- **Profesyonel Klasör Yapısı:** Proje dosyaları, dokümanlar ve scriptler standart ve derli toplu bir düzen (`src/`, `docs/`, `scripts/`) altına alınarak GitHub için optimize edildi.
- **Dinamik Durum Çubuğu (Status Bar):** Ekranın en altında yer alan durum çubuğu ile aktif işlem modu (DÜZENLEME/SEÇİM), dosya adı, düzenleme durumu (`*` işareti ile değişiklik göstergesi), imleç koordinatları (Satır/Sütun) ve arama sonuç sayıları (`[Eşleşme: X/Y]`) anlık olarak gösterilmektedir.
- **Gelişmiş Arama Gezinmesi (CTRL+G):** Arama sonuçları arasında gezinirken hangi eşleşmede olduğunuzu durum çubuğu üzerinden kolayca takip edebilirsiniz.
- **Gelişmiş Yatay Kaydırma (Horizontal Scrolling):** Yazım esnasında satırların yapay olarak bölünmesi engellendi ve akıllı yatay kaydırma özelliğinin doğal çalışması sağlandı.
- **y/n ve e/h Desteği:** Kaydedilmemiş dosyalardan çıkış yaparken veya yeni dosya açarken gelen onay kutularında hem İngilizce (`y`/`n`) hem de Türkçe (`e`/`h`) karakterler desteklenmektedir.
- **Otomasyon Scripti:** Üniversite teslim formatı olan öğrenci numaralı klasör yapısını kuran, Windows/Linux binary'lerini derleyen ve `Kodlar.txt` dosyasını otomatik birleştiren paketleme aracı eklendi.

## 📁 Proje Klasör Yapısı

```
me-tor/ (Proje Kök Dizini)
├── src/
│   └── MiniNotepad/          # C# Kaynak Kodları (.NET 10)
│       ├── Core/             # EditorBuffer, Cursor, UndoManager
│       ├── IO/               # İnteraktif FileExplorer (Dosya Yöneticisi)
│       ├── UI/               # Renderer, Toolbox (Görsel Katman)
│       └── Utils/            # Arama ve Metin Arama Algoritmaları
├── docs/                     # Akademik Rapor ve Dokümantasyonlar
│   ├── 25-26 bahar prolab proje 1.pdf # Proje isterleri dökümanı
│   ├── Rapor.tex             # IEEE Formatında Hazırlanmış Proje Raporu (LaTeX)
│   ├── sunum.md              # Sunum Slayt İçerikleri
│   └── *.md                  # Kod mimarisi ve bileşen detayları
├── scripts/                  # Derleme ve Otomasyon Araçları
│   └── bundle.sh             # Üniversite teslim paketleme aracı
├── .gitignore                # Git dışı bırakılacak dosyalar (bin, obj, binary çıktılar)
├── README.md                 # Proje tanıtım ve kullanım kılavuzu
└── sndnd.txt                 # Geliştirici hata ve düzeltme notları
```

## 🛠️ Kurulum ve Çalıştırma

Projeyi yerel makinenizde çalıştırmak için .NET 10 SDK yüklü olmalıdır.

### Geliştirme Modunda Çalıştırma:
```bash
cd src/MiniNotepad
dotnet run
```

### Derleme:
```bash
cd src/MiniNotepad
dotnet build
```

## 📦 Otomatik Paketleme ve Teslimat Hazırlığı (University Bundling)

Üniversite teslimlerinde istenen öğrenci numaralı klasör yapısını (`240201092-240201016`), bağımsız çalışabilen (self-contained) Linux/Windows binary'lerini ve tüm kaynak kodların tek bir dosyada birleştiği `Kodlar.txt` dosyasını otomatik oluşturmak için kök dizinde şu komutu çalıştırmanız yeterlidir:

```bash
./scripts/bundle.sh
```

Bu script sırasıyla şu adımları tamamlar:
1. `240201092-240201016` klasörünü sıfırdan oluşturur.
2. Tüm kaynak kod dosyalarını ilgili klasör yapısına kopyalar.
3. Linux ve Windows için tek dosya halinde çalışabilen (self-contained SingleFile) `main` ve `main.exe` binary'lerini derler ve kopyalar.
4. Binary çıktılarını `Release/` dizinine de kopyalar.
5. Tüm `.cs` dosyalarını akademisyenlerin inceleyebileceği sırada birleştirip `Kodlar.txt` dosyasını üretir.

## ⌨️ Kısayollar ve Kontroller

| Kısayol | İşlem |
|---------|-------|
| `CTRL + O` | Dosya Aç (İnteraktif Dosya Yöneticisi) |
| `CTRL + S` / `CTRL + T` | Kaydet (Mevcut dosyaya yazar veya yeni dosya ise Farklı Kaydet açar) |
| `CTRL + A` | Farklı Kaydet |
| `CTRL + F` | Metin Bul (Kısmi Arama) |
| `CTRL + G` | Eşleşme Bulucu (Tam Eşleşme) ve [OKLAR] ile eşleşmeler arası geçiş |
| `CTRL + R` | Metni Değiştir (Tüm Eşleşmeleri Bul ve Değiştir) |
| `CTRL + Z` | Geri Al (Undo - Sınırsız Stack Tabanlı) |
| `CTRL + C` | Seçili Metni Kopyala |
| `CTRL + X` | Seçili Metni Kes |
| `CTRL + V` | Metin Yapıştır |
| `CTRL + Yön Tuşları` | Karakter bazlı metin seçimi (Sarı renkli vurgu) |
| `CTRL + SHIFT + Yön Tuşları` | Kelime bazlı metin seçimi |
| `CTRL + Backspace` | Kelime bazlı silme |
| `Backspace` | Karakter silme |
| `ESC` | Programdan Çıkış (Kaydedilmemiş değişiklik varsa onay ister) |
