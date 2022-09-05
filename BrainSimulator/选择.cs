//
// Copyright (c) Charles Simon. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
//  

using System.Collections.Generic;
using System.Linq;

namespace BrainSimulator
{
    public class 选择
    {

        //array of rectangular selection areas
        public List<选择矩阵> selectedRectangles = new List<选择矩阵>();

        //step counter 
        int 位置 = -1;

        //the nueron ID of the current neurons
        public int 选中神经元索引;

        //list to avoid duplicates in enumeration
        List<神经元> 已访问神经元列表 = new List<神经元>();

        //create a sorted list of all the neurons in the selection
        public List<int> 枚举选择的神经元()
        {
            位置 = -1;
            已访问神经元列表.Clear();

            List<int> neuronsInSelection = new List<int>();
            neuronsInSelection.Clear();
            for (int i = 0; i < selectedRectangles.Count; i++)
            {
                if (selectedRectangles[i] != null)
                {
                    List<int> neuronsInRectangle = new List<int>();
                    foreach (int neuronID in selectedRectangles[i].矩阵中的神经元())
                    {
                        neuronsInRectangle.Add(neuronID);
                    }
                    neuronsInSelection = neuronsInSelection.Union(neuronsInRectangle).ToList();
                }
            }
            neuronsInSelection.Sort();
            return neuronsInSelection;
        }

        List<int> selectedNeurons = new List<int>();
        public IEnumerable<神经元> 神经元集合
        {
            get
            {
                if (selectedNeurons.Count == 0)
                {
                    selectedNeurons = 枚举选择的神经元();
                }
                for (int i = 0; i < selectedNeurons.Count; i++)
                    yield return MainWindow.此神经元数组.获取神经元(selectedNeurons[i]);
                selectedNeurons.Clear();
            }
        }

        public 神经元 获取选中的神经元()
        {
            位置++;
            int currentStart = 0;
            神经元 n = null;
            for (int i = 0; i < selectedRectangles.Count; i++)
            {
                if (selectedRectangles[i] != null)
                {
                    while (位置 < currentStart + selectedRectangles[i].GetLength())
                    {
                        //the index into the current rectangle
                        int index = 位置 - currentStart;
                        selectedRectangles[i].获取选择区域(out int X1, out int Y1, out int X2, out int Y2);
                        int height = Y2 - Y1 + 1;
                        选中神经元索引 = selectedRectangles[i].首个选中的神经元 + (index / height) *
                            MainWindow.此神经元数组.rows + index % height;
                        if (选中神经元索引 > MainWindow.此神经元数组.arraySize) return null;
                        n = MainWindow.此神经元数组.获取神经元(选中神经元索引);
                        if (!已访问神经元列表.Contains(n))
                        {
                            已访问神经元列表.Add(n);
                            return n;
                        }
                        else
                            位置++;

                    }
                }
                if (selectedRectangles[i] != null)
                    currentStart += selectedRectangles[i].GetLength();
            }
            return n;
        }

        //returns the number of times the neuron occurs in the selection area
        public int NeuronInSelection(int neuronIndex)
        {
            int count = 0;
            for (int i = 0; i < selectedRectangles.Count; i++)
                if (selectedRectangles[i] != null)
                    if (selectedRectangles[i].NeuronIsInSelection(neuronIndex))
                        count++;
            return count;
        }

        //counts the number of neurons in the selection (and deducts for overlapping selections
        public int 获取选中神经元数()
        {
            int count = 0;
            枚举选择的神经元();
            for (神经元 n = 获取选中的神经元(); n != null; n = 获取选中的神经元())
            {
                count++;
            }
            return (int)count;
        }

        public void 获取选择的矩形边界(out int X1o, out int Y1o, out int X2o, out int Y2o)
        {
            X1o = Y1o = int.MaxValue;
            X2o = Y2o = int.MinValue;
            for (int i = 0; i < selectedRectangles.Count; i++)
            {
                if (selectedRectangles[i] != null)
                {
                    selectedRectangles[i].获取选择区域(out int X1, out int Y1, out int X2, out int Y2);
                    if (X1 < X1o) X1o = X1;
                    if (Y1 < Y1o) Y1o = Y1;
                    if (X2 > X2o) X2o = X2;
                    if (Y2 > Y2o) Y2o = Y2;
                }
            }
        }
    }
}
