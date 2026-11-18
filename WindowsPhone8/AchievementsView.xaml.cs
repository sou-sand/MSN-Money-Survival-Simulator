using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace MSN_Money_Survival_Simulator
{
    public sealed partial class AchievementsView : UserControl
    {
        public AchievementsView()
        {
            InitializeComponent();
            UpdateCount();
            Game.Current.AchievementUnlocked += AchievementUnlocked;
            Unloaded += (s, e) => Game.Current.AchievementUnlocked -= AchievementUnlocked;
        }

        private void AchievementUnlocked(object sender, Achievement e)
        {
            UpdateCount();
        }

        private void UpdateCount()
        {
            UnlockCount.Text = Game.Current.Achievements.Count(a => a.IsUnlocked) + " / " +
                               Game.Current.Achievements.Count;
        }
    }
}
