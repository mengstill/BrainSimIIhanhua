#pragma once

#include <string>
#include <vector>
#include <atomic>
#include "SynapseBase.h"


namespace NeuronEngine { class SynapseBase; }
namespace NeuronEngine { class NeuronArrayBase; }

namespace NeuronEngine
{
	class NeuronBase
	{
	public:
		enum class modelType {Std,Color,FloatValue,LIF,Random,Burst,Always};

		//神经元的结束值
		float lastCharge = 0;

		//空向量占用内存，因此这是指向向量的指针，仅在需要时分配
		std::vector<SynapseBase>* synapses = NULL;

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
		
		std::vector<SynapseBase>* synapsesFrom = NULL;

		//这是一个滚动您自己的互斥体，因为互斥体在CLI代码中不存在，并导致编译失败
		//this is a roll-your-own mutex because mutex doesn't exist in CLI code and causes compile fails
		std::atomic<int> vectorLock = 0;
		//std::mutex aLock;
		

	private:
		const float  threshold = 1.0f;


	public:
		__declspec(dllexport)  NeuronBase(int ID);
		__declspec(dllexport)  ~NeuronBase();

		__declspec(dllexport)  int GetId();
		__declspec(dllexport)  modelType GetModel();
		__declspec(dllexport)  void SetModel(modelType value);
		__declspec(dllexport)  float GetLastCharge();
		__declspec(dllexport)  void SetLastCharge(float value);
		__declspec(dllexport)  float GetCurrentCharge();
		__declspec(dllexport)  void SetCurrentCharge(float value);

		__declspec(dllexport)  void AddSynapse(NeuronBase* n, float weight, SynapseBase::modelType model = SynapseBase::modelType::Fixed, bool noBackPtr = true);
		__declspec(dllexport)  void AddSynapseFrom(NeuronBase* n, float weight, SynapseBase::modelType model = SynapseBase::modelType::Fixed);
		__declspec(dllexport)  void DeleteSynapse(NeuronBase* n);
		__declspec(dllexport)  void GetLock();
		__declspec(dllexport)  void ClearLock();
		__declspec(dllexport)  std::vector<SynapseBase> GetSynapses();
		__declspec(dllexport)  std::vector<SynapseBase> GetSynapsesFrom();
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

		float NewHebbianWeight(float y, float offset, SynapseBase::modelType model, int numberOfSynapses);

		NeuronBase(const NeuronBase& t)
		{
			model = t.model;
			id = t.id;
			leakRate = t.leakRate;
		}
		NeuronBase& operator = (const NeuronBase& t)
		{
			return *this;
		}

	};
}

