#define CompilingNeuronWrapper
#pragma once

#include <Windows.h> 
#include <vector>
#include <string>
#include <tuple>
#include <array>


using namespace std;

namespace NeuronEngine
{
	class 神经元列表Base;
	class 神经元Base;
	class 突触Base;

	typedef unsigned char byte;

	namespace CLI
	{
		struct 突触 { int target; float weight; int model; };

		public ref class 神经元列表Base
		{
		public:
			神经元列表Base();
			~神经元列表Base();
			void Initialize(int numberOfNeurons);
			//脉冲，突触运行
			void Fire();
			int 获取数组大小();
			int 获取线程数();
			void 设置线程数量(int i);
			int GetRefractoryDelay();
			void SetRefractoryDelay(int i);
			long long 获取次代();
			void 设置次代(long long i);
			int 获取激活的神经元数量();
			long long 获取总突触数();
			long 获取使用中的神经元总数();

			cli::array<byte>^ 获取神经元(int src);
			float GetNeuronLastCharge(int i);
			void SetNeuronLastCharge(int i, float value);
			void SetNeuronCurrentCharge(int i, float value);
			void AddToNeuronCurrentCharge(int i, float value);
			bool 获取神经元是否使用中(int i);
			System::String^ 获取神经元标签(int i);
			
			System::String^ GetRemoteFiring();
			cli::array<byte>^ GetRemoteFiringSynapses();

			void 设置神经元标签(int i, System::String^ newLabel);
			int 获取神经元模型(int i);
			void 设置神经元模型(int i, int model);
			float GetNeuronLeakRate(int i);
			void SetNeuronLeakRate(int i, float value);
			int GetNeuronAxonDelay(int i);
			void SetNeuronAxonDelay(int i, int value);
			long long GetNeuronLastFired(int i);
			cli::array<byte>^ 获取突触数组(int src);
			cli::array<byte>^ GetSynapsesFrom(int src);

			void 添加突触(int src, int dest, float weight, int model, bool noBackPtr);
			void 添加输入突触(int src, int dest, float weight, int model);
			void 删除突触(int src, int dest);
			void 删除输入突触(int src, int dest);

		private:
			// Pointer to our implementation
			NeuronEngine::神经元列表Base* theNeuronArray = NULL;
			cli::array<byte>^ ReturnArray(std::vector<突触Base> synapses);
		};
	}
}
