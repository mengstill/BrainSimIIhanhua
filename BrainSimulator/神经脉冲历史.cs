using System;
using System.Collections.Generic;

namespace BrainSimulator
{
    class 神经冲动历史
    {
        const int maxSamples = 1000;
        public class 记录
        {
            /// <summary>
            /// 代数
            /// </summary>
            public long generation = 0;
            public float value = 0;
        }
        public class 神经元历史记录
        {
            public int 神经元ID;
            public List<记录> 记录列表 = new();
        }
        public static List<神经元历史记录> 历史列表 = new ();

        public static long 最早的值()
        {
            long retVal = long.MaxValue;
            for (int i = 0; i < 历史列表.Count; i++)
            {
                if (历史列表[i].记录列表.Count > 0)
                    retVal = Math.Min(retVal, 历史列表[i].记录列表[0].generation);
            }
            return retVal;
        }

        public static bool 神经元是否在神经脉冲历史中(int id)
        {
            for (int i = 0; i < 历史列表.Count; i++)
            {
                if (历史列表[i].神经元ID == id)
                {
                    return true;
                }
            }
            return false;
        }
        public static void 添加神经元进历史窗口(int id)
        {
            if (神经元是否在神经脉冲历史中(id)) return;
            神经元历史记录 entry = new();
            entry.神经元ID = id;
            历史列表.Add(entry);
        }
        public static void 从历史窗口移除神经元(int id)
        {
            for (int i = 0; i < 历史列表.Count; i++)
            {
                if (历史列表[i].神经元ID == id)
                {
                    历史列表.RemoveAt(i);
                    return;
                }
            }
        }

        public static void 更新神经脉冲历史()
        {
            if (MainWindow.此神经元数组 == null) return;
            for (int i = 0; i < 历史列表.Count; i++)
            {
                神经元历史记录 active = 历史列表[i];
                float lastCharge = MainWindow.此神经元数组.获取神经元(active.神经元ID).最后更改;
                if (active.记录列表.Count > maxSamples)
                    active.记录列表.RemoveAt(0);
                active.记录列表.Add(new 记录 { generation = MainWindow.此神经元数组.Generation, value = lastCharge });
            }
        }

        public static void 移除()
        {
            for (int i = 0; i < 历史列表.Count; i++)
            {
                历史列表[i].记录列表.Clear();
            }
        }
        public static void 移除所有()
        {
            移除();
            历史列表.Clear();
        }
    }
}
