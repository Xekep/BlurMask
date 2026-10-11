using System.Text.Json;
using System.Text.Json.Serialization;
using Avalonia.Input;

namespace BlurMask;

internal enum ShortcutAction
{
    ToggleInk, Cursor, Pen, Highlighter, Eraser, Undo, Clear, Screenshot,
    IncreaseSize, DecreaseSize, Line, Ellipse, Rectangle, Arrow, Text,
    Whiteboard, Blackboard, LastColor, ToggleToolbar, ToggleFading,
    QuickColor1, QuickColor2, QuickColor3, QuickColor4, QuickColor5, QuickColor6,
    Redo, Canvas, Screen
}

internal sealed record ShortcutBinding(Key Key, KeyModifiers Modifiers)
{
    internal static ShortcutBinding None => new(Key.None, KeyModifiers.None);
    internal bool Enabled => Key != Key.None;
    internal bool Valid => !Enabled || (VirtualKey != 0 &&
        (Modifiers & ~(KeyModifiers.Control | KeyModifiers.Shift | KeyModifiers.Alt | KeyModifiers.Meta)) == 0 &&
        (Modifiers.HasFlag(KeyModifiers.Control) || Modifiers.HasFlag(KeyModifiers.Alt) || Modifiers.HasFlag(KeyModifiers.Meta) || Key is >= Key.F1 and <= Key.F24));
    internal uint VirtualKey => Key switch
    {
        >= Key.A and <= Key.Z => (uint)(0x41 + Key - Key.A),
        >= Key.D0 and <= Key.D9 => (uint)(0x30 + Key - Key.D0),
        >= Key.NumPad0 and <= Key.NumPad9 => (uint)(0x60 + Key - Key.NumPad0),
        >= Key.F1 and <= Key.F24 => (uint)(0x70 + Key - Key.F1),
        Key.PrintScreen => 0x2C, Key.OemOpenBrackets => 0xDB, Key.OemCloseBrackets => 0xDD,
        Key.OemPlus => 0xBB, Key.OemMinus => 0xBD, Key.OemComma => 0xBC, Key.OemPeriod => 0xBE,
        Key.OemSemicolon => 0xBA, Key.OemQuestion => 0xBF, Key.OemTilde => 0xC0, Key.OemPipe => 0xDC, Key.OemQuotes => 0xDE,
        Key.Space => 0x20, Key.Tab => 0x09, Key.Enter => 0x0D, Key.Back => 0x08, Key.Delete => 0x2E,
        Key.Insert => 0x2D, Key.Home => 0x24, Key.End => 0x23, Key.PageUp => 0x21, Key.PageDown => 0x22,
        Key.Left => 0x25, Key.Up => 0x26, Key.Right => 0x27, Key.Down => 0x28,
        _ => 0
    };
    internal uint NativeModifiers => 0x4000u | (Modifiers.HasFlag(KeyModifiers.Control) ? 2u : 0) |
        (Modifiers.HasFlag(KeyModifiers.Shift) ? 4u : 0) | (Modifiers.HasFlag(KeyModifiers.Alt) ? 1u : 0) |
        (Modifiers.HasFlag(KeyModifiers.Meta) ? 8u : 0);
    public override string ToString()
    {
        if (!Enabled) return UiText.T("Не назначено");
        var key = Key switch
        {
            >= Key.D0 and <= Key.D9 => ((int)Key - (int)Key.D0).ToString(),
            Key.OemOpenBrackets => "[", Key.OemCloseBrackets => "]", Key.OemPlus => "+", Key.OemMinus => "-", _ => Key.ToString()
        };
        return (Modifiers.HasFlag(KeyModifiers.Control) ? "Ctrl+" : "") + (Modifiers.HasFlag(KeyModifiers.Alt) ? "Alt+" : "") +
            (Modifiers.HasFlag(KeyModifiers.Shift) ? "Shift+" : "") + (Modifiers.HasFlag(KeyModifiers.Meta) ? "Meta+" : "") + key;
    }
}

internal sealed record AppSettings
{
    public string LinuxDisplayBackend { get; init; } = "auto";
    public string Language { get; init; } = "system";
    public string AiMode { get; init; } = "text";
    public bool HotkeysEnabled { get; init; } = true;
    public Dictionary<ShortcutAction, ShortcutBinding> Shortcuts { get; init; } = DefaultShortcuts();
    internal AppSettings CloneSettings() => this with { Shortcuts = new(Shortcuts) };
    internal static Dictionary<ShortcutAction, ShortcutBinding> DefaultShortcuts()
    {
        const KeyModifiers mods = KeyModifiers.Control | KeyModifiers.Shift;
        Dictionary<ShortcutAction, ShortcutBinding> result = new();
        foreach (var (action, key) in new[]
        {
            (ShortcutAction.ToggleInk, Key.D1), (ShortcutAction.Cursor, Key.D2), (ShortcutAction.Pen, Key.D3),
            (ShortcutAction.Highlighter, Key.D4), (ShortcutAction.Eraser, Key.D5), (ShortcutAction.Undo, Key.D6),
            (ShortcutAction.Clear, Key.D7), (ShortcutAction.Screenshot, Key.PrintScreen),
            (ShortcutAction.IncreaseSize, Key.OemCloseBrackets), (ShortcutAction.DecreaseSize, Key.OemOpenBrackets),
            (ShortcutAction.Line, Key.L), (ShortcutAction.Ellipse, Key.E), (ShortcutAction.Rectangle, Key.R),
            (ShortcutAction.Arrow, Key.A), (ShortcutAction.Text, Key.T), (ShortcutAction.Whiteboard, Key.W),
            (ShortcutAction.Blackboard, Key.B), (ShortcutAction.LastColor, Key.D9), (ShortcutAction.ToggleToolbar, Key.D0),
            (ShortcutAction.ToggleFading, Key.F), (ShortcutAction.Redo, Key.Y), (ShortcutAction.Canvas, Key.C), (ShortcutAction.Screen, Key.D)
        }) result[action] = new(key, mods);
        for (var i = 0; i < 6; i++) result[ShortcutAction.QuickColor1 + i] = new(Key.D1 + i, KeyModifiers.Alt | KeyModifiers.Shift);
        return result;
    }
    internal static string Label(ShortcutAction action) => UiText.T(action switch
    {
        ShortcutAction.ToggleInk => "Скрыть / показать рисунок", ShortcutAction.Cursor => "Обычный курсор", ShortcutAction.Pen => "Перо",
        ShortcutAction.Highlighter => "Маркер", ShortcutAction.Eraser => "Ластик", ShortcutAction.Undo => "Отменить", ShortcutAction.Clear => "Очистить",
        ShortcutAction.Screenshot => "Снимок экрана", ShortcutAction.IncreaseSize => "Увеличить толщину", ShortcutAction.DecreaseSize => "Уменьшить толщину",
        ShortcutAction.Line => "Линия", ShortcutAction.Ellipse => "Эллипс", ShortcutAction.Rectangle => "Прямоугольник", ShortcutAction.Arrow => "Стрелка",
        ShortcutAction.Text => "Текст", ShortcutAction.Whiteboard => "Белая доска", ShortcutAction.Blackboard => "Чёрная доска",
        ShortcutAction.LastColor => "Предыдущий цвет", ShortcutAction.ToggleToolbar => "Скрыть / показать панель", ShortcutAction.ToggleFading => "Исчезающие чернила",
        ShortcutAction.Redo => "Повторить", ShortcutAction.Canvas => "Отдельное полотно", ShortcutAction.Screen => "Рисовать на экране",
        _ => "Быстрый цвет"
    }) + (action is >= ShortcutAction.QuickColor1 and <= ShortcutAction.QuickColor6 ? $" {(int)action - (int)ShortcutAction.QuickColor1 + 1}" : "");
    internal string? Validate()
    {
        if (LinuxDisplayBackend is not ("auto" or "x11" or "wayland") || Language is not ("system" or "en" or "fr" or "de" or "ru") || AiMode is not ("text" or "math") || Shortcuts is null) return UiText.T("Некорректные настройки");
        var used = new HashSet<ShortcutBinding>();
        foreach (var action in Enum.GetValues<ShortcutAction>())
        {
            if (!Shortcuts.TryGetValue(action, out var binding) || binding is null || !binding.Valid) return UiText.T("Некорректное сочетание клавиш");
            if (binding.Enabled && !used.Add(binding)) return UiText.T("Сочетание уже назначено другой команде");
        }
        return null;
    }
}

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, WriteIndented = true)]
[JsonSerializable(typeof(AppSettings))]
internal partial class SettingsJsonContext : JsonSerializerContext;

internal static class SettingsStore
{
    internal static string PathName => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData, Environment.SpecialFolderOption.DoNotVerify), "BlurMask", "settings.json");
    internal static AppSettings Load(string? path = null)
    {
        try
        {
            var settings = JsonSerializer.Deserialize(File.ReadAllText(path ?? PathName), SettingsJsonContext.Default.AppSettings);
            if (settings is null || settings.Shortcuts is null) return new();
            foreach (var (action, binding) in AppSettings.DefaultShortcuts()) settings.Shortcuts.TryAdd(action, binding);
            return settings.Validate() is null ? settings : new();
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException or NotSupportedException) { return new(); }
    }
    internal static void Save(AppSettings settings, string? path = null)
    {
        if (settings.Validate() is { } error) throw new InvalidOperationException(error);
        var destination = path ?? PathName;
        Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
        var temporary = destination + ".tmp";
        File.WriteAllText(temporary, JsonSerializer.Serialize(settings, SettingsJsonContext.Default.AppSettings));
        File.Move(temporary, destination, true);
    }
}
