using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace MSN_Money_Survival_Simulator
{
    /// <summary>Trophy case, populated from the shared achievement collection.</summary>
    public sealed partial class AchievementsPage : Page
    {
        public AchievementsPage()
        {
            InitializeComponent();
            DataContext = Game.Current;
            UpdateCount();
            Game.Current.AchievementUnlocked += OnAchievementUnlocked;
        }

        protected override void OnNavigatedFrom(Windows.UI.Xaml.Navigation.NavigationEventArgs e)
        {
            Game.Current.AchievementUnlocked -= OnAchievementUnlocked;
            base.OnNavigatedFrom(e);
        }

        private void OnAchievementUnlocked(object sender, Achievement e)
        {
            UpdateCount();
            if (!Game.Current.SoundEnabled) return;
            try
            {
                Sfx.Source = new Uri("ms-appx:///Assets/sfx/unlock.wav", UriKind.Absolute);
                Sfx.Play();
            }
            catch (Exception)
            {
            }
        }

        private void UpdateCount()
        {
            int total = Game.Current.Achievements.Count;
            int unlocked = 0;
            foreach (var a in Game.Current.Achievements) if (a.IsUnlocked) unlocked++;
            UnlockCount.Text = unlocked + " / " + total;
        }
    }
}
