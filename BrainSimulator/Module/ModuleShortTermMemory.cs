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
    public class ModuleShortTermMemory : ModuleBase
    {
        //any public variable you create here will automatically be saved and restored  with the network
        //unless you precede it with the [XmlIgnore] directive
        //[XlmIgnore] 
        //public theStatus = 1;


        //set size parameters as needed in the constructor
        //set max to be -1 if unlimited
        public ModuleShortTermMemory()
        {
            minHeight = 2;
            maxHeight = 100;
            minWidth = 3;
            maxWidth = 3;
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
            mv.GetNeuronAt(0, 0).标签名 = "Rd";
            mv.GetNeuronAt(1, 0).标签名 = "Clr";
            mv.GetNeuronAt(0, 0).添加突触(mv.GetNeuronAt(1,0).id,1);


            for (int i = 1; i < mv.Height; i++)
            {
                神经元 n0 = mv.GetNeuronAt(0, i);
                n0.标签名 = "I" + (i - 1).ToString();
                神经元 n1 = mv.GetNeuronAt(1, i);
                神经元 n2 = mv.GetNeuronAt(2, i);
                n2.标签名 = "O" + (i - 1).ToString();
                n2.模型 = 神经元.模型类型.LIF;
                n2.泄露率 = 0.9f;

                GetNeuron("Rd").添加突触(n1.id, 0.9f);
                GetNeuron("Clr").添加突触(n1.id, -1);
                GetNeuron("Clr").添加突触(n2.id, 0.5f);

                n0.添加突触(n1.id, 0.9f);
                n1.添加突触(n0.id, 1);
                n1.添加突触(n2.id, 0.5f);
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
