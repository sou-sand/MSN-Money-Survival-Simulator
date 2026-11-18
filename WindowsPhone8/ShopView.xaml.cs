using System;
using System.Windows;
using System.Windows.Controls;

namespace MSN_Money_Survival_Simulator
{
    public sealed partial class ShopView : UserControl
    {
        public ShopView()
        {
            InitializeComponent();
        }

        private void Buy_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            Upgrade upgrade = button == null ? null : button.Tag as Upgrade;
            if (Game.Current.Buy(upgrade))
            {
                try
                {
                    Sfx.Source = new Uri("/Assets/sfx/buy.wav", UriKind.Relative);
                    Sfx.Play();
                }
                catch (Exception) { }
            }
            UpgradeList.SelectedItem = null;
        }
    }
}
