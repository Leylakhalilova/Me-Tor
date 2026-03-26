using System.Text;

namespace MiniNotepad.IO;

public static class FileExplorer
{
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

        File.WriteAllLines(path, content);
    }

    public static string? Explore(string title, string? initialPath = null, string? defaultFileName = null)
    {
        string currentDir;
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
            Console.WriteLine($"Mevcut Dizin: {currentDir}");
            Console.WriteLine(new string('-', Console.WindowWidth));

            List<FileSystemInfo> entries = new List<FileSystemInfo>();
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

            if (entries.Count == 0)
            {
                Console.WriteLine(" (Dizin boş) ");
            }
            else
            {
                // selectedIndex sınırlarını koru
                if (selectedIndex < 0) selectedIndex = 0;
                if (entries.Count > 0 && selectedIndex >= entries.Count) selectedIndex = entries.Count - 1;

                // Scroll ayarı
                if (selectedIndex < scrollOffset) scrollOffset = selectedIndex;
                if (selectedIndex >= scrollOffset + maxLines) scrollOffset = selectedIndex - maxLines + 1;

                for (int i = scrollOffset; i < Math.Min(scrollOffset + maxLines, entries.Count); i++)
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

                    string type = entries[i] is DirectoryInfo ? "[Klasör]" : "[Dosya] ";
                    Console.WriteLine($"{type} {entries[i].Name}");
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
                string fileName = inputBuffer.ToString().Trim();
                
                // Eğer isim yazılmamışsa ama bir şey seçiliyse
                if (string.IsNullOrEmpty(fileName) && entries.Count > 0)
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
                
                if (!string.IsNullOrEmpty(fileName))
                {
                    string fullPath = Path.Combine(currentDir, fileName);
                    if (Directory.Exists(fullPath))
                    {
                        currentDir = fullPath;
                        inputBuffer.Clear();
                        selectedIndex = 0;
                        scrollOffset = 0;
                    }
                    else
                    {
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
                if (selectedIndex < entries.Count - 1) selectedIndex++;
            }
            else if (keyInfo.Key == ConsoleKey.RightArrow) // İleri
            {
                if (entries.Count > 0)
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
                var parent = Directory.GetParent(currentDir);
                if (parent != null)
                {
                    currentDir = parent.FullName;
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

    // Eski metodu silebiliriz veya geriye dönük uyumluluk için bırakabiliriz. 
    // Ama Explore varken PromptForPath'e gerek yok.
    public static string? PromptForPath(string message)
    {
        return Explore(message);
    }
}
