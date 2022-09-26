//
// Copyright (c) [Name]. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
//  

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Serialization;

namespace BrainSimulator.Modules
{
    public class ModulePatternRecognizer : ModuleBase
    {

        const int maxPatterns = 6;
        public ModulePatternRecognizer()
        {
            minHeight = 3;
            minWidth = 3;
        }

        float targetWeightPos = 0;
        float targetWeightNeg = 0;
        int maxTries = 4;


        //fill this method in with code which will execute
        //once for each cycle of the engine

        int waitingForInput = 0;
        public override void Fire()
        {
            Init();  //be sure to leave this here

            //Do any hebbian synapse adjustment
            if (GetNeuron("Learning") is 神经元 nlearing && nlearing.Fired())
            {
                for (int i = 0; i < mv.Height; i++)
                {
                    if (mv.GetNeuronAt(0, i) is 神经元 n)
                    {
                        if (n.LastFired > MainWindow.此神经元数组.Generation - 4)
                        {
                            waitingForInput = 0;
                            //adjust the incoming synapse weights
                            int firedCount = 0;
                            for (int j = 0; j < n.synapsesFrom.Count; j++)
                            {
                                突触 s = n.synapsesFrom[j];
                                if (s.模型字段 != 突触.模型类型.Fixed)
                                {
                                    神经元 nSource = MainWindow.此神经元数组.获取神经元(s.目前神经元);
                                    if (nSource.LastFired > MainWindow.此神经元数组.Generation - 4)
                                    {
                                        firedCount++;
                                    }
                                }
                            }
                            //if no inputs fired, things might not work so skip for now
                            if (firedCount == 0)
                                continue;


                            for (int j = 0; j < n.synapsesFrom.Count; j++)
                            {
                                突触 s = n.synapsesFrom[j];
                                if (s.模型字段 != 突触.模型类型.Fixed)
                                {
                                    神经元 nSource = MainWindow.此神经元数组.获取神经元(s.目前神经元);
                                    if (nSource.LastFired > MainWindow.此神经元数组.Generation - 4)
                                    {
                                        nSource.添加突触(n.id, (s.权重字段 + targetWeightPos) / 2, s.模型字段);
                                    }
                                    else
                                    {
                                        nSource.添加突触(n.id, (s.权重字段 + targetWeightNeg) / 2, s.模型字段);
                                    }
                                }
                            }

                            //clear partial charges from others
                            for (int k = 0; k < mv.Height; k++)
                            {
                                if (mv.GetNeuronAt(0, k) is 神经元 n5)
                                {
                                    n5.currentCharge = 0;
                                    n5.更新();
                                }
                            }
                            break;
                        }
                    }
                }

                神经元 nRdOut = GetNeuron("RdOut");
                if ((MainWindow.此神经元数组.Generation % 36) == 0)
                {
                    waitingForInput = maxTries; //countdown tries to read
                                         //fire rdOut
                    nRdOut.currentCharge = 1;
                    nRdOut.更新();
                }
                if (waitingForInput > 1 && (MainWindow.此神经元数组.Generation % 6) == 0)
                {
                    waitingForInput--;
                    nRdOut.currentCharge = 1;
                    nRdOut.更新();
                }
                //if we get here, no neuron has fired yet
                if (waitingForInput == 1 && (MainWindow.此神经元数组.Generation % 6) == 0)
                {
                    waitingForInput--;
                    //if we've tried enough, learn a new pattern
                    //1) select the neuron to use 2) fire it

                    long oldestFired = long.MaxValue;
                    神经元 oldestNeuron = null;
                    for (int i = 0; i < mv.Height; i++)
                    {
                        if (mv.GetNeuronAt(0, i) is 神经元 n2)
                        {
                            if (n2.LastFired < oldestFired)
                            {
                                oldestFired = n2.LastFired;
                                oldestNeuron = n2;
                            }
                        }
                    }
                    if (oldestNeuron != null)
                    {
                        oldestNeuron.CurrentCharge = 1;
                        oldestNeuron.更新();
                        //zero out the old synapses
                        List<突触> prevSynapses = new List<突触>();
                        for (int j = 0; j < oldestNeuron.synapsesFrom.Count; j++)
                        {
                            prevSynapses.Add(oldestNeuron.synapsesFrom[j]);
                        }
                        oldestNeuron.删除所有突触(false);
                        for (int j = 0; j < prevSynapses.Count; j++)
                        {
                            float theWeight = prevSynapses[j].权重字段;
                            if (prevSynapses[j].模型字段 != 突触.模型类型.Fixed)
                                theWeight = 0;
                            theNeuronArray.获取神经元(prevSynapses[j].目标神经元字段).添加突触(
                                oldestNeuron.id, theWeight, prevSynapses[j].模型字段);
                        }
                    }
                }
            }
            else //not learning
            {
                神经元 nRdOut = GetNeuron("RdOut");
                for (int i = 0; i < mv.Height; i++)
                {
                    if (mv.GetNeuronAt(0, i) is 神经元 n)
                    {
                        if (n.LastFired > MainWindow.此神经元数组.Generation - 4)
                        {
                            waitingForInput = 0;
                        }
                    }
                }

                            if ((MainWindow.此神经元数组.Generation % 36) == 0)
                {
                    waitingForInput = maxTries; //countdown tries to read
                    nRdOut.currentCharge = 1;
                    nRdOut.更新();
                }
                if (waitingForInput > 1 && (MainWindow.此神经元数组.Generation % 6) == 0)
                {
                    waitingForInput--;
                    nRdOut.currentCharge = 1;
                    nRdOut.更新();
                }
            }
        }

        private void GetTargetWeights(int firedCount)
        {
            //the target pos weight = reciprocal of the number of neurons firing so if they all fire
            //the output will fire on the first try
            targetWeightPos = .000001f + 1f / (float)(firedCount);
            //the target neg weight
            targetWeightNeg = -0.008f;

            int bestErrorCount = 0;
            string bestResult = "";
            for (float targetWeightNegTrial = 0; targetWeightNegTrial < .4f; targetWeightNegTrial += .0001f)
            {
                int w = 0;
                string result = "";
                int k;
                for (k = 0; k < 10; k++) //bit errors
                {
                    float charge = 0;
                    for (w = 0; w < 40; w++) //generations
                    {
                        charge += (firedCount - k) * targetWeightPos -
                            (k) * targetWeightNegTrial;
                        if (charge >= 1)
                        {
                            if (!result.Contains(":" + w.ToString()))
                            {
                                result += " " + k + ":" + w;
                                goto resultFound;
                            }
                            else
                            {
                                result += " " + k + ":" + w + "XX ";
                                goto duplicateFound;
                            }
                        }
                    }
                    //we never got a hit
                    if (k > bestErrorCount)// && !result.Contains("XX"))
                    {
                        bestErrorCount = k;
                        bestResult = targetWeightNegTrial + " : " + result;
                        targetWeightNeg = -targetWeightNegTrial;
                    }
                    break;
                resultFound:
                    if (k > bestErrorCount)// && !result.Contains("XX"))
                    {
                        bestErrorCount = k;
                        bestResult = targetWeightNeg + " : " + result;
                        targetWeightNeg = -targetWeightNegTrial;
                    }
                    continue;
                }
            duplicateFound:
                if (k > bestErrorCount)// && !result.Contains("XX"))
                {
                    bestErrorCount = k;
                    bestResult = targetWeightNeg + " : " + result;
                    targetWeightNeg = -targetWeightNegTrial;
                }
                continue;
            }
        }

        private void AddIncomingSynapses()
        {
            神经元 nLearning = mv.GetNeuronAt(2, 1);
            nLearning.标签名 = "Learning";
            nLearning.添加突触(nLearning.id, 1);
            神经元 nRdOut = mv.GetNeuronAt(2, 0);
            nRdOut.标签名 = "RdOut";
            for (int i = 0; i < mv.Height; i++)
            {
                神经元 n1 = mv.GetNeuronAt(0, i);
                if (i != 0)
                    n1.清空();
                n1.标签名 = "P" + i;
            }
            if (GetNeuron("P0") is 神经元 n)
            {
                foreach (突触 s in n.突触来源列表)
                {
                    神经元 nSource = MainWindow.此神经元数组.获取神经元(s.目前神经元);
                    if (nSource.标签名 != "")
                    {
                        for (int j = 0; j < mv.Height; j++)
                        {
                            神经元 nTarget = mv.GetNeuronAt(0, j);
                            nSource.添加突触(nTarget.id, 0f, 突触.模型类型.Hebbian2);
                            MainWindow.此神经元数组.获取神经元位置(nSource.id, out int col, out int row);
                            while (MainWindow.此神经元数组.GetNeuron(col, ++row).标签名 != "")
                            {
                                MainWindow.此神经元数组.GetNeuron(col, row).添加突触(nTarget.id, 0f, 突触.模型类型.Hebbian2);
                            }
                        }
                    }
                }
            }
            //add mutual suppression
            for (int i = 0; i < mv.Height; i++)
                for (int j = 0; j < mv.Height; j++)
                {
                    if (j != i)
                    {
                        神经元 nSource = mv.GetNeuronAt(0, i);
                        神经元 nTarget = mv.GetNeuronAt(0, j);
                        nSource.添加突触(nTarget.id, -1);
                    }
                }

            //add latch column
            for (int i = 0; i < mv.Height; i++)
            {
                神经元 nSource = mv.GetNeuronAt(0, i);
                神经元 nTarget = mv.GetNeuronAt(1, i);
                nSource.添加突触(nTarget.id, .9f);
                nRdOut.添加突触(nTarget.id, -1);                
            }

            if (GetNeuron("P0") is 神经元 nP0)
                GetTargetWeights(nP0.synapsesFrom.Count(x => x.模型字段 == 突触.模型类型.Hebbian2) / 2);
            MainWindow.Update();
        }

        //called whenever the size of the module rectangle changes
        //for example, you may choose to reinitialize whenever size changes
        //delete if not needed
        public override void SizeChanged()
        {
            if (mv == null) return;
            AddIncomingSynapses();
        }


        public override void Initialize()
        {
            Init();
            AddIncomingSynapses();
        }
    }
}
