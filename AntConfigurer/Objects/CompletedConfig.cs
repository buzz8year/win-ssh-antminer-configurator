using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace AntConfigurer.Objects
{
    class CompletedConfig
    {
        protected String MacAddr;
        protected String IpAddr;
        
        protected Boolean ApiListen = true;
        protected Boolean ApiNetwork = true;
        protected String ApiAllow = "A:0/0,W:*";
        protected String ApiGroups = "A:stats:pools:devs:summary:version";
        
        protected Boolean BitmainUseVil = true;
        protected String BitmainVoltage = "0706";
        protected UInt16 BitmainFreq = 550;
        
        protected String MultiVersion = "1";
        protected List<ConfigElement> PoolConfigs;
        

        public String GetMacAddr()
        {
            return this.MacAddr;
        }

        public CompletedConfig SetMacAddr(String macAddr)
        {
            this.MacAddr = macAddr;
            return this;
        }

        public String GetIpAddr()
        {
            return this.IpAddr;
        }

        public CompletedConfig SetIpAddr(String ipAddr)
        {
            this.IpAddr = ipAddr;
            return this;
        }

        public Boolean GetApiListen()
        {
            return this.ApiListen;
        }

        public CompletedConfig SetApiListen(Boolean apiListen)
        {
            this.ApiListen = apiListen;
            return this;
        }

        public Boolean GetApiNetwork()
        {
            return this.ApiNetwork;
        }

        public CompletedConfig SetApiNetwork(Boolean apiNetwork)
        {
            this.ApiNetwork = apiNetwork;
            return this;
        }

        public String GetApiGroups()
        {
            return this.ApiGroups;
        }

        public CompletedConfig SetApiGroups(String apiGroups)
        {
            this.ApiGroups = apiGroups;
            return this;
        }

        public String GetApiAllow()
        {
            return this.ApiAllow;
        }

        public CompletedConfig SetApiAllow(String apiAllow)
        {
            this.ApiAllow = apiAllow;
            return this;
        }

        public Boolean GetBitmainUseVil()
        {
            return this.BitmainUseVil;
        }

        public CompletedConfig SetBimainUseVil(Boolean bitmainUseVil)
        {
            this.BitmainUseVil = bitmainUseVil;
            return this;
        }

        public UInt16 GetBitmainFreq()
        {
            return this.BitmainFreq;
        }

        public CompletedConfig SetBitmainFreq(UInt16 bitmainFreq)
        {
            this.BitmainFreq = bitmainFreq;
            return this;
        }

        public String GetBitmainVoltage()
        {
            return this.BitmainVoltage;
        }

        public CompletedConfig SetBitmainVoltage(String bitmainVoltage)
        {
            this.BitmainVoltage = bitmainVoltage;
            return this;
        }

        public String GetMultiVersion()
        {
            return this.MultiVersion;
        }

        public CompletedConfig SetMultiVersion(String multiVersion)
        {
            this.MultiVersion = multiVersion;
            return this;
        }

        public List<ConfigElement> GetPoolConfigs()
        {
            return this.PoolConfigs;
        }

        public CompletedConfig SetPoolConfigs(List<ConfigElement> configs)
        {
            this.PoolConfigs = configs;
            return this;
        }

        public String GenerateJsonConfig(Boolean escapeQuotes = true)
        {
            String newConfigString;
            
            if (escapeQuotes)
            {
                newConfigString = "{\"pools\": [";
            }
            else
            {
                newConfigString = "{'pools': [";
            }

            int i = 0;
            
            foreach (ConfigElement singleConfig in this.PoolConfigs)
            {
                if (i != 0)
                {
                    newConfigString += ",";
                }

                newConfigString += "{";
                if (escapeQuotes)
                {
                    newConfigString += "\"url\": \"" + singleConfig.GetUrl() + "\",";
                    newConfigString += "\"user\": \"" + singleConfig.GetWorker() + "\",";
                    newConfigString += "\"pass\": \"" + singleConfig.GetPass() + "\"";
                }
                else
                {
                    newConfigString += "'url':'" + singleConfig.GetUrl() + "',";
                    newConfigString += "'user: '" + singleConfig.GetWorker() + "',";
                    newConfigString += "'pass': '" + singleConfig.GetPass() + "'";
                }

                newConfigString += "}";
                i++;
            }
            newConfigString += "],";
            
            if (escapeQuotes)
            {
                newConfigString += ("\"api-listen\": " + (this.GetApiListen() ? "true" : "false")) + ",";
                newConfigString += ("\"api-network\": " + (this.GetApiNetwork() ? "true" : "false")) + ",";
                newConfigString += ("\"api-groups\": \"" + this.GetApiGroups()) + "\",";
                newConfigString += ("\"api-allow\": \"" + this.GetApiAllow()) + "\",";
                newConfigString += ("\"bitmain-use-vil\": " + (this.GetBitmainUseVil() ? "true" : "false")) + ",";
                newConfigString += ("\"bitmain-freq\": \"" + this.GetBitmainFreq()) + "\",";
                newConfigString += ("\"bitmain-voltage\": \"" + this.GetBitmainVoltage()) + "\",";
                newConfigString += ("\"multi-version\": \"" + this.GetMultiVersion()) + "\"";
            }
            else
            {
                newConfigString += ("'api-listen': " + (this.GetApiListen() ? "true" : "false")) + ",";
                newConfigString += ("'api-network': " + (this.GetApiNetwork() ? "true" : "false")) + ",";
                newConfigString += ("'api-groups': '" + this.GetApiGroups()) + "',";
                newConfigString += ("'api-allow': '" + this.GetApiAllow()) + "',";
                newConfigString += ("'bitmain-use-vil': " + (this.GetBitmainUseVil() ? "true" : "false")) + ",";
                newConfigString += ("'bitmain-freq': '" + this.GetBitmainFreq()) + "',";
                newConfigString += ("'bitmain-voltage': '" + this.GetBitmainVoltage()) + "',";
                newConfigString += ("'multi-version': '" + this.GetMultiVersion()) + "'";
            }
            newConfigString += "}";

            return newConfigString;
        }

        public Boolean UploadFromJsonString(String json)
        {
            try
            {
                JObject parsed = JObject.Parse(json);
                var childrens = parsed.Children();

                using (var sequenceEnum = childrens.GetEnumerator())
                {
                    while (sequenceEnum.MoveNext())
                    {
                        JValue objValue;
                        String realValue;
                        
                        // Do something with the sequenceEnum.Current.
                        var currentElement = sequenceEnum.Current as JProperty;
                        
                        switch (currentElement.Name)
                        {
                            case "api-listen":
                                objValue = currentElement.Value as JValue;
                                realValue = objValue.Value.ToString();
                                this.SetApiListen(Strings.CastStringToBoolean(objValue.Value.ToString()));
                                break;
                            
                            case "api-network":
                                objValue = currentElement.Value as JValue;
                                realValue = objValue.Value.ToString();
                                this.SetApiNetwork(Strings.CastStringToBoolean(objValue.Value.ToString()));
                                break;
                            
                            case "api-groups":
                                objValue = currentElement.Value as JValue;
                                realValue = objValue.Value.ToString();
                                this.SetApiGroups(realValue);
                                break;
                            
                            case "api-allow":
                                objValue = currentElement.Value as JValue;
                                realValue = objValue.Value.ToString();
                                this.SetApiAllow(realValue);
                                break;
                            
                            case "bitmain-use-vil":
                                objValue = currentElement.Value as JValue;
                                realValue = objValue.Value.ToString();
                                this.SetBimainUseVil(Strings.CastStringToBoolean(objValue.Value.ToString()));
                                break;
                            
                            case "bitmain-freq":
                                objValue = currentElement.Value as JValue;
                                realValue = objValue.Value.ToString();
                                this.SetBitmainFreq(UInt16.Parse(realValue));
                                break;
                            
                            case "bitmain-voltage":
                                objValue = currentElement.Value as JValue;
                                realValue = objValue.Value.ToString();
                                this.SetBitmainVoltage(realValue);
                                break;
                            
                            case "multi-version":
                                objValue = currentElement.Value as JValue;
                                realValue = objValue.Value.ToString();
                                this.SetMultiVersion(realValue);
                                break;
                        }
                    }
                }
                
                // String random_stuff = parsed.GetValue("random_stuff").ToString();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }

            return true;
        }
    }
}
