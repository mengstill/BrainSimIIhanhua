using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Serialization;

namespace BrainSimulator
{
    /// <summary>
    /// Interaction logic for ModuleDescription.xaml
    /// </summary>
    public partial class ModuleDescriptionDlg : Window
    {
        string moduleType = "";
        public ModuleDescriptionDlg(string theModuleType)
        {
            InitializeComponent();
            moduleType = theModuleType;
            string fileName = Path.GetFullPath(".").ToLower();
            if (fileName.Contains("program"))
            {
                buttonSave.IsEnabled = false;
            }
            var modules = 跨语言接口.GetArrayOfModuleTypes();

            foreach (var v in modules)
            {
                moduleSelector.Items.Add(v.Name.Replace("Module", ""));
            }
            moduleSelector.SelectedItem = theModuleType.Replace("Module", "");
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            模块描述文件.设置工具提示(moduleType, ToolTipText.Text);
            模块描述文件.设置描述(moduleType, Description.Text);
            模块描述文件.保存();
        }

        private void moduleSelector_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (sender is ComboBox cb)
            {
                moduleType = "Module" + cb.SelectedItem.ToString();
                ToolTipText.Text = 模块描述文件.获取工具提示(moduleType);
                Description.Text = 模块描述文件.获取描述(moduleType);
            }
        }
    }

    public class 模块描述文件
    {
        public class ModuleDescription
        {
            public string moduleName;
            public string toolTip;
            public string description;
        }
        public static List<ModuleDescription> theModuleDescriptions = null;

        public static string 获取工具提示(string moduleName)
        {
            if (theModuleDescriptions == null) 读取();
            //if (theModuleDescriptions.Count == 0) GetLegacyDescriptions();
            ModuleDescription desc = theModuleDescriptions.Find(t => t.moduleName == moduleName);
            if (desc != null) return desc.toolTip;
            return "";
        }

        //for backward compatibility;
        //private static void GetLegacyDescriptions()
        //{
        //    var modules = Utils.GetArrayOfModuleTypes();

        //    foreach (var v in modules)
        //    {
        //        //get the tooltip
        //        Type t = Type.GetType("BrainSimulator.Modules." + v.Name);
        //        Modules.ModuleBase aModule = (Modules.ModuleBase)Activator.CreateInstance(t);
        //        string toolTip = aModule.ShortDescription;
        //        string description = aModule.LongDescription;
        //        ModuleDescription desc = new ModuleDescription { moduleName = t.Name, description = description, toolTip = toolTip, };
        //        theModuleDescriptions.Add(desc);
        //    }
        //}

        public static void 设置工具提示(string moduleName, string theDescription)
        {
            ModuleDescription desc = theModuleDescriptions.Find(t => t.moduleName == moduleName);
            if (desc != null)
                desc.toolTip = theDescription;
            else
            {
                desc = new ModuleDescription { moduleName = moduleName, toolTip = theDescription, description = "", };
                theModuleDescriptions.Add(desc);
            }
        }
        public static string 获取描述(string moduleName)
        {
            if (theModuleDescriptions == null) 读取();
            ModuleDescription desc = theModuleDescriptions.Find(t => t.moduleName == moduleName);
            if (desc != null) return desc.description;
            return "";
        }
        public static void 设置描述(string moduleName, string theDescription)
        {
            ModuleDescription desc = theModuleDescriptions.Find(t => t.moduleName == moduleName);
            if (desc != null)
                desc.description = theDescription;
            else
            {
                desc = new ModuleDescription { moduleName = moduleName, description = theDescription, toolTip = "", };
                theModuleDescriptions.Add(desc);
            }
        }

        public static bool 读取()
        {
            Stream file;
            string location = AppDomain.CurrentDomain.BaseDirectory;
            location += "\\Networks\\ModuleDescriptions.xml";
            file = File.Open(location, FileMode.Open, FileAccess.Read);


            XmlSerializer reader = new XmlSerializer(typeof(List<ModuleDescription>));
            theModuleDescriptions = (List<ModuleDescription>)reader.Deserialize(file);
            //try
            //{
            //    XmlSerializer reader = new XmlSerializer(typeof(List<模块描述>));
            //    theModuleDescriptions = (List<模块描述>)reader.Deserialize(file);
            //}
            //catch (Exception e)
            //{
            //    MessageBox.Show("模块描述 XML 文件读取失败，因为: " + e.Message);
            //    return false;
            //}
            file.Close();
            return true;
        }

        public static bool 保存()
        {
            Stream file;
            string fileName = Path.GetFullPath(".").ToLower();
            //we're running with source...save to the source version
            if (fileName.Contains("\\bin"))
            {
                fileName = fileName.Replace("\\bin", "");
                fileName = fileName.Replace("\\release", "");
                fileName = fileName.Replace("\\x64", "");
                fileName = fileName.Replace("\\debug", "");
            }
            fileName += "\\Networks\\ModuleDescriptions.xml";
            try
            {
                file = File.Create(fileName);
                XmlSerializer writer = new XmlSerializer(typeof(List<ModuleDescription>));
                writer.Serialize(file, theModuleDescriptions);
            }
            catch (Exception e)
            {
                MessageBox.Show("模块描述 XML 文件写入失败，因为: " + e.Message);
                return false;
            }
            file.Position = 0; ;

            file.Close();

            return true;
        }


    }
}
