# EditorBuffer.cs - Kod Analizi ve Çalışma Mantığı

`EditorBuffer.cs`, MiniNotepad uygulamasının **çekirdek (core)** sınıfıdır. Metin verisinin bellekte nasıl tutulduğunu, karakter ekleme/silme işlemlerini, satır yönetimini ve arama/değiştirme işlevlerini kontrol eder.

---

## 1. Temel Veri Yapıları ve Değişkenler

```csharp
public List<StringBuilder> Lines { get; private set; } = new() { new StringBuilder() };
public bool IsModified { get; set; } = false;
public string? CurrentFilePath { get; set; }
public UndoManager UndoManager { get; } = new();
```

*   **`Lines`**: Metni `List<StringBuilder>` yapısında tutar. 
    *   **Neden StringBuilder?** C#'ta `string` değişmez (immutable) bir yapıdır. Her karakter eklemede yeni bir string oluşturulur. `StringBuilder` ise mevcut bellek alanı üzerinde değişiklik yapmaya izin vererek performansı artırır.
    *   **Neden List?** Her satırı ayrı bir obje olarak tutmak, satır ekleme/silme ve satırlar arası geçiş işlemlerini kolaylaştırır.
*   **`UndoManager`**: Yapılan her işlemi (ekleme, silme, satır birleştirme vb.) bir yığın (stack) yapısında saklar. Bu sayede `Ctrl+Z` (Undo) işlevi gerçekleştirilir.
*   **`IsModified`**: Dosyada kaydedilmemiş değişiklik olup olmadığını takip eder.

---

## 2. Metin Yükleme (Load)

```csharp
public void Load(IEnumerable<string> content, string? path)
{
    Lines.Clear();
    foreach (var line in content)
    {
        Lines.Add(new StringBuilder(line));
    }
    if (Lines.Count == 0) Lines.Add(new StringBuilder());
    CurrentFilePath = path;
    IsModified = false;
    ClearSearch();
}
```

*   Dışarıdan (örneğin bir dosyadan) gelen satır dizisini okur.
*   Mevcut `Lines` listesini temizler ve her yeni satırı bir `StringBuilder` olarak listeye ekler.
*   Arama sonuçlarını temizler ve dosya yolunu günceller.

---

## 3. Karakter İşlemleri (Insert & Delete)

### Karakter Ekleme
```csharp
public void InsertChar(int x, int y, char c)
{
    Lines[y].Insert(x, c); // Belirtilen satır (y) ve sütuna (x) karakteri yerleştirir.
    UndoManager.PushAction(x, y, c.ToString(), ActionType.InsertChar);
    IsModified = true;
    ClearSearch();
}
```

### Karakter Silme (Backspace Mantığı)
```csharp
public void DeleteChar(int x, int y)
{
    if (x > 0) { // Satır içindeysek
        char c = Lines[y][x - 1];
        Lines[y].Remove(x - 1, 1); // İmlecin solundaki karakteri siler.
        UndoManager.PushAction(x - 1, y, c.ToString(), ActionType.DeleteChar);
    }
    else if (y > 0) { // Satır başındaysak
        // Üst satırla birleştirme işlemi yapılır.
        var currentLine = Lines[y].ToString();
        var prevLineLength = Lines[y - 1].Length;
        Lines[y - 1].Append(currentLine); // Mevcut satırı üst satırın sonuna ekler.
        Lines.RemoveAt(y); // Boş kalan mevcut satırı siler.
        UndoManager.PushAction(prevLineLength, y - 1, null, ActionType.MergeLines);
    }
}
```

---

## 4. Satır Yönetimi (NewLine)

```csharp
public void NewLine(int x, int y)
{
    var currentLine = Lines[y].ToString();
    var leftPart = currentLine.Substring(0, x); // İmlecin solundaki metin
    var rightPart = currentLine.Substring(x);    // İmlecin sağındaki metin

    Lines[y] = new StringBuilder(leftPart); // Mevcut satırı sol parça ile günceller.
    Lines.Insert(y + 1, new StringBuilder(rightPart)); // Sağ parçayı yeni bir satır olarak ekler.
    UndoManager.PushAction(x, y, null, ActionType.SplitLine);
}
```

*   Bu metod, kullanıcı **Enter** tuşuna bastığında çalışır. Bir satırı imlecin olduğu yerden ikiye böler.

---

## 5. Seçim ve Toplu Silme (Selection)

```csharp
public void DeleteSelection(EditorCursor cursor)
{
    // ... Başlangıç ve bitiş koordinatları hesaplanır ...
    if (startY == endY) {
        Lines[startY].Remove(startX, endX - startX); // Tek satırlık silme
    }
    else {
        // Çok satırlı silme: İlk satırın başı ile son satırın sonunu birleştirir,
        // aradaki tüm satırları siler.
        string firstLineStart = Lines[startY].ToString().Substring(0, startX);
        string lastLineEnd = Lines[endY].ToString().Substring(endX);
        Lines[startY] = new StringBuilder(firstLineStart + lastLineEnd);
        Lines.RemoveRange(startY + 1, endY - startY);
    }
}
```

---

## 6. Arama ve Değiştirme (Search & Replace)

```csharp
public void ReplaceAll(string oldText, string newText)
{
    // Regex (Düzenli İfadeler) kullanarak tam kelime eşleşmesi (\b) aranır.
    string pattern = @"\b" + System.Text.RegularExpressions.Regex.Escape(oldText) + @"\b";
    
    // Tüm Lines listesi taranır ve eşleşen tüm kelimeler yenisiyle değiştirilir.
    // Case-insensitive (Büyük/Küçük harf duyarsız) arama yapılır.
}
```

---

## 7. Geri Alma Mekanizması (Undo)

```csharp
public void Undo(EditorCursor cursor)
{
    var action = UndoManager.PopAction(); // Son yapılan işlemi geri çağırır.
    switch (action.Type)
    {
        case ActionType.InsertChar:
            Lines[action.Y].Remove(action.X, action.Text!.Length); // Eklenen silinir.
            break;
        case ActionType.DeleteChar:
            Lines[action.Y].Insert(action.X, action.Text!); // Silinen geri eklenir.
            break;
        case ActionType.SplitLine:
            // İkiye bölünen satırlar tekrar birleştirilir.
            break;
        // ... Diğer durumlar ...
    }
}
```

---

### Sunum İçin Önemli Notlar:
*   **StringBuilder kullanımı performansı korur.**
*   **Liste yapısı satır bazlı işlemleri (yukarı-aşağı hareket, satır silme) kolaylaştırır.**
*   **Geri alma işlemi için her atomik işlem (karakter yazma, silme) UndoManager'a kaydedilir.**
