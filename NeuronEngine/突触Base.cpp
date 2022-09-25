#include "pch.h"


#include "突触Base.h"
#include "神经元Base.h"

namespace NeuronEngine
{
	神经元Base* 突触Base::获取目标神经元()
	{
		return targetNeuron;
	}
	void 突触Base::设置目标神经元(神经元Base* target)
	{
		targetNeuron = target;
	}
	float 突触Base::获取权重()
	{
		return weight;
	}
	void 突触Base::设置权重(float value)
	{
		weight = value;
	}

	突触Base::modelType 突触Base::获取模型()
	{
		return model;
	}
	void 突触Base::设置模型(突触Base::modelType value)
	{
		model = value;
	}
}
