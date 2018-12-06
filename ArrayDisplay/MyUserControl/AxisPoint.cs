using System;
using System.Collections.Generic;
using System.Windows;

namespace ArrayDisplay.MyUserControl {
    public class AxisPoint {
        readonly float[] allStep = {0.01F, 0.1F, 0.5F, 1, 5, 10, 50, 100, 500, 1000, 5000, 10000};
        readonly List<Point> listPoint = new List<Point>();
        int length = 100; //坐标轴长度，像素点
        int maxRealStep = 20; //物理坐标步长
        float maxValue = 100; //最大值
        float minValue; //最小值
        float spanLength;
        float step = 50; //步长

        public AxisPoint() {
            RigthShow = false;
            LeftShow = false;
        }

        public float MinValue { set { minValue = value; } }

        public float MaxValue { set { maxValue = value; } }

        public int Length { set { length = value; } get { return length; } }

        public bool LeftShow { get; private set; }

        public bool RigthShow { get; private set; }

        public float SpanLength { set { spanLength = value; } }

        public List<Point> ListPoint { get { return listPoint; } }

        public int MaxRealStep { set { maxRealStep = value; } }

        void GetStep() {
            for (int n = 0; n < allStep.Length; n++) {
                step = allStep[n];
                float num = (maxValue - minValue) / step;
                float num2 = length / maxRealStep;
                if (num > 1 && num <= num2) break;
            }
            //Console.WriteLine("{0}  {1}   {2}", step, minValue, maxValue);
        }

        public void GetRule() {
            float leftRng = 0.0F;
            float rightRng = 0.0F;
            float middleRng = 0.0F;
            float rng = maxValue - minValue;

            if (Math.Abs(spanLength) < 0.0001) GetStep();
            else step = spanLength;

            //计算起始点是否能整除步长
            float mod = minValue / step - (int) (minValue / step);

            if (Math.Abs(mod) > 0.0001) {
                int j = (int) (minValue / step);
                float x2;
                while (true) {
                    float x1 = j * step;
                    x2 = (j + 1) * step;
                    j++;
                    if (x1 < minValue && x2 > minValue) break;
                }
                leftRng = x2 - minValue; //左边频率范围 
            }
            else leftRng = 0;

            float start = minValue + leftRng; //第一个能整除步长的起始值
            //Console.WriteLine("#####{0}  {1}  {2}  {3}", step.ToString("f0"), leftRng.ToString("f0"), minValue.ToString("f0"), _start.ToString("f0"));

            float templeft = leftRng;
            while (true) {
                float temp = templeft + step;
                if (temp < rng) templeft = temp;
                else break;
            }

            rightRng = rng - templeft; //计算右边的频率范围，当该值小于步长
            middleRng = rng - leftRng - rightRng; //计算完左边，右边，中间的频率范围

            float rulerCnt = rng / step;

            float realStep = length / rulerCnt; //物理坐标步长

            if (leftRng > 0) {
                if ((leftRng * realStep / step) > 30) //判断空间是否足够
                    LeftShow = true;
                Point p1 = new Point(0, minValue);
                listPoint.Add(p1);
            }
            else LeftShow = true;
            //right
            double pos;
            pos = realStep * leftRng / step;
            Point p2 = new Point(pos, start);
            listPoint.Add(p2);

            float midleft = 0;
            int i = 1;
            double pos1 = pos;

            while (midleft < middleRng) {
                pos = i * realStep + pos1;
                midleft += step;

                //第i个点
                Point px = new Point(pos, start + i * step);
                listPoint.Add(px);
                i++;
            }
            //right
            if (rightRng > 0) {
                if ((rightRng * realStep / step) > 30) //判断空间是否足够
                    RigthShow = true;
                Point pn = new Point(length, maxValue);
                listPoint.Add(pn);
            }
            else RigthShow = true;
        }
    }
}
