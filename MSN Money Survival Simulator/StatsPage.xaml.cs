using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

namespace MSN_Money_Survival_Simulator
{
    /// <summary>Live statistics with a hand-drawn XAML cash-flow sparkline.</summary>
    public sealed partial class StatsPage : Page
    {
        private readonly List<double> history = new List<double>();
        private readonly DispatcherTimer sampleTimer;

        public StatsPage()
        {
            InitializeComponent();
            DataContext = Game.Current;
            history.Add(Game.Current.Cash);

            sampleTimer = new DispatcherTimer();
            sampleTimer.Interval = TimeSpan.FromSeconds(1);
            sampleTimer.Tick += SampleTimer_Tick;
            sampleTimer.Start();
        }

        protected override void OnNavigatedFrom(Windows.UI.Xaml.Navigation.NavigationEventArgs e)
        {
            sampleTimer.Stop();
            base.OnNavigatedFrom(e);
        }

        private void SampleTimer_Tick(object sender, object e)
        {
            history.Add(Game.Current.Cash);
            if (history.Count > 60) history.RemoveAt(0);
            DrawChart();
        }

        private void ChartCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            DrawChart();
        }

        private void DrawChart()
        {
            ChartCanvas.Children.Clear();
            double width = ChartCanvas.ActualWidth;
            double height = ChartCanvas.ActualHeight;
            if (width < 20 || height < 20 || history.Count < 2) return;

            // Horizontal guide lines.
            for (int i = 1; i <= 3; i++)
            {
                double y = height * i / 4.0;
                Line guide = new Line();
                guide.X1 = 0;
                guide.Y1 = y;
                guide.X2 = width;
                guide.Y2 = y;
                guide.Stroke = new SolidColorBrush(Color.FromArgb(255, 38, 52, 62));
                guide.StrokeThickness = 1;
                ChartCanvas.Children.Add(guide);
            }

            double min = history.Min();
            double max = history.Max();
            double span = Math.Max(1, max - min);
            PointCollection points = new PointCollection();
            for (int i = 0; i < history.Count; i++)
            {
                double x = (double)i / (history.Count - 1) * (width - 16) + 8;
                double y = height - 14 - ((history[i] - min) / span) * (height - 28);
                points.Add(new Point(x, y));
            }

            Polyline curve = new Polyline();
            curve.Points = points;
            curve.Stroke = new SolidColorBrush(Color.FromArgb(255, 0, 188, 212));
            curve.StrokeThickness = 2.5;
            ChartCanvas.Children.Add(curve);

            Point last = points[points.Count - 1];
            Ellipse head = new Ellipse();
            head.Width = 8;
            head.Height = 8;
            head.Fill = new SolidColorBrush(Color.FromArgb(255, 255, 179, 0));
            Canvas.SetLeft(head, last.X - 4);
            Canvas.SetTop(head, last.Y - 4);
            ChartCanvas.Children.Add(head);
        }
    }
}
