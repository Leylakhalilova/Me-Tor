# EditorCursor.cs: Satır Satır Teknik Analiz

Bu sınıf, MiniNotepad projesinde imlecin (cursor) koordinatlarını, metin seçim (selection) mantığını ve akıllı navigasyon özelliklerini yöneten "beyin" kısmıdır.

---

## 1. Sınıf Tanımı ve Değişkenler
```csharp
public class EditorCursor
{
    public int X { get; set; } = 0; // İmlecin yatay konumu (Sütun)
    public int Y { get; set; } = 0; // İmlecin dikey konumu (Satır)

    public int SelectionStartX { get; set; } = -1; // Seçimin başladığı X noktası
    public int SelectionStartY { get; set; } = -1; // Seçimin başladığı Y noktası
    public bool IsSelecting => SelectionStartX != -1 && SelectionStartY != -1;
}
```
*   **X ve Y**: İmlecin o anki koordinatlarıdır. `X=0, Y=0` dosyanın en başını temsil eder.
*   **SelectionStartX/Y**: Eğer kullanıcı bir seçim yapmıyorsa bu değerler `-1` tutulur. Seçim başladığında imlecin o anki koordinatlarını "hafızaya" alırız.
*   **IsSelecting**: Bir "Property" (Özellik)dir. Eğer başlangıç koordinatları `-1` değilse, program o an bir seçim yapıldığını anlar.

---

## 2. Seçim Başlatma ve Temizleme
```csharp
public void StartSelection()
{
    if (!IsSelecting) // Eğer zaten bir seçim hali hazırda yoksa
    {
        SelectionStartX = X;
        SelectionStartY = Y;
    }
}

public void ClearSelection()
{
    SelectionStartX = -1;
    SelectionStartY = -1;
}
```
*   **StartSelection**: Kullanıcı `Shift` tuşuna bastığında tetiklenir. İmlecin o anki yerini "çapa" (anchor) olarak belirler.
*   **ClearSelection**: Seçim iptal edildiğinde (örneğin `Shift` bırakılıp ok tuşuna basıldığında) koordinatları `-1` yaparak sistemi sıfırlar.

---

## 3. Seçim Alanı Kontrolü (IsInsideSelection) - Kritik Mantık
Bu metod, ekrana metin çizilirken (Renderer) her bir karakter için çağrılır: "Bu karakteri maviye boyamalı mıyım?"

```csharp
// Koordinat Normalleştirme (Swap)
if (startY > endY || (startY == endY && startX > endX))
{
    // Başlangıç noktası bitiş noktasından sonraysa (yukarı/sola seçim), 
    // değerleri yer değiştiririz. Böylece hesaplama hep 'küçükten büyüğe' yapılır.
}
```

**Karar Mantığı (Satır Bazlı):**
1.  **Satır Dışındaysa**: `y < startY || y > endY` ise karakter kesinlikle seçili değildir.
2.  **Ara Satırlardaysa**: `y > startY && y < endY` ise satırın tamamı seçilidir.
3.  **Tek Satırlık Seçim**: `startY == endY` ise karakter `startX` ile `endX` arasındaysa seçilidir.
4.  **Başlangıç Satırı**: `y == startY` ise karakter `startX`'ten sonraysa seçilidir.
5.  **Bitiş Satırı**: `y == endY` ise karakter `endX`'ten önceyse seçilidir.

---

## 4. Akıllı Hareket Sistemi (Move)
Bu metod sadece `X++` veya `Y++` yapmaz, satır sonlarını ve sınırları kontrol eder.

```csharp
if (X < 0) // Satırın en solundayken sola basılırsa
{
    if (Y > 0) // Eğer bir üst satır varsa
    {
        Y--; // Üst satıra çık
        X = lines[Y].Length; // Üst satırın en sonuna git
    }
}
```
*   **Neden?**: Notepad gibi gerçek editörlerdeki "satır atlama" hissini simüle etmek için.
*   **Math.Clamp**: İmlecin dosya boyutundan dışarı çıkmamasını (negatif olmamasını veya toplam satır sayısını aşmamasını) tek satırda sağlar.

---

## 5. Kelime Kelime Atlama (MoveToPreviousWord / NextWord)
Bu metodlar `CTRL + Sol/Sağ` kombinasyonu için yazılmıştır.

```csharp
// MoveToNextWord Mantığı:
// 1. Karakter olmayanları (Boşluk, Tab vb.) atla
while (newX < line.Length && !char.IsWhiteSpace(line[newX])) { newX++; }
// 2. Bir sonraki kelimenin başına kadar boşlukları atla
while (newX < line.Length && char.IsWhiteSpace(line[newX])) { newX++; }
```
*   **Mantık**: Kelimeleri karakter karakter değil, blok blok geçmek için `char.IsWhiteSpace` kontrolü kullanılır. Bu, büyük dosyalarda navigasyonu hızlandırır.
