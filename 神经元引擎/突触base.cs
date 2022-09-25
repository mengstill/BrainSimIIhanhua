using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace 神经元引擎
{
    public class 突触base
    {
        public enum 模型类型
        {
            Fixed, Binary, Hebbian1, Hebbian2, Hebbian3
        }

        神经元base? 目标神经元;
        float 权重 = 0;
        模型类型 模型 = 模型类型.Fixed;
        
        public void 设置目标神经元(神经元base 神经元)
        {
            this.目标神经元 = 神经元;
        }

        public 神经元base? 获取目标神经元()
        {
            return 目标神经元;
        }
        public void 设置权重(float 权重)
        {
            this.权重 = 权重;
        }
        public float 获取权重()
        {
            return 权重;
        }

        public void 设置模型(模型类型 模型)
        {
            this.模型 = 模型;
        }

        public 模型类型 获取模型()
        {
            return 模型;
        }
    }
}
