# MiniNotepad (Terminal Metin Editörü)

Bu proje, C# ile geliştirilmiş, tamamen komut satırı (terminal/konsol) üzerinden çalışan bir **Metin Editörü** uygulamasıdır. Klasik Windows Not Defteri'nin terminal versiyonu gibi çalışır.

## Özellikler
- **Temel Metin Düzenleme:** Yazma, silme, alt satıra geçme.
- **Dosya Gezgini:** Terminal içinde interaktif dosya açma ve kaydetme (CTRL+O, CTRL+S, CTRL+A).
- **Geri Alma (Undo):** Yapılan işlemleri geri alma (CTRL+Z).
- **Arama:** Metin içinde kelime bulma (CTRL+F).
- **Akıllı Kaydırma (Scrolling):** Büyük dosyalarda otomatik ekran kaydırma.
- **Güvenli Çıkış:** Değişiklikleri kaydetmeden çıkarken uyarı.

## Teknik Mimari

Proje 4 ana parçadan oluşur:

### 1. Program.cs (Kontrolcü)
Uygulamanın giriş noktasıdır. Win32 API kullanarak terminal ayarlarını (QuickEdit OFF, CTRL+S yakalama vb.) düzenler ve ana klavye dinleme döngüsünü yönetir.

### 2. Core (Çekirdek)
- **EditorBuffer:** Metni `List<StringBuilder>` olarak bellekte tutar. Bu yapı, satır ekleme ve silme işlemlerini çok hızlı yapar.
- **EditorCursor:** İmlecin koordinatlarını (X, Y) ve sınır kontrollerini yönetir.
- **UndoManager:** Yapılan işlemleri bir `Stack` veri yapısında tutarak geri alma işlevini sağlar.

### 3. UI (Arayüz - Renderer)
- Metni ve menüleri terminal ekranına çizer.
- Sadece görünür olan satırları ve sütunları işler (Rendering).
- Satır numaralarını gösterir.

### 4. IO (Giriş/Çıkış - FileExplorer)
- Terminal üzerinde çalışan interaktif bir dosya gezginidir.
- Klasörler arasında gezinme, sürücü seçimi ve dosya adı geçerliliği kontrolünü yapar.

## Kısayollar
| Kısayol | İşlem |
|---------|-------|
| `CTRL + O` | Dosya Aç |
| `CTRL + S` | Kaydet |
| `CTRL + A` | Farklı Kaydet |
| `CTRL + Z` | Geri Al |
| `CTRL + F` | Metin Bul |
| `ESC` | Çıkış |
| `OKLAR` | İmleç Hareketi |
| `ENTER` | Yeni Satır |
| `BACKSPACE` | Sil |

## Kurulum
Projeyi derlemek ve çalıştırmak için:
```bash
dotnet build
dotnet run
```
