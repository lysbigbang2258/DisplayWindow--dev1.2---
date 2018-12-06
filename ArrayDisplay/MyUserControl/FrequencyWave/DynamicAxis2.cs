using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace ArrayDisplay.MyUserControl.FrequencyWave {
    public class DynamicAxis2 {
        readonly double[] dSolutions =
        {1, 2, 2.5, 5, 10, 20, 25, 50, 100, 200, 250, 500, 1000, 2500, 5000, 10000};

        readonly List<Point> points = new List<Point>();
        Canvas canvasLine;
        Canvas canvasText;

        bool isE;
        int pixles;

        public Canvas CanvasLine { set { canvasLine = value; } }

        public Canvas CanvasText { set { canvasText = value; } }

        public int Pixles { set { pixles = value; } }

        public void Regulate(ref double d_min, ref double d_max, int i_axis_num) {
            int i;
            if (i_axis_num < 1 || d_max <= d_min) return;
            double dDelta = d_max - d_min;
            //if (dDelta < 1.0)
            //{
            //	dMax += (1.0 - dDelta) / 2.0;
            //	dMin -= (1.0 - dDelta) / 2.0;
            //}
            dDelta = d_max - d_min;
            int iExp = (int) (Math.Log10(dDelta) / Math.Log10(10.0)) - 3;
            double dMutiplier = Math.Pow(10, iExp);

            for (i = 0; i < dSolutions.Length; i++) {
                double dMultiCal = dMutiplier * dSolutions[i];
                if ((int) (dDelta / dMultiCal) <= i_axis_num) break;
            }
            if (i == dSolutions.Length) return;

            points.Clear();

            double dInterval = dMutiplier * dSolutions[i];
            double dStartPoint;

            double exp = Math.Log10(dInterval);
            if (exp <= -2 || exp >= 4) isE = true;
            else isE = false;

            if ((int) Math.Ceiling(d_min / dInterval) == d_min / dInterval) dStartPoint = d_min / dInterval * dInterval;
            else dStartPoint = ((int) Math.Ceiling(d_min / dInterval) - 1) * dInterval;

            int iAxisIndex;
            for (iAxisIndex = 0;; iAxisIndex++) {
                double temp = dStartPoint + dInterval * iAxisIndex;
                Point p = new Point();
                p.X = temp;
                p.Y = temp;
                points.Add(p);
                if (temp >= d_max) break;
            }
            double max = points[points.Count - 1].X;
            double min = points[0].X;
            int count = points.Count;
            points.Clear();
            for (iAxisIndex = 0; iAxisIndex < count; iAxisIndex++) {
                double temp = dStartPoint + dInterval * iAxisIndex;
                Point p = new Point();
                p.X = (temp - min) * pixles / (max - min);
                p.Y = temp;
                points.Add(p);
            }

            d_min = min;
            d_max = max;

            Draw();
        }

        public void Draw() {
            SolidColorBrush brush = new SolidColorBrush(Color.FromRgb(47, 233, 4));
            canvasLine.Children.Clear();
            canvasText.Children.Clear();

            for (int i = 0; i < points.Count; i++) {
                Line line = new Line();
                line.Stroke = brush;
                line.X1 = 0;
                line.Y1 = points[i].X; //canvasLine.ActualHeight - points[i].X;
                line.X2 = 10;
                line.Y2 = points[i].X; //canvasLine.ActualHeight - points[i].X;

                canvasLine.Children.Add(line);

                TextBlock tb = new TextBlock();
                tb.Width = 70;
                tb.Foreground = brush;
                tb.Background = null; //new SolidColorBrush(Colors.Wheat);

                float y = (float) points[i].Y;
                if (isE) tb.Text = y.ToString("0.0E+0");
                else tb.Text = y.ToString("F2");

                tb.TextAlignment = TextAlignment.Center;
                canvasText.Children.Add(tb);
                Canvas.SetBottom(tb, points[i].X);
                tb.Loaded += ytb_Loaded;
            }
        }

        void ytb_Loaded(object sender, RoutedEventArgs e) {
            TextBlock tb = sender as TextBlock;
            TranslateTransform ttf = new TranslateTransform();
            ttf.Y = tb.ActualHeight / 2;
            tb.RenderTransform = ttf;
        }
    }
}
