//
// Copyright (c) Charles Simon. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
//  

using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace BrainSimulator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //TODO: Migrate this to a separate object
        static bool 引擎是否暂停 = false;
        static long engineElapsed = 0;
        static long displayElapsed = 0;
        static bool 更新显示 = false;

        static List<int> engineTimerMovingAverage;
        static public void UpdateEngineLabel(int firedCount)
        {
            if (引擎是否暂停) return;
            if (engineTimerMovingAverage == null)
            {
                engineTimerMovingAverage = new List<int>();
                for (int i = 0; i < 100; i++)
                {
                    engineTimerMovingAverage.Add(0);
                }
            }
            engineTimerMovingAverage.RemoveAt(0);
            engineTimerMovingAverage.Add((int)engineElapsed);
            string engineStatus = "运行, 速度: " + thisWindow.slider.Value + "  循环: " + 此神经元数组.Generation.ToString("N0") +
            "  " + firedCount.ToString("N0") + " 神经元冲动  " + (engineTimerMovingAverage.Average() / 10000f).ToString("F2") + "ms";
            thisWindow.设置窗口底部状态(3, engineStatus, 0);
        }

        static bool 引擎是否被取消 = false;
        private void 引擎循环()
        {
            while (!引擎是否被取消)
            {
                if (数组是否为空())
                {
                    Thread.Sleep(100);
                }
                else if (IsEngineSuspended())
                {
                    if (更新显示)
                    {
                        Application.Current.Dispatcher.Invoke((Action)delegate
                        {
                            设置窗口底部状态(3, "停止运行   循环: " + 此神经元数组.Generation.ToString("N0"), 0);
                            //thisWindow.SetPlayPauseButtonImage(true);
                        });
                        更新显示 = false;
                        displayUpdateTimer.Start();
                    }
                    Thread.Sleep(100); //check the engineDelay every 100 ms.
                    引擎是否暂停 = true;
                }
                else
                {
                    引擎是否暂停 = false;
                    if (此神经元数组 != null)
                    {
                        long start = 跨语言接口.GetPreciseTime();
                        此神经元数组.Fire();
                        long end = 跨语言接口.GetPreciseTime();
                        engineElapsed = end - start;

                        if (更新显示)
                        {
                            Application.Current.Dispatcher.Invoke((Action)delegate
                            {
                                long dStart = 跨语言接口.GetPreciseTime();
                                if (!fullUpdateNeeded)
                                    此神经元数组视图.更新神经元颜色();
                                else
                                    此神经元数组视图.更新();
                                fullUpdateNeeded = false;
                                long dEnd = 跨语言接口.GetPreciseTime();
                                displayElapsed = dEnd - dStart;
                            });
                            更新显示 = false;
                            displayUpdateTimer.Start();
                        }
                    }
                    Thread.Sleep(Math.Abs(引擎延迟));
                }
            }
        }

        // stack to make sure we supend and resume the engine properly
        static Stack<int> engineSpeedStack = new Stack<int>();

        public bool IsEngineSuspended()
        {
            return engineSpeedStack.Count > 0;
        }

        public static void 暂停引擎()
        {
            // just pushing an int here, we won't restore it later
            // 只是在这里推一个int，我们以后不会恢复它
            engineSpeedStack.Push(引擎延迟);
            if (此神经元数组 == null)
                return;

            // wait for the engine to actually stop before returning
            // 等待引擎真正停止后再返回
            while (此神经元数组 != null && !引擎是否暂停)
            {
                Thread.Sleep(100);
                System.Windows.Forms.Application.DoEvents();
            }
        }

        public static void 恢复引擎()
        {
            // first pop the top to make sure we balance the suspends and resumes
            // 首先弹出顶部以确保我们平衡暂停和恢复
            if (engineSpeedStack.Count > 0)
                引擎延迟 = engineSpeedStack.Pop();
            if (此神经元数组 == null)
                return;

            // resume the engine
            // 恢复引擎
            // on shutdown, the current application may be gone when this is requested
            // 关机时，当前应用程序可能会在请求时消失
            if (此神经元数组 != null && Application.Current != null)
            {
                Application.Current.Dispatcher.Invoke((Action)delegate
                {
                    MainWindow.thisWindow.SetSliderPosition(引擎延迟);
                });
            }
        }
    }
}
