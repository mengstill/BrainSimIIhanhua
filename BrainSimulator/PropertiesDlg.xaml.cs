//
// Copyright (c) Charles Simon. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
//  

using System.Linq;
using System.Threading;
using System.Windows;

namespace BrainSimulator
{
    /// <summary>
    /// Interaction logic for PropertiesDlg.xaml
    /// </summary>
    public partial class PropertiesDlg : Window
    {
        public PropertiesDlg()
        {
            InitializeComponent();
            if (MainWindow.数组是否为空())
            { Close(); return; }

            txtFileName.Text = MainWindow.currentFileName;
            txtFileName.ToolTip = MainWindow.currentFileName;
            txtRows.Text = MainWindow.此神经元数组.rows.ToString("N0");
            txtColumns.Text = (MainWindow.此神经元数组.arraySize / MainWindow.此神经元数组.rows).ToString("N0");
            txtNeurons.Text = MainWindow.此神经元数组.arraySize.ToString("N0");
            if (MainWindow.useServers)
            {
                神经元客户端.获取服务列表();
                Thread.Sleep(1000);
                txtNeuronsInUse.Text = 神经元客户端.服务列表.Sum(x => x.neuronsInUse).ToString("N0");
                txtSynapses.Text = 神经元客户端.服务列表.Sum(x => x.totalSynapses).ToString("N0");
            }
            else
            {
                MainWindow.此神经元数组.GetCounts(out long synapseCount, out int neuronInUseCount);
                txtNeuronsInUse.Text = neuronInUseCount.ToString("N0");
                txtSynapses.Text = synapseCount.ToString("N0");
            }
            //            Owner = MainWindow.thisWindow;
        }

        private void btnDialogOk_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.暂停引擎();
            int.TryParse(txtColumns.Text, out int newCols);
            int.TryParse(txtRows.Text, out int newRows);
            int oldCols = MainWindow.此神经元数组.arraySize / MainWindow.此神经元数组.rows;
            int oldRows = MainWindow.此神经元数组.rows;
            if (newCols < oldCols || newRows < oldRows)
            {
                MessageBox.Show("只能让神经元阵列变大.");
                return;
            }
            if (newCols != oldCols || newRows != oldRows)
            {
                MainWindow.神经元数组视图.ClearSelection();
                选择矩阵 rr = new 选择矩阵(0, oldCols, oldRows);
                MainWindow.神经元数组视图.theSelection.selectedRectangles.Add(rr);
                MainWindow.神经元数组视图.CopyNeurons();
                MainWindow.神经元数组视图.ClearSelection();
                MainWindow.此神经元数组 = new NeuronArray();
                MainWindow.此神经元数组.初始化(newRows * newCols, newRows);
                MainWindow.此神经元数组.rows = newRows;
                MainWindow.神经元数组视图.targetNeuronIndex = 0;
                MainWindow.神经元数组视图.PasteNeurons();
                MainWindow.此神经元数组.ShowSynapses = true;
                MainWindow.此神经元数组.加载完成 = true;
                MainWindow.thisWindow.SetShowSynapsesCheckBox(true);
                MainWindow.神经元数组视图.ClearShowingSynapses();
                神经冲动历史.移除所有();
                MainWindow.关闭历史窗口();
            }
            this.Close();
            MainWindow.恢复引擎();
        }
        private void btnDialogCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
