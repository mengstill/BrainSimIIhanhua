//
// Copyright (c) Charles Simon. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
//  

using System;
using System.Collections.Generic;
using System.Windows;
using System.Xml.Serialization;

namespace BrainSimulator
{
    public class 神经元参数
    {
        public string Label { get; set; }
        public 神经元.模型类 Model { get; set; }
        public float LeakRate { get; set; }
        public int AxonDelay { get; set; }
        public float LastCharge { get; set; }
        public int LastChargeInt { get; set; }
        public bool ShowSynapses { get; set; }
        public bool RecordHistory { get; set; }
        public List<突触参数> Synapses { get; set; } = new List<突触参数>();

        public 神经元参数() { }
        public 神经元参数(神经元 神经元)
        {
            Label = 神经元.标签名;
            Model = 神经元.模型字段;
            LeakRate = 神经元.泄露率属性;
            AxonDelay = 神经元.突触延迟;
            LastCharge = 神经元.LastCharge;
            LastChargeInt = 神经元.LastChargeInt;
            ShowSynapses = 神经元.显示突触;
            RecordHistory = 神经元.RecordHistory;
            foreach(突触 突触 in 神经元.synapses)
            {
                Synapses.Add(突触.转突触参数());
            }


        }
        public void 神经元初始化(神经元 神经元)
        {
            神经元.标签名 = Label;
            神经元.模型字段 = Model;
            神经元.泄露率属性 = LeakRate;
            神经元.突触延迟 = AxonDelay;
            if(神经元.模型字段 != 神经元.模型类.Color)
            {
                神经元.LastCharge = LastCharge;
                神经元.currentCharge = LastCharge;
            }
            else
            {
                神经元.LastChargeInt = LastChargeInt;
                神经元.currentCharge = LastChargeInt;
            }
            神经元.显示突触 = ShowSynapses;
            神经元.RecordHistory = RecordHistory;
            foreach(突触参数 参数 in Synapses)
            {
                突触 s = new 突触();
                s.目标神经元字段=参数.TargetNeuron;
                s.权重字段 = 参数.Weight;
                if (参数.IsHebbian) s.模型字段 = 突触.模型类型.Hebbian1;
                else s.模型字段 = 参数.Model;
                神经元.添加突触(s.神经元, s.权重字段, s.模型字段);
            }
        }
    }

    public class 神经元/* : 神经元结构*/
    {
        public int id = -1;
        public bool 是否使用;
        public float lastCharge;
        public float currentCharge;
        //public float 泄露率 = 0.1f; //仅用于LIF模型

        public int 突触延迟 = 0;
        int 突触计数 = 0;
        public 神经元.模型类 模型字段;
        public int dummy; //TODO :Don't know why this is here, it is not required for alignment
                          //TODO : 不知道为什么会在这里，不需要对齐
        int nextFiring = 0; //仅用于随机模型和连续模型
        public long lastFired;

        const float threshold = 1.0f;

        public object 锁=new object();

        public string 标签 = "";

        public List<突触> synapses = new List<突触>();
        public List<突触> synapsesFrom = new List<突触>();
        NeuronArray 所有者数组 = MainWindow.此神经元数组;

        //这仅在 NeuronView 中使用，但在这里，您可以在添加神经元类型时添加工具提示
        //工具提示将自动出现在神经元类型选择器组合框中
        public static string[] modelToolTip = { "Integrate & Fire",
            "RGB value (no processing)",
            "Float value (no procesing",
            "Leaky Integrate & Fire",
            "Fires at random intervals",
            "Fires a burst,",
            "Always fire",
        };

        public NeuronArray 所有者 { get => 所有者数组; set => 所有者数组 = value; }

        //IMPORTANT:
        //Lastcharge is a stable readable value of the output of a neuron
        //CurrentCharge is an internal accumulation variable which must be set for the engine to act on a neuron's value
        //"Color" neurons and "FloatValue" neurons are special in that last and current should always be the same.langu
        /// <summary>
        /// 当前更改
        /// </summary>
        public float CurrentCharge
        {
            get { return (float)lastCharge; }
            set { currentCharge = value; }
        }

        //get/set last charge. Setting also sets current charge

        //TODO: change this to SetValue
        /// <summary>
        /// 最后更改
        /// </summary>
        public float LastCharge { get { return (float)lastCharge; } set { lastCharge = value; } }// Update(); } }

        //get/set last charge as raw integer Used by COLOR nueurons
        /// <summary>
        /// 最后更改int
        /// </summary>
        public int LastChargeInt { get { return (int)lastCharge; } set { lastCharge = value; 更新(); } }
        public void SetValueInt(int value) { LastChargeInt = value; 更新(); }

        /// <summary>
        /// 突触列表
        /// </summary>
        public List<突触> 突触列表 { get => synapses; }
        /// <summary>
        /// 突触来源列表
        /// </summary>
        [XmlIgnore]
        public List<突触> 突触来源列表 { get => synapsesFrom; }

        public long LastFired { get => lastFired; }

        public bool Fired() { return (LastCharge >= 1); }
        //public void SetValue(float value) { lastCharge = value; currentCharge = value; Update(); }
        public void SetValue(float value)
        {
            currentCharge = value;
            if (模型字段 == 模型类.FloatValue)
                lastCharge = value;
            更新();
        }

        public enum 模型类 { IF, Color, FloatValue, LIF, Random, Burst, Always };

        public int Id { get => id; set => id = value; }

        public const string toolTipSeparator = "||?";
        public string 标签名
        {
            get
            {
                string theLabel = 所有者数组.从缓存中获取标签(Id);
                if (theLabel == null) theLabel = "";
                if (theLabel != "")
                {
                    int tooltipStart = theLabel.IndexOf(toolTipSeparator);
                    if (tooltipStart != -1)
                        theLabel = theLabel.Substring(0, tooltipStart);
                }
                return theLabel;
            }
            set
            {
                string theLabel = 所有者数组.从缓存中获取标签(Id);
                int tooltipStart = theLabel.IndexOf(toolTipSeparator);
                if (tooltipStart != -1)
                    标签 = value + toolTipSeparator + theLabel.Substring(tooltipStart + toolTipSeparator.Length);
                else
                    标签 = value;

                if (标签 == toolTipSeparator) 标签 = "";
                if (标签 != "")
                    所有者数组.向缓存中添加标签(Id, 标签);
                else
                    所有者数组.从缓存中移除标签(Id);

                //更改标签会强制神经元在屏幕上更新，即使它没有被使用
                if (标签 != "")
                {
                    MainWindow.Update();
                }
            }
        }
        public string ToolTip
        {
            get
            {
                string theToolTip = 所有者数组.从缓存中获取标签(Id);
                if (theToolTip == null) theToolTip = "";
                int tooltipStart = theToolTip.IndexOf(toolTipSeparator);
                if (tooltipStart != -1)
                    theToolTip = theToolTip.Substring(tooltipStart + toolTipSeparator.Length);
                else
                    theToolTip = "";
                return theToolTip;
            }
            set
            {
                string theToolTip = 所有者数组.从缓存中获取标签(Id);
                string theLabel = theToolTip;
                if (theLabel.Contains(toolTipSeparator))
                    theLabel = theLabel.Substring(0, theLabel.IndexOf(toolTipSeparator));

                if (value != "")
                    标签 = theLabel + toolTipSeparator + value;
                else
                    标签 = theLabel;

                if (标签 == toolTipSeparator) 标签 = "";
                if (标签 != "")
                    所有者数组.向缓存中添加标签(Id, 标签);
                else
                    所有者数组.从缓存中移除标签(Id);
            }
        }

        public bool RecordHistory
        {
            get
            {
                return 神经冲动历史.神经元是否在神经脉冲历史中(id);
            }
            set
            {
                if (value)
                {
                    神经冲动历史.添加神经元进历史窗口(id);
                    神经元视图.打开历史窗口();
                }
                else
                    神经冲动历史.从历史窗口移除神经元(id);
            }
        }

        public bool 显示突触
        {
            get
            {
                return MainWindow.神经元数组视图.IsShowingSynapses(id);
            }
            set
            {
                if (value)
                    MainWindow.神经元数组视图.添加突触显示(id);
                else
                    MainWindow.神经元数组视图.移除突触显示(id);
            }
        }
        public float 泄露率;
        public float 泄露率属性 { get => 泄露率; set { 泄露率 = value; 更新(); } }
        public int AxonDelay
        {
            get => 突触延迟;
            set { 突触延迟 = value; 更新(); }
        }

        public 模型类 模型 { get => (神经元.模型类)模型字段; set { 模型字段 = (模型类)value; 更新(); } }


        public 神经元(int id)
        {
            泄露率 = 0.1f;
            nextFiring = 0;
            this.id = id;
        }
        public void 更新()
        {
            所有者数组.设置所有神经元(this);
        }
        public 神经元()
        {
            if (所有者数组 == null)
                所有者数组 = MainWindow.myClipBoard;
        }

        //一个神经元被定义为正在使用，如果它有任何连接到它的突触或者它有一个标签
        public bool InUse()
        {
            return ((突触列表 != null && 突触列表.Count != 0) || (突触来源列表 != null && 突触来源列表.Count != 0) || 标签名 != "");
        }

        public void Reset()
        {
            标签名 = "";
            模型字段 = 模型类.IF;
            SetValue(0);
        }

        public void 添加突触(神经元 n, float weight, 突触.模型类型 model, bool noBackPtr=false)
        {
            lock (锁) 
            {
                突触 s1 = new 突触();
                s1.权重=weight;
                s1.设置目标神经元(n);
                s1.模型字段 = model;

                if (synapses.Count == 0)
                {
                    synapses = new();
                    synapses.Capacity = 100;
                }
                for (int i = 0; i < synapses.Count; i++)
                {
                    if (synapses[i].获取目标神经元() == n)
                    {
                        //update an existing synapse
                        synapses[i].权重 = weight;
                        synapses[i].模型字段 = model;
                        goto alreadyInList;
                    }
                }
                //else create a new synapse
                synapses.Add(s1);
            alreadyInList:;
            }

            if (noBackPtr) return;

            //now add the synapsesFrom entry to the target neuron
            //this requires locking because multiply neurons may link to a single neuron simultaneously requiring backpointers.
            //The previous does not lock because you don't write to the same neuron from multiple threads
            lock (锁)
            {
                突触 s2 = new();
                s2.权重 = weight;
                s2.设置目标神经元(n);
                s2.模型字段 = model;

                if (n.synapsesFrom.Count == 0)
                {
                    n.synapsesFrom = new();
                    n.synapsesFrom.Capacity=10;
                }
                for (int i = 0; i < n.synapsesFrom.Count; i++)
                {
                    突触 s = n.synapsesFrom[i];
                    if (n.synapsesFrom[i].获取目标神经元() == this)
                    {
                        n.synapsesFrom[i].权重 = weight;
                        n.synapsesFrom[i].模型字段 = model;
                        goto alreadyInList2;
                    }
                }
                n.synapsesFrom.Add(s2);
            alreadyInList2:;
            }
            return;
        }
        public void 添加突触(int targetNeuron, float weight)
        {
            所有者数组.添加突触(Id, targetNeuron, weight, 突触.模型类型.Fixed, false);
        }

        public void 撤销与添加突触(int targetNeuron, float weight, 突触.模型类型 model = 突触.模型类型.Fixed)
        {
            //TODO，先检查一下突触是否已经存在，保存旧的权重
            突触 s = 查找突触(targetNeuron);
            if (s != null)
                MainWindow.此神经元数组.添加突触撤销(Id, targetNeuron, s.权重字段, s.模型字段, false, false);
            else
                MainWindow.此神经元数组.添加突触撤销(Id, targetNeuron, 0, 突触.模型类型.Fixed, true, false);

            所有者数组.添加突触(Id, targetNeuron, weight, model, false);

        }

        public void 添加撤销信息()
        {
            MainWindow.此神经元数组.添加神经元撤销(this);
        }

        public void 删除所有突触(bool deleteOutgoing = true, bool deleteIncoming = true)
        {
            if (deleteOutgoing)
            {
                foreach (突触 s in 突触列表)
                    删除突触(s.获取目标神经元());
                突触列表.Clear();
            }
            if (deleteIncoming)
            {
                foreach (突触 s in synapsesFrom)
                {
                    所有者数组.DeleteSynapse(s.目标神经元字段, id);
                }
                synapsesFrom.Clear();
            }
        }

        public override string ToString()
        {
            return "n:" + Id;
        }
        public void 撤销并且删除突触(int targetNeuron)
        {
            突触 s = 查找突触(targetNeuron);
            if (s != null)
                MainWindow.此神经元数组.添加突触撤销(id, targetNeuron, s.权重字段, s.模型字段, false, true);

            删除突触(神经元数组base.神经元数组[targetNeuron]);
        }
        public void 删除突触(神经元 n)
        {
            lock (锁)
            {
                if (synapses.Count != 0)
                {
                    for (int i = 0; i < synapses.Count; i++)
                    {
                        if (synapses[i].获取目标神经元() == n)
                        {
                            synapses.Remove(synapses[i]);
                            break;
                        }
                    }
                }
                
            }
            //if (((long long)n >> 63) != 0) return;
            lock (n.锁)
            {
                if (n.synapsesFrom.Count != 0)
                {
                    for (int i = 0; i < n.synapsesFrom.Count; i++)
                    {
                        突触 s = n.synapsesFrom[i];
                        if (s.获取目标神经元() == this)
                        {
                            n.synapsesFrom.Remove(n.synapsesFrom[i]);
                            break;
                        }
                    }
                }
            }
        }



        public 突触 查找突触(int targetNeuron)
        {
            if (突触列表 == null) return null;
            for (int i = 0; i < 突触列表.Count; i++)
            {
                if (((突触)突触列表[i]).目前神经元 == targetNeuron)
                    return (突触)突触列表[i];
            }
            return null;
        }
        public 突触 查找突触来源列表(int fromNeuron)
        {
            if (突触来源列表 == null) return null;
            for (int i = 0; i < 突触来源列表.Count; i++)
            {
                if (((突触)突触来源列表[i]).目前神经元 == fromNeuron)
                    return (突触)突触来源列表[i];
            }
            return null;
        }

        public 神经元 Clone()
        {
            神经元 n = (神经元)this.MemberwiseClone();
            n.标签 = 标签名;
            n.synapses = new List<突触>();
            n.synapsesFrom = new List<突触>();
            n.RecordHistory = RecordHistory;
            n.显示突触 = 显示突触;
            return n;
        }
        //将此内容复制到 n
        public void 复制(神经元 n)
        {
            n.标签名 = this.标签名;
            n.ToolTip = this.ToolTip;
            n.lastCharge = this.lastCharge;
            n.currentCharge = this.currentCharge;
            n.泄露率属性 = this.泄露率属性;
            n.突触延迟 = this.突触延迟;
            n.模型字段 = this.模型字段;
            n.RecordHistory = this.RecordHistory;
            n.显示突触 = this.显示突触;
            n.synapsesFrom = new List<突触>(); ;
        }
        public 神经元 复制()
        {
            神经元 n = new 神经元();
            n.id = this.id;
            n.标签 = this.标签;
            n.lastCharge = this.lastCharge;
            n.currentCharge = this.currentCharge;
            n.泄露率属性 = this.泄露率属性;
            n.突触延迟 = this.突触延迟;
            n.模型字段 = this.模型字段;
            n.RecordHistory = this.RecordHistory;
            n.显示突触 = this.显示突触;
            return n;
        }
        public void 撤销并清空()
        {
            MainWindow.此神经元数组.添加神经元撤销(this);
            for (int i = 0; i < synapses.Count; i++)
            {
                撤销并且删除突触(synapses[i].目标神经元字段);
            }
            for (int i = 0; i < synapsesFrom.Count; i++)
            {
                MainWindow.此神经元数组.获取神经元(synapsesFrom[i].目标神经元字段).
                    撤销并且删除突触(this.id);
            }
            清空();
        }
        public void 清空()
        {
            标签名 = "";
            ToolTip = "";
            currentCharge = 0;
            lastCharge = 0;
            模型字段 = 模型类.IF;
            泄露率属性 = 0.1f;
            AxonDelay = 0;
            删除所有突触();
            MainWindow.此神经元数组.设置所有神经元(this);
            synapses = new List<突触>();
            synapsesFrom = new List<突触>();
            RecordHistory = false;
            显示突触 = false;
        }

        public 神经元参数 转神经元参数()
        {
            return new 神经元参数();
        }

        public void AddToCurrentValue(float weight)
        {
            currentCharge = currentCharge + weight;
            if (currentCharge >= threshold)
                神经元数组base.添加神经元到激活列表组(id);

        }

        public List<突触> 获取synapses()
        {
            return synapses;
        }

        public List<突触> GetSynapsesFrom()
        {
            return synapsesFrom;
        }

        public void AddSynapseFrom(神经元 n, float weight, 突触.模型类型 model)
        {
            lock (锁)
            {
                突触 s1 = new 突触();
                s1.权重 = weight;
                s1.设置目标神经元(n);
                s1.模型字段 = model;

                if (synapsesFrom.Count == 0)
                {
                }
                for (int i = 0; i < synapsesFrom.Count; i++)
                {
                    if (synapsesFrom[i].获取目标神经元() == n)
                    {
                        //update an existing synapse
                        synapsesFrom[i].权重 = weight;
                        synapsesFrom[i].模型字段 = model;
                        goto alreadyInList;
                    }
                }
                //else create a new synapse
                synapsesFrom.Add(s1);
            }
        alreadyInList:;
        }
        static double n2 = 0.0;
        static int n2_cached = 0;
        static Random 随机器 = new();
        double rand_normal(double mean, double stddev)
        {

            if (n2_cached<=0)
            {
                double x, y, r;
                do
                {
                    x = 2.0 * 随机器.NextDouble() - 1;
                    y = 2.0 * 随机器.NextDouble() - 1;

                    r = x * x + y * y;
                } while (r == 0.0 || r > 1.0);
                {
                    double d = Math.Sqrt(-2.0 * Math.Log(r) / r);
                    double n1 = x * d;
                    n2 = y * d;
                    double result = n1 * stddev + mean;
                    n2_cached = 1;
                    return result;
                }
            }
            else
            {
                n2_cached = 0;
                return n2 * stddev + mean;
            }
        }
        public bool Fire1(long cycle)
        {
            if (泄露率属性 < 0f) return false;
            if (模型 == 模型类.Color)
            {
                神经元数组base.添加神经元到激活列表组(id);
                return true;
            }
            //if (model == modelType::FloatValue) return false;
            if (模型 == 模型类.Always)
            {
                nextFiring--;
                if (泄露率属性 >= 0 && nextFiring <= 0) //泄漏率是标准偏差
                {
                    currentCharge = currentCharge + threshold;
                }
                if (泄露率属性 >= 0) //负泄漏率表示“禁用”
                    神经元数组base.添加神经元到激活列表组(id);
            }
            if (模型 == 模型类.Random)
            {
                nextFiring--;
                if (泄露率属性 >= 0 && nextFiring <= 0) //泄漏率是标准偏差
                {
                    currentCharge = currentCharge + threshold;
                }
                if (泄露率属性 >= 0) //负泄漏率表示“禁用”
                    神经元数组base.添加神经元到激活列表组(id);
            }
            if (模型 == 模型类.Burst)
            {
                if (currentCharge < 0)
                {
                    突触计数 = 0;
                }
                //force internal firing
                if (突触计数 > 0)
                {
                    nextFiring--;
                    if (nextFiring <= 0) //Firing Rate
                    {
                        突触计数--;
                        currentCharge = currentCharge + threshold;
                        if (突触计数 > 0)
                            nextFiring = (int)泄露率属性;
                    }
                    神经元数组base.添加神经元到激活列表组(id);
                }
                else if (突触计数 == 0) 突触计数--;
            }

            //code to implement a refractory period
            if (cycle < lastFired + 神经元数组base.refractoryDelay)
            {
                currentCharge = 0;
                神经元数组base.添加神经元到激活列表组(id);
            }

            //check for firing
            if (模型 != 模型类.FloatValue && currentCharge < 0) currentCharge = 0;
            if (currentCharge != lastCharge)
            {
                lastCharge = currentCharge;
                神经元数组base.添加神经元到激活列表组(id);
            }

            if (模型 == 模型类.LIF && 突触计数 != 0)
            {
                突触计数 = 突触计数 >> 1;
                神经元数组base.添加神经元到激活列表组(id);
                if ((突触计数 & 0x001) != 0)
                {
                    return true;
                }
            }

            if (currentCharge >= threshold)
            {
                if (模型 == 模型类.LIF && 突触延迟 != 0)
                {
                    突触计数 |= (1 << 突触延迟);
                    lastFired = cycle;
                    currentCharge = 0;
                    神经元数组base.添加神经元到激活列表组(id);
                    return false;
                }
                if (模型 == 模型类.Burst && 突触计数 < 0)
                {
                    nextFiring = (int)泄露率属性;
                    if (nextFiring < 1) nextFiring = 1;
                    突触计数 = 突触延迟 - 1;
                }
                if (模型 == 模型类.Always)
                {
                    nextFiring = 突触延迟;
                }
                if (模型 == 模型类.Random)
                {
                    double newNormal = rand_normal((double)突触延迟, (double)泄露率属性);
                    if (newNormal < 1) newNormal = 1;
                    nextFiring = (int)newNormal;
                }
                if (模型 != 模型类.FloatValue)
                    currentCharge = 0;
                lastFired = cycle;
                return true;
            }
            if (模型 == 模型类.LIF)
            {
                currentCharge = currentCharge * (1 - 泄露率属性);
                神经元数组base.添加神经元到激活列表组(id);
            }
            return false;
        }
        public void Fire2()
        {
            if (模型 == 模型类.FloatValue) return;
            if (模型 == 模型类.Color && lastCharge != 0)
                return;
            else if (模型 != 模型类.Color && lastCharge < threshold && (突触计数 & 0x1) == 0)
                return; //did the neuron fire?
            神经元数组base.添加神经元到激活列表组(id);
            if (synapses != null)
            {
                lock (锁)
                {
                    for (int i = 0; i < synapses.Count; i++) //process all the synapses sourced by this neuron
                    {
                        突触 s = synapses[i];
                        神经元 nTarget = s.获取目标神经元();
                        if (((long)nTarget.id >> 63) != 0) //does this synapse go to another server
                        {
                            神经元数组base.remoteQueue.Enqueue(s);
                        }

                        else
                        {
                            lock (锁)
                            {
                                var current = nTarget.currentCharge;
                                float desired = current + s.权重;
                                while (System.Threading.Interlocked.CompareExchange(ref currentCharge,current, desired)<=0)
                                {
                                    current = nTarget.currentCharge;
                                    desired = current + s.权重;
                                }
                            }


                            神经元数组base.添加神经元到激活列表组(nTarget.id);
                        }
                    }
                }
            }
        }
    const int ranges1 = 7;
    double[] cutoffs1 = { 1, .5, .34, .25, .2, .15, 0 };
    double[] posIncr1 = { 0, .1, .05, .025, .01, .012, .01 };
    double[] negIncr1 = { -.01, -.1, -.017, -.00625, -.002, -.002, -.001 };

//play with this for experimentation
const int ranges2 = 7;
    double[] cutoffs2 = { .5, .25, .1, 0, -.1, -.25, -1 };
    double[] posIncr2 = { .2, .1, .05, .05, .05, .1, .5 };
//	double negIncr2[ranges2] = { -.5, -.1, -.05, -.05,  -.05, -.1,  -.2 };
    double[] negIncr2 = { -.25, -.05, -.025, -.025, -.025, -.05, -.1 };
//	double negIncr2[ranges2] = { -.125, -.025, -.0125, -.0125,  -.0125, -.025,  -.05 };



public void Fire3()
        {
            if (模型 == 模型类.FloatValue) return;
            if (模型 == 模型类.Color && lastCharge != 0)
                return;
            if (synapses != null)
            {
                //while (vectorLock.exchange(1) == 1) { } //prevent the vector of synapses from changing while we're looking at it
                for (int i = 0; i < synapses.Count; i++) //process all the synapses sourced by this neuron
                {
                    突触 s = synapses[i];
                    神经元? nTarget = s.获取目标神经元();

                    if (s.模型字段 == 突触.模型类型.Hebbian1)
                    {
                        //did the target neuron fire after this stimulation?
                        float weight = s.权重;
                        if (nTarget.currentCharge >= 1 && currentCharge >= 1)
                        {
                            //strengthen the synapse
                            weight = 新建赫布权重(weight, .1f, s.模型字段, 1);
                        }
                        if (nTarget.currentCharge >= 1 && currentCharge < 1 ||
                            nTarget.currentCharge < 1 && currentCharge >= 1)
                        {
                            //weaken the synapse
                            weight = 新建赫布权重(weight, -.1f, s.模型字段, 1);
                        }
                        synapses[i].权重 = weight;
                    }
                }
                //vectorLock = 0;
            }
            if (synapsesFrom != null && currentCharge >= threshold)
            {
                int numHebbian = 0;
                int numPosHebbian = 0;
                lock (锁)
                {
                    for (int i = 0; i < synapsesFrom.Count; i++) //process all the synapses sourced by this neuron
                    {
                        突触 s = synapsesFrom[i];
                        if (s.模型字段 != 突触.模型类型.Fixed)
                        {
                            numHebbian++;
                            if (s.权重 >= 0) numPosHebbian++;
                        }
                    }
                    for (int i = 0; i < synapsesFrom.Count; i++) //process all the synapses sourced by this neuron
                    {
                        突触 s = synapsesFrom[i];
                        if (s.模型字段 == 突触.模型类型.Hebbian2 || s.模型字段 == 突触.模型类型.Binary)
                        {
                            神经元 nTarget = s.获取目标神经元();
                            //did this neuron fire coincident or just after the target (the source since these are FROM synapses)
                            float weight = s.权重;
                            int delay = 0;
                            if (s.模型字段 == 突触.模型类型.Hebbian2) delay = 6;

                            if (s.模型字段 == 突触.模型类型.Hebbian2 ||
                                s.模型字段 == 突触.模型类型.Binary)
                            {
                                if (nTarget.lastFired >= lastFired - delay)
                                {
                                    //strengthen the synapse
                                    weight = 新建赫布权重(weight, .1f, s.模型字段, numHebbian);
                                }
                                else
                                {
                                    //weaken the synapse
                                    weight = 新建赫布权重(weight, -.1f, s.模型字段, numHebbian);
                                }
                                //update the synapse in "From"
                                synapsesFrom[i].权重 = weight;
                                //update the synapse in "To"
                                for (int b = 0; b < nTarget.synapses.Count; b++)
                                {
                                    if (nTarget.synapses[b].获取目标神经元() == this)
                                    {
                                        lock (nTarget.锁)
                                        {
                                            nTarget.synapses[b].权重 = weight;
                                        }

                                    }
                                }
                            }

                        }
                    }
                }
            }
        }

        public float 新建赫布权重(float weight, float offset, 突触.模型类型 model, int numberOfSynapses1)
        {
            float numberOfSynapses = numberOfSynapses1 / 2.0f;
            float y = weight * numberOfSynapses;
            if (model == 突触.模型类型.Binary)
            {
                if (offset > 0) return 1.0f / (float)numberOfSynapses;
                return 0;
            }
            else if (model == 突触.模型类型.Hebbian1)
            {
                int i = 0;
                y = weight;
                for (i = 0; i < ranges1; i++)
                {
                    if (y >= cutoffs1[i])
                    {
                        if (offset > 0)
                            y += (float)posIncr1[i];
                        else
                            y += (float)negIncr1[i];
                        if (y < 0) y = 0;
                        if (y > 1) y = 1;
                        break;
                    }
                }
            }
            else if (model == 突触.模型类型.Hebbian2)
            {

                float maxVal = 1.0f / numberOfSynapses;
                float curWeight = weight * numberOfSynapses;
                float x = 0;
                if (curWeight >= 1)
                {
                    curWeight = 1;
                }
                else if (curWeight <= -1)
                {
                    curWeight = -1;
                }
                //			else
                x = MathF.Atanh(curWeight);

                if (offset != 0)
                {
                    x += offset;
                    curWeight = MathF.Tanh(x);
                }
                else
                {
                    x *= 0.5f;
                    curWeight = MathF.Tanh(x);
                }
                y = curWeight / numberOfSynapses;
                if (y < -maxVal) y = -maxVal;
                if (y > maxVal) y = maxVal;
            }
            return y;
        }
        public List<突触> 获取突触数组()
        {
            return synapses;
        }

        public int 获取突触数量()
        {
            return synapses.Count;
        }
    }
}
