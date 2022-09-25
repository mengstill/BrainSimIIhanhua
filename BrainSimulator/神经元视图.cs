//
// Copyright (c) Charles Simon. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
//  

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Collections.Generic;
using System.Diagnostics;


namespace BrainSimulator
{
    public partial class 神经元视图
    {
        public static 显示参数 dp;
        public static Canvas theCanvas;     //reflection of canvas in neuronarrayview
        static 神经元数组视图 此神经元数组视图;

        public static readonly DependencyProperty NeuronIDProperty =
                DependencyProperty.Register("NeuronID", typeof(int), typeof(MenuItem));
        public int NeuronID
        {
            get { return (int)GetValue(NeuronIDProperty); }
            set { SetValue(NeuronIDProperty, value); }
        }
        private static float ellipseSize = 0.7f;

        public static UIElement GetNeuronView(神经元 n, 神经元数组视图 theNeuronArrayViewI, out TextBlock tb)
        {
            tb = null;
            此神经元数组视图 = theNeuronArrayViewI;

            Point p = dp.神经元坐标(n.id);

            SolidColorBrush s1 = GetNeuronColor(n);

            Shape r = null;
            if (dp.显示神经元边界())
            {
                r = new Ellipse();
                r.Width = dp.神经元图示大小 * ellipseSize;
                r.Height = dp.神经元图示大小 * ellipseSize;
            }
            else
            {
                r = new Rectangle();
                r.Width = dp.神经元图示大小-2;
                r.Height = dp.神经元图示大小-2;
            }
            r.Fill = s1;
            if (dp.显示神经元外框())
            {
                r.Stroke = Brushes.Black;
                r.StrokeThickness = 1;
            }

            float offset = (1 - ellipseSize) / 2f;
            Canvas.SetLeft(r, p.X + dp.神经元图示大小 * offset);
            Canvas.SetTop(r, p.Y + dp.神经元图示大小 * offset);

            if (n.标签名 != "" || n.模型字段 != 神经元.模型类型.IF)
            {
                tb = new TextBlock();
                //l.Content = n.Label;
                tb.FontSize = dp.神经元图示大小 * .25;
                tb.Foreground = Brushes.White;
                Canvas.SetLeft(tb, p.X + dp.神经元图示大小 * offset);
                Canvas.SetTop(tb, p.Y + dp.神经元图示大小 * offset);
                Canvas.SetZIndex(tb, 100);

                string theLabel = GetNeuronLabel(n);
                string theToolTip = n.ToolTip;
                if (theToolTip != "")
                {
                    r.ToolTip = new ToolTip { Content = theToolTip };
                    tb.ToolTip = new ToolTip { Content = theToolTip };
                }
                tb.Text = theLabel;
                tb.SetValue(NeuronIDProperty, n.id);
                tb.SetValue(神经元数组视图.ShapeType, 神经元数组视图.shapeType.Neuron);
            }
            r.SetValue(NeuronIDProperty,n.id);
            r.SetValue(神经元数组视图.ShapeType, 神经元数组视图.shapeType.Neuron);
            return r;
        }

        public static string GetNeuronLabel(神经元 n)
        {
            string retVal = "";
            if (!dp.显示神经元标签()) return retVal;

            retVal = n.标签名;
            if (retVal.Length > 0 && retVal[0] == '_')
                retVal = retVal.Remove(0, 1);
            if (n.模型字段 == 神经元.模型类型.LIF)
            {
                if (n.leakRate泄露速度 == 0)
                    retVal += "\rD=" + n.突触延迟.ToString();
                else if (n.leakRate泄露速度 < 1)
                    retVal += "\rL=" + n.leakRate泄露速度.ToString("f2").Substring(1);
                else
                    retVal += "\rL=" + n.leakRate泄露速度.ToString("f1");
            }
            if (n.模型字段 == 神经元.模型类型.Burst)
                retVal += "\rB=" + n.突触延迟.ToString();
            if (n.模型字段 == 神经元.模型类型.Random)
                retVal += "\rR=" + n.突触延迟.ToString();
            if (n.模型字段 == 神经元.模型类型.Always)
                retVal += "\rA=" + n.突触延迟.ToString();
            return retVal;
        }

        public static SolidColorBrush GetNeuronColor(神经元 n)
        {
            // figure out which color to use
            if (n.模型字段 == 神经元.模型类型.Color)
            {
                SolidColorBrush brush = new SolidColorBrush(跨语言接口.IntToColor(n.LastChargeInt));
                return brush;
            }
            float value = n.LastCharge;
            Color c = 跨语言接口.RainbowColorFromValue(value);
            SolidColorBrush s1 = new SolidColorBrush(c);
            if (!n.是否使用 && n.模型 == 神经元.模型类型.IF && n.标签名 =="")
                s1.Opacity = .50;
            if ((n.leakRate泄露速度 < 0) || float.IsNegativeInfinity(1.0f / n.leakRate泄露速度))
                s1 = new SolidColorBrush(Colors.LightSalmon);
            return s1;
        }
    }
}
