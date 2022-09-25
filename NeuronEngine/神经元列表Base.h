#pragma once


#include "神经元Base.h"
#include "突触Base.h"
#include <vector>
#include <atomic>


#ifndef CompilingNeuronWrapper
#include <concurrent_queue.h>
#endif
#include <string>
#define NeuronWrapper _

namespace NeuronEngine
{
	class 神经元列表Base
	{
	public:
		__declspec(dllexport) 神经元列表Base();
		__declspec(dllexport) ~神经元列表Base();
		__declspec(dllexport) void Initialize(int theSize, 神经元Base::modelType t = 神经元Base::modelType::Std);
		__declspec(dllexport) 神经元Base* 获取神经元(int i);
		__declspec(dllexport) int 获取数组大小();
		__declspec(dllexport) long long GetTotalSynapseCount();
		__declspec(dllexport) long 获取使用中神经元数量();
		__declspec(dllexport) void Fire();
		__declspec(dllexport) long long 获取次代();
		__declspec(dllexport) void 设置次代(long long i);
		__declspec(dllexport) int 获取激活数量();
		__declspec(dllexport) int 获取线程总数();
		__declspec(dllexport) void 设置线程总数(int i);
		__declspec(dllexport) void GetBounds(int taskID, int& start, int& end);
		__declspec(dllexport) std::string GetRemoteFiringString();
		__declspec(dllexport) 突触Base GetRemoteFiringSynapse();
		__declspec(dllexport) static int GetRefractoryDelay();
		__declspec(dllexport) static void SetRefractoryDelay(int i);


	private:
		int 数组大小 = 0;
		int 线程总数 = 124;
		std::vector<神经元Base> 神经元数组;
		std::atomic<long> 激活数量 = 0;
		long long 循环数 = 0;
		static int refractoryDelay;

		static std::vector<unsigned long long> 激活列表1;
		static std::vector<unsigned long long> 激活列表2;

	private:
		__declspec(noinline) void 神经元进程1(int taskID); //这些都是未联机的，因此分析器更有意义
		__declspec(noinline) void 神经元进程2(int taskID);
		__declspec(noinline) void 神经元进程3(int taskID);
		void GetBounds64(int taskID, int& start, int& end);

	public:
		static void 添加神经元到激活列表组(int id);
		static bool 是否需要清除激活列表组;
		static void 清除激活列表组();

	public:
#ifndef CompilingNeuronWrapper
		static concurrency::concurrent_queue<突触Base> remoteQueue;
		static concurrency::concurrent_queue<神经元Base *> fire2Queue;
#endif // !NeuronWrapper
	};
}