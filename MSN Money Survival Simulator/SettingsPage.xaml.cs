using System;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace MSN_Money_Survival_Simulator
{
    /// <summary>Settings, timeline reboot and destructive-reset confirmation.</summary>
    public sealed partial class SettingsPage : Page
    {
        public SettingsPage()
        {
            InitializeComponent();
            DataContext = Game.Current;
        }

        private void Ascend_Click(object sender, RoutedEventArgs e)
        {
            Game.Current.Ascend();
        }

        private async void Reset_Click(object sender, RoutedEventArgs e)
        {
            MessageDialog dialog = new MessageDialog(
                "This deletes all cash, upgrades, targets, trophies and timeline reboots. Start completely fresh?",
                "Reset MSN Money Survival Simulator?");
            dialog.Commands.Add(new UICommand("Reset", command => Game.Current.ResetAll()));
            dialog.Commands.Add(new UICommand("Cancel"));
            dialog.DefaultCommandIndex = 1;
            dialog.CancelCommandIndex = 1;
            await dialog.ShowAsync();
        }
    }
}
