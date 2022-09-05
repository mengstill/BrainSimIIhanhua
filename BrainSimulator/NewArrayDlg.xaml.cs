//
// Copyright (c) Charles Simon. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
//  

using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows.Controls;
using System.Windows.Media;

namespace BrainSimulator
{
    /// <summary>
    /// Interaction logic for NewArray.xaml
    /// </summary>
    public partial class NewArrayDlg : Window
    {
        private const int sizeCount = 1000;
        string crlf = "\r\n";
        public bool returnValue = false;
        ulong approxSynapseSize = 16;
        ulong assumedSynapseCount = 20;
        ulong maxNeurons = 0;
        [ThreadStatic]
        static Random rand = new Random();

        int arraySize;
        bool previousUseNeurons = false;

        public NewArrayDlg()
        {
            InitializeComponent();
            bool previousShowSynapses = false;
            if (MainWindow.此神经元数组 != null)
            {
                previousShowSynapses = MainWindow.此神经元数组.ShowSynapses;
            }

            cbUseServers.IsChecked = MainWindow.useServers;
            buttonSpeedTest.IsEnabled = MainWindow.useServers;
            buttonRefresh.IsEnabled = MainWindow.useServers;
            //textBoxRows.Text = "1000";
            //textBoxColumns.Text = "1000";
            textBoxRows.Text = "15";
            textBoxColumns.Text = "30";

            ulong neuronSize1 = 55;

            ulong availablePhysicalMemory = new Microsoft.VisualBasic.Devices.ComputerInfo().AvailablePhysicalMemory;
            ulong totalPhysicalMemory = new Microsoft.VisualBasic.Devices.ComputerInfo().TotalPhysicalMemory;
            long memoryCurrentlyInUse = GC.GetTotalMemory(true);
            //ulong neuronSize = approxNeuronSize + (approxSynapseSize * assumedSynapseCount);
            ulong neuronSize = neuronSize1 + (approxSynapseSize * assumedSynapseCount);
            maxNeurons = availablePhysicalMemory / neuronSize;

            string text = "";
            text += "总内存: " + totalPhysicalMemory.ToString("##,#") + crlf;
            text += "有效内存: " + availablePhysicalMemory.ToString("##,#") + crlf;
            text += "最大神经元可能数量: " + maxNeurons.ToString("##,#") + crlf;
            text += "假设平均值 " + assumedSynapseCount + " 每个神经元的突触" + crlf;
            textBlock.Text = text;

            previousUseNeurons = MainWindow.useServers;
            cbUseServers.IsChecked = MainWindow.useServers;
            UpdateServerTextBox();
            textBoxColumns.Focus();
        }

        private void UpdateServerTextBox()
        {
            if (cbUseServers.IsChecked == true)
            {
                神经元客户端.GetServerList();
                Thread.Sleep(1000);
                if (神经元客户端.serverList.Count == 0)
                {
                    ServerList.Text = "未检测到服务器";
                    buttonSpeedTest.IsEnabled = false;
                }
                else
                {
                    int.TryParse(textBoxColumns.Text, out cols);
                    int.TryParse(textBoxRows.Text, out rows);
                    ServerList.Text = "";
                    MainWindow.useServers = true;
                    int numServers = 神经元客户端.serverList.Count;
                    int neuronsNeeded = rows * cols;
                    for (int i = 0; i < numServers; i++)
                    {
                        神经元客户端.Server s = 神经元客户端.serverList[i];
                        s.firstNeuron = i * neuronsNeeded / numServers;
                        s.lastNeuron = (i + 1) * neuronsNeeded / numServers;
                        ServerList.Text += s.ipAddress.ToString() + " " + s.name + " " + s.firstNeuron + " " + s.lastNeuron + "\n";
                    }
                    buttonSpeedTest.IsEnabled = true;
                }
            }
            else
            {
                ServerList.Text = "";
            }
        }

        int rows;
        int cols;
        int refractory = 0;
        private void ButtonOK_Click(object sender, RoutedEventArgs e)
        {
            bool previousShowSynapses = false;
            if (MainWindow.此神经元数组 != null)
            {
                previousShowSynapses = MainWindow.此神经元数组.ShowSynapses;
            }
            MainWindow.清除所有模型对话框();
            MainWindow.CloseHistoryWindow();
            MainWindow.CloseNotesWindow();
            MainWindow.神经元数组视图.ClearShowingSynapses();
            if (MainWindow.此神经元数组 != null)
            {
                lock (MainWindow.此神经元数组.Modules)
                {
                    MainWindow.此神经元数组.Modules.Clear();
                }
            }
            MainWindow.神经元数组视图.ClearSelection();
            MainWindow.此神经元数组 = new 神经元数组();

            if (!int.TryParse(textBoxColumns.Text, out cols)) return;
            if (!int.TryParse(textBoxRows.Text, out rows)) return;
            if (cols <= 0) return;
            if (rows <= 0) return;
            //if (checkBoxSynapses.IsChecked == true) doSynapses = true;
            if (!int.TryParse(Refractory.Text, out refractory)) return;

            arraySize = rows * cols;
            //progressBar.Maximum = arraySize;

            //int.TryParse(textBoxSynapses.Text, out synapsesPerNeuron);
            MainWindow.神经元数组视图.Dp.神经元图示大小 = 62;
            MainWindow.神经元数组视图.Dp.DisplayOffset = new Point(0, 0);

            if (MainWindow.useServers && 神经元客户端.serverList.Count > 0)
            {
                //TODO: Replace this with a multicolumn UI
                MainWindow.此神经元数组.初始化(arraySize, rows);
                string[] lines = ServerList.Text.Split('\n');
                神经元客户端.serverList.Clear();
                foreach (string line in lines)
                {
                    if (line == "") continue;
                    string[] command = line.Split(' ');
                    神经元客户端.Server s = new 神经元客户端.Server();
                    s.ipAddress = IPAddress.Parse(command[0]);
                    s.name = command[1];
                    int.TryParse(command[2], out s.firstNeuron);
                    int.TryParse(command[3], out s.lastNeuron);
                    神经元客户端.serverList.Add(s);
                }

                int totalNeuronsInServers = 0;
                for (int i = 0; i < 神经元客户端.serverList.Count; i++)
                    totalNeuronsInServers += 神经元客户端.serverList[i].lastNeuron - 神经元客户端.serverList[i].firstNeuron;
                if (totalNeuronsInServers != arraySize)
                {
                    MessageBox.Show("服务器神经元分配不等于总神经元!");
                    buttonOK.IsEnabled = true;
                    returnValue = false;
                    return;
                }

                神经元客户端.InitServers(0, arraySize);
                神经元客户端.WaitForDoneOnAllServers();
                returnValue = true;
                Close();
                returnValue = true;
            }
            else
            {
                GC.Collect(3, GCCollectionMode.Forced, true);
                MainWindow.此神经元数组.初始化(arraySize, rows);
                MainWindow.此神经元数组.RefractoryDelay = refractory;
         
                MainWindow.此神经元数组.ShowSynapses = previousShowSynapses;
                MainWindow.thisWindow.SetShowSynapsesCheckBox(previousShowSynapses);
                Close();
                returnValue = true;
            }
            MainWindow.此神经元数组.加载完成 = true;
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.useServers = previousUseNeurons;
            this.Close();
        }

 
        private void Button_Refresh(object sender, RoutedEventArgs e)
        {
            UpdateServerTextBox();
        }

        //PING speed test
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string s = ServerList.SelectedText;
            if (!IPAddress.TryParse(s, out IPAddress targetIp))
            {
                MessageBox.Show("突出显示 IP 地址");
                return;
            }
            神经元客户端.pingCount = 0;
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            string payload = 神经元客户端.CreatePayload(1450);
            for (int i = 0; i < 100000; i++)
            {
                神经元客户端.SendToServer(targetIp, "Ping");
            }
            sw.Stop();
            double packetSendNoPayload = ((double)sw.ElapsedMilliseconds) / 100000.0;
            Thread.Sleep(1000);

            sw.Start();
            for (int i = 0; i < 100000; i++)
            {
                神经元客户端.SendToServer(targetIp, "Ping " + payload);
            }
            sw.Stop();
            double packetSendBigPayload = ((double)sw.ElapsedMilliseconds) / 100000.0;
            Thread.Sleep(1000);

            List<long> rawData = new List<long>();
            for (int i = 0; i < 1000; i++)
                rawData.Add(神经元客户端.Ping(targetIp, ""));
            double latencyNoPayload = ((double)rawData.Average()) / 10000.0;
            rawData.Clear();
            for (int i = 0; i < 1000; i++)
                rawData.Add(神经元客户端.Ping(targetIp, payload));
            double latencyBigPayload = ((double)rawData.Average()) / 10000.0;

            PingLabel.Content = "包速度: " + packetSendNoPayload.ToString("F4") + "ms-" + packetSendBigPayload.ToString("F4") + "ms  R/T 延迟:  "
                + latencyNoPayload.ToString("F4") + "ms-" + latencyBigPayload.ToString("F4") + "ms " + 神经元客户端.pingCount;
            PingLabel1.Visibility = Visibility.Visible;
        }

        private void CheckBoxUseServers_Checked(object sender, RoutedEventArgs e)
        {
            神经元客户端.Init();
            UpdateServerTextBox();
            if (神经元客户端.serverList.Count > 0)
                MainWindow.useServers = true;
            buttonRefresh.IsEnabled = true; ;
        }

        private void CheckBoxUseServers_Unchecked(object sender, RoutedEventArgs e)
        {
            MainWindow.useServers = false;
            buttonRefresh.IsEnabled = MainWindow.useServers;
            UpdateServerTextBox();
        }


        //engine Refractory up/dn  buttons 
        private void Button_RefractoryUpClick(object sender, RoutedEventArgs e)
        {
            refractory++;
            Refractory.Text = refractory.ToString();
        }

        private void Button_RefractoryDnClick(object sender, RoutedEventArgs e)
        {
            refractory--;
            if (refractory < 0) refractory = 0;
            Refractory.Text = refractory.ToString();
        }

        private void textBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (textBoxColumns == null || textBoxRows == null || LabelNeuronCount == null) return;
            if (sender is TextBox tb)
            {
                if (!int.TryParse(tb.Text, out int value) || value <= 0)
                    tb.Background = new SolidColorBrush(Colors.Pink);
                else
                    tb.Background = new SolidColorBrush(Colors.LightGreen);
            }
            if (int.TryParse(textBoxColumns.Text, out int cols) &&
                int.TryParse(textBoxRows.Text, out int rows))
            {
                long total = (long)rows * (long)cols;
                LabelNeuronCount.Content = "神经元总数: " + total.ToString("##,#");
                if ((ulong)(rows * cols) > maxNeurons)
                {
                    LabelNeuronCount.Content = "神经元总数 > 估计最大值!";
                    textBoxColumns.Background = new SolidColorBrush(Colors.Red);
                    textBoxRows.Background = new SolidColorBrush(Colors.Red);
                }
                else
                {
                    textBoxColumns.Background = new SolidColorBrush(Colors.LightGreen);
                    textBoxRows.Background = new SolidColorBrush(Colors.LightGreen);
                }
            }
            else
                LabelNeuronCount.Content = "神经元计数：错误";
        }

        private void textBoxColumns_GotFocus(object sender, RoutedEventArgs e)
        {
            textBoxColumns.SelectAll();
        }

        private void textBoxRows_GotFocus(object sender, RoutedEventArgs e)
        {
            textBoxRows.SelectAll();
        }
    }
}
