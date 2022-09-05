//
// Copyright (c) Charles Simon. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
//  

namespace BrainSimulator.Modules
{
    public class ModuleMotor : ModuleBase
    {
        public ModuleMotor()
        {
            minHeight = 6;
            minWidth = 2;
        }

        public override void Fire()
        {
            Init();  //be sure to leave this here
        }

        public override void Initialize()
        {
            神经元 nEnable = mv.GetNeuronAt(0);
            nEnable.标签名 = "Enable";
            神经元 nDisable = mv.GetNeuronAt(1);
            nDisable.标签名 = "Disable";
            nEnable.添加突触(nDisable.Id, -1);
            nDisable.添加突触(nDisable.Id, 1);
            //nDisable.CurrentCharge = 1;
            for (int i = 2; i < mv.NeuronCount; i++)
            {
                神经元 n = mv.GetNeuronAt(i);
                nDisable.添加突触(n.Id, -1);
            }

            神经元 nStop = mv.GetNeuronAt(2);
            nStop.标签名 = "Stop";
            神经元 nGo = mv.GetNeuronAt(3);
            nGo.标签名 = "Go";
            神经元 nLeft = mv.GetNeuronAt(4);
            nLeft.标签名 = "Left";
            神经元 nRight = mv.GetNeuronAt(5);
            nRight.标签名 = "Right";
            nStop.添加突触(nGo.Id, -1);
            nGo.添加突触(nGo.Id, 1);
            神经元 nFwd = GetNeuron("ModuleMove", "^");
            if (nFwd != null)
                nGo.添加突触(nFwd.Id, 1);
            神经元 nKBGo = GetNeuron("KBOut", "Go");
            if (nKBGo != null)
                nKBGo.添加突触(nGo.Id, 1);
            神经元 nKBStop = GetNeuron("KBOut", "Stop");
            if (nKBStop != null)
                nKBStop.添加突触(nStop.Id, 1);
            神经元 nL = GetNeuron("ModuleTurn", "<");
            神经元 nR = GetNeuron("ModuleTurn", ">");
            if (nL != null)
                nLeft.添加突触(nL.Id, 1);
            if (nR != null)
                nRight.添加突触(nR.Id, 1);
            神经元 nLTurn = GetNeuron("KBOut", "LTurn");
            if (nLTurn != null)
                nLTurn.添加突触(nLeft.Id, 1);
            神经元 nRTurn = GetNeuron("KBOut", "RTurn");
            if (nRTurn != null)
                nRTurn.添加突触(nRight.Id, 1);
            nStop.添加突触(nLeft.Id, -1);
            nStop.添加突触(nRight.Id, -1);
            nLeft.添加突触(nLeft.Id, 1);
            nRight.添加突触(nRight.Id, 1);

        }
    }
}
