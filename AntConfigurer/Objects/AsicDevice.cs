using System;

namespace AntConfigurer.Objects
{
    class AsicDevice
    {
        private String _name;
        private Boolean _confirmed;
        
        private String _ipAddr;
        private String _realIpAddr;
        private String _macAddr;

        public string Name { get => _name; set => _name = value; }
        public string IpAddr { get => _ipAddr; set => _ipAddr = value; }
        public string MacAddr { get => _macAddr; set => _macAddr = value; }
        public bool Confirmed { get => _confirmed; set => _confirmed = value; }
        public string RealIpAddr { get => _realIpAddr; set => _realIpAddr = value; }
    }
}
