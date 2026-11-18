using System;
using System.Windows;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace MSN_Money_Survival_Simulator
{
    public partial class App : Application
    {
        public PhoneApplicationFrame RootFrame { get; private set; }
        private bool phoneApplicationInitialized;

        public App()
        {
            UnhandledException += Application_UnhandledException;
            InitializeComponent();
            InitializePhoneApplication();
        }

        private void InitializePhoneApplication()
        {
            if (phoneApplicationInitialized) return;

            RootFrame = new PhoneApplicationFrame();
            RootFrame.Navigated += CompleteInitializePhoneApplication;
            RootFrame.NavigationFailed += RootFrame_NavigationFailed;
            phoneApplicationInitialized = true;

            RootVisual = RootFrame;
            RootFrame.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
        }

        private void CompleteInitializePhoneApplication(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            if (RootVisual != RootFrame) RootVisual = RootFrame;
            RootFrame.Navigated -= CompleteInitializePhoneApplication;
        }

        private void RootFrame_NavigationFailed(object sender, System.Windows.Navigation.NavigationFailedEventArgs e)
        {
            // Surface a real failure rather than a black screen.
            MessageBox.Show("Failed to load " + e.Uri + ": " +
                (e.Exception == null ? "unknown error" : e.Exception.Message), "Game error", MessageBoxButton.OK);
            e.Handled = true;
        }

        private void Application_Launching(object sender, LaunchingEventArgs e) { }
        private void Application_Activated(object sender, ActivatedEventArgs e) { }

        private void Application_Deactivated(object sender, DeactivatedEventArgs e)
        {
            Game.Current.Save();
        }

        private void Application_Closing(object sender, ClosingEventArgs e)
        {
            Game.Current.Save();
        }

        private void Application_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            if (System.Diagnostics.Debugger.IsAttached) return;
            MessageBox.Show("Something went wrong: " + e.ExceptionObject.Message, "Game error", MessageBoxButton.OK);
            e.Handled = true;
        }
    }
}
