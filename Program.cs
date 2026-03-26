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
                            var openPath = FileExplorer.PromptForPath("Açılacak dosya yolu");
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
                                FileExplorer.Save(buffer.CurrentFilePath, buffer.GetLines());
                                buffer.IsModified = false;
                            }
                            break;
                        case ConsoleKey.A: // Farklı Kaydet
                            var saveAsPath = FileExplorer.PromptForPath("Kaydedilecek dosya yolu");
                            if (!string.IsNullOrWhiteSpace(saveAsPath))
                            {
                                FileExplorer.Save(saveAsPath, buffer.GetLines());
                                buffer.CurrentFilePath = saveAsPath;
                                buffer.IsModified = false;
                            }
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.SetCursorPosition(0, Console.WindowHeight - 1);
                    Console.BackgroundColor = ConsoleColor.Red;
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write($"HATA: {ex.Message} (Devam etmek için bir tuşa basın)");
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
                        cursor.Move(0, -1, buffer.Lines.Count, buffer.Lines[Math.Max(0, cursor.Y - 1)].Length);
                        break;
                    case ConsoleKey.DownArrow:
                        cursor.Move(0, 1, buffer.Lines.Count, buffer.Lines[Math.Min(buffer.Lines.Count - 1, cursor.Y + 1)].Length);
                        break;
                    case ConsoleKey.LeftArrow:
                        cursor.Move(-1, 0, buffer.Lines.Count, buffer.Lines[cursor.Y].Length);
                        break;
                    case ConsoleKey.RightArrow:
                        cursor.Move(1, 0, buffer.Lines.Count, buffer.Lines[cursor.Y].Length);
                        break;
                    case ConsoleKey.Enter:
                        buffer.NewLine(cursor.X, cursor.Y);
                        cursor.X = 0;
                        cursor.Y++;
                        break;
                    case ConsoleKey.Backspace:
                        if (cursor.X > 0)
                        {
                            buffer.DeleteChar(cursor.X, cursor.Y);
                            cursor.X--;
                        }
                        else if (cursor.Y > 0)
                        {
                            int prevLineLen = buffer.Lines[cursor.Y - 1].Length;
                            buffer.DeleteChar(cursor.X, cursor.Y);
                            cursor.Y--;
                            cursor.X = prevLineLen;
                        }
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
                                        var path = FileExplorer.PromptForPath("Kaydedilecek dosya yolu");
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
                                    Console.Write($"KAYDETME HATASI: {ex.Message} (Çıkmak için bir tuşa basın)");
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
