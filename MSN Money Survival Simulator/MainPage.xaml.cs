using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

namespace MSN_Money_Survival_Simulator
{
    /// <summary>
    /// Application shell: persistent HUD + app bars, with a Frame that swaps
    /// dynamic pages in and out. The game timer lives here so it never resets
    /// when the user navigates.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private readonly DispatcherTimer tickTimer;
        private readonly DispatcherTimer toastTimer;
        private double toastRemaining;
        private bool initialNavigationDone;

        public MainPage()
        {
            InitializeComponent();
            DataContext = Game.Current;

            // IMPORTANT: do NOT navigate the nested Frame here. On Windows 8.1 the
            // Frame has no size yet in the constructor, which can leave the page
            // blank. ContentFrame_Loaded performs the first navigation safely.

            Game.Current.PropertyChanged += Game_PropertyChanged;

            tickTimer = new DispatcherTimer();
            tickTimer.Interval = TimeSpan.FromMilliseconds(50);
            tickTimer.Tick += TickTimer_Tick;
            tickTimer.Start();

            toastTimer = new DispatcherTimer();
            toastTimer.Interval = TimeSpan.FromMilliseconds(100);
            toastTimer.Tick += ToastTimer_Tick;
        }

        private void TickTimer_Tick(object sender, object e)
        {
            Game.Current.Tick(0.05);
            if (Game.Current.ShouldAutoSave) Game.Current.Save();
        }

        private void Game_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "WelcomeMessage")
                ShowToast(Game.Current.WelcomeMessage);
        }

        private void Nav_Click(object sender, RoutedEventArgs e)
        {
            AppBarButton button = sender as AppBarButton;
            if (button == null) return;
            string key = button.Tag as string;
            Type page = PageFor(key);
            if (page == null) return;
            if (ContentFrame.CurrentSourcePageType != page)
                NavigateSafely(page);
            UpdateNavHighlight(key);
        }

        /// <summary>
        /// The nested Frame is now in the visual tree with a real size, so this is
        /// the safe point to show the first page. A one-shot guard prevents a
        /// resume/reload from resetting the back stack.
        /// </summary>
        private void ContentFrame_Loaded(object sender, RoutedEventArgs e)
        {
            if (initialNavigationDone) return;
            initialNavigationDone = true;

            NavigateSafely(typeof(ArenaPage));
            UpdateNavHighlight("arena");

            // Belt-and-braces: if navigation silently produced no content, try
            // once more on the next layout pass so the user never sees a blank app.
            ContentFrame.LayoutUpdated += ContentFrame_LayoutUpdated;
        }

        private void ContentFrame_LayoutUpdated(object sender, object e)
        {
            if (ContentFrame.Content != null)
            {
                ContentFrame.LayoutUpdated -= ContentFrame_LayoutUpdated;
                return;
            }
            ContentFrame.LayoutUpdated -= ContentFrame_LayoutUpdated;
            NavigateSafely(typeof(ArenaPage));
            UpdateNavHighlight("arena");
        }

        private void ContentFrame_NavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            ShowNavigationError(e.SourcePageType != null ? e.SourcePageType.Name : "page",
                e.Exception != null ? e.Exception.Message : "unknown error");
        }

        private void NavigateSafely(Type page)
        {
            try
            {
                ContentFrame.Navigate(page);
                ContentFrame.UpdateLayout();
            }
            catch (Exception ex)
            {
                ShowNavigationError(page.Name, ex.Message);
            }
        }

        private async void ShowNavigationError(string pageName, string message)
        {
            try
            {
                var dialog = new Windows.UI.Popups.MessageDialog(
                    "Failed to load " + pageName + ": " + message, "Game error");
                dialog.Commands.Add(new Windows.UI.Popups.UICommand("OK"));
                await dialog.ShowAsync();
            }
            catch (Exception)
            {
                // Never let diagnostics break the game.
            }
        }

        private static Type PageFor(string key)
        {
            switch (key)
            {
                case "arena": return typeof(ArenaPage);
                case "shop": return typeof(shop);
                case "stats": return typeof(StatsPage);
                case "achievements": return typeof(AchievementsPage);
                case "settings": return typeof(SettingsPage);
                default: return null;
            }
        }

        private void UpdateNavHighlight(string key)
        {
            AppBarButton[] buttons = { NavArena, NavShop, NavStats, NavAchievements, NavSettings };
            string[] keys = { "arena", "shop", "stats", "achievements", "settings" };
            for (int i = 0; i < buttons.Length; i++)
                buttons[i].Opacity = keys[i] == key ? 1.0 : 0.55;
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            if (ContentFrame.CanGoBack)
            {
                ContentFrame.GoBack();
                Type current = ContentFrame.CurrentSourcePageType;
                UpdateNavHighlight(current == typeof(ArenaPage) ? "arena"
                    : current == typeof(shop) ? "shop"
                    : current == typeof(StatsPage) ? "stats"
                    : current == typeof(AchievementsPage) ? "achievements" : "settings");
            }
            else
            {
                ContentFrame.Navigate(typeof(ArenaPage));
                UpdateNavHighlight("arena");
            }
        }

        private void Sound_Click(object sender, RoutedEventArgs e)
        {
            Game.Current.SoundEnabled = !Game.Current.SoundEnabled;
            SoundButton.Label = Game.Current.SoundEnabled ? "SOUND ON" : "MUTED";
            SoundButton.Opacity = Game.Current.SoundEnabled ? 1.0 : 0.5;
        }

        private void ShowToast(string message)
        {
            if (string.IsNullOrWhiteSpace(message)) return;
            ToastText.Text = message;
            Toast.Visibility = Visibility.Visible;
            Toast.Opacity = 1;
            toastRemaining = 3200;
            if (!toastTimer.IsEnabled) toastTimer.Start();
        }

        private void ToastTimer_Tick(object sender, object e)
        {
            toastRemaining -= 100;
            if (toastRemaining <= 600)
                Toast.Opacity = Math.Max(0, toastRemaining / 600.0);
            if (toastRemaining > 0) return;

            toastTimer.Stop();
            Toast.Visibility = Visibility.Collapsed;
            Toast.Opacity = 0;
        }
    }
}
