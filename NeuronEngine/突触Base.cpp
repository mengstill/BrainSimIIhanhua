#include "pch.h"


#include "突触Base.h"
#include "神经元Base.h"

namespace NeuronEngine
{
	神经元Base* 突触Base::GetTarget()
	{
		return targetNeuron;
	}
	void 突触Base::SetTarget(神经元Base* target)
	{
		targetNeuron = target;
	}
	float 突触Base::GetWeight()
	{
		return weight;
	}
	void 突触Base::SetWeight(float value)
	{
		weight = value;
	}

	突触Base::modelType 突触Base::GetModel()
	{
		return model;
	}
	void 突触Base::SetModel(突触Base::modelType value)
	{
		model = value;
	}
}
