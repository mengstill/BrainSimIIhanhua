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
			void Initialize(int 神经元数量);
			//脉冲，突触运行
			void Fire();
			int 获取数组大小();
			int 获取线程数();
			void 设置线程数量(int i);
			int GetRefractoryDelay获取耐火材料延迟();
			void SetRefractoryDelay(int i);
			long long 获取次代();
			void 设置次代(long long i);
			int 获取激活的神经元数量();
			long long 获取总突触数();
			long 获取使用中的神经元总数();

			cli::array<byte>^ 获取神经元(int src);
			float GetNeuronLastCharge获取神经元上一次的脉冲(int i);
			void SetNeuronLastCharge设置神经元上一次的脉冲(int i, float value);
			void SetNeuronCurrentCharge设置神经元当前的脉冲(int i, float value);
			void AddToNeuronCurrentCharge加入神经元当前的脉冲(int i, float value);
			bool 获取神经元是否使用中(int i);
			System::String^ 获取神经元标签(int i);
			
			System::String^ GetRemoteFiring获取远程激活();
			cli::array<byte>^ GetRemoteFiringSynapses获取远程激活突触();

			void 设置神经元标签(int i, System::String^ 新标签);
			int 获取神经元模型(int i);
			void 设置神经元模型(int i, int model);
			float GetNeuronLeakRate获取神经元释放率(int i);
			void SetNeuronLeakRate设置神经元释放率(int i, float value);
			int GetNeuronAxonDelay(int i);
			void SetNeuronAxonDelay(int i, int value);
			long long GetNeuronLastFired(int i);
			cli::array<byte>^ 获取突触数组(int src);
			cli::array<byte>^ GetSynapsesFrom获取突触源(int src);

			void 添加突触(int src, int dest, float weight, int model, bool noBackPtr);
			void 添加输入突触(int src, int dest, float weight, int model);
			void 删除突触(int src, int dest);
			void 删除输入突触(int src, int dest);

		private:
			// Pointer to our implementation
			NeuronEngine::神经元列表Base* theNeuronArray该神经元列表 = NULL;
			cli::array<byte>^ ReturnArray(std::vector<突触Base> synapses);
		};
	}
}
