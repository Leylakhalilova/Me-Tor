using System.Text;
using MiniNotepad.Core;
using MiniNotepad.UI;
using MiniNotepad.IO;

namespace MiniNotepad;

class Program
{
    static void Main(string[] args)
    {
        // UTF-8 desteği
        Console.OutputEncoding = Encoding.UTF8;
        Console.CursorVisible = true;

        var buffer = new EditorBuffer();
        var cursor = new EditorCursor();
        var renderer = new Renderer();

        bool running = true;
        renderer.Render(buffer, cursor);

        while (running)
        {
            var keyInfo = Console.ReadKey(true);

            // Control tuşu kombinasyonları
            if (keyInfo.Modifiers.HasFlag(ConsoleModifiers.Control))
            {
                try
                {
                    switch (keyInfo.Key)
                    {
                        case ConsoleKey.O: // Aç
                            var openPath = FileExplorer.Explore("Dosya Aç");
                            if (!string.IsNullOrWhiteSpace(openPath))
                            {
                                var lines = FileExplorer.Open(openPath);
                                buffer.Load(lines, openPath);
                                cursor.X = 0;
                                cursor.Y = 0;
                            }
                            break;
                        case ConsoleKey.S: // Kaydet
                            if (string.IsNullOrEmpty(buffer.CurrentFilePath))
                            {
                                goto case ConsoleKey.A;
                            }
                            else
                            {
                                // Proje isterine göre her durumda gezgin açılabilir veya direkt kaydedilebilir.
                                // Burada kullanıcıya kolaylık olması için varsayılan olarak mevcut yolu veriyoruz.
                                string? savePath = FileExplorer.Explore("Dosya Kaydet", Path.GetDirectoryName(buffer.CurrentFilePath), Path.GetFileName(buffer.CurrentFilePath));
                                if (!string.IsNullOrWhiteSpace(savePath))
                                {
                                    FileExplorer.Save(savePath, buffer.GetLines());
                                    buffer.CurrentFilePath = savePath;
                                    buffer.IsModified = false;
                                }
                            }
                            break;
                        case ConsoleKey.A: // Farklı Kaydet
                            var saveAsPath = FileExplorer.Explore("Farklı Kaydet", 
                                buffer.CurrentFilePath != null ? Path.GetDirectoryName(buffer.CurrentFilePath) : null,
                                buffer.CurrentFilePath != null ? Path.GetFileName(buffer.CurrentFilePath) : null);
                            if (!string.IsNullOrWhiteSpace(saveAsPath))
                            {
                                FileExplorer.Save(saveAsPath, buffer.GetLines());
                                buffer.CurrentFilePath = saveAsPath;
                                buffer.IsModified = false;
                            }
                            break;
                        case ConsoleKey.Z: // Geri Al
                            buffer.Undo(cursor);
                            break;
                        case ConsoleKey.F: // Bul
                            Console.SetCursorPosition(0, Console.WindowHeight - 1);
                            Console.Write("Aranacak metin: ");
                            string? searchTerm = Console.ReadLine();
                            if (!string.IsNullOrEmpty(searchTerm))
                            {
                                // Basit arama: ilk eşleşmeyi bul
                                bool found = false;
                                for (int i = 0; i < buffer.Lines.Count; i++)
                                {
                                    int idx = buffer.Lines[i].ToString().IndexOf(searchTerm);
                                    if (idx != -1)
                                    {
                                        cursor.Y = i;
                                        cursor.X = idx;
                                        found = true;
                                        break;
                                    }
                                }
                                if (!found)
                                {
                                    Console.SetCursorPosition(0, Console.WindowHeight - 1);
                                    Console.Write("Bulunamadı! (Bir tuşa basın)");
                                    Console.ReadKey(true);
                                }
                            }
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.SetCursorPosition(0, Console.WindowHeight - 1);
                    Console.BackgroundColor = ConsoleColor.Red;
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write($"HATA: {ex.Message.PadRight(Console.WindowWidth - 10)}");
                    Console.ResetColor();
                    Console.ReadKey(true);
                }
            }
            else
            {
                // Temel navigasyon ve düzenleme
                switch (keyInfo.Key)
                {
                    case ConsoleKey.UpArrow:
                        cursor.Move(0, -1, buffer.Lines);
                        break;
                    case ConsoleKey.DownArrow:
                        cursor.Move(0, 1, buffer.Lines);
                        break;
                    case ConsoleKey.LeftArrow:
                        cursor.Move(-1, 0, buffer.Lines);
                        break;
                    case ConsoleKey.RightArrow:
                        cursor.Move(1, 0, buffer.Lines);
                        break;
                    case ConsoleKey.Home:
                        cursor.X = 0;
                        break;
                    case ConsoleKey.End:
                        cursor.X = buffer.Lines[cursor.Y].Length;
                        break;
                    case ConsoleKey.Enter:
                        buffer.NewLine(cursor.X, cursor.Y);
                        cursor.X = 0;
                        cursor.Y++;
                        break;
                    case ConsoleKey.Backspace:
                        if (cursor.X > 0 || cursor.Y > 0)
                        {
                            int oldX = cursor.X;
                            int oldY = cursor.Y;
                            if (cursor.X == 0)
                            {
                                cursor.Y--;
                                cursor.X = buffer.Lines[cursor.Y].Length;
                            }
                            else
                            {
                                cursor.X--;
                            }
                            buffer.DeleteChar(oldX, oldY);
                        }
                        break;
                    case ConsoleKey.Delete:
                        buffer.DeleteForward(cursor.X, cursor.Y);
                        break;
                    case ConsoleKey.Escape:
                        if (buffer.IsModified)
                        {
                            Console.SetCursorPosition(0, Console.WindowHeight - 1);
                            Console.Write("Değişiklikleri kaydetmek istiyor musunuz? (e/h): ");
                            var choice = Console.ReadKey(true).Key;
                            if (choice == ConsoleKey.E)
                            {
                                try
                                {
                                    if (string.IsNullOrEmpty(buffer.CurrentFilePath))
                                    {
                                        var path = FileExplorer.Explore("Kapanış Öncesi Kaydet");
                                        if (!string.IsNullOrWhiteSpace(path))
                                            FileExplorer.Save(path, buffer.GetLines());
                                    }
                                    else
                                    {
                                        FileExplorer.Save(buffer.CurrentFilePath, buffer.GetLines());
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Console.SetCursorPosition(0, Console.WindowHeight - 1);
                                    Console.BackgroundColor = ConsoleColor.Red;
                                    Console.Write($"KAYDETME HATASI: {ex.Message}");
                                    Console.ResetColor();
                                    Console.ReadKey(true);
                                }
                            }
                        }
                        running = false;
                        break;
                    default:
                        if (!char.IsControl(keyInfo.KeyChar))
                        {
                            buffer.InsertChar(cursor.X, cursor.Y, keyInfo.KeyChar);
                            cursor.X++;
                        }
                        break;
                }
            }

            renderer.Render(buffer, cursor);
        }
    }
}
