#pragma once

#include <string>
#include <vector>
#include <atomic>
#include "突触Base.h"


namespace NeuronEngine { class 突触Base; }
namespace NeuronEngine { class 神经元列表Base; }

namespace NeuronEngine
{
	class 神经元Base
	{
	public:
		enum class modelType {Std,Color,FloatValue,LIF,Random,Burst,Always};

		//神经元的结束值
		float lastCharge = 0;

		//空向量占用内存，因此这是指向向量的指针，仅在需要时分配
		std::vector<突触Base>* synapses = NULL;

	private:
		//神经元的累积值
		std::atomic<float> currentCharge = 0;

		modelType model = modelType::Std;
		
		float leakRate = 0.1f; //仅用于LIF模型
		int nextFiring = 0; //仅用于随机模型和连续模型
		long long lastFired = 0; //上次触发的时间戳
		int id = -1; //一个非法值，它将捕获
		wchar_t* label = NULL;
		int axonDelay = 0;
		int axonCounter = 0;
		
		std::vector<突触Base>* synapsesFrom = NULL;

		//这是一个滚动您自己的互斥体，因为互斥体在CLI代码中不存在，并导致编译失败
		//this is a roll-your-own mutex because mutex doesn't exist in CLI code and causes compile fails
		std::atomic<int> vectorLock = 0;
		//std::mutex aLock;
		

	private:
		const float  threshold = 1.0f;


	public:
		__declspec(dllexport)  神经元Base(int ID);
		__declspec(dllexport)  ~神经元Base();

		__declspec(dllexport)  int GetId();
		__declspec(dllexport)  modelType GetModel();
		__declspec(dllexport)  void SetModel(modelType value);
		__declspec(dllexport)  float GetLastCharge();
		__declspec(dllexport)  void SetLastCharge(float value);
		__declspec(dllexport)  float GetCurrentCharge();
		__declspec(dllexport)  void SetCurrentCharge(float value);

		__declspec(dllexport)  void AddSynapse(神经元Base* n, float weight, 突触Base::modelType model = 突触Base::modelType::Fixed, bool noBackPtr = true);
		__declspec(dllexport)  void AddSynapseFrom(神经元Base* n, float weight, 突触Base::modelType model = 突触Base::modelType::Fixed);
		__declspec(dllexport)  void DeleteSynapse(神经元Base* n);
		__declspec(dllexport)  void GetLock();
		__declspec(dllexport)  void ClearLock();
		__declspec(dllexport)  std::vector<突触Base> GetSynapses();
		__declspec(dllexport)  std::vector<突触Base> GetSynapsesFrom();
		__declspec(dllexport)  int GetSynapseCount();

		__declspec(dllexport)  bool GetInUse();
		__declspec(dllexport)  wchar_t* GetLabel();
		__declspec(dllexport)  void SetLabel(const wchar_t*);


		__declspec(dllexport)  float GetLeakRate();
		__declspec(dllexport)  void SetLeakRate(float value);
		__declspec(dllexport)  int GetAxonDelay();
		__declspec(dllexport)  void SetAxonDelay(int value);
		__declspec(dllexport)  long long GetLastFired();

		__declspec(dllexport)  void AddToCurrentValue(float weight);

		__declspec(dllexport)  bool Fire1(long long generation);
		void Fire2();
		void Fire3();

		float NewHebbianWeight(float y, float offset, 突触Base::modelType model, int numberOfSynapses);

		神经元Base(const 神经元Base& t)
		{
			model = t.model;
			id = t.id;
			leakRate = t.leakRate;
		}
		神经元Base& operator = (const 神经元Base& t)
		{
			return *this;
		}

	};
}

