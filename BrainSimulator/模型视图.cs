//
// Copyright (c) Charles Simon. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
//  


using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Shapes;
using System.Xml.Serialization;

namespace BrainSimulator
{

    public partial class 模块视图
    {
        int firstNeuron = 0;
        string label;
        string moduleTypeStr;
        int color;
        Modules.ModuleBase theModule = null;
        int width = 0;
        int height = 0;
        [XmlIgnore] //used when displaying the module at small scales
        public System.Windows.Media.Imaging.WriteableBitmap bitmap = null;

        public IEnumerable<神经元> Neurons
        {
            get
            {
                for (int i = 0; i < NeuronCount; i++)
                    yield return GetNeuronAt(i);
            }
        }


        public 模块视图(int firstNeuron1, int width, int height, string theLabel, string theModuleTypeStr, int theColor)
        {
            FirstNeuron = firstNeuron1;
            Width = width;
            Height = height;
            Label = theLabel;
            ModuleTypeStr = theModuleTypeStr;
            color = theColor;
            Type t = Type.GetType("BrainSimulator.Modules." + theModuleTypeStr);
            theModule = (Modules.ModuleBase)Activator.CreateInstance(t);
        }

        public 模块视图() { }
        public string Label { get => label.StartsWith("Module") ? label.Replace("Module", "") : label; set => label = value; }
        public int FirstNeuron { get => firstNeuron; set => firstNeuron = value; }
        public int Height
        {
            get => height;
            set
            {
                for (int row = value; row < height; row++)
                {
                    for (int col = 0; col < width; col++)
                        GetNeuronAt(col, row).清空();
                }
                height = value;
                if (TheModule != null) TheModule.SizeChanged();
            }
        }
        public int Width
        {
            get => width;
            set
            {
                for (int col = value; col < width; col++)
                {
                    for (int row = 0; row < height; row++)
                        GetNeuronAt(col, row).清空();
                }
                width = value;
                if (TheModule != null) TheModule.SizeChanged();
            }
        }
        public int Color { get => color; set => color = value; }

        public string ModuleTypeStr { get => moduleTypeStr; set => moduleTypeStr = value; }
        private int Rows { get { return MainWindow.此神经元数组.rows; } }

        public int NeuronCount { get { return Width * Height; } }
        public Modules.ModuleBase TheModule { get => theModule; set => theModule = value; }
        public int LastNeuron { get { return firstNeuron + (height - 1) + Rows * (Width - 1); } }

        public int GetNeuronOffset(神经元 n)
        {
            return GetNeuronOffset(n.Id);
        }

        public int GetNeuronOffset(int id)
        {
            id -= firstNeuron;
            int col = id / Rows;
            int row = id % Rows;
            id = col * height + row;
            return id;
        }
        public Rectangle GetRectangle(显示参数 dp)
        {
            Rectangle r = new Rectangle();
            Point p1 = dp.神经元坐标(firstNeuron);
            Point p2 = dp.神经元坐标(LastNeuron);
            p2.X += dp.神经元图示大小;
            p2.Y += dp.神经元图示大小;
            r.Width = Math.Abs(p2.X - p1.X);
            r.Height = Math.Abs(p2.Y - p1.Y);
            Canvas.SetTop(r, p1.Y);
            Canvas.SetLeft(r, p1.X);
            return r;
        }


        public 神经元 GetFreeNeuron()
        {
            foreach (神经元 n in Neurons)
                if (!n.InUse())
                    return n;
            return null;
        }
        //this gets the neuron with an index relative to the area itself
        public 神经元 GetNeuronAt(int index)
        {
            if (index >= NeuronCount) return null;
            int neuronIndex = (index % Height) + (index / Height) * Rows + firstNeuron;
            return MainWindow.此神经元数组.获取神经元(neuronIndex);
        }

        public 神经元 GetNeuronAt(int X, int Y)
        {
            if (X < 0) return null;
            if (Y < 0) return null;
            if (X >= Width) return null;
            if (Y >= Height) return null;
            int index = firstNeuron + Y + X * Rows;
            return MainWindow.此神经元数组.获取神经元(index);
        }

        public int GetNeuronIndexAt(int x,int y)
        {
            int index = firstNeuron + y + x * Rows;
            return index;
        }

        public int GetAbsNeuronIndexAt(int X, int Y)
        {
            int index = Y + X * Rows;
            return index;
        }

        public 神经元 GetNeuronAt(string label)
        {
            for (int i = 0; i < NeuronCount; i++)
            {
                神经元 n = GetNeuronAt(i);
                if (n.标签名.ToLower() == label.ToLower())
                    return n;
            }
            return null;
        }

        public void GetBounds(out int X1, out int Y1, out int X2, out int Y2)
        {
            选择矩阵 nsr = new 选择矩阵(firstNeuron, Width, Height);
            nsr.获取选择区域(out X1, out Y1, out X2, out Y2);
        }
        public void GetAbsNeuronLocation(int index, out int X, out int Y)
        {
            X = index / Rows;
            Y = index % Rows;
        }

        public void GetNeuronLocation(神经元 n, out int X, out int Y)
        {
            GetAbsNeuronLocation(n.Id, out int X1, out int Y1);
            GetAbsNeuronLocation(FirstNeuron, out int X2, out int Y2);
            X = X1 - X2;
            Y = Y1 - Y2;
        }

        public void GetNeuronLocation(int nIndex, out int X, out int Y)
        {
            GetAbsNeuronLocation(nIndex, out int X1, out int Y1);
            GetAbsNeuronLocation(FirstNeuron, out int X2, out int Y2);
            X = X1 - X2;
            Y = Y1 - Y2;
        }
    }
}