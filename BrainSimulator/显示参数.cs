//
// Copyright (c) Charles Simon. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
//  

using System.Windows;
using System.Xml.Serialization;

namespace BrainSimulator
{
    public class 显示参数
    {
        private float 神经元显示大小 = 45; //这是缩放级别
        private Point displayOffset = new Point(0, 0); //the pan position平移位置
        private int neuronRows = -1;     //这个数字让我们将一维数组显示为二维数组

        public bool 显示突触箭头() { return 神经元显示大小 > 55; }
        public bool 显示突触宽线() { return 神经元显示大小 > 50; }
        public bool 显示突触箭头光标() { return 神经元显示大小 > 50; }
        public bool 显示突触() { return 神经元显示大小 > 40; }

        public bool 显示神经元箭头光标() { return 神经元显示大小 > 10; }
        public bool 显示神经元外框() { return 神经元显示大小 > 15; }
        public bool 显示神经元边界() { return 神经元显示大小 > 10; }
        public bool 显示神经元标签() { return 神经元显示大小 > 20; }
        public bool 显示神经元() { return 神经元显示大小 > 6; }

        [XmlIgnore]
        public int maxSynapsesToDisplay = 2000;

        public float 神经元图示大小
        {
            get
            {
                return 神经元显示大小;
            }

            set
            {
                神经元显示大小 = value;
                突触视图.dp = this;
                神经元视图.dp = this;
            }
        }

        public Point DisplayOffset
        {
            get
            {
                return displayOffset;
            }

            set
            {
                displayOffset = value;
                突触视图.dp = this;
                神经元视图.dp = this;
            }
        }

        public int 神经元行数
        {
            get
            {
                return neuronRows;
            }

            set
            {
                neuronRows = value;
                突触视图.dp = this;
                神经元视图.dp = this;
            }
        }

        //返回画布上神经元的左上角
        public Point 神经元坐标(int index)
        {
            Point p = new Point(DisplayOffset.X, DisplayOffset.Y);
            p.Y += index % 神经元行数 * 神经元显示大小;
            p.X += index / 神经元行数 * 神经元显示大小;
            return p;
        }

        //返回该点的神经元 ID
        //该点是画布位置
        public int 坐标上的神经元(Point p)
        {
            p -= (Vector)DisplayOffset;
            int x = (int)(p.X / 神经元显示大小);
            int y = (int)(p.Y / 神经元显示大小);
            if (y >= 神经元行数) y = 神经元行数 - 1;
            int index = x * 神经元行数 + y;
            return index;
        }
        public void GetRowColFromPoint(Point p, out int x, out int y)
        {
            p -= (Vector)DisplayOffset;
            x = (int)(p.X / 神经元显示大小);
            y = (int)(p.Y / 神经元显示大小);
        }
        public int GetAbsNeuronAt(int X, int Y)
        {
            return X * 神经元行数 + Y;
        }

        public void GetAbsNeuronLocation(int index, out int X, out int Y)
        {
            X = index / 神经元行数;
            Y = index % 神经元行数;
        }

    }
}
