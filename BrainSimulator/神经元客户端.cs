using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BrainSimulator
{

    class 神经元客户端
    {

        static UdpClient serverClient = null; //listen only
        static UdpClient clientServer; //send/broadcast only
        static IPAddress broadCastAddress; 

        const int clientServerPort = 49002;
        const int serverClientPort = 49003;
        public static void Init()
        {
            if (serverClient != null) return; //already initialized

            //what is my ipaddress
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    byte[] ips = ip.GetAddressBytes();
                    broadCastAddress = IPAddress.Parse(ips[0] +"."+ ips[1] + "."+ips[2]+".255");
                }
            }

            serverClient = new UdpClient(serverClientPort);
            serverClient.Client.ReceiveBufferSize = 10000000;

            clientServer = new UdpClient();
            clientServer.EnableBroadcast = true;

            Task.Run(() =>
            {
                从服务器接收();
            });

        }

        public class Server
        {
            public string name;
            public IPAddress ipAddress;
            public int firstNeuron;
            public int lastNeuron;
            public bool busy = false;
            /// <summary>
            /// 多少代
            /// </summary>
            public long generation;
            public int firedCount;
            public long totalSynapses;
            public int neuronsInUse;
        }
        public static List<Server> 服务列表;
        public static void 获取服务列表()
        {
            服务列表 = new List<Server>();
            广播("GetServerInfo");
        }
        /// <summary>
        /// 获取系统精确时间作为文件时间
        /// </summary>
        /// <param name="filetime"></param>
        [DllImport("Kernel32.dll", CallingConvention = CallingConvention.Winapi)]
        private static extern void GetSystemTimePreciseAsFileTime(out long filetime);
        static long returnTime;
        public static long pingCount = 0;
        public static long Ping(IPAddress targetIp,string payload)
        {
            //run the test
            returnTime = 0;
            GetSystemTimePreciseAsFileTime(out long startTime);
            向服务器发送消息(targetIp,"Ping " + payload);
            while (returnTime == 0) Thread.Sleep(1);
            long elapsed = returnTime - startTime;
            return elapsed;
        }

        public static string 创建有效载荷(int payloadSize)
        {
            //create the payload
            string payload1 = "0123456789";
            string payload = "";
            for (int i = 0; i < (payloadSize + 1) / 10; i++) //fill the dummy, then truncate to desired length
                payload += payload1;
            payload = payload.Substring(0, payloadSize);
            return payload;
        }

        public static void 初始化服务器(int synapsesPerNeuron,int arraySize)
        {
            string message = "InitServers "+synapsesPerNeuron + " " + arraySize + " ";
            for (int i = 0; i < 服务列表.Count; i++)
            {
                message += 服务列表[i].ipAddress + " " + 服务列表[i].firstNeuron + " " + 服务列表[i].lastNeuron + " ";
            }
            广播(message);
            在所有服务器上等待完成();
        }
        public static void 标记服务器忙()
        {
            for (int i = 0; i < 服务列表.Count; i++)
                服务列表[i].busy = true;
        }
        public static void Fire()
        {
            标记服务器忙();
            广播("Fire");
            在所有服务器上等待完成();
            标记服务器忙();
            广播("Transfer");
            在所有服务器上等待完成();
        }
        static 神经元 tempNeuron = null;
        public static 神经元 获取神经元(int id)
        {
            tempNeuron = null;
            广播("GetNeuron " + id);
            while (tempNeuron == null) 
                Thread.Sleep(1);
            return tempNeuron;

        }
        static List<神经元> tempNeurons = null;
        public static List<神经元> 获取神经元(int id, int count)
        {
            tempNeurons = new List<神经元>();
            int start = id;
            int remaining = count;

            int i;
            for (i = 0; i < 服务列表.Count; i++)
                if (start >= 服务列表[i].firstNeuron && start < 服务列表[i].lastNeuron) break;
            int accumNeurons = 0;
            while (i < 服务列表.Count && start + count > 服务列表[i].lastNeuron) //handle split across multiple servers
            {
                int firstCount = 服务列表[i].lastNeuron - start;
                广播("GetNeurons " + start + " " + firstCount);
                remaining -= firstCount;
                accumNeurons += firstCount;
                start = 服务列表[i].lastNeuron;
                while (tempNeurons.Count < accumNeurons) Thread.Sleep(1);
                i++;
            }

            广播("GetNeurons " + start + " " + remaining);
            while (tempNeurons.Count < count) 
                Thread.Sleep(1);
            tempNeurons.Sort((t1, t2) => t1.id.CompareTo(t2.id)); //the neurons may be returned in different order
            return tempNeurons;
        }
        public static void 设置神经元(神经元 n)
        {
            string command = "SetNeuron ";
            command += n.id + " ";
            command += (int)n.模型 + " ";
            command += n.当前更改 + " ";
            command += n.最后更改 + " ";
            command += n.leakRate泄露速度 + " ";
            command += n.突触延迟 + " ";
            广播(command);
        }

        static private int 获取服务索引(int neuronID)
        {
            for (int i = 0; i < 服务列表.Count; i++)
                if (neuronID >= 服务列表[i].firstNeuron && neuronID < 服务列表[i].lastNeuron) return i;
            return -1;
        }
        public static void 添加突触(int src, int dest, float weight, 突触.modelType model, bool noBackPtr)
        {
            string command = "AddSynapse ";
            command += src + " ";
            command += dest + " ";
            command += weight + " ";
            command += (int)model + " ";
            int srcServer = 获取服务索引(src);
            向服务器发送消息(服务列表[srcServer].ipAddress, command);
            int destServer = 获取服务索引(dest);
            if (srcServer != destServer)
                向服务器发送消息(服务列表[destServer].ipAddress, command);
        }
        public static void 删除突触(int src, int dest)
        {
            string command = "DeleteSynapse ";
            command += src + " ";
            command += dest + " ";
            广播(command);
        }

        static List<突触> tempSynapses = null;
        public static List<突触> GetSynapses(int id)
        {
            tempSynapses = null;
            string command = "GetSynapses " + id;
            广播(command);
            while (tempSynapses == null) Thread.Sleep(1);
            return tempSynapses;
        }
        public static List<突触> 获取突触(int id)
        {
            tempSynapses = null;
            string command = "GetSynapsesFrom " + id;
            广播(command);
            while (tempSynapses == null) Thread.Sleep(1);
            return tempSynapses;
        }

        public static void 在所有服务器上等待完成()
        {
            while (服务列表.FindIndex(x => x.busy == true) != -1) Thread.Sleep(1);
        }

        static void 处理输入消息(string message)
        {
            string[] commands = message.Trim().Split(' ');
            string command = commands[0];
            switch (command)
            {
                case "PingBack":
                    GetSystemTimePreciseAsFileTime(out returnTime);
                    pingCount++;
                    break;

                case "ServerInfo":
                    int index = 服务列表.FindIndex(x => x.name == commands[2]);
                    if (index == -1)
                    {
                        Server s = new Server();
                        IPAddress.TryParse(commands[1], out s.ipAddress);
                        s.name = commands[2];
                        int.TryParse(commands[3], out s.firstNeuron);
                        int.TryParse(commands[4], out s.lastNeuron);
                        if (commands.Length > 5) int.TryParse(commands[5], out s.neuronsInUse);
                        if (commands.Length > 6) long.TryParse(commands[6], out s.totalSynapses);
                        服务列表.Add(s);
                        index = 服务列表.Count - 1;
                    }
                    break;

                case "Done":
                    index = 服务列表.FindIndex(x => x.name == commands[1]);
                    if (index != -1)
                    {
                        服务列表[index].busy = false;
                        long.TryParse(commands[2], out 服务列表[index].generation);
                        int.TryParse(commands[3], out 服务列表[index].firedCount);
                    }
                    break;

                case "Neuron":
                    神经元 n = new 神经元();
                    int.TryParse(commands[1], out n.id);
                    int.TryParse(commands[2], out int tempModel);
                    n.模型 = (神经元.模型类型)tempModel;
                    float.TryParse(commands[3], out n.最后更改);
                    float.TryParse(commands[4], out n.leakRate泄露速度);
                    int.TryParse(commands[5], out n.突触延迟);
                    bool.TryParse(commands[6], out n.是否使用);
                    tempNeuron = n;
                    break;

                case "Neurons":
                    int.TryParse(commands[1], out int count);
                    for (int i = 2; i < commands.Length; i += 6)
                    {
                        n = new 神经元();
                        int.TryParse(commands[i], out n.id);
                        int.TryParse(commands[i+1], out tempModel);
                        n.模型 = (神经元.模型类型)tempModel;
                        float.TryParse(commands[i+2], out n.最后更改);
                        float.TryParse(commands[i+3], out n.leakRate泄露速度);
                        int.TryParse(commands[i+4], out n.突触延迟);
                        bool.TryParse(commands[i+5], out n.是否使用);
                        tempNeurons.Add(n);
                    }
                    break;

                case "Synapses":
                    int.TryParse(commands[1], out int neuronID);
                    List<突触> synapses = new List<突触>();
                    for (int i = 2; i < commands.Length; i += 3)
                    {
                        突触 s = new 突触();
                        int.TryParse(commands[i], out s.targetNeuron);
                        float.TryParse(commands[i + 1], out s.weight);
                        int.TryParse(commands[i + 2], out int modelInt);
                        s.model = (突触.modelType)modelInt;
                        synapses.Add(s);
                    }
                    tempSynapses = synapses;
                    break;

                case "SynapsesFrom":
                    int.TryParse(commands[1], out neuronID);
                    synapses = new List<突触>();
                    for (int i = 2; i < commands.Length; i += 3)
                    {
                        突触 s = new 突触();
                        int.TryParse(commands[i], out s.targetNeuron);
                        float.TryParse(commands[i + 1], out s.weight);
                        int.TryParse(commands[i + 2], out int modelInt);
                        s.model = (突触.modelType)modelInt;
                        synapses.Add(s);
                    }
                    tempSynapses = synapses;
                    break;

            }
        }
        //TODO: neuron labels cannot contain '...' or '_'
        public static void 从服务器接收()
        {
            while (true)
            {
                string incomingMessage = "";
                var from = new IPEndPoint(IPAddress.Any, serverClientPort);
                var recvBuffer = serverClient.Receive(ref from);
                incomingMessage += Encoding.UTF8.GetString(recvBuffer);
                while (incomingMessage.EndsWith("..."))
                {
                    recvBuffer = serverClient.Receive(ref from);
                    string nextPart = Encoding.UTF8.GetString(recvBuffer);
                    if (nextPart.IndexOf("...") == -1)
                        处理输入消息(nextPart);
                    else
                    {
                        int posOfSpace = nextPart.IndexOf(' ');
                        nextPart = nextPart.Substring(posOfSpace + 1);
                        incomingMessage += nextPart;
                    }
                }
                //Debug.WriteLine("Receive from server: "+incomingMessage);
                incomingMessage = incomingMessage.Replace("...", "");
                处理输入消息(incomingMessage);
            }
        }
        public static void 广播(string message)
        {
            //Debug.WriteLine("Broadcast: " + message);
            byte[] datagram = Encoding.UTF8.GetBytes(message);
            IPEndPoint ipEnd = new IPEndPoint(broadCastAddress, clientServerPort);
            clientServer.SendAsync(datagram, datagram.Length, ipEnd);
        }
        public static void 向服务器发送消息(IPAddress serverIp, string message)
        {
            //Debug.WriteLine("Send to server: " + ip + ": " + message);
            byte[] datagram = Encoding.UTF8.GetBytes(message);
            IPEndPoint ipEnd = new IPEndPoint(serverIp, clientServerPort);
            clientServer.Send(datagram, datagram.Length, ipEnd);
        }
    }
}
