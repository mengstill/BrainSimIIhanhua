//using NeuronEngine.CLI;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace BrainSimulator
{

    public class 神经元数组操作 : /*神经元列表Base*/神经元数组base
    {
        public 神经元 获取神经元部分参数(int i)
        {
            return 获取神经元(i);
        }
        public void 设置所有神经元( 神经元 n)
        {
            if (MainWindow.useServers && n.所有者 == MainWindow.此神经元数组)
            {
                神经元客户端.设置神经元(n);
            }
            else
            {
                int i = n.id;
                SetNeuronCurrentCharge(i, n.currentCharge);
                SetNeuronLastCharge(i, n.lastCharge);
                设置神经元标签(i,ref n.标签);
                SetNeuronLeakRate(i, n.泄露率);
                设置神经元模型(i, (int)n.模型字段);
                SetNeuronAxonDelay(i, n.突触延迟);
            }
        }
        public 神经元 获取用于绘图的神经元(int i)
        {
            if (MainWindow.useServers)
            {
                神经元 retVal = 神经元客户端.获取神经元(i);
                return retVal;
            }
            else
            {
                神经元 retVal = new 神经元();
                retVal.id = i;
                retVal.LastCharge = GetNeuronLastCharge(i);
                retVal.是否使用 = 获取神经元是否使用中(i);
                retVal.标签 = 获取神经元标签(i);
                retVal.模型字段 = (神经元.模型类型)获取神经元模型(i);
                retVal.泄露率 = GetNeuronLeakRate(i);
                retVal.突触延迟 = GetNeuronAxonDelay(i);
                return retVal;
            }
        }
        public 神经元 添加突触(神经元 n)
        {
            if (MainWindow.useServers && n.所有者 == MainWindow.此神经元数组)
            {
                n.synapses = 神经元客户端.GetSynapses(n.id);
                n.synapsesFrom = 神经元客户端.获取突触(n.id);
            }
            return n;
        }
        public 神经元 获取完整的神经元(int i, bool fromClipboard = false)
        {
            if (MainWindow.useServers && !fromClipboard)
            {
                神经元 retVal = 神经元客户端.获取神经元(i);
                //retVal.synapses = NeuronClient.GetSynapses(i);
                //retVal.synapsesFrom = NeuronClient.GetSynapsesFrom(i);
                return retVal;
            }
            else
            {
                神经元 n = 获取神经元部分参数(i);

                神经元 retVal = new 神经元();

                retVal.id = n.id;
                retVal.currentCharge = n.currentCharge;
                retVal.lastCharge = n.lastCharge;
                retVal.lastFired = n.lastFired;
                retVal.是否使用 = n.是否使用;
                retVal.泄露率 = n.泄露率;
                retVal.模型字段 = n.模型字段;
                retVal.突触延迟 = n.突触延迟;

                retVal.标签 = retVal.标签名;// GetNeuronLabel(i);
                if (retVal.ToolTip != "")
                    retVal.标签 += 神经元.toolTipSeparator + retVal.ToolTip;

                retVal.synapses = 获取突触列表(i);
                retVal.synapsesFrom = 从列表中获取突触(i);
                return retVal;
            }
        }
        public List<突触> 获取突触列表(int i)
        {
            return 获取突触数组(i);
        }
        public List<突触> 从列表中获取突触(int i)
        {
            return GetSynapsesFrom(i);
        }
        //List<突触> 变换为突触列表(byte[] input)
        //{
        //    List<突触> retVal = new List<突触>();
        //    突触 s = new 突触();
        //    int sizeOfSynapse = Marshal.SizeOf(s);
        //    int numberOfSynapses = input.Length / sizeOfSynapse;
        //    byte[] oneSynapse = new byte[sizeOfSynapse];
        //    for (int i = 0; i < numberOfSynapses; i++)
        //    {
        //        int offset = i * sizeOfSynapse;
        //        for (int k = 0; k < sizeOfSynapse; k++)
        //            oneSynapse[k] = input[k + offset];
        //        GCHandle handle = GCHandle.Alloc(oneSynapse, GCHandleType.Pinned);
        //        s = (突触)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(突触));
        //        retVal.Add(s);
        //        handle.Free();
        //    }
        //    return retVal;
        //}

        //神经元结构 变换为神经元(byte[] input)
        //{
        //    神经元结构 n = new 神经元结构();
        //    GCHandle handle = GCHandle.Alloc(input, GCHandleType.Pinned);
        //    n = (神经元结构)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(神经元结构));
        //    handle.Free();
        //    return n;
        //}
    }
}
