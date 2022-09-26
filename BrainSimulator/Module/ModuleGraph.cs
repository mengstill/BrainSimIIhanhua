//
// Copyright (c) Charles Simon. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
//  

using System;
using System.Collections.Generic;

namespace BrainSimulator.Modules
{
    public class ModuleGraph : ModuleBase
    {
        string[] cols = { "in", "thing", "this", "parent", "child", "attrib", "allAttr", "anyAttr", "match", "next", "nMtch", "head", "alt", "recur", "out", "say", "0" };
        string theInPhrase = "";


        public ModuleGraph()
        {
            minHeight = 10;
            minWidth = 18;
        }
        public override void Fire()
        {
            HandleSequenceSearch();
            HandleSpeechIn1();
            HandleVoiceRequest();
        }

        void AddSynapse(string source, int sourceRow, string dest, int destRow, float weight)
        {
            神经元 n1 = mv.GetNeuronAt(Array.IndexOf(cols, source), sourceRow);
            神经元 n2 = mv.GetNeuronAt(Array.IndexOf(cols, dest), destRow);
            if (n1 != null && n2 != null)
            {
                n1.添加突触(n2.Id, weight); 
            }
        }
        //probably a bad thing to use both attribs and sequenceItems
        public void AddNode(string parent, string name, string[] attribs = null, string[] sequenceItems = null)
        {
            int parentRow = -1;
            int newRow = -1;
            for (int j = 0; j < mv.Height; j++)
            {
                if (parent != "" && mv.GetNeuronAt(0, j).标签名.ToLower() == parent.ToLower()) parentRow = j;
                if (mv.GetNeuronAt(0, j).标签名 == "")
                {
                    newRow = j;
                    break;
                }
            }
            if (newRow == -1) return;  //the module is full
            if (name == "") name = ".";
            mv.GetNeuronAt(0, newRow).标签名 = name;
            if (name != "." )
                mv.GetNeuronAt(Array.IndexOf(cols, "say"), newRow).添加突触(GetSpokenWord(name), 1, 突触.模型类型.Fixed);

            if (parentRow != -1)
            {
                AddSynapse("child", parentRow, "alt", newRow, 1);
                AddSynapse("parent", newRow, "alt", parentRow, 1);
                //na.GetNeuronAt(Array.IndexOf(cols, "child"), parentRow).AddSynapse(na.GetNeuronAt(Array.IndexOf(cols, "alt"), newRow).Id, 1, false);
                //na.GetNeuronAt(Array.IndexOf(cols, "parent"), newRow).AddSynapse(na.GetNeuronAt(Array.IndexOf(cols, "alt"), parentRow).Id, 1, false);
            }

            //add any attributes
            for (int i = 0; attribs != null && i < attribs.Length; i++)
            {
                mv.GetNeuronAt("'1'").添加突触(mv.GetNeuronAt(Array.IndexOf(cols, "match"), newRow), -(attribs.Length - 1), 突触.模型类型.Fixed);
                for (int j = 0; j < mv.Height; j++)
                {
                    if (mv.GetNeuronAt(0, j).标签名 == attribs[i])
                    {
                        mv.GetNeuronAt(Array.IndexOf(cols, "attrib"), newRow).添加突触(mv.GetNeuronAt(Array.IndexOf(cols, "alt"), j), 1, 突触.模型类型.Fixed);
                        mv.GetNeuronAt(Array.IndexOf(cols, "allAttr"), j).添加突触(mv.GetNeuronAt(Array.IndexOf(cols, "match"), newRow), 1, 突触.模型类型.Fixed);
                        mv.GetNeuronAt(Array.IndexOf(cols, "anyAttr"), j).添加突触(mv.GetNeuronAt(Array.IndexOf(cols, "thing"), newRow), 1, 突触.模型类型.Fixed);
                        mv.GetNeuronAt(Array.IndexOf(cols, "attrib"), j).添加突触(mv.GetNeuronAt(Array.IndexOf(cols, "alt"), newRow), 1, 突触.模型类型.Fixed);

                        break;
                    }
                }
            }

            //add any sequence
            int curRow = newRow;
            int prevRow = newRow - 1;
            int nextRow = curRow + 1;
            神经元 nBeginning = mv.GetNeuronAt(Array.IndexOf(cols, "in"), curRow);
            nBeginning.标签名 = name;
            for (int i = 0; sequenceItems != null && i < sequenceItems.Length && nextRow < mv.Height; i++)
            {
                if (i != 0)
                    mv.GetNeuronAt(Array.IndexOf(cols, "in"), curRow).标签名 = ".";


                for (int j = 0; j < mv.Height; j++)
                {
                    if (mv.GetNeuronAt(0, j).标签名 == sequenceItems[i])
                    {
                        //handle searching for a sequence
                        //the initial entry
                        mv.GetNeuronAt(Array.IndexOf(cols, "in"), j).添加突触(mv.GetNeuronAt(Array.IndexOf(cols, "match"), curRow), 1, 突触.模型类型.Fixed);
                        mv.GetNeuronAt(Array.IndexOf(cols, "in"), curRow).添加突触(mv.GetNeuronAt(Array.IndexOf(cols, "match"), curRow), -1, 突触.模型类型.Fixed);
                        mv.GetNeuronAt(Array.IndexOf(cols, "match"), curRow).添加突触(mv.GetNeuronAt(Array.IndexOf(cols, "in"), curRow), 1, 突触.模型类型.Fixed);

                        //subsequent entries
                        if (prevRow >= 0)
                            mv.GetNeuronAt(Array.IndexOf(cols, "thing"), j).添加突触(mv.GetNeuronAt(Array.IndexOf(cols, "nMtch"), prevRow), 1, 突触.模型类型.Fixed);
                        mv.GetNeuronAt("nMtch").添加突触(mv.GetNeuronAt(Array.IndexOf(cols, "in"), curRow), -1, 突触.模型类型.Fixed);
                        mv.GetNeuronAt(Array.IndexOf(cols, "thing"), curRow).添加突触(mv.GetNeuronAt(Array.IndexOf(cols, "nMtch"), curRow), 1, 突触.模型类型.Fixed);
                        mv.GetNeuronAt("'1'").添加突触(mv.GetNeuronAt(Array.IndexOf(cols, "nMtch"), curRow), -1, 突触.模型类型.Fixed);

                        mv.GetNeuronAt(Array.IndexOf(cols, "nMtch"), curRow).添加突触(mv.GetNeuronAt(Array.IndexOf(cols, "in"), curRow), -1, 突触.模型类型.Fixed);
                        mv.GetNeuronAt(Array.IndexOf(cols, "thing"), curRow).添加突触(mv.GetNeuronAt(Array.IndexOf(cols, "head"), curRow), 1, 突触.模型类型.Fixed);
                        mv.GetNeuronAt(Array.IndexOf(cols, "head"), curRow).添加突触(nBeginning, 1, 突触.模型类型.Fixed);
                        mv.GetNeuronAt(Array.IndexOf(cols, "attrib"), curRow).添加突触(mv.GetNeuronAt(Array.IndexOf(cols, "alt"), j), 1, 突触.模型类型.Fixed);

                        if (i != 0)
                            mv.GetNeuronAt(Array.IndexOf(cols, "head"), curRow).添加突触(mv.GetNeuronAt(Array.IndexOf(cols, "in"), curRow), -1, 突触.模型类型.Fixed);

                        if (i < sequenceItems.Length - 1)
                        {
                            //handle playing a sequence
                            mv.GetNeuronAt(Array.IndexOf(cols, "next"), curRow).添加突触(mv.GetNeuronAt(Array.IndexOf(cols, "thing"), nextRow), 1, 突触.模型类型.Fixed);
                            mv.GetNeuronAt(Array.IndexOf(cols, "next"), curRow).添加突触(mv.GetNeuronAt(Array.IndexOf(cols, "in"), nextRow), 1, 突触.模型类型.Fixed);

                            //handle searching
                            mv.GetNeuronAt(Array.IndexOf(cols, "nMtch"), curRow).添加突触(mv.GetNeuronAt(Array.IndexOf(cols, "in"), nextRow), 1, 突触.模型类型.Fixed);
                        }
                        else
                        {
                            //stop the playback
                            mv.GetNeuronAt(Array.IndexOf(cols, "next"), curRow).添加突触(mv.GetNeuronAt(Array.IndexOf(cols, "next"), 0), -1, 突触.模型类型.Fixed);

                            //searching...last element needs to match for "head" to work
                        }
                        break;
                    }
                }
                prevRow = curRow;
                curRow = nextRow;
                nextRow = curRow + 1;
            }
        }
        public void Test()
        {
            //AddThing("Attribute", "Size");
            //AddThing("Size", "Big");
            //AddThing("Size", "Little");
            AddNode("Attribute", "Color");
            AddNode("Color", "Red");
            AddNode("Color", "Blue");
            //AddThing("Attribute", "Shape");
            //AddThing("Shape", "Square");
            //AddThing("Shape", "Circle");
            //AddThing("Thing", "R-S", new string[] { "Red", "Square" });
            //AddThing("Thing", "R-C", new string[] { "Red", "Circle" });
            //AddThing("Thing", "B-B-C", new string[] { "Big", "Blue", "Circle" });
            //AddThing("Thing", "L-B-C", new string[] { "Little", "Blue", "Circle" });

            //AddThing("Attribute", "Digit");
            //AddThing("Digit", "1");
            //AddThing("Digit", "2");
            //AddThing("Digit", "3");
            //AddThing("Digit", "4");
            //AddThing("Digit", "5");
            //AddThing("Digit", "9");
            //AddThing("Digit", "point");
            //AddThing("Sequence", "Count", null, new string[] { "1", "2", "3" });
            //AddThing("Sequence", "Down", null, new string[] { "5", "4", "3", "2", "1" });
            //AddThing("Sequence", "pi", null, new string[] { "3", "point", "1", "4", "1", "5", "9" });



            AddNode("Attribute", "word");
            AddNode("word", "Mary");
            AddNode("word", "had");
            AddNode("word", "a");
            AddNode("word", "little");
            AddNode("word", "lamb");
            AddNode("Sequence", "M1", null, new string[] { "Mary", "had", "a", "little", "lamb" });

        }



        int FindRowByLabel(string label)
        {
            int retVal = -1;
            for (int i = 0; i < mv.Height; i++)
            {
                if (mv.GetNeuronAt(0, i).标签名.ToLower() == label) return i;
            }
            return retVal;
        }

        bool phraseIsComplete = false;
        List<string> searchSequence = new List<string>();

        void HandleSequenceSearch()
        {
            if (searchSequence.Count == 0) return;
            if (searchSequence[0] == "nop")
            {
            }
            else if (searchSequence[0] == "clr")
            {
                mv.GetNeuronAt("clr").SetValue(1);
            }
            else if (searchSequence[0] == "attrib")
            {
                mv.GetNeuronAt("attrib").SetValue(1);
            }
            else if (searchSequence[0] == "attrib0")
            {
                mv.GetNeuronAt("attrib").SetValue(0);
            }
            else if (searchSequence[0] == "match")
            {
                mv.GetNeuronAt("match").SetValue(1);
            }
            else if (searchSequence[0] == "match0")
            {
                mv.GetNeuronAt("match").SetValue(0);
            }
            else if (searchSequence[0] == "nMtch")
            {
                mv.GetNeuronAt("nMtch").SetValue(1);
            }
            else if (searchSequence[0] == "next")
            {
                mv.GetNeuronAt("next").SetValue(1);
            }
            else if (searchSequence[0] == "next0")
            {
                mv.GetNeuronAt("next").SetValue(0);
            }
            else if (searchSequence[0] == "head")
            {
                mv.GetNeuronAt("head").SetValue(1);
            }
            else
            {
                神经元 n = mv.GetNeuronAt(searchSequence[0]);
                if (n != null && n.LastCharge == 0)
                    n.SetValue(1);
                else if (n != null)
                    n.SetValue(0);
            }
            searchSequence.RemoveAt(0);
        }


        void HandleVoiceRequest()
        {
            if (!phraseIsComplete) return;
            for (int i = 2; i < mv.Height; i++)
                mv.GetNeuronAt(0, i).SetValue(0);
            theInPhrase = theInPhrase.ToLower();
            theInPhrase = theInPhrase.Replace("a ", "").
                Replace("an ", "").
                Replace("the ", "").
                Replace("some ", "").
                Replace("is ", "").
                Replace("with ", "").
                Replace("which ", "").
                Replace("are ", "").
                Replace("containing ", "").
                Replace("  ", " ");
            string[] words = theInPhrase.Split(' ');


            //if (words[0] == "what")
            //{
            //    int row = FindRowByLabel(words[1]);
            //    if (words[1] == "pi" || words[1] == "mary")
            //    {
            //        if (words[1] == "mary" ) row = FindRowByLabel("M1");
            //        na.GetNeuronAt(0, row).SetValue(1);
            //        na.GetNeuronAt(Array.IndexOf(cols, "next"), 0).SetValue(1);
            //        prePend = words[1] + " is ";
            //    }
            //    else if (row != -1)
            //    {
            //        na.GetNeuronAt(0, row).SetValue(1);
            //        na.GetNeuronAt(Array.IndexOf(cols, "parent"), 0).SetValue(1);
            //        prePend = words[1] + " is a ";
            //    }
            //    else
            //    {
            //        toSpeak = "I don't know";
            //    }
            //}
            //if (words[0] == "name")
            //{
            //    PluralizationService ps = PluralizationService.CreateService(new CultureInfo("en-us"));
            //    if (words[1] == "things")
            //    {
            //        int row = FindRowByLabel(words[2]);
            //        if (row != -1)
            //        {
            //            na.GetNeuronAt(0, row).SetValue(1);
            //            na.GetNeuronAt(Array.IndexOf(cols, "attrib"), 0).SetValue(1);
            //            insertAnd = "and ";
            //            postPend = " are " + words[2];
            //        }

            //    }
            //    else
            //    {
            //        string sing = ps.Singularize(words[1]);
            //        int row = FindRowByLabel(sing);
            //        if (row != -1)
            //        {
            //            na.GetNeuronAt(0, row).SetValue(1);
            //            na.GetNeuronAt(Array.IndexOf(cols, "child"), 0).SetValue(1);
            //            insertAnd = "and ";
            //            postPend = " are " + words[1];
            //        }
            //        else
            //        {
            //            toSpeak = "I don't know";
            //        }
            //    }
            //}
            if (words[0] == "sequence")
            {
                if (words.Length == 2)
                { //say the sequence by name

                }
                else //earch for the sequence
                {
                    searchSequence.Add("match");
                    for (int i = 1; i < words.Length; i++)
                    {
                        searchSequence.Add(words[i]);
                        searchSequence.Add("nMtch");
                        if (i == 1)
                            searchSequence.Add("match0");
                        searchSequence.Add(words[i]);
                    }
                    searchSequence.Add("head");
                    searchSequence.Add("nop");
                    searchSequence.Add("nop");
                    searchSequence.Add("nop");
                    searchSequence.Add("nop");
                    searchSequence.Add("attrib");
                    searchSequence.Add("nop");
                    searchSequence.Add("next");
                    searchSequence.Add("nop");
                    searchSequence.Add("nop");
                    searchSequence.Add("nop");
                    searchSequence.Add("nop");
                    searchSequence.Add("nop");
                    searchSequence.Add("nop");
                    searchSequence.Add("nop");
                    searchSequence.Add("nop");
                    searchSequence.Add("nop");
                    searchSequence.Add("nop");
                    searchSequence.Add("nop");
                    searchSequence.Add("nop");
                    searchSequence.Add("nop");
                    searchSequence.Add("nop");
                    searchSequence.Add("nop");
                    searchSequence.Add("next0");
                    searchSequence.Add("clr");
                    searchSequence.Add("attrib0");
                }
            }
            if (words[0] == "add")
            {
                if (words.Length < 3) return;
                int rowParent = FindRowByLabel(words[1]);
                if (rowParent != -1)
                {
                    AddNode(words[1], words[2]);
                    //      toSpeak = words[1] + " " + words[2] + " added";
                }
                else
                {
                    //      toSpeak = "Category not found";
                }
            }
            theInPhrase = "";
            phraseIsComplete = false;
        }

        private void HandleSpeechIn1()
        {
            模块视图 naIn = MainWindow.此神经元数组.FindModuleByLabel("ModuleSpeechIn");
            if (naIn == null) return;

            foreach (神经元 n in naIn.Neurons)
            {
                if (!n.InUse()) break;
                if (n.LastCharge == 1)
                {
                    if (theInPhrase != "") theInPhrase += " ";
                    theInPhrase += n.标签名;
                    return;
                }
            }
            if (theInPhrase != "")
                phraseIsComplete = true;
        }
        public 神经元 GetSpokenWord(string word)
        {
            模块视图 naOut = MainWindow.此神经元数组.FindModuleByLabel("ModuleSPeechOut");
            神经元 n = null;
            if (naOut != null)
            {
                for (int i = 0; i < naOut.NeuronCount; i++)
                {
                    神经元 n1 = naOut.GetNeuronAt(i);
                    if (n1.标签名 == word) return n1;
                    if (n1.标签名 == "")
                    {
                        n1.标签名 = word;
                        return n1;
                    }
                }
            }
            return n;
        }
        public override void Initialize()
        {
            ClearNeurons();
            神经元 n0 = mv.GetNeuronAt(1, 1);
            n0.标签名 = "'1'";
            n0.添加突触(n0, 1, 突触.模型类型.Fixed);
            n0.SetValue(1);
            mv.GetNeuronAt(0, 1).标签名 = "clr";
            for (int i = 0; i < cols.Length; i++)
            {
                神经元 n = mv.GetNeuronAt(i, 0);
                n.标签名 = cols[i];
            }
            //put in the vertical synapses for the columns which need them
            for (int i = 0; i < cols.Length; i++)
            {
                if (cols[i] == "this" || cols[i] == "this" ||
                    cols[i] == "attrib" || cols[i] == "parent" ||
                    cols[i] == "child" || cols[i] == "next" || cols[i] == "nMtch" || cols[i] == "head" ||
                    cols[i] == "anyAttr" || cols[i] == "allAttr" ||
                    cols[i] == "recur" || cols[i] == "head" ||
                    cols[i] == "match" ||
                    cols[i] == "say")
                {
                    神经元 n = mv.GetNeuronAt(i, 0);
                    神经元 n1 = mv.GetNeuronAt(i, 1);
                    n.添加突触(n1, -1, 突触.模型类型.Fixed);
                    n0.添加突触(n1, 1, 突触.模型类型.Fixed);
                    for (int j = 2; j < mv.Height; j++)
                    {
                        n1.添加突触(mv.GetNeuronAt(i, j), -1, 突触.模型类型.Fixed);
                    }
                }
            }
            //make the clr neuron clear all the input neurons
            神经元 nClr = mv.GetNeuronAt("clr");
            for (int j = 2; j < mv.Height; j++)
            {
                nClr.添加突触(mv.GetNeuronAt(Array.IndexOf(cols, "in"), j), -1, 突触.模型类型.Fixed);
            }
            mv.GetNeuronAt("recur").添加突触(nClr, 1, 突触.模型类型.Fixed);

            //put in all the horizontal synapses
            for (int j = 2; j < mv.Height; j++)
            {
                mv.GetNeuronAt(Array.IndexOf(cols, "in"), j).添加突触(mv.GetNeuronAt(Array.IndexOf(cols, "in"), j), 1, 突触.模型类型.Fixed);
                mv.GetNeuronAt(Array.IndexOf(cols, "in"), j).添加突触(mv.GetNeuronAt(Array.IndexOf(cols, "thing"), j), 10, 突触.模型类型.Fixed);
                mv.GetNeuronAt(Array.IndexOf(cols, "thing"), j).添加突触(mv.GetNeuronAt(Array.IndexOf(cols, "this"), j), 1, 突触.模型类型.Fixed);
                mv.GetNeuronAt(Array.IndexOf(cols, "thing"), j).添加突触(mv.GetNeuronAt(Array.IndexOf(cols, "attrib"), j), 1, 突触.模型类型.Fixed);
                mv.GetNeuronAt(Array.IndexOf(cols, "thing"), j).添加突触(mv.GetNeuronAt(Array.IndexOf(cols, "anyAttr"), j), 1, 突触.模型类型.Fixed);
                mv.GetNeuronAt(Array.IndexOf(cols, "thing"), j).添加突触(mv.GetNeuronAt(Array.IndexOf(cols, "allAttr"), j), 1, 突触.模型类型.Fixed);
                mv.GetNeuronAt(Array.IndexOf(cols, "thing"), j).添加突触(mv.GetNeuronAt(Array.IndexOf(cols, "parent"), j), 1, 突触.模型类型.Fixed);
                mv.GetNeuronAt(Array.IndexOf(cols, "thing"), j).添加突触(mv.GetNeuronAt(Array.IndexOf(cols, "child"), j), 1, 突触.模型类型.Fixed);
                mv.GetNeuronAt(Array.IndexOf(cols, "thing"), j).添加突触(mv.GetNeuronAt(Array.IndexOf(cols, "next"), j), 1, 突触.模型类型.Fixed);


                mv.GetNeuronAt(Array.IndexOf(cols, "this"), j).添加突触(mv.GetNeuronAt(Array.IndexOf(cols, "out"), j), 1, 突触.模型类型.Fixed);
                mv.GetNeuronAt(Array.IndexOf(cols, "alt"), j).添加突触(mv.GetNeuronAt(Array.IndexOf(cols, "out"), j), 1, 突触.模型类型.Fixed);
                //na.GetNeuronAt(Array.IndexOf(cols, "out"), j).AddSynapse(na.GetNeuronAt(Array.IndexOf(cols, "in"), j).Id, -1, false);
                mv.GetNeuronAt(Array.IndexOf(cols, "out"), j).添加突触(mv.GetNeuronAt(Array.IndexOf(cols, "recur"), j), 1, 突触.模型类型.Fixed);
                mv.GetNeuronAt(Array.IndexOf(cols, "recur"), j).添加突触(mv.GetNeuronAt(Array.IndexOf(cols, "in"), j), 2, 突触.模型类型.Fixed); //2 because the out is suppressing
                mv.GetNeuronAt(Array.IndexOf(cols, "match"), j).添加突触(mv.GetNeuronAt(Array.IndexOf(cols, "thing"), j), 1, 突触.模型类型.Fixed);



                mv.GetNeuronAt(Array.IndexOf(cols, "out"), j).添加突触(mv.GetNeuronAt(Array.IndexOf(cols, "say"), j), 1, 突触.模型类型.Fixed);
                mv.GetNeuronAt(Array.IndexOf(cols, "out"), j).添加突触(mv.GetNeuronAt(Array.IndexOf(cols, "0"), j), 1, 突触.模型类型.Fixed);
                mv.GetNeuronAt(Array.IndexOf(cols, "0"), j).添加突触(mv.GetNeuronAt(Array.IndexOf(cols, "say"), j), -1, 突触.模型类型.Fixed);

                mv.GetNeuronAt(Array.IndexOf(cols, "next"), j).添加突触(mv.GetNeuronAt(Array.IndexOf(cols, "in"), j), -1, 突触.模型类型.Fixed);
                mv.GetNeuronAt(Array.IndexOf(cols, "next"), j).添加突触(mv.GetNeuronAt(Array.IndexOf(cols, "thing"), j), -30, 突触.模型类型.Fixed);
            }
            //make som coluimns into an always-fire
            mv.GetNeuronAt(Array.IndexOf(cols, "next"), 0).添加突触(mv.GetNeuronAt(Array.IndexOf(cols, "next"), 0), 1, 突触.模型类型.Fixed);
            mv.GetNeuronAt(Array.IndexOf(cols, "say"), 0).添加突触(mv.GetNeuronAt(Array.IndexOf(cols, "say"), 0), 1, 突触.模型类型.Fixed);
            mv.GetNeuronAt(Array.IndexOf(cols, "attrib"), 0).添加突触(mv.GetNeuronAt(Array.IndexOf(cols, "attrib"), 0), 1, 突触.模型类型.Fixed);
            mv.GetNeuronAt(Array.IndexOf(cols, "match"), 0).添加突触(mv.GetNeuronAt(Array.IndexOf(cols, "match"), 0), 1, 突触.模型类型.Fixed);

            //na.GetNeuronAt(0, 2).Label = "Attribute";
            //na.GetNeuronAt(0, 3).Label = "Thing";
            //na.GetNeuronAt(0, 4).Label = "Sequence";
            //na.GetNeuronAt("say").SetValue(1);
            //if (naOut != null)
            //{
            //    na.GetNeuronAt(Array.IndexOf(cols, "say"), 2).AddSynapse(GetSpokenWord("Attribute").Id, 1, false);
            //    na.GetNeuronAt(Array.IndexOf(cols, "say"), 3).AddSynapse(GetSpokenWord("Thing").Id, 1, false);
            //    na.GetNeuronAt(Array.IndexOf(cols, "say"), 4).AddSynapse(GetSpokenWord("Sequence").Id, 1, false);
            //}
            Test();
        }



    }
}
