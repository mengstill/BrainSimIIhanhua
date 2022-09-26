using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrainSimulator
{
    public class 神经元数组base
    {
        int 数组大小 = 0;
        int 线程总数 = 124;
        public static List<神经元> 神经元数组 = new();
        long 激活数量 = 0;
        long 循环数 = 0;
        public static int refractoryDelay;

        public static ConcurrentQueue<突触> remoteQueue;
        public static ConcurrentQueue<神经元> fire2Queue;
        static List<ulong> 激活列表1;
        static List<ulong> 激活列表2;

        public string GetRemoteFiringString()
        {
            string retVal = "";
            突触 s = new 突触();
            int count = 0;
            while (remoteQueue.TryDequeue(out s) && count++ < 90) //splits up long strings for transmission
            {
                retVal += s.获取目标神经元().id +" ";
                retVal += ((float)s.权重) + " ";
                retVal += ((int)s.模型字段) + " ";
            }
            return retVal;
        }
        public 突触 GetRemoteFiringSynapse()
        {
            return new 突触();
        }
        public 神经元数组base()
        {
        }
        public void Inttiallize(int 此大小, 神经元.模型类 模型)
        {
            数组大小 = 此大小;
            int expandedSize = 数组大小;
            if (expandedSize % 64 != 0)
                expandedSize = ((expandedSize / 64) + 1) * 64;
            数组大小 = expandedSize;
            神经元数组.Capacity=expandedSize;
            for (int i = 0; i < expandedSize; i++)
            {
                神经元 n = new(i);
                //n.SetModel(NeuronBase::modelType::LIF);  /for testing
                神经元数组.Add(n);
            }
            激活列表1.Capacity = expandedSize / 64;
            激活列表2.Capacity = expandedSize / 64;

            int fireListCount = expandedSize / 64;
            for (int i = 0; i < fireListCount; i++)
            {
                激活列表1.Add(0xffffffffffffffff);
                激活列表2.Add(0);

            }
        }
        public long 获取次代() { return 循环数; }
        public void 设置次代(long i) { 循环数 = i; }
        public 神经元 获取神经元(int i)
        {
            return 神经元数组[i];
        }
        public int 获取数组大小()
        {
            return 数组大小;
        }
        public long 获取突触总数()
        {
            long count = 0;
            Parallel.For(0, 线程总数, 
            (int value) =>
                {
                    int start = 0, end = 0;
                    GetBounds(value, ref start, ref end);
                    for (int i = start; i < end; i++)
                    {
                        count += (long)获取神经元(i).获取突触数量(); ;
                    }
                } 
            );
            return count;
        }
        public long 获取使用中神经元数量()
        {
            long count = 0;
            Parallel.For(0, 线程总数, (int value)=> {
                int start = 0, end = 0;
                GetBounds(value,ref start,ref end);
                for (int i = start; i < end; i++)
                {
                    if (获取神经元(i).是否使用)
                        count++;
                }
            });
            return count;
        }
        public void GetBounds(int taskID, ref int start, ref int end)
        {
            int numberToProcess = 数组大小 / 线程总数;
            int remainder = 数组大小 % 线程总数;
            start = numberToProcess * taskID;
            end = start + numberToProcess;
            if (taskID < remainder)
            {
                start += taskID;
                end = start + numberToProcess + 1;
            }
            else
            {
                start += remainder;
                end += remainder;
            }
        }
        void GetBounds64(int 任务ID, ref int 起始, ref int 结束)
        {
            int 每线程数量 = 数组大小 / 线程总数;
            if (每线程数量 % 64 == 0)
            {
                int 余数 = 数组大小 % 线程总数;
                起始 = 每线程数量 * 任务ID;
                结束 = 起始 + 每线程数量;
                if (任务ID < 余数)
                {
                    起始 += 任务ID;
                    结束 = 起始 + 每线程数量 + 1;
                }
                else
                {
                    起始 += 余数;
                    结束 += 余数;
                }
            }
            else
            {
                每线程数量 = (每线程数量 / 64 + 1) * 64;
                int 可用线程总数 = 数组大小 / 每线程数量;
                if (任务ID > 可用线程总数)
                {
                    起始 = 0;
                    结束 = 0;
                    return;
                }
                int remainder = 数组大小 % 每线程数量;
                起始 = 每线程数量 * 任务ID;
                结束 = 起始 + 每线程数量;
                if (任务ID == 可用线程总数)
                {
                    结束 = 起始 + remainder;
                }
            }
        }

        public void Fire()
        {
            if (是否需要清除激活列表组)
                清除激活列表组();
            是否需要清除激活列表组 = false;
            循环数++;
            激活数量 = 0;

            //parallel_for(0, 线程总数, [&](int value) {
            //    神经元进程1(value);
            //});
            //parallel_for(0, 线程总数, [&](int value) {
            //    神经元进程2(value);
            //});

            神经元进程3(0);

        }

        public int 获取激活神经元数量()
        {
            return (int)激活数量;
        }
        public int 获取线程总数()
        {
            return 线程总数;
        }
        public void 设置线程总数(int i)
        {
            线程总数 = i;
        }

        public  int GetRefractoryDelay()
        {
            return refractoryDelay;
        }
        public void SetRefractoryDelay(int i)
        {
            refractoryDelay = i;
        }
        public static void 添加神经元到激活列表组(int id)
        {
            int index = id / 64;
            int offset = id % 64;
            ulong bitMask = 0x1;
            bitMask = bitMask << offset;
            激活列表1[index] |= bitMask;
        }
        public static void 清除激活列表组()
        {
            for (int i = 0; i < 激活列表1.Count; i++)
            {
                激活列表1[i] = 0xffffffffffffffff;
                激活列表2[i] = 0;
            }
        }
        void 神经元进程1(int taskID)
        {
            int start = 0, end = 0;
            GetBounds64(taskID, ref start, ref end);
            start /= 64;
            end /= 64;
            for (int i = start; i < end; i++)
            {
                ulong tempVal = 激活列表1[i];

                激活列表1[i] = 0;
                ulong bitMask = 0x1;
                for (int j = 0; j < 64; j++)
                {
                    if ((tempVal & bitMask) > 0)
                    {
                        int neuronID = i * 64 + j;
                        神经元 theNeuron = 获取神经元(neuronID);
                        if (!theNeuron.Fire1(循环数))
                        {
                            tempVal &= ~bitMask; //clear the bit if not firing for 2nd phase
                        }
                        else
                            激活数量++;
                    }
                    bitMask = bitMask << 1;
                }
                激活列表2[i] = tempVal;
            }
        }//这些都是未联机的，因此分析器更有意义

        void 神经元进程2(int taskID)
        {
            int start = 0, end = 0;
            GetBounds64(taskID, ref start, ref end);
            start /= 64;
            end /= 64;
            for (int i = start; i < end; i++)
            {
                ulong tempVal = 激活列表2[i];
                ulong bitMask = 0x1;
                for (int j = 0; j < 64; j++)
                {
                    if ((tempVal & bitMask) > 0)
                    {
                        神经元 theNeuron = 获取神经元(i * 64 + j);
                        theNeuron.Fire2();
                    }
                    bitMask = bitMask << 1;
                }

            }
        }
        void 神经元进程3(int taskID)
        {
            for (int i = 0; i < 数组大小; i++)
            {
                神经元 theNeuron = 获取神经元(i);
                theNeuron.Fire3();
            }
        }


        public static bool 是否需要清除激活列表组;

        public void Initialize(int numberOfNeurons) { }
        public int 获取线程数()
        {
            return 线程总数;
        }
        public void 设置线程数量(int i)
        {
            线程总数 = i;
        }
        //public int 获取激活的神经元数量() 
        //{
        //    return 获取激活数量();
        //}
        //public long 获取总突触数() 
        //{
        //    return 获取突触总数();
        //}
        //public long 获取使用中的神经元总数()
        //{
        //    return 获取使用中神经元数量();
        //}
        public float GetNeuronLastCharge(int i) 
        {
            神经元 n=获取神经元(i);
            return n.lastCharge;
        }
        public void SetNeuronLastCharge(int i, float value) 
        {
            神经元 n = 获取神经元(i);
            n.lastCharge = value;
        }
        public void SetNeuronCurrentCharge(int i, float value) 
        {
            神经元 n = 获取神经元(i);
            n.currentCharge = value;
        }
        public void AddToNeuronCurrentCharge(int i, float value) 
        {
            神经元 n = 获取神经元(i);
            n.AddToCurrentValue(value);
        }
        public bool 获取神经元是否使用中(int i)
        {
            神经元 n = 获取神经元(i);
            return n.InUse();
        }
        public string 获取神经元标签(int i) 
        {
            string 标签字符串 = 获取神经元(i).标签;
            if (标签字符串.Length != 0)
            {
                return 标签字符串;
            }
            else
            {
                return "";
            }
        }

        public string GetRemoteFiring() 
        {
            return GetRemoteFiringString();
        }
        public List<突触> GetRemoteFiringSynapses()
        {
            List<突触> 突触列表 = new();
            突触 s1= GetRemoteFiringSynapse();
            while (s1.获取目标神经元() != null)
            {
                突触列表.Add(s1 );
                s1= GetRemoteFiringSynapse();
            }
            return 突触列表;
        }

        public void 设置神经元标签(int i, ref String newLabel) 
        {
            获取神经元(i).标签 = newLabel;
        }
        public int 获取神经元模型(int i) 
        {
            神经元 n=获取神经元(i);
            return (int)n.模型;
        }
        public void 设置神经元模型(int i, int model)
        {
            神经元 n = 获取神经元(i);
            n.模型 = (神经元.模型类)model;
        }
        public float GetNeuronLeakRate(int i) 
        {
            神经元 n = 获取神经元(i);
            return n.泄露率属性;
        }
        public void SetNeuronLeakRate(int i, float value) 
        {
            神经元 n = 获取神经元(i);
            n.泄露率属性=value;
        }
        public int GetNeuronAxonDelay(int i)
        {
            神经元 n = 获取神经元(i);
            return n.AxonDelay;
        }
        public void SetNeuronAxonDelay(int i, int value)
        {
            神经元 n = 获取神经元(i);
            n.AxonDelay = value;
        }
        public long GetNeuronLastFired(int i) 
        {
            神经元 n = 获取神经元(i);
            return n.lastFired;

        }
        public List<突触> 获取突触数组(int src) 
        {
            神经元 n=获取神经元(src);
            lock (n.锁) 
            {
                List<突触> 突触列表 = n.获取突触数组();
                return 突触列表;
            }
            
        }
        public List<突触> GetSynapsesFrom(int src) 
        {
            神经元 n = 获取神经元(src);
            lock (n.锁)
            {
                List<突触> 突触列表 = n.GetSynapsesFrom();
                return 突触列表;
            }
        }

        public void 添加突触(int src, int dest, float weight, int model, bool noBackPtr) 
        {
            if (src < 0) return;
            神经元 n = 获取神经元(src);
            if (dest < 0)
                n.添加突触(获取神经元(dest), weight, (突触.模型类型)model, noBackPtr);

            else
                n.添加突触(获取神经元(dest), weight, (突触.模型类型)model, noBackPtr);

        }
        public void 添加输入突触(int src, int dest, float weight, int model) 
        {
            if (dest < 0) return;
            神经元 n = 获取神经元(dest);
            if (src < 0)
                n.AddSynapseFrom(获取神经元(src), weight, (突触.模型类型)model);

            else
                n.AddSynapseFrom(获取神经元(src), weight, (突触.模型类型)model);

        }
        public void 删除突触(int src, int dest) 
        {
            if (src < 0) return;
            神经元 n = 获取神经元(src);
            if (dest < 0)
                n.删除突触(获取神经元(dest));

            else
                n.删除突触(获取神经元(dest));
        }
        public void 删除输入突触(int src, int dest) 
        {
            if (dest < 0) return;
            神经元 n = 获取神经元(dest);
            if (src < 0)
                n.删除突触(获取神经元(src));

            else
                n.删除突触(获取神经元(src));
        }
    }
}
