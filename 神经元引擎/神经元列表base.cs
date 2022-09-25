using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace 神经元引擎
{
    internal class 神经元列表base
    {
        int 数组大小 = 0;
        int 线程总数 = 124;
        List<神经元base> 神经元数组;
        long 激活数量 = 0;
        long 循环数 = 0;
        static int refractoryDelay;

        //Concurrency::concurrent_queue<突触Base> 神经元列表Base::remoteQueue;
        //Concurrency::concurrent_queue<神经元Base*> 神经元列表Base::fire2Queue;
        static List<ulong> 激活列表1;
        static List<ulong> 激活列表2;

        public string GetRemoteFiringString()
        {
            //string retVal("");
            //突触base s = new 突触base();
            //int count = 0;
            //while (remoteQueue.try_pop(s) && count++ < 90) //splits up long strings for transmission
            //{
            //    retVal += std::to_string(-(long long)s.获取目标神经元()) +" ";
            //    retVal += std::to_string((float)s.获取权重()) + " ";
            //    retVal += std::to_string((int)s.获取模型()) + " ";
            //}
            //return retVal;

            return "";
        }
        public 突触base GetRemoteFiringSynapse()
        {
            return new 突触base();
        }
        public 神经元列表base()
        {
        }
        public void Inttiallize(int 此大小,神经元base.模型类型 模型)
        {
            数组大小 = 此大小;
            int expandedSize = 数组大小;
            if (expandedSize % 64 != 0)
                expandedSize = ((expandedSize / 64) + 1) * 64;
            数组大小 = expandedSize;
            //神经元数组.reserve(expandedSize);
            //for (int i = 0; i < expandedSize; i++)
            //{
            //    神经元base n(i);
            //    //n.SetModel(NeuronBase::modelType::LIF);  /for testing
            //    神经元数组.push_back(n);
            //}
            //激活列表1.reserve(expandedSize / 64);
            //激活列表2.reserve(expandedSize / 64);

            //int fireListCount = expandedSize / 64;
            //for (int i = 0; i < fireListCount; i++)
            //{
            //    激活列表1.push_back(0xffffffffffffffff);
            //    激活列表2.push_back(0);

            //}
        }
        public long 获取次代() { return 循环数; }
        public void 设置次代(long i) { 循环数 = i; }
        public 神经元base 获取神经元(int i)
        {
            return  神经元数组[i];
        }
        public int 获取数组大小()
        {
            return 数组大小;
        }
        public long GetTotalSynapseCount()
        {
            long count = 0;
            //parallel_for(0, 线程总数, [&](int value) {
            //    int start=0, end=0;
            //    GetBounds(value,ref start,ref end);
            //    for (int i = start; i < end; i++)
            //    {
            //        count += (long)获取神经元(i).获取突触数量(); ;
            //    }
        
		    return count;
        }
        public long 获取使用中神经元数量()
        {
            long count = 0;
            //parallel_for(0, 线程总数, [&](int value) {
            //    int start, end;
            //    GetBounds(value, start, end);
            //    for (int i = start; i < end; i++)
            //    {
            //        if (获取神经元(i)->GetInUse())
            //            count++;
            //    }
            //});
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

        public int 获取激活数量()
        {
            return (int)激活数量;
        }
        public int 获取线程总数()
        {
            return 线程总数;
        }
        public void 设置线程总数(int i)
        {
            线程总数=i;
        }

        public static int GetRefractoryDelay()
        {
            return refractoryDelay;
        }
        public static void SetRefractoryDelay(int i)
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
            int start=0, end = 0;
            GetBounds64(taskID,ref start,ref end);
            start /= 64;
            end /= 64;
            for (int i = start; i < end; i++)
            {
                ulong tempVal = 激活列表1[i];

                激活列表1[i] = 0;
                ulong bitMask = 0x1;
                for (int j = 0; j < 64; j++)
                {
                    if ((tempVal & bitMask)>0)
                    {
                        int neuronID = i * 64 + j;
                        神经元base theNeuron = 获取神经元(neuronID);
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
            int start=0, end=0;
            GetBounds64(taskID,ref start,ref end);
            start /= 64;
            end /= 64;
            for (int i = start; i < end; i++)
            {
                ulong tempVal = 激活列表2[i];
                ulong bitMask = 0x1;
                for (int j = 0; j < 64; j++)
                {
                    if ((tempVal & bitMask)>0)
                    {
                        神经元base theNeuron = 获取神经元(i * 64 + j);
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
                神经元base theNeuron = 获取神经元(i);
                theNeuron.Fire3();
            }
        }


        public static bool 是否需要清除激活列表组;


        //static concurrency::concurrent_queue<突触Base> remoteQueue;
        //static concurrency::concurrent_queue<神经元Base*> fire2Queue;
    }
}
