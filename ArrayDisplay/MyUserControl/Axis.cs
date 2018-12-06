using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace ArrayDisplay.MyUserControl {
    public class Axis {
        /// <summary>
        /// 生成刻度线和刻度值
        /// </summary>
        /// <param name="canvas_line">待生成刻度线</param>
        /// <param name="canvas_text">刻度线对应刻度值</param>
        /// <param name="min">数据最小值</param>
        /// <param name="max">数据最大值</param>
        /// <param name="realstep">步长</param>
        /// <param name="pixels">像素长度</param>
        public static void DrawyAxis(Canvas canvas_line, Canvas canvas_text, float min, float max,
                                     int realstep, int pixels) {
            SolidColorBrush brush = new SolidColorBrush(Colors.Black);
            AxisPoint axis = new AxisPoint();
            axis.MinValue = min;
            axis.MaxValue = max;
            axis.MaxRealStep = realstep;
            axis.Length = pixels;
            axis.GetRule();

            canvas_line.Children.Clear();
            canvas_text.Children.Clear();

            for(int i = 0; i < axis.ListPoint.Count; i++) {
                Line line = new Line();
                line.Stroke = brush;
                line.X1 = 0;
                line.Y1 = axis.ListPoint[i].X;
                line.X2 = 10;
                line.Y2 = axis.ListPoint[i].X;

                canvas_line.Children.Add(line);

                if (i == 0) if (!axis.LeftShow) continue;
                if (i == axis.ListPoint.Count - 1) if (!axis.RigthShow) continue;

                TextBlock tb = new TextBlock();
                tb.Width = 40;
                tb.Foreground = brush;
                tb.Background = null; //new System.Windows.Media.SolidColorBrush(Colors.White);

                tb.Text = axis.ListPoint[i].Y.ToString("F1");
                tb.TextAlignment = TextAlignment.Center;

                canvas_text.Children.Add(tb);
                Canvas.SetBottom(tb, axis.ListPoint[i].X);
                tb.Loaded += ytb_Loaded;
            }
        }

        /// <summary>
        /// </summary>
        public static void DrawyAxis(Canvas canvas_line, Canvas canvas_text, float min, float max,
                                     int realstep, int pixels, float spanlength) {
            SolidColorBrush brush = new SolidColorBrush(Colors.Black);
            AxisPoint axis = new AxisPoint {MinValue = min, MaxValue = max, MaxRealStep = realstep, Length = pixels, SpanLength = spanlength};
            axis.GetRule();

            canvas_line.Children.Clear();
            canvas_text.Children.Clear();

            for(int i = 0; i < axis.ListPoint.Count; i++) {
                Line line = new Line {Stroke = brush, X1 = 0, Y1 = axis.ListPoint[i].X, X2 = 10, Y2 = axis.ListPoint[i].X};

                canvas_line.Children.Add(line);

                if (i == 0) if (!axis.LeftShow) continue;
                if (i == axis.ListPoint.Count - 1) if (!axis.RigthShow) continue;

                TextBlock tb = new TextBlock();
                tb.Width = 40;
                tb.Foreground = brush;
                tb.Background = null; //new System.Windows.Media.SolidColorBrush(Colors.White);

                tb.Text = axis.ListPoint[i].Y.ToString("F1");
                tb.TextAlignment = TextAlignment.Center;

                canvas_text.Children.Add(tb);
                Canvas.SetBottom(tb, axis.ListPoint[i].X);
                tb.Loaded += ytb_Loaded;
            }
        }

        /// <summary>
        /// </summary>
        public static void DrawyAxis(Canvas canvas_line, Canvas canvas_text, int min, int max,
                                     int realstep, int pixels) {
            SolidColorBrush brush = new SolidColorBrush(Colors.Black);
            AxisPoint axis = new AxisPoint();
            axis.MinValue = min;
            axis.MaxValue = max;
            axis.MaxRealStep = realstep;

            axis.Length = pixels;
            axis.GetRule();

            canvas_line.Children.Clear();
            canvas_text.Children.Clear();

            for(int i = 0; i < axis.ListPoint.Count; i++) {
                Line line = new Line();
                line.Stroke = brush;
                line.X1 = 0;
                line.Y1 = axis.ListPoint[i].X;
                line.X2 = 10;
                line.Y2 = axis.ListPoint[i].X;

                canvas_line.Children.Add(line);

                if (i == 0) if (!axis.LeftShow) continue;
                if (i == axis.ListPoint.Count - 1) if (!axis.RigthShow) continue;

                TextBlock tb = new TextBlock();
                tb.Width = 40;
                tb.Foreground = brush;
                tb.Background = null; //new System.Windows.Media.SolidColorBrush(Colors.White);

                tb.Text = axis.ListPoint[i].Y.ToString("F0");
                tb.TextAlignment = TextAlignment.Center;

                canvas_text.Children.Add(tb);
                Canvas.SetBottom(tb, axis.ListPoint[i].X);
                tb.Loaded += ytb_Loaded;
            }
        }

        /// <summary>
        /// </summary>
        public static void DrawyAxis(Canvas canvas_line, Canvas canvas_text, int min, int max,
                                     int realstep, int pixels, float span_length) {
            SolidColorBrush brush = new SolidColorBrush(Colors.Black);
            AxisPoint axis = new AxisPoint();
            axis.MinValue = min;
            axis.MaxValue = max;
            axis.MaxRealStep = realstep;
            axis.SpanLength = span_length;
            axis.Length = pixels;
            axis.GetRule();

            canvas_line.Children.Clear();
            canvas_text.Children.Clear();

            for(int i = 0; i < axis.ListPoint.Count; i++) {
                Line line = new Line();
                line.Stroke = brush;
                line.X1 = 0;
                line.Y1 = axis.ListPoint[i].X;
                line.X2 = 10;
                line.Y2 = axis.ListPoint[i].X;

                canvas_line.Children.Add(line);

                if (i == 0) if (!axis.LeftShow) continue;
                if (i == axis.ListPoint.Count - 1) if (!axis.RigthShow) continue;

                TextBlock tb = new TextBlock();
                tb.Width = 40;
                tb.Foreground = brush;
                tb.Background = null; //new System.Windows.Media.SolidColorBrush(Colors.White);

                tb.Text = axis.ListPoint[i].Y.ToString("F0");
                tb.TextAlignment = TextAlignment.Center;

                canvas_text.Children.Add(tb);
                Canvas.SetBottom(tb, axis.ListPoint[i].X);
                tb.Loaded += ytb_Loaded;
            }
        }

        /// <summary>
        /// </summary>
        public static void DrawyAxisReverse(Canvas canvas_line, Canvas canvas_text, int min, int max,
                                            int realstep, int pixels) {
            SolidColorBrush brush = new SolidColorBrush(Colors.Black);
            AxisPoint axis = new AxisPoint();
            axis.MinValue = min;
            axis.MaxValue = max;
            axis.MaxRealStep = realstep;

            axis.Length = pixels;
            axis.GetRule();

            canvas_line.Children.Clear();
            canvas_text.Children.Clear();

            for(int i = 0; i < axis.ListPoint.Count; i++) {
                Line line = new Line();
                line.Stroke = brush;
                line.X1 = 0;
                line.Y1 = axis.ListPoint[i].X;
                line.X2 = 10;
                line.Y2 = axis.ListPoint[i].X;

                canvas_line.Children.Add(line);

                if (i == 0) if (!axis.LeftShow) continue;
                if (i == axis.ListPoint.Count - 1) if (!axis.RigthShow) continue;

                TextBlock tb = new TextBlock();
                tb.Width = 70;
                tb.Foreground = brush;
                tb.Background = null;

                int temp = max - (int) axis.ListPoint[i].Y;
                tb.Text = temp.ToString("F0") + "'";
                tb.TextAlignment = TextAlignment.Center;

                canvas_text.Children.Add(tb);
                Canvas.SetBottom(tb, axis.ListPoint[i].X);
                tb.Loaded += ytb_Loaded;
            }
        }

        /// <summary>
        /// </summary>
        public static void DrawyAxisReverse(Canvas canvas_line, Canvas canvas_text, int min, int max,
                                            int realstep, int pixels, float span_length) {
            SolidColorBrush brush = new SolidColorBrush(Colors.Black);
            AxisPoint axis = new AxisPoint();
            axis.MinValue = min;
            axis.MaxValue = max;
            axis.MaxRealStep = realstep;
            axis.SpanLength = span_length;
            axis.Length = pixels;
            axis.GetRule();

            canvas_line.Children.Clear();
            canvas_text.Children.Clear();

            for(int i = 0; i < axis.ListPoint.Count; i++) {
                Line line = new Line();
                line.Stroke = brush;
                line.X1 = 0;
                line.Y1 = axis.ListPoint[i].X;
                line.X2 = 10;
                line.Y2 = axis.ListPoint[i].X;

                canvas_line.Children.Add(line);

                if (i == 0) if (!axis.LeftShow) continue;
                if (i == axis.ListPoint.Count - 1) if (!axis.RigthShow) continue;

                TextBlock tb = new TextBlock();
                tb.Width = 70;
                tb.Foreground = brush;
                tb.Background = null;

                int temp = max - (int) axis.ListPoint[i].Y;
                tb.Text = temp.ToString("F0") + "'";
                tb.TextAlignment = TextAlignment.Center;

                canvas_text.Children.Add(tb);
                Canvas.SetBottom(tb, axis.ListPoint[i].X);
                tb.Loaded += ytb_Loaded;
            }
        }

        /// <summary>
        /// </summary>
        public static void DrawxAxis(Canvas canvas_line, Canvas canvas_text, float min, float max,
                                     int realstep, int pixels) {
            SolidColorBrush brush = new SolidColorBrush(Colors.Black);
            AxisPoint axis = new AxisPoint();
            axis.MinValue = min;
            axis.MaxValue = max;
            axis.MaxRealStep = realstep;
            axis.Length = pixels;
            axis.GetRule();

            canvas_line.Children.Clear();
            canvas_text.Children.Clear();

            for(int i = 0; i < axis.ListPoint.Count; i++) {
                Line line = new Line();
                line.Stroke = brush;
                line.Y1 = 0;
                line.X1 = axis.ListPoint[i].X;
                line.Y2 = 10;
                line.X2 = axis.ListPoint[i].X;

                canvas_line.Children.Add(line);

                if (i == 0) if (!axis.LeftShow) continue;
                if (i == axis.ListPoint.Count - 1) if (!axis.RigthShow) continue;
                TextBlock tb = new TextBlock();
                tb.Width = 40;
                tb.Foreground = brush;
                tb.Background = null; //new SolidColorBrush(Colors.White);
                tb.Text = axis.ListPoint[i].Y.ToString("F1");
                tb.TextAlignment = TextAlignment.Center;
                canvas_text.Children.Add(tb);
                Canvas.SetLeft(tb, axis.ListPoint[i].X);
                tb.Loaded += xtb_Loaded;
            }
        }

        /// <summary>
        /// </summary>
        public static void DrawxAxis(Canvas canvas_line, Canvas canvas_text, float min, float max,
                                     int realstep, int pixels, float span_length) {
            SolidColorBrush brush = new SolidColorBrush(Colors.Black);
            AxisPoint axis = new AxisPoint();
            axis.MinValue = min;
            axis.MaxValue = max;
            axis.MaxRealStep = realstep;
            axis.Length = pixels;
            axis.SpanLength = span_length;
            axis.GetRule();

            canvas_line.Children.Clear();
            canvas_text.Children.Clear();

            for(int i = 0; i < axis.ListPoint.Count; i++) {
                Line line = new Line();
                line.Stroke = brush;
                line.Y1 = 0;
                line.X1 = axis.ListPoint[i].X;
                line.Y2 = 10;
                line.X2 = axis.ListPoint[i].X;

                canvas_line.Children.Add(line);

                if (i == 0) if (!axis.LeftShow) continue;
                if (i == axis.ListPoint.Count - 1) if (!axis.RigthShow) continue;
                TextBlock tb = new TextBlock();
                tb.Width = 40;
                tb.Foreground = brush;
                tb.Background = null; //new SolidColorBrush(Colors.White);
                tb.Text = axis.ListPoint[i].Y.ToString("F1");
                tb.TextAlignment = TextAlignment.Center;
                canvas_text.Children.Add(tb);
                Canvas.SetLeft(tb, axis.ListPoint[i].X);
                tb.Loaded += xtb_Loaded;
            }
        }

        /// <summary>
        /// </summary>
        public static void DrawxAxis(Canvas canvas_line, Canvas canvas_text, int min, int max,
                                     int realstep, int pixels) {
            SolidColorBrush brush = new SolidColorBrush(Colors.Black);
            AxisPoint axis = new AxisPoint();
            axis.MinValue = min;
            axis.MaxValue = max;
            axis.MaxRealStep = realstep;
            axis.Length = pixels;
            axis.GetRule();

            canvas_line.Children.Clear();
            canvas_text.Children.Clear();

            for(int i = 0; i < axis.ListPoint.Count; i++) {
                Line line = new Line();
                line.Stroke = brush;
                line.Y1 = 0;
                line.X1 = axis.ListPoint[i].X;
                line.Y2 = 10;
                line.X2 = axis.ListPoint[i].X;

                canvas_line.Children.Add(line);

                if (i == 0) if (!axis.LeftShow) continue;
                if (i == axis.ListPoint.Count - 1) if (!axis.RigthShow) continue;
                TextBlock tb = new TextBlock();
                tb.Width = 40;
                tb.Foreground = brush;
                tb.Background = null; //new SolidColorBrush(Colors.White);
                tb.Text = axis.ListPoint[i].Y.ToString("F0");
                tb.TextAlignment = TextAlignment.Center;
                canvas_text.Children.Add(tb);
                Canvas.SetLeft(tb, axis.ListPoint[i].X);
                tb.Loaded += xtb_Loaded;
            }
        }

        /// <summary>
        /// </summary>
        public static void DrawxAxis(Canvas canvas_line, Canvas canvas_text, int min, int max,
                                     int realstep, int pixels, int span_length) {
            SolidColorBrush brush = new SolidColorBrush(Colors.Black);
            AxisPoint axis = new AxisPoint();
            axis.MinValue = min;
            axis.MaxValue = max;
            axis.MaxRealStep = realstep;
            axis.Length = pixels;
            axis.SpanLength = span_length;
            axis.GetRule();

            canvas_line.Children.Clear();
            canvas_text.Children.Clear();

            for(int i = 0; i < axis.ListPoint.Count; i++) {
                Line line = new Line();
                line.Stroke = brush;
                line.Y1 = 0;
                line.X1 = axis.ListPoint[i].X;
                line.Y2 = 10;
                line.X2 = axis.ListPoint[i].X;

                canvas_line.Children.Add(line);

                if (i == 0) if (!axis.LeftShow) continue;
                if (i == axis.ListPoint.Count - 1) if (!axis.RigthShow) continue;
                TextBlock tb = new TextBlock();
                tb.Width = 40;
                tb.Foreground = brush;
                tb.Background = null; //new SolidColorBrush(Colors.White);
                tb.Text = axis.ListPoint[i].Y.ToString("F0");
                tb.TextAlignment = TextAlignment.Center;
                canvas_text.Children.Add(tb);
                Canvas.SetLeft(tb, axis.ListPoint[i].X);
                tb.Loaded += xtb_Loaded;
            }
        }

        /// <summary>
        /// </summary>
        static void xtb_Loaded(object sender, RoutedEventArgs e) {
            TextBlock tb = sender as TextBlock;
            TranslateTransform ttf = new TranslateTransform();
            ttf.X = -tb.ActualWidth / 2;
            tb.RenderTransform = ttf;
        }

        /// <summary>
        /// </summary>
        static void ytb_Loaded(object sender, RoutedEventArgs e) {
            TextBlock tb = sender as TextBlock;
            TranslateTransform ttf = new TranslateTransform();
            ttf.Y = tb.ActualHeight / 2;
            tb.RenderTransform = ttf;
        }
    }
}
