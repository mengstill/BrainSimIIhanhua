//
// Copyright (c) Charles Simon. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
//  

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Xml.Serialization;

namespace BrainSimulator
{
    public partial class 神经元数组 : 神经元处理
    {

        /// UNDO Handling UNDO 处理
        /// 
        //these lists keep track of changed things for undo
        //这些列表跟踪更改的内容以进行撤消
        List<撤消点> 撤销数组 = new List<撤消点>(); //checkpoints for multi-undos
                                                          //多重撤销的检查点
        List<突触撤销> 突触撤销信息 = new List<突触撤销>();
        List<神经元撤消> 神经元撤销信息 = new List<神经元撤消>();
        List<选择撤消> 选择撤销信息 = new List<选择撤消>();
        List<模块撤消> 模块撤消信息 = new List<模块撤消>();

        //Undoes everything back to the last undo point.
        //将所有内容撤消到最后一个撤消点。
        public void 撤销()
        {
            if (撤销数组.Count == 0) return;
            int 突触点位 = 撤销数组.Last().突触点位;
            int 神经元点位 = 撤销数组.Last().神经元点位;
            int 选择点位 = 撤销数组.Last().选择点位;
            int 模块点位 = 撤销数组.Last().模块点位;
            撤销数组.RemoveAt(撤销数组.Count - 1);

            int undoCounter = 0;
            while (模块撤消信息.Count > 模块点位)
            {
                undoCounter++;
                撤销模块();
            } 
            while (选择撤销信息.Count > 选择点位)
            {
                undoCounter++;
                撤销选择();
            }
            while (神经元撤销信息.Count > 神经元点位)
            {
                undoCounter++;
                撤销神经元();
            }
            while (突触撤销信息.Count > 突触点位)
            {
                undoCounter++;
                撤销突触();
            }
            if (undoCounter == 0) 撤销();
        }

        public void 设置撤消点()
        {
            //only add if this is different from the latest
            //仅在与最新版本不同时添加
            if (撤销数组.Count > 0 &&
                撤销数组.Last().突触点位 == 突触撤销信息.Count &&
                撤销数组.Last().神经元点位 == 神经元撤销信息.Count &&
                撤销数组.Last().选择点位 == 选择撤销信息.Count &&
                撤销数组.Last().模块点位 == 模块撤消信息.Count
                ) return;
            撤销数组.Add(new 撤消点
            {
                突触点位 = 突触撤销信息.Count,
                神经元点位 = 神经元撤销信息.Count,
                选择点位 = 选择撤销信息.Count,
                模块点位 = 模块撤消信息.Count
            });
        }
        public int 获取撤销数量()
        {
            return 撤销数组.Count;
        }
        struct 突触撤销
        {
            public int source, target;
            public float 权重;
            public bool 新突触;
            public bool 删除突触;
            public 突触.modelType 模块;
        }
        struct 神经元撤消
        {
            public 神经元 以前的神经元;
            public bool neuronIsShowingSynapses;
        }
        struct 选择撤消
        {
            public 选择 选择状态;
        }
        struct 模块撤消
        {
            public int index;
            public 模块视图 模型状态;
        }
        struct 撤消点
        {
            public int 突触点位;
            public int 神经元点位;
            public int 选择点位;
            public int 模块点位;
        }


        public void 添加突触撤销(int source, int target, float 权重, 突触.modelType 模块, bool 新突触, bool 删除突触)
        {
            突触撤销 s;
            s = new 突触撤销
            {
                source = source,
                target = target,
                权重 = 权重,
                模块 = 模块,
                新突触 = 新突触,
                删除突触 = 删除突触,
            };
            突触撤销信息.Add(s);
        }
        public void 添加神经元撤销(神经元 n)
        {
            神经元 n1 = n.Copy();
            神经元撤销信息.Add(new 神经元撤消 { 以前的神经元 = n1, neuronIsShowingSynapses = MainWindow.神经元数组视图.IsShowingSynapses(n1.id) });
        }
        public void 添加选择撤销()
        {
            选择撤消 s1 = new 选择撤消();
            s1.选择状态 = new 选择();
            foreach (选择矩阵 nsr in MainWindow.神经元数组视图.theSelection.selectedRectangles)
            {
                选择矩阵 nsr1 = new 选择矩阵(nsr.首个选中的神经元, nsr.高度, nsr.宽度);
                s1.选择状态.selectedRectangles.Add(nsr1);
            }
            if (s1.选择状态.selectedRectangles.Count > 0)
                选择撤销信息.Add(s1);
        }
        public void 添加模块撤销(int index, 模块视图 currentModule)
        {
            模块撤消 m1 = new 模块撤消();
            m1.index = index;
            if (currentModule == null)
                m1.模型状态 = null;
            else
                m1.模型状态 = new 模块视图()
                {
                    Width = currentModule.Width,
                    Height = currentModule.Height,
                    FirstNeuron = currentModule.FirstNeuron,
                    Color = currentModule.Color,
                    ModuleTypeStr = currentModule.TheModule.GetType().Name,
                    Label = currentModule.Label
                };
            模块撤消信息.Add(m1);
        }

        private void 撤销神经元()
        {
            if (神经元撤销信息.Count == 0) return;
            神经元撤消 n = 神经元撤销信息.Last();
            神经元撤销信息.RemoveAt(神经元撤销信息.Count - 1);
            神经元 n1 = n.以前的神经元.Copy();
            if (n1.标签名 != "") 从缓存中移除标签(n1.Id);
            if (n1.标签 != "") 向缓存中添加标签(n1.Id, n1.标签);

            if (n.neuronIsShowingSynapses)
                MainWindow.神经元数组视图.添加突触显示(n1.id);
            n1.更新();
        }
        private void 撤销选择()
        {
            MainWindow.神经元数组视图.theSelection.selectedRectangles.Clear();
            foreach (选择矩阵 nsr in 选择撤销信息.Last().选择状态.selectedRectangles)
            {
                选择矩阵 nsr1 = new 选择矩阵(nsr.首个选中的神经元, nsr.高度, nsr.宽度);
                MainWindow.神经元数组视图.theSelection.selectedRectangles.Add(nsr1);
            }
            选择撤销信息.RemoveAt(选择撤销信息.Count - 1);
        }
        private void 撤销模块()
        {
            lock (MainWindow.此神经元数组.模块)
            {
                if (模块撤消信息.Count == 0) return;
                模块撤消 m1 = 模块撤消信息.Last();
                if (m1.模型状态 == null)  //the module was just added
                {
                    模块视图.删除模块(m1.index);
                }
                else
                {
                    if (m1.index == -1) //the module was just deleted
                    {
                        模块视图 mv = new 模块视图
                        {
                            Width = m1.模型状态.Width,
                            Height = m1.模型状态.Height,
                            FirstNeuron = m1.模型状态.FirstNeuron,
                            Color = m1.模型状态.Color,
                            ModuleTypeStr = m1.模型状态.ModuleTypeStr,
                            Label = m1.模型状态.Label,
                        };
                        // modules.Add(mv);
                        模块视图.创造模块(mv.Label, mv.ModuleTypeStr, 跨语言接口.IntToColor(mv.Color), mv.FirstNeuron, mv.Width, mv.Height);
                    }
                    else
                    {
                        模块[m1.index].FirstNeuron = m1.模型状态.FirstNeuron;
                        模块[m1.index].Width = m1.模型状态.Width;
                        模块[m1.index].Height = m1.模型状态.Height;
                        模块[m1.index].Color = m1.模型状态.Color;
                        模块[m1.index].ModuleTypeStr = m1.模型状态.ModuleTypeStr;
                        模块[m1.index].Label = m1.模型状态.Label;
                    }
                }
                模块撤消信息.RemoveAt(模块撤消信息.Count - 1);
            }
        }
        private void 撤销突触()
        {
            if (突触撤销信息.Count == 0) return;
            突触撤销 s = 突触撤销信息.Last();
            突触撤销信息.RemoveAt(突触撤销信息.Count - 1);

            神经元 n = 获取神经元(s.source);
            if (s.新突触) //the synapse was added so delete it
            {
                n.删除突触(s.target);
            }
            else if (s.删除突触) //the synapse was deleted so add it back
            {
                n.添加突触(s.target, s.权重, s.模块);
            }
            else //weight/type changed 
            {
                n.添加突触(s.target, s.权重, s.模块);
            }
            n.更新();
        }
    }
}
