//
// Copyright (c) Charles Simon. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
//  

using System.Runtime.InteropServices;

namespace BrainSimulator
{
    public class 突触参数
    {
        public int TargetNeuron { get; set; }
        public float Weight { get; set; }
        public bool IsHebbian { get; set; }
        public 突触.modelType Model { get; set; } = 突触.modelType.Fixed;
    }


    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class 突触
    {
        public enum modelType { Fixed, Binary, Hebbian1, Hebbian2 };

        public int 目标神经元字段;
        public float 权重字段;
        public modelType 模型字段 = modelType.Fixed;

        //this is only used in SynapseView but is here so you can add the tooltip when you add a synapse type and 
        //the tooltip will automatically appear in the synapse type selector combobox
        public static string[] modelToolTip = { "Fixed Weight",
            "Binary, one-shot learning",
            "Hebbian w/ range [0,1]",
            "Hebbian w/ range [-1,1]",
            "UNDER DEVELOPMENT, Do not use! Hebbian w/ range [-1/n,1/n] where n is number of synapses",
        };

        //a synapse connects two neurons and has a weight
        //the neuron is "owned" by one neuron and the targetNeuron is the index  of the destination neuron
        public 突触()
        {
            目标神经元字段 = -1;
            权重字段 = 1;
        }
        public 突触(int targetNeuronIn, float weightIn, modelType modelIn)
        {
            目标神经元字段 = targetNeuronIn;
            权重字段 = weightIn;
            模型字段 = modelIn;
        }

        public float 权重
        {
            get => 权重字段;
            set => 权重字段 = value;
        }

        public int 目前神经元
        {
            get => 目标神经元字段;
            set => 目标神经元字段 = value;
        }

        public 突触参数 转突触参数()
        {
            if (模型字段 == modelType.Hebbian1)
            {
                return new 突触参数()
                {
                    Model = 模型字段,
                    TargetNeuron = 目标神经元字段,
                    Weight = 权重,
                    IsHebbian=true 
                };
            }
            else
            {
                return new 突触参数()
                {
                    Model = 模型字段,
                    TargetNeuron = 目标神经元字段,
                    Weight = 权重
                };
            }
        }
    }
}
