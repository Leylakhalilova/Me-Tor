# Renderer.cs - Görselleştirme ve Ekran Yönetimi Analizi

`Renderer.cs`, metin düzenleyicinin görsel kısmını yöneten sınıftır. Console ekranına karakterlerin çizilmesi, renklerin ayarlanması ve ekranın kaydırılması işlemlerini yapar.

---

## 1. Ekran Parametreleri ve Kaydırma (Scrolling)

```csharp
public int ScrollY { get; private set; } = 0;
public int ScrollX { get; private set; } = 0;
```

*   **Scrolling Mantığı**: Eğer dosya 100 satırsa ama terminal ekranı sadece 30 satır gösteriyorsa, `ScrollY` değeri hangi satırdan itibaren çizime başlayacağımızı belirler.
*   **UpdateScroll**: İmleç (cursor) ekranın dışına çıktığında (örneğin aşağı tuşuyla görünmeyen bir satıra gidildiğinde), `ScrollY` veya `ScrollX` değerlerini güncelleyerek imlecin tekrar görünür alana gelmesini sağlar.

---

## 2. Akıllı Çizim Sistemi (Anti-Flicker)

Uygulama `Console.Clear()` metodunu kullanmaz. Çünkü `Clear()` tüm ekranı silip baştan yazdığı için gözle görülür bir titremeye (flickering) neden olur.

Bunun yerine:
*   `Console.SetCursorPosition(0, y)` ile ilgili satıra gidilir.
*   Satırın üzerine yeni içerik yazılır.
*   Satırın sonundaki boşluklar `new string(' ', ...)` ile temizlenir.

---

## 3. Metin Alanının Çizilmesi (RenderBuffer)

Bu metot, metni ekrana basarken 3 farklı katmanı kontrol eder:

1.  **Satır Numaraları**: Her satırın başında (Cyan/Turkuaz renkte) satır numarası ve bir ayraç (`|`) çizer.
2.  **Metin Vurgulama (Highlighting)**:
    *   **Seçim (Selection)**: Kullanıcı Shift tuşuyla metin seçmişse, o bölgeyi **Sarı** arka planla çizer.
    *   **Arama (Search)**: Aranan kelimeyle eşleşen yerleri **Mavi** renkle vurgular.
    *   **Aktif Arama**: Arama sonuçları arasında o an odaklanılan kelimeyi **Lacivert** (DarkBlue) yapar.

---

## 4. Dinamik Toolbox (Üst Menü)

```csharp
private void CalculateToolboxHeight()
```

*   Üstteki yardım menüsü (Dosya, Kaydet, Bul vb.) sabit bir yükseklikte değildir. 
*   Eğer terminal penceresini küçültürseniz, menü öğeleri otomatik olarak alt satıra geçer ve `ToolboxHeight` değeri artar. Metin alanı da buna göre aşağı kaydırılır.

---

## 5. İmleç Dönüşümü (UpdateCursor)

```csharp
int screenX = cursor.X - ScrollX + LineNumberWidth + 2;
int screenY = cursor.Y - ScrollY + ToolboxHeight;
```

*   **Mantık**: Düzenleyici içindeki metin koordinatları (X, Y), ekran koordinatlarıyla aynı değildir. 
*   Metin 0. kolonda başlasa bile, ekranda satır numaralarından sonra (yaklaşık 6-7 karakter içeriden) başlar.
*   `UpdateCursor` metodu, mantıksal koordinatları ekranın fiziksel koordinatlarına dönüştürerek Windows imlecini doğru harfin üzerine yerleştirir.

---

### Sunum İçin Anahtar Noktalar:
*   **Performans**: Karakter karakter değil, stil değiştikçe blok blok çizim yaparak hızı artırır.
*   **Kullanıcı Deneyimi**: Kaydırma (Scrolling) sayesinde sınırsız büyüklükte dosyalar düzenlenebilir.
*   **Görsellik**: Seçim ve arama vurgulamaları ile modern bir editör deneyimi sunar.
