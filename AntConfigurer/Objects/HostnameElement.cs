using System;
using System.Collections.Generic;

namespace AntConfigurer.Objects
{
    class HostnameElement
    {
        private Int32 _ctr;
        private String _hostname;
        private String _ipAddress;
        private List<String> _errors = new List<string>();

        public String GetIpAddress()
        {
            return this._ipAddress;
        }

        public HostnameElement SetIpAddress(String ipAddress)
        {
            this._ipAddress = ipAddress;
            return this;
        }

        public String GetHostname()
        {
            return this._hostname;
        }

        public HostnameElement SetHostname(String hostname)
        {
            this._hostname = hostname;
            return this;
        }

        public Int32 GetCtr()
        {
            return this._ctr;
        }

        public HostnameElement SetCtr(Int32 ctr)
        {
            this._ctr = ctr;
            return this;
        }

        public List<String> GetErrors()
        {
            return this._errors;
        }

        public Boolean Check()
        {
            this._errors = new List<string>();
            return true;
        }
    }
}
