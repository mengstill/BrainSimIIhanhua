//
// Copyright (c) Charles Simon. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
//  

using System;
using System.Collections.Generic;

namespace BrainSimulator.Modules
{
    public class ModuleHearWords : ModuleBase
    {
        private List<string> words = new List<string>();


        public override void Fire()
        {
            Init();  //be sure to leave this here
            if (words.Count > 0)
            {
                string word = words[0];
                foreach (神经元 n in mv.Neurons)
                {
                    if (n.标签名 == word)
                    {
                        n.SetValue(1);
//                        n.CurrentCharge = 1;
                        break;
                    }
                    else if (n.标签名 == "")
                    {
                        //the word has not been heard before, add it
                        n.标签名 = word;
                        n.CurrentCharge = 1;
                        //connection to UKS 
                        ModuleUKSN nmUKS = (ModuleUKSN)FindModleu(typeof(ModuleUKSN));
                        if (nmUKS != null)
                        {
                            IList<Thing> words = nmUKS.Labeled("Word").Children;
                            Thing w = nmUKS.Valued(word, words);
                            if (w == null)
                            {
                                string label = "w" + char.ToUpper(word[0]) + word.Substring(1);
                                w = nmUKS.AddThing(label, nmUKS.Labeled("Word"), word);
                            }
                            神经元 n1 = nmUKS.GetNeuron(w);
                            if (n1 != null)
                                n.添加突触(n1.Id, 1);
                            //TODO: add a synapse to the speakwords module as well
                        }
                        break;
                    }
                }
                words.RemoveAt(0);
            }
        }

        public void HearPhrase(string phrase)
        {
            if (words.Count != 0) return;
            string[] words1 = phrase.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string word in words1)
            {
                words.Add(word);
            }
        }

        //fill this method in with code which will execute once
        //when the module is added, when "initialize" is selected from the context menu,
        //or when the engine restart button is pressed
        public override void Initialize()
        {
            foreach (神经元 n in mv.Neurons)
            {
                n.标签名 = "";
                n.删除所有突触();
            }

            神经元 n1 = mv.GetNeuronAt(0);
            n1.标签名 = "good";
            神经元 n1Target = GetNeuron("Module2DUKS", "Positive");
            if (n1Target != null)
                n1.添加突触(n1Target.Id, 1);
            神经元 n2 = mv.GetNeuronAt(1);
            n2.标签名 = "no";
            神经元 n2Target = GetNeuron("Module2DUKS", "Negative");
            if (n2Target != null)
                n2.添加突触(n2Target.Id, 1);
        }
    }
}
