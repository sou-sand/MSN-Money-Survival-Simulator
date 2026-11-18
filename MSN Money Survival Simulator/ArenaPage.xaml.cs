using System;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;

namespace MSN_Money_Survival_Simulator
{
    /// <summary>The clicker arena: fast feedback, floating numbers, combo and target kills.</summary>
    public sealed partial class ArenaPage : Page
    {
        private readonly Random random = new Random();

        public ArenaPage()
        {
            InitializeComponent();
            DataContext = Game.Current;
            Game.Current.BossDefeated += OnBossDefeated;
        }

        private void ArenaPage_Loaded(object sender, RoutedEventArgs e)
        {
            // A Storyboard must not target elements before the page is in the tree.
            if (Game.Current.ReducedMotion) return;
            try
            {
                PulseStoryboard.Begin();
            }
            catch (Exception)
            {
                // Purely decorative; the game works without the pulse.
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            Game.Current.BossDefeated -= OnBossDefeated;
            base.OnNavigatedFrom(e);
        }

        private void Target_Click(object sender, RoutedEventArgs e)
        {
            double damage = Game.Current.Click();
            FloatDamage(damage, Game.Current.Combo >= 5);
            PlaySound(Game.Current.Combo >= 10 ? "combo" : "click");
        }

        private void FloatDamage(double amount, bool critical)
        {
            if (Game.Current.ReducedMotion) return;

            TextBlock label = new TextBlock();
            label.Text = "-" + Game.Format(amount);
            label.FontSize = critical ? 28 : 21;
            label.Foreground = new SolidColorBrush(critical ? Color.FromArgb(255, 255, 179, 0) : Colors.White);

            Point center = BossCenter();
            double left = center.X + random.Next(-55, 55);
            double top = center.Y + random.Next(-45, 35);
            Canvas.SetLeft(label, left);
            Canvas.SetTop(label, top);
            FxCanvas.Children.Add(label);

            Storyboard storyboard = new Storyboard();
            AddAnimation(storyboard, label, "(Canvas.Top)", top, top - 95, 780, 0);
            AddAnimation(storyboard, label, "(UIElement.Opacity)", 1, 0, 780, 0);
            storyboard.Completed += (s, e) => FxCanvas.Children.Remove(label);
            storyboard.Begin();
        }

        private Point BossCenter()
        {
            GeneralTransform transform = TargetButton.TransformToVisual(FxCanvas);
            return transform.TransformPoint(new Point(TargetButton.ActualWidth / 2, TargetButton.ActualHeight / 2));
        }

        private static void AddAnimation(Storyboard storyboard, DependencyObject target,
            string path, double from, double to, int durationMs, int delayMs)
        {
            DoubleAnimation animation = new DoubleAnimation();
            animation.From = from;
            animation.To = to;
            animation.Duration = TimeSpan.FromMilliseconds(durationMs);
            animation.BeginTime = TimeSpan.FromMilliseconds(delayMs);
            storyboard.Children.Add(animation);
            Storyboard.SetTarget(animation, target);
            Storyboard.SetTargetProperty(animation, path);
        }

        private void OnBossDefeated(object sender, double reward)
        {
            KillRewardText.Text = "+" + Game.Format(reward) + " collected";
            KillBanner.Visibility = Visibility.Visible;
            KillBanner.Opacity = 0;

            Storyboard storyboard = new Storyboard();
            AddAnimation(storyboard, KillBanner, "(UIElement.Opacity)", 0, 1, 200, 0);
            AddAnimation(storyboard, KillBanner, "(UIElement.Opacity)", 1, 0, 500, 1900);
            storyboard.Completed += (s, e) => KillBanner.Visibility = Visibility.Collapsed;
            storyboard.Begin();

            PlaySound("kill");
            SpawnConfetti();
        }

        private void SpawnConfetti()
        {
            if (Game.Current.ReducedMotion) return;
            Point center = BossCenter();
            Color[] colors = { Color.FromArgb(255, 0, 188, 212), Color.FromArgb(255, 255, 179, 0),
                               Color.FromArgb(255, 125, 255, 212), Color.FromArgb(255, 255, 82, 82) };

            for (int i = 0; i < 14; i++)
            {
                Ellipse piece = new Ellipse();
                piece.Width = random.Next(6, 12);
                piece.Height = piece.Width;
                piece.Fill = new SolidColorBrush(colors[i % colors.Length]);
                double left = center.X;
                double top = center.Y;
                Canvas.SetLeft(piece, left);
                Canvas.SetTop(piece, top);
                FxCanvas.Children.Add(piece);

                double dx = random.Next(-220, 220);
                double dy = random.Next(-200, 60);
                Storyboard sb = new Storyboard();
                AddAnimation(sb, piece, "(Canvas.Left)", left, left + dx, 900, 0);
                AddAnimation(sb, piece, "(Canvas.Top)", top, top + dy, 900, 0);
                AddAnimation(sb, piece, "(UIElement.Opacity)", 1, 0, 900, 0);
                sb.Completed += (s, e) => FxCanvas.Children.Remove(piece);
                sb.Begin();
            }
        }

        private void PlaySound(string name)
        {
            if (!Game.Current.SoundEnabled) return;
            try
            {
                Sfx.Source = new Uri("ms-appx:///Assets/sfx/" + name + ".wav", UriKind.Absolute);
                Sfx.Play();
            }
            catch (Exception)
            {
                // Audio is a bonus; never let it break a click.
            }
        }
    }
}
