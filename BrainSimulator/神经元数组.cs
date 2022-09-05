//
// Copyright (c) Charles Simon. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
//  

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Xml.Serialization;

namespace BrainSimulator
{
    /// <summary>
    /// 神经元数组
    /// </summary>
    public partial class NeuronArray : 神经元处理
    {
        /// <summary>
        /// 网络节点
        /// </summary>
        public string networkNotes = "";
        /// <summary>
        /// 隐藏节点
        /// </summary>
        public bool hideNotes = false;
        public long Generation = 0;
        /// <summary>
        /// 引擎速度
        /// </summary>
        public int EngineSpeed = 250;
        /// <summary>
        /// 引擎是否暂停
        /// </summary>
        public bool EngineIsPaused = false;
        /// <summary>
        /// 数组大小
        /// </summary>
        public int arraySize;
        /// <summary>
        /// 行数
        /// </summary>
        public int rows;

        public int lastFireCount = 0;
        internal List<模块视图> 模块 = new List<模块视图>();
        public 显示参数 displayParams;

        //these have nothing to do with the 神经元数组 but are here so it will be saved and restored with the network
        //这些与 神经元数组 无关，但在此处，因此将通过网络保存和恢复
        private bool showSynapses = false;
        public bool ShowSynapses
        {
            get => showSynapses;
            set => showSynapses = value;
        }
        public int Cols { get => arraySize / rows; }
        private bool loadComplete = false;
        [XmlIgnore]
        public bool 加载完成 { get => loadComplete; set => loadComplete = value; }
        /// <summary>
        /// 模型
        /// </summary>
        public List<模块视图> Modules
        {
            get { return 模块; }
        }

        private Dictionary<int, string> 标签缓存 = new Dictionary<int, string>();
        public void 向缓存中添加标签(int nID, string label)
        {
            try
            {
                标签缓存.Add(nID, label);
            }
            catch
            {
                标签缓存[nID] = label;
            }
        }
        public void 从缓存中移除标签(int nID)
        {
            try
            {
                标签缓存.Remove(nID);
            }
            catch { };
        }
        public string 从缓存中获取标签(int nID)
        {
            if (标签缓存.ContainsKey(nID))
                return 标签缓存[nID];
            else
                return "";
        }

        public void 清理标签缓存()
        {
            标签缓存.Clear();
        }

        public List<string> 从缓存标签中获取值()
        {
            return 标签缓存.Values.ToList();
        }
        public List<int> 从缓存标签中获取键()
        {
            return 标签缓存.Keys.ToList();
        }


        private int refractoryDelay = 0;
        public int RefractoryDelay
        { get => refractoryDelay; set { refractoryDelay = value; SetRefractoryDelay(refractoryDelay); } }

        public void 初始化(int count, int in行数, bool clipBoard = false)
        {
            rows = in行数;
            arraySize = count;
            清理标签缓存();
            if (!MainWindow.useServers || clipBoard)
                base.Initialize(count);
            else
            {
                神经元客户端.初始化服务器(0, count);
            }
        }

        public NeuronArray()
        {
        }

        public 神经元 获取神经元(int id, bool fromClipboard = false)
        {
            神经元 n = GetCompleteNeuron(id, fromClipboard);
            return n;
        }
        public 神经元 GetNeuron(string label)
        {
            if (标签缓存.ContainsValue(label))
            {
                int nID = 标签缓存.FirstOrDefault(x => x.Value == label).Key;
                return 获取神经元(nID);
            }
            else
            {
                string searchKey = label + 神经元.toolTipSeparator;
                int nID = 标签缓存.FirstOrDefault(x => x.Value.StartsWith(searchKey)).Key;
                if (nID != 0)
                    return 获取神经元(nID);
            }
            return null;
        }


        public void GetCounts(out long synapseCount, out int useCount)
        {
            if (MainWindow.useServers)
            {
                useCount = 神经元客户端.服务列表.Sum(x => x.neuronsInUse);
                synapseCount = 神经元客户端.服务列表.Sum(x => x.totalSynapses);
            }
            else
            {
                synapseCount = GetTotalSynapses();
                useCount = GetTotalNeuronsInUse();
            }
        }
        /// <summary>
        /// 脉冲,此处网络运行
        /// </summary>
        public new void Fire()
        {
            if (MainWindow.useServers)
            {
                神经元客户端.Fire();
                lastFireCount = 0;
                for (int i = 0; i < 神经元客户端.服务列表.Count; i++)
                    lastFireCount += 神经元客户端.服务列表[i].firedCount;
                Generation = 神经元客户端.服务列表[0].generation;
            }
            else
            {
                base.Fire();
                Generation = GetGeneration();
                lastFireCount = GetFiredCount();
            }
            处理程序动作();
            神经冲动历史.更新神经脉冲历史();
        }
        public void 添加突触(int src, int dest, float weight, 突触.modelType model, bool noBackPtr)
        {
            if (MainWindow.useServers && this == MainWindow.此神经元数组)
                神经元客户端.添加突触(src, dest, weight, model, noBackPtr);
            else
                base.AddSynapse(src, dest, weight, (int)model, noBackPtr);
        }
        new public void DeleteSynapse(int src, int dest)
        {
            if (MainWindow.useServers && this == MainWindow.此神经元数组)
                神经元客户端.删除突触(src, dest);
            else
                base.DeleteSynapse(src, dest);
        }

        //fires all the modules
        //触发所有模块
        private void 处理程序动作()
        {
            int badModule = -1;
            string message = "";
            lock (模块)
            {
                List<int> firstNeurons = new List<int>();
                for (int i = 0; i < 模块.Count; i++)
                    firstNeurons.Add(模块[i].FirstNeuron);
                firstNeurons.Sort();

                for (int i = 0; i < 模块.Count; i++)
                {
                    模块视图 na = 模块.Find(x => x.FirstNeuron == firstNeurons[i]);
                    if (na.TheModule != null)
                    {
                        //if not in debug mode, trap exceptions and offer to delete module
                        //如果不在调试模式下，则捕获异常并提供删除模块
#if !DEBUG
                        try
                        {
                            if (na.TheModule.isEnabled)
                                na.TheModule.Fire();
                        }
                        catch (Exception e)
                        {
                            // Get stack trace for the exception with source file information
                            var st = new StackTrace(e, true);
                            // Get the top stack frame
                            var frame = st.GetFrame(0);
                            // Get the line number from the stack frame
                            var line = frame.GetFileLineNumber();

                            message = "Module " + na.Label + " threw an unhandled exception with the message:\n" + e.Message;
                            message += "\nat line " + line;
                            message += "\n\n Would you like to remove it from this network?";
                            badModule = i;
                        }
#else
                        if (na.TheModule.isEnabled)
                            na.TheModule.Fire();
#endif
                    }
                }
            }
            if (message != "")
            {
                MessageBoxResult mbr = MessageBox.Show(message, "Remove Module?", MessageBoxButton.YesNo);
                if (mbr == MessageBoxResult.Yes)
                {
                    模块视图.删除模块(badModule);
                    MainWindow.Update();
                }
            }
        }

        public 模块视图 FindModuleByLabel(string label)
        {
            模块视图 moduleView = 模块.Find(na => na.Label.Trim() == label);
            if (moduleView == null)
            {
                if (label.StartsWith("Module"))
                {
                    label = label.Replace("Module", "");
                    moduleView = 模块.Find(na => na.Label.Trim() == label);
                }
            }
            return moduleView;
        }

        public void SetNeuron(int i, 神经元 n)
        {
            设置所有神经元(n);
        }


        public int 获取神经元索引(int x, int y)
        {
            return x * rows + y;
        }

        public 神经元 GetNeuron(int x, int y)
        {
            return 获取神经元(获取神经元索引(x, y));
        }

        public void 获取神经元位置(int index, out int x, out int y)
        {
            x = index / rows;
            y = index % rows;
        }

    }
}
