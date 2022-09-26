﻿
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;

namespace CsEngineTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        static NeuronHandler theNeuronArray = null;
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            int neuronCount = 1000000;
            int synapsesPerNeuron = 1000;
            MessageBox.Show("开始数组分配");
            theNeuronArray = new NeuronHandler();
            MessageBox.Show("已删除任何现有阵列");
            theNeuronArray.Initialize(neuronCount);
            MessageBox.Show("数组分配完成");
            int test = theNeuronArray.获取数组大小();
            int threads = theNeuronArray.获取线程数();
            theNeuronArray.设置线程数量(16);
            threads = theNeuronArray.获取线程数();

            theNeuronArray.SetNeuronCurrentCharge设置神经元当前的脉冲(1, 1.4f);
            theNeuronArray.SetNeuronCurrentCharge设置神经元当前的脉冲(2, 0.9f);
            theNeuronArray.Fire(); //should transfer current chargest to last
            float a = theNeuronArray.GetNeuronLastCharge获取神经元上一次的脉冲(1);
            float b = theNeuronArray.GetNeuronLastCharge获取神经元上一次的脉冲(2);

            string s0 = theNeuronArray.获取神经元标签(1);
            theNeuronArray.设置神经元标签(1, "Fred");
            string s1 = theNeuronArray.获取神经元标签(1);
            theNeuronArray.设置神经元标签(1, "George");
            string s2 = theNeuronArray.获取神经元标签(1);

            theNeuronArray.添加突触(2, 4, .75f, 1, false);
            List<Synapse> synapses2 = theNeuronArray.GetSynapsesList(2);
            theNeuronArray.添加突触(1, 2, .5f, 0, false);
            List<Synapse> synapses1 = theNeuronArray.GetSynapsesList(1);
            theNeuronArray.添加突触(1, 3, .6f, 0, false);
            synapses1 = theNeuronArray.GetSynapsesList(1);
            theNeuronArray.添加突触(1, 4, .75f, 1, false);
            synapses1 = theNeuronArray.GetSynapsesList(1);
            theNeuronArray.添加突触(2, 4, .75f, 1, false);
            long count = theNeuronArray.获取总突触数();
            List<Synapse> synapses0 = theNeuronArray.GetSynapsesList(0);
            synapses1 = theNeuronArray.GetSynapsesList(1);
            List<Synapse> synapsesFrom = theNeuronArray.GetSynapsesFromList(4);

            NeuronPartial n = theNeuronArray.GetPartialNeuron(1);
            theNeuronArray.Fire();
            long gen = theNeuronArray.获取次代();
            NeuronPartial n1 = theNeuronArray.GetPartialNeuron(1);
            NeuronPartial n2 = theNeuronArray.GetPartialNeuron(2);
            NeuronPartial n3 = theNeuronArray.GetPartialNeuron(3);
            theNeuronArray.删除突触(1, 3);
            theNeuronArray.删除突触(1, 2);


            MessageBox.Show("分配突触");
            Parallel.For(0, neuronCount, x =>
            {
                //for (int x = 0; x < neuronCount; x++)
                //{
                for (int j = 0; j < synapsesPerNeuron; j++)
                {
                    int target = x + j;
                    if (target >= theNeuronArray.获取数组大小()) target -= theNeuronArray.获取数组大小();
                    if (target < 0) target += theNeuronArray.获取数组大小();
                    theNeuronArray.添加突触(x, target, 1.0f, 0, true);
                }
            });
            for (int i = 0; i < neuronCount / 100; i++)
                theNeuronArray.SetNeuronCurrentCharge设置神经元当前的脉冲(100 * i, 1);
            MessageBox.Show("突触和充电完成");
            Stopwatch sw = new Stopwatch();
            string msg = "";
            for (int i = 0; i < 10; i++)
            {
                sw.Start();
                theNeuronArray.Fire();
                sw.Stop();
                msg += "Gen: " + theNeuronArray.获取次代() + "  FireCount: " + theNeuronArray.获取激活的神经元数量() + " time: " + sw.Elapsed.Milliseconds.ToString() + "\n";
                sw.Reset();
            }
            sw.Stop();
            MessageBox.Show("完成激活10x\n" + msg);
        }
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog
            {
                Filter = "XML Network Files|*.xml",
                Title = "选择大脑模拟器文件"
            };
            // Show the Dialog.  
            // If the user clicked OK in the dialog and  
            Nullable<bool> result = openFileDialog1.ShowDialog();
            if (result ?? false)
            {
                string currentFileName = openFileDialog1.FileName;
                bool loadSuccessful = XmlFile.Load(ref theNeuronArray, currentFileName);
                if (!loadSuccessful)
                {
                    currentFileName = "";
                }
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog
            {
                Filter = "XML Network Files|*.xml",
                Title = "选择大脑模拟器文件"
            };
            // Show the Dialog.  
            // If the user clicked OK in the dialog and  
            Nullable<bool> result = openFileDialog1.ShowDialog();
            if (result ?? false)
            {
                string currentFileName = openFileDialog1.FileName;
                bool loadSuccessful = XmlFile.Save(ref theNeuronArray, currentFileName);
                if (!loadSuccessful)
                {
                    currentFileName = "";
                }
            }

        }
    }
}
