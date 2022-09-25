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
    public class ModuleRateDecoder3 : ModuleBase
    {
        //any public variable you create here will automatically be saved and restored  with the network
        //unless you precede it with the [XmlIgnore] directive
        //[XlmIgnore] 
        //public theStatus = 1;


        //set size parameters as needed in the constructor
        //set max to be -1 if unlimited

        float theLeakRate = 0.13f;

        public ModuleRateDecoder3()
        {
            minHeight = 2;
            maxHeight = 15;
            minWidth = 6;
            maxWidth = 6;
        }


        //fill this method in with code which will execute
        //once for each cycle of the engine
        public override void Fire()
        {
            Init();  //be sure to leave this here

            //if you want the dlg to update, use the following code whenever any parameter changes
            // call UpdateDialog
        }

        //fill this method in with code which will execute once
        //when the module is added, when "initialize" is selected from the context menu,
        //or when the engine restart button is pressed
        public override void Initialize()
        {
            Init();
            SetUpNeurons(mv.Height - 1);
        }

        private void SetUpNeurons(int levelCount)
        {
            神经元 nRd = mv.GetNeuronAt(0, 0);
            nRd.标签名 = "Rd";  
            神经元 nIn = mv.GetNeuronAt(1, 0);
            nIn.标签名 = "In";
            神经元 nIn1 = mv.GetNeuronAt(2, 0);
            nIn1.标签名 = "In1";
            神经元 nClr = mv.GetNeuronAt(3, 0);
            nClr.标签名 = "Clr";

            nRd.添加突触(nClr.id, 1);
            nIn.添加突触(nIn1.id, 1);

            for (int i = 0; i < levelCount; i++)
            {
                神经元 ni = mv.GetNeuronAt(0, i + 1);
                ni.清空();
                神经元 ni1 = mv.GetNeuronAt(1, i + 1);
                ni1.清空();
                神经元 nm = mv.GetNeuronAt(2, i + 1);
                ni.清空();
                神经元 no = mv.GetNeuronAt(3, i + 1);
                no.清空();
                no.模型字段 = 神经元.模型类型.LIF;
                no.泄露率 = 0.13f;
                神经元 noP = mv.GetNeuronAt(4, i + 1);
                noP.清空();
                神经元 noN = mv.GetNeuronAt(5, i + 1);
                noN.清空();
            }

            神经元 nLast = mv.GetNeuronAt(0, mv.Height - 1);
            神经元 nLast1 = mv.GetNeuronAt(1, mv.Height - 1);
            nLast.添加突触(nIn1.id, 0.5f);
            nLast1.添加突触(nIn1.id, 0.5f);

            for (int i = 0; i < levelCount; i++)
            {
                神经元 ni = mv.GetNeuronAt(0, i + 1);
                ni.模型 = 神经元.模型类型.LIF;
                ni.泄露率 = theLeakRate;
                神经元 ni1 = mv.GetNeuronAt(1, i + 1);
                ni1.模型 = 神经元.模型类型.LIF;
                ni1.泄露率 = theLeakRate;
                神经元 nm = mv.GetNeuronAt(2, i + 1);
                神经元 no = mv.GetNeuronAt(3, i + 1);
                神经元 noP = mv.GetNeuronAt(4, i + 1);
                神经元 noN = mv.GetNeuronAt(5, i + 1);
                noP.标签名 = "O" + i;
                noN.标签名 = "O-" + i;


                ni.添加突触(nm.id, 1);
                ni1.添加突触(nm.id, 1);
                nm.添加突触(no.id, 0.1f);
                no.添加突触(nm.id, 1);
                nClr.添加突触(noN.id, 1f);
                no.添加突触(noN.id, -1f);
                no.添加突触(noP.id, 1f);


                nClr.添加突触(no.id, -1f);
                nRd.添加突触(no.id, 0.99f);

                float weight = GetWeight(4 + i);
                weight += .001f; //differentiates between < and = 
                nIn.添加突触(ni.id, weight);
                nIn1.添加突触(ni1.id, weight);

                for (int j = 0; j < levelCount; j++)
                {
                    if (j != i)
                    {
                        ni.添加突触(mv.GetNeuronAt(0, j + 1).id, -1f);
                        ni1.添加突触(mv.GetNeuronAt(1, j + 1).id, -1f);
                    }
                }
            }
        }

        float GetWeight(int count)
        {
            float decayFactor = (float)Math.Pow((1 - theLeakRate), count);
            float w = 1 / (1 + decayFactor);
            return w;
        }

        
        //called whenever the size of the module rectangle changes, delete if not needed
        //for example, you may choose to reinitialize whenever size changes
        //delete if not needed
        public override void SizeChanged()
        {
            if (mv == null) return; //things aren't initialized yet
            SetUpNeurons(mv.Height - 1);
        }
    }
}
