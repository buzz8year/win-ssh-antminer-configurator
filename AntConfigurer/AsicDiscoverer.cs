using AntConfigurer.Objects;
using System.Collections.Generic;
using System;

namespace AntConfigurer
{
    class AsicDiscoverer
    {
        protected static Boolean Parsing = false;
        protected static List<AsicDevice> Devices = new List<AsicDevice>();

        public static void Start()
        {
            if (AsicDiscoverer.Parsing)
                return;
        }

        public static void Stop() {}
    }
}
