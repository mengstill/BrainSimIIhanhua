#include "pch.h"

#include "神经元引擎包装器.h"
#include "..\NeuronEngine\神经元列表Base.h"
#include <Windows.h>


using namespace System;
using namespace std;
using namespace System::Runtime::InteropServices;


namespace NeuronEngine
{
	namespace CLI
	{
		神经元列表Base::神经元列表Base()
		{
		}
		神经元列表Base::~神经元列表Base()
		{
			delete theNeuronArray;
		}
		void 神经元列表Base::Initialize(int neuronCount)
		{
			if (theNeuronArray != NULL)
				delete theNeuronArray;
			theNeuronArray = new NeuronEngine::神经元列表Base();
			theNeuronArray->Initialize(neuronCount);
		}
		void 神经元列表Base::Fire()
		{
			theNeuronArray->Fire();
		}
		int 神经元列表Base::获取数组大小()
		{
			return theNeuronArray->获取数组大小();
		}
		long long 神经元列表Base::获取次代()
		{
			return theNeuronArray->获取次代();
		}
		void 神经元列表Base::设置次代(long long i)
		{
			theNeuronArray->设置次代(i);
		}
		int 神经元列表Base::获取激活的神经元数量()
		{
			return theNeuronArray->获取激活数量();
		}
		void 神经元列表Base::设置线程数量(int theCount)
		{
			theNeuronArray->设置线程总数(theCount);
		}
		int 神经元列表Base::获取线程数()
		{
			return theNeuronArray->获取线程总数();
		}
		void 神经元列表Base::SetRefractoryDelay(int i)
		{
			theNeuronArray->SetRefractoryDelay(i);
		}
		int 神经元列表Base::GetRefractoryDelay()
		{
			return theNeuronArray->GetRefractoryDelay();
		}
		float 神经元列表Base::GetNeuronLastCharge(int i)
		{
			神经元Base* n = theNeuronArray->获取神经元(i);
			return n->GetLastCharge();
		}
		void 神经元列表Base::SetNeuronLastCharge(int i, float value)
		{
			神经元Base* n = theNeuronArray->获取神经元(i);
			n->SetLastCharge(value);
		}
		void 神经元列表Base::SetNeuronCurrentCharge(int i, float value)
		{
			神经元Base* n = theNeuronArray->获取神经元(i);
			n->SetCurrentCharge(value);
		}
		void 神经元列表Base::AddToNeuronCurrentCharge(int i, float value)
		{
			神经元Base* n = theNeuronArray->获取神经元(i);
			n->AddToCurrentValue(value);
		}
		float 神经元列表Base::GetNeuronLeakRate(int i)
		{
			神经元Base* n = theNeuronArray->获取神经元(i);
			return n->获取泄露率();
		}
		void 神经元列表Base::SetNeuronLeakRate(int i, float value)
		{
			神经元Base* n = theNeuronArray->获取神经元(i);
			n->设置泄露率(value);
		}
		int 神经元列表Base::GetNeuronAxonDelay(int i)
		{
			神经元Base* n = theNeuronArray->获取神经元(i);
			return n->GetAxonDelay();
		}
		void 神经元列表Base::SetNeuronAxonDelay(int i, int value)
		{
			神经元Base* n = theNeuronArray->获取神经元(i);
			n->SetAxonDelay(value);
		}
		long long 神经元列表Base::GetNeuronLastFired(int i)
		{
			神经元Base* n = theNeuronArray->获取神经元(i);
			return n->GetLastFired();
		}
		int 神经元列表Base::获取神经元模型(int i)
		{
			神经元Base* n = theNeuronArray->获取神经元(i);
			return (int)n->获取模型();
		}
		void 神经元列表Base::设置神经元模型(int i, int model)
		{
			神经元Base* n = theNeuronArray->获取神经元(i);
			n->设置模型((神经元Base::modelType) model);
		}
		bool 神经元列表Base::获取神经元是否使用中(int i)
		{
			神经元Base* n = theNeuronArray->获取神经元(i);
			return n->GetInUse();
		}
		void 神经元列表Base::设置神经元标签(int i, String^ newLabel)
		{
			const wchar_t* chars = (const wchar_t*)(Marshal::StringToHGlobalAuto(newLabel)).ToPointer();
			theNeuronArray->获取神经元(i)->设置标签(chars);
			Marshal::FreeHGlobal(IntPtr((void*)chars));
		}
		String^ 神经元列表Base::获取神经元标签(int i)
		{
			wchar_t* labelChars = theNeuronArray->获取神经元(i)->获取标签();
			if (labelChars != NULL)
			{
				std::wstring label(labelChars);
				String^ str = gcnew String(label.c_str());
				return str;
			}
			else
			{
				String^ str = gcnew String("");
				return str;
			}
		}
		String^ 神经元列表Base::GetRemoteFiring()
		{
			std::string remoteFiring = theNeuronArray->GetRemoteFiringString();
			String^ str = gcnew String(remoteFiring.c_str());
			return str;
		}
		cli::array<byte>^ 神经元列表Base::GetRemoteFiringSynapses()
		{
			std::vector<突触Base> tempVec;
			突触Base s1 = theNeuronArray->GetRemoteFiringSynapse();
			while (s1.获取目标神经元() != NULL)
			{
				tempVec.push_back(s1);
				s1 = theNeuronArray->GetRemoteFiringSynapse();
			}
			return ReturnArray(tempVec);
		}

		void 神经元列表Base::添加突触(int src, int dest, float weight, int model, bool noBackPtr)
		{
			if (src < 0)return;
			神经元Base* n = theNeuronArray->获取神经元(src);
			if (dest < 0)
				n->添加突触((神经元Base*)(long long)dest, weight, (突触Base::modelType) model, noBackPtr);
			else
				n->添加突触(theNeuronArray->获取神经元(dest), weight, (突触Base::modelType)model, noBackPtr);
		}
		void 神经元列表Base::添加输入突触(int src, int dest, float weight, int model)
		{
			if (dest < 0)return;
			神经元Base* n = theNeuronArray->获取神经元(dest);
			if (src < 0)
				n->AddSynapseFrom((神经元Base*)(long long)src, weight, (突触Base::modelType)model);
			else
				n->AddSynapseFrom(theNeuronArray->获取神经元(src), weight, (突触Base::modelType)model);
		}
		void 神经元列表Base::删除突触(int src, int dest)
		{
			if (src < 0) return;
			神经元Base* n = theNeuronArray->获取神经元(src);
			if (dest < 0)
				n->删除突触((神经元Base*)(long long)dest);
			else
				n->删除突触(theNeuronArray->获取神经元(dest));
		}
		void 神经元列表Base::删除输入突触(int src, int dest)
		{
			if (dest < 0)return;
			神经元Base* n = theNeuronArray->获取神经元(dest);
			if (src < 0)
				n->删除突触((神经元Base*)(long long)src);
			else
				n->删除突触(theNeuronArray->获取神经元(src));
		}


		cli::array<byte>^ 神经元列表Base::获取突触数组(int src)
		{
			神经元Base* n = theNeuronArray->获取神经元(src);
			n->GetLock();
			std::vector<突触Base> tempVec = n->获取突触数组();
			n->ClearLock();
			return ReturnArray(tempVec);

		}
		cli::array<byte>^ 神经元列表Base::GetSynapsesFrom(int src)
		{
			神经元Base* n = theNeuronArray->获取神经元(src);
			n->GetLock();
			std::vector<突触Base> tempVec = n->GetSynapsesFrom();
			n->ClearLock();
			return ReturnArray(tempVec);
		}

		cli::array<byte>^ 神经元列表Base::ReturnArray(std::vector<突触Base> tempVec)
		{
			if (tempVec.size() == 0)
			{
				return gcnew cli::array<byte>(0);
			}
			byte* firstElem = (byte*)&tempVec.at(0);
			const size_t SIZE = tempVec.size(); //#of synapses
			const int byteCount = (int)(SIZE * sizeof(突触));
			cli::array<byte>^ tempArr = gcnew cli::array<byte>(byteCount);
			int k = 0;
			for (int j = 0; j < tempVec.size(); j++)
			{
				突触 s;
				s.model = (int)tempVec.at(j).获取模型();
				s.weight = tempVec.at(j).获取权重();
				//if the top bit of the target is not set, it's a raw pointer
				//if it is set, this is the negative of a global neuron ID
				神经元Base* target = tempVec.at(j).获取目标神经元();
				if (((long long)target >> 63) != 0 || target == NULL)
					s.target = (int)(long long)(tempVec.at(j).获取目标神经元());
				else
					s.target = tempVec.at(j).获取目标神经元()->获取ID();
				byte* firstElem = (byte*)&s;
				for (int i = 0; i < sizeof(突触); i++)
				{
					tempArr[k++] = *(firstElem + i);
				}
			}
			return tempArr;
		}

		struct Neuron {
			int id;  bool inUse; float lastCharge; float currentCharge;
			float leakRate; int axonDelay; 神经元Base::modelType model; long long lastFired;
		};
		cli::array<byte>^ 神经元列表Base::获取神经元(int src)
		{
			神经元Base* n = theNeuronArray->获取神经元(src);
			const int byteCount = sizeof(Neuron);
			cli::array<byte>^ tempArr = gcnew cli::array<byte>(byteCount);
			Neuron n1;
			memset(&n1, 0, byteCount); //clear out the space between struct elements
			n1.id = n->获取ID();
			n1.inUse = n->GetInUse();
			n1.lastCharge = n->GetLastCharge();
			n1.currentCharge = n->GetCurrentCharge();
			n1.leakRate = n->获取泄露率();
			n1.lastFired = n->GetLastFired();
			n1.model = n->获取模型();
			n1.axonDelay = n->GetAxonDelay();
			byte* firstElem = (byte*)&n1;
			for (int i = 0; i < sizeof(Neuron); i++)
			{
				tempArr[i] = *(firstElem + i);
			}
			return tempArr;
		}
		long long 神经元列表Base::获取总突触数()
		{
			return theNeuronArray->GetTotalSynapseCount();
		}
		long 神经元列表Base::获取使用中的神经元总数()
		{
			return theNeuronArray->获取使用中神经元数量();
		}
	}
}