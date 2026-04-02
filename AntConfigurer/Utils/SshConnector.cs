using System;
using System.Collections.Generic;
using AntConfigurer.Objects;
using Renci.SshNet;

namespace AntConfigurer.Utils
{
    class SshConnector
    {
        private String _ipAddress;
        private String _commonHname;
        private String _username;
        private String _password;
        private UInt16 _port = 22;
        
        private SshClient _client;
        private CompletedConfig _lastFullConfig;
        
        private Boolean _connectionEstablished = false;
        private Boolean _shouldNotifyMonitoring = false;
        
        private List<String> _errors = new List<string>();
        private List<String> _executionLog = new List<String>();

        private const String DefaultNetworkConfHname = "antMiner";
        private const Boolean DefaulNetworkConfDhcp = true;

        public String GetIpAddress()
        {
            return this._ipAddress;
        }

        public SshConnector SetIpAddress(String ipAddress)
        {
            this._ipAddress = ipAddress;
            return this;
        }

        public String GetCommonHhame()
        {
            return this._commonHname;
        }

        public SshConnector SetCommonHname(String commonHname)
        {
            this._commonHname = commonHname;
            return this;
        }

        public String GetUsername()
        {
            return this._username;
        }

        public SshConnector SetUsername(String username)
        {
            this._username = username;
            return this;
        }

        public String GetPassword()
        {
            return this._password;
        }

        public SshConnector SetPassword(String password)
        {
            this._password = password;
            return this;
        }

        public UInt16 GetPort()
        {
            return this._port;
        }

        public SshConnector SetPort(UInt16 port)
        {
            this._port = port;
            return this;
        }

        public List<String> GetExecutionLog()
        {
            return this._executionLog;
        }

        public List<String> GetErrors()
        {
            return this._errors;
        }

        public Boolean GetShouldNotifyMonitoring()
        {
            return this._shouldNotifyMonitoring;
        }

        public CompletedConfig GetLastFullConfig()
        {
            return this._lastFullConfig;
        }

        public Boolean Connect(int connectionTimeout = 100)
        {
            this._errors.Clear();

            if (this._connectionEstablished && this._client.IsConnected)
            {
                return true;
            }

            try
            {
                if (!Strings.ValidateIpV4(this._ipAddress))
                {
                    throw new Exception("An exception occured! SSH connection didn't receive an IP address");
                }

                if (String.IsNullOrWhiteSpace(this._username))
                {
                    throw new Exception("An exception occured! No username was provided");
                }
                
                ConnectionInfo connectionInfo = new ConnectionInfo(this._ipAddress, 22, this._username, new AuthenticationMethod[] { new PasswordAuthenticationMethod(this._username, this._password) });
                connectionInfo.Timeout = new TimeSpan(0, 0, connectionTimeout);
                
                this._client = new SshClient(connectionInfo)
                {
                    KeepAliveInterval = new TimeSpan(0, 5, 00)
                };

                _client.Connect();
                this._connectionEstablished = true;
            }
            catch (Exception e)
            {
                this._errors.Add(e.Message);
            }

            return this._errors.Count < 1;
        }

        public void Disconnect()
        {
            try
            {
                this._connectionEstablished = false;
                this._client.Disconnect();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public Boolean Execute(List<String> commandsList)
        {
            this._errors.Clear();
            this._executionLog.Clear();

            var isConnectionEstablished = this.Connect();
            
            if (!isConnectionEstablished)
            {
                return false;
            }

            try
            {
                foreach (String command in commandsList)
                {
                    using (var cmd = this._client.CreateCommand(command))
                    {
                        this._executionLog.Add("> Command: " + command);
                        cmd.Execute();
                        
                        this._executionLog.Add("> Exit code: " + cmd.ExitStatus);
                    }
                }
            }
            catch (Exception e)
            {
                this._errors.Add(e.Message);
            }

            if (this._client.IsConnected)
            {
                this.Disconnect();
            }

            this._connectionEstablished = false;

            return this._errors.Count < 1;
        }

        public String InspectMachine()
        {
            List<String> result = new List<string>();
            this._errors.Clear();

            var isConnectionEstablished = this.Connect(10);
            
            if (!isConnectionEstablished)
            {
                this._errors.Add("Connection is not established");
                return null;
            }

            try
            {
#if DEBUG
                // String config_folder = "/home/asic/config/";
                String configFolder = "/config/";
#else
                String config_folder = "/config/";
#endif
                
                // Checking if the folder exists
                String cmdText = "test -e " + configFolder + " && echo folder exists || echo no folder";
                var cmd = this._client.CreateCommand(cmdText);
                
                result.Add("$ " + cmdText);
                this._executionLog.Add("> Command: " + cmdText);
                cmd.Execute();
                
                this._executionLog.Add("> Exit code: " + cmd.ExitStatus);
                result.Add(cmd.Result);

                if (cmd.Result.StartsWith("no folder"))
                {
                    throw new Exception("Folder with configs wasn't found");
                }

                cmdText = "test -e " + configFolder + "bmminer.conf && echo file exists || echo file not found";
                cmd = this._client.CreateCommand(cmdText);
                result.Add("$ " + cmdText);
                
                this._executionLog.Add("> Command: " + cmdText);
                cmd.Execute();
                
                this._executionLog.Add("> Exit code: " + cmd.ExitStatus);
                result.Add(cmd.Result);

                if (cmd.Result.StartsWith("file not found"))
                {
                    throw new Exception("File with configs wasn't found");
                }

                cmdText = "/sbin/ifconfig | sed '1,1!d' | sed 's/.*HWaddr //' | sed 's/\\ .*//'";
                cmd = this._client.CreateCommand(cmdText);
                
                this._executionLog.Add("> Command: " + cmdText);
                cmd.Execute();
                
                this._executionLog.Add("> Exit code: " + cmd.ExitStatus);
                var macAddr = cmd.Result;

                return macAddr;
            }
            catch (Exception e)
            {
                this._errors.Add(e.Message);
                result.Add(e.Message);
            }

            if (this._client.IsConnected)
            {
                this.Disconnect();
            }

            this._connectionEstablished = false;
            return null;
        }

        public List<String> UploadConfig(List<ConfigElement> configs, Boolean overrideOldSettings = true)
        {
            List<String> result = new List<string>();
            
            this._errors.Clear();
            this._executionLog.Clear();
            
            this._shouldNotifyMonitoring = false;
            this._lastFullConfig = null;

            var isConnectionEstablished = this.Connect();
            
            if (!isConnectionEstablished)
            {
                this._errors.Add("Connection is not established");
                return result;
            }

            try
            {
#if DEBUG
                String configFolder = "/home/asic/config/";
#else
                String config_folder = "/config/";
#endif
                
                // Checking if the folder exists
                String cmdText = "test -e " + configFolder + " && echo folder exists || echo no folder";
                var cmd = this._client.CreateCommand(cmdText);
                
                result.Add("$ " + cmdText);
                this._executionLog.Add("> Command: " + cmdText);
                cmd.Execute();
                
                this._executionLog.Add("> Exit code: " + cmd.ExitStatus);
                result.Add(cmd.Result);

                if (cmd.Result.StartsWith("no folder"))
                {
                    throw new Exception("Folder with configs wasn't found");
                }

                // Retrieving MAC address
                cmdText = "/sbin/ifconfig | sed '1,1!d' | sed 's/.*HWaddr //' | sed 's/\\ .*//'";
                cmd = this._client.CreateCommand(cmdText);
                
                this._executionLog.Add("> Command: " + cmdText);
                cmd.Execute();
                
                this._executionLog.Add("> Exit code: " + cmd.ExitStatus);
                var macAddr = cmd.Result;

                foreach (ConfigElement conf in configs)
                {
                    if (String.IsNullOrWhiteSpace(conf.GetWorker()))
                    {
                        conf.GenerateWorkerName(conf.GetAsicIp(), macAddr);
                    }
                    else
                    {
                        String realWName = conf.GetWorker();
                        
                        if (!String.IsNullOrWhiteSpace(conf.GetPrefix()))
                        {
                            realWName = conf.GetPrefix() + realWName;
                        }

                        if (!String.IsNullOrWhiteSpace(conf.GetSuffix()))
                        {
                            realWName += conf.GetSuffix();
                        }

                        conf.SetWorker(realWName);
                    }

                }

                /*
                cmd_text = "cd " + config_folder;
                cmd = this.client.CreateCommand("cd " + config_folder);
                result.Add("$ cd " + config_folder);
                
                this.execution_log.Add("> Command: cd " + config_folder);
                cmd.Execute();
                
                this.execution_log.Add("> Exit code: " + cmd.ExitStatus);
                result.Add(cmd.Result);
                */

                cmdText = "test -e " + configFolder + "bmminer.conf && echo file exists || echo file not found";
                cmd = this._client.CreateCommand(cmdText);
                result.Add("$ " + cmdText);
                
                this._executionLog.Add("> Command: " + cmdText);
                cmd.Execute();
                
                this._executionLog.Add("> Exit code: " + cmd.ExitStatus);
                result.Add(cmd.Result);

                CompletedConfig fullConfig;
                
                if (cmd.Result.StartsWith("file not found"))
                {
                    result.Add("File with configs wasn't found. Attempting to create...");
                    cmdText = "touch " + configFolder + "bmminer.conf";
                    
                    cmd = this._client.CreateCommand(cmdText);
                    result.Add("$ " + cmdText);
                    
                    this._executionLog.Add("> Command: " + cmdText);
                    cmd.Execute();
                    
                    this._executionLog.Add("> Exit code: " + cmd.ExitStatus);
                    result.Add(cmd.Result);

                    fullConfig = new CompletedConfig();
                    fullConfig.SetPoolConfigs(configs);
                }
                else
                {
                    cmdText = "test -e " + configFolder + "bmmine.conf.backup && echo file exists || echo file not found";
                    cmd = this._client.CreateCommand(cmdText);
                    result.Add("$ " + cmdText);
                    
                    this._executionLog.Add("> Command: " + cmdText);
                    cmd.Execute();
                    
                    this._executionLog.Add("> Exit code: " + cmd.ExitStatus);
                    result.Add(cmd.Result);

                    if (!overrideOldSettings && cmd.Result.StartsWith("file exists"))
                    {
                        result.Add("Old configs were found and you don't want to override current. So, this device will be skipped");
                        
                        if (this._client.IsConnected)
                        {
                            this.Disconnect();
                        }

                        this._connectionEstablished = false;

                        return result;
                    }

                    result.Add("File with configs was found. Old config will be backuped and removed");
                    cmdText = "rm -f" + configFolder + "bmmine.conf.backup";
                    
                    cmd = this._client.CreateCommand(cmdText);
                    cmd.Execute();

                    cmdText = "cat " + configFolder + "bmminer.conf | tr -d '\\r\\n'";
                    this._executionLog.Add("> Command: " + cmdText);
                    
                    cmd = this._client.CreateCommand(cmdText);
                    cmd.Execute();
                    
                    this._executionLog.Add("> Exit code: " + cmd.ExitStatus);
                    var data = cmd.Result;

                    fullConfig = new CompletedConfig();
                    fullConfig.SetPoolConfigs(configs);
                    fullConfig.UploadFromJsonString(data);

                    cmdText = "mv " + configFolder + "bmminer.conf " + configFolder + "bmmine.conf.backup";
                    cmd = this._client.CreateCommand(cmdText);
                    cmd.Execute();
                    
                    cmdText = "touch " + configFolder + "bmminer.conf";
                    cmd = this._client.CreateCommand(cmdText);
                    cmd.Execute();
                }

                fullConfig.SetIpAddr(this._ipAddress).SetMacAddr(macAddr);

                String configString2Upload = fullConfig.GenerateJsonConfig();
                result.Add("Writing new configs...");
                
                cmdText = "printf '" + configString2Upload + "' > " + configFolder + "bmminer.conf";
                cmd = this._client.CreateCommand(cmdText);
                
                this._executionLog.Add("> Command: " + cmdText);
                cmd.Execute();
                
                this._executionLog.Add("> Exit code: " + cmd.ExitStatus);
                result.Add(cmd.Result);

                result.Add("Changing file access to 400...");
                cmdText = "chmod 400 " + configFolder + "bmminer.conf";
                cmd = this._client.CreateCommand(cmdText);
                
                this._executionLog.Add("> Command: " + cmdText);
                cmd.Execute();
                
                this._executionLog.Add("> Exit code: " + cmd.ExitStatus);
                result.Add(cmd.Result);

                if (!String.IsNullOrWhiteSpace(this._commonHname))
                {
                    result.Add("Ok, we have hostname, let's rewrite it...");
                    cmdText = "test -e " + configFolder + "network.conf && echo file exists || echo file not found";
                    
                    cmd = this._client.CreateCommand(cmdText);
                    result.Add("$ " + cmdText);
                    
                    this._executionLog.Add("> Command: " + cmdText);
                    cmd.Execute();
                    
                    this._executionLog.Add("> Exit code: " + cmd.ExitStatus);
                    result.Add(cmd.Result);

                    Dictionary<String, String> networkConfiguration = new Dictionary<string, string>();
                    
                    if (cmd.Result.StartsWith("file not found"))
                    {
                        result.Add("No hostname file. Initializing with default and writing with our custom settings...");
                        networkConfiguration.Add("hostname", this._commonHname);
                        networkConfiguration.Add("dhcp", DefaulNetworkConfDhcp.ToString());

                        /*
                        cmd_text = "touch " + config_folder + "network.conf";
                        cmd = this.client.CreateCommand(cmd_text);
                        cmd.Execute();
                        */
                    }
                    else
                    {
                        cmdText = "test -e " + configFolder + "ntwrk.conf.backup && echo file exists || echo file not found";
                        cmd = this._client.CreateCommand(cmdText);
                        result.Add("$ " + cmdText);
                        
                        this._executionLog.Add("> Command: " + cmdText);
                        cmd.Execute();
                        
                        this._executionLog.Add("> Exit code: " + cmd.ExitStatus);
                        result.Add(cmd.Result);

                        cmdText = "rm -f" + configFolder + "ntwrk.conf.backup";
                        cmd = this._client.CreateCommand(cmdText);
                        cmd.Execute();

                        result.Add("Reading old settings...");
                        cmdText = "cat " + configFolder + "network.conf";//| tr -d '\\r\\n'
                        this._executionLog.Add("> Command: " + cmdText);
                        
                        cmd = this._client.CreateCommand(cmdText);
                        cmd.Execute();
                        
                        this._executionLog.Add("> Exit code: " + cmd.ExitStatus);

                        var rawResult = cmd.Result;

                        Console.WriteLine(rawResult);

                        var splittedResult = cmd.Result.Split('\n');
                        
                        for (var ptr = 0; ptr < splittedResult.Length; ptr++)
                        {
                            var splittedConfigElement = splittedResult[ptr].Split('=');
                            
                            if (splittedConfigElement.Length == 2 && splittedConfigElement[0].Length > 0)
                            {
                                networkConfiguration[splittedConfigElement[0]] = splittedConfigElement[1];
                            }
                        }

                        cmdText = "mv " + configFolder + "network.conf " + configFolder + "ntwrk.conf.backup";
                        cmd = this._client.CreateCommand(cmdText);
                        cmd.Execute();
                        
                        cmdText = "touch " + configFolder + "network.conf";
                        cmd = this._client.CreateCommand(cmdText);
                        cmd.Execute();
                    }

                    result.Add("Rewriting with custom settings...");
                    networkConfiguration["hostname"] = this._commonHname;
                    List<String> newNetworkConf = new List<string>();
                    
                    foreach (KeyValuePair<string, string> entry in networkConfiguration)
                    {
                        newNetworkConf.Add(entry.Key + "=" + entry.Value);
                    }

                    String newNetworkConfString = String.Join("\n", newNetworkConf) + "\n";
                    Console.WriteLine(newNetworkConfString);

                    result.Add("Writing new network configs...");
                    cmdText = "printf '" + newNetworkConfString + "' > " + configFolder + "network.conf";
                    cmd = this._client.CreateCommand(cmdText);
                    
                    this._executionLog.Add("> Command: " + cmdText);
                    cmd.Execute();
                    
                    this._executionLog.Add("> Exit code: " + cmd.ExitStatus);
                    result.Add(cmd.Result);

                    result.Add("Changing network file access to 400...");
                    cmdText = "chmod 400 " + configFolder + "network.conf";
                    cmd = this._client.CreateCommand(cmdText);
                    
                    this._executionLog.Add("> Command: " + cmdText);
                    cmd.Execute();
                    
                    this._executionLog.Add("> Exit code: " + cmd.ExitStatus);
                    result.Add(cmd.Result);
                }

                // Ok, config was uploaded. Now, we need to restart this device
                result.Add("Sending restart command to miner...");
                
                /*
                cmd_text = "/etc/init.d/bmminer.sh restart >/dev/null 2>&1";
                cmd = this.client.CreateCommand(cmd_text);
                
                this.execution_log.Add("> Command" + cmd_text);
                cmd.Execute();
                
                this.execution_log.Add("> Exit code: " + cmd.ExitStatus);
                result.Add(cmd.Result);
                */
                
                this._client.RunCommand("/sbin/shutdown -r now >/dev/null 2>&1");

                this._lastFullConfig = fullConfig;
                this._shouldNotifyMonitoring = true;
            }
            catch (Exception e)
            {
                this._errors.Add(e.Message);
                result.Add(e.Message);
            }

            if (this._client.IsConnected)
            {
                this.Disconnect();
            }

            this._connectionEstablished = false;

            return result;
        }

        public List<String> UploadNetworkConfig(HostnameElement config, Boolean overrideOldSettings = true)
        {
            List<String> result = new List<string>();
            this._errors.Clear();
            this._executionLog.Clear();
            this._shouldNotifyMonitoring = false;

            var isConnectionEstablished = this.Connect(300);
            if (!isConnectionEstablished)
            {
                this._errors.Add("Connection is not established");
                return result;
            }

            try
            {
#if DEBUG
                String configFolder = "/home/asic/config/";
#else
                String config_folder = "/config/";
#endif
                
                // Checking if the folder exists
                String cmdText = "test -e " + configFolder + " && echo folder exists || echo no folder";
                var cmd = this._client.CreateCommand(cmdText);
                result.Add("$ " + cmdText);
                
                this._executionLog.Add("> Command: " + cmdText);
                cmd.Execute();
                
                this._executionLog.Add("> Exit code: " + cmd.ExitStatus);
                result.Add(cmd.Result);

                if (cmd.Result.StartsWith("no folder"))
                {
                    throw new Exception("Folder with configs wasn't found");
                }

                // Retrieving MAC address
                cmdText = "/sbin/ifconfig | sed '1,1!d' | sed 's/.*HWaddr //' | sed 's/\\ .*//'";
                cmd = this._client.CreateCommand(cmdText);
                
                this._executionLog.Add("> Command: " + cmdText);
                cmd.Execute();
                
                this._executionLog.Add("> Exit code: " + cmd.ExitStatus);
                var macAddr = cmd.Result;

                result.Add("Ok, we have hostname, let's rewrite it...");
                cmdText = "test -e " + configFolder + "network.conf && echo file exists || echo file not found";
                
                cmd = this._client.CreateCommand(cmdText);
                result.Add("$ " + cmdText);
                
                this._executionLog.Add("> Command: " + cmdText);
                cmd.Execute();
                
                this._executionLog.Add("> Exit code: " + cmd.ExitStatus);
                result.Add(cmd.Result);

                Dictionary<String, String> networkConfiguration = new Dictionary<string, string>();
                
                if (cmd.Result.StartsWith("file not found"))
                {
                    result.Add("No hostname file. Initializing with default and writing with our custom settings...");
                    networkConfiguration.Add("hostname", config.GetHostname());
                    networkConfiguration.Add("dhcp", DefaulNetworkConfDhcp.ToString());

                    /*
                    cmd_text = "touch " + config_folder + "network.conf";
                    cmd = this.client.CreateCommand(cmd_text);
                    cmd.Execute();
                    */
                }
                else
                {
                    cmdText = "test -e " + configFolder + "ntwrk.conf.backup && echo file exists || echo file not found";
                    cmd = this._client.CreateCommand(cmdText);
                    result.Add("$ " + cmdText);
                    
                    this._executionLog.Add("> Command: " + cmdText);
                    cmd.Execute();
                    
                    this._executionLog.Add("> Exit code: " + cmd.ExitStatus);
                    result.Add(cmd.Result);

                    cmdText = "rm -f" + configFolder + "ntwrk.conf.backup";
                    cmd = this._client.CreateCommand(cmdText);
                    cmd.Execute();

                    result.Add("Reading old settings...");
                    cmdText = "cat " + configFolder + "network.conf";//| tr -d '\\r\\n'
                    this._executionLog.Add("> Command: " + cmdText);
                    
                    cmd = this._client.CreateCommand(cmdText);
                    cmd.Execute();
                    
                    this._executionLog.Add("> Exit code: " + cmd.ExitStatus);

                    var rawResult = cmd.Result;

                    Console.WriteLine(rawResult);

                    var splittedResult = cmd.Result.Split('\n');
                    
                    for (var ptr = 0; ptr < splittedResult.Length; ptr++)
                    {
                        var splittedConfigElement = splittedResult[ptr].Split('=');
                        
                        if (splittedConfigElement.Length == 2 && splittedConfigElement[0].Length > 0)
                        {
                            networkConfiguration[splittedConfigElement[0]] = splittedConfigElement[1];
                        }
                    }

                    cmdText = "mv " + configFolder + "network.conf " + configFolder + "ntwrk.conf.backup";
                    cmd = this._client.CreateCommand(cmdText);
                    cmd.Execute();
                    
                    cmdText = "touch " + configFolder + "network.conf";
                    cmd = this._client.CreateCommand(cmdText);
                    cmd.Execute();
                }

                result.Add("Rewriting with custom settings...");
                networkConfiguration["hostname"] = config.GetHostname();
                List<String> newNetworkConf = new List<string>();
                
                foreach (KeyValuePair<string, string> entry in networkConfiguration)
                {
                    newNetworkConf.Add(entry.Key + "=" + entry.Value);
                }

                String newNetworkConfString = String.Join("\n", newNetworkConf) + "\n";
                Console.WriteLine(newNetworkConfString);

                result.Add("Writing new network configs...");
                cmdText = "printf '" + newNetworkConfString + "' > " + configFolder + "network.conf";
                cmd = this._client.CreateCommand(cmdText);
                
                this._executionLog.Add("> Command: " + cmdText);
                cmd.Execute();
                
                this._executionLog.Add("> Exit code: " + cmd.ExitStatus);
                result.Add(cmd.Result);

                result.Add("Changing network file access to 400...");
                cmdText = "chmod 400 " + configFolder + "network.conf";
                cmd = this._client.CreateCommand(cmdText);
                
                this._executionLog.Add("> Command: " + cmdText);
                cmd.Execute();
                
                this._executionLog.Add("> Exit code: " + cmd.ExitStatus);
                result.Add(cmd.Result);

                result.Add("Sending restart command to miner...");
                
                /*
                // cmd_text = "/etc/init.d/network.sh > /dev/null 2>&1";
                cmd_text = "cd . && /etc/init.d/network.sh restart >/dev/null 2>&1 &";
                cmd = this.client.CreateCommand(cmd_text);
                
                this.execution_log.Add("> Command" + cmd_text);
                cmd.Execute();
                
                this.execution_log.Add("> Exit code: " + cmd.ExitStatus);
                result.Add(cmd.Result);
                */
                
                this._client.RunCommand("/sbin/shutdown -r now >/dev/null 2>&1");
            }
            catch (Exception e)
            {
                this._errors.Add(e.Message);
                result.Add(e.Message);
            }

            if (this._client.IsConnected)
            {
                this.Disconnect();
            }

            this._connectionEstablished = false;

            return result;
        }
    }
}
