using System;
using System.Linq;

namespace AntConfigurer.Objects
{
    class JsonDevice
    {
        public String Ip { get; set; }
        public String MacAddr { get; set; }
        public String WorkerName { get; set; }
        public String Location { get; set; }
        public String Config { get; set; }
        
        public JsonDevice() {}

        /// <summary> Generates class from CompletedConfig element </summary>
        /// <param name="config"> Completed config element with all params set </param>
        public JsonDevice(CompletedConfig config)
        {
            this.Ip = config.GetIpAddr();
            this.MacAddr = config.GetMacAddr();
            this.Config = config.GenerateJsonConfig();

            var simpleConfig = config.GetPoolConfigs().First();
            
            if (simpleConfig != null)
                this.WorkerName = simpleConfig.GetWorker();
        }
    }
}
