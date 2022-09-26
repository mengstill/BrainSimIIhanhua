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
    public class ModuleChainCounter : ModuleBase
    {
        //any public variable you create here will automatically be saved and restored  with the network
        //unless you precede it with the [XmlIgnore] directive
        //[XlmIgnore] 
        //public theStatus = 1;


        //set size parameters as needed in the constructor
        //set max to be -1 if unlimited
        public ModuleChainCounter()
        {
            minHeight = 3;
            maxHeight = 100;
            minWidth = 3;
            maxWidth = 100;
        }


        //fill this method in with code which will execute
        //once for each cycle of the engine
        public override void Fire()
        {
            Init();  //be sure to leave this here

            //if you want the dlg to update, use the following code whenever any parameter changes
            // UpdateDialog();
        }

        //fill this method in with code which will execute once
        //when the module is added, when "initialize" is selected from the context menu,
        //or when the engine restart button is pressed
        public override void Initialize()
        {
            AddSynapses();
        }


        private void AddSynapses()
        {
            Init();
            ClearNeurons(false);
            mv.GetNeuronAt(0, 0).标签名 = "In";

            int chainWidth = 0;
            int chainHeight = 0;

            int theCount = 0;

            if (mv.Height > mv.Width)
            {
                chainWidth = mv.Width - 2;
                chainHeight = mv.Height;
                //add the count neuron labels
                theCount = mv.Height;
                for (int j = 0; j < theCount; j++)
                {
                    mv.GetNeuronAt(mv.Width - 1, j).标签名 = (j + 1).ToString();
                    mv.GetNeuronAt(mv.Width - 2, j).标签名 = "i" + (j + 1).ToString();
                    mv.GetNeuronAt(mv.Width-2,j).模型 = 神经元.模型类.LIF;
                    mv.GetNeuronAt(mv.Width- 2,j).泄露率属性 = 1f;
                }
            }
            else
            {
                chainHeight = mv.Height - 2;
                chainWidth = mv.Width;
                //add the count neuron labels
                theCount = mv.Width;
                for (int j = 0; j < theCount; j++)
                {
                    mv.GetNeuronAt(j, mv.Height - 1).标签名 = (j + 1).ToString();
                    mv.GetNeuronAt(j, mv.Height - 2).标签名 = "i" + (j + 1).ToString();
                    mv.GetNeuronAt(j, mv.Height - 2).模型 = 神经元.模型类.LIF;
                    mv.GetNeuronAt(j, mv.Height - 2).泄露率属性 = 1f;
                }
            }

            //add the synapses for the chain
            for (int i = 0; i < chainHeight; i++)
            {
                for (int j = 0; j < chainWidth - 1; j++)
                {
                    mv.GetNeuronAt(j, i).添加突触(mv.GetNeuronAt(j + 1, i).id, 1);
                }
                if (i < chainHeight - 1)
                {
                    mv.GetNeuronAt(chainWidth - 1, i).添加突触(mv.GetNeuronAt(0, i + 1).id, 1);
                }
            }

            //add the synapses to the count neurons
            for (int k = 0; k < theCount; k++)
            {
                float theWeight = 1 / (float)(k + 1);
                神经元 target = GetNeuron("i" + (k + 1).ToString());
                for (int i = 0; i < chainHeight; i++)
                {
                    for (int j = 0; j < chainWidth; j++)
                    {
                        mv.GetNeuronAt(j, i).添加突触(target.id, theWeight);
                    }
                }
            }

            for (int k = 0; k < theCount; k++)
            {
                神经元 source = GetNeuron("i" + (k + 1).ToString());
                神经元 target = GetNeuron((k + 1).ToString());
                source.添加突触(target.id, 1);
                for (int j = k-1; j >= 0; j--)
                {
                    target = GetNeuron((j + 1).ToString());
                    source.添加突触(target.id, -1);
                }

            }
        }

        //the following can be used to massage public data to be different in the xml file
        //delete if not needed
        public override void SetUpBeforeSave()
        {
        }
        public override void 设置后负荷()
        {
        }

        //called whenever the size of the module rectangle changes
        //for example, you may choose to reinitialize whenever size changes
        //delete if not needed
        public override void SizeChanged()
        {
            if (mv == null) return;
            AddSynapses();
        }
    }
}
