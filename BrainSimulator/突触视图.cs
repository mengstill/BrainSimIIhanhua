//
// Copyright (c) Charles Simon. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
//  

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace BrainSimulator
{

    public class 突触视图
    {
        public static 显示参数 dp;
        static 神经元数组视图 theNeuronArrayView = null;
        public static Canvas theCanvas;

        public static readonly DependencyProperty SourceIDProperty =
                DependencyProperty.Register("SourceID", typeof(int), typeof(Shape));
        public static readonly DependencyProperty TargetIDProperty =
                DependencyProperty.Register("TargetID", typeof(int), typeof(Shape));
        public static readonly DependencyProperty WeightValProperty =
                DependencyProperty.Register("WeightVal", typeof(float), typeof(Shape));
        public static readonly DependencyProperty ModelProperty =
                DependencyProperty.Register("ModelVal", typeof(突触.modelType), typeof(Shape));



        static bool PtOnScreen(Point p)
        {
            if (p.X < -dp.神经元图示大小) return false;
            if (p.Y < -dp.神经元图示大小) return false;
            if (p.X > theCanvas.ActualWidth + dp.神经元图示大小) return false;
            if (p.Y > theCanvas.ActualHeight + dp.神经元图示大小) return false;
            return true;
        }

        public static Shape GetSynapseView(int i, Point p1, 突触 s, 神经元数组视图 theNeuronArrayView1)
        {
            theNeuronArrayView = theNeuronArrayView1;
            Point p2 = dp.神经元坐标(s.TargetNeuron);
            if (!PtOnScreen(p1) && !PtOnScreen(p2)) return null;

            Shape l = GetSynapseShape(p1, p2, s.model);
            l.Stroke = new SolidColorBrush(跨语言接口.RainbowColorFromValue(s.weight));
            if (l is Ellipse E)
            { }
            else
                l.Fill = l.Stroke;
            l.SetValue(SourceIDProperty, i);
            l.SetValue(TargetIDProperty, s.TargetNeuron);
            l.SetValue(WeightValProperty, s.Weight);
            l.SetValue(ModelProperty, s.model);
            l.SetValue(神经元数组视图.ShapeType, 神经元数组视图.shapeType.Synapse);

            return l;
        }
        public static Shape GetSynapseShape(Point p1, Point p2, 突触.modelType model)
        {
            //returns a line from the source to the destination (with a link arrow at larger zooms
            //unless the source and destination are the same in which it returns an arc
            Shape s;
            if (p1 != p2)
            {
                Line l = new Line();
                l.X1 = p1.X + dp.神经元图示大小 / 2;
                l.X2 = p2.X + dp.神经元图示大小 / 2;
                l.Y1 = p1.Y + dp.神经元图示大小 / 2;
                l.Y2 = p2.Y + dp.神经元图示大小 / 2;
                s = l;
                if (dp.显示突触箭头())
                {
                    Vector offset = new Vector(dp.神经元图示大小 / 2, dp.神经元图示大小 / 2);
                    s = DrawLinkArrow(p1 + offset, p2 + offset, model != 突触.modelType.Fixed);
                }
            }
            else
            {
                s = new Ellipse();
                s.Height = s.Width = dp.神经元图示大小 * .6;
                Canvas.SetTop(s, p1.Y + dp.神经元图示大小 / 4);
                Canvas.SetLeft(s, p1.X + dp.神经元图示大小 / 2);
            }
            s.Stroke = Brushes.Red;
            s.StrokeThickness = 1;
            if (dp.显示突触宽线())
            {
                s.StrokeThickness = Math.Min(4, dp.神经元图示大小 / 15);
            }

            return s;
        }

        public static Shape DrawLinkArrow(Point p1, Point p2, bool canLearn) //helper to put an arrow in a synapse line
        {
            GeometryGroup lineGroup = new GeometryGroup();
            double theta = Math.Atan2((p2.Y - p1.Y), (p2.X - p1.X)) * 180 / Math.PI;

            PathGeometry pathGeometry = new PathGeometry();
            PathFigure pathFigure = new PathFigure();
            Vector v = p2 - p1;
            double lengthFactor = ((dp.神经元图示大小 * 0.7f / 2f) + 3) / v.Length;
            //lengthfactor = 0.5; //how it used to be
            v = v * lengthFactor;

            Point p = new Point();
            p = p2 - v;
            pathFigure.StartPoint = p;
            double arrowWidth = dp.神经元图示大小 / 20;
            double arrowLength = dp.神经元图示大小 / 15;
            if (canLearn)
            {
                arrowWidth *= 2;
            }
            Point lpoint = new Point(p.X + arrowWidth, p.Y + arrowLength);
            Point rpoint = new Point(p.X - arrowWidth, p.Y + arrowLength);
            LineSegment seg1 = new LineSegment();
            seg1.Point = lpoint;
            pathFigure.Segments.Add(seg1);

            LineSegment seg2 = new LineSegment();
            seg2.Point = rpoint;
            pathFigure.Segments.Add(seg2);

            LineSegment seg3 = new LineSegment();
            seg3.Point = p;
            pathFigure.Segments.Add(seg3);

            pathGeometry.Figures.Add(pathFigure);
            RotateTransform transform = new RotateTransform();
            transform.Angle = theta + 90;
            transform.CenterX = p.X;
            transform.CenterY = p.Y;
            pathGeometry.Transform = transform;
            lineGroup.Children.Add(pathGeometry);

            LineGeometry connectorGeometry = new LineGeometry();
            connectorGeometry.StartPoint = p1;
            connectorGeometry.EndPoint = p2;
            lineGroup.Children.Add(connectorGeometry);

            System.Windows.Shapes.Path path = new System.Windows.Shapes.Path();
            path.Data = lineGroup;
            path.StrokeThickness = 2;
            return path;
        }


        //these aren't added to synapses (for performance) but are built on the fly if the user right-clicks
        public static void CreateContextMenu(int i, 突触 s, ContextMenu cm)
        {
            cmCancelled = false;
            weightChanged = false;

            //set defaults for next synapse add
            theNeuronArrayView.末尾突触模型 = s.model;
            theNeuronArrayView.末尾突触权重 = s.weight;


            cm.SetValue(SourceIDProperty, i);
            cm.SetValue(TargetIDProperty, s.TargetNeuron);
            cm.SetValue(WeightValProperty, s.Weight);
            cm.SetValue(ModelProperty, s.model);

            cm.Closed += Cm_Closed;
            cm.PreviewKeyDown += Cm_PreviewKeyDown;
            cm.Opened += Cm_Opened;

            神经元 nSource = MainWindow.此神经元数组.获取神经元(i);
            神经元 nTarget = MainWindow.此神经元数组.获取神经元(s.targetNeuron);
            StackPanel sp = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 3, 3, 3) };
            sp.Children.Add(new Label { Content = "Source: ", Padding = new Thickness(0) });
            string source = nSource.id.ToString();
            if (nSource.标签 != "")
                source = nSource.标签名;
            TextBox t0 = 跨语言接口.ContextMenuTextBox(source, "Source", 150);
            t0.TextChanged += TextChanged;
            sp.Children.Add(t0);
            cm.Items.Add(new MenuItem { Header = sp, StaysOpenOnClick = true });

            sp = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 3, 3, 3) };
            sp.Children.Add(new Label { Content = "Target: ", Padding = new Thickness(0) });
            string target = nTarget.id.ToString();
            if (nTarget.标签 != "")
                target = nTarget.标签名;
            TextBox t1 = 跨语言接口.ContextMenuTextBox(target, "Target", 150);
            t1.TextChanged += TextChanged;
            sp.Children.Add(t1);
            cm.Items.Add(new MenuItem { Header = sp, StaysOpenOnClick = true });

            //The Synapse model
            sp = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 3, 3, 3) };
            sp.Children.Add(new Label { Content = "Model: ", Padding = new Thickness(0) });
            ComboBox cb = new ComboBox()
            {
                Width = 100,
                Name = "Model"
            };
            for (int index = 0; index < Enum.GetValues(typeof(突触.modelType)).Length; index++)
            {
                突触.modelType model = (突触.modelType)index;
                cb.Items.Add(new ListBoxItem()
                {
                    Content = model.ToString(),
                    ToolTip = 突触.modelToolTip[index],
                    Width = 100,
                });
            }
            cb.SelectedIndex = (int)s.model;
            sp.Children.Add(cb);
            cm.Items.Add(new MenuItem { Header = sp, StaysOpenOnClick = true });

            cm.Items.Add(跨语言接口.CreateComboBoxMenuItem("SynapseWeight", s.weight, synapseWeightValues, "F3", "Weight: ", 100, ComboBox_ContentChanged));

            MenuItem mi = new MenuItem();
            mi.Header = "Delete";
            mi.Click += DeleteSynapse_Click;
            cm.Items.Add(mi);

            sp = new StackPanel { Orientation = Orientation.Horizontal };
            Button b0 = new Button { Content = "OK", Width = 100, Height = 25, Margin = new Thickness(10), IsDefault = true };
            b0.Click += B0_Click;
            sp.Children.Add(b0);
            b0 = new Button { Content = "Cancel", Width = 100, Height = 25, Margin = new Thickness(10), IsCancel = true };
            b0.Click += B0_Click;
            sp.Children.Add(b0);

            cm.Items.Add(new MenuItem { Header = sp, StaysOpenOnClick = true });

        }

        private static void TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox tb)
            {
                //is numeric?
                if (int.TryParse(tb.Text, out int newID))
                {
                    if (newID < 0 || newID >= MainWindow.此神经元数组.数组大小)
                        tb.Background = new SolidColorBrush(Colors.Pink);
                    else
                        tb.Background = new SolidColorBrush(Colors.LightGreen);
                }
                else //is non-numeric
                {
                    神经元 n = MainWindow.此神经元数组.GetNeuron(tb.Text);
                    {
                        if (n == null)
                            tb.Background = new SolidColorBrush(Colors.Pink);
                        else
                            tb.Background = new SolidColorBrush(Colors.LightGreen);
                    }
                }
            }
        }

        static bool weightChanged = false;
        static List<float> synapseWeightValues = new List<float> { 1, .5f, .334f, .25f, .2f, 0, -1 };
        private static void ComboBox_ContentChanged(object sender, object e)
        {
            if (sender is ComboBox cb)
            {
                if (!cb.IsArrangeValid) return;
                if (cb.Name == "SynapseWeight")
                {
                    weightChanged = true;
                    跨语言接口.ValidateInput(cb, -1, 1);
                }
            }
        }

        private static void Cm_Opened(object sender, RoutedEventArgs e)
        {
            //when the context menu opens, focus on the label and position text cursor to end
            if (sender is ContextMenu cm)
            {
                if (跨语言接口.FindByName(cm, "SynapseWeight") is ComboBox cc)
                {
                    //this hack finds the textbox within a combobox
                    TextBox tb = (TextBox)cc.Template.FindName("PART_EditableTextBox", cc);
                    tb.Focus();
                    tb.Select(0, tb.Text.Length);
                }
            }
        }

        private static void Cm_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            ContextMenu cm = sender as ContextMenu;
            if (e.Key == Key.Delete)
            {
                var focussedControl = FocusManager.GetFocusedElement(cm);
                if (focussedControl.GetType() != typeof(TextBox))
                {
                    MainWindow.此神经元数组.获取神经元((int)cm.GetValue(SourceIDProperty)).删除突触((int)cm.GetValue(TargetIDProperty));
                    MainWindow.Update();
                    cmCancelled = true;
                    cm.IsOpen = false;
                }
            }
            if (e.Key == Key.Enter)
            {
                cm.IsOpen = false;
            }
            //This hack is here because textboxes don't like to lose focus if the mouse moves aroundt the context menu
            //When this becomes a window, all this will go away
            if (e.Key == Key.Tab)
            {
                {
                    var focussedControl = FocusManager.GetFocusedElement(cm);
                    if (focussedControl is TextBox tb)
                    {
                        if (tb.Name == "Source")
                        {
                            Control tt = 跨语言接口.FindByName(cm, "Target");
                            if (tt is TextBox tbtt)
                            {
                                tbtt.Focus();
                                e.Handled = true;
                            }
                            else
                            {
                                Control cc = 跨语言接口.FindByName(cm, "Model");
                                cc.Focus();
                                e.Handled = true;
                            }
                        }
                        else if (tb.Name == "Target")
                        {
                            Control cc = 跨语言接口.FindByName(cm, "Model");
                            cc.Focus();
                            e.Handled = true;

                        }
                    }
                }
            }

        }

        static bool cmCancelled = false;
        private static void Cm_Closed(object sender, RoutedEventArgs e)
        {
            if (sender is ContextMenu cm)
            {
                if (cmCancelled || (Keyboard.GetKeyStates(Key.Escape) & KeyStates.Down) > 0)
                {
                    cm.IsOpen = false;
                    MainWindow.Update();
                    return;
                }
                int sourceID = (int)cm.GetValue(SourceIDProperty);
                int targetID = (int)cm.GetValue(TargetIDProperty);
                神经元 nSource = MainWindow.此神经元数组.获取神经元(sourceID);
                神经元 nTarget = MainWindow.此神经元数组.获取神经元(targetID);
                int newSourceID = sourceID;
                int newTargetID = targetID;

                Control cc = 跨语言接口.FindByName(cm, "Target");
                if (cc is TextBox tb)
                {
                    string targetLabel = tb.Text;
                    if (nTarget.标签 != targetLabel)
                    {
                        if (!int.TryParse(tb.Text, out newTargetID))
                        {
                            newTargetID = targetID;
                            神经元 n = MainWindow.此神经元数组.GetNeuron(targetLabel);
                            if (n != null)
                                newTargetID = n.id;
                        }
                    }
                }
                cc = 跨语言接口.FindByName(cm, "Source");
                if (cc is TextBox tb1)
                {
                    string sourceLabel = tb1.Text;
                    if (nSource.标签 != sourceLabel)
                    {
                        if (!int.TryParse(tb1.Text, out newSourceID))
                        {
                            newSourceID = sourceID;
                            神经元 n = MainWindow.此神经元数组.GetNeuron(sourceLabel);
                            if (n != null)
                                newSourceID = n.id;
                        }
                    }
                }
                if (newSourceID < 0 || newSourceID >= MainWindow.此神经元数组.数组大小 ||
                    newTargetID < 0 || newTargetID >= MainWindow.此神经元数组.数组大小
                    )
                {
                    MessageBox.Show("Neuron outside array boundary!");
                    return;
                }
                cc = 跨语言接口.FindByName(cm, "SynapseWeight");
                float newWeight = 1f;
                if (cc is ComboBox tb2)
                {
                    float.TryParse(tb2.Text, out newWeight);
                    if (weightChanged)
                    {
                        theNeuronArrayView.末尾突触权重 = newWeight;
                        跨语言接口.AddToValues(newWeight, synapseWeightValues);
                    }
                }
                cc = 跨语言接口.FindByName(cm, "Model");
                突触.modelType newModel = 突触.modelType.Fixed;
                if (cc is ComboBox cb0)
                {
                    ListBoxItem lbi = (ListBoxItem)cb0.SelectedItem;
                    newModel = (突触.modelType)System.Enum.Parse(typeof(突触.modelType), lbi.Content.ToString());
                    theNeuronArrayView.末尾突触模型 = newModel;
                }

                if (newSourceID != sourceID || newTargetID != targetID)
                {
                    MainWindow.此神经元数组.设置撤消点();
                    MainWindow.此神经元数组.获取神经元((int)cm.GetValue(SourceIDProperty)).撤销并且删除突触((int)cm.GetValue(TargetIDProperty));
                }
                神经元 nNewSource = MainWindow.此神经元数组.获取神经元(newSourceID);
                nNewSource.撤销与添加突触(newTargetID, newWeight, newModel);
                cm.IsOpen = false;
                MainWindow.Update();
            }
        }
        private static void B0_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button b)
            {
                if (b.Parent is StackPanel sp)
                {
                    if (sp.Parent is MenuItem mi)
                    {
                        if (mi.Parent is ContextMenu cm)
                        {
                            if ((string)b.Content == "Cancel")
                                cmCancelled = true;
                            Cm_Closed(cm, e);
                        }
                    }
                }
            }
        }


        public static void StepAndRepeatSynapse_Click(object sender, RoutedEventArgs e)
        {
            MenuItem mi = sender as MenuItem;
            ContextMenu cm = mi.Parent as ContextMenu;
            theNeuronArrayView.StepAndRepeat(
                (int)cm.GetValue(SourceIDProperty),
                (int)cm.GetValue(TargetIDProperty),
                (float)cm.GetValue(WeightValProperty),
                突触.modelType.Fixed); //TODO: handle hebbian/model
        }

        public static void DeleteSynapse_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.此神经元数组.设置撤消点();
            MenuItem mi = (MenuItem)sender;
            ContextMenu cm = mi.Parent as ContextMenu;
            MainWindow.此神经元数组.获取神经元((int)cm.GetValue(SourceIDProperty)).撤销并且删除突触((int)cm.GetValue(TargetIDProperty));
            cm.IsOpen = false;
            cmCancelled = true;
            MainWindow.Update();
        }

    }
}
