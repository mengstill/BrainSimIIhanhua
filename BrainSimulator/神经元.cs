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
        public 神经元.模型类型 Model { get; set; }
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
            LeakRate = 神经元.leakRate泄露速度;
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
            神经元.leakRate泄露速度 = LeakRate;
            神经元.突触延迟 = AxonDelay;
            if(神经元.模型字段 != 神经元.模型类型.Color)
            {
                神经元.LastCharge = LastCharge;
                神经元.当前更改 = LastCharge;
            }
            else
            {
                神经元.LastChargeInt = LastChargeInt;
                神经元.当前更改 = LastChargeInt;
            }
            神经元.显示突触 = ShowSynapses;
            神经元.RecordHistory = RecordHistory;
            foreach(突触参数 参数 in Synapses)
            {
                突触 s = new 突触();
                s.目标神经元字段=参数.TargetNeuron;
                s.权重字段 = 参数.Weight;
                if (参数.IsHebbian) s.模型字段 = 突触.modelType.Hebbian1;
                else s.模型字段 = 参数.Model;
                神经元.添加突触(s.目标神经元字段, s.权重字段, s.模型字段);
            }
        }
    }

    public class 神经元 : 神经元部分参数
    {
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
            get { return (float)最后更改; }
            set { 当前更改 = value; }
        }

        //get/set last charge. Setting also sets current charge

        //TODO: change this to SetValue
        /// <summary>
        /// 最后更改
        /// </summary>
        public float LastCharge { get { return (float)最后更改; } set { 最后更改 = value; } }// Update(); } }

        //get/set last charge as raw integer Used by COLOR nueurons
        /// <summary>
        /// 最后更改int
        /// </summary>
        public int LastChargeInt { get { return (int)最后更改; } set { 最后更改 = value; 更新(); } }
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
            当前更改 = value;
            if (模型字段 == 模型类型.FloatValue)
                最后更改 = value;
            更新();
        }

        public enum 模型类型 { IF, Color, FloatValue, LIF, Random, Burst, Always };

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

        public float 泄露率 { get => leakRate泄露速度; set { leakRate泄露速度 = value; 更新(); } }
        public int AxonDelay
        {
            get => 突触延迟;
            set { 突触延迟 = value; 更新(); }
        }

        public 模型类型 模型 { get => (神经元.模型类型)模型字段; set { 模型字段 = (模型类型)value; 更新(); } }

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
            模型字段 = 模型类型.IF;
            SetValue(0);
        }

        public void 添加突触(int targetNeuron, float weight, 突触.modelType model)
        {
            所有者数组.添加突触(Id, targetNeuron, weight, model, false);
        }
        public void 添加突触(int targetNeuron, float weight)
        {
            所有者数组.添加突触(Id, targetNeuron, weight, 突触.modelType.Fixed, false);
        }

        public void 撤销与添加突触(int targetNeuron, float weight, 突触.modelType model = 突触.modelType.Fixed)
        {
            //TODO，先检查一下突触是否已经存在，保存旧的权重
            突触 s = 查找突触(targetNeuron);
            if (s != null)
                MainWindow.此神经元数组.添加突触撤销(Id, targetNeuron, s.权重字段, s.模型字段, false, false);
            else
                MainWindow.此神经元数组.添加突触撤销(Id, targetNeuron, 0, 突触.modelType.Fixed, true, false);

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
                    删除突触(s.目标神经元字段);
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

            删除突触(targetNeuron);
        }
        public void 删除突触(int targetNeuron)
        {
            所有者数组.DeleteSynapse(Id, targetNeuron);
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
            n.最后更改 = this.最后更改;
            n.当前更改 = this.当前更改;
            n.泄露率 = this.泄露率;
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
            n.最后更改 = this.最后更改;
            n.当前更改 = this.当前更改;
            n.泄露率 = this.泄露率;
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
            当前更改 = 0;
            最后更改 = 0;
            模型字段 = 模型类型.IF;
            泄露率 = 0.1f;
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
    }
}
