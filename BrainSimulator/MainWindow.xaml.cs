//
// Copyright (c) Charles Simon. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
//  

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
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

    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            if (e.Args.Length == 1)
                StartupString = e.Args[0];
        }
        private static string startupString = "";

        public static string StartupString { get => startupString; set => startupString = value; }
    }
    public partial class MainWindow : Window
    {
        //Globals
        public static 神经元数组视图 神经元数组视图 = null;
        public static 神经元数组 此神经元数组 = null;
        //for cut-copy-paste
        public static 神经元数组 myClipBoard = null; //refactor back to private

        public static FiringHistoryWindow fwWindow = null;
        public static NotesDialog notesWindow = null;

        readonly Thread 引擎线程;

        public static bool useServers = false;

        private static int 引擎延迟 = 500; //wait after each cycle of the engine, 0-1000

        //timer to update the neuron values 
        readonly private DispatcherTimer displayUpdateTimer = new DispatcherTimer();

        // if the control key is pressed...used for adding multiple selection areas
        public static bool ctrlPressed = false;
        public static bool shiftPressed = false;

        //the name of the currently-loaded network file
        public static string currentFileName = "";

        public static MainWindow thisWindow;
        readonly Window splashScreen = new SplashScreeen();

        public ProgressDialog progressDialog;

        public MainWindow()
        {
            //this puts up a dialog on unhandled exceptions
#if !DEBUG
            AppDomain.CurrentDomain.UnhandledException += (sender, eventArgs) =>
                {
                    string message = "Brain Simulator encountered an error--";
                    Version version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
                    DateTime buildDate = new DateTime(2000, 1, 1).AddDays(version.Build).AddSeconds(version.Revision * 2);
                    message += "\r\nVersion: " + $"{version.Major}.{version.Minor}.{version.Build}   ({buildDate})";
                    message += "\r\n.exe location: " + System.Reflection.Assembly.GetExecutingAssembly().Location;
                    message += "\r\nPROGRAM WILL EXIT  (this message copied to clipboard)";

                    message += "\r\n\r\n"+ eventArgs.ExceptionObject.ToString();
                 
                    System.Windows.Forms.Clipboard.SetText(message);
                    MessageBox.Show(message);
                    Application.Current.Shutdown(255);
                };
#endif

            引擎线程 = new Thread(new ThreadStart(引擎循环)) { Name = "EngineThread" };

            //testing of crash message...
            //throw new FileNotFoundException();

            InitializeComponent();
            
            引擎线程.Priority = ThreadPriority.Lowest;

            displayUpdateTimer.Tick += DisplayUpdate_TimerTick;
            displayUpdateTimer.Interval = TimeSpan.FromMilliseconds(100);
            displayUpdateTimer.Start();

            神经元数组视图 = 此神经元数组视图;
            Width = 1100;
            Height = 600;
            slider_ValueChanged(slider, null);

            thisWindow = this;

            splashScreen.Left = 300;
            splashScreen.Top = 300;
            splashScreen.Show();
            DispatcherTimer splashHide = new DispatcherTimer
            {
                Interval = new TimeSpan(0, 0, 3),
            };
            splashHide.Tick += SplashHide_Tick;
            splashHide.Start();
            if (Properties.Settings.Default.UpgradeRequired)
            {
                Properties.Settings.Default.Upgrade();
                Properties.Settings.Default.UpgradeRequired = false;
                Properties.Settings.Default.Save();
            }
            CheckForVersionUpdate();

        }

        private void SplashHide_Tick(object sender, EventArgs e)
        {
            Application.Current.MainWindow = this;
            splashScreen.Close();
            ((DispatcherTimer)sender).Stop();

            bool showHelp = (bool)Properties.Settings.Default["ShowHelp"];
            cbShowHelpAtStartup.IsChecked = showHelp;
            if (showHelp)
            {
                MenuItemHelp_Click(null, null);
            }


            //this is here because the file can be loaded before the mainwindow displays so
            //module dialogs may open before their owner so this happens a few seconds later
            if (此神经元数组 != null)
            {
                lock (此神经元数组.Modules)
                {
                    foreach (模块视图 na in 此神经元数组.Modules)
                    {
                        if (na.TheModule != null)
                        {
                            na.TheModule.SetDlgOwner(this);
                        }
                    }
                }
            }
            神经元视图.打开历史窗口();
        }

        public static void CloseHistoryWindow()
        {
            if (fwWindow != null)
                fwWindow.Close();
            神经冲动历史.历史列表.Clear();
            神经冲动历史.移除();
        }

        private void ShowDialogs()
        {
            foreach (模块视图 na in 此神经元数组.模块)
            {
                if (na.TheModule != null && na.TheModule.dlgIsOpen)
                {
                    Application.Current.Dispatcher.Invoke((Action)delegate
                    {
                        na.TheModule.ShowDialog();
                    });
                }
            }
            if (!此神经元数组.隐藏节点 && 此神经元数组.网络节点 != "")
                Application.Current.Dispatcher.Invoke((Action)delegate
                {
                    MenuItemNotes_Click(null, null);
                });
            神经元视图.打开历史窗口();
        }

        private void LoadMRUMenu()
        {
            MRUListMenu.Items.Clear();
            StringCollection MRUList = (StringCollection)Properties.Settings.Default["MRUList"];
            if (MRUList == null)
                MRUList = new StringCollection();
            foreach (string fileItem in MRUList)
            {
                if (fileItem == null) continue;
                string shortName = Path.GetFileNameWithoutExtension(fileItem);
                MenuItem mi = new MenuItem() { Header = shortName };
                mi.Click += MRUListItem_Click;
                mi.ToolTip = fileItem;
                MRUListMenu.Items.Add(mi);
            }
        }
        private void LoadModuleTypeMenu()
        {
            var moduleTypes = 跨语言接口.GetArrayOfModuleTypes();

            foreach (var moduleType in moduleTypes)
            {
                string moduleName = moduleType.Name;
                moduleName = moduleName.Replace("Module", "");
                Modules.ModuleBase theModule = (Modules.ModuleBase)Activator.CreateInstance(moduleType);
                string toolTip = 模块描述文件.获取工具提示(moduleType.Name);

                MenuItem mi = new MenuItem { Header = moduleName, ToolTip = toolTip, };
                mi.Click += InsertModule_Click;
                InsertModuleMenu.Items.Add(mi);

                ModuleListComboBox.Items.Add(new Label { Content = moduleName, ToolTip = toolTip, Margin = new Thickness(0), Padding = new Thickness(0) });
            }
        }

        private void LoadFindMenus()
        {
            if (数组是否为空()) return;
            NeuronMenu.Items.Clear();

            List<string> neuronLabelList = 此神经元数组.从缓存标签中获取值();
            List<int> neuronIdList = 此神经元数组.从缓存标签中获取键();
            for (int i = 0; i < neuronLabelList.Count && i < 100; i++)
            {
                string shortLabel = neuronLabelList[i];
                if (shortLabel.Length > 20) shortLabel = shortLabel.Substring(0, 20);
                shortLabel += " (" + neuronIdList[i] + ")";
                neuronLabelList[i] = shortLabel;
            }
            neuronLabelList.Sort();
            if (neuronLabelList.Count > 100)
                neuronLabelList.RemoveRange(100, neuronLabelList.Count - 100);
            NeuronMenu.IsEnabled = (neuronLabelList.Count == 0) ? false : true;
            foreach (string s in neuronLabelList)
            {
                MenuItem mi = new MenuItem { Header = s };
                mi.Click += NeuronMenu_Click;
                NeuronMenu.Items.Add(mi);
            }


            ModuleMenu.Items.Clear();
            List<string> moduleLabelList = new List<string>();
            for (int i = 0; i < 此神经元数组.Modules.Count; i++)
            {
                模块视图 theModule = 此神经元数组.Modules[i];
                string shortLabel = theModule.Label;
                if (shortLabel.Length > 10) shortLabel = shortLabel.Substring(0, 10);
                shortLabel += " (" + theModule.FirstNeuron + ")";
                moduleLabelList.Add(shortLabel);
            }
            moduleLabelList.Sort();
            ModuleMenu.IsEnabled = (moduleLabelList.Count == 0) ? false : true;
            foreach (string s in moduleLabelList)
            {
                MenuItem mi = new MenuItem { Header = s };
                mi.Click += NeuronMenu_Click;
                ModuleMenu.Items.Add(mi);
            }
        }

        private void 设置KB状态()
        {
            string kbString = "";
            if (ctrlPressed) kbString += "CTRL ";
            if (shiftPressed) kbString += "SHFT ";
            KBStatus.Text = kbString;
        }

        private void 设置标题栏()
        {
            Title = "Brain Simulator II " + System.IO.Path.GetFileNameWithoutExtension(currentFileName);
        }

        /// <summary>
        /// 在主窗口底部的状态栏中设置 a 字段
        /// </summary>
        /// <param name="field">0-4 设置选择要更新的字段</param> 
        /// <param name="message"></param>
        /// <param name="severity">0-2 = 绿色、黄色、红色</param> 
        public void 设置窗口底部状态(int field, string message, int severity = 0)
        {
            TextBlock tb = null;
            switch (field)
            {
                case 0: tb = statusField0; break;
                case 1: tb = statusField1; break;
                case 2: tb = statusField2; break;
                case 3: tb = statusField3; break;
                case 4: tb = statusField4; break;
            }
            SolidColorBrush theColor = null;
            switch (severity)
            {
                case 0: theColor = new SolidColorBrush(Colors.LightGreen); break;
                case 1: theColor = new SolidColorBrush(Colors.Yellow); break;
                case 2: theColor = new SolidColorBrush(Colors.Pink); break;
            }
            tb.Background = theColor;
            tb.Text = message;
        }
        /// <summary>
        /// 非常酷的进度条，可以随时显示
        /// </summary>
        /// <param name="value">0-100 完成百分比 0 初始化，100 关闭</param> 
        /// <param name="label"></param>
        /// <returns>如果按下取消按钮，则为 true</returns>  
        /// 
        float prevValue = 0;
        public bool 设置程序(float value, string label)
        {
            if (value != 0 && value < 100 && Math.Abs(prevValue - value) < 0.1)
            {
                return false;
            }
            bool retVal = false;
            prevValue = value;
            if (Application.Current.Dispatcher.CheckAccess())
            {
                retVal = progressDialog.设置程序(value, label);
                AllowUIToUpdate();
            }
            else
            {
                Application.Current.Dispatcher.Invoke((Action)delegate
                {
                    retVal = progressDialog.设置程序(value, label);
                });
            }
            return retVal;
        }

        //这个小助手让进度条在 UI 线程中的长时间操作中更新
        void AllowUIToUpdate()
        {
            DispatcherFrame frame = new DispatcherFrame();
            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Render, new DispatcherOperationCallback(delegate (object parameter)
            {
                frame.Continue = false;
                return null;
            }), null);

            Dispatcher.PushFrame(frame);
            //EDIT:
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background,
                                          new Action(delegate { }));
        }
        public static bool Busy()
        {
            if (thisWindow == null) return true;
            if (thisWindow.progressDialog == null) return true;
            if (thisWindow.progressDialog.Visibility == Visibility.Visible) return true;
            return false;
        }

        //Enable/disable menu item specified by "Entry"...pass in the Menu.Items as the root to search
        private void EnableMenuItem(ItemCollection mm, string Entry, bool enabled)
        {
            foreach (Object m1 in mm)
            {
                if (m1.GetType() == typeof(MenuItem))
                {
                    MenuItem m = (MenuItem)m1;
                    if (m.Header.ToString() == Entry)
                    {
                        m.IsEnabled = enabled;
                        return;
                    }
                    else
                        EnableMenuItem(m.Items, Entry, enabled);
                }
            }

            return;
        }

        //this enables and disables various menu entries based on circumstances
        private void MainMenu_MouseEnter(object sender, MouseEventArgs e)
        {
            LoadMRUMenu();
            LoadFindMenus();

            if (此神经元数组 != null && !useServers) ThreadCount.Text = 此神经元数组.GetThreadCount().ToString();
            else ThreadCount.Text = "";
            if (此神经元数组 != null) Refractory.Text = 此神经元数组.GetRefractoryDelay().ToString();
            else Refractory.Text = "";

            if (currentFileName != "" &&
                xml文件.CanWriteTo(currentFileName, out string message)
                && 此神经元数组 != null)
            {
                EnableMenuItem(MainMenu.Items, "_保存", true);
                SaveButton.IsEnabled = true;
            }
            else
            {
                EnableMenuItem(MainMenu.Items, "_保存", false);
                SaveButton.IsEnabled = false;
            }
            if (!引擎是否暂停)
            {
                EnableMenuItem(MainMenu.Items, "运行", false);
                EnableMenuItem(MainMenu.Items, "暂停", true);
            }
            else
            {
                EnableMenuItem(MainMenu.Items, "运行", true);
                EnableMenuItem(MainMenu.Items, "暂停", false);
            }
            if (useServers)
            {
                var tb0 = 跨语言接口.FindByName(MainMenu, "ThreadCount");
                if (tb0.Parent is UIElement ui)
                    ui.Visibility = Visibility.Collapsed;
                tb0 = 跨语言接口.FindByName(MainMenu, "Refractory");
                if (tb0.Parent is UIElement ui1)
                    ui1.Visibility = Visibility.Collapsed;
            }
            else
            {
                var tb0 = 跨语言接口.FindByName(MainMenu, "ThreadCount");
                if (tb0.Parent is UIElement ui)
                    ui.Visibility = Visibility.Visible;
                tb0 = 跨语言接口.FindByName(MainMenu, "Refractory");
                if (tb0.Parent is UIElement ui1)
                    ui1.Visibility = Visibility.Visible;
            }
            if (数组是否为空())
            {
                EnableMenuItem(MainMenu.Items, "_保存", false);
                EnableMenuItem(MainMenu.Items, "另存为", false);
                EnableMenuItem(MainMenu.Items, "_Insert", false);
                EnableMenuItem(MainMenu.Items, "_Properties", false);
                EnableMenuItem(MainMenu.Items, "_Notes", false);
            }
            else
            {
                EnableMenuItem(MainMenu.Items, "另存为", true);
                EnableMenuItem(MainMenu.Items, "_Insert", true);
                EnableMenuItem(MainMenu.Items, "_Properties", true);
                EnableMenuItem(MainMenu.Items, "_Notes", true);
                MenuItem mi = (MenuItem)跨语言接口.FindByName(MainMenu, "展示突触");
                if (mi != null)
                    mi.IsChecked = 此神经元数组.ShowSynapses;
            }
            if (此神经元数组视图.theSelection.selectedRectangles.Count == 0)
            {
                EnableMenuItem(MainMenu.Items, " 剪切", false);
                EnableMenuItem(MainMenu.Items, " 复制", false);
                EnableMenuItem(MainMenu.Items, " 删除", false);
                EnableMenuItem(MainMenu.Items, " 移动", false);
                EnableMenuItem(MainMenu.Items, " 清除选择", false);
            }
            else
            {
                EnableMenuItem(MainMenu.Items, " 剪切", true);
                EnableMenuItem(MainMenu.Items, " 复制", true);
                EnableMenuItem(MainMenu.Items, " 删除", true);
                if (神经元数组视图.targetNeuronIndex < 0)
                    EnableMenuItem(MainMenu.Items, " 移动", false);
                else
                    EnableMenuItem(MainMenu.Items, " 移动", true);
                EnableMenuItem(MainMenu.Items, " 清除选择", true);
            }
            if (神经元数组视图.targetNeuronIndex < 0 || !xml文件.WindowsClipboardContainsNeuronArray())
            {
                EnableMenuItem(MainMenu.Items, " 粘贴", false);
            }
            else
            {
                EnableMenuItem(MainMenu.Items, " 粘贴", true);
            }
            if (此神经元数组 != null && 此神经元数组.获取撤销数量() > 0)
            {
                EnableMenuItem(MainMenu.Items, " 撤销", true);
            }
            else
            {
                EnableMenuItem(MainMenu.Items, " 撤销", false);

            }

            if (myClipBoard == null)
            {
                EnableMenuItem(MainMenu.Items, "保存剪贴板", false);
            }
            else
            {
                EnableMenuItem(MainMenu.Items, "保存剪贴板", true);
            }
        }

        static List<int> displayTimerMovingAverage;
        static public void UpdateDisplayLabel(float zoomLevel)
        {
            if (displayTimerMovingAverage == null)
            {
                displayTimerMovingAverage = new List<int>();
                for (int i = 0; i < 10; i++)
                {
                    displayTimerMovingAverage.Add(0);
                }
            }
            displayTimerMovingAverage.RemoveAt(0);
            displayTimerMovingAverage.Add((int)displayElapsed);
            string formatString = "N0";
            if (zoomLevel < 10) formatString = "N1";
            if (zoomLevel < 1) formatString = "N2";
            if (zoomLevel < .1f) formatString = "N3";
            string displayStatus = "缩放级别: " + zoomLevel.ToString(formatString) + ",  " + (displayTimerMovingAverage.Average() / 10000f).ToString("F2") + "ms";
            thisWindow.设置窗口底部状态(2, displayStatus, 0);
        }

        static bool fullUpdateNeeded = false;
        public static void Update()
        {
            if (thisWindow.IsEngineSuspended())
            {
                Application.Current.Dispatcher.Invoke((Action)delegate
                {
                    神经元数组视图.更新();
                });
            }
            else
            {
                fullUpdateNeeded = true;
            }
        }

        public static void 清除所有模型对话框()
        {
            if (此神经元数组 != null)
            {
                lock (此神经元数组.Modules)
                {
                    foreach (模块视图 na in 此神经元数组.Modules)
                    {
                        if (na.TheModule != null)
                        {
                            na.TheModule.CloseDlg();
                        }
                    }
                }
            }
        }

        public static bool 数组是否为空()
        {
            if (MainWindow.此神经元数组 == null) return true;
            if (MainWindow.此神经元数组.数组大小 == 0) return true;
            if (MainWindow.此神经元数组.行数 == 0) return true;
            if (MainWindow.此神经元数组.Cols == 0) return true;
            if (!MainWindow.此神经元数组.加载完成) return true;
            return false;
        }

        public void 创建空网络()
        {
            此神经元数组 = new 神经元数组();
            神经元数组视图.Dp.神经元图示大小 = 62;
            神经元数组视图.Dp.DisplayOffset = new Point(0, 0);
            此神经元数组.初始化(450, 15);
            此神经元数组.加载完成 = true;
            Update();
        }

        public void 更新空内存显示()
        {
            ulong availablePhysicalMemory = new Microsoft.VisualBasic.Devices.ComputerInfo().AvailablePhysicalMemory;
            MainWindow.thisWindow.设置窗口底部状态(4, "有效内存: " + availablePhysicalMemory.ToString("##,#"), 0);
        }

    }
}
