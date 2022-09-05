using NeuronEngine.CLI;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace BrainSimulator
{

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class 神经元部分参数
    {
        public int id;
        public bool 是否使用;
        public float 最后更改;
        public float 当前更改;
        public float leakRate泄露速度;
        public int 突触延迟;
        public 神经元.模型类型 模型;
        public int dummy; //TODO :Don't know why this is here, it is not required for alignment
                          //TODO : 不知道为什么会在这里，不需要对齐
        public long lastFired;
    };

    public class 神经元处理 : NeuronArrayBase
    {
        public 神经元部分参数 获取神经元部分参数(int i)
        {
            return 变换为神经元(GetNeuron(i));
        }
        public void 设置所有神经元( 神经元 n)
        {
            if (MainWindow.useServers && n.Owner == MainWindow.此神经元数组)
            {
                神经元客户端.设置神经元(n);
            }
            else
            {
                int i = n.id;
                SetNeuronCurrentCharge(i, n.当前更改);
                SetNeuronLastCharge(i, n.最后更改);
                SetNeuronLabel(i, n.标签);
                SetNeuronLeakRate(i, n.leakRate泄露速度);
                SetNeuronModel(i, (int)n.模型);
                SetNeuronAxonDelay(i, n.突触延迟);
            }
        }
        public 神经元 GetNeuronForDrawing(int i)
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
                retVal.是否使用 = GetNeuronInUse(i);
                retVal.标签 = GetNeuronLabel(i);
                retVal.模型 = (神经元.模型类型)GetNeuronModel(i);
                retVal.leakRate泄露速度 = GetNeuronLeakRate(i);
                retVal.突触延迟 = GetNeuronAxonDelay(i);
                return retVal;
            }
        }
        public 神经元 添加突触(神经元 n)
        {
            if (MainWindow.useServers && n.Owner == MainWindow.此神经元数组)
            {
                n.突触列表 = 神经元客户端.GetSynapses(n.id);
                n.突触来源列表 = 神经元客户端.GetSynapsesFrom(n.id);
            }
            return n;
        }
        public 神经元 GetCompleteNeuron(int i, bool fromClipboard = false)
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
                神经元部分参数 n = 获取神经元部分参数(i);

                神经元 retVal = new 神经元();

                retVal.id = n.id;
                retVal.当前更改 = n.当前更改;
                retVal.最后更改 = n.最后更改;
                retVal.lastFired = n.lastFired;
                retVal.是否使用 = n.是否使用;
                retVal.leakRate泄露速度 = n.leakRate泄露速度;
                retVal.模型 = n.模型;
                retVal.突触延迟 = n.突触延迟;

                retVal.标签 = retVal.标签名;// GetNeuronLabel(i);
                if (retVal.ToolTip != "")
                    retVal.标签 += 神经元.toolTipSeparator + retVal.ToolTip;

                retVal.突触列表 = 获取突触列表(i);
                retVal.突触来源列表 = 从列表中获取突触(i);
                return retVal;
            }
        }
        public List<突触> 获取突触列表(int i)
        {
            return 变换为突触列表(GetSynapses(i));
        }
        public List<突触> 从列表中获取突触(int i)
        {
            return 变换为突触列表(GetSynapsesFrom(i));
        }
        List<突触> 变换为突触列表(byte[] input)
        {
            List<突触> retVal = new List<突触>();
            突触 s = new 突触();
            int sizeOfSynapse = Marshal.SizeOf(s);
            int numberOfSynapses = input.Length / sizeOfSynapse;
            byte[] oneSynapse = new byte[sizeOfSynapse];
            for (int i = 0; i < numberOfSynapses; i++)
            {
                int offset = i * sizeOfSynapse;
                for (int k = 0; k < sizeOfSynapse; k++)
                    oneSynapse[k] = input[k + offset];
                GCHandle handle = GCHandle.Alloc(oneSynapse, GCHandleType.Pinned);
                s = (突触)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(突触));
                retVal.Add(s);
                handle.Free();
            }
            return retVal;
        }

        神经元部分参数 变换为神经元(byte[] input)
        {
            神经元部分参数 n = new 神经元部分参数();
            GCHandle handle = GCHandle.Alloc(input, GCHandleType.Pinned);
            n = (神经元部分参数)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(神经元部分参数));
            handle.Free();
            return n;
        }
    }
}
