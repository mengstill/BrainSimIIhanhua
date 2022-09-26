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
	class ��Ԫ�б�Base;
	class ��ԪBase;
	class ͻ��Base;

	typedef unsigned char byte;

	namespace CLI
	{
		struct ͻ�� { int target; float weight; int model; };

		public ref class ��Ԫ�б�Base
		{
		public:
			��Ԫ�б�Base();
			~��Ԫ�б�Base();
			void Initialize(int ��Ԫ����);
			//���壬ͻ������
			void Fire();
			int ��ȡ�����С();
			int ��ȡ�߳���();
			void �����߳�����(int i);
			int GetRefractoryDelay��ȡ�ͻ�����ӳ�();
			void SetRefractoryDelay(int i);
			long long ��ȡ�δ�();
			void ���ôδ�(long long i);
			int ��ȡ�������Ԫ����();
			long long ��ȡ��ͻ����();
			long ��ȡʹ���е���Ԫ����();

			cli::array<byte>^ ��ȡ��Ԫ(int src);
			float GetNeuronLastCharge��ȡ��Ԫ��һ�ε�����(int i);
			void SetNeuronLastCharge������Ԫ��һ�ε�����(int i, float value);
			void SetNeuronCurrentCharge������Ԫ��ǰ������(int i, float value);
			void AddToNeuronCurrentCharge������Ԫ��ǰ������(int i, float value);
			bool ��ȡ��Ԫ�Ƿ�ʹ����(int i);
			System::String^ ��ȡ��Ԫ��ǩ(int i);
			
			System::String^ GetRemoteFiring��ȡԶ�̼���();
			cli::array<byte>^ GetRemoteFiringSynapses��ȡԶ�̼���ͻ��();

			void ������Ԫ��ǩ(int i, System::String^ �±�ǩ);
			int ��ȡ��Ԫģ��(int i);
			void ������Ԫģ��(int i, int model);
			float GetNeuronLeakRate��ȡ��Ԫ�ͷ���(int i);
			void SetNeuronLeakRate������Ԫ�ͷ���(int i, float value);
			int GetNeuronAxonDelay(int i);
			void SetNeuronAxonDelay(int i, int value);
			long long GetNeuronLastFired(int i);
			cli::array<byte>^ ��ȡͻ������(int src);
			cli::array<byte>^ GetSynapsesFrom��ȡͻ��Դ(int src);

			void ���ͻ��(int src, int dest, float weight, int model, bool noBackPtr);
			void �������ͻ��(int src, int dest, float weight, int model);
			void ɾ��ͻ��(int src, int dest);
			void ɾ������ͻ��(int src, int dest);

		private:
			// Pointer to our implementation
			NeuronEngine::��Ԫ�б�Base* theNeuronArray����Ԫ�б� = NULL;
			cli::array<byte>^ ReturnArray(std::vector<ͻ��Base> synapses);
		};
	}
}
