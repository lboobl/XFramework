namespace MySql.Data.MySqlClient
{
    using Microsoft.Win32;
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Configuration.Install;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;
    using System.Xml;

    [RunInstaller(true)]
    public class CustomInstaller : Installer
    {
        private int perfMonIndex;

        public CustomInstaller()
        {
            if (File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.System) + @"\loadperf.dll"))
            {
                PerformanceCounterInstaller installer = new PerformanceCounterInstaller {
                    CategoryName = Resources.PerfMonCategoryName,
                    CategoryHelp = Resources.PerfMonCategoryHelp,
                    CategoryType = PerformanceCounterCategoryType.SingleInstance
                };
                CounterCreationData data = new CounterCreationData(Resources.PerfMonHardProcName, Resources.PerfMonHardProcHelp, PerformanceCounterType.NumberOfItems32);
                CounterCreationData data2 = new CounterCreationData(Resources.PerfMonSoftProcName, Resources.PerfMonSoftProcHelp, PerformanceCounterType.RateOfCountsPerSecond32);
                installer.Counters.Add(data);
                installer.Counters.Add(data2);
                this.perfMonIndex = base.Installers.Add(installer);
            }
        }

        private static void AddProviderToMachineConfig()
        {
            object obj2 = Registry.GetValue(@"HKEY_LOCAL_MACHINE\Software\Microsoft\.NETFramework\", "InstallRoot", null);
            if (obj2 == null)
            {
                throw new Exception("Unable to retrieve install root for .NET framework");
            }
            AddProviderToMachineConfigInDir(obj2.ToString());
            string str = obj2.ToString();
            str = str.Substring(0, str.Length - 1);
            str = string.Format("{0}64{1}", str, Path.DirectorySeparatorChar);
            if (Directory.Exists(str))
            {
                AddProviderToMachineConfigInDir(str);
            }
        }

        private static void AddProviderToMachineConfigInDir(string path)
        {
            string str = string.Format(@"{0}v2.0.50727\CONFIG\machine.config", path);
            StreamReader reader = new StreamReader(str);
            string xml = reader.ReadToEnd();
            reader.Close();
            XmlDocument document = new XmlDocument();
            document.LoadXml(xml);
            XmlElement newChild = (XmlElement) document.CreateNode(XmlNodeType.Element, "add", "");
            newChild.SetAttribute("name", "MySQL Data Provider");
            newChild.SetAttribute("invariant", "MySql.Data.MySqlClient");
            newChild.SetAttribute("description", ".Net Framework Data Provider for MySQL");
            Assembly executingAssembly = Assembly.GetExecutingAssembly();
            string str3 = string.Format("MySql.Data.MySqlClient.MySqlClientFactory, {0}", executingAssembly.FullName);
            newChild.SetAttribute("type", str3);
            XmlNodeList elementsByTagName = document.GetElementsByTagName("DbProviderFactories");
            foreach (XmlNode node in elementsByTagName[0].ChildNodes)
            {
                if (node.Attributes != null)
                {
                    foreach (XmlAttribute attribute in node.Attributes)
                    {
                        if ((attribute.Name == "invariant") && (attribute.Value == "MySql.Data.MySqlClient"))
                        {
                            elementsByTagName[0].RemoveChild(node);
                            break;
                        }
                    }
                    continue;
                }
            }
            elementsByTagName[0].AppendChild(newChild);
            XmlTextWriter w = new XmlTextWriter(str, null) {
                Formatting = Formatting.Indented
            };
            document.Save(w);
            w.Flush();
            w.Close();
        }

        public override void Install(IDictionary stateSaver)
        {
            base.Install(stateSaver);
            AddProviderToMachineConfig();
        }

        private static void RemoveProviderFromMachineConfig()
        {
            object obj2 = Registry.GetValue(@"HKEY_LOCAL_MACHINE\Software\Microsoft\.NETFramework\", "InstallRoot", null);
            if (obj2 == null)
            {
                throw new Exception("Unable to retrieve install root for .NET framework");
            }
            RemoveProviderFromMachineConfigInDir(obj2.ToString());
            string str = obj2.ToString();
            str = str.Substring(0, str.Length - 1);
            str = string.Format("{0}64{1}", str, Path.DirectorySeparatorChar);
            if (Directory.Exists(str))
            {
                RemoveProviderFromMachineConfigInDir(str);
            }
        }

        private static void RemoveProviderFromMachineConfigInDir(string path)
        {
            string str = string.Format(@"{0}v2.0.50727\CONFIG\machine.config", path);
            StreamReader reader = new StreamReader(str);
            string xml = reader.ReadToEnd();
            reader.Close();
            XmlDocument document = new XmlDocument();
            document.LoadXml(xml);
            XmlNodeList elementsByTagName = document.GetElementsByTagName("DbProviderFactories");
            foreach (XmlNode node in elementsByTagName[0].ChildNodes)
            {
                if ((node.Attributes != null) && (node.Attributes["name"].Value == "MySQL Data Provider"))
                {
                    elementsByTagName[0].RemoveChild(node);
                    break;
                }
            }
            XmlTextWriter w = new XmlTextWriter(str, null) {
                Formatting = Formatting.Indented
            };
            document.Save(w);
            w.Flush();
            w.Close();
        }

        public override void Uninstall(IDictionary savedState)
        {
            if (!PerformanceCounterCategory.Exists(Resources.PerfMonCategoryName))
            {
                base.Installers.RemoveAt(this.perfMonIndex);
            }
            base.Uninstall(savedState);
            RemoveProviderFromMachineConfig();
        }
    }
}

