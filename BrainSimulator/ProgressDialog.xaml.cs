using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace BrainSimulator
{
    /// <summary>
    /// Interaction logic for ProgressDialog.xaml
    /// </summary>
    public partial class ProgressDialog : Window
    {
        bool cancelPressed = false;
        DateTime 开始时间;
        DateTime 剩余时间;

        public ProgressDialog()
        {
            InitializeComponent();
            开始时间 = DateTime.Now;
            timeLabel.Content = "";
            Owner = MainWindow.thisWindow;
        }

        public bool 设置程序(float value, string label)
        {
            if (value == 100)
            {
                MainWindow.thisWindow.MainMenu.IsEnabled = true;
                MainWindow.thisWindow.MainToolBar.IsEnabled = true;
                cancelPressed = true;
                CancelProgressBar();
            }
            else if (value == 0)
            {
                cancelPressed = false;
                if (label != "")
                    theLabel.Text = label;
                this.Visibility = Visibility.Visible;

                开始时间 = DateTime.Now;
                MainWindow.thisWindow.MainMenu.IsEnabled = false;
                MainWindow.thisWindow.MainToolBar.IsEnabled = false;
                theProgressBar.Value = 0;
                cancelPressed = false;
                timeLabel.Content = "计算预计持续时间...";
            }
            ProcessProgress(value);
            return cancelPressed;
        }

        private bool ProcessProgress(float value)
        {
            // value is range 0 to 100, we can calculate the total time from time spent till now...
            DateTime currentTime = DateTime.Now;
            TimeSpan elapsedTime = currentTime - 开始时间;
            if (value == 0.0) value = 0.000001F;   // avoid infinity factor for very large tasks...
            float factor = (100 - value);
            TimeSpan remainingTime = TimeSpan.FromTicks((long)(elapsedTime.Ticks * factor));

            if (value < 0.1)
            {
                timeLabel.Content = "等待第一次进度更新...";
            }
            else if(remainingTime.TotalSeconds > 0 && elapsedTime.TotalSeconds + remainingTime.TotalSeconds < 120)
            {
                // recompute for seconds left rather than ETA...
                factor = (100 - value) / 100;
                remainingTime = TimeSpan.FromTicks((long)(elapsedTime.Ticks * factor));
                timeLabel.Content = (int)remainingTime.TotalSeconds + " 剩余秒数";
            }
            else
            {
                // recompute for ETA rather than time left.
                剩余时间 = DateTime.Now;
                剩余时间 = 剩余时间.AddSeconds(remainingTime.TotalSeconds);
                if (开始时间.Date == 剩余时间.Date)
                {
                    timeLabel.Content = "已用时间: " + string.Concat(elapsedTime.ToString()).Substring(0, 8) +
                                        "\n约 完成时间: " + string.Concat(剩余时间.ToShortTimeString());
                }
                else
                {
                    timeLabel.Content = "已用时间: " + string.Concat(elapsedTime.ToString()).Substring(0, 8) +
                                        "\n约 完成时间: " + string.Concat(剩余时间);
                }
            }
            theProgressBar.Value = value;
            MainWindow.thisWindow.更新空内存显示();
            return cancelPressed;
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            cancelPressed = true;
            开始时间 = DateTime.Now;
            MainWindow.thisWindow.MainMenu.IsEnabled = true;
            MainWindow.thisWindow.MainToolBar.IsEnabled = true;
            CancelProgressBar();
        }

        public void CancelProgressBar()
        {
            this.Hide();
            this.Visibility = Visibility.Collapsed;
        }
    }

}
