//
// Copyright (c) Charles Simon. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
//  

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace BrainSimulator
{
    public partial class 神经元数组视图 : UserControl
    {

        private 显示参数 dp = new 显示参数();

        public int targetNeuronIndex = -1;

        //needed for handling selections of areas of neurons
        Rectangle dragRectangle = null;
        int firstSelectedNeuron = -1;
        int lastSelectedNeuron = -1;

        //keeps track of the multiple selection areas
        public 选择 theSelection = new 选择();

        int Rows { get { return MainWindow.此神经元数组.行数; } }

        //this helper class keeps track of the neurons on the screen so they can change color without repainting
        private List<NeuronOnScreen> neuronsOnScreen = new List<NeuronOnScreen>();
        public class NeuronOnScreen
        {
            public int neuronIndex;
            public UIElement graphic;
            public float prevValue;
            public TextBlock label;
            public List<synapseOnScreen> synapsesOnScreen = null;
            public struct synapseOnScreen
            {
                public int target;
                public float prevWeight;
                public Shape graphic;
            }
            public NeuronOnScreen(int index, UIElement e, float value, TextBlock Label)
            {
                neuronIndex = index; graphic = e; prevValue = value; label = Label;
            }
        };

        List<int> showSynapses = new List<int>();
        public void 添加突触显示(int neuronID)
        {
            if (!showSynapses.Contains(neuronID))
                showSynapses.Add(neuronID);
        }
        public void RemoveShowSynapses(int neuronID)
        {
            showSynapses.Remove(neuronID);
        }
        public bool IsShowingSynapses(int neuronID)
        {
            return showSynapses.Contains(neuronID);
        }
        public void ClearShowingSynapses()
        {
            showSynapses.Clear();
        }

        public 神经元数组视图()
        {
            InitializeComponent();
            zoomRepeatTimer.Tick += Dt_Tick;
#if DEBUG
            System.Diagnostics.PresentationTraceSources.DataBindingSource.Switch.Level = System.Diagnostics.SourceLevels.Critical;
#endif
        }

        public 显示参数 Dp { get => dp; set => dp = value; }

        private Canvas targetNeuronCanvas = null;

        //refresh the display of the neuron network
        public void 更新()
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            神经元数组 theNeuronArray = MainWindow.此神经元数组;

            Canvas labelCanvas = new Canvas();
            Canvas.SetLeft(labelCanvas, 0);
            Canvas.SetTop(labelCanvas, 0);


            //Two canvases for synapses, ALL is for synapses behing neurons, Special is for synapses in front of neurons
            Canvas allSynapsesCanvas = new Canvas();
            Canvas.SetLeft(allSynapsesCanvas, 0);
            Canvas.SetTop(allSynapsesCanvas, 0);

            Canvas specialSynapsesCanvas = new Canvas();
            Canvas.SetLeft(specialSynapsesCanvas, 0);
            Canvas.SetTop(specialSynapsesCanvas, 0);


            int neuronCanvasCount = 2;
            Canvas[] neuronCanvas = new Canvas[neuronCanvasCount];
            for (int i = 0; i < neuronCanvasCount; i++)
                neuronCanvas[i] = new Canvas();

            Canvas legendCanvas = new Canvas();
            Canvas.SetLeft(legendCanvas, 0);
            Canvas.SetTop(legendCanvas, 0);

            targetNeuronCanvas = new Canvas();
            Canvas.SetLeft(targetNeuronCanvas, 0);
            Canvas.SetTop(targetNeuronCanvas, 0);

            if (MainWindow.数组是否为空()) return;

            //Debug.WriteLine("Update " + MainWindow.theNeuronArray.Generation);
            dp.神经元行数 = MainWindow.此神经元数组.行数;
            theCanvas.Children.Clear();
            neuronsOnScreen.Clear();
            int columns = MainWindow.此神经元数组.数组大小 / dp.神经元行数;

            //draw some background grid and labels
            int boxSize = 250;
            if (columns > 2500) boxSize = 1000;
            for (int i = 0; i <= theNeuronArray.行数; i += boxSize)
            {
                Line l = new Line
                {
                    X1 = dp.DisplayOffset.X + 0,
                    X2 = dp.DisplayOffset.X + columns * dp.神经元图示大小,
                    Y1 = dp.DisplayOffset.Y + i * dp.神经元图示大小,
                    Y2 = dp.DisplayOffset.Y + i * dp.神经元图示大小,
                    Stroke = new SolidColorBrush(Colors.Red),
                };
                legendCanvas.Children.Add(l);
            }
            for (int j = 0; j <= columns; j += boxSize)
            {
                Line l = new Line
                {
                    X1 = dp.DisplayOffset.X + j * dp.神经元图示大小,
                    X2 = dp.DisplayOffset.X + j * dp.神经元图示大小,
                    Y1 = dp.DisplayOffset.Y + 0,
                    Y2 = dp.DisplayOffset.Y + theNeuronArray.行数 * dp.神经元图示大小,
                    Stroke = new SolidColorBrush(Colors.Red),
                };
                legendCanvas.Children.Add(l);
            }

            int refNo = 1;
            for (int i = 0; i < theNeuronArray.行数; i += boxSize)
            {
                for (int j = 0; j < columns; j += boxSize)
                {
                    Point p = new Point((j + boxSize / 2) * dp.神经元图示大小, (i + boxSize / 2) * dp.神经元图示大小);
                    p += (Vector)dp.DisplayOffset;
                    Label l = new Label();
                    l.Content = refNo++;
                    l.FontSize = dp.神经元图示大小 * 10;
                    if (l.FontSize < 25) l.FontSize = 25;
                    if (l.FontSize > boxSize * dp.神经元图示大小 * 0.75)
                        l.FontSize = boxSize * dp.神经元图示大小 * 0.75;
                    l.Foreground = Brushes.White;
                    l.HorizontalAlignment = HorizontalAlignment.Center;
                    l.VerticalAlignment = VerticalAlignment.Center;
                    l.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

                    Canvas.SetLeft(l, p.X - l.DesiredSize.Width / 2);
                    Canvas.SetTop(l, p.Y - l.DesiredSize.Height / 2);
                    legendCanvas.Children.Add(l);

                }
            }

            //draw the module rectangles
            lock (theNeuronArray.Modules)
            {
                for (int i = 0; i < MainWindow.此神经元数组.Modules.Count; i++)
                {
                    模块视图 nr = MainWindow.此神经元数组.Modules[i];
                    选择矩阵 nsr = new 选择矩阵(nr.FirstNeuron, nr.Width, nr.Height);
                    Rectangle r = nsr.获取矩阵(dp);
                    r.Fill = new SolidColorBrush(跨语言接口.IntToColor(nr.Color));
                    r.SetValue(ShapeType, shapeType.Module);
                    r.SetValue(模块视图.AreaNumberProperty, i);
                    theCanvas.Children.Add(r);

                    //Label moduleLabel = new Label();
                    TextBlock moduleLabel = new();
                    moduleLabel.Text = nr.Label;
                    moduleLabel.Background = new SolidColorBrush(Colors.White);
                    if (!nr.TheModule.isEnabled)
                        moduleLabel.Background = new SolidColorBrush(Colors.LightGray);
                    moduleLabel.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                    Canvas.SetLeft(moduleLabel, Canvas.GetLeft(r));
                    Canvas.SetTop(moduleLabel, Canvas.GetTop(r)-moduleLabel.DesiredSize.Height);
                    moduleLabel.SetValue(ShapeType, shapeType.Module);
                    moduleLabel.SetValue(模块视图.AreaNumberProperty, i);
                    labelCanvas.Children.Add(moduleLabel);
                }
            }
            //draw any selection rectangle(s)
            for (int i = 0; i < theSelection.selectedRectangles.Count; i++)
            {
                Rectangle r = theSelection.selectedRectangles[i].获取矩阵(dp);
                r.Fill = new SolidColorBrush(Colors.Pink);
                r.SetValue(模块视图.AreaNumberProperty, i);
                r.SetValue(ShapeType, shapeType.Selection);

                theCanvas.Children.Add(r);
                模块视图 nr = new 模块视图
                {
                    Label = "new",
                    Width = theSelection.selectedRectangles[i].宽度,
                    Height = theSelection.selectedRectangles[i].高度,
                    Color = 跨语言接口.ColorToInt(Colors.Aquamarine),
                    ModuleTypeStr = ""
                };

                if (!dp.显示神经元())
                {
                    //TODO is any part of the rectangle visible?
                    int height = (int)r.Height;
                    int width = (int)r.Width;
                    float vRatio = (float)(r.Height / (float)nr.Height);
                    float hRatio = (float)(r.Width / (float)nr.Width);

                    if (height > 1 && width > 1)
                    {
                        theSelection.selectedRectangles[i].bitmap = new System.Windows.Media.Imaging.WriteableBitmap(width, height, 96, 96, PixelFormats.Bgr32, null);

                        uint[] pixels = new uint[width * height]; ;
                        for (int x = 0; x < width; x++)
                            for (int y = 0; y < height; y++)
                            {
                                {
                                    int k = width * y + x;
                                    int index = theSelection.selectedRectangles[i].获取神经元索引((int)(x / hRatio), (int)(y / vRatio));
                                    神经元 n = MainWindow.此神经元数组.GetNeuronForDrawing(index);
                                    uint val = (uint)跨语言接口.ColorToInt(神经元视图.GetNeuronColor(n).Color);
                                    pixels[k] = val;
                                }
                            }
                        // apply pixels to bitmap
                        theSelection.selectedRectangles[i].bitmap.WritePixels(new Int32Rect(0, 0, width, height), pixels, width * 4, 0);

                        Image img = new Image();
                        img.Source = theSelection.selectedRectangles[i].bitmap;
                        Canvas.SetLeft(img, Canvas.GetLeft(r));
                        Canvas.SetTop(img, Canvas.GetTop(r));
                        theCanvas.Children.Add(img);
                        img.SetValue(模块视图.AreaNumberProperty, -i - 1);
                    }
                }
            }
            if (dragRectangle != null) theCanvas.Children.Add(dragRectangle);


            //highlight the "target" neuron
            SetTargetNeuronSymbol();

            //draw the neurons
            int synapseCount = 0;
            if (dp.显示神经元())
            {
                dp.GetRowColFromPoint(new Point(0, 0), out int startCol, out int startRow);
                startRow--;
                startCol--;
                dp.GetRowColFromPoint(new Point(theCanvas.ActualWidth, theCanvas.ActualHeight), out int endCol, out int endRow);
                endRow++;
                endCol++;
                if (startRow < 0) startRow = 0;
                if (startCol < 0) startCol = 0;
                if (endRow > Rows) endRow = Rows;
                if (endCol > columns) endCol = columns;
                for (int col = startCol; col < endCol; col++)
                {
                    List<神经元> neuronColumn = null;
                    for (int row = startRow; row < endRow; row++)
                    {
                        int neuronID = dp.GetAbsNeuronAt(col, row);
                        if (neuronID >= 0 && neuronID < theNeuronArray.数组大小)
                        {
                            神经元 n;
                            if (MainWindow.useServers)
                            {
                                //buffering a column of neurons makes a HUGE performance difference
                                if (neuronColumn == null)
                                    neuronColumn = 神经元客户端.获取神经元(neuronID, endRow - startRow);
                                n = neuronColumn[row - startRow];
                            }
                            else
                            {
                                n = theNeuronArray.GetCompleteNeuron(neuronID);
                            }
                            UIElement l = 神经元视图.GetNeuronView(n, this, out TextBlock lbl);
                            if (l != null)
                            {
                                int canvas = neuronID % neuronCanvasCount;
                                neuronCanvas[canvas].Children.Add(l);

                                if (lbl != null && dp.显示神经元标签())
                                {
                                    if (l is Shape s && s.Fill is SolidColorBrush b && b.Color == Colors.White)
                                        lbl.Foreground = new SolidColorBrush(Colors.Black);
                                    lbl.SetValue(ShapeType, shapeType.Neuron);
                                    labelCanvas.Children.Add(lbl);
                                }

                                NeuronOnScreen neuronScreenCache = null;
                                if ((n.是否使用 || n.标签名 != "" || n.当前更改 != 0 || n.最后更改 != 0) && (l is Ellipse || l is Rectangle))
                                {
                                    neuronScreenCache = new NeuronOnScreen(neuronID, l, -10, lbl);
                                }

                                if (synapseCount < dp.maxSynapsesToDisplay &&
                                    dp.显示突触() && (MainWindow.此神经元数组.ShowSynapses || IsShowingSynapses(n.id)))
                                {
                                    if (MainWindow.useServers && n.是否使用)
                                        n = theNeuronArray.添加突触(n);
                                    Point p1 = dp.神经元坐标(neuronID);

                                    if (n.突触列表 != null)
                                    {
                                        foreach (突触 s in n.突触列表)
                                        {
                                            Shape l1 = 突触视图.GetSynapseView(neuronID, p1, s, this);
                                            if (l1 != null)
                                            {
                                                if (IsShowingSynapses(n.id))
                                                    specialSynapsesCanvas.Children.Add(l1);
                                                else
                                                    allSynapsesCanvas.Children.Add(l1);
                                                synapseCount++;
                                                if (neuronScreenCache != null && s.model != 突触.modelType.Fixed)
                                                {
                                                    if (neuronScreenCache.synapsesOnScreen == null)
                                                        neuronScreenCache.synapsesOnScreen = new List<NeuronOnScreen.synapseOnScreen>();
                                                    neuronScreenCache.synapsesOnScreen.Add(
                                                        new NeuronOnScreen.synapseOnScreen { target = s.targetNeuron, prevWeight = s.weight, graphic = l1 });
                                                }
                                            }
                                        }
                                    }
                                    if (n.SynapsesFrom != null)
                                    {
                                        foreach (突触 s in n.SynapsesFrom)
                                        {
                                            //check the synapesFrom to draw synapes which source outside the window
                                            dp.GetAbsNeuronLocation(s.targetNeuron, out int x, out int y);
                                            if (x >= startCol && x < endCol && y >= startRow && y < endRow) continue;
                                            Point p2 = dp.神经元坐标(s.targetNeuron);
                                            突触 s1 = new 突触() { targetNeuron = neuronID, Weight = s.Weight };
                                            Shape l1 = 突触视图.GetSynapseView(s.targetNeuron, p2, s1, this);
                                            if (l1 != null)
                                                allSynapsesCanvas.Children.Add(l1);
                                        }
                                    }
                                }
                                if (neuronScreenCache != null)
                                    neuronsOnScreen.Add(neuronScreenCache);

                            }
                        }
                    }
                }
            }

            theCanvas.Children.Add(legendCanvas);

            theCanvas.Children.Add(allSynapsesCanvas);
            theCanvas.Children.Add(targetNeuronCanvas);
            for (int i = 0; i < neuronCanvasCount; i++)
            {
                theCanvas.Children.Add(neuronCanvas[i]);
            }

            theCanvas.Children.Add(specialSynapsesCanvas);
            theCanvas.Children.Add(labelCanvas);
            if (synapseShape != null) //synapse rubber-banding
                theCanvas.Children.Add(synapseShape);

            UpdateScrollbars();
            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;
            if (synapseCount >= dp.maxSynapsesToDisplay)
                MainWindow.thisWindow.设置窗口底部状态(0, "突触过多，无法显示", 1);
            else
            {
                if (!dp.显示神经元())
                    MainWindow.thisWindow.设置窗口底部状态(0, "网格大小: " + (boxSize * boxSize).ToString("#,##"),1);
                else
                    MainWindow.thisWindow.设置窗口底部状态(0,"OK",0);
            }
            MainWindow.thisWindow.更新空内存显示();
            //Debug.WriteLine("Update Done " + elapsedMs + "ms");
        }

        public void AddNeuronToUpdateList(int neuronID)
        {
            if (neuronsOnScreen.Count == 0) return;
            if (MainWindow.thisWindow.progressDialog.Visibility == Visibility.Visible)
                return;
            神经元 n = MainWindow.此神经元数组.GetCompleteNeuron(neuronID);
            UIElement l = 神经元视图.GetNeuronView(n, this, out TextBlock lbl);
            theCanvas.Children.Add(l);
            if (lbl != null)    
                theCanvas.Children.Add(lbl);
            NeuronOnScreen neuronScreenCache = new NeuronOnScreen(neuronID, l, -10, lbl);
            neuronsOnScreen.Add(neuronScreenCache);
        }

        //just update the colors of the neurons based on their current charge
        //and synapses based on current weight
        public void 更新神经元颜色()
        {
            if (MainWindow.useServers)
            {
                int index = 0; //current index into neuronsOnScreen array
                int begin = 0; //beginning of a continuout sequences of neurons to retrieve
                while (index < neuronsOnScreen.Count)
                {
                    if (index == neuronsOnScreen.Count - 1 || neuronsOnScreen[index].neuronIndex + 1 != neuronsOnScreen[index + 1].neuronIndex)
                    {
                        List<神经元> neuronColumn = 神经元客户端.获取神经元(neuronsOnScreen[begin].neuronIndex, index - begin + 1);
                        for (int i = 0; i < neuronColumn.Count; i++)
                        {
                            int nosIndex = i + begin;
                            if (nosIndex < neuronsOnScreen.Count && neuronColumn[i].最后更改 != neuronsOnScreen[nosIndex].prevValue)
                            {
                                neuronsOnScreen[nosIndex].prevValue = neuronColumn[i].最后更改;
                                if (neuronsOnScreen[nosIndex].graphic is Shape e)
                                {
                                    e.Fill = 神经元视图.GetNeuronColor(neuronColumn[i]);
                                }
                            }
                        }
                        begin = index + 1;
                    }
                    index++;
                }
            }
            else
            {
                //for small arrays, repaint everything so synapse weights will update
                //if (false) //use this for testing of 
                //if (neuronsOnScreen.Count < 451 && scale == 1)
                //{
                //    Update();
                //    if (MainWindow.theNeuronArray != null)
                //    {
                //        MainWindow.UpdateDisplayLabel(dp.NeuronDisplaySize);
                //        MainWindow.UpdateEngineLabel((int)MainWindow.theNeuronArray.lastFireCount);
                //    }
                //    return;
                //}

                SetTargetNeuronSymbol();

                for (int i = 0; i < neuronsOnScreen.Count; i++)
                {
                    NeuronOnScreen a = neuronsOnScreen[i];
                    神经元 n = MainWindow.此神经元数组.GetNeuronForDrawing(a.neuronIndex);
                    if (neuronsOnScreen[i].synapsesOnScreen != null)
                    {
                        n.突触列表 = MainWindow.此神经元数组.获取突触列表(n.id);
                        for (int j = 0; j < neuronsOnScreen[i].synapsesOnScreen.Count; j++)
                        {
                            NeuronOnScreen.synapseOnScreen sOnS = neuronsOnScreen[i].synapsesOnScreen[j];
                            foreach (突触 s in n.突触列表)
                            {
                                if (sOnS.target == s.targetNeuron)
                                {
                                    if (sOnS.prevWeight != s.weight)
                                    {
                                        sOnS.graphic.Stroke = sOnS.graphic.Fill = new SolidColorBrush(跨语言接口.RainbowColorFromValue(s.weight));
                                        sOnS.prevWeight = s.weight;
                                    }
                                    break;
                                }
                            }
                        }
                    }
                    if (a.graphic is Shape e)
                    {
                        float x = n.最后更改;
                        SolidColorBrush newColor = null;
                        if (x != a.prevValue)
                        {
                            a.prevValue = x;

                            newColor = 神经元视图.GetNeuronColor(n);
                            e.Fill = newColor;
                            if (n.最后更改 != 0 && e.Fill.Opacity != 1)
                                e.Fill.Opacity = 1;
                        }

                        string newLabel = 神经元视图.GetNeuronLabel(n);
                        if (newLabel.IndexOf('|') != -1) newLabel = newLabel.Substring(0, newLabel.IndexOf('|'));
                        if (a.label != null && newLabel != (string)a.label.Text)
                        {
                            a.label.Text = newLabel;
                            if (e.Fill.Opacity != 1)
                                e.Fill.Opacity = 1;
                        }
                        if (a.label == null && newLabel != "")
                        {
                            UIElement l = 神经元视图.GetNeuronView(n, this, out TextBlock lbl);
                            if (e.Fill.Opacity != 1)
                                e.Fill.Opacity = 1;
                            a.label = lbl;
                            theCanvas.Children.Add(lbl);
                        }
                        if (newColor != null && a.label != null)
                        {
                            if (newColor.Color == Colors.White)
                                a.label.Foreground = new SolidColorBrush(Colors.Black);
                            else
                                a.label.Foreground = new SolidColorBrush(Colors.White);
                        }
                    }
                }
            }

            //Update the content of modules on small display scales
            for (int i = 0; i < theSelection.selectedRectangles.Count; i++)
            {
                Rectangle r = theSelection.selectedRectangles[i].获取矩阵(dp);
                模块视图 nr = new 模块视图
                {
                    Label = "new",
                    Width = theSelection.selectedRectangles[i].宽度,
                    Height = theSelection.selectedRectangles[i].高度,
                    Color = 跨语言接口.ColorToInt(Colors.Aquamarine),
                    ModuleTypeStr = ""
                };

                if (!dp.显示神经元())
                {
                    //TODO is any part of the rectangle visible?
                    int height = (int)r.Height;
                    int width = (int)r.Width;
                    float vRatio = (float)(r.Height / (float)nr.Height);
                    float hRatio = (float)(r.Width / (float)nr.Width);

                    uint[] pixels = new uint[width * height]; ;
                    for (int x = 0; x < width; x++)
                        for (int y = 0; y < height; y++)
                        {
                            {
                                int k = width * y + x;
                                int index = theSelection.selectedRectangles[i].获取神经元索引((int)(x / hRatio), (int)(y / vRatio));
                                神经元 n = MainWindow.此神经元数组.GetNeuronForDrawing(index);
                                uint val = (uint)跨语言接口.ColorToInt(神经元视图.GetNeuronColor(n).Color);
                                pixels[k] = val;
                            }
                        }
                    // apply pixels to bitmap
                    if (theSelection.selectedRectangles[i].bitmap != null)
                        try
                        {
                            theSelection.selectedRectangles[i].bitmap.WritePixels(new Int32Rect(0, 0, width, height), pixels, width * 4, 0);
                        }
                        catch (Exception)
                        {
                            //this exception can occur on a collision between Update and UpdateNeuronColors...
                            //if we ignore it, it may be benign.
                            //MessageBox.Show("Bitmap Write failed: " + e.Message);
                        }
                }
            }


            if (MainWindow.此神经元数组 != null)
            {
                MainWindow.UpdateDisplayLabel(dp.神经元图示大小);
                MainWindow.UpdateEngineLabel((int)MainWindow.此神经元数组.lastFireCount);
            }
        }

        private void SetTargetNeuronSymbol()
        {
            if (targetNeuronIndex != -1 && scale == 1)
            {
                Ellipse r = new Ellipse();
                Point p1 = dp.神经元坐标(targetNeuronIndex);
                r.Width = r.Height = dp.神经元图示大小;
                Canvas.SetTop(r, p1.Y);
                Canvas.SetLeft(r, p1.X);
                r.Fill = new SolidColorBrush(Colors.LightBlue);
                targetNeuronCanvas.Children.Clear();
                targetNeuronCanvas.Children.Add(r);
            }
        }

        private void theCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            神经元视图.theCanvas = theCanvas;//??  
            突触视图.theCanvas = theCanvas;//??
            if (MainWindow.数组是否为空()) return;
            更新();
        }
    }
}
