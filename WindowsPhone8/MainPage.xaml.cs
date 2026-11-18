using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Microsoft.Phone.Shell;

namespace MSN_Money_Survival_Simulator
{
    /// <summary>
    /// Phone shell. The HUD and app bar stay put while lightweight UserControl
    /// views are swapped into ContentHost, so the timer never resets.
    /// </summary>
    public partial class MainPage : Microsoft.Phone.Controls.PhoneApplicationPage
    {
        private readonly DispatcherTimer tickTimer;
        private readonly DispatcherTimer toastTimer;
        private double toastRemaining;
        private string currentView;

        public MainPage()
        {
            InitializeComponent();
            DataContext = Game.Current;

            Game.Current.PropertyChanged += Game_PropertyChanged;
            ShowView("arena");

            tickTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(100) };
            tickTimer.Tick += TickTimer_Tick;
            tickTimer.Start();

            toastTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(100) };
            toastTimer.Tick += ToastTimer_Tick;
            UpdateSoundLabel();
        }

        private void TickTimer_Tick(object sender, EventArgs e)
        {
            Game.Current.Tick(0.1);
            if (Game.Current.ShouldAutoSave) Game.Current.Save();
        }

        private void Game_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "WelcomeMessage") ShowToast(Game.Current.WelcomeMessage);
        }

        private void Nav_Click(object sender, EventArgs e)
        {
            string key = null;
            ApplicationBarIconButton button = sender as ApplicationBarIconButton;
            if (button != null) key = KeyForText(button.Text);
            ApplicationBarMenuItem item = sender as ApplicationBarMenuItem;
            if (item != null) key = KeyForText(item.Text);
            if (key != null) ShowView(key);
        }

        private static string KeyForText(string text)
        {
            switch (text)
            {
                case "arena": return "arena";
                case "upgrades": return "shop";
                case "stats": return "stats";
                case "trophies": return "achievements";
                case "settings": return "settings";
                default: return null;
            }
        }

        private void ShowView(string key)
        {
            if (currentView == key) return;
            currentView = key;

            UserControl view;
            switch (key)
            {
                case "shop": view = new ShopView(); break;
                case "stats": view = new StatsView(); break;
                case "achievements": view = new AchievementsView(); break;
                case "settings": view = new SettingsView(); break;
                default: view = new ArenaView(); key = "arena"; break;
            }
            view.DataContext = Game.Current;
            ContentHost.Content = view;
        }

        private void Sound_Click(object sender, EventArgs e)
        {
            Game.Current.SoundEnabled = !Game.Current.SoundEnabled;
            UpdateSoundLabel();
        }

        private void UpdateSoundLabel()
        {
            if (MenuSound != null) MenuSound.Text = Game.Current.SoundEnabled ? "sound: on" : "sound: off";
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

        private void ToastTimer_Tick(object sender, EventArgs e)
        {
            toastRemaining -= 100;
            if (toastRemaining <= 600) Toast.Opacity = Math.Max(0, toastRemaining / 600.0);
            if (toastRemaining > 0) return;
            toastTimer.Stop();
            Toast.Visibility = Visibility.Collapsed;
            Toast.Opacity = 0;
        }

        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {
            tickTimer.Stop();
            Game.Current.Save();
            base.OnNavigatedFrom(e);
        }
    }
}
