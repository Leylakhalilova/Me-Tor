# MiniNotepad: Kod ve Yapı Analizi (Line-by-Line)

Bu dosya, MiniNotepad projesinin mimarisini ve kodların satır satır ne işe yaradığını açıklar.

## 1. Proje Klasör Yapısı
- **Core/**: Uygulamanın beyni. Metin verisi, imleç ve geri alma mantığı burada.
- **IO/**: Dosya sistemi işlemleri ve terminal içi dosya gezgini.
- **UI/**: Ekranın çizilmesi ve kullanıcıya gösterilecek arayüz.
- **Utils/**: Arama ve yardımcı algoritmalar.
- **Program.cs**: Uygulamanın giriş noktası ve tuş kontrol merkezi.

---

## 2. Program.cs (Giriş ve Kontrol)

Bu dosya, Windows terminalinin davranışını değiştirerek başlar ve bir döngü içinde tuşları dinler.

```csharp
[DllImport("kernel32.dll", SetLastError = true)]
static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);
```
- **Neden?**: Windows terminali normalde `CTRL+S` gibi tuşları kendi işlevleri için kullanır. Bu Win32 API çağrısı, terminalin bu tuşları "yutmasını" engeller ve kontrolü tamamen C# kodumuza verir.

```csharp
while (running) {
    var keyInfo = Console.ReadKey(true);
```
- **Neden?**: `true` parametresi, basılan tuşun ekrana hemen yazılmasını engeller. Önce biz kontrol ederiz (Harf mi? Kısayol mu?), sonra biz yazdırırız.

---

## 3. Core/EditorBuffer.cs (Metin Yönetimi)

Metnin hafızada nasıl tutulduğu buradadır.

```csharp
public List<StringBuilder> Lines { get; private set; } = new() { new StringBuilder() };
```
- **Neden?**: Metni tek bir devasa `string` olarak tutmak yerine, satır satır `StringBuilder` listesi olarak tutuyoruz.
- **Avantajı**: 1000 satırlık bir dosyada 500. satıra bir harf eklediğinizde, tüm dosyayı değil sadece o satırı güncelleriz. Bu inanılmaz bir performans sağlar.

```csharp
public void NewLine(int x, int y) {
    var currentLine = Lines[y].ToString();
    var leftPart = currentLine.Substring(0, x);
    var rightPart = currentLine.Substring(x);
    Lines[y] = new StringBuilder(leftPart);
    Lines.Insert(y + 1, new StringBuilder(rightPart));
}
```
- **Mantık**: Enter'a basıldığında, imlecin solundaki metin mevcut satırda kalır, sağındaki metin ise koparılıp bir alt satıra (y+1) yeni bir satır olarak eklenir.

---

## 4. Core/EditorCursor.cs (İmleç Hareketleri)

İmlecin ekran üzerinde değil, metin üzerindeki "mantıksal" konumunu yönetir.

```csharp
public void Move(int dx, int dy, List<StringBuilder> lines) {
    if (X < 0) { // Satır başına gelip sola basılırsa
        if (Y > 0) { Y--; X = lines[Y].Length; } // Bir üst satırın sonuna git
    }
}
```
- **Mantık**: İmleç sadece sağa-sola gitmez; satır başındayken sola basıldığında otomatik olarak üst satıra çıkma gibi akıllı navigasyon özelliklerini burada simüle ediyoruz.

---

## 5. IO/FileExplorer.cs (Dosya Gezgini)

Terminal içinde çalışan bir "Mini Windows Explorer" gibidir.

```csharp
public static string? Explore(string title, ...) {
    // ...
    var dirInfo = new DirectoryInfo(currentDir);
    entries.AddRange(dirInfo.GetDirectories());
    entries.AddRange(dirInfo.GetFiles());
}
```
- **Neden?**: İşletim sisteminin dosya seçme penceresini terminalde açamayacağımız için, `System.IO` kütüphanesini kullanarak klasörleri listeliyoruz ve kullanıcıya seçtiriyoruz.

---

## 6. UI/Renderer.cs (Ekran Çizimi)

En kritik kısımlardan biridir. Ekranı temizleyip her şeyi baştan yazar.

```csharp
private void UpdateScroll(EditorCursor cursor) {
    if (cursor.Y >= ScrollY + viewHeight) ScrollY = cursor.Y - viewHeight + 1;
}
```
- **Neden?**: Eğer dosya 100 satır ama terminal 30 satırsa, ekranın aşağı doğru "kayması" (scrolling) gerekir. `ScrollY` değişkeni, ekranın o an dosyanın hangi satırından itibaren çizilmeye başlanacağını belirler.

```csharp
Console.SetCursorPosition(0, ToolboxHeight + i);
Console.Write($"{lineIdx + 1,3} | "); // Satır numarası
```
- **Mantık**: Her satırın başına 3 karakter genişliğinde sağa yaslı satır numarası ekler.

---

## 7. Core/UndoManager.cs (Geri Alma)

```csharp
private readonly Stack<EditorAction> _undoStack = new();
```
- **Neden?**: Geri alma işlemi "LIFO" (Son giren ilk çıkar) kuralına dayanır. Bu yüzden `Stack` veri yapısı en uygunudur. Yapılan her harf değişikliği bu yığına atılır ve `CTRL+Z` ile geri çekilir.
