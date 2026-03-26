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

        // Klasör yoksa oluşturmayı deneyebiliriz veya hata verebiliriz. 
        // Şimdilik sadece yazmayı deniyoruz, sistem hatasını yakalayacağız.
        File.WriteAllLines(path, content);
    }

    public static string? PromptForPath(string message)
    {
        // Temizleme ve mesaj yazdırma (Renderer'dan bağımsız bir şekilde alt satıra yazıyoruz)
        Console.SetCursorPosition(0, Console.WindowHeight - 1);
        Console.Write(new string(' ', Console.WindowWidth));
        Console.SetCursorPosition(0, Console.WindowHeight - 1);
        Console.Write($"{message}: ");
        
        string? path = Console.ReadLine();
        
        // Temizle
        Console.SetCursorPosition(0, Console.WindowHeight - 1);
        Console.Write(new string(' ', Console.WindowWidth));
        
        return path;
    }
}
