﻿
//
// Copyright (c) Charles Simon. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
//  

using System;
using System.Collections.Generic;
using System.Windows;

namespace BrainSimulator
{
    public partial class 神经元数组视图
    {
        struct BoundarySynapse
        {
            public int innerNeuronID;
            //public string outerNeuronName;
            public int outerNeuronID;
            public float weight;
            public 突触.模型类型 model;
        }
        List<BoundarySynapse> boundarySynapsesOut = new List<BoundarySynapse>();
        List<BoundarySynapse> boundarySynapsesIn = new List<BoundarySynapse>();

        public void ClearSelection()
        {
            theSelection.selectedRectangles.Clear();
            targetNeuronIndex = -1;
        }

        //copy the selection to a clipboard
        public void CopyNeurons()
        {
            //get list of neurons to copy
            List<int> neuronsToCopy = theSelection.枚举选择的神经元();
            theSelection.获取选择的矩形边界(out int X1o, out int Y1o, out int X2o, out int Y2o);
            MainWindow.myClipBoard = new NeuronArray();
            NeuronArray myClipBoard;
            myClipBoard = MainWindow.myClipBoard;
            myClipBoard.初始化((X2o - X1o + 1) * (Y2o - Y1o + 1), (Y2o - Y1o + 1),true);
            boundarySynapsesOut.Clear();
            boundarySynapsesIn.Clear();

            //copy the neurons
            foreach (int nID in neuronsToCopy)
            {
                int destId = 获取剪切板Id(X1o, Y1o, nID);
                //copy the source neuron to the clipboard
                神经元 sourceNeuron = MainWindow.此神经元数组.获取神经元(nID);
                神经元 destNeuron = sourceNeuron.Clone();
                destNeuron.所有者 = myClipBoard;
                destNeuron.Id = destId;
                destNeuron.标签名 = sourceNeuron.标签名;
                destNeuron.ToolTip= sourceNeuron.ToolTip;
                destNeuron.显示突触 = sourceNeuron.显示突触;
                myClipBoard.SetNeuron(destId, destNeuron);
            }

            //copy the synapses (this is two-pass so we make sure all neurons exist prior to copying
            foreach (int nID in neuronsToCopy)
            {
                神经元 sourceNeuron = MainWindow.此神经元数组.获取神经元(nID);
                if (MainWindow.useServers)
                {
                    sourceNeuron.synapses = 神经元客户端.GetSynapses(sourceNeuron.id);
                }

                int destId = 获取剪切板Id(X1o, Y1o, nID);
                神经元 destNeuron = myClipBoard.获取神经元(destId);
                destNeuron.所有者 = myClipBoard;
                if (sourceNeuron.突触列表 != null)
                    foreach (突触 s in sourceNeuron.突触列表)
                    {
                        //only copy synapses with both ends in the selection
                        if (neuronsToCopy.Contains(s.目前神经元))
                        {
                            destNeuron.添加突触(获取剪切板Id(X1o, Y1o, s.目前神经元), s.权重, s.模型字段);
                        }
                        else
                        {
                            string targetName = MainWindow.此神经元数组.获取神经元(s.目标神经元字段).标签;
                            if (targetName != "")
                            {
                                boundarySynapsesOut.Add(new BoundarySynapse
                                {
                                    innerNeuronID = destNeuron.id,
                                    outerNeuronID = s.目标神经元字段,
                                    weight = s.权重字段,
                                    model = s.模型字段
                                });
                            }
                        }
                    }
                if (sourceNeuron.突触来源列表 != null)
                    foreach (突触 s in sourceNeuron.突触来源列表)
                    {
                        if (!neuronsToCopy.Contains(s.目前神经元))
                        {
                            string sourceName = MainWindow.此神经元数组.获取神经元(s.目标神经元字段).标签;
                            if (sourceName != "")
                            {
                                boundarySynapsesIn.Add(new BoundarySynapse
                                {
                                    innerNeuronID = destNeuron.id,
                                    outerNeuronID = s.目标神经元字段,
                                    weight = s.权重字段,
                                    model = s.模型字段
                                }); ;
                            }
                        }
                    }
            }

            //copy modules
            foreach (模块视图 mv in MainWindow.此神经元数组.模块)
            {
                if (theSelection.NeuronInSelection(mv.FirstNeuron) > 0 && theSelection.NeuronInSelection(mv.LastNeuron) > 0)
                {
                    模块视图 newMV = new 模块视图()
                    {
                        FirstNeuron = 获取剪切板Id(X1o, Y1o, mv.FirstNeuron),
                        TheModule = mv.TheModule,
                        Color = mv.Color,
                        Height = mv.Height,
                        Width = mv.Width,
                        Label = mv.Label,
                        ModuleTypeStr = mv.ModuleTypeStr,
                    };

                    myClipBoard.模块.Add(newMV);
                }
            }
            xml文件.保存(myClipBoard, "剪切板");
        }

        private int 获取剪切板Id(int X1o, int Y1o, int nID)
        {
            //get the row & col in the neuronArray
            MainWindow.此神经元数组.获取神经元位置(nID, out int col, out int row);
            //get the destID in the clipboard
            int destId = MainWindow.myClipBoard.获取神经元索引(col - X1o, row - Y1o);
            return destId;
        }

        private int 获取神经元数组ID(int nID)
        {
            MainWindow.myClipBoard.获取神经元位置(nID, out int col, out int row);
            MainWindow.此神经元数组.获取神经元位置(targetNeuronIndex, out int targetCol, out int targetRow);
            int destId = MainWindow.此神经元数组.获取神经元索引(col + targetCol, row + targetRow);
            return destId;
        }


        public void CutNeurons()
        {
            CopyNeurons();
            DeleteModulesInSelection();
            DeleteSelection();
            ClearSelection();
            更新();
        }

        private void DeleteModulesInSelection()
        {
            lock (MainWindow.此神经元数组.模块)
            {
                for (int i = 0; i < MainWindow.此神经元数组.模块.Count; i++)
                {
                    模块视图 mv = MainWindow.此神经元数组.模块[i];
                    if (theSelection.NeuronInSelection(mv.FirstNeuron) > 0 && theSelection.NeuronInSelection(mv.LastNeuron) > 0)
                    {
                        MainWindow.此神经元数组.添加模块撤销(i, mv);
                        mv.TheModule.CloseDlg();
                        MainWindow.此神经元数组.模块.RemoveAt(i);
                        i--;
                    }
                }
            }
        }

        public void PasteNeurons()
        {
            NeuronArray myClipBoard = MainWindow.myClipBoard;

            if (!MainWindow.神经元数组视图.dp.显示神经元())
            {
                MessageBox.Show("Pasting not allowed at this zoom factor");
                return;
            }
            if (targetNeuronIndex == -1) return;
            if (myClipBoard == null) return;

            if (!xml文件.Windows剪贴板是否包含神经元数组()) return;

            myClipBoard = MainWindow.myClipBoard;
            //We are pasting neurons from the clipboard.  
            //The arrays have different sizes so we may by row-col.

            //first check to see if the destination is claar and warn
            List<int> targetNeurons = new List<int>();
            for (int i = 0; i < myClipBoard.arraySize; i++)
            {
                if (myClipBoard.获取神经元(i,true) != null)
                {
                    targetNeurons.Add(获取神经元数组ID(i));
                }
            }

            MainWindow.此神经元数组.获取神经元位置(targetNeuronIndex, out int col, out int row);
            if (col + myClipBoard.Cols > MainWindow.此神经元数组.Cols ||
                row + myClipBoard.rows > MainWindow.此神经元数组.rows)
            {
                MessageBoxResult result = MessageBox.Show("Paste would exceed neuron array boundary!", "Error", MessageBoxButton.OK);
                return;
            }

            if (!IsDestinationClear(targetNeurons, 0, true))
            {
                MessageBoxResult result = MessageBox.Show("Some desination neurons are in use and will be overwritten, continue?", "Continue", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.No) return;
            }

            MainWindow.暂停引擎();
            MainWindow.此神经元数组.设置撤消点();
            //now paste the neurons
            for (int i = 0; i < myClipBoard.arraySize; i++)
            {
                if (myClipBoard.获取神经元(i) != null)
                {
                    int destID = 获取神经元数组ID(i);
                    MainWindow.此神经元数组.获取神经元(destID).添加撤销信息();
                    神经元 n = myClipBoard.获取完整的神经元(i,true);
                    n.所有者 = myClipBoard;
                    n.synapses = myClipBoard.获取突触列表(i);

                    神经元 sourceNeuron = n.Clone();
                    sourceNeuron.id = destID;
                    while (sourceNeuron.标签 != "" && MainWindow.此神经元数组.GetNeuron(sourceNeuron.标签) != null)
                    {
                        int num = 0;
                        int digitCount = 0;
                        while (sourceNeuron.标签 != "" && Char.IsDigit(sourceNeuron.标签[sourceNeuron.标签.Length - 1]))
                        {
                            int.TryParse(sourceNeuron.标签[sourceNeuron.标签.Length - 1].ToString(), out int digit);
                            num = num + (int)Math.Pow(10, digitCount) * digit;
                            digitCount++;
                            sourceNeuron.标签 = sourceNeuron.标签.Substring(0, sourceNeuron.标签.Length - 1);
                        }
                        num++;
                        sourceNeuron.标签 = sourceNeuron.标签 + num.ToString();
                    }
                    sourceNeuron.所有者 = MainWindow.此神经元数组;
                    sourceNeuron.标签名 = sourceNeuron.标签;
                    sourceNeuron.ToolTip = n.ToolTip;
                    sourceNeuron.显示突触 = n.显示突触;
                    MainWindow.此神经元数组.SetNeuron(destID, sourceNeuron);


                    foreach (突触 s in n.突触列表)
                    {
                        MainWindow.此神经元数组.获取神经元(destID).
                            撤销与添加突触(获取神经元数组ID(s.目前神经元), s.权重, s.模型字段);
                    }
                }
            }

            //handle boundary synapses
            //处理边界突触
            foreach (BoundarySynapse b in boundarySynapsesOut)
            {
                int sourceID = 获取神经元数组ID(b.innerNeuronID);
                神经元 targetNeuron = MainWindow.此神经元数组.获取神经元(b.outerNeuronID);
                if (targetNeuron != null)
                    MainWindow.此神经元数组.获取神经元(sourceID).撤销与添加突触(targetNeuron.id, b.weight, b.model);
            }
            foreach (BoundarySynapse b in boundarySynapsesIn)
            {
                int targetID = 获取神经元数组ID(b.innerNeuronID);
                神经元 sourceNeuron = MainWindow.此神经元数组.获取神经元(b.outerNeuronID);
                if (sourceNeuron != null)
                    sourceNeuron.撤销与添加突触(targetID, b.weight, b.model);
            }

            //paste modules
            //粘贴模块
            foreach (模块视图 mv in myClipBoard.模块)
            {
                模块视图 newMV = new 模块视图()
                {
                    FirstNeuron = 获取神经元数组ID(mv.FirstNeuron),
                    TheModule = mv.TheModule,
                    Color = mv.Color,
                    Height = mv.Height,
                    Width = mv.Width,
                    Label = mv.Label,
                    ModuleTypeStr = mv.ModuleTypeStr,
                };

                MainWindow.此神经元数组.模块.Add(newMV);
            }
            MainWindow.恢复引擎();
            更新();
        }

        public void ConnectFromHere()
        {
            if (targetNeuronIndex == -1) return;
            MainWindow.此神经元数组.设置撤消点();
            神经元 targetNeuron = MainWindow.此神经元数组.获取神经元(targetNeuronIndex);
            List<int> neuronsInSelection = theSelection.枚举选择的神经元();
            for (int i = 0; i < neuronsInSelection.Count; i++)
            {
                targetNeuron.撤销与添加突触(neuronsInSelection[i], 末尾突触权重, 末尾突触模型);
            }
            更新();
        }

        public void ConnectToHere()
        {
            if (targetNeuronIndex == -1) return;
            MainWindow.此神经元数组.设置撤消点();
            List<int> neuronsInSelection = theSelection.枚举选择的神经元();
            for (int i = 0; i < neuronsInSelection.Count; i++)
            {
                神经元 n = MainWindow.此神经元数组.获取神经元(neuronsInSelection[i]);
                n.撤销与添加突触(targetNeuronIndex, 末尾突触权重, 末尾突触模型);
            }
            更新();
        }


        public void DeleteSelection(bool deleteBoundarySynapses = true, bool allowUndo = true)
        {
            if (allowUndo)
                MainWindow.此神经元数组.设置撤消点();

            DeleteModulesInSelection();

            List<int> neuronsToDelete = theSelection.枚举选择的神经元();
            foreach (int nID in neuronsToDelete)
            {
                神经元 n = MainWindow.此神经元数组.获取神经元(nID);
                if (deleteBoundarySynapses)
                {
                    foreach(突触 s in n.synapsesFrom)
                    {
                        神经元 source = MainWindow.此神经元数组.获取神经元(s.目标神经元字段);
                        if (source != null && theSelection.NeuronInSelection(source.id)==0)
                        {
                            source.撤销并且删除突触(n.id);
                        }
                    }
                }
                for (int i = 0; i < n.synapses.Count; i++)
                    n.撤销并且删除突触(n.synapses[i].目标神经元字段);
                n.添加撤销信息();
                n.CurrentCharge = 0;
                n.LastCharge = 0;
                n.模型 = 神经元.模型类型.IF;

                n.标签名 = "";
                n.ToolTip = "";
                
                n.更新();
            }
            更新();
        }

        private bool IsDestinationClear(List<int> neuronsToMove, int offset, bool flagOverlap = false)
        {
            bool retVal = true;
            foreach (int id in neuronsToMove)
            {
                if (flagOverlap || !neuronsToMove.Contains(id + offset))
                {
                    神经元 n = MainWindow.此神经元数组.获取神经元(id + offset);
                    if (n.InUse())
                    {
                        retVal = false;
                        break;
                    }
                }
            }
            return retVal;
        }

        //move the neurons from the selected area to the second (start point) and stretch all the synapses
        public void MoveNeurons(bool dragging = false)
        {
            if (theSelection.选中神经元索引 == -1) return;
            if (theSelection.selectedRectangles.Count == 0) return;

            List<int> neuronsToMove = theSelection.枚举选择的神经元();
            int maxCol = 0;
            int maxRow = 0;
            MainWindow.此神经元数组.获取神经元位置(theSelection.selectedRectangles[0].首个选中的神经元, out int col0, out int row0);
            foreach (int id in neuronsToMove)
            {
                MainWindow.此神经元数组.获取神经元位置(id, out int tcol, out int trow);
                if (maxCol < tcol - col0) maxCol = tcol - col0;
                if (maxRow < trow - row0) maxRow = trow - row0;
            }

            int offset = targetNeuronIndex - theSelection.selectedRectangles[0].首个选中的神经元;
            if (offset == 0) return;

            MainWindow.此神经元数组.获取神经元位置(targetNeuronIndex, out int col, out int row);
            if (col + maxCol >= MainWindow.此神经元数组.Cols ||
                row + maxRow >= MainWindow.此神经元数组.rows ||
                row < 0 || 
                col < 0)
            {
                if (!dragging)
                     MessageBox.Show("Move would exceed neuron array boundary!", "Error", MessageBoxButton.OK);
                return;
            }

            if (!IsDestinationClear(neuronsToMove, offset))
            {
                MessageBoxResult result = MessageBox.Show("Some destination neurons are in use and will be overwritten, continue?\n\nYou can also right-click the final destination neuron and select 'Move Here.'", "Continue", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.No) return;
            }

            if (!dragging)
            {
                MainWindow.此神经元数组.设置撤消点();
                MainWindow.此神经元数组.添加选择撤销();
            }
            //change the order of copying to keep from overwriting ourselves
            if (offset > 0) neuronsToMove.Reverse();
            foreach (int source in neuronsToMove)
            {
                神经元 sourceNeuron = MainWindow.此神经元数组.获取神经元(source);
                神经元 destNeuron = MainWindow.此神经元数组.获取神经元(source + offset);
                MoveOneNeuron(sourceNeuron, destNeuron);
                if (MainWindow.神经元数组视图.IsShowingSynapses(source))
                {
                    MainWindow.神经元数组视图.移除突触显示(source);
                    MainWindow.神经元数组视图.添加突触显示(source + offset);
                }
            }

            foreach (模块视图 mv in MainWindow.此神经元数组.模块)
            {
                if (theSelection.NeuronInSelection(mv.FirstNeuron) > 0 && theSelection.NeuronInSelection(mv.LastNeuron) > 0)
                {
                    mv.FirstNeuron += offset;
                }
            }

            try
            {
                targetNeuronIndex = -1;
                foreach(选择矩阵 nsr in theSelection.selectedRectangles)
                {
                    nsr.首个选中的神经元 += offset;
                }
                更新();
            }
            catch { }
        }

        public void MoveOneNeuron(神经元 n, 神经元 nNewLocation, bool addUndo = true)
        {
            if (addUndo)
            {
                n.添加撤销信息();
                nNewLocation.添加撤销信息();
            }
            if(MainWindow.useServers)
            {
                n.synapses = 神经元客户端.GetSynapses(n.id);
                n.synapsesFrom = 神经元客户端.获取突触(n.id);
            }

            //copy the neuron attributes and delete them from the old neuron.
            n.复制(nNewLocation);
            MainWindow.此神经元数组.设置所有神经元(nNewLocation);
            //nNewLocation.RecordHistory = n.RecordHistory;
            //n.recordHistory = false;
            //if (FiringHistory.NeuronIsInFiringHistory(n.id))
            //{
            //    FiringHistory.RemoveNeuronFromHistoryWindow(n.id);
            //    FiringHistory.AddNeuronToHistoryWindow(nNewLocation.id);
            //}

            //for all the synapses going out this neuron, change to going from new location
            //don't use a foreach here because the body of the loop may delete a list entry
            for (int k = 0; k < n.突触列表.Count; k++)
            {
                突触 s = n.突触列表[k];
                if (addUndo)
                {
                    if (s.目标神经元字段 != n.id)
                        nNewLocation.撤销与添加突触(s.目标神经元字段, s.权重字段, s.模型字段);
                    else
                        nNewLocation.撤销与添加突触(nNewLocation.id, s.权重字段, s.模型字段);
                    n.撤销并且删除突触(n.synapses[k].目标神经元字段);
                }
                else
                {
                    if (s.目标神经元字段 != n.id)
                        nNewLocation.添加突触(s.目标神经元字段, s.权重字段, s.模型字段);
                    else
                        nNewLocation.添加突触(nNewLocation.id, s.权重字段, s.模型字段);
                    n.删除突触(n.synapses[k].目标神经元字段);
                }
            }

            //for all the synapses coming into this neuron, change the synapse target to new location
            for (int k = 0; k < n.突触来源列表.Count; k++)
            {
                突触 reverseSynapse = n.突触来源列表[k]; //(from synapses are sort-of backward
                if (reverseSynapse.目标神经元字段 != -1) //?
                {
                    神经元 sourceNeuron = MainWindow.此神经元数组.获取神经元(reverseSynapse.目标神经元字段);
                    sourceNeuron.撤销并且删除突触(n.id);
                    if (sourceNeuron.id != n.id)
                        if (addUndo)
                        {
                            sourceNeuron.撤销与添加突触(nNewLocation.id, reverseSynapse.权重字段, reverseSynapse.模型字段);
                        }
                        else
                        {
                            sourceNeuron.添加突触(nNewLocation.id, reverseSynapse.权重字段, reverseSynapse.模型字段);
                        }
                }
            }

            n.清空();
        }

        public void StepAndRepeat(int source, int target, float weight, 突触.模型类型 model)
        {
            int distance = target - source;
            theSelection.枚举选择的神经元();
            for (神经元 n = theSelection.获取选中的神经元(); n != null; n = theSelection.获取选中的神经元())
            {
                n.撤销与添加突触(theSelection.选中神经元索引 + distance, weight, model);
            }
            更新();
        }

        private Random rand;
        public void CreateRandomSynapses(int synapsesPerNeuron)
        {
            MainWindow.此神经元数组.设置撤消点();
            MainWindow.thisWindow.设置程序(0, "Allocating Random Synapses");
            List<int> neuronsInSelection = theSelection.枚举选择的神经元();
            for (int i = 0; i < neuronsInSelection.Count; i++)
            {
                if (MainWindow.thisWindow.设置程序(100f * i / (float)neuronsInSelection.Count, "")) 
                    break;
                神经元 n = MainWindow.此神经元数组.获取神经元(neuronsInSelection[i]);
                if (rand == null) rand = new Random();
                for (int j = 0; j < synapsesPerNeuron; j++)
                {
                    int targetNeuron = neuronsInSelection[rand.Next(neuronsInSelection.Count)];
                    float weight = (rand.Next(521) / 1000f) - .2605f;
                    n.撤销与添加突触(targetNeuron, weight, 突触.模型类型.Fixed);
                }
            }
            MainWindow.thisWindow.设置程序(100, "");
            更新();
        }

        public void MutualSuppression()
        {
            MainWindow.此神经元数组.设置撤消点();
            MainWindow.thisWindow.设置程序(0, "Allocating Synapses");

            List<int> neuronsInSelection = theSelection.枚举选择的神经元();
            for (int i = 0; i < neuronsInSelection.Count; i++) 
            {
                if (MainWindow.thisWindow.设置程序(100 * i / (float)neuronsInSelection.Count, "")) break; 
                神经元 n = MainWindow.此神经元数组.获取神经元(neuronsInSelection[i]);
                foreach (神经元 n1 in theSelection.神经元集合)
                {
                    if (n.id != n1.id)
                    {
                        n.撤销与添加突触(n1.id, -1, 突触.模型类型.Fixed);
                    }
                }
            }
            MainWindow.thisWindow.设置程序(100, "");
            更新();
        }
    }
}
