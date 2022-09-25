using BrainSimulator.Modules;
using System;
using System.Text;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Windows;
using System.Xml;
using System.Xml.Serialization;

namespace BrainSimulator
{
    public class xml文件
    {
        //this is the set of moduletypes that the xml serializer will save
        //这是 xml 序列化程序将保存的模块类型集
        static private Type[] 获取模型类型数组()
        {
            Type[] listOfBs = (from domainAssembly in AppDomain.CurrentDomain.GetAssemblies()
                               from assemblyType in domainAssembly.GetTypes()
                               where assemblyType.IsSubclassOf(typeof(ModuleBase))
                               //                               where typeof(ModuleBase).IsAssignableFrom(assemblyType)
                               select assemblyType).ToArray();
            List<Type> list = new List<Type>();
            for (int i = 0; i < listOfBs.Length; i++)
                list.Add(listOfBs[i]);
            list.Add(typeof(PointPlus));
            list.Add(typeof(显示参数));
            list.Add(typeof(HSLColor));
            return list.ToArray();
        }

        public static void RemoveFileFromMRUList(string filePath)
        {
            StringCollection MRUList = (StringCollection)Properties.Settings.Default["MRUList"];
            if (MRUList == null)
                MRUList = new StringCollection();
            MRUList.Remove(filePath); //remove it if it's already there如果已经存在，请将其删除
            Properties.Settings.Default["MRUList"] = MRUList;
            Properties.Settings.Default.Save();
        }

        /// <summary>
        /// this checks to see if the Windows Clipboard contains neurons and loads them if it does
        /// 这将检查Windows剪贴板是否包含神经元，如果包含，则加载神经元
        /// </summary>
        /// <returns></returns>
        public static bool Windows剪贴板是否包含神经元数组()
        {
            bool retVal = false;
            try
            {
                if (Clipboard.ContainsText())
                {
                    string content = Clipboard.GetText();
                    if (content.Contains("<NeuronArray"))
                    {
                        retVal = true;
                        加载神经元数组(ref MainWindow.myClipBoard, "ClipBoard");
                    }
                }
            }
            catch
            {
                retVal = false;
            }
            return retVal;
        }

        public static bool 加载神经元数组(ref NeuronArray theNeuronArray, string fileName)
        {
            bool fromClipboard = fileName == "ClipBoard";
            Stream file;
            if (!fromClipboard)
            {
                try
                {
                    file = File.Open(fileName, FileMode.Open, FileAccess.Read);
                }
                catch (Exception e)
                {
                    MessageBox.Show("无法打开文件，因为: " + e.Message);
                    RemoveFileFromMRUList(fileName);
                    return false;
                }

                // first check if the required start tag is present in the file...
                byte[] buffer = new byte[60];
                file.Read(buffer, 0, 60);
                string line = Encoding.UTF8.GetString(buffer, 0, buffer.Length);
                if (line.Contains("NeuronArray"))
                {
                    file.Seek(0, SeekOrigin.Begin);
                }
                else
                {
                    file.Close();
                    MessageBox.Show("文件不是有效的 Brain Simulator II XML 文件.");
                    return false;
                }

                MainWindow.thisWindow.设置程序(0, "加载网络文件");
            }
            else
            {
                file = new MemoryStream();
                StreamWriter sw = new StreamWriter(file);
                string temp = Clipboard.GetText();
                sw.Write(temp);
                sw.Flush();
                file.Seek(0, SeekOrigin.Begin);
            }
            theNeuronArray = new NeuronArray();

            XmlSerializer reader1 = new XmlSerializer(typeof(NeuronArray), 获取模型类型数组());
            try
            {
                theNeuronArray = (NeuronArray)reader1.Deserialize(file);
            }
            catch (Exception e)
            {
                file.Close();
                MessageBox.Show("网络文件加载失败，将打开一个空白网络. \r\n\r\n" + e.InnerException, "文件加载错误",
                    MessageBoxButton.OK,MessageBoxImage.Error,MessageBoxResult.OK,MessageBoxOptions.DefaultDesktopOnly);
                MainWindow.thisWindow.设置程序(100,"");
                return false;
            }

            file.Position = 0;
            //the above automatically loads the content of the neuronArray object but can't load the neurons themselves
            //because of formatting changes
            XmlDocument xmldoc = new XmlDocument();
            XmlNodeList neuronNodes;
            xmldoc.Load(file);
            file.Close();

            int arraySize = theNeuronArray.arraySize;
            theNeuronArray.初始化(arraySize, theNeuronArray.rows);
            neuronNodes = xmldoc.GetElementsByTagName("Neuron");

            for (int i = 0; i < neuronNodes.Count; i++)
            {
                if (!fromClipboard)
                {
                    var progress = i / (float)neuronNodes.Count;
                    progress *= 100;
                    if (progress != 0 && MainWindow.thisWindow.设置程序(progress, ""))
                    {
                        MainWindow.thisWindow.设置程序(100, "");
                        return false;
                    }
                }
                XmlElement neuronNode = (XmlElement)neuronNodes[i];
                XmlNodeList idNodes = neuronNode.GetElementsByTagName("Id");
                int id = i; //this is a hack to read obsolete files where all neurons were included but no Id's
                            //这是一种破解方法，可以读取包含所有神经元但没有Id的过时文件
                if (idNodes.Count > 0)
                    int.TryParse(idNodes[0].InnerText, out id);
                if (id == -1) continue;

                神经元 n = theNeuronArray.获取神经元(id);
                n.所有者 = theNeuronArray;
                n.id = id;

                foreach (XmlElement node in neuronNode.ChildNodes)
                {
                    string name = node.Name;
                    switch (name)
                    {
                        case "Label":
                            n.标签名 = node.InnerText;
                            break;
                        case "Model":
                            Enum.TryParse(node.InnerText, out 神经元.模型类型 theModel);
                            n.模型字段 = theModel;
                            break;
                        case "LeakRate":
                            float.TryParse(node.InnerText, out float leakRate);
                            n.leakRate泄露速度 = leakRate;
                            break;
                        case "AxonDelay":
                            int.TryParse(node.InnerText, out int axonDelay);
                            n.突触延迟 = axonDelay;
                            break;
                        case "LastCharge":
                            if (n.模型字段 != 神经元.模型类型.Color)
                            {
                                float.TryParse(node.InnerText, out float lastCharge);
                                n.LastCharge = lastCharge;
                                n.当前更改 = lastCharge;
                            }
                            else //is color
                            {
                                int.TryParse(node.InnerText, out int lastChargeInt);
                                n.LastChargeInt = lastChargeInt;
                                n.当前更改 = lastChargeInt; //current charge is not used on color neurons
                            }
                            break;
                        case "ShowSynapses":
                            bool.TryParse(node.InnerText, out bool showSynapses);
                            n.显示突触 = showSynapses;
                            break;
                        case "RecordHistory":
                            bool.TryParse(node.InnerText, out bool recordHistory);
                            n.RecordHistory = recordHistory;
                            break;
                        case "Synapses":
                            theNeuronArray.设置所有神经元(n);
                            XmlNodeList synapseNodess = node.GetElementsByTagName("Synapse");
                            foreach (XmlNode synapseNode in synapseNodess)
                            {
                                突触 s = new 突触();
                                foreach (XmlNode synapseAttribNode in synapseNode.ChildNodes)
                                {
                                    string name1 = synapseAttribNode.Name;
                                    switch (name1)
                                    {
                                        case "TargetNeuron":
                                            int.TryParse(synapseAttribNode.InnerText, out int target);
                                            s.目标神经元字段 = target;
                                            break;
                                        case "Weight":
                                            float.TryParse(synapseAttribNode.InnerText, out float weight);
                                            s.权重字段 = weight;
                                            break;
                                        case "IsHebbian": //Obsolete: backwards compatibility
                                            bool.TryParse(synapseAttribNode.InnerText, out bool isheb);
                                            if (isheb) s.模型字段 = 突触.modelType.Hebbian1;
                                            else s.模型字段 = 突触.modelType.Fixed;
                                            break;
                                        case "Model":
                                            Enum.TryParse(synapseAttribNode.InnerText, out 突触.modelType model);
                                            s.模型字段 = model;
                                            break;
                                    }
                                }
                                n.添加突触(s.目标神经元字段, s.权重字段, s.模型字段);
                            }
                            break;
                    }
                }
                theNeuronArray.设置所有神经元(n);
            }
            if (!fromClipboard)
                MainWindow.thisWindow.设置程序(100, "");
            return true;
        }

        public static bool 能否写入(string fileName)
        {
            return 能否写入(fileName, out _);
        }
        public static bool 能否写入(string fileName, out string message)
        {
            FileStream file1;
            message = "";
            if (File.Exists(fileName))
            {
                try
                {
                    file1 = File.Open(fileName, FileMode.Open);
                    file1.Close();
                    return true;
                }
                catch (Exception e)
                {
                    message = e.Message;
                    return false;
                }
            }
            return true;

        }

        /// <summary>
        /// if you pass in the fileName 'ClipBoard', the save is to the windows clipboard
        /// 如果传入文件名“剪贴板”，则保存到windows剪贴板
        /// </summary>
        /// <param name="此神经元列表"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static bool 保存(NeuronArray 此神经元列表, string fileName)
        {
            Stream file;
            string tempFile = "";
            bool fromClipboard = fileName == "ClipBoard";
            if (!fromClipboard)
            {
                //Check for file access
                if (!能否写入(fileName, out string message))
                {
                    MessageBox.Show("无法保存文件，因为： " + message);
                    return false;
                }

                MainWindow.thisWindow.设置程序(0, "保存网络文件");

                tempFile = System.IO.Path.GetTempFileName();
                file = File.Create(tempFile);
            }
            else
            {
                file = new MemoryStream();
            }
            Type[] extraTypes = 获取模型类型数组();
            try
            {
                //想要更改为json文档,就要在这里入手了,首先将神经元变成相应的参数,然后都塞入
                //神经元数组中的参数数组,最后在序列化类,最后保存即可
                XmlSerializer writer = new XmlSerializer(typeof(NeuronArray), extraTypes);
                writer.Serialize(file, 此神经元列表);
            }
            catch (Exception e)
            {
                if (e.InnerException != null)
                    MessageBox.Show("Xml文件写入失败，因为: " + e.InnerException.Message);
                else
                    MessageBox.Show("Xml文件写入失败，因为: " + e.Message);
                MainWindow.thisWindow.设置程序(100,"");
                return false;
            }
            file.Position = 0; 

            XmlDocument xml文档 = new XmlDocument();
            xml文档.Load(file);

            XmlElement root = xml文档.DocumentElement;
            XmlNode neuronsNode = xml文档.CreateNode("element", "Neurons", "");
            root.AppendChild(neuronsNode);

            for (int i = 0; i < 此神经元列表.arraySize; i++)
            {
                if (!fromClipboard)
                {
                    var progress = i / (float)此神经元列表.arraySize;
                    progress *= 100;
                    if (MainWindow.thisWindow.设置程序(progress, ""))
                    {
                        MainWindow.thisWindow.设置程序(100, "");
                        return false;
                    }
                }
                神经元 n = 此神经元列表.获取用于绘图的神经元(i);
                if (fromClipboard) n.所有者 = 此神经元列表;
                if (n.是否使用 || n.标签名 != "" || fromClipboard)
                {
                    n = 此神经元列表.获取完整的神经元(i, fromClipboard);
                    n.所有者 = 此神经元列表;
                    string label = n.标签名;
                    if (n.ToolTip != "") label += 神经元.toolTipSeparator + n.ToolTip;
                    //this is needed bacause inUse is true if any synapse points to this neuron--
                    //we don't need to bother with that if it's the only thing 
                    if (n.synapses.Count != 0 || label != "" || n.最后更改 != 0 || n.leakRate泄露速度 != 0.1f
                        || n.模型字段 != 神经元.模型类型.IF)
                    {
                        XmlNode neuronNode = xml文档.CreateNode("element", "Neuron", "");
                        neuronsNode.AppendChild(neuronNode);

                        XmlNode attrNode = xml文档.CreateNode("element", "Id", "");
                        attrNode.InnerText = n.id.ToString();
                        neuronNode.AppendChild(attrNode);

                        if (n.模型字段 != 神经元.模型类型.IF)
                        {
                            attrNode = xml文档.CreateNode("element", "Model", "");
                            attrNode.InnerText = n.模型字段.ToString();
                            neuronNode.AppendChild(attrNode);
                        }
                        if (n.模型字段 != 神经元.模型类型.Color && n.最后更改 != 0)
                        {
                            attrNode = xml文档.CreateNode("element", "LastCharge", "");
                            attrNode.InnerText = n.最后更改.ToString();
                            neuronNode.AppendChild(attrNode);
                        }
                        if (n.模型字段 == 神经元.模型类型.Color && n.LastChargeInt != 0)
                        {
                            attrNode = xml文档.CreateNode("element", "LastCharge", "");
                            attrNode.InnerText = n.LastChargeInt.ToString();
                            neuronNode.AppendChild(attrNode);
                        }
                        if (n.leakRate泄露速度 != 0.1f)
                        {
                            attrNode = xml文档.CreateNode("element", "LeakRate", "");
                            attrNode.InnerText = n.leakRate泄露速度.ToString();
                            neuronNode.AppendChild(attrNode);
                        }
                        if (n.突触延迟 != 0)
                        {
                            attrNode = xml文档.CreateNode("element", "AxonDelay", "");
                            attrNode.InnerText = n.突触延迟.ToString();
                            neuronNode.AppendChild(attrNode);
                        }
                        if (label != "")
                        {
                            attrNode = xml文档.CreateNode("element", "Label", "");
                            attrNode.InnerText = label;
                            neuronNode.AppendChild(attrNode);
                        }
                        if (n.显示突触)
                        {
                            attrNode = xml文档.CreateNode("element", "ShowSynapses", "");
                            attrNode.InnerText = "True";
                            neuronNode.AppendChild(attrNode);
                        }
                        if (n.RecordHistory && !fromClipboard)
                        {
                            attrNode = xml文档.CreateNode("element", "RecordHistory", "");
                            attrNode.InnerText = "True";
                            neuronNode.AppendChild(attrNode);
                        }
                        if (n.synapses.Count > 0)
                        {
                            XmlNode synapsesNode = xml文档.CreateNode("element", "Synapses", "");
                            neuronNode.AppendChild(synapsesNode);
                            foreach (突触 s in n.synapses)
                            {
                                XmlNode synapseNode = xml文档.CreateNode("element", "Synapse", "");
                                synapsesNode.AppendChild(synapseNode);

                                if (s.权重字段 != 1)
                                {
                                    attrNode = xml文档.CreateNode("element", "Weight", "");
                                    attrNode.InnerText = s.权重字段.ToString();
                                    synapseNode.AppendChild(attrNode);
                                }

                                attrNode = xml文档.CreateNode("element", "TargetNeuron", "");
                                attrNode.InnerText = s.目标神经元字段.ToString();
                                synapseNode.AppendChild(attrNode);

                                if (s.模型字段 != 突触.modelType.Fixed)
                                {
                                    attrNode = xml文档.CreateNode("element", "Model", "");
                                    attrNode.InnerText = s.模型字段.ToString();
                                    synapseNode.AppendChild(attrNode);
                                }
                            }
                        }
                    }
                }
            }

            file.Position = 0;
            xml文档.Save(file);
            if (!fromClipboard)
            {
                file.Close();
                try
                {
                    File.Copy(tempFile, fileName, true);
                    File.Delete(tempFile);
                    MainWindow.thisWindow.设置程序(100, "");
                }
                catch (Exception e)
                {
                    MainWindow.thisWindow.设置程序(100, "");
                    MessageBox.Show("无法保存文件，因为： " + e.Message);
                    return false;
                }
            }
            else
            {
                file.Position = 0;
                StreamReader str = new StreamReader(file);
                string temp = str.ReadToEnd();
                Clipboard.SetText(temp);
            }

            return true;
        }

    }
}
