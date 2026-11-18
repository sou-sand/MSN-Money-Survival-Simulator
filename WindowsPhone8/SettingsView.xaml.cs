using System.Windows;
using System.Windows.Controls;

namespace MSN_Money_Survival_Simulator
{
    public sealed partial class SettingsView : UserControl
    {
        public SettingsView()
        {
            InitializeComponent();
        }

        private void Ascend_Click(object sender, RoutedEventArgs e)
        {
            if (!Game.Current.CanAscend) return;
            Game.Current.Ascend();
            MessageBox.Show("Timeline rewritten. All future damage x" +
                Game.Current.PrestigeMultiplier.ToString("0.##") + ".", "New era", MessageBoxButton.OK);
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show(
                "Erase every trace of the campaign? This cannot be undone.",
                "Reset everything", MessageBoxButton.OKCancel);
            if (result == MessageBoxResult.OK) Game.Current.ResetAll();
        }
    }
}
