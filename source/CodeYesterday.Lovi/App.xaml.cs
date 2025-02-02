using CodeYesterday.Lovi.Services;

namespace CodeYesterday.Lovi
{
    public partial class App
    {
        private readonly IUserSettingsService<App> _userSettings;
        private readonly IViewManagerInternal _viewManager;

        public App(IUserSettingsService<App> userSettings, IViewManagerInternal viewManager)
        {
            _userSettings = userSettings;
            _viewManager = viewManager;
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            var window = new Window(new MainPage()) { Title = "Lovi" };

            if (_userSettings.TryGetValue("Window.X", out double x) &&
                _userSettings.TryGetValue("Window.Y", out double y) &&
                _userSettings.TryGetValue("Window.Width", out double width) &&
                _userSettings.TryGetValue("Window.Height", out double height))
            {
                window.X = x;
                window.Y = y;
                window.Width = width;
                window.Height = height;
            }

            window.Deactivated += WindowOnDeactivated;
            window.Destroying += WindowOnDestroying;
            window.SizeChanged += WindowOnSizeChanged;

            return window;
        }

        private void WindowOnDestroying(object? sender, EventArgs e)
        {
            _viewManager.OnAppExitAsync(CancellationToken.None);
            SavePosition(sender as Window);
        }

        private void WindowOnDeactivated(object? sender, EventArgs e)
        {
            SavePosition(sender as Window);
        }

        private void WindowOnSizeChanged(object? sender, EventArgs e)
        {
            SavePosition(sender as Window);
        }

        private void SavePosition(Window? window)
        {
            if (window is null) return;
            if (window.X < -30000 || window.Y < -30000) return; // Minimized

            _userSettings.SetValue("Window.X", window.X);
            _userSettings.SetValue("Window.Y", window.Y);
            _userSettings.SetValue("Window.Width", window.Width);
            _userSettings.SetValue("Window.Height", window.Height);
        }
    }
}
