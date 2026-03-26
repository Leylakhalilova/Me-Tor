using System.Text;

namespace MiniNotepad.IO;

public static class FileExplorer
{
    private static readonly char[] IllegalChars = { '<', '>', ':', '"', '/', '\\', '|', '?', '*' };

    public static string[] Open(string path)
    {
        if (Directory.Exists(path))
            throw new IOException("Belirtilen yol bir klasördür, dosya değil.");
            
        if (!File.Exists(path))
            throw new FileNotFoundException("Dosya bulunamadı.", path);

        return File.ReadAllLines(path);
    }

    public static void Save(string path, IEnumerable<string> content)
    {
        if (Directory.Exists(path))
            throw new IOException("Belirtilen yol bir klasördür, dosya değil.");

        // Dosya adı geçerliliğini kontrol et
        string? fileName = Path.GetFileName(path);
        if (!string.IsNullOrEmpty(fileName) && fileName.Any(c => IllegalChars.Contains(c)))
        {
             // Drive letter check (e.g. C:) - on Windows Path.GetFileName might return it if it's just 'C:'
             if (!(path.Length == 2 && path[1] == ':'))
                throw new IOException("Dosya adı geçersiz karakterler içeriyor.");
        }

        File.WriteAllLines(path, content);
    }

    public static string? Explore(string title, string? initialPath = null, string? defaultFileName = null)
    {
        string currentDir = "";
        bool showingDrives = false;
        
        if (string.IsNullOrWhiteSpace(initialPath))
        {
            currentDir = Directory.GetCurrentDirectory();
        }
        else if (File.Exists(initialPath))
        {
            currentDir = Path.GetDirectoryName(initialPath) ?? Directory.GetCurrentDirectory();
        }
        else if (Directory.Exists(initialPath))
        {
            currentDir = Path.GetFullPath(initialPath);
        }
        else
        {
            currentDir = Directory.GetCurrentDirectory();
        }

        StringBuilder inputBuffer = new StringBuilder(defaultFileName ?? "");
        int scrollOffset = 0;
        int selectedIndex = 0;

        while (true)
        {
            int maxLines = Console.WindowHeight - 10;
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"=== {title.ToUpper()} ===");
            Console.ResetColor();
            
            if (showingDrives)
                Console.WriteLine("Mevcut Dizin: Sürücü Seçimi");
            else
                Console.WriteLine($"Mevcut Dizin: {currentDir}");
                
            Console.WriteLine(new string('-', Console.WindowWidth));

            List<FileSystemInfo> entries = new List<FileSystemInfo>();
            List<DriveInfo> drives = new List<DriveInfo>();

            if (showingDrives)
            {
                drives.AddRange(DriveInfo.GetDrives().Where(d => d.IsReady));
            }
            else
            {
                try
                {
                    var dirInfo = new DirectoryInfo(currentDir);
                    entries.AddRange(dirInfo.GetDirectories().OrderBy(d => d.Name));
                    entries.AddRange(dirInfo.GetFiles().OrderBy(f => f.Name));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Hata: {ex.Message}");
                }
            }

            int totalCount = showingDrives ? drives.Count : entries.Count;

            if (totalCount == 0 && !showingDrives)
            {
                Console.WriteLine(" (Dizin boş) ");
            }
            else
            {
                // selectedIndex sınırlarını koru
                if (selectedIndex < 0) selectedIndex = 0;
                if (totalCount > 0 && selectedIndex >= totalCount) selectedIndex = totalCount - 1;

                // Scroll ayarı
                if (selectedIndex < scrollOffset) scrollOffset = selectedIndex;
                if (selectedIndex >= scrollOffset + maxLines) scrollOffset = selectedIndex - maxLines + 1;

                for (int i = scrollOffset; i < Math.Min(scrollOffset + maxLines, totalCount); i++)
                {
                    if (i == selectedIndex)
                    {
                        Console.BackgroundColor = ConsoleColor.Gray;
                        Console.ForegroundColor = ConsoleColor.Black;
                        Console.Write(" > ");
                    }
                    else
                    {
                        Console.Write("   ");
                    }

                    if (showingDrives)
                    {
                        Console.WriteLine($"[Sürücü] {drives[i].Name} ({drives[i].VolumeLabel})");
                    }
                    else
                    {
                        string type = entries[i] is DirectoryInfo ? "[Klasör]" : "[Dosya] ";
                        Console.WriteLine($"{type} {entries[i].Name}");
                    }
                    Console.ResetColor();
                }
            }

            Console.SetCursorPosition(0, Console.WindowHeight - 4);
            Console.WriteLine(new string('-', Console.WindowWidth));
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("Kısayollar: [OKLAR] Gezin/İleri/Geri | [ENTER] Seç/Onayla | [ESC] İptal");
            Console.ResetColor();
            Console.Write($"> Dosya Adı (veya seçim yapın): {inputBuffer}");

            var keyInfo = Console.ReadKey(true);

            if (keyInfo.Key == ConsoleKey.Enter)
            {
                string input = inputBuffer.ToString().Trim();
                
                // Eğer isim yazılmamışsa ama bir şey seçiliyse
                if (string.IsNullOrEmpty(input))
                {
                    if (showingDrives && drives.Count > 0)
                    {
                        currentDir = drives[selectedIndex].Name;
                        showingDrives = false;
                        selectedIndex = 0;
                        scrollOffset = 0;
                        continue;
                    }
                    else if (entries.Count > 0)
                    {
                        var selected = entries[selectedIndex];
                        if (selected is DirectoryInfo)
                        {
                            currentDir = selected.FullName;
                            selectedIndex = 0;
                            scrollOffset = 0;
                            inputBuffer.Clear();
                            continue;
                        }
                        else
                        {
                            return selected.FullName;
                        }
                    }
                }
                
                if (!string.IsNullOrEmpty(input))
                {
                    // absolute path check
                    string fullPath;
                    if (Path.IsPathRooted(input))
                    {
                        fullPath = input;
                    }
                    else
                    {
                        fullPath = Path.Combine(currentDir, input);
                    }

                    if (Directory.Exists(fullPath))
                    {
                        currentDir = fullPath;
                        inputBuffer.Clear();
                        selectedIndex = 0;
                        scrollOffset = 0;
                        showingDrives = false;
                    }
                    else
                    {
                        // Geçersiz karakter kontrolü (sadece dosya kısmı için)
                        string? fName = Path.GetFileName(fullPath);
                        if (!string.IsNullOrEmpty(fName) && fName.Any(c => IllegalChars.Contains(c)))
                        {
                            // ignore for now or show error?
                            continue; 
                        }
                        return fullPath;
                    }
                }
            }
            else if (keyInfo.Key == ConsoleKey.UpArrow)
            {
                if (selectedIndex > 0) selectedIndex--;
            }
            else if (keyInfo.Key == ConsoleKey.DownArrow)
            {
                if (selectedIndex < totalCount - 1) selectedIndex++;
            }
            else if (keyInfo.Key == ConsoleKey.RightArrow) // İleri
            {
                if (showingDrives)
                {
                    currentDir = drives[selectedIndex].Name;
                    showingDrives = false;
                    selectedIndex = 0;
                    scrollOffset = 0;
                }
                else if (entries.Count > 0)
                {
                    var selected = entries[selectedIndex];
                    if (selected is DirectoryInfo)
                    {
                        currentDir = selected.FullName;
                        selectedIndex = 0;
                        scrollOffset = 0;
                        inputBuffer.Clear();
                    }
                    else if (string.IsNullOrEmpty(inputBuffer.ToString()))
                    {
                        inputBuffer.Append(selected.Name);
                    }
                }
            }
            else if (keyInfo.Key == ConsoleKey.LeftArrow) // Geri
            {
                if (showingDrives) continue;

                var parent = Directory.GetParent(currentDir);
                if (parent != null)
                {
                    currentDir = parent.FullName;
                    selectedIndex = 0;
                    scrollOffset = 0;
                }
                else
                {
                    // Root'dayız, sürücüleri göster
                    showingDrives = true;
                    selectedIndex = 0;
                    scrollOffset = 0;
                }
            }
            else if (keyInfo.Key == ConsoleKey.Escape)
            {
                return null;
            }
            else if (keyInfo.Key == ConsoleKey.Backspace)
            {
                if (inputBuffer.Length > 0) inputBuffer.Remove(inputBuffer.Length - 1, 1);
            }
            else if (!char.IsControl(keyInfo.KeyChar))
            {
                inputBuffer.Append(keyInfo.KeyChar);
            }
        }
    }

    public static string? PromptForPath(string message)
    {
        return Explore(message);
    }
}
