using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;

namespace BlurMask;

internal sealed class SettingsWindow : Window
{
    private readonly DrawingSession _session;
    private readonly Dictionary<ShortcutAction, Button> _buttons = [];
    private readonly ComboBox _language;
    private readonly ComboBox _aiFont;
    private readonly CheckBox _enabled;
    private readonly TextBlock _message = new() { TextWrapping = TextWrapping.Wrap, FontSize = 12, Foreground = Brushes.Firebrick };
    private readonly TabControl _tabs;
    private AppSettings _draft;
    private ShortcutAction? _capturing;
    private static readonly string[] Languages = ["system", "en", "fr", "de", "ru"];
    internal AppSettings Draft => _draft;

    internal SettingsWindow(DrawingSession session, IReadOnlyList<ShortcutAction> unavailable)
    {
        _session = session; _draft = session.Settings.CloneSettings();
        Title = "BlurMask · " + UiText.T("Настройки");
        Width = 610; Height = 690; MinWidth = 530; MinHeight = 480;
        Topmost = true; ShowInTaskbar = false; CanMinimize = false; CanMaximize = false;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
        RequestedThemeVariant = ThemeVariant.Light;
        Background = new SolidColorBrush(Color.Parse("#FAFAF7"));
        var root = new DockPanel { Margin = new Thickness(24), LastChildFill = true };
        var heading = new TextBlock { Text = UiText.T("Настройки"), FontSize = 25, FontWeight = FontWeight.SemiBold, Margin = new Thickness(0, 0, 0, 18) };
        DockPanel.SetDock(heading, Dock.Top); root.Children.Add(heading);
        var footer = new StackPanel { Spacing = 10, Margin = new Thickness(0, 16, 0, 0) };
        footer.Children.Add(_message);
        var actions = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8, HorizontalAlignment = HorizontalAlignment.Right };
        var cancel = new Button { Content = UiText.T("Отмена"), MinWidth = 90 };
        var save = new Button { Content = UiText.T("Сохранить"), MinWidth = 110, Name = "SaveSettings" };
        cancel.Click += (_, _) => Close(); save.Click += (_, _) => Save();
        actions.Children.Add(cancel); actions.Children.Add(save); footer.Children.Add(actions);
        DockPanel.SetDock(footer, Dock.Bottom); root.Children.Add(footer);

        var general = new StackPanel { Spacing = 14, Margin = new Thickness(0, 20, 0, 0) };
        general.Children.Add(new TextBlock { Text = UiText.T("Язык интерфейса"), FontWeight = FontWeight.SemiBold });
        _language = new ComboBox { ItemsSource = new[] { UiText.T("Как в системе"), "English", "Français", "Deutsch", "Русский" },
            SelectedIndex = Math.Max(0, Array.IndexOf(Languages, _draft.Language)), HorizontalAlignment = HorizontalAlignment.Stretch };
        general.Children.Add(_language);
        general.Children.Add(new TextBlock { Text = UiText.T("Изменение языка вступит в силу после перезапуска."), FontSize = 12, Foreground = Brushes.Gray, TextWrapping = TextWrapping.Wrap });
        general.Children.Add(new TextBlock { Text = UiText.T("Шрифт AI-пера"), FontWeight = FontWeight.SemiBold });
        _aiFont = new ComboBox { ItemsSource = InkFonts.Names.Select(name => new TextBlock { Text = name + " · Aa Бб 123", FontFamily = InkFonts.Face(name).FontFamily, FontSize = 17 }).ToArray(),
            SelectedIndex = Math.Max(0, Array.IndexOf(InkFonts.Names, _draft.AiFont)), HorizontalAlignment = HorizontalAlignment.Stretch };
        general.Children.Add(_aiFont);
        _enabled = new CheckBox { Content = UiText.T("Включить горячие клавиши"), IsChecked = _draft.HotkeysEnabled, Margin = new Thickness(0, 10, 0, 0) };
        general.Children.Add(_enabled);
        general.Children.Add(new TextBlock { Text = UiText.T(OperatingSystem.IsWindows() ? "На Windows клавиши работают поверх других приложений." : "На этой платформе клавиши работают, когда окно BlurMask активно."),
            FontSize = 12, Foreground = Brushes.Gray, TextWrapping = TextWrapping.Wrap });
        if (unavailable.Count > 0)
            general.Children.Add(new TextBlock { Text = UiText.T("Некоторые сочетания заняты другим приложением:") + "\n" +
                string.Join(", ", unavailable.Select(action => session.ShortcutText(action))), Foreground = Brushes.DarkOrange, FontSize = 12, TextWrapping = TextWrapping.Wrap });

        var shortcuts = new StackPanel { Spacing = 7, Margin = new Thickness(0, 16, 0, 0) };
        shortcuts.Children.Add(new TextBlock { Text = UiText.T("Нажмите сочетание справа и введите новое. Backspace — отключить, Esc — отменить."),
            FontSize = 12, Foreground = Brushes.Gray, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 0, 0, 9) });
        var reset = new Button { Content = UiText.T("Сбросить клавиши как в Epic Pen"), HorizontalAlignment = HorizontalAlignment.Left, Margin = new Thickness(0, 0, 0, 10) };
        reset.Click += (_, _) => { EndCapture(); _draft = _draft with { Shortcuts = AppSettings.DefaultShortcuts() }; _enabled.IsChecked = true; RefreshButtons(); _message.Text = ""; };
        shortcuts.Children.Add(reset);
        foreach (var action in Enum.GetValues<ShortcutAction>())
        {
            var row = new Grid { ColumnDefinitions = new ColumnDefinitions("*,190") };
            row.Children.Add(new TextBlock { Text = AppSettings.Label(action), FontSize = 13, VerticalAlignment = VerticalAlignment.Center, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 0, 12, 0) });
            var button = new Button { Content = _draft.Shortcuts[action].ToString(), Name = "Shortcut" + action, HorizontalAlignment = HorizontalAlignment.Stretch,
                HorizontalContentAlignment = HorizontalAlignment.Center, FontSize = 12, MinHeight = 33 };
            button.Click += (_, _) => BeginCapture(action);
            Grid.SetColumn(button, 1); row.Children.Add(button); _buttons[action] = button; shortcuts.Children.Add(row);
        }
        _tabs = new TabControl { ItemsSource = new[]
        {
            new TabItem { Header = UiText.T("Общие"), Content = new ScrollViewer { Content = general } },
            new TabItem { Header = UiText.T("Горячие клавиши"), Content = new ScrollViewer { Content = shortcuts, HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled } }
        } };
        _tabs.SelectionChanged += (_, _) => EndCapture();
        root.Children.Add(_tabs); Content = root;
        AddHandler(KeyDownEvent, CaptureKey, RoutingStrategies.Tunnel);
    }
    internal void ShowKeyboardTab() => _tabs.SelectedIndex = 1;
    private void BeginCapture(ShortcutAction action)
    {
        EndCapture(); _capturing = action; _message.Text = "";
        _buttons[action].Content = UiText.T("Нажмите клавиши…"); _buttons[action].Focus();
    }
    private void EndCapture() { _capturing = null; _message.Text = ""; RefreshButtons(); }
    private void RefreshButtons() { foreach (var (action, button) in _buttons) button.Content = _draft.Shortcuts[action].ToString(); }
    private void CaptureKey(object? sender, KeyEventArgs e)
    {
        if (_capturing is not { } action) { if (e.Key == Key.Escape) { Close(); e.Handled = true; } return; }
        e.Handled = true;
        if (e.Key == Key.Escape) { EndCapture(); return; }
        if (e.Key is Key.LeftCtrl or Key.RightCtrl or Key.LeftShift or Key.RightShift or Key.LeftAlt or Key.RightAlt or Key.LWin or Key.RWin) return;
        var binding = e.Key is Key.Back or Key.Delete && e.KeyModifiers == KeyModifiers.None ? ShortcutBinding.None : new ShortcutBinding(e.Key, e.KeyModifiers);
        if (!binding.Valid) { _message.Text = UiText.T("Добавьте Ctrl, Alt или Meta, либо используйте F1–F24."); return; }
        if (binding.Enabled && _draft.Shortcuts.Any(pair => pair.Key != action && pair.Value == binding))
        { _message.Text = UiText.T("Сочетание уже назначено другой команде"); return; }
        _draft.Shortcuts[action] = binding; _message.Text = ""; EndCapture();
    }
    internal bool Save()
    {
        EndCapture();
        _draft = _draft with { Language = Languages[Math.Clamp(_language.SelectedIndex, 0, Languages.Length - 1)], HotkeysEnabled = _enabled.IsChecked == true,
            AiFont = InkFonts.Names[Math.Clamp(_aiFont.SelectedIndex, 0, InkFonts.Names.Length - 1)] };
        if (_draft.Validate() is { } error) { _message.Text = error; return false; }
        try { _session.SaveSettings(_draft); Close(); return true; }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidOperationException)
        { _message.Text = UiText.T("Не удалось сохранить настройки") + ": " + ex.Message; return false; }
    }
}
