using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Principal;
using System.Diagnostics;
using NetFwTypeLib;

namespace SetupFirewall
{
    class Program
    {
        public static bool IsAdministrator()
        {
            var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        public static bool Confirm(string prompt)
        {
            Console.WriteLine(prompt);
            Console.WriteLine("    Do you want to continue? Y/N");
            ConsoleKeyInfo xx = new ConsoleKeyInfo();
            while (xx.Key != ConsoleKey.Y && xx.Key != ConsoleKey.N)
                xx = Console.ReadKey();
            if (xx.Key.ToString().ToLower() == "n") return false;
            Console.WriteLine();
            return true;
        }

        static void Main(string[] args)
        {
            if (!Confirm("该程序将修改您的防火墙规则以启用将 Brain Simulator II 与 NeuronServer 一起使用")) return;

            if (!IsAdministrator())
            {
                if (!Confirm("需要管理员权限")) return;


                var proc = new ProcessStartInfo();
                proc.UseShellExecute = true;
                proc.WorkingDirectory = Environment.CurrentDirectory;
                proc.FileName = System.Reflection.Assembly.GetExecutingAssembly().Location.Replace("dll","exe");
                proc.Verb = "runas";
                
                try
                {
                    Process.Start(proc);
                }
                catch (Exception)
                {
                    Console.WriteLine("提升失败.");
                    return;
                }
                return;
            }

            string[] theName = { "BrainSimulator.exe", "NeuronServer.exe" };
            INetFwPolicy2 firewallPolicy = (INetFwPolicy2)Activator.CreateInstance(
                Type.GetTypeFromProgID("HNetCfg.FwPolicy2"));

            if (Confirm("如果规则已经存在，删除它们? "))
            {

                INetFwRule theRule;
                for (int i = 0; i < theName.Length; i++)
                {
                    try
                    {
                        theRule = firewallPolicy.Rules.Item(theName[i]);

                        while (theRule != null)
                        {
                            firewallPolicy.Rules.Remove(theName[i]);
                            Console.WriteLine("已删除的规则: " + theName[i]);
                            theRule = firewallPolicy.Rules.Item(theName[i]);
                        }
                    }
                    catch
                    {
                        //get here if the rule doesn't exist
                    }
                }
            }

            if (!Confirm("创建新规则")) return;
            string filePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string directory = System.IO.Path.GetDirectoryName(filePath);

            try
            {
                for (int i = 0; i < theName.Length; i++)
                {
                    string fullExecutable = directory + "\\" + theName[i];
                    INetFwRule firewallRule = (INetFwRule)Activator.CreateInstance(
                        Type.GetTypeFromProgID("HNetCfg.FWRule"));
                    firewallRule.ApplicationName = fullExecutable;
                    firewallRule.Action = NET_FW_ACTION_.NET_FW_ACTION_ALLOW;
                    firewallRule.Description = "Allow " + theName[i] + " to receive UDP packets";
                    firewallRule.Direction = NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_IN; //or OUT
                    firewallRule.Enabled = true;
                    firewallRule.Protocol = (int)NET_FW_IP_PROTOCOL_.NET_FW_IP_PROTOCOL_UDP;// 17;
                    firewallRule.Profiles = 6;
                    firewallRule.InterfaceTypes = "all";
                    firewallRule.Name = theName[i];
                    firewallPolicy.Rules.Add(firewallRule);
                    Console.WriteLine("Created rule: " + theName[i]);

                }

                Console.Write("成功...按任意键");
                Console.ReadKey();
            }
            catch (Exception e)
            {
                Console.Write("失败是因为: " + e.Message + "按任意键");
                Console.ReadKey();
            }
        }
    }
}
