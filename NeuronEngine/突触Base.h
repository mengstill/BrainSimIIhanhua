#pragma once

namespace NeuronEngine { class 神经元Base; }

namespace NeuronEngine
{
	class  __declspec(dllexport) 突触Base
	{
	public:
		enum class modelType { Fixed, Binary, Hebbian1, Hebbian2,Hebbian3};

		void 设置目标神经元(神经元Base * target);
		神经元Base* 获取目标神经元();
		float 获取权重();
		void 设置权重(float value);
		void 设置模型(modelType value);
		modelType 获取模型();

	private:
		神经元Base* targetNeuron = 0; //指向目标神经元的指针
		float weight = 0; //突触权重
		modelType model = modelType::Fixed;
	};
}
