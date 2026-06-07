Proje sunumunuz için hazırladığım kapsamlı rehber aşağıdadır. Bu belge, projenin mimarisinden her bir satır kodun işlevine, PDF'deki isterlerin kodda nerede karşılandığına kadar her detayı içermektedir.

  Mini Notepad++ Proje Sunum ve Teknik Detay Rehberi

  Bu belge, Kocaeli Üniversitesi Programlama Laboratuvarı-II dersi kapsamında geliştirilen Konsol Tabanlı Metin Editörü projesinin sunumu için hazırlanmıştır.

  ---

  1. Proje Genel Yapısı ve Mimari (Architecture)

  Proje, C# .NET platformu üzerinde geliştirilmiştir ve MVC (Model-View-Controller) benzeri bir ayrım gözetilerek modüler bir yapıda tasarlanmıştır:

   * Program.cs (Main Entry/Controller): Uygulamanın ana döngüsü (Event Loop). Kullanıcı girdilerini yakalar (Input Handling), kısayolları yönetir ve ilgili komutları Core katmanına iletir.
   * Core/ (Model/Logic):
       * EditorBuffer.cs: Metin verisinin (karakter karakter ve satır satır) tutulduğu ve düzenlendiği ana sınıftır.
       * UndoManager.cs: "Geri Al" (Undo) mekanizmasını Stack veri yapısı ile yönetir.
       * Cursor.cs: İmleç koordinatlarını (X, Y) ve metin seçme işlemlerini yönetir.
   * UI/ (View/Rendering):
       * Renderer.cs: Konsol ekranını çizdirir. Toolbox, satır numaraları, metin içeriği ve vurgulamaları (Highlighting) yönetir. Flicker (titreme) önleyici yöntemler kullanır.
       * Toolbox.cs: Üst menüdeki komutları ve kısayolları tanımlar.
   * IO/ (Data Access):
       * FileExplorer.cs: Dosya sistemi üzerinde gezinme, klasör listeleme, dosya açma ve kaydetme işlemlerini (Custom File Manager) yönetir.
   * Utils/ (Helpers):
       * SearchAlgorithms.cs: Metin arama işlemlerinde kullanılan algoritmaları içerir.

  ---

  2. PDF İsterlerinin Kodda Karşılıkları (Mapping Requirements to Code)

  2.1. Genel Uygulama Özellikleri
   * main.exe ile başlatma: Program.cs dosyasındaki Main metodu ile başlar.
   * Başlangıçta boş dosya: Program.cs'de new EditorBuffer() oluşturulur ve buffer.CurrentFilePath = "yeni_dosya.txt" olarak atanır.
   * Toolbox (Şerit Yapısı): UI/Renderer.cs içindeki RenderToolbox metodu ile ekranın en üstünde çizilir. Dinamik olarak satır yüksekliğini ayarlar.
   * Fare Yok, Tamamen Klavye: Console.ReadKey(true) kullanılarak tüm girdiler klavyeden alınır.
   * İmleç Tasarımı: Renderer.cs içindeki UpdateCursor metodu, Console.SetCursorPosition ile Windows terminalinin standart beyaz yanıp sönen imlecini metin pozisyonuna yerleştirir.
   * Türkçe Karakter Desteği: Program.cs başında Console.OutputEncoding = Encoding.UTF8 ayarı ile sağlanmıştır.

  2.2. Dosya İşlemleri (IO)
   * Dosya Yöneticisi: IO/FileExplorer.cs içinde Explore metodu tamamen özel olarak kodlanmıştır.
       * Gezinme: Directory.GetDirectories() ve GetFiles() ile listeleme yapılır. Ok tuşları ile seçilir.
       * İleri/Geri: LeftArrow (Geri) üst dizine çıkar, RightArrow (İleri) klasör içine girer.
   * Kaydet/Aç: FileExplorer.Save ve Open metotları standart C# File API'lerini kullanır.

  2.3. Metin Düzenleme ve Kısayollar

  ┌──────────────────┬───────────────────────┬─────────────────────────┬─────────────────────────────────────────────────────────────────────────────┐
  │ Özellik          │ Kısayol               │ Kod Konumu (Program.cs) │ Mantık (Logic)                                                              │
  ├──────────────────┼───────────────────────┼─────────────────────────┼─────────────────────────────────────────────────────────────────────────────┤
  │ Bul (Kısmi)      │ CTRL+F                │ isCtrlF bloğu           │ buffer.UpdateSearch(term, false) - Regex ile kısmi eşleşme.                 │
  │ Eşleşme Bulucu   │ CTRL+G                │ isCtrlG bloğu           │ buffer.UpdateSearch(term, true) - \b word boundary ile tam kelime arama.    │
  │ Değiştir         │ CTRL+H (Kodda CTRL+R) │ isCtrlR bloğu           │ buffer.ReplaceAll(old, new) - Tüm eşleşmeleri toplu değiştirir.             │
  │ Seçme (Karakter) │ CTRL + Yön            │ isNavKey ve isCtrl      │ cursor.StartSelection() ve cursor.Move() ile koordinatlar tutulur.          │
  │ Seçme (Kelime)   │ CTRL+SHIFT+Yön        │ isShift bloğu           │ cursor.MoveToNextWord / MoveToPreviousWord metotları.                       │
  │ Kopyala          │ CTRL+C                │ isCtrlC bloğu           │ buffer.GetSelectedText metni internalClipboard değişkenine alır.            │
  │ Kes              │ CTRL+X                │ isCtrlX bloğu           │ Kopyalar ve ardından buffer.DeleteSelection çağırır.                        │
  │ Yapıştır         │ CTRL+V                │ isCtrlV bloğu           │ buffer.InsertText ile imleç konumuna metni ekler.                           │
  │ Geri Al (Undo)   │ CTRL+Z                │ isCtrlZ bloğu           │ buffer.Undo() -> UndoManager stack'ten son aksiyonu çeker ve tersini yapar. │
  │ Kelime Silme     │ CTRL+Backspace        │ isCtrlBackspace         │ buffer.DeleteWord -> Boşlukları ve ardından kelime karakterlerini siler.    │
  │ Karakter Silme   │ Backspace             │ ConsoleKey.Backspace    │ buffer.DeleteChar -> StringBuilder.Remove kullanır.                         │
  └──────────────────┴───────────────────────┴─────────────────────────┴─────────────────────────────────────────────────────────────────────────────┘


  2.4. Satır Numaralandırma
   * Otomatik Artış/Azalış: UI/Renderer.cs içindeki RenderBuffer döngüsünde lineIdx + 1 ifadesi ile her satırın başına yazılır. Metin eklendikçe veya silindikçe buffer.Lines listesinin boyutu değiştiği için
     numaralar dinamik olarak güncellenir.

  2.5. Çıkış İşlemi
   * ESC: Program.cs'de ConsoleKey.Escape yakalanır.
   * Kaydetme Sorusu: buffer.IsModified bayrağı true ise Değişiklikleri kaydetmek istiyor musunuz? (e/h) sorusu sorulur.

  ---

  3. Veri Yapıları ve Tasarım Kararları (Teknik Detaylar)

   * Metin Depolama: List<StringBuilder>.
       * Neden? StringBuilder karakter ekleme ve silme işlemlerinde (mutable) String'e göre çok daha performanslıdır. List yapısı ise satır ekleme ve silmede esneklik sağlar.
   * Undo (Geri Al) Mekanizması: Stack<EditorAction>.
       * Neden? LIFO (Last In First Out) prensibi "Son yapılanı ilk geri al" mantığına tam uyar. EditorAction kaydı; yapılan işlemin tipini (Ekleme, Silme, Satır Bölme vb.), koordinatlarını ve etkilenen metni
         saklar.
   * Arama Algoritması: System.Text.RegularExpressions (Regex).
       * Neden? Hem kısmi eşleşme (partial match) hem de tam kelime eşleşmesi (\b sınır belirleyici ile) için en optimize ve güçlü yoldur.
   * Ekran Yenileme (Rendering): Çift tamponlama (double buffering) mantığına yakın bir sistem kurulmuştur. Console.Clear() yerine satırlar tek tek üzerine yazılır (Console.SetCursorPosition), bu da ekranın
     yanıp sönmesini (flickering) minimuma indirir.

  ---

  4. Kritik Kod Blokları Açıklaması (Önemli Sorular İçin)

  Soru: "Geri Al" (Undo) işlemini nasıl gerçekleştirdiniz?
  Cevap: UndoManager sınıfında bir Stack tutuyoruz. Her düzenleme işleminde (karakter yazma, silme, yapıştırma) PushAction metodu ile o işlemin tersini yapmamızı sağlayacak bilgileri kaydediyoruz. Örneğin, bir
  karakter silindiğinde, o karakteri ve silindiği koordinatı stack'e atıyoruz. CTRL+Z'ye basıldığında stack'ten son elemanı Pop edip silinen karakteri geri Insert ediyoruz.

  Soru: Metin Seçme (Selection) mantığı nasıl çalışıyor?
  Cevap: EditorCursor sınıfı SelectionStartX ve SelectionStartY adında iki değişken tutar. Kullanıcı CTRL+Yön tuşlarına bastığında imlecin o anki konumu "başlangıç" olarak işaretlenir. İmleç hareket ettikçe
  mevcut konumu "bitiş" olarak kabul edilir. IsInsideSelection(x, y) metodu, o an çizilmekte olan karakterin bu koordinatlar arasında olup olmadığını kontrol eder; arasındaysa Renderer o karakteri
  ConsoleColor.Yellow (Sarı) arka plan ile çizer.

  Soru: Dosya Yöneticisi'ni (File Explorer) nasıl tasarladınız?
  Cevap: IO/FileExplorer.Explore metodu bir sonsuz döngü içinde çalışır. Directory.GetFileSystemEntries() ile o anki klasörün içeriğini alırız. Kullanıcı ok tuşlarıyla listede gezer. "Enter" basıldığında eğer
  seçilen bir klasörse Directory.SetCurrentDirectory ile içeri gireriz, eğer dosyaysa yolunu döndürürüz. Ayrıca kullanıcı elle dosya ismi de yazabilir.

  Soru: Büyük dosyalarda performans sorununu nasıl önlediniz? (Scrolling)
  Cevap: Renderer sınıfında ScrollX ve ScrollY değişkenleri var. Tüm dosyayı değil, sadece terminalin pencere boyutu kadar olan kısmı (Console.WindowHeight ve Width) ekrana çizdiriyoruz. İmleç ekranın dışına
  çıktığında bu scroll değerlerini güncelleyerek "pencereyi" metin üzerinde kaydırıyoruz.

  ---

  5. Sunum Sırasında İzlenecek Yol (Demo Akışı)

   1. Açılış: Uygulamayı başlatın ve boş bir ekran geldiğini gösterin.
   2. Yazma: Birkaç satır metin yazın, Türkçe karakterleri (ğ, ü, ş, İ) kullanın.
   3. Düzenleme: CTRL+Backspace ile kelime silin, CTRL+Z ile geri alın.
   4. Seçme & Kopyalama: Bir cümleyi CTRL+Yön tuşlarıyla sarı yapın, CTRL+C ile kopyalayıp alt satıra CTRL+V ile yapıştırın.
   5. Arama: CTRL+F ile bir kelime aratın, mavi vurguyu gösterin. CTRL+G ile tam kelime aratıp eşleşmeler arasında (Ok tuşlarıyla) gezin.
   6. Dosya Kaydetme: CTRL+S basın, Dosya Yöneticisi açılacak. Bir isim verip kaydedin.
   7. Dosya Açma: CTRL+O ile başka bir dosya seçip içeriğin yüklendiğini gösterin.
   8. Kapanış: ESC basın, değişiklik varsa "Kaydetmek istiyor musunuz?" sorusuna 'h' diyerek çıkın.
