# Program.cs: Teknik Derin Bakış ve Kod Analiz Rehberi

Bu döküman, MiniNotepad projesinin `Program.cs` dosyasındaki kodların satır satır mantığını, teknik arka planını ve kod örneklerini içermektedir.

---

## 1. Windows Çekirdek (Kernel) Ayarları ve "Magic Numbers"
*(Bu bölüm projenin Windows terminalini "profesyonel mod"a geçirmesini sağlar. Önceki kısımlarda detaylandırılmıştır.)*

---

## 2. Nesne Tabanlı Yapı (OOP) Hazırlığı
*(Buffer, Cursor ve Renderer nesnelerinin oluşturulduğu kısımdır.)*

---

## 3. ANA UYGULAMA DÖNGÜSÜ (WHILE LOOP) - DERİNLEMESİNE ANALİZ

Uygulamanın tüm mantığı bu `while` döngüsü içinde döner. Her bir tuş basışında bu döngü bir tur atar.

### 3.1. Tuş Okuma ve Modifikatörler
```csharp
while (running) // running değişkeni 'true' olduğu sürece döner
{
    var keyInfo = Console.ReadKey(true); // 1. SATIR: Tuşu yakala
    bool isCtrl = (keyInfo.Modifiers & ConsoleModifiers.Control) != 0; // 2. SATIR: Ctrl kontrolü
    bool isShift = (keyInfo.Modifiers & ConsoleModifiers.Shift) != 0;  // 3. SATIR: Shift kontrolü
```
*   **1. SATIR**: `Console.ReadKey(true)` programı burada durdurur ve kullanıcıdan bir tuş bekler. `true` parametresi, terminalin o harfi ekrana kendisinin yazmasını engeller; çünkü ekrana yazma işini biz `Renderer` ile yapacağız.
*   **2. ve 3. SATIR**: Basılan tuşla beraber `Ctrl` veya `Shift` tuşuna da basılıp basılmadığını "Bitwise" işlemiyle kontrol eder.

### 3.2. Kısayol (Shortcut) Tanımlamaları
```csharp
bool isCtrlS = (keyInfo.Key == ConsoleKey.S && isCtrl) || (keyInfo.KeyChar == (char)19);
```
*   Burada sistem hem `ConsoleKey.S` tuşunu hem de bu tuşun ASCII karşılığını (`char 19`) kontrol eder. Bu sayede program hem Windows hem de Linux/macOS terminallerinde aynı stabilitede çalışır.

### 3.3. Kısayol İşlemleri (Büyük IF Bloğu)
Eğer basılan tuş bir kısayolsa (Örn: `isCtrlS`), program metin düzenleme modundan çıkıp "Komut" moduna geçer:
```csharp
if (isCtrlS || isCtrlO || ...) // Kısayol basıldı mı?
{
    if (isCtrlS) 
    {
        // Dosya kaydetme fonksiyonlarını çağırır
        FileExplorer.Save(buffer.CurrentFilePath, buffer.GetLines());
    }
    else if (isCtrlZ) 
    {
        buffer.Undo(cursor); // Yapılan son işlemi geri alır
    }
    // ... (Kopyala, Yapıştır, Arama vb.)
}
```

### 3.4. Navigasyon ve Yazma (Switch-Case Bloğu)
Kısayol basılmadıysa, kullanıcının ya imleci hareket ettirdiği ya da yazı yazdığı kabul edilir:

#### A) Ok Tuşları ve Seçim
```csharp
case ConsoleKey.UpArrow:
    if (isCtrl) cursor.StartSelection(); // Ctrl basılıysa seçimi başlat
    cursor.Move(0, -1, buffer.Lines);    // İmleci bir satır yukarı taşı
    break;
```
*   Kullanıcı sadece Yukarı Ok basarsa imleç gezer. `Ctrl + Yukarı Ok` yaparsa imlecin geçtiği yerler "Seçili" (mavi) olarak işaretlenir.

#### B) Enter Tuşu (Yeni Satır)
```csharp
case ConsoleKey.Enter:
    buffer.ClearSearch(); // Arama yapılıyorsa temizle
    if (cursor.IsSelecting) buffer.DeleteSelection(cursor); // Seçili metin varsa sil
    buffer.NewLine(cursor.X, cursor.Y); // Mevcut satırı ikiye böl
    cursor.X = 0; // İmleci yeni satırın başına al
    cursor.Y++;   // Bir alt satıra geç
    break;
```

#### C) Backspace (Silme)
```csharp
case ConsoleKey.Backspace:
    if (cursor.X > 0 || cursor.Y > 0) // Eğer silinecek bir şey varsa
    {
        // ... (Koordinat hesaplama mantığı) ...
        buffer.DeleteChar(oldX, oldY); // Karakteri hafızadan (buffer) sil
    }
    break;
```

#### D) Normal Karakter Girişi (Default Case)
```csharp
default:
    if (!char.IsControl(keyInfo.KeyChar)) // Harf, Sayı veya Sembol mü?
    {
        // OTOMATİK SATIR KAYDIRMA (Word Wrap)
        if (cursor.X >= Console.WindowWidth - 7) // Satır sonuna gelindiyse
        {
            buffer.NewLine(cursor.X, cursor.Y); // Alt satıra geç
            cursor.X = 0; cursor.Y++;
        }
        buffer.InsertChar(cursor.X, cursor.Y, keyInfo.KeyChar); // Karakteri ekle
        cursor.X++; // İmleci sağa kaydır
    }
    break;
```

### 3.5. Döngünün Sonu ve Görsel Güncelleme
```csharp
    renderer.Render(buffer, cursor); // 1. SATIR: Her şeyi yeniden çiz!
} // 2. SATIR: Başa dön ve yeni bir tuş bekle.
```
*   **1. SATIR**: Döngünün en kritik yeridir. `buffer`'da veya `cursor`'da yapılan tüm değişikliklerin kullanıcı tarafından görülmesini sağlar. Ekranı milisaniyeler içinde temizler ve yeni metni/imleç konumunu çizer.

---

## 4. Genel Özet
`Program.cs` içindeki bu döngü, kullanıcı her tuşa bastığında şu sırayı izler:
1.  Tuşu Duy (Listen)
2.  Tuşu Anla (Analyze)
3.  Hafızayı Güncelle (Update Buffer)
4.  Görüntüyü Yenile (Render)
