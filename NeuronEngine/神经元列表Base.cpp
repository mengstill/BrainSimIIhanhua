#include "pch.h"

#include "神经元列表Base.h"
#include <windows.h>
#include <ppl.h>
#include <iostream>
#include <random>


using namespace concurrency;
using namespace std;

namespace NeuronEngine
{
	Concurrency::concurrent_queue<突触Base> 神经元列表Base::remoteQueue;
	Concurrency::concurrent_queue<神经元Base*> 神经元列表Base::fire2Queue;
	std::vector<unsigned long long> 神经元列表Base::激活列表1;
	std::vector<unsigned long long> 神经元列表Base::激活列表2;

	bool 神经元列表Base::是否需要清除激活列表组;
	int 神经元列表Base::refractoryDelay = 0;

	std::string 神经元列表Base::GetRemoteFiringString()
	{
		std::string retVal("");
		突触Base s;
		int count = 0;
		while (remoteQueue.try_pop(s) && count++ < 90) //splits up long strings for transmission
		{
			retVal += std::to_string(-(long long)s.获取目标神经元()) + " ";
			retVal += std::to_string((float)s.获取权重()) + " ";
			retVal += std::to_string((int)s.获取模型()) + " ";
		}
		return retVal;
	}
	突触Base 神经元列表Base::GetRemoteFiringSynapse()
	{
		突触Base s;
		if (remoteQueue.try_pop(s))
		{
			return s;
		}
		s.设置目标神经元(NULL);
		return s;
	}

	神经元列表Base::神经元列表Base()
	{
	}
	神经元列表Base::~神经元列表Base()
	{
	}

	void 神经元列表Base::Initialize(int theSize, 神经元Base::modelType t)
	{
		数组大小 = theSize;
		int expandedSize = 数组大小;
		if (expandedSize % 64 != 0)
			expandedSize = ((expandedSize / 64) + 1) * 64;
		数组大小 = expandedSize;
		神经元数组.reserve(expandedSize);
		for (int i = 0; i < expandedSize; i++)
		{
			神经元Base n(i);
			//n.SetModel(NeuronBase::modelType::LIF);  /for testing
			神经元数组.push_back(n);
		}
		激活列表1.reserve(expandedSize / 64);
		激活列表2.reserve(expandedSize / 64);

		int fireListCount = expandedSize / 64;
		for (int i = 0; i < fireListCount; i++)
		{
			激活列表1.push_back(0xffffffffffffffff);
			激活列表2.push_back(0);

		}
	}
	long long 神经元列表Base::获取次代()
	{
		return 循环数;
	}
	void 神经元列表Base::设置次代(long long i)
	{
		循环数 = i;
	}
	神经元Base* 神经元列表Base::获取神经元(int i)
	{
		return &神经元数组[i];
	}


	int 神经元列表Base::获取数组大小()
	{
		return 数组大小;
	}

	long long 神经元列表Base::GetTotalSynapseCount()
	{
		std::atomic<long long> count = 0;
		parallel_for(0, 线程总数, [&](int value) {
			int start, end;
			GetBounds(value, start, end);
			for (int i = start; i < end; i++)
			{
				count += (long long)获取神经元(i)->获取突触数量();;
			}
			});
		return count;;
	}

	long 神经元列表Base::获取使用中神经元数量()
	{
		std::atomic<long> count = 0;
		parallel_for(0, 线程总数, [&](int value) {
			int start, end;
			GetBounds(value, start, end);
			for (int i = start; i < end; i++)
			{
				if (获取神经元(i)->GetInUse())
					count++;
			}
			});
		return count;;
	}

	void 神经元列表Base::
		GetBounds(int taskID, int& start, int& end)
	{
		int numberToProcess = 数组大小 / 线程总数;
		int remainder = 数组大小 % 线程总数;
		start = numberToProcess * taskID;
		end = start + numberToProcess;
		if (taskID < remainder)
		{
			start += taskID;
			end = start + numberToProcess + 1;
		}
		else
		{
			start += remainder;
			end += remainder;
		}
	}

	//this is just like getBounds except that start and end must be even multiples of 64
	//so there won't be collisions on the firelists
	void 神经元列表Base::GetBounds64(int 任务ID, int& 起始, int& 结束)
	{
		int 每线程数量 = 数组大小 / 线程总数;
		if (每线程数量 % 64 == 0)
		{
			int 余数 = 数组大小 % 线程总数;
			起始 = 每线程数量 * 任务ID;
			结束 = 起始 + 每线程数量;
			if (任务ID < 余数)
			{
				起始 += 任务ID;
				结束 = 起始 + 每线程数量 + 1;
			}
			else
			{
				起始 += 余数;
				结束 += 余数;
			}
		}
		else
		{
			每线程数量 = (每线程数量 / 64 + 1) * 64;
			int 可用线程总数 = 数组大小 / 每线程数量;
			if (任务ID > 可用线程总数)
			{
				起始 = 0;
				结束 = 0;
				return;
			}
			int remainder = 数组大小 % 每线程数量;
			起始 = 每线程数量 * 任务ID;
			结束 = 起始 + 每线程数量;
			if (任务ID == 可用线程总数)
			{
				结束 = 起始 + remainder;
			}
		}
	}
	void 神经元列表Base::Fire()
	{
		if (是否需要清除激活列表组)
			清除激活列表组();
		是否需要清除激活列表组 = false;
		循环数++;
		激活数量 = 0;

		parallel_for(0, 线程总数, [&](int value) {
			神经元进程1(value);
			});
		parallel_for(0, 线程总数, [&](int value) {
			神经元进程2(value);
			});
		
			神经元进程3(0);
			
	}
	int 神经元列表Base::获取激活数量()
	{
		return 激活数量;
	}
	int 神经元列表Base::获取线程总数()
	{
		return 线程总数;
	}
	void 神经元列表Base::设置线程总数(int i)
	{
		线程总数 = i;
	}
	int 神经元列表Base::GetRefractoryDelay()
	{
		return refractoryDelay;
	}
	void 神经元列表Base::SetRefractoryDelay(int i)
	{
		refractoryDelay = i;
	}
	void 神经元列表Base::添加神经元到激活列表组(int id)
	{
		int index = id / 64;
		int offset = id % 64;
		unsigned long long bitMask = 0x1;
		bitMask = bitMask << offset;
		激活列表1[index] |= bitMask;
	}
	void 神经元列表Base::清除激活列表组()
	{
		for (int i = 0; i < 激活列表1.size(); i++)
		{
			激活列表1[i] = 0xffffffffffffffff;
			激活列表2[i] = 0;

		}
	}

	void 神经元列表Base::神经元进程1(int taskID)
	{
		int start, end;
		GetBounds64(taskID, start, end);
		start /= 64;
		end /= 64;
		for (int i = start; i < end; i++)
		{
			unsigned long long tempVal = 激活列表1[i];

			激活列表1[i] = 0;
			unsigned long long bitMask = 0x1;
			for (int j = 0; j < 64; j++)
			{
				if (tempVal & bitMask)
				{
					int neuronID = i * 64 + j;
					神经元Base* theNeuron = 获取神经元(neuronID);
					if (!theNeuron->Fire1(循环数))
					{
						tempVal &= ~bitMask; //clear the bit if not firing for 2nd phase
					}
					else
						激活数量++;
				}
				bitMask = bitMask << 1;
			}
			激活列表2[i] = tempVal;
		}
	}
	void 神经元列表Base::神经元进程2(int taskID)
	{
		int start, end;
		GetBounds64(taskID, start, end);
		start /= 64;
		end /= 64;
		for (int i = start; i < end; i++)
		{
			unsigned long long tempVal = 激活列表2[i];
			unsigned long long bitMask = 0x1;
			for (int j = 0; j < 64; j++)
			{
				if (tempVal & bitMask)
				{
					神经元Base* theNeuron = 获取神经元(i * 64 + j);
					theNeuron->Fire2();
				}
				bitMask = bitMask << 1;
			}

		}
	}

	void 神经元列表Base::神经元进程3(int taskID)
	{
		for (int i = 0; i < 数组大小; i++)
		{
			神经元Base* theNeuron = 获取神经元(i);
			theNeuron->Fire3();
		}
	}
}
