//
// Copyright (c) Charles Simon. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
//  

using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace BrainSimulator
{
    public class 选择矩阵
    {
        int firstSelectedNeuron;

        int width;
        int height;
        public int 行数 { get { return MainWindow.此神经元数组.rows; } }


        public int 首个选中的神经元 { get => firstSelectedNeuron; set => firstSelectedNeuron = value; }
        public int 最后选中的神经元 { get { return 首个选中的神经元 + (高度 - 1) + 行数 * (宽度 - 1); } }
        public int 宽度 { get => width; set => width = value; }
        public int 高度 { get => height; set => height = value; }
        public System.Windows.Media.Imaging.WriteableBitmap bitmap = null;


        public 选择矩阵(int iFirstSelectedNeuron, int width, int height)
        {
            首个选中的神经元 = iFirstSelectedNeuron;
            高度 = height;
            宽度 = width;
        }

        public int 获取神经元索引(int x,int y)
        {
            return firstSelectedNeuron + x * (MainWindow.此神经元数组.rows) + y;
        }

        public IEnumerable<int> 矩阵中的神经元()
        {
            int count = width * height;
            for (int i = 0; i < count; i++)
            {
                int row = i % height;
                int col = i / height;
                int target = firstSelectedNeuron + row + MainWindow.此神经元数组.rows * col;
                yield return target;
            }
        }



        //in neuron numbers
        public void 获取选择区域(out int X1, out int Y1, out int X2, out int Y2)
        {
            Y1 = 首个选中的神经元 % 行数;
            X1 = 首个选中的神经元 / 行数;
            Y2 = Y1 + 高度 - 1;
            X2 = X1 + 宽度 - 1;
        }

        //in pixels
        public Rectangle 获取矩阵(显示参数 dp)
        {
            Rectangle r = new Rectangle();
            Point p1 = dp.神经元坐标(首个选中的神经元);
            Point p2 = dp.神经元坐标(最后选中的神经元);
            p2.X += dp.神经元图示大小;
            p2.Y += dp.神经元图示大小;
            r.Width = p2.X - p1.X;
            r.Height = p2.Y - p1.Y;
            Canvas.SetTop(r, p1.Y);
            Canvas.SetLeft(r, p1.X);
            return r;
        }

        public bool NeuronIsInSelection(int neuronIndex)
        {
            获取选择区域(out int X1, out int Y1, out int X2, out int Y2);
            int selX = neuronIndex / 行数;
            int selY = neuronIndex % 行数;
            if (selX >= X1 && selX <= X2 && selY >= Y1 && selY <= Y2)
                return true;
            return false;
        }

        public int GetLength()
        {
            获取选择区域(out int X1, out int Y1, out int X2, out int Y2);
            return (X2 - X1 + 1) * (Y2 - Y1 + 1);
        }
    }
}
