using System;
using System.Linq;

namespace ArrayDisplay.MyUserControl {
    /// <summary>
    /// 裁剪数据长度
    /// </summary>
    public class CutData {
        public CutData(int src_length, int dst_length, int cut_left, int cut_right, int cut_length)
        {
            SrcDataLength = src_length;
            DstDataLength = dst_length;
            CutLeft = cut_left;
            CutRight = cut_right;
            CutDataLength = cut_length;
        }
        /// <summary>
        /// 原始数据长度
        /// </summary>
        public int SrcDataLength { get; set; }
        /// <summary>
        /// 目标数据长度
        /// </summary>
        public int DstDataLength { get; set; }
        /// <summary>
        /// 待裁剪数据长度
        /// </summary>
        public int CutDataLength { get; set; }

        public int CutLeft { get; set; }

        public int CutRight { get; set; }

        public void ReCutLength(float scaler, int src_center) {
            int count = 1;
            int centerleft = src_center;
            int centerright = src_center;
            int src_points_nums = (int) (SrcDataLength / scaler);

            if (src_points_nums < DstDataLength) src_points_nums = DstDataLength;

            int cut_nums = CutRight - CutLeft + 1;//裁剪像素长度

            if (src_points_nums < cut_nums) //放大
                while (count < src_points_nums) {
                    if (centerleft > CutLeft) {
                        count++;
                        centerleft--;
                    }
                    if (count == src_points_nums) break;
                    if (centerright < CutRight) {
                        count++;
                        centerright++;
                    }
                    if (count == src_points_nums) break;
                }
            else if (src_points_nums > cut_nums) //缩小
                while (count < src_points_nums) {
                    if (centerleft > 0) {
                        count++;
                        centerleft--;
                    }
                    if (count == src_points_nums) break;
                    if (centerright < (SrcDataLength - 1)) {
                        count++;
                        centerright++;
                    }
                    if (count == src_points_nums) break;
                }
            else //不变
            {
                centerleft = CutLeft;
                centerright = CutRight;
            }
            CutDataLength = src_points_nums;//输出
            CutLeft = centerleft;//输出
            CutRight = centerright;//输出
        }

        /// <summary>
        ///     获取中心值
        /// </summary>
        /// <param name="data"></param>
        /// <param name="center_data"></param>
        public void GetCenterData(float[] data, ref float[] center_data) {
            int count = 0;
            int front = 0;
            int back = 0;
            var selectData = new float[CutDataLength];  //关键
            Array.Copy(data, CutLeft, selectData, 0, CutDataLength);
            float step = CutDataLength * 1.0F / DstDataLength;
            for (int i = 0; i < DstDataLength; i++) {
                front = back;
                back = (int) (i * step + step);
                var sectionBuf = new float[back - front];
                Array.Copy(selectData, count, sectionBuf, 0, sectionBuf.Length);
                count += sectionBuf.Length;
                center_data[i] = sectionBuf[(int)(sectionBuf.Length / 2 + 0.5)];
            }
        }

        /// <summary>
        ///     计算平均值
        /// </summary>
        float GetAverageValue(float[] farray) {
            float sum = 0;
            for (int i = 0; i < farray.Length; i++) sum += farray[i];
            return sum / farray.Length;
        }

        /// <summary>
        ///     最大值和平均值
        /// </summary>
        /// <param name="data"></param>
        public void GetMaxAverageData(float[] data, ref float[] max_data, ref float[] average_data) {
            int count = 0;
            int front = 0;
            int back = 0;
            int len = CutDataLength;
            var selectData = new float[len];
            Array.Copy(data, CutLeft, selectData, 0, len);
            float step = len * 1.0F / DstDataLength;
            for (int i = 0; i < DstDataLength; i++) {
                front = back;
                back = (int) (i * step + step);
                var sectionBuf = new float[back - front];
                Array.Copy(selectData, count, sectionBuf, 0, sectionBuf.Length);
                count += sectionBuf.Length;
                max_data[i] = sectionBuf.Max();
                average_data[i] = GetAverageValue(sectionBuf); //取平均值
            }
        }

        /// <summary>
        ///     最大值和最小值
        /// </summary>
        /// <param name="data"></param>
        public void GetMaxMinData(float[] data, ref float[] max_data, ref float[] min_data) {
            int count = 0;
            int front = 0;
            int back = 0;
            int len = CutDataLength;
            var selectData = new float[len];
            Array.Copy(data, CutLeft, selectData, 0, len);
            float step = len * 1.0F / DstDataLength;
            for (int i = 0; i < DstDataLength; i++) {
                front = back;
                back = (int) (i * step + step);
                var sectionBuf = new float[back - front];
                Array.Copy(selectData, count, sectionBuf, 0, sectionBuf.Length);
                count += sectionBuf.Length;
                max_data[i] = sectionBuf.Max();
                min_data[i] = sectionBuf.Min();
            }
        }

        /// <summary>
        ///     最大值和最小值
        /// </summary>
        /// <param name="data"></param>
        public void GetMaxData(float[] data, ref float[] max_data) {
            int count = 0;
            int front = 0;
            int back = 0;
            int len = CutDataLength;
            var selectData = new float[len];
            Array.Copy(data, CutLeft, selectData, 0, len);
            float step = len * 1.0F / DstDataLength;
            for (int i = 0; i < DstDataLength; i++) {
                front = back;
                back = (int) (i * step + step);
                var sectionBuf = new float[back - front];
                Array.Copy(selectData, count, sectionBuf, 0, sectionBuf.Length);
                count += sectionBuf.Length;
                max_data[i] = sectionBuf.Max();
            }
        }
    }
}
