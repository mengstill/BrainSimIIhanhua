using NeuronEngine.CLI;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace CsEngineTest
{
    public class Neuron : NeuronPartial
    {
        public string label = "";
        public List<Synapse> synapses = new List<Synapse>();
        public List<Synapse> synapsesFrom = new List<Synapse>();
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Synapse
    {
        public int target;
        public float weight;
        public bool isHebbian;
    };
    public enum modelType { Std, Color, FloatValue, LIF, Random };
    public enum synapseModelType { fixedWeight, binary, hebbian1 };

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class NeuronPartial
    {
        public int id;
        public bool inUse;
        public float lastCharge;
        public float currentCharge;
        public float leakRate;
        public modelType model;
        public long lastFired;
    };

    public class NeuronHandler : 神经元列表Base
    {
        public NeuronPartial GetPartialNeuron(int i)
        {
            return ConvertToNeuron(获取神经元(i));
        }
        public Neuron GetCompleteNeuron(int i)
        {
            Neuron retVal = (Neuron)ConvertToNeuron(获取神经元(i));
            retVal.label = 获取神经元标签(i);
            retVal.synapses = GetSynapsesList(i);
            retVal.synapsesFrom = GetSynapsesFromList(i);
            return retVal;
        }
        public List<Synapse> GetSynapsesList(int i)
        {
            return ConvertToSynapseList(获取突触数组(i));
        }
        public List<Synapse> GetSynapsesFromList(int i)
        {
            return ConvertToSynapseList(GetSynapsesFrom(i));
        }
        List<Synapse> ConvertToSynapseList(byte[] input)
        {
            List<Synapse> retVal = new List<Synapse>();
            Synapse s = new Synapse();
            int sizeOfSynapse = Marshal.SizeOf(s);
            int numberOfSynapses = input.Length / sizeOfSynapse;
            byte[] oneSynapse = new byte[sizeOfSynapse];
            for (int i = 0; i < numberOfSynapses; i++)
            {
                int offset = i * sizeOfSynapse;
                for (int k = 0; k < sizeOfSynapse; k++)
                    oneSynapse[k] = input[k + offset];
                GCHandle handle = GCHandle.Alloc(oneSynapse, GCHandleType.Pinned);
                s = (Synapse)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(Synapse));
                retVal.Add(s);
                handle.Free();
            }
            return retVal;
        }

        NeuronPartial ConvertToNeuron(byte[] input)
        {
            NeuronPartial n = new NeuronPartial();
            GCHandle handle = GCHandle.Alloc(input, GCHandleType.Pinned);
            n = (NeuronPartial)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(NeuronPartial));
            handle.Free();
            return n;
        }


    }
}
