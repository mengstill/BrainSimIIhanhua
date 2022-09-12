#pragma once

namespace NeuronEngine { class 神经元Base; }

namespace NeuronEngine
{
	class  __declspec(dllexport) 突触Base
	{
	public:
		enum class modelType { Fixed, Binary, Hebbian1, Hebbian2,Hebbian3};

		void SetTarget(神经元Base * target);
		神经元Base* GetTarget();
		float GetWeight();
		void SetWeight(float value);
		void SetModel(modelType value);
		modelType GetModel();

	private:
		神经元Base* targetNeuron = 0; //指向目标神经元的指针
		float weight = 0; //突触权重
		modelType model = modelType::Fixed;
	};
}
