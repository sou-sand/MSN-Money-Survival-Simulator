using System;
using System.ComponentModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace MSN_Money_Survival_Simulator
{
    /// <summary>Dynamic upgrade shop. Cards are generated from the shared game model.</summary>
    public sealed partial class shop : Page
    {
        public shop()
        {
            InitializeComponent();
            DataContext = Game.Current;
            Game.Current.PropertyChanged += OnGamePropertyChanged;
        }

        protected override void OnNavigatedFrom(Windows.UI.Xaml.Navigation.NavigationEventArgs e)
        {
            Game.Current.PropertyChanged -= OnGamePropertyChanged;
            base.OnNavigatedFrom(e);
        }

        private void OnGamePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != "WelcomeMessage") return;
            if (Game.Current.WelcomeMessage.StartsWith("Purchased", StringComparison.Ordinal))
                PlaySound();
        }

        private void PlaySound()
        {
            if (!Game.Current.SoundEnabled) return;
            try
            {
                Sfx.Source = new Uri("ms-appx:///Assets/sfx/buy.wav", UriKind.Absolute);
                Sfx.Play();
            }
            catch (Exception)
            {
            }
        }
    }
}
