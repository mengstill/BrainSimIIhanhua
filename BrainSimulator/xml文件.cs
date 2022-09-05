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
            MRUList.Remove(filePath); //remove it if it's already there
            Properties.Settings.Default["MRUList"] = MRUList;
            Properties.Settings.Default.Save();
        }

        //this checks to see if the Windows Clipboard contains neurons and loads them if it does
        public static bool WindowsClipboardContainsNeuronArray()
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
                        Load(ref MainWindow.myClipBoard, "ClipBoard");
                    }
                }
            }
            catch
            {
                retVal = false;
            }
            return retVal;
        }

        public static bool Load(ref 神经元数组 theNeuronArray, string fileName)
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
                    MessageBox.Show("Could not open file because: " + e.Message);
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
                    MessageBox.Show("File is not a valid Brain Simulator II XML file.");
                    return false;
                }

                MainWindow.thisWindow.设置程序(0, "Loading Network File");
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
            theNeuronArray = new 神经元数组();

            XmlSerializer reader1 = new XmlSerializer(typeof(神经元数组), 获取模型类型数组());
            try
            {
                theNeuronArray = (神经元数组)reader1.Deserialize(file);
            }
            catch (Exception e)
            {
                file.Close();
                MessageBox.Show("Network file load failed, a blank network will be opened. \r\n\r\n"+e.InnerException,"File Load Error",
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

            int arraySize = theNeuronArray.数组大小;
            theNeuronArray.初始化(arraySize, theNeuronArray.行数);
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
                if (idNodes.Count > 0)
                    int.TryParse(idNodes[0].InnerText, out id);
                if (id == -1) continue;

                神经元 n = theNeuronArray.获取神经元(id);
                n.Owner = theNeuronArray;
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
                            n.模型 = theModel;
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
                            if (n.模型 != 神经元.模型类型.Color)
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
                            n.ShowSynapses = showSynapses;
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
                                            s.targetNeuron = target;
                                            break;
                                        case "Weight":
                                            float.TryParse(synapseAttribNode.InnerText, out float weight);
                                            s.weight = weight;
                                            break;
                                        case "IsHebbian": //Obsolete: backwards compatibility
                                            bool.TryParse(synapseAttribNode.InnerText, out bool isheb);
                                            if (isheb) s.model = 突触.modelType.Hebbian1;
                                            else s.model = 突触.modelType.Fixed;
                                            break;
                                        case "Model":
                                            Enum.TryParse(synapseAttribNode.InnerText, out 突触.modelType model);
                                            s.model = model;
                                            break;
                                    }
                                }
                                n.添加突触(s.targetNeuron, s.weight, s.model);
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

        public static bool CanWriteTo(string fileName)
        {
            return CanWriteTo(fileName, out _);
        }
        public static bool CanWriteTo(string fileName, out string message)
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

        //if you pass in the fileName 'ClipBoard', the save is to the windows clipboard
        public static bool Save(神经元数组 theNeuronArray, string fileName)
        {
            Stream file;
            string tempFile = "";
            bool fromClipboard = fileName == "ClipBoard";
            if (!fromClipboard)
            {
                //Check for file access
                if (!CanWriteTo(fileName, out string message))
                {
                    MessageBox.Show("Could not save file because: " + message);
                    return false;
                }

                MainWindow.thisWindow.设置程序(0, "Saving Network File");

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
                XmlSerializer writer = new XmlSerializer(typeof(神经元数组), extraTypes);
                writer.Serialize(file, theNeuronArray);
            }
            catch (Exception e)
            {
                if (e.InnerException != null)
                    MessageBox.Show("Xml file write failed because: " + e.InnerException.Message);
                else
                    MessageBox.Show("Xml file write failed because: " + e.Message);
                MainWindow.thisWindow.设置程序(100,"");
                return false;
            }
            file.Position = 0; ;

            XmlDocument xmldoc = new XmlDocument();
            xmldoc.Load(file);

            XmlElement root = xmldoc.DocumentElement;
            XmlNode neuronsNode = xmldoc.CreateNode("element", "Neurons", "");
            root.AppendChild(neuronsNode);

            for (int i = 0; i < theNeuronArray.数组大小; i++)
            {
                if (!fromClipboard)
                {
                    var progress = i / (float)theNeuronArray.数组大小;
                    progress *= 100;
                    if (MainWindow.thisWindow.设置程序(progress, ""))
                    {
                        MainWindow.thisWindow.设置程序(100, "");
                        return false;
                    }
                }
                神经元 n = theNeuronArray.GetNeuronForDrawing(i);
                if (fromClipboard) n.Owner = theNeuronArray;
                if (n.是否使用 || n.标签名 != "" || fromClipboard)
                {
                    n = theNeuronArray.GetCompleteNeuron(i, fromClipboard);
                    n.Owner = theNeuronArray;
                    string label = n.标签名;
                    if (n.ToolTip != "") label += 神经元.toolTipSeparator + n.ToolTip;
                    //this is needed bacause inUse is true if any synapse points to this neuron--
                    //we don't need to bother with that if it's the only thing 
                    if (n.突触列表.Count != 0 || label != "" || n.最后更改 != 0 || n.leakRate泄露速度 != 0.1f
                        || n.模型 != 神经元.模型类型.IF)
                    {
                        XmlNode neuronNode = xmldoc.CreateNode("element", "Neuron", "");
                        neuronsNode.AppendChild(neuronNode);

                        XmlNode attrNode = xmldoc.CreateNode("element", "Id", "");
                        attrNode.InnerText = n.id.ToString();
                        neuronNode.AppendChild(attrNode);

                        if (n.模型 != 神经元.模型类型.IF)
                        {
                            attrNode = xmldoc.CreateNode("element", "Model", "");
                            attrNode.InnerText = n.模型.ToString();
                            neuronNode.AppendChild(attrNode);
                        }
                        if (n.模型 != 神经元.模型类型.Color && n.最后更改 != 0)
                        {
                            attrNode = xmldoc.CreateNode("element", "LastCharge", "");
                            attrNode.InnerText = n.最后更改.ToString();
                            neuronNode.AppendChild(attrNode);
                        }
                        if (n.模型 == 神经元.模型类型.Color && n.LastChargeInt != 0)
                        {
                            attrNode = xmldoc.CreateNode("element", "LastCharge", "");
                            attrNode.InnerText = n.LastChargeInt.ToString();
                            neuronNode.AppendChild(attrNode);
                        }
                        if (n.leakRate泄露速度 != 0.1f)
                        {
                            attrNode = xmldoc.CreateNode("element", "LeakRate", "");
                            attrNode.InnerText = n.leakRate泄露速度.ToString();
                            neuronNode.AppendChild(attrNode);
                        }
                        if (n.突触延迟 != 0)
                        {
                            attrNode = xmldoc.CreateNode("element", "AxonDelay", "");
                            attrNode.InnerText = n.突触延迟.ToString();
                            neuronNode.AppendChild(attrNode);
                        }
                        if (label != "")
                        {
                            attrNode = xmldoc.CreateNode("element", "Label", "");
                            attrNode.InnerText = label;
                            neuronNode.AppendChild(attrNode);
                        }
                        if (n.ShowSynapses)
                        {
                            attrNode = xmldoc.CreateNode("element", "ShowSynapses", "");
                            attrNode.InnerText = "True";
                            neuronNode.AppendChild(attrNode);
                        }
                        if (n.RecordHistory && !fromClipboard)
                        {
                            attrNode = xmldoc.CreateNode("element", "RecordHistory", "");
                            attrNode.InnerText = "True";
                            neuronNode.AppendChild(attrNode);
                        }
                        if (n.突触列表.Count > 0)
                        {
                            XmlNode synapsesNode = xmldoc.CreateNode("element", "Synapses", "");
                            neuronNode.AppendChild(synapsesNode);
                            foreach (突触 s in n.突触列表)
                            {
                                XmlNode synapseNode = xmldoc.CreateNode("element", "Synapse", "");
                                synapsesNode.AppendChild(synapseNode);

                                if (s.weight != 1)
                                {
                                    attrNode = xmldoc.CreateNode("element", "Weight", "");
                                    attrNode.InnerText = s.weight.ToString();
                                    synapseNode.AppendChild(attrNode);
                                }

                                attrNode = xmldoc.CreateNode("element", "TargetNeuron", "");
                                attrNode.InnerText = s.targetNeuron.ToString();
                                synapseNode.AppendChild(attrNode);

                                if (s.model != 突触.modelType.Fixed)
                                {
                                    attrNode = xmldoc.CreateNode("element", "Model", "");
                                    attrNode.InnerText = s.model.ToString();
                                    synapseNode.AppendChild(attrNode);
                                }
                            }
                        }
                    }
                }
            }

            file.Position = 0;
            xmldoc.Save(file);
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
                    MessageBox.Show("Could not save file because: " + e.Message);
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
