using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using 神经元引擎;

namespace 神经元引擎
{
    public class 神经元base
    {
        public enum 模型类型
        {
            Std, Color, FloatValue, LIF, Random, Burst, Always
        }
        public float lastCharge = 0;
        public List<突触base> 突触数组;

        float currentCharge = 0;
        模型类型 模型 = 模型类型.Std;

        float 泄露率 = 0.1f; //仅用于LIF模型
        int nextFiring = 0; //仅用于随机模型和连续模型
        long lastFired = 0; //上次触发的时间戳
        int id = -1; //一个非法值，它将捕获
        string label = "";
        int 突触延迟 = 0;
        int 突触计数 = 0;

        List<突触base> synapsesFrom;

        int vectorlock = 0;

        const float threshold = 1.0f;

        public 神经元base(int ID)
        {
            泄露率 = 0.1f;
            nextFiring = 0;
            id = ID;
        }
        public int 获取ID()
        {
            return id;
        }
        public 模型类型 获取模型()
        {
            return 模型;
        }
        public void 设置模型(模型类型 模型)
        {
            this.模型 = 模型;
        }
        public float getlastcharge()
        {
            return lastCharge;
        }
        public void setlastcharget(float 值)
        {
            神经元列表base.是否需要清除激活列表组 = true;
            lastCharge = 值;
        }
        public float GetCurrentCharge()
        {
            return currentCharge;
        }

        public void SetCurrentCharge(float value)
        {
            神经元列表base.是否需要清除激活列表组 = true;
            currentCharge = value;
        }
        public float 获取泄露率()
        {
            return 泄露率;
        }
        public void 设置泄露率(float value)
        {
            泄露率 = value;
        }
        public int GetAxonDelay()
        {
            return 突触延迟;
        }
        public void SetAxonDelay(int value)
        {
            突触延迟 = value;
        }
        public long GetLastFired()
        {
            return lastFired;
        }
        public string 获取标签()
        {
            return label;

        }
        public void 设置标签(string 标签)
        {
            //delete label;
            //label = NULL;
            //size_t len = wcslen(newLabel);
            //if (len > 0)
            //{
            //    label = new wchar_t[len + 2];
            //    wcscpy_s(label, len + 2, newLabel);
            //}
        }

        public bool GetInUse() 
        {
            bool retVal = (label != null) || (突触数组 != null && 突触数组.Count != 0) || (synapsesFrom != null && synapsesFrom.Count != 0) || (模型 != 模型类型.Std);
            return retVal;
        }


        public void AddSynapseFrom(ref 神经元base n, float weight, 突触base.模型类型 model)
        {
            //while (vectorLock.exchange(1) == 1) { }

            突触base s1=new 突触base();
            s1.设置权重(weight);
            s1.设置目标神经元(n);
            s1.设置模型(model);

            if (synapsesFrom ==null)
            {
                synapsesFrom = new List<突触base>();
                synapsesFrom.Capacity=10;
            }
            for (int i = 0; i < synapsesFrom.Count; i++)
            {
                if (synapsesFrom[i].获取目标神经元() == n)
                {
                    //update an existing synapse
                    synapsesFrom[i].设置权重(weight);
                    synapsesFrom[i].设置模型(model);
                    //goto alreadyInList;
                }
            }
            //else create a new synapse
            synapsesFrom.Add(s1);
        //alreadyInList:
            //vectorLock = 0;
        }
        public void 添加突触(ref 神经元base n, float weight, 突触base.模型类型 model = 突触base.模型类型.Fixed, bool noBackPtr = true)
        {
            //while (vectorLock.exchange(1) == 1) { }

            突触base s1 = new 突触base();
            s1.设置权重(weight);
            s1.设置目标神经元(n);
            s1.设置模型(model);

            if (突触数组 == null)
            {
                突触数组 = new List<突触base>();
                突触数组.Capacity=100;
            }
            for (int i = 0; i < 突触数组.Count; i++)
            {
                if (突触数组[i].获取目标神经元() == n)
                {
                    //update an existing synapse
                    突触数组[i].设置权重(weight);
                    突触数组[i].设置模型(model);
                    //goto alreadyInList;
                }
            }
            //else create a new synapse
            突触数组.Add(s1);
        //alreadyInList:
        //    vectorLock = 0;

            if (noBackPtr) return;

            //now add the synapsesFrom entry to the target neuron
            //this requires locking because multiply neurons may link to a single neuron simultaneously requiring backpointers.
            //The previous does not lock because you don't write to the same neuron from multiple threads

            //while (n->vectorLock.exchange(1) == 1) { }
            突触base s2=new 突触base();
            s2.设置目标神经元(this);
            s2.设置权重(weight);
            s2.设置模型(model);

            if (n.synapsesFrom == null)
            {
                n.synapsesFrom = new List<突触base>();
                n.synapsesFrom.Capacity=10;
            }
            for (int i = 0; i < n.synapsesFrom.Count; i++)
            {
                突触base s = n.synapsesFrom[i];
                if (n.synapsesFrom[i].获取目标神经元() == this)
                {
                    n.synapsesFrom[i].设置权重(weight);
                    n.synapsesFrom[i].设置模型(model);
                    //goto alreadyInList2;
                }
            }
            n.synapsesFrom.Add(s2);
        //alreadyInList2:
        //    n->vectorLock = 0;
            return;
        }

        public void 删除突触(ref 神经元base n)
        {
            //while (vectorLock.exchange(1) == 1) { }
            if (突触数组 != null)
            {
                for (int i = 0; i < 突触数组.Count; i++)
                {
                    if (突触数组[i].获取目标神经元() == n)
                    {
                        突触数组.Remove(突触数组[i]);
                        break;
                    }
                }
                if (突触数组.Count == 0)
                {
                    //delete synapses;
                    突触数组 = null;
                }
            }
            //vectorLock = 0;
            //if (((long)n >> 63) != 0) return;
            //while (n->vectorLock.exchange(1) == 1) { }
            if (n.synapsesFrom != null)
            {
                for (int i = 0; i < n.synapsesFrom.Count; i++)
                {
                    突触base s = n.synapsesFrom[i];
                    if (s.获取目标神经元() == this)
                    {
                        n.synapsesFrom.Remove(n.synapsesFrom[i]);
                        //if (n.synapsesFrom.Count == 0)
                        //{
                        //    //delete n.synapsesFrom;
                        //    n.synapsesFrom = null;
                        //}
                        break;
                    }
                }
            }
            //n->vectorLock = 0;
        }
        public int 获取突触数量()
        {
            if (突触数组 == null) return 0;
            return (int)突触数组.Count;
        }

        public List<突触base> 获取突触数组()
        {
            if (突触数组 == null)
            {
                List<突触base> 突触列表 = new List<突触base>();
                return 突触列表;
            }
            else
            {
                List<突触base> 突触列表 = new List<突触base>(突触数组);
                return 突触列表;
            }

        }
        public List<突触base> GetSynapsesFrom()
        {
            if(synapsesFrom == null)
            {
                List<突触base> 突触列表 = new List<突触base>();
                return 突触列表;
            }
            else
            {
                List<突触base> 突触列表 = new List<突触base>(synapsesFrom);
                return 突触列表;
            }
        }
        public void GetLock()
        {
            //while (vectorLock.exchange(1) == 1) { }
        }
        public void ClearLock()
        {
            //vectorLock = 0;
        }

        public void AddToCurrentValue(float weight)
        {
            currentCharge = currentCharge + weight;
            if (currentCharge >= threshold)
                神经元列表base.添加神经元到激活列表组(id);
        }
        double rand_normal(double mean, double stddev)
        {
            //static double n2 = 0.0;
            //static int n2_cached = 0;
            //if (!n2_cached)
            //{
            //    double x, y, r;
            //    do
            //    {
            //        x = 2.0 * rand() / RAND_MAX - 1;
            //        y = 2.0 * rand() / RAND_MAX - 1;

            //        r = x * x + y * y;
            //    } while (r == 0.0 || r > 1.0);
            //    {
            //        double d = sqrt(-2.0 * log(r) / r);
            //        double n1 = x * d;
            //        n2 = y * d;
            //        double result = n1 * stddev + mean;
            //        n2_cached = 1;
            //        return result;
            //    }
            //}
            //else
            //{
            //    n2_cached = 0;
            //    return n2 * stddev + mean;
            //}

            return 0;
        }


        public bool Fire1(long cycle)
        {
            if (泄露率<0f) return false;
            if (模型 == 模型类型.Color)
            {
                神经元列表base.添加神经元到激活列表组(id);
                return true;
            }
            //if (model == modelType::FloatValue) return false;
            if (模型 == 模型类型.Always)
            {
                nextFiring--;
                if (泄露率 >= 0 && nextFiring <= 0) //泄漏率是标准偏差
                {
                    currentCharge = currentCharge + threshold;
                }
                if (泄露率 >= 0) //负泄漏率表示“禁用”
                    神经元列表base.添加神经元到激活列表组(id);
            }
            if (模型 == 模型类型.Random)
            {
                nextFiring--;
                if (泄露率 >= 0 && nextFiring <= 0) //泄漏率是标准偏差
                {
                    currentCharge = currentCharge + threshold;
                }
                if (泄露率 >= 0) //负泄漏率表示“禁用”
                    神经元列表base.添加神经元到激活列表组(id);
            }
            if (模型 == 模型类型.Burst)
            {
                if (currentCharge < 0)
                {
                    突触计数 = 0;
                }
                //force internal firing
                if (突触计数 > 0)
                {
                    nextFiring--;
                    if (nextFiring <= 0) //Firing Rate
                    {
                        突触计数--;
                        currentCharge = currentCharge + threshold;
                        if (突触计数 > 0)
                            nextFiring = (int)泄露率;
                    }
                    神经元列表base.添加神经元到激活列表组(id);
                }
                else if (突触计数 == 0) 突触计数--;
            }

            //code to implement a refractory period
            if (cycle < lastFired + 神经元列表base.GetRefractoryDelay())
            {
                currentCharge = 0;
                神经元列表base.添加神经元到激活列表组(id);
            }

            //check for firing
            if (模型 != 模型类型.FloatValue && currentCharge < 0) currentCharge = 0;
            if (currentCharge != lastCharge)
            {
                lastCharge = currentCharge;
                神经元列表base.添加神经元到激活列表组(id);
            }

            if (模型 == 模型类型.LIF && 突触计数 != 0)
            {
                突触计数 = 突触计数 >> 1;
                神经元列表base.添加神经元到激活列表组(id);
                if ((突触计数 & 0x001) != 0)
                {
                    return true;
                }
            }

            if (currentCharge >= threshold)
            {
                if (模型 == 模型类型.LIF && 突触延迟 != 0)
                {
                    突触计数 |= (1 << 突触延迟);
                    lastFired = cycle;
                    currentCharge = 0;
                    神经元列表base.添加神经元到激活列表组(id);
                    return false;
                }
                if (模型 == 模型类型.Burst && 突触计数 < 0)
                {
                    nextFiring = (int)泄露率;
                    if (nextFiring < 1) nextFiring = 1;
                    突触计数 = 突触延迟 - 1;
                }
                if (模型 == 模型类型.Always)
                {
                    nextFiring = 突触延迟;
                }
                if (模型 == 模型类型.Random)
                {
                    double newNormal = rand_normal((double)突触延迟, (double)泄露率);
                    if (newNormal < 1) newNormal = 1;
                    nextFiring = (int)newNormal;
                }
                if (模型 != 模型类型.FloatValue)
                    currentCharge = 0;
                lastFired = cycle;
                return true;
            }
            if (模型 == 模型类型.LIF)
            {
                currentCharge = currentCharge * (1 - 泄露率);
                神经元列表base.添加神经元到激活列表组(id);
            }
            return false;
        }
        public void Fire2()
        {
            if (模型 == 模型类型.FloatValue) return;
            if (模型 == 模型类型.Color && lastCharge != 0)
                return;
            else if (模型 != 模型类型.Color && lastCharge < threshold && (突触计数 & 0x1) == 0)
                return; //did the neuron fire?
            神经元列表base.添加神经元到激活列表组(id);
            if (突触数组 != null)
            {
                //while (vectorLock.exchange(1) == 1) { } //prevent the vector of synapses from changing while we're looking at it
                for (int i = 0; i < 突触数组.Count; i++) //process all the synapses sourced by this neuron
                {
                    突触base s = 突触数组[i];
                    神经元base nTarget = s.获取目标神经元();
                    //if (((long)nTarget >> 63) != 0) //does this synapse go to another server
                    //{
                    //    神经元列表base.remoteQueue.push(s);
                    //}

                    //else
                    //{   //nTarget->currentCharge += s.GetWeight(); //not supported until C++20
                    //    auto current = nTarget->currentCharge.load(std::memory_order_relaxed);
                    //    float desired = current + s.获取权重();
                    //    while (!nTarget->currentCharge.compare_exchange_weak(current, desired))
                    //    {
                    //        current = nTarget->currentCharge.load(std::memory_order_relaxed);
                    //        desired = current + s.获取权重();
                    //    }

                    //    神经元列表Base::添加神经元到激活列表组(nTarget->id);
                    //}
                }
                //vectorLock = 0;
            }
        }
        //const int ranges1 = 7;
        //unsafe fixed double cutoffs1[ranges1] = { 1, .5, .34, .25, .2, .15, 0 };
        //unsafe fixed double posIncr1[ranges1] = { 0, .1, .05, .025, .01, .012, .01 };
        //unsafe fixed double negIncr1[ranges1] = { -.01, -.1, -.017, -.00625, -.002, -.002, -.001 };

        ////play with this for experimentation
        //const int ranges2 = 7;
        //unsafe fixed double cutoffs2[ranges2] = { .5, .25, .1, 0, -.1, -.25, -1 };
        //unsafe fixed double posIncr2[ranges2] = { .2, .1, .05, .05, .05, .1, .5 };
        ////	double negIncr2[ranges2] = { -.5, -.1, -.05, -.05,  -.05, -.1,  -.2 };
        //unsafe fixed double negIncr2[ranges2] = { -.25, -.05, -.025, -.025, -.025, -.05, -.1 };
        ////	double negIncr2[ranges2] = { -.125, -.025, -.0125, -.0125,  -.0125, -.025,  -.05 };



        public void Fire3()
        {
            if (模型 == 模型类型.FloatValue) return;
            if (模型 == 模型类型.Color && lastCharge != 0)
                return;
            if (突触数组 != null)
            {
                //while (vectorLock.exchange(1) == 1) { } //prevent the vector of synapses from changing while we're looking at it
                for (int i = 0; i < 突触数组.Count; i++) //process all the synapses sourced by this neuron
                {
                    突触base s = 突触数组[i];
                    神经元base? nTarget = s.获取目标神经元();

                    if (s.获取模型() == 突触base.模型类型.Hebbian1)
                    {
                        //did the target neuron fire after this stimulation?
                        float weight = s.获取权重();
                        if (nTarget.currentCharge >= 1 && currentCharge >= 1)
                        {
                            //strengthen the synapse
                            weight = 新建赫布权重(weight, .1f, s.获取模型(), 1);
                        }
                        if (nTarget.currentCharge >= 1 && currentCharge < 1 ||
                            nTarget.currentCharge < 1 && currentCharge >= 1)
                        {
                            //weaken the synapse
                            weight = 新建赫布权重(weight, -.1f, s.获取模型(), 1);
                        }
                        突触数组[i].设置权重(weight);
                    }
                }
                //vectorLock = 0;
            }
            if (synapsesFrom != null && currentCharge >= threshold)
            {
                int numHebbian = 0;
                int numPosHebbian = 0;
                //while (vectorLock.exchange(1) == 1) { } //prevent the vector of synapses from changing while we're looking at it
                for (int i = 0; i < synapsesFrom.Count; i++) //process all the synapses sourced by this neuron
                {
                    突触base s = synapsesFrom[i];
                    if (s.获取模型() != 突触base.模型类型.Fixed)
                    {
                        numHebbian++;
                        if (s.获取权重() >= 0) numPosHebbian++;
                    }
                }
                for (int i = 0; i < synapsesFrom.Count; i++) //process all the synapses sourced by this neuron
                {
                    突触base s = synapsesFrom[i];
                    if (s.获取模型() == 突触base.模型类型.Hebbian2 || s.获取模型() == 突触base.模型类型.Binary)
                    {
                        神经元base nTarget = s.获取目标神经元();
                        //did this neuron fire coincident or just after the target (the source since these are FROM synapses)
                        float weight = s.获取权重();
                        int delay = 0;
                        if (s.获取模型() == 突触base.模型类型.Hebbian2) delay = 6;

                        if (s.获取模型() == 突触base.模型类型.Hebbian2 ||
                            s.获取模型() == 突触base.模型类型.Binary)
                        {
                            if (nTarget.lastFired >= lastFired - delay)
                            {
                                //strengthen the synapse
                                weight = 新建赫布权重(weight, .1f, s.获取模型(), numHebbian);
                            }
                            else
                            {
                                //weaken the synapse
                                weight = 新建赫布权重(weight, -.1f, s.获取模型(), numHebbian);
                            }
                            //update the synapse in "From"
                            synapsesFrom[i].设置权重(weight);
                            //update the synapse in "To"
                            for (int b = 0; b < nTarget.突触数组.Count; b++)
                            {
                                if (nTarget.突触数组[b].获取目标神经元() == this)
                                {
                                    //while (nTarget->vectorLock.exchange(1) == 1) { }
                                    nTarget.突触数组[b].设置权重(weight);
                                    //nTarget->vectorLock = 0;
                                }
                            }
                        }

                    }
                }
                //vectorLock = 0; 
            }
        }

        public float 新建赫布权重(float weight, float offset, 突触base.模型类型 model, int numberOfSynapses1)
        {
            float numberOfSynapses = numberOfSynapses1 / 2.0f;
            float y = weight * numberOfSynapses;
            if (model == 突触base.模型类型.Binary)
            {
                if (offset > 0) return 1.0f / (float)numberOfSynapses;
                return 0;
            }
            else if (model == 突触base.模型类型.Hebbian1)
            {
                int i = 0;
                y = weight;
                //for (i = 0; i < ranges1; i++)
                //{
                //    if (y >= cutoffs1[i])
                //    {
                //        if (offset > 0)
                //            y += (float)posIncr1[i];
                //        else
                //            y += (float)negIncr1[i];
                //        if (y < 0) y = 0;
                //        if (y > 1) y = 1;
                //        break;
                //    }
                //}
            }
            else if (model == 突触base.模型类型.Hebbian2)
            {

                float maxVal = 1.0f / numberOfSynapses;
                float curWeight = weight * numberOfSynapses;
                float x = 0;
                if (curWeight >= 1)
                {
                    curWeight = 1;
                }
                else if (curWeight <= -1)
                {
                    curWeight = -1;
                }
                //			else
                x = MathF.Atanh(curWeight);

                if (offset != 0)
                {
                    x += offset;
                    curWeight = MathF.Tanh(x);
                }
                else
                {
                    x *= 0.5f;
                    curWeight = MathF.Tanh(x);
                }
                y = curWeight / numberOfSynapses;
                if (y < -maxVal) y = -maxVal;
                if (y > maxVal) y = maxVal;
            }
            return y;
        }

        public 神经元base(神经元base t)
        {
            this.模型 = t.模型;
            id = t.id;
            this.泄露率 = t.泄露率;
        }
         //public static 神经元base operator = (神经元base t)
         //{
         //    return this;
         //}


}
}
