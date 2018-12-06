using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace ArrayDisplay.MyUserControl.FrequencyWave {
    public class CordAxis {
        static readonly float[] rulerValue = {
            0, 0.1f, 0.2f, 0.4f, 0.5f, 1, 2, 4, 5, 6, 8, 10, 20, 40, 50, 60, 80, 100, 150, 300, 500, 1000, 10000
        };

        readonly List<Point> points = new List<Point>();

        Canvas canvasLine;
        Canvas canvasText;
        float fmax;

        float fmin;
        int num;
        int span;

        public float FMin { set { fmin = value; } }

        public float FMax { set { fmax = value; } }

        public int Num { set { num = value; } }

        public int Span { set { span = value; } }

        public Canvas CanvasLine { set { canvasLine = value; } }

        public Canvas CanvasText { set { canvasText = value; } }

        public void DrawAxis(ref float min, ref float max) {
            int j = 0;
            int i = 0;

            //if (Math.Abs(fmax) > Math.Abs(fmin))
            //{
            //	fmin = -fmax;
            //}
            //else
            //{
            //	fmax = Math.Abs(fmin);
            //}

            for (j = 0; j < rulerValue.Length; j++) if ((rulerValue[j] * num) >= Math.Abs(fmax - fmin)) break;
            //if((num/2*RulerValue[j])> Math.Abs(fmax))
            //	break;

            if (j == rulerValue.Length) return;

            if (fmin < 0) {
                i = 0;
                while (i < num) {
                    if ((-i * rulerValue[j]) <= fmin) break;
                    i++;
                }
                min = -i * rulerValue[j];
            }
            else {
                i = (int) (fmin / rulerValue[j]);
                while (true) {
                    if ((i * rulerValue[j]) <= fmin && (i * rulerValue[j] + rulerValue[j]) > fmin) break;
                    i++;
                }
                min = i * rulerValue[j];
            }

            points.Clear();
            max = min + num * rulerValue[j];

            int len = num * span;
            float step;
            if (max < fmax) {
                max += rulerValue[j];
                num = num + 1;
                step = len / (float) num;
            }
            else step = span;

            for (int n = 0; n < (num + 1); n++) {
                Point p = new Point();
                p.X = n * step;
                p.Y = min + n * rulerValue[j];
                points.Add(p);
            }
            Draw();
            //Console.WriteLine("{0:f1}  {1:f1}  {2:f1}  {3:f1}  {4:f1}", fmin,fmax,min, max, RulerValue[j]);
        }

        public void Draw() {
            SolidColorBrush brush = new SolidColorBrush(Color.FromRgb(47, 233, 4));

            canvasLine.Children.Clear();
            canvasText.Children.Clear();

            for (int i = 0; i < points.Count; i++) {
                Line line = new Line();
                line.Stroke = brush;
                line.X1 = 0;
                line.Y1 = points[i].X;
                line.X2 = 10;
                line.Y2 = points[i].X;

                canvasLine.Children.Add(line);

                TextBlock tb = new TextBlock();
                tb.Width = 70;
                tb.Foreground = brush;
                tb.Background = null;

                float y = (float) points[i].Y;
                tb.Text = y.ToString("F1");

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
