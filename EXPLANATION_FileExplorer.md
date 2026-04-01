# FileExplorer.cs - Dosya Sistemi ve Gezgin Analizi

`FileExplorer.cs`, uygulamanın dosyaları okuma, yazma ve kullanıcıya görsel bir dosya seçme arayüzü sunma görevlerini yöneten statik bir sınıftır.

---

## 1. Hata Yönetimi ve Güvenlik (Open & Save)

Uygulamanın kararlı çalışması için dosya işlemlerinde sıkı kontroller yapılır:

*   **Klasör Kontrolü**: Bir dosyayı açmaya çalışırken, verilen yolun bir klasör (`Directory.Exists`) olup olmadığı kontrol edilir. Klasörler dosya gibi açılamayacağı için hata fırlatılır.
*   **Geçersiz Karakterler**: `IllegalChars` dizisi (`<, >, :, ", /, \, |, ?, *`) ile dosya adları kontrol edilir. Bu, işletim sisteminin kabul etmeyeceği isimlerin verilmesini önler.
*   **Dosya Varlığı**: Açılmak istenen dosya yerinde yoksa `FileNotFoundException` ile kullanıcı uyarılır.

---

## 2. Görsel Dosya Gezgini (Explore Metodu)

Bu metodun kalbi, kullanıcı bir seçim yapana veya iptal edene kadar dönen bir `while(true)` döngüsüdür.

### Gezinme Mantığı:
*   **Dizin Listeleme**: `DirectoryInfo` sınıfı kullanılarak o anki klasördeki klasörler ve dosyalar listelenir.
*   **Sürücü Desteği**: Sadece klasörler arasında değil, bilgisayardaki farklı sürücüler (C:, D: vb.) arasında da geçiş yapılabilir.
*   **Sıralama**: Daha iyi bir deneyim için önce klasörler, sonra dosyalar alfabetik olarak gösterilir.

---

## 3. UI ve Kaydırma (Scrolling) Mekanizması

Terminal ekranı kısıtlı bir alana sahip olduğu için (örneğin 30 satır), yüzlerce dosyanın olduğu bir klasörü göstermek için özel bir mantık kullanılır:

```csharp
if (selectedIndex < scrollOffset) scrollOffset = selectedIndex;
if (selectedIndex >= scrollOffset + maxLines) scrollOffset = selectedIndex - maxLines + 1;
```

*   **selectedIndex**: Kullanıcının ok tuşlarıyla üzerinde durduğu öğe.
*   **scrollOffset**: Ekranda gösterilen ilk öğenin listedeki sırası.
*   Bu iki değişken sayesinde, kullanıcı listenin altına indikçe liste yukarı doğru kayar (scrolling).

---

## 4. Klavye Kısayolları ve Kontroller

*   **Yukarı/Aşağı Oklar**: Listede gezinmeyi sağlar.
*   **Sol Ok**: Bir üst klasöre (Parent Directory) çıkmayı sağlar.
*   **Sağ Ok / Enter**: 
    *   Seçili öğe bir klasörse içine girer.
    *   Seçili öğe bir dosyaysa o dosyanın yolunu uygulamaya döndürür.
*   **Yazma (Input Buffer)**: Kullanıcı sadece seçim yapmakla kalmaz, kaydetmek istediği dosyanın adını klavyeden doğrudan yazabilir.

---

## 5. Dinamik Ekran Güncelleme

Gezgin her hareketinizde `Console.Clear()` ile ekranı temizler ve:
1.  Üstte mevcut dizin yolunu,
2.  Ortada klasör/dosya listesini,
3.  Altta ise kullanılabilir kısayolları ve yazma alanını çizer.

---

### Sunum İçin Önemli Notlar:
*   **Statik Sınıf**: Bu sınıfın durumu (`state`) yoktur, sadece araç setidir (`Utility class`).
*   **UX (Kullanıcı Deneyimi)**: Kullanıcının dosya yolunu elle yazma zahmetini ortadan kaldırır, görsel bir seçim sunar.
*   **Platform Bağımsızlığı**: `Path.Combine` ve `Path.GetDirectoryName` gibi metodlar kullanılarak hem Windows hem de Linux/macOS yollarıyla uyumlu çalışması hedeflenmiştir.
