using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Management;

namespace NoiseAnalysis.ThreadPoolConfig
{
    class GetTreadPool
    {
        private int maxTread=0;
        private GetTreadPool() {
            ManagementClass c = new ManagementClass(new ManagementPath("Win32_Processor"));
            // Get the properties in the class
            ManagementObjectCollection moc = c.GetInstances();
            foreach (ManagementObject mo in moc)
            {
                PropertyDataCollection properties = mo.Properties;
                maxTread+= (int)properties["NumberOfLogicalProcessors"].Value;
            }    
        }
        public static readonly GetTreadPool instance = new GetTreadPool();

    }
}
