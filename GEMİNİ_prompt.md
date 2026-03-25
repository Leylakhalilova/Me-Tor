Merhaba Emir, Prolab 1 projenin detaylarını inceledim. Bir iş analisti gözüyle baktığımda, bu projenin en büyük zorluğu metin manipülasyonu ile konsolun görsel yönetimi arasındaki dengeyi kurmak. Oyun geliştirici şapkamla ise, bunu aslında her karesi (frame) klavyeden gelen girdiyle güncellenen "sıra tabanlı" bir oyun motoru gibi düşünmeni öneririm.

[cite_start]Kocaeli Üniversitesi'ndeki bu projen için[cite: 2, 3], C# kullanarak hazırladığım en temelden sona giden stratejik yol haritası aşağıdadır.

---

## 🛠️ Faz 1: Temel Mimari ve Veri Yapıları

Bu aşamada projenin "beynini" oluşturuyoruz. Metni nasıl saklayacağın, performansını ve kod kaliteni belirler.

* [cite_start]**Veri Yapısı Seçimi:** Her karakterin bir veri yapısında tutulması istendiği için `List<List<char>>` veya `List<StringBuilder>` yapısını kullanabilirsin[cite: 82, 83]. Bu, satır ve sütun yönetimini kolaylaştırır.
* [cite_start]**Undo/Redo Sistemi:** Geri alma (Undo) işlemi için bir `Stack` yapısı kurmalısın[cite: 68, 84]. Her anlamlı işlemde (yazma, silme) metnin o anki durumunu veya yapılan değişikliği bu yığına atmalısın.
* [cite_start]**İmleç ve Seçim Takibi:** `CursorX`, `CursorY` ve seçim işlemleri için `SelectionStart` ile `SelectionEnd` indekslerini takip eden bir sınıf oluşturmalısın[cite: 85].

## 📺 Faz 2: Konsol Render Motoru (Görselleştirme)

Konsolu bir ekran gibi yönetme aşaması. `Console.SetCursorPosition` senin en yakın dostun olacak.

* [cite_start]**Toolbox (Şerit) Çizimi:** Ekranın en üstüne sabit bir alan ayır ve komutları (Dosya Aç, Kaydet vb.) kısayollarıyla birlikte buraya yazdır[cite: 16, 25].
* [cite_start]**Satır Numaralandırma:** Ekranın sol tarafına, metin her güncellendiğinde dinamik olarak artan/azalan numaraları ekle[cite: 72, 74, 75].
* [cite_start]**Karakter Desteği:** `Console.OutputEncoding = Encoding.UTF8;` kullanarak Türkçe karakter sorununu en baştan çöz[cite: 29].

## ⌨️ Faz 3: Girdi (Input) ve Temel Düzenleme

Klavyeden gelen her tuş basışını bir "event" gibi yakalaman gerekiyor.

* [cite_start]**Input Loop:** `while(true)` içinde `Console.ReadKey(true)` kullanarak klavyeyi dinle[cite: 14].
* **Karakter Yazımı:** Basılan tuş bir kontrol tuşu değilse, imlecin olduğu konuma karakteri ekle.
* **Silme Mekanizması:**
  * [cite_start]`Backspace`: Tek karakter silme[cite: 71].
  * [cite_start]`CTRL + Backspace`: Kelime bazlı silme [boşluğa kadar olan kısmı siler](cite: 70).
* **Navigasyon:** Yön tuşlarıyla imleci metin içinde hareket ettir.

## 🔍 Faz 4: Gelişmiş Metin Fonksiyonları (Algoritmalar)

İşin içine biraz bilgisayar bilimi teorisi eklediğimiz kısım.

* [cite_start]**Arama ve Değiştirme (CTRL+F, G, H):** String eşleşme algoritması (Basit `IndexOf` veya daha ileri seviye `KMP` algoritması) kullanarak eşleşmeleri bul[cite: 49, 55, 86].
* **Vurgulama (Highlight):**
  * [cite_start]Arama sonuçlarını **mavi** arka planla göster[cite: 50].
  * [cite_start]Seçili metni (CTRL + Yön Tuşları) **sarı** arka planla göster[cite: 63].
* [cite_start]**Clipboard İşlemleri:** `CTRL+C`, `CTRL+X`, `CTRL+V` kombinasyonları için kendi iç "kopyalama tamponunu" (string bir değişken) yönet[cite: 65, 66, 67].

## 📂 Faz 5: Dosya Yönetim Sistemi

Bu bölüm projenin en "arayüz" gibi hissettiren kısmıdır.

* [cite_start]**Özel Dosya Gezgini:** `Directory.GetFiles()` ve `Directory.GetDirectories()` kullanarak konsolda bir liste oluştur[cite: 38, 39].
* [cite_start]**Gezinme:** "İleri" ve "Geri" tuşlarıyla klasörler arasında geçiş yapma mantığını kur[cite: 40, 41].
* [cite_start]**Kaydetme/Açma:** `StreamReader` ve `StreamWriter` kullanarak verileri `.txt` veya benzeri formatlarda işle[cite: 32, 33].

## 🏁 Faz 6: Çıkış ve Final Kontrolleri

* [cite_start]**ESC ile Çıkış:** ESC tuşuna basıldığında kaydedilmemiş değişiklik varsa "Kaydetmek istiyor musunuz? (y/n)" uyarısını ekle[cite: 77, 78, 79].
* **Hata Yönetimi:** Dosya bulunamadığında veya hatalı yetki durumlarında uygulamanın çökmesini engelle.

---

### Proje Teslimi İçin Önemli Not (İş Analisti Tavsiyesi)

[cite_start]Raporunun **IEEE formatında** ve 4 sayfa olması gerektiğini unutma[cite: 93]. [cite_start]Demo sırasında "Neden bu veri yapısını seçtin?" sorusuna hazırlıklı olmalısın; bu yüzden `List` veya `Stack` seçimlerini algoritma karmaşıklığı (Big O) üzerinden açıklayabilmen sana artı puan kazandıracaktır[cite: 101, 102].

Bu adımlardan hangisiyle başlamak istersin? İstersen **C# ile Undo (Geri Al) mekanizması için Stack yapısını** nasıl kurabileceğini detaylandırabilirim.
