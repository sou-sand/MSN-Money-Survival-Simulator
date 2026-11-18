using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace MSN_Money_Survival_Simulator
{
    /// <summary>Phone click arena: touch feedback, combo, floating numbers and kills.</summary>
    public sealed partial class ArenaView : UserControl
    {
        private readonly Random random = new Random();
        private Storyboard pulse;

        public ArenaView()
        {
            InitializeComponent();
            Game.Current.BossDefeated += OnBossDefeated;
            Unloaded += ArenaView_Unloaded;
        }

        private void ArenaView_Unloaded(object sender, RoutedEventArgs e)
        {
            Game.Current.BossDefeated -= OnBossDefeated;
            if (pulse != null) pulse.Stop();
        }

        private void ArenaView_Loaded(object sender, RoutedEventArgs e)
        {
            if (Game.Current.ReducedMotion) return;
            try
            {
                DoubleAnimation animation = new DoubleAnimation
                {
                    From = 0.04,
                    To = 0.18,
                    Duration = TimeSpan.FromSeconds(1.6),
                    AutoReverse = true,
                    RepeatBehavior = RepeatBehavior.Forever
                };
                Storyboard.SetTarget(animation, PulseEllipse);
                Storyboard.SetTargetProperty(animation, new PropertyPath("Opacity"));
                pulse = new Storyboard();
                pulse.Children.Add(animation);
                pulse.Begin();
            }
            catch (Exception)
            {
                // Decorative only.
            }
        }

        private void Target_Click(object sender, RoutedEventArgs e)
        {
            double damage = Game.Current.Click();
            FloatDamage(damage, Game.Current.Combo >= 5);
            PlaySound(Game.Current.Combo >= 10 ? "combo" : "click");
        }

        private Point BossCenter()
        {
            GeneralTransform transform = TargetButton.TransformToVisual(FxCanvas);
            return transform.Transform(new Point(TargetButton.ActualWidth / 2, TargetButton.ActualHeight / 2));
        }

        private void FloatDamage(double amount, bool critical)
        {
            if (Game.Current.ReducedMotion) return;
            TextBlock label = new TextBlock
            {
                Text = "-" + Game.Format(amount),
                FontSize = critical ? 26 : 20,
                Foreground = new SolidColorBrush(critical
                    ? Color.FromArgb(255, 255, 179, 0) : Colors.White)
            };

            Point center = BossCenter();
            double left = center.X + random.Next(-45, 45);
            double top = center.Y + random.Next(-40, 30);
            Canvas.SetLeft(label, left);
            Canvas.SetTop(label, top);
            FxCanvas.Children.Add(label);

            Storyboard sb = new Storyboard();
            Animate(sb, label, "(Canvas.Top)", top, top - 80, 750, 0);
            Animate(sb, label, "Opacity", 1, 0, 750, 0);
            sb.Completed += (s, e) => FxCanvas.Children.Remove(label);
            sb.Begin();
        }

        private void OnBossDefeated(object sender, double reward)
        {
            KillRewardText.Text = "+" + Game.Format(reward) + " collected";
            KillBanner.Visibility = Visibility.Visible;

            Storyboard sb = new Storyboard();
            Animate(sb, KillBanner, "Opacity", 0, 1, 200, 0);
            Animate(sb, KillBanner, "Opacity", 1, 0, 500, 1900);
            sb.Completed += (s, e) => KillBanner.Visibility = Visibility.Collapsed;
            sb.Begin();

            PlaySound("kill");
            SpawnConfetti();
        }

        private void SpawnConfetti()
        {
            if (Game.Current.ReducedMotion) return;
            Point center = BossCenter();
            Color[] colors =
            {
                Color.FromArgb(255, 0, 188, 212), Color.FromArgb(255, 255, 179, 0),
                Color.FromArgb(255, 125, 255, 212), Color.FromArgb(255, 255, 82, 82)
            };

            for (int i = 0; i < 12; i++)
            {
                Ellipse piece = new Ellipse
                {
                    Width = random.Next(5, 10),
                    Height = 8,
                    Fill = new SolidColorBrush(colors[i % colors.Length])
                };
                double left = center.X;
                double top = center.Y;
                Canvas.SetLeft(piece, left);
                Canvas.SetTop(piece, top);
                FxCanvas.Children.Add(piece);

                double dx = random.Next(-160, 160);
                double dy = random.Next(-170, 40);
                Storyboard sb = new Storyboard();
                Animate(sb, piece, "(Canvas.Left)", left, left + dx, 850, 0);
                Animate(sb, piece, "(Canvas.Top)", top, top + dy, 850, 0);
                Animate(sb, piece, "Opacity", 1, 0, 850, 0);
                sb.Completed += (s, e) => FxCanvas.Children.Remove(piece);
                sb.Begin();
            }
        }

        private static void Animate(Storyboard sb, DependencyObject target, string path,
            double from, double to, int durationMs, int delayMs)
        {
            DoubleAnimation animation = new DoubleAnimation
            {
                From = from,
                To = to,
                Duration = TimeSpan.FromMilliseconds(durationMs),
                BeginTime = TimeSpan.FromMilliseconds(delayMs)
            };
            Storyboard.SetTarget(animation, target);
            Storyboard.SetTargetProperty(animation, new PropertyPath(path));
            sb.Children.Add(animation);
        }

        private void PlaySound(string name)
        {
            if (!Game.Current.SoundEnabled) return;
            try
            {
                Sfx.Source = new Uri("/Assets/sfx/" + name + ".wav", UriKind.Relative);
                Sfx.Play();
            }
            catch (Exception)
            {
                // Audio is optional.
            }
        }
    }
}
