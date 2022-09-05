//
// Copyright (c) [Name]. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
//  

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Serialization;

namespace BrainSimulator.Modules
{
    public class ModuleBoundaryByNeurons : ModuleBase
    {
        //any public variable you create here will automatically be stored with the network
        //unless you precede it with the [XmlIgnore] directive
        //[XlmIgnore] 
        //public theStatus = 1;

        //fill this method in with code which will execute
        //once for each cycle of the engine
        public override void Fire()
        {
            Init();  //be sure to leave this here

            模块视图 naSource = theNeuronArray.FindModuleByLabel("ImageFile");
        }
        public override void Initialize()
        {
            模块视图 naSource = theNeuronArray.FindModuleByLabel("ImageFile");
            if (naSource == null)
            {
                MessageBox.Show("Boundary module requires ImageFile module for input.");
                return;
            }
            foreach (神经元 n in naSource.Neurons)
            {
                n.Clear();
                n.Model = 神经元.模型类型.Color;
            }
            foreach (神经元 n in mv.Neurons)
            {
                n.Clear();
                n.Model = 神经元.模型类型.LIF;
                n.LeakRate = 1f;
            }

            theNeuronArray.获取神经元位置(mv.FirstNeuron, out int col, out int row);

            mv.Width = naSource.Width * 6;
            mv.Height = naSource.Height;

            if (col + mv.Width >= theNeuronArray.Cols ||
                row + mv.Height >= theNeuronArray.行数)
            {
                MessageBox.Show(mv.Label + " would exceed neuron array boundaries.");
                return;
            }

            //Up Down Left Right angles: UL DR UR DL
            for (int x = 0; x < naSource.Width; x++)
                for (int y = 0; y < naSource.Height; y++)
                {
                    神经元 n0 = naSource.GetNeuronAt(x, y);
                    神经元 n1 = naSource.GetNeuronAt(x + 1, y);
                    神经元 n2 = naSource.GetNeuronAt(x, y + 1);
                    神经元 n3 = naSource.GetNeuronAt(x + 1, y + 1);


                    //the letter indicates which side is white...WB = L BW = R
                    int x1 = x * 6;
                    神经元 nL = mv.GetNeuronAt(x1, y);
                    神经元 nR = mv.GetNeuronAt(x1 + 1, y);
                    神经元 nU = mv.GetNeuronAt(x1 + 2, y);
                    神经元 nD = mv.GetNeuronAt(x1 + 3, y);
                    神经元 nXU = mv.GetNeuronAt(x1 + 4, y); //angle up
                    神经元 nXD = mv.GetNeuronAt(x1 + 5, y); //angle down
                    /*
                     * L:   10
                     *      10
                     * R:   01
                     *      01
                     * U:   11
                     *      00
                     * D:   00
                     *      11
                     * XU:  01  //only one of the two 0's is needed
                     *      10
                     * XD:  01
                     *      10
                     * 
                     * */

                    nL.标签名 = "*|";
                    nR.标签名 = "|*";
                    nU.标签名 = @"/*\";
                    nD.标签名 = @"\*/";
                    nXU.标签名 = @"/";
                    nXD.标签名 = @"\";

                    AddSynapse(n0, nXU, -.5f);
                    AddSynapse(n1, nXU, .75f);
                    AddSynapse(n2, nXU, .75f);
                    AddSynapse(n3, nXU, -.5f);

                    AddSynapse(n0, nXD, .75f);
                    AddSynapse(n1, nXD, -.5f);
                    AddSynapse(n2, nXD, -.5f);
                    AddSynapse(n3, nXD, .75f);


                    AddSynapse(n0, nL, .5f);
                    AddSynapse(n1, nL, -1f);
                    AddSynapse(n2, nL, .5f);
                    AddSynapse(n3, nL, -1f);

                    AddSynapse(n0, nR, -1f);
                    AddSynapse(n1, nR, .5f);
                    AddSynapse(n2, nR, -1f);
                    AddSynapse(n3, nR, .5f);

                    AddSynapse(n0, nU, .5f);
                    AddSynapse(n1, nU, .5f);
                    AddSynapse(n2, nU, -1f);
                    AddSynapse(n3, nU, -1f);

                    AddSynapse(n0, nD, -1f);
                    AddSynapse(n1, nD, -1f);
                    AddSynapse(n2, nD, .5f);
                    AddSynapse(n3, nD, .5f);


                }
        }

        void AddSynapse(神经元 source, 神经元 dest, float weight)
        {
            if (source == null || dest == null) return;
            source.添加突触(dest.id, weight);
        }

    }
}

