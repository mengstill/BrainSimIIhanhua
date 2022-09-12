#include "pch.h"

#include "神经元Base.h"
#include "突触Base.h"
#include "神经元列表Base.h"
#include <cmath>

namespace NeuronEngine
{
	神经元Base::神经元Base(int ID)
	{
		leakRate = 0.1f;
		nextFiring = 0;
		id = ID;
	}

	神经元Base::~神经元Base()
	{
		delete synapses;
		delete synapsesFrom;
		delete label;
	}

	int 神经元Base::GetId()
	{
		return id;
	}

	神经元Base::modelType 神经元Base::GetModel()
	{
		return model;
	}

	void 神经元Base::SetModel(modelType value)
	{
		model = value;
	}
	float 神经元Base::GetLastCharge()
	{
		return lastCharge;
	}
	void 神经元Base::SetLastCharge(float value)
	{
		神经元列表Base::是否需要清除激活列表组 = true;
		lastCharge = value;
	}
	float 神经元Base::GetCurrentCharge()
	{
		return currentCharge;
	}
	void 神经元Base::SetCurrentCharge(float value)
	{
		神经元列表Base::是否需要清除激活列表组 = true;
		currentCharge = value;
	}
	float 神经元Base::GetLeakRate()
	{
		return leakRate;
	}
	void 神经元Base::SetLeakRate(float value)
	{
		leakRate = value;
	}
	int 神经元Base::GetAxonDelay()
	{
		return axonDelay;
	}
	void 神经元Base::SetAxonDelay(int value)
	{
		axonDelay = value;
	}
	long long 神经元Base::GetLastFired()
	{
		return lastFired;
	}
	wchar_t* 神经元Base::GetLabel()
	{
		return label;
	}
	void 神经元Base::SetLabel(const wchar_t* newLabel)
	{
		delete label;
		label = NULL;
		size_t len = wcslen(newLabel);
		if (len > 0)
		{
			label = new wchar_t[len + 2];
			wcscpy_s(label, len + 2, newLabel);
		}
	}
	bool 神经元Base::GetInUse()
	{
		bool retVal = (label != NULL) || (synapses != NULL && synapses->size() != 0) || (synapsesFrom != NULL && synapsesFrom->size() != 0) || (model != modelType::Std);

		return retVal;
	}

	void 神经元Base::AddSynapseFrom(神经元Base* n, float weight, 突触Base::modelType model)
	{
		while (vectorLock.exchange(1) == 1) {}

		突触Base s1;
		s1.SetWeight(weight);
		s1.SetTarget(n);
		s1.SetModel(model);

		if (synapsesFrom == NULL)
		{
			synapsesFrom = new std::vector<突触Base>();
			synapsesFrom->reserve(10);
		}
		for (int i = 0; i < synapsesFrom->size(); i++)
		{
			if (synapsesFrom->at(i).GetTarget() == n)
			{
				//update an existing synapse
				synapsesFrom->at(i).SetWeight(weight);
				synapsesFrom->at(i).SetModel(model);
				goto alreadyInList;
			}
		}
		//else create a new synapse
		synapsesFrom->push_back(s1);
	alreadyInList:
		vectorLock = 0;
	}

	void 神经元Base::AddSynapse(神经元Base* n, float weight, 突触Base::modelType model, bool noBackPtr)
	{
		while (vectorLock.exchange(1) == 1) {}

		突触Base s1;
		s1.SetWeight(weight);
		s1.SetTarget(n);
		s1.SetModel(model);

		if (synapses == NULL)
		{
			synapses = new std::vector<突触Base>();
			synapses->reserve(100);
		}
		for (int i = 0; i < synapses->size(); i++)
		{
			if (synapses->at(i).GetTarget() == n)
			{
				//update an existing synapse
				synapses->at(i).SetWeight(weight);
				synapses->at(i).SetModel(model);
				goto alreadyInList;
			}
		}
		//else create a new synapse
		synapses->push_back(s1);
	alreadyInList:
		vectorLock = 0;

		if (noBackPtr) return;

		//now add the synapsesFrom entry to the target neuron
		//this requires locking because multiply neurons may link to a single neuron simultaneously requiring backpointers.
		//The previous does not lock because you don't write to the same neuron from multiple threads

		while (n->vectorLock.exchange(1) == 1) {}
		突触Base s2;
		s2.SetTarget(this);
		s2.SetWeight(weight);
		s2.SetModel(model);

		if (n->synapsesFrom == NULL)
		{
			n->synapsesFrom = new std::vector<突触Base>();
			n->synapsesFrom->reserve(10);
		}
		for (int i = 0; i < n->synapsesFrom->size(); i++)
		{
			突触Base s = n->synapsesFrom->at(i);
			if (n->synapsesFrom->at(i).GetTarget() == this)
			{
				n->synapsesFrom->at(i).SetWeight(weight);
				n->synapsesFrom->at(i).SetModel(model);
				goto alreadyInList2;
			}
		}
		n->synapsesFrom->push_back(s2);
	alreadyInList2:
		n->vectorLock = 0;
		return;
	}
	void 神经元Base::DeleteSynapse(神经元Base* n)
	{
		while (vectorLock.exchange(1) == 1) {}
		if (synapses != NULL)
		{
			for (int i = 0; i < synapses->size(); i++)
			{
				if (synapses->at(i).GetTarget() == n)
				{
					synapses->erase(synapses->begin() + i);
					break;
				}
			}
			if (synapses->size() == 0)
			{
				delete synapses;
				synapses = NULL;
			}
		}
		vectorLock = 0;
		if (((long long)n >> 63) != 0) return;
		while (n->vectorLock.exchange(1) == 1) {}
		if (n->synapsesFrom != NULL)
		{
			for (int i = 0; i < n->synapsesFrom->size(); i++)
			{
				突触Base s = n->synapsesFrom->at(i);
				if (s.GetTarget() == this)
				{
					n->synapsesFrom->erase(n->synapsesFrom->begin() + i);
					if (n->synapsesFrom->size() == 0)
					{
						delete n->synapsesFrom;
						n->synapsesFrom = NULL;
					}
					break;
				}
			}
		}
		n->vectorLock = 0;
	}
	int 神经元Base::GetSynapseCount()
	{
		if (synapses == NULL) return 0;
		return (int)synapses->size();
	}
	std::vector<突触Base> 神经元Base::GetSynapses()
	{
		if (synapses == NULL)
		{
			std::vector<突触Base> tempVec = std::vector<突触Base>();
			return tempVec;
		}
		std::vector<突触Base> tempVec = std::vector<突触Base>(*synapses);
		return tempVec;
	}
	std::vector<突触Base> 神经元Base::GetSynapsesFrom()
	{
		if (synapsesFrom == NULL)
		{
			std::vector<突触Base> tempVec = std::vector<突触Base>();
			return tempVec;
		}
		std::vector<突触Base> tempVec = std::vector<突触Base>(*synapsesFrom);
		return tempVec;
	}
	void 神经元Base::GetLock()
	{
		while (vectorLock.exchange(1) == 1) {}
	}
	void 神经元Base::ClearLock()
	{
		vectorLock = 0;
	}

	void 神经元Base::AddToCurrentValue(float weight)
	{
		currentCharge = currentCharge + weight;
		if (currentCharge >= threshold)
			神经元列表Base::添加神经元到激活列表组(id);

	}

	//get a random number with a normal distribution around 
	double rand_normal(double mean, double stddev)
	{//Box muller method
		static double n2 = 0.0;
		static int n2_cached = 0;
		if (!n2_cached)
		{
			double x, y, r;
			do
			{
				x = 2.0 * rand() / RAND_MAX - 1;
				y = 2.0 * rand() / RAND_MAX - 1;

				r = x * x + y * y;
			} while (r == 0.0 || r > 1.0);
			{
				double d = sqrt(-2.0 * log(r) / r);
				double n1 = x * d;
				n2 = y * d;
				double result = n1 * stddev + mean;
				n2_cached = 1;
				return result;
			}
		}
		else
		{
			n2_cached = 0;
			return n2 * stddev + mean;
		}
	}

	//神经元放电是两相的，因此网络独立于神经元顺序
	//调用此函数时，调用方将神经元添加到fireList2。
	bool 神经元Base::Fire1(long long cycle)
	{
		if (signbit(leakRate))return false;
		if (model == modelType::Color)
		{
			神经元列表Base::添加神经元到激活列表组(id);
			return true;
		}
		//if (model == modelType::FloatValue) return false;
		if (model == modelType::Always)
		{
			nextFiring--;
			if (leakRate >= 0 && nextFiring <= 0) //泄漏率是标准偏差
			{
				currentCharge = currentCharge + threshold;
			}
			if (leakRate >= 0) //负泄漏率表示“禁用”
				神经元列表Base::添加神经元到激活列表组(id);
		}
		if (model == modelType::Random)
		{
			nextFiring--;
			if (leakRate >= 0 && nextFiring <= 0) //泄漏率是标准偏差
			{
				currentCharge = currentCharge + threshold;
			}
			if (leakRate >= 0) //负泄漏率表示“禁用”
				神经元列表Base::添加神经元到激活列表组(id);
		}
		if (model == modelType::Burst)
		{
			if (currentCharge < 0)
			{
				axonCounter = 0;
			}
			//force internal firing
			if (axonCounter > 0)
			{
				nextFiring--;
				if (nextFiring <= 0) //Firing Rate
				{
					axonCounter--;
					currentCharge = currentCharge + threshold;
					if (axonCounter > 0)
						nextFiring = (int)leakRate;
				}
				神经元列表Base::添加神经元到激活列表组(id);
			}
			else if (axonCounter == 0) axonCounter--;
		}

		//code to implement a refractory period
		if (cycle < lastFired + 神经元列表Base::GetRefractoryDelay())
		{
			currentCharge = 0;
			神经元列表Base::添加神经元到激活列表组(id);
		}

		//check for firing
		if (model != modelType::FloatValue && currentCharge < 0)currentCharge = 0;
		if (currentCharge != lastCharge)
		{
			lastCharge = currentCharge;
			神经元列表Base::添加神经元到激活列表组(id);
		}

		if (model == modelType::LIF && axonCounter != 0)
		{
			axonCounter = axonCounter >> 1;
			神经元列表Base::添加神经元到激活列表组(id);
			if ((axonCounter & 0x001) != 0)
			{
				return true;
			}
		}

		if (currentCharge >= threshold)
		{
			if (model == modelType::LIF && axonDelay != 0)
			{
				axonCounter |= (1 << axonDelay);
				lastFired = cycle;
				currentCharge = 0;
				神经元列表Base::添加神经元到激活列表组(id);
				return false;
			}
			if (model == modelType::Burst && axonCounter < 0)
			{
				nextFiring = (int)leakRate;
				if (nextFiring < 1) nextFiring = 1;
				axonCounter = axonDelay - 1;
			}
			if (model == modelType::Always)
			{
				nextFiring = axonDelay;
			}
			if (model == modelType::Random)
			{
				double newNormal = rand_normal((double)axonDelay, (double)leakRate);
				if (newNormal < 1) newNormal = 1;
				nextFiring = (int)newNormal;
			}
			if (model != modelType::FloatValue)
				currentCharge = 0;
			lastFired = cycle;
			return true;
		}
		if (model == modelType::LIF)
		{
			currentCharge = currentCharge * (1 - leakRate);
			神经元列表Base::添加神经元到激活列表组(id);
		}
		return false;
	}


	void 神经元Base::Fire2()
	{
		if (model == modelType::FloatValue) return;
		if (model == modelType::Color && lastCharge != 0)
			return;
		else if (model != modelType::Color && lastCharge < threshold && (axonCounter & 0x1) == 0)
			return; //did the neuron fire?
		神经元列表Base::添加神经元到激活列表组(id);
		if (synapses != NULL)
		{
			while (vectorLock.exchange(1) == 1) {} //prevent the vector of synapses from changing while we're looking at it
			for (int i = 0; i < synapses->size(); i++) //process all the synapses sourced by this neuron
			{
				突触Base s = synapses->at(i);
				神经元Base* nTarget = s.GetTarget();
				if (((long long)nTarget >> 63) != 0) //does this synapse go to another server
				{
					神经元列表Base::remoteQueue.push(s);
				}
				else
				{	//nTarget->currentCharge += s.GetWeight(); //not supported until C++20
					auto current = nTarget->currentCharge.load(std::memory_order_relaxed);
					float desired = current + s.GetWeight();
					while (!nTarget->currentCharge.compare_exchange_weak(current, desired))
					{
						current = nTarget->currentCharge.load(std::memory_order_relaxed);
						desired = current + s.GetWeight();
					}

					//if (desired >= threshold) //this conditional improves performance but 
					//introduces a potental bug where accumulated charge might be negative
					神经元列表Base::添加神经元到激活列表组(nTarget->id);
					/*
					if (s.GetModel() == SynapseBase::modelType::Hebbian1)
					{
						//did the target neuron fire after this stimulation?
						float weight = s.GetWeight();
						if (desired >= threshold)
						{
							//strengthen the synapse
							//weight = NewHebbianWeight(weight, .1f, s.GetModel(), 1);
						}
						else
						{
							//weaken the synapse
							weight = NewHebbianWeight(weight, -.1f, s.GetModel(), 1);
						}
						synapses->at(i).SetWeight(weight);
					}
				}
				vectorLock = 0;
			}
		}
		if (synapsesFrom != NULL)
		{
			int numHebbian = 0;
			int numPosHebbian = 0;
			while (vectorLock.exchange(1) == 1) {} //prevent the vector of synapses from changing while we're looking at it
			for (int i = 0; i < synapsesFrom->size(); i++) //process all the synapses sourced by this neuron
			{
				SynapseBase s = synapsesFrom->at(i);
				if (s.GetModel() != SynapseBase::modelType::Fixed)
				{
					numHebbian++;
					if (s.GetWeight() >= 0) numPosHebbian++;
				}
			}
			for (int i = 0; i < synapsesFrom->size(); i++) //process all the synapses sourced by this neuron
			{
				SynapseBase s = synapsesFrom->at(i);
				if (s.GetModel() == SynapseBase::modelType::Hebbian2 || s.GetModel() == SynapseBase::modelType::Hebbian1 || s.GetModel() == SynapseBase::modelType::Binary)
				{
					NeuronBase* nTarget = s.GetTarget();
					//did this neuron fire coincident or just after the target (the source since these are FROM synapses)
					float weight = s.GetWeight();
					int delay = 1;
					if (s.GetModel() == SynapseBase::modelType::Hebbian2 || s.GetModel() == SynapseBase::modelType::Hebbian1) delay = 6;
								//	if (s.GetModel() == SynapseBase::modelType::Hebbian2 && nTarget->lastFired <= lastFired - 100)
								//		{
							//				weight = NewHebbianWeight(weight, 0, s.GetModel(), numHebbian);
						//				}
					//					else
					if (s.GetModel() == SynapseBase::modelType::Hebbian2 ||
						s.GetModel() == SynapseBase::modelType::Hebbian1 ||
						s.GetModel() == SynapseBase::modelType::Binary)
					{
						if (nTarget->lastFired >= lastFired - delay)
						{
							//strengthen the synapse
							weight = NewHebbianWeight(weight, .1f, s.GetModel(), numHebbian);
						}
						else
						{
							//weaken the synapse
							weight = NewHebbianWeight(weight, -.1f, s.GetModel(), numHebbian);
						}
						//update the synapse in "From"
						synapsesFrom->at(i).SetWeight(weight);
						//update the synapse in "To"
						for (int i = 0; i < nTarget->synapses->size(); i++)
						{
							if (nTarget->synapses->at(i).GetTarget() == this)
							{
								while (nTarget->vectorLock.exchange(1) == 1) {}
								nTarget->synapses->at(i).SetWeight(weight);
								nTarget->vectorLock = 0;
							}
						}
				}
				*/
				}
			}
			vectorLock = 0;
		}
	}
	//This is table handles synapse weight learning
	//It is called if a Hebbian synapse fires and either DOES or DOES NOT cause firing in the target
	//Consider it to be a lookup table until we figure out how weights actually vary
	const int ranges1 = 7;
	double cutoffs1[ranges1] = { 1,    .5,  .34,   .25,     .2,  .15,    0 };
	double posIncr1[ranges1] = { 0,    .1,   .05,  .025,   .01,  .012,   .01 };
	double negIncr1[ranges1] = { -.01,-.1, -.017, -.00625,-.002, -.002,  -.001 };

	//play with this for experimentation
	const int ranges2 = 7;
	double cutoffs2[ranges2] = { .5,   .25,    .1,  0,   -.1 ,-.25, -1 };
	double posIncr2[ranges2] = { .2,    .1,   .05,  .05,  .05,  .1,   .5 };
	//	double negIncr2[ranges2] = { -.5, -.1, -.05, -.05,  -.05, -.1,  -.2 };
	double negIncr2[ranges2] = { -.25, -.05, -.025, -.025,  -.025, -.05,  -.1 };
	//	double negIncr2[ranges2] = { -.125, -.025, -.0125, -.0125,  -.0125, -.025,  -.05 };

	void 神经元Base::Fire3()
	{
		if (model == modelType::FloatValue) return;
		if (model == modelType::Color && lastCharge != 0)
			return;
		if (synapses != NULL)
		{
			while (vectorLock.exchange(1) == 1) {} //prevent the vector of synapses from changing while we're looking at it
			for (int i = 0; i < synapses->size(); i++) //process all the synapses sourced by this neuron
			{
				突触Base s = synapses->at(i);
				神经元Base* nTarget = s.GetTarget();

				if (s.GetModel() == 突触Base::modelType::Hebbian1)
				{
					//did the target neuron fire after this stimulation?
					float weight = s.GetWeight();
					if (nTarget->currentCharge >= 1 && currentCharge >= 1)
					{
						//strengthen the synapse
						weight = NewHebbianWeight(weight, .1f, s.GetModel(), 1);
					}
					if (nTarget->currentCharge >= 1 && currentCharge < 1 ||
						nTarget->currentCharge < 1 && currentCharge >= 1)
					{
						//weaken the synapse
						weight = NewHebbianWeight(weight, -.1f, s.GetModel(), 1);
					}
					synapses->at(i).SetWeight(weight);
				}
			}
			vectorLock = 0;
		}
		if (synapsesFrom != NULL && currentCharge >= threshold)
		{
			int numHebbian = 0;
			int numPosHebbian = 0;
			while (vectorLock.exchange(1) == 1) {} //prevent the vector of synapses from changing while we're looking at it
			for (int i = 0; i < synapsesFrom->size(); i++) //process all the synapses sourced by this neuron
			{
				突触Base s = synapsesFrom->at(i);
				if (s.GetModel() != 突触Base::modelType::Fixed)
				{
					numHebbian++;
					if (s.GetWeight() >= 0) numPosHebbian++;
				}
			}
			for (int i = 0; i < synapsesFrom->size(); i++) //process all the synapses sourced by this neuron
			{
				突触Base s = synapsesFrom->at(i);
				if (s.GetModel() == 突触Base::modelType::Hebbian2 || s.GetModel() == 突触Base::modelType::Binary)
				{
					神经元Base* nTarget = s.GetTarget();
					//did this neuron fire coincident or just after the target (the source since these are FROM synapses)
					float weight = s.GetWeight();
					int delay = 0;
					if (s.GetModel() == 突触Base::modelType::Hebbian2) delay = 6;

					if (s.GetModel() == 突触Base::modelType::Hebbian2 ||
						s.GetModel() == 突触Base::modelType::Binary)
					{
						if (nTarget->lastFired >= lastFired - delay)
						{
							//strengthen the synapse
							weight = NewHebbianWeight(weight, .1f, s.GetModel(), numHebbian);
						}
						else
						{
							//weaken the synapse
							weight = NewHebbianWeight(weight, -.1f, s.GetModel(), numHebbian);
						}
						//update the synapse in "From"
						synapsesFrom->at(i).SetWeight(weight);
						//update the synapse in "To"
						for (int i = 0; i < nTarget->synapses->size(); i++)
						{
							if (nTarget->synapses->at(i).GetTarget() == this)
							{
								while (nTarget->vectorLock.exchange(1) == 1) {}
								nTarget->synapses->at(i).SetWeight(weight);
								nTarget->vectorLock = 0;
							}
						}
					}

				}
			}
			vectorLock = 0;
		}
	}

	float 神经元Base::NewHebbianWeight(float weight, float offset, 突触Base::modelType model, int numberOfSynapses1) //sign of float is all that's presently used
	{
		float numberOfSynapses = numberOfSynapses1 / 2.0f;
		float y = weight * numberOfSynapses;
		if (model == 突触Base::modelType::Binary)
		{
			if (offset > 0)return 1.0f / (float)numberOfSynapses;
			return 0;
		}
		else if (model == 突触Base::modelType::Hebbian1)
		{
			int i = 0;
			y = weight;
			for (i = 0; i < ranges1; i++)
			{
				if (y >= cutoffs1[i])
				{
					if (offset > 0)
						y += (float)posIncr1[i];
					else
						y += (float)negIncr1[i];
					if (y < 0)y = 0;
					if (y > 1) y = 1;
					break;
				}
			}
		}
		else if (model == 突触Base::modelType::Hebbian2)
		{

			float maxVal = 1.0f / numberOfSynapses;
			float curWeight = weight * numberOfSynapses;
			float x = 0;
			if (curWeight >= 1)
			{
				curWeight = 1;
			}
			else if (curWeight <= -1)
			{
				curWeight = -1;
			}
			//			else
			x = atanh(curWeight);

			if (offset != 0)
			{
				x += offset;
				curWeight = tanh(x);
			}
			else
			{
				x *= 0.5;
				curWeight = tanh(x);
			}
			y = curWeight / numberOfSynapses;
			if (y < -maxVal)y = -maxVal;
			if (y > maxVal) y = maxVal;
			//int i = 0;
			//for (i = 0; i < ranges2; i++)
			//{
			//	if (y >= cutoffs2[i])
			//	{
			//		if (offset > 0)
			//			y += (float)posIncr2[i];
			//		else
			//			y += (float)negIncr2[i];
			//		y = y / numberOfSynapses;
			//		if (y < -maxVal)y = -maxVal;
			//		if (y > maxVal) y = maxVal;
			//		break;
			//	}
			//}
		}
		return y;
	}


}

//another way of handling hebbian synapses
//it workes from the target so it can weaken synapses from neurons which don't fire
//if (synapsesFrom != NULL)
//{
//	int hebbianCount = 0;
//	for (int i = 0; i < synapsesFrom->size(); i++)
//	{
//		SynapseBase s = synapsesFrom->at(i);
//		if (s.IsHebbian()) hebbianCount++;
//	}
//	for (int i = 0; i < synapsesFrom->size(); i++)
//	{
//		SynapseBase s = synapsesFrom->at(i);
//		if (s.IsHebbian())
//		{
//			NeuronBase* nTarget = s.GetTarget();
//			while (nTarget->vectorLock.exchange(1) == 1) {}
//			float newWeight = s.GetWeight(); //target = .25 
//			if (nTarget->lastCharge >= threshold)
//			{
//				//hit
//				float target = 1.1f / hebbianCount;
//				newWeight = (newWeight+target)/2; 
//			}
//			else
//			{
//				//miss
//				float target = -.2f;
//				newWeight = (newWeight + target) / 2;
//			}
//			synapsesFrom->at(i).SetWeight(newWeight);
//			if (nTarget->synapses != NULL) //set the forward synapse weight
//			{
//				for (int j = 0; j < nTarget->synapses->size(); j++)
//				{
//					if (nTarget->synapses->at(j).GetTarget() == this)
//					{
//						nTarget->synapses->at(j).SetWeight(newWeight);
//						break;
//					}
//				}
//			}
//			nTarget->vectorLock = 0;
//		}
//	}
//}
