using System.Text;
using System.Runtime.InteropServices;
using MiniNotepad.Core;
using MiniNotepad.UI;
using MiniNotepad.IO;

namespace MiniNotepad;

class Program
{
    // Windows terminal ayarlarını değiştirmek için Win32 API
    [DllImport("kernel32.dll", SetLastError = true)]
    static extern IntPtr GetStdHandle(int nStdHandle);

    [DllImport("kernel32.dll", SetLastError = true)]
    static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

    [DllImport("kernel32.dll", SetLastError = true)]
    static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

    const int STD_INPUT_HANDLE = -10;
    const uint ENABLE_PROCESSED_INPUT = 0x0001;

    static void Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;
        Console.InputEncoding = Encoding.UTF8;
        Console.CursorVisible = true;
        Console.TreatControlCAsInput = true;

        // Windows'ta CTRL+S gibi tuşların terminal tarafından yutulmasını engelle
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            IntPtr hIn = GetStdHandle(STD_INPUT_HANDLE);
            if (GetConsoleMode(hIn, out uint mode))
            {
                // ENABLE_PROCESSED_INPUT (0x0001): CTRL+C, CTRL+S gibi tuşların sistem tarafından işlenmesini engeller
                // ENABLE_QUICK_EDIT_MODE (0x0040): Fare ile seçim yapmayı engeller (inputu durdurabilir)
                // ENABLE_EXTENDED_FLAGS (0x0080): QuickEdit gibi flagleri değiştirmek için gereklidir
                mode &= ~ENABLE_PROCESSED_INPUT;
                mode &= ~0x0040u; // QuickEdit OFF (uint literal eklendi)
                mode |= 0x0080u;  // Extended Flags ON (uint literal eklendi)
                SetConsoleMode(hIn, mode);
            }
        }

        var buffer = new EditorBuffer();
        var cursor = new EditorCursor();
        var renderer = new Renderer();

        bool running = true;
        renderer.Render(buffer, cursor);

        while (running)
        {
            var keyInfo = Console.ReadKey(true);
            bool isCtrl = (keyInfo.Modifiers & ConsoleModifiers.Control) != 0;
            bool isShift = (keyInfo.Modifiers & ConsoleModifiers.Shift) != 0;

            // Hem ConsoleKey hem de ASCII 19 (CTRL+S) kontrolü yapıyoruz
            // Bazı Windows terminallerinde sadece KeyChar 19 gelir, bazılarında Modifiers+Key
            bool isCtrlS = (keyInfo.Key == ConsoleKey.S && (keyInfo.Modifiers & ConsoleModifiers.Control) != 0) || 
                           (keyInfo.KeyChar == (char)19);
            bool isCtrlZ = (keyInfo.Key == ConsoleKey.Z && (keyInfo.Modifiers & ConsoleModifiers.Control) != 0) || 
                           (keyInfo.KeyChar == (char)26);

            bool isCtrlG = (keyInfo.Key == ConsoleKey.G && (keyInfo.Modifiers & ConsoleModifiers.Control) != 0) || 
                           (keyInfo.KeyChar == (char)7); // CTRL+G
            bool isCtrlH = (keyInfo.Key == ConsoleKey.H && (keyInfo.Modifiers & ConsoleModifiers.Control) != 0) || 
                           (keyInfo.KeyChar == (char)8); // CTRL+H

            if ((keyInfo.Modifiers & ConsoleModifiers.Control) != 0 || isCtrlS || isCtrlZ || isCtrlG || isCtrlH)
            {
                try
                {
                    // CTRL+S ise Kaydet
                    if (isCtrlS)
                    {
                        cursor.ClearSelection();
                        // ... existing save logic ... (shortened for brevity in thought, but I must provide full code)
                        if (string.IsNullOrEmpty(buffer.CurrentFilePath))
                        {
                            var saveAsPath = FileExplorer.Explore("Farklı Kaydet", null, null);
                            if (!string.IsNullOrWhiteSpace(saveAsPath))
                            {
                                FileExplorer.Save(saveAsPath, buffer.GetLines());
                                buffer.CurrentFilePath = saveAsPath;
                                buffer.IsModified = false;
                            }
                        }
                        else
                        {
                            string? savePath = FileExplorer.Explore("Dosya Kaydet", Path.GetDirectoryName(buffer.CurrentFilePath), Path.GetFileName(buffer.CurrentFilePath));
                            if (!string.IsNullOrWhiteSpace(savePath))
                            {
                                FileExplorer.Save(savePath, buffer.GetLines());
                                buffer.CurrentFilePath = savePath;
                                buffer.IsModified = false;
                            }
                        }
                    }
                    else if (isCtrlZ)
                    {
                        buffer.Undo(cursor);
                        cursor.ClearSelection();
                    }
                    else if (isCtrlG)
                    {
                        cursor.ClearSelection();
                        Console.SetCursorPosition(0, Console.WindowHeight - 1);
                        Console.Write("Aranacak metin (Eşleşme Bulucu): ");
                        string? searchTerm = ReadLineWithEsc();
                        if (!string.IsNullOrEmpty(searchTerm))
                        {
                            buffer.UpdateSearch(searchTerm);
                            if (buffer.Matches.Count > 0)
                            {
                                var firstMatch = buffer.Matches[0];
                                cursor.Y = firstMatch.LineIndex;
                                cursor.X = firstMatch.ColumnIndex;
                                buffer.CurrentMatchIndex = 0;
                            }
                            else
                            {
                                ShowStatus("Eşleşme bulunamadı!");
                            }
                        }
                    }
                    else if (isCtrlH)
                    {
                        cursor.ClearSelection();
                        Console.SetCursorPosition(0, Console.WindowHeight - 1);
                        Console.Write("Değiştirilecek kelime: ");
                        string? oldText = ReadLineWithEsc();
                        if (oldText != null)
                        {
                            Console.SetCursorPosition(0, Console.WindowHeight - 1);
                            Console.Write(new string(' ', Console.WindowWidth - 1));
                            Console.SetCursorPosition(0, Console.WindowHeight - 1);
                            Console.Write("Yeni kelime: ");
                            string? newText = ReadLineWithEsc();
                            if (newText != null)
                            {
                                buffer.ReplaceAll(oldText, newText);
                                ShowStatus("Tüm eşleşmeler değiştirildi.");
                            }
                        }
                    }
                    else
                    {
                        switch (keyInfo.Key)
                        {
                            case ConsoleKey.O: // Aç
                                cursor.ClearSelection();
                                var openPath = FileExplorer.Explore("Dosya Aç");
                                if (!string.IsNullOrWhiteSpace(openPath))
                                {
                                    var lines = FileExplorer.Open(openPath);
                                    buffer.Load(lines, openPath);
                                    cursor.X = 0;
                                    cursor.Y = 0;
                                }
                                break;
                            case ConsoleKey.A: // Farklı Kaydet
                                cursor.ClearSelection();
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
                            case ConsoleKey.F: // Bul
                                cursor.ClearSelection();
                                Console.SetCursorPosition(0, Console.WindowHeight - 1);
                                Console.Write("Aranacak metin: ");
                                string? searchTerm = ReadLineWithEsc();
                                if (!string.IsNullOrEmpty(searchTerm))
                                {
                                    buffer.UpdateSearch(searchTerm);
                                    if (buffer.Matches.Count > 0)
                                    {
                                        var firstMatch = buffer.Matches[0];
                                        cursor.Y = firstMatch.LineIndex;
                                        cursor.X = firstMatch.ColumnIndex;
                                    }
                                    else
                                    {
                                        ShowStatus("Bulunamadı!");
                                    }
                                }
                                else
                                {
                                    buffer.ClearSearch();
                                }
                                break;
                            case ConsoleKey.Backspace:
                                cursor.ClearSelection();
                                buffer.DeleteWord(cursor);
                                break;
                            case ConsoleKey.LeftArrow:
                            case ConsoleKey.RightArrow:
                            case ConsoleKey.UpArrow:
                            case ConsoleKey.DownArrow:
                                buffer.ClearSearch();
                                cursor.StartSelection();
                                if (isShift)
                                {
                                    if (keyInfo.Key == ConsoleKey.LeftArrow) cursor.MoveToPreviousWord(buffer.Lines);
                                    else if (keyInfo.Key == ConsoleKey.RightArrow) cursor.MoveToNextWord(buffer.Lines);
                                    else if (keyInfo.Key == ConsoleKey.UpArrow) cursor.Move(0, -1, buffer.Lines);
                                    else if (keyInfo.Key == ConsoleKey.DownArrow) cursor.Move(0, 1, buffer.Lines);
                                }
                                else
                                {
                                    if (keyInfo.Key == ConsoleKey.LeftArrow) cursor.Move(-1, 0, buffer.Lines);
                                    else if (keyInfo.Key == ConsoleKey.RightArrow) cursor.Move(1, 0, buffer.Lines);
                                    else if (keyInfo.Key == ConsoleKey.UpArrow) cursor.Move(0, -1, buffer.Lines);
                                    else if (keyInfo.Key == ConsoleKey.DownArrow) cursor.Move(0, 1, buffer.Lines);
                                }
                                break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    ShowStatus($"HATA: {ex.Message}");
                }
            }
            else
            {
                // Temel navigasyon ve düzenleme (Seçimi temizle)
                if (keyInfo.Key != ConsoleKey.None)
                {
                    cursor.ClearSelection();
                }

                switch (keyInfo.Key)
                {
                    case ConsoleKey.UpArrow:
                        if (buffer.Matches.Count > 0 && buffer.CurrentMatchIndex != -1)
                        {
                            buffer.CurrentMatchIndex = (buffer.CurrentMatchIndex - 1 + buffer.Matches.Count) % buffer.Matches.Count;
                            var m = buffer.Matches[buffer.CurrentMatchIndex];
                            cursor.Y = m.LineIndex;
                            cursor.X = m.ColumnIndex;
                        }
                        else
                        {
                            cursor.Move(0, -1, buffer.Lines);
                        }
                        break;
                    case ConsoleKey.DownArrow:
                        if (buffer.Matches.Count > 0 && buffer.CurrentMatchIndex != -1)
                        {
                            buffer.CurrentMatchIndex = (buffer.CurrentMatchIndex + 1) % buffer.Matches.Count;
                            var m = buffer.Matches[buffer.CurrentMatchIndex];
                            cursor.Y = m.LineIndex;
                            cursor.X = m.ColumnIndex;
                        }
                        else
                        {
                            cursor.Move(0, 1, buffer.Lines);
                        }
                        break;
                    case ConsoleKey.LeftArrow:
                        buffer.ClearSearch();
                        cursor.Move(-1, 0, buffer.Lines);
                        break;
                    case ConsoleKey.RightArrow:
                        buffer.ClearSearch();
                        cursor.Move(1, 0, buffer.Lines);
                        break;
                    case ConsoleKey.Home:
                        buffer.ClearSearch();
                        cursor.X = 0;
                        break;
                    case ConsoleKey.End:
                        buffer.ClearSearch();
                        cursor.X = buffer.Lines[cursor.Y].Length;
                        break;
                    case ConsoleKey.Enter:
                        buffer.ClearSearch();
                        buffer.NewLine(cursor.X, cursor.Y);
                        cursor.X = 0;
                        cursor.Y++;
                        break;
                    case ConsoleKey.Backspace:
                        buffer.ClearSearch();
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
                        buffer.ClearSearch();
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
                            buffer.ClearSearch();
                            int viewWidth = Console.WindowWidth - 7; // LineNumberWidth(4) + " | "(3)
                            if (cursor.X >= viewWidth)
                            {
                                buffer.NewLine(cursor.X, cursor.Y);
                                cursor.X = 0;
                                cursor.Y++;
                            }
                            buffer.InsertChar(cursor.X, cursor.Y, keyInfo.KeyChar);
                            cursor.X++;
                        }
                        break;
                }
            }

            renderer.Render(buffer, cursor);
        }
    }

    static string? ReadLineWithEsc()
    {
        StringBuilder sb = new StringBuilder();
        int startX = Console.CursorLeft;
        int startY = Console.CursorTop;

        while (true)
        {
            var key = Console.ReadKey(true);
            if (key.Key == ConsoleKey.Enter)
            {
                Console.WriteLine();
                return sb.ToString();
            }
            if (key.Key == ConsoleKey.Escape)
            {
                return null;
            }
            if (key.Key == ConsoleKey.Backspace)
            {
                if (sb.Length > 0)
                {
                    sb.Remove(sb.Length - 1, 1);
                    Console.SetCursorPosition(Console.CursorLeft - 1, Console.CursorTop);
                    Console.Write(" ");
                    Console.SetCursorPosition(Console.CursorLeft - 1, Console.CursorTop);
                }
            }
            else if (!char.IsControl(key.KeyChar))
            {
                sb.Append(key.KeyChar);
                Console.Write(key.KeyChar);
            }
        }
    }

    static void ShowStatus(string message)
    {
        Console.SetCursorPosition(0, Console.WindowHeight - 1);
        Console.Write(new string(' ', Console.WindowWidth - 1));
        Console.SetCursorPosition(0, Console.WindowHeight - 1);
        Console.Write(message + " (Bir tuşa basın)");
        Console.ReadKey(true);
    }
}
