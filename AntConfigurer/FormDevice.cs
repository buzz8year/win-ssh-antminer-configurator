using AntConfigurer.Objects;
using System;
using System.Net;
using System.Collections.Generic;
using System.Windows.Forms;
using AntConfigurer.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Specialized;
using System.Text;

namespace AntConfigurer
{
    public partial class FormDevice : Form
    {
        protected bool AutoDiscoveringIsActive = false;

        public FormDevice()
        {
            InitializeComponent();
            UdpListener.Winform = this;
            UdpListener.Log = log_box;
        }

        private void Auto_discover_button_Click(object sender, EventArgs e)
        {
            var button = sender as Button;
            var currentListenerState = UdpListener.IsListening();
            var log = log_box;

            var deviceTable = device_list_table;
            var hnameUploadTable = hostname_upload_panel;
            
            // device_table.Controls.Clear();

            if (currentListenerState)
            {
                pingator_button.Enabled = true;
                
                // UDP listener already running. We need to halt execution and dump the results
                log.AppendText("Halting IP reporting tool...\r\n");
                UdpListener.Stop();
                
                log.AppendText("IP reporting tool halted\r\n");
                button.Text = "Search for devices";

                // Clearing out old data
                while (deviceTable.Controls.Count > 0)
                    deviceTable.Controls[0].Dispose();

                while (hnameUploadTable.Controls.Count > 0)
                    hnameUploadTable.Controls[0].Dispose();

                // Resetting styles
                deviceTable.RowCount = 0;
                deviceTable.ColumnCount = 6;
                deviceTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 10F));//Checkbox row
                deviceTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 10F));//Num row
                deviceTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));//ASIC Detected IP
                deviceTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));//ASIC Received IP
                deviceTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));//ASIC MAC Addr
                deviceTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 5F));//ASIC Confirmed
                // device_table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, F));//ASIC Confirmed

                deviceTable.RowCount = deviceTable.RowCount + 1;
                deviceTable.RowStyles.Add(new RowStyle(SizeType.AutoSize, 15F));
                // device_table.RowStyles.Add(new RowStyle(SizeType.Absolute, 15F));
                
                var allSelect = new CheckBox
                {
                    Name = "select_all_devices_checkbox"
                };
                allSelect.Click += new System.EventHandler(this.All_select_click);

                deviceTable.Controls.Add(allSelect, 0, deviceTable.RowCount - 1);
                deviceTable.Controls.Add(new Label() { Text = "№" }, 1, deviceTable.RowCount - 1);
                deviceTable.Controls.Add(new Label() { Text = "Detected IP" }, 2, deviceTable.RowCount - 1);
                deviceTable.Controls.Add(new Label() { Text = "Received IP" }, 3, deviceTable.RowCount - 1);
                deviceTable.Controls.Add(new Label() { Text = "Mac addr" }, 4, deviceTable.RowCount - 1);
                deviceTable.Controls.Add(new Label() { Text = "Device confirmed" }, 5, deviceTable.RowCount - 1);

                hnameUploadTable.RowCount = 0;
                hnameUploadTable.ColumnCount = 7;

                hnameUploadTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 5F)); // Num Row
                hnameUploadTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 5F)); // Num Row
                hnameUploadTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F)); // ASIC Detected IP
                hnameUploadTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F)); // ASIC Received IP
                hnameUploadTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F)); // ASIC MAC Addr
                hnameUploadTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 5F)); // ASIC Confirmed
                hnameUploadTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 10F)); // ASIC Hostname

                hnameUploadTable.RowCount = hnameUploadTable.RowCount + 1;
                hnameUploadTable.RowStyles.Add(new RowStyle(SizeType.AutoSize, 30F));

                var allSelectHname = new CheckBox
                {
                    Name = "select_all_devices_hname_checkbox"
                };
                allSelectHname.Click += new System.EventHandler(this.All_select_hname_click);

                hnameUploadTable.Controls.Add(allSelectHname, 0, hnameUploadTable.RowCount - 1);
                hnameUploadTable.Controls.Add(new Label() { Text = "№" }, 1, hnameUploadTable.RowCount - 1);
                hnameUploadTable.Controls.Add(new Label() { Text = "Detected IP" }, 2, hnameUploadTable.RowCount - 1);
                hnameUploadTable.Controls.Add(new Label() { Text = "Received IP" }, 3, hnameUploadTable.RowCount - 1);
                hnameUploadTable.Controls.Add(new Label() { Text = "Mac addr" }, 4, hnameUploadTable.RowCount - 1);
                hnameUploadTable.Controls.Add(new Label() { Text = "Device confirmed" }, 5, hnameUploadTable.RowCount - 1);
                hnameUploadTable.Controls.Add(new Label() { Text = "ASIC hostname" }, 6, hnameUploadTable.RowCount - 1);
                
                var foundDevices = UdpListener.GetDevices();

                #if DEBUG
                    for (var i = 0; i < 20; i++)
                    {
                        var testDevice = new AsicDevice
                        {
                            Confirmed = true,
                            IpAddr = "109.234.34.6",
                            MacAddr = "0F:89:33:A1:04",
                            RealIpAddr = "109.234.34.6"
                        };
                        foundDevices.Add(testDevice);
                    }
                #endif

                foreach (var device in foundDevices)
                {
                    deviceTable.RowCount = deviceTable.RowCount + 1;
                    deviceTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
                    
                    var asicCheckbox = new CheckBox
                    {
                        Name = "asic_checkbox_" + (deviceTable.RowCount - 1).ToString()
                    };
                    
                    deviceTable.Controls.Add(asicCheckbox, 0, deviceTable.RowCount - 1);
                    deviceTable.Controls.Add(new Label() { Text = (deviceTable.RowCount - 1).ToString() }, 1, deviceTable.RowCount - 1);
                    deviceTable.Controls.Add(new Label() { Text = device.IpAddr }, 2, deviceTable.RowCount - 1);
                    deviceTable.Controls.Add(new Label() { Text = device.RealIpAddr }, 3, deviceTable.RowCount - 1);
                    deviceTable.Controls.Add(new Label() { Text = device.MacAddr }, 4, deviceTable.RowCount - 1);
                    deviceTable.Controls.Add(new Label() { Text = device.Confirmed ? "Yes" : "No" }, 5, deviceTable.RowCount - 1);

                    hnameUploadTable.RowCount = hnameUploadTable.RowCount + 1;
                    hnameUploadTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));

                    var asicHnameCheckbox = new CheckBox
                    {
                        Name = "asic_hname_checkbox_" + (deviceTable.RowCount - 1).ToString()
                    };

                    hnameUploadTable.Controls.Add(asicHnameCheckbox, 0, hnameUploadTable.RowCount - 1);
                    hnameUploadTable.Controls.Add(new Label() { Text = (hnameUploadTable.RowCount - 1).ToString() }, 1, hnameUploadTable.RowCount - 1);
                    hnameUploadTable.Controls.Add(new Label() { Text = device.IpAddr }, 2, hnameUploadTable.RowCount - 1);
                    hnameUploadTable.Controls.Add(new Label() { Text = device.RealIpAddr }, 3, hnameUploadTable.RowCount - 1);
                    hnameUploadTable.Controls.Add(new Label() { Text = device.MacAddr }, 4, hnameUploadTable.RowCount - 1);
                    hnameUploadTable.Controls.Add(new Label() { Text = device.Confirmed ? "Yes" : "No" }, 5, hnameUploadTable.RowCount - 1);
                    hnameUploadTable.Controls.Add(new TextBox() { Name = "hname_upload_val_" + (hnameUploadTable.RowCount - 1) }, 6, hnameUploadTable.RowCount - 1);
                }

                Console.WriteLine(deviceTable.RowCount);
                deviceTable.AutoScrollMinSize = new System.Drawing.Size(50, 20 * (deviceTable.RowCount + 1));
                hnameUploadTable.AutoScrollMinSize = new System.Drawing.Size(50, 20 * (hnameUploadTable.RowCount + 1));
            }
            else
            {
                log.Text = "";
                log.AppendText("Starting IP reporting tool...\r\n");
                button.Text = "Halt search";

                pingator_button.Enabled = false;
                UdpListener.Start();
            }
        }

        private void All_select_click(object sender, EventArgs e)
        {
            var checkbox = sender as CheckBox;
            var otherControls = device_list_table.Controls;
            var isChecked = checkbox.Checked;

            foreach (Control control in otherControls)
            {
                if (control.ToString().StartsWith("System.Windows.Forms.CheckBox"))
                {
                    var chkbx = control as CheckBox;
                    
                    if (!chkbx.Name.Equals("select_all_devices_checkbox"))
                        chkbx.Checked = isChecked;
                }
            }
        }

        private void All_select_hname_click(object sender, EventArgs e)
        {
            var checkbox = sender as CheckBox;
            var otherControls = hostname_upload_panel.Controls;
            var isChecked = checkbox.Checked;

            foreach (Control control in otherControls)
            {
                if (control.ToString().StartsWith("System.Windows.Forms.CheckBox"))
                {
                    var chkbx = control as CheckBox;
                    
                    if (!chkbx.Name.Equals("select_all_devices_hname_checkbox"))
                        chkbx.Checked = isChecked;
                }
            }
        }

        private void Ds_wr_settings_Click(object sender, EventArgs e)
        {
            var btn = sender as Button;

            String ipAddress = ds_ip_address.Text;
            String login = ds_username.Text;
            String pass = ds_password.Text; // idQ6Jk5XEPAqp8tg

            // Validating an IP address
            if (!Strings.ValidateIpV4(ipAddress))
            {
                var errorsAsString = String.Join(Environment.NewLine, Strings.GetLastErrors());
                MessageBox.Show(errorsAsString, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Validating login
            if (String.IsNullOrWhiteSpace(login))
            {
                MessageBox.Show("Login is not provided", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            uploader_state_tb.Text = "";

            // Password is not required, so, we'll skip check
            // And will check ASIC pool settings
            var tabControls = direct_connect_tab.Controls;
            List<ConfigElement> configs = new List<ConfigElement>();
            
            for (Int32 i = 1; i <= 3; i++)
            {
                String poolUrl = tabControls.Find("ds_url_" + i, true)[0].Text,
                       poolUser = tabControls.Find("ds_user_" + i, true)[0].Text,
                       poolWorkerName = tabControls.Find("ds_worker_" + i, true)[0].Text,
                       poolPass = tabControls.Find("ds_pass_" + i, true)[0].Text;

                var config = new ConfigElement();
                config.SetAsicIp(ipAddress).SetUrl(poolUrl).SetWorker(poolWorkerName).SetPass(poolPass).SetUsername(poolUser);

                if (!config.Check(true))
                {
                    var errorsAsString = "An error occured at config for pool " + i + ": " + String.Join(Environment.NewLine, config.GetErrors());
                    MessageBox.Show(errorsAsString, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                else configs.Add(config);
            }

            // All configs are set and verified. Let's convert them to JSON and check the connection to our device
            SshConnector connector = new SshConnector();
            connector.SetIpAddress(ipAddress).SetUsername(login).SetPassword(pass);

            if (!connector.Connect())
            {
                var errorsAsString = "An error occured while connecting to " + login + '@' + ipAddress + ": " + String.Join(Environment.NewLine, connector.GetErrors());
                MessageBox.Show(errorsAsString, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            else
            {
                var result = connector.UploadConfig(configs);
                
                foreach (String line in result)
                    uploader_state_tb.AppendText(line + "\r\n");
            }
        }

        private void selected_upload_Click(object sender, EventArgs e)
        {
            var btn = sender as Button;
            var otherControls = device_list_table.Controls;

            // Locking button
            btn.Enabled = false;

            List<CheckBox> checkboxes = new List<CheckBox>();
            
            foreach (Control control in otherControls)
            {
                if (control.ToString().StartsWith("System.Windows.Forms.CheckBox"))
                {
                    var chkbx = control as CheckBox;
                    
                    if (!chkbx.Name.Equals("select_all_devices_checkbox") && chkbx.Checked)
                        checkboxes.Add(chkbx);
                }
            }

            if (checkboxes.Count == 0)
            {
                MessageBox.Show("Please, select at least one device", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                btn.Enabled = true;
                return;
            }

            dl_stat_box.Text = "";
            List<String> ips = new List<string>();
            
            foreach (CheckBox chkbx in checkboxes)
            {
                var spliitedName = chkbx.Name.Split('_');
                int index = Int32.Parse(spliitedName[spliitedName.Length - 1]);

                var ipLabel = device_list_table.GetControlFromPosition(3, index) as Label;
                String ipAddress = ipLabel.Text;
                
                if (!Strings.ValidateIpV4(ipAddress))
                    dl_stat_box.AppendText("Couldn't parse an ip. IP was " + ipAddress + "\r\n");
                
                else ips.Add(ipAddress);
            }

            if (ips.Count < 1)
            {
                MessageBox.Show("No valid IPs were provided", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                btn.Enabled = true;
                return;
            }

            String login = dev_list_login.Text;
            String pass = dev_list_pass.Text;

            if (String.IsNullOrWhiteSpace(login))
            {
                MessageBox.Show("Login is not provided", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                btn.Enabled = true;
                return;
            }

            Boolean shouldNotify = notify_monitoring.Checked;
            String url = monitoring_url.Text;
            
            if (shouldNotify && String.IsNullOrWhiteSpace(url))
            {
                var message = "To notify monitoring, you need to provide monitoring URL";
                MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                btn.Enabled = true;
                return;
            }
            var selectedLocation = asics_location.SelectedIndex;
            
            if (shouldNotify)
            {
                if (selectedLocation < 1)
                {
                    var message = "Please, select location of your devices";
                    MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    btn.Enabled = true;
                    return;
                }
            }

            String commonHostname = ds_common_hostname.Text;
            String commonHostnamePref = dl_common_hname_pref.Text;

            if (!String.IsNullOrWhiteSpace(commonHostnamePref))
                commonHostname = commonHostnamePref + commonHostname;

            var tabControls = devices_tab.Controls;
            List<ConfigElement> configs = new List<ConfigElement>();
            
            for (Int32 i = 1; i <= 3; i++)
            {
                String poolUrl = tabControls.Find("dl_url_" + i, true)[0].Text,
                       poolUser = tabControls.Find("dl_user_" + i, true)[0].Text,
                       poolWorkerName = tabControls.Find("dl_worker_" + i, true)[0].Text,
                       poolWorkerPreffix = tabControls.Find("dl_preffix_" + i, true)[0].Text,
                       poolWorkerSuffix = tabControls.Find("dl_suffix_" + i, true)[0].Text,
                       poolPass = tabControls.Find("dl_pass_" + i, true)[0].Text;

                var config = new ConfigElement();
                config.SetUrl(poolUrl)
                    .SetWorker(poolWorkerName)
                    .SetPass(poolPass)
                    .SetUsername(poolUser)
                    .SetPrefix(poolWorkerPreffix)
                    .SetSuffix(poolWorkerSuffix);

                if (!config.Check(true, true))
                {
                    var errorsAsString = "An error occured at config for pool " + i + ": " + String.Join(Environment.NewLine, config.GetErrors());
                    MessageBox.Show(errorsAsString, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    btn.Enabled = true;
                    return;
                }
                else configs.Add(config);
            }

            // Should we override old settings or just skip them if bmmine.conf.backup exists?
            Boolean overrideOld = override_old_settings.Checked;
            
            String overriderText = overrideOld 
                ? "Old configs will be replaced\r\n" 
                : "Program will skip device if bmmine.conf.backup exists\r\n";

            dl_stat_box.AppendText(overriderText);
            dl_stat_box.AppendText("Ok, checklist passed. Starting config upload...\r\n");
            
            // Dictionary<String, List<CompletedConfig>> uploaded_configs = new Dictionary<string, List<CompletedConfig>>();
            List<CompletedConfig> configs2Server = new List<CompletedConfig>();
            
            foreach (String currentIp in ips)
            {
                try
                {
                    dl_stat_box.AppendText("Processing ASIC with IP " + currentIp + ". Checking connection...\r\n");
                    SshConnector connector = new SshConnector();
                    
                    connector.SetIpAddress(currentIp)
                        .SetUsername(login)
                        .SetPassword(pass)
                        .SetCommonHname(commonHostname);

                    if (!connector.Connect())
                    {
                        var errorsAsString = "An error occured while connecting to " + login + '@' + currentIp + ": " + String.Join(Environment.NewLine, connector.GetErrors());
                        dl_stat_box.AppendText(errorsAsString + "\r\n");
                        continue;
                    }
                    dl_stat_box.AppendText("Connection established. Starting config upload to " + currentIp + "\r\n");
                    
                    /*
                    foreach (ConfigElement config in configs)
                        config.SetAsicIp(_current_ip).GenerateWorkerName(_current_ip);
                    */
                    var result = connector.UploadConfig(configs, overrideOld);
                    
                    foreach (String line in result)
                        dl_stat_box.AppendText(line + "\r\n");

                    if (connector.GetShouldNotifyMonitoring() && connector.GetLastFullConfig() != null)
                        configs2Server.Add(connector.GetLastFullConfig());
                }
                catch (Exception exc)
                {
                    var errorsAsString = "Uncatched error has occured while processing " + currentIp + ". Error was: " + exc.Message + ". Stack trace: " + exc.StackTrace + "\r\n";
                    // MessageBox.Show(errors_as_string, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    
                    dl_stat_box.AppendText(errorsAsString);
                }
            }

            // Starting upload configs to server if we have any
            if (shouldNotify && configs2Server.Count > 0)
            {
                dl_stat_box.AppendText("Sending data to monitoring system...\r\n");
                List<JsonDevice> convertedConfigs = new List<JsonDevice>();
                
                foreach (CompletedConfig conf in configs2Server)
                {
                    // converted_configs.Add(new JsonDevice(conf));
                    var jsonDevice = new JsonDevice(conf);
                    
                    jsonDevice.Location = selectedLocation.ToString();
                    convertedConfigs.Add(jsonDevice);
                }

                var jsonString = JsonConvert.SerializeObject(convertedConfigs);
                dl_stat_box.AppendText("Upload completed!\r\n");

                if (!url.StartsWith("http://") && !url.StartsWith("https://"))
                    url = "http://" + url;

                using (var client = new WebClient())
                {
                    try
                    {
                        var values = new NameValueCollection();
                        values["configs"] = jsonString;

                        client.Credentials = new NetworkCredential("bigfive", "1000000$");
                        var response = client.UploadValues(url + "/Miners/ConfiguredDevices/Add/", values);

                        var responseString = Encoding.UTF8.GetString(response);

                        if (String.IsNullOrWhiteSpace(responseString))
                            throw new Exception("Empty server response received");

                        dynamic parsedString = JsonConvert.DeserializeObject(responseString);

                        dynamic content = parsedString.content.result ?? null;
                        dynamic totalCount = parsedString.content.total_dev_count ?? "0";
                        dynamic procCount = parsedString.content.processed_dev_count ?? "0";

                        dl_stat_box.AppendText("Total number of recogonized devices - " + totalCount + "\r\n");
                        dl_stat_box.AppendText("Total number of processed devices - " + totalCount + "\r\n");

                    }
                    catch (Exception exc)
                    {
                        var message = exc.Message;
                        MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            btn.Enabled = true;
        }

        private void auto_detector_button_Click(object sender, EventArgs e)
        {
            MessageBox.Show("This function is under development and will be implemented in future releases", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void notify_monitoring_CheckedChanged(object sender, EventArgs e)
        {
            var chckbx = sender as CheckBox;

            if (chckbx.Checked)
            {
                monitoring_url.Enabled = true;
                asics_location.Enabled = true;
                retrieve_location.Enabled = true;
            }
            else
            {
                monitoring_url.Enabled = false;
                asics_location.Enabled = false;
                retrieve_location.Enabled = false;
            }
        }

        private void retrieve_location_Click(object sender, EventArgs e)
        {
            var btn = sender as Button;
            var url = monitoring_url.Text;

            if (String.IsNullOrWhiteSpace(url))
            {
                var message = "To retrieve available locations, you need to provide monitoring URL";
                MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!url.StartsWith("http://") && !url.StartsWith("https://"))
                url = "http://" + url;

            // Make an attempt to connect to a monitoring service
            using (var client = new WebClient())
            {
                client.Credentials = new NetworkCredential("bigfive", "1000000$");
                var result = client.DownloadString(url + "/Api/Locations/List");

                if (String.IsNullOrWhiteSpace(result))
                {
                    var message = "An error occured while trying to connect to monitoring service";
                    MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                JObject parsed = JObject.Parse(result);
                var childrens = parsed.Children();

                Dictionary<string, string> locations = new Dictionary<string, string>();
                JArray locationsAsJson = null;
                
                using (var sequenceEnum = childrens.GetEnumerator())
                {
                    while (sequenceEnum.MoveNext())
                    {
                        JObject objValue;
                        
                        // Do something with sequenceEnum.Current.
                        var currentElement = sequenceEnum.Current as JProperty;
                        
                        switch (currentElement.Name)
                        {
                            case "content":
                                objValue = currentElement.Value as JObject;
                                if (objValue != null)
                                    locationsAsJson = objValue["locations"] as JArray;
                                break;
                        }

                        if (locationsAsJson != null) 
                            break;
                    }
                }
                asics_location.DataSource = null;

                if (locationsAsJson == null)
                {
                    var message = "No locations were received from server";
                    MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var parsedLocations = locationsAsJson.ToObject<List<Location>>();
                var locationsListCtrl = asics_location;
                var emptyLocation = new Location();
                
                emptyLocation.Id = 0;
                emptyLocation.Name = "Select one";
                parsedLocations.Insert(0, emptyLocation);

                /*
                foreach (Location location in parsed_locations)
                    asics_location.Items.Insert(location.id.ToString(), location.name);
                */
                
                locationsListCtrl.DataSource = new BindingSource(parsedLocations, null);
                locationsListCtrl.DisplayMember = "Name";
                locationsListCtrl.ValueMember = "Id";
            }
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            var button = sender as Button;
            var log = log_box;

            var leftIpRange = left_ip_boundary.Text;
            var rightIpRange = right_ip_boundary.Text;

            if (!Strings.ValidateIpV4(leftIpRange))
            {
                MessageBox.Show("Left IP boundary is an incorrect IP address", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!Strings.ValidateIpV4(rightIpRange))
            {
                MessageBox.Show("Right IP boundary is an incorrect IP address", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            leftIpRange = leftIpRange.Trim();
            rightIpRange = rightIpRange.Trim();

            /*
            var cmp = left_ip_range.CompareTo(right_ip_range);
            if (cmp > 0)
            {
                // Why are you trying to break me?
                var tmp = left_ip_range;
                left_ip_range = right_ip_range;
                right_ip_range = tmp;
            }
            */

            var login = autodetector_login.Text;
            var pass = autodetector_pass.Text;

            if (String.IsNullOrWhiteSpace(login))
            {
                MessageBox.Show("Please, provide login", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            log.Text = "";
            log.AppendText("Discovering ASIC devices in automatic mode...\r\n");
            Dictionary<String, String> ip2MacList = new Dictionary<string, string>();
            
            try
            {
                button.Enabled = false;

                String[] leftOctets = leftIpRange.Split('.');
                String[] rightOctets = rightIpRange.Split('.');

                var connector = new SshConnector();
                // Console.WriteLine(left_ip_range);
                // Console.WriteLine(right_ip_range);

                for (var octet1 = Int32.Parse(leftOctets[0]); octet1 <= Int32.Parse(rightOctets[0]); octet1++)
                {
                    for (var octet2 = Int32.Parse(leftOctets[1]); octet2 <= Int32.Parse(rightOctets[1]); octet2++)
                    {
                        for (var octet3 = Int32.Parse(leftOctets[2]); octet3 <= Int32.Parse(rightOctets[2]); octet3++)
                        {
                            for (var octet4 = Int32.Parse(leftOctets[3]); octet4 <= Int32.Parse(rightOctets[3]); octet4++)
                            {
                                String currentIp = octet1.ToString() + "." + octet2.ToString() + "." + octet3.ToString() + "." + octet4.ToString();

                                connector = new SshConnector();
                                connector.SetIpAddress(currentIp).SetUsername(login).SetPassword(pass);
                                Console.WriteLine("Processing IP " + currentIp);

                                var result = connector.InspectMachine();
                                
                                if (!String.IsNullOrWhiteSpace(result))
                                {
                                    ip2MacList.Add(currentIp, result);
                                    log.AppendText("Device found at IP: " + currentIp + " + with MAC address + '" + result + "'\r\n");
                                }
                                else Console.WriteLine("Device found, but it's not an ASIC device at " + currentIp);
                            }
                        }
                    }
                }

                var deviceTable = device_list_table;
                var hnameUploadTable = hostname_upload_panel;
                
                // Clearing out old data
                while (deviceTable.Controls.Count > 0)
                    deviceTable.Controls[0].Dispose();

                while (hnameUploadTable.Controls.Count > 0)
                    hnameUploadTable.Controls[0].Dispose();

                // Resetting styles
                deviceTable.RowCount = 0;
                deviceTable.ColumnCount = 6;
                deviceTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 5F)); // Checkbox Row
                deviceTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 5F)); // Num Row
                deviceTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F)); // ASIC Detected IP
                deviceTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F)); // ASIC Received IP
                deviceTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F)); // ASIC MAC Addr
                deviceTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 5F)); // ASIC Confirmed
                // device_table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, F)); // ASIC Confirmed

                deviceTable.RowCount = deviceTable.RowCount + 1;
                deviceTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 15F));
                
                var allSelect = new CheckBox
                {
                    Name = "select_all_devices_checkbox"
                };
                allSelect.Click += new System.EventHandler(this.All_select_click);

                deviceTable.Controls.Add(allSelect, 0, deviceTable.RowCount - 1);
                deviceTable.Controls.Add(new Label() { Text = "№" }, 1, deviceTable.RowCount - 1);
                deviceTable.Controls.Add(new Label() { Text = "Detected IP" }, 2, deviceTable.RowCount - 1);
                deviceTable.Controls.Add(new Label() { Text = "Received IP" }, 3, deviceTable.RowCount - 1);
                deviceTable.Controls.Add(new Label() { Text = "Mac addr" }, 4, deviceTable.RowCount - 1);
                deviceTable.Controls.Add(new Label() { Text = "Device confirmed" }, 5, deviceTable.RowCount - 1);

                hnameUploadTable.RowCount = 0;
                hnameUploadTable.ColumnCount = 7;

                hnameUploadTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 5F)); // Num Row
                hnameUploadTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 5F)); // Num Row
                hnameUploadTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F)); // ASIC Detected IP
                hnameUploadTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F)); // ASIC Received IP
                hnameUploadTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F)); // ASIC MAC Addr
                hnameUploadTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 5F)); // ASIC Confirmed
                hnameUploadTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 10F)); // ASIC Hostname

                hnameUploadTable.RowCount = hnameUploadTable.RowCount + 1;
                hnameUploadTable.RowStyles.Add(new RowStyle(SizeType.AutoSize, 30F));

                var allSelectHname = new CheckBox
                {
                    Name = "select_all_devices_hname_checkbox"
                };
                allSelectHname.Click += new System.EventHandler(this.All_select_hname_click);

                hnameUploadTable.Controls.Add(allSelectHname, 0, hnameUploadTable.RowCount - 1);
                hnameUploadTable.Controls.Add(new Label() { Text = "№" }, 1, hnameUploadTable.RowCount - 1);
                hnameUploadTable.Controls.Add(new Label() { Text = "Detected IP" }, 2, hnameUploadTable.RowCount - 1);
                hnameUploadTable.Controls.Add(new Label() { Text = "Received IP" }, 3, hnameUploadTable.RowCount - 1);
                hnameUploadTable.Controls.Add(new Label() { Text = "Mac addr" }, 4, hnameUploadTable.RowCount - 1);
                hnameUploadTable.Controls.Add(new Label() { Text = "Device confirmed" }, 5, hnameUploadTable.RowCount - 1);
                hnameUploadTable.Controls.Add(new Label() { Text = "ASIC hostname" }, 6, hnameUploadTable.RowCount - 1);

                // var found_devices = UdpListener.GetDevices();
                foreach (KeyValuePair<string, string> entry in ip2MacList)
                {
                    deviceTable.RowCount = deviceTable.RowCount + 1;
                    deviceTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
                    
                    var asicCheckbox = new CheckBox
                    {
                        Name = "asic_checkbox_" + (deviceTable.RowCount - 1).ToString()
                    };
                    
                    deviceTable.Controls.Add(asicCheckbox, 0, deviceTable.RowCount - 1);
                    deviceTable.Controls.Add(new Label() { Text = (deviceTable.RowCount - 1).ToString() }, 1, deviceTable.RowCount - 1);
                    deviceTable.Controls.Add(new Label() { Text = entry.Key }, 2, deviceTable.RowCount - 1);
                    deviceTable.Controls.Add(new Label() { Text = entry.Key }, 3, deviceTable.RowCount - 1);
                    deviceTable.Controls.Add(new Label() { Text = entry.Value }, 4, deviceTable.RowCount - 1);
                    deviceTable.Controls.Add(new Label() { Text = "No" }, 5, deviceTable.RowCount - 1);
                    
                    hnameUploadTable.RowCount = hnameUploadTable.RowCount + 1;
                    hnameUploadTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
                    
                    var asicHnameCheckbox = new CheckBox
                    {
                        Name = "asic_hname_checkbox_" + (deviceTable.RowCount - 1).ToString()
                    };

                    hnameUploadTable.Controls.Add(asicHnameCheckbox, 0, hnameUploadTable.RowCount - 1);
                    hnameUploadTable.Controls.Add(new Label() { Text = (hnameUploadTable.RowCount - 1).ToString() }, 1, hnameUploadTable.RowCount - 1);
                    hnameUploadTable.Controls.Add(new Label() { Text = entry.Key }, 2, hnameUploadTable.RowCount - 1);
                    hnameUploadTable.Controls.Add(new Label() { Text = entry.Key }, 3, hnameUploadTable.RowCount - 1);
                    hnameUploadTable.Controls.Add(new Label() { Text = entry.Value }, 4, hnameUploadTable.RowCount - 1);
                    hnameUploadTable.Controls.Add(new Label() { Text = "No" }, 5, hnameUploadTable.RowCount - 1);
                    hnameUploadTable.Controls.Add(new TextBox() { Name = "hname_upload_val_" + (hnameUploadTable.RowCount - 1) }, 6, hnameUploadTable.RowCount - 1);
                }
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message, "Critical error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Console.WriteLine(exc.Message);
                return;
            }
            log.AppendText("Total number of found devices: " + ip2MacList.Count + "\r\n");

            button.Enabled = true;
        }

        private void upload_hostname_button_Click(object sender, EventArgs e)
        {
            var button = sender as Button;
            var otherControls = hostname_upload_panel.Controls;

            // Locking button
            button.Enabled = false;

            List<CheckBox> checkboxes = new List<CheckBox>();
            List<HostnameElement> hostnames = new List<HostnameElement>();
            
            foreach (Control control in otherControls)
            {
                if (control.ToString().StartsWith("System.Windows.Forms.CheckBox"))
                {
                    var chkbx = control as CheckBox;
                    
                    if (!chkbx.Name.Equals("select_all_devices_hname_checkbox") && chkbx.Checked)
                        checkboxes.Add(chkbx);
                }
            }

            if (checkboxes.Count == 0)
            {
                MessageBox.Show("Please, select at least one device", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                button.Enabled = true;
                return;
            }

            hname_upload_stat_box.Text = "";
            var commonHostname = comm_hostname_text.Text;
            int i = 1;
            
            foreach (CheckBox chkbx in checkboxes)
            {
                var spliitedName = chkbx.Name.Split('_');
                int index = Int32.Parse(spliitedName[spliitedName.Length - 1]);

                var ipLabel = hostname_upload_panel.GetControlFromPosition(3, index) as Label;
                String ipAddress = ipLabel.Text;
                
                HostnameElement element = new HostnameElement();
                element.SetCtr(i++);
                
                if (!Strings.ValidateIpV4(ipAddress))
                {
                    hname_upload_stat_box.AppendText("Couldn't parse an ip. IP was " + ipAddress + "\r\n");
                    continue;
                }
                String newHostname = "";
                
                if (String.IsNullOrWhiteSpace(commonHostname))
                {
                    newHostname = (hostname_upload_panel.GetControlFromPosition(6, index) as TextBox).Text;
                    
                    if (String.IsNullOrWhiteSpace(newHostname))
                    {
                        hname_upload_stat_box.AppendText("Empty hostname for item + " + element.GetCtr() + "\r\n");
                        continue;
                    }
                }
                else newHostname = commonHostname;

                element.SetIpAddress(ipAddress).SetHostname(newHostname);
                hostnames.Add(element);
            }

            if (hostnames.Count < 1)
            {
                MessageBox.Show("No valid device were provided", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                button.Enabled = true;
                return;
            }

            String login = hname_upload_login.Text;
            String pass = hname_upload_pass.Text;

            if (String.IsNullOrWhiteSpace(login))
            {
                MessageBox.Show("Login is not provided", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                button.Enabled = true;
                return;
            }

            foreach (var element in hostnames)
            {
                try
                {
                    hname_upload_stat_box.AppendText("Processing ASIC with IP " + element.GetIpAddress() + ". Checking connection...\r\n");
                    SshConnector connector = new SshConnector();
                    connector.SetIpAddress(element.GetIpAddress()).SetUsername(login).SetPassword(pass).SetCommonHname(commonHostname);

                    if (!connector.Connect())
                    {
                        var errorsAsString = "An error occured while connecting to " + login + '@' + element.GetIpAddress() + ": " + String.Join(Environment.NewLine, connector.GetErrors());
                        hname_upload_stat_box.AppendText(errorsAsString + "\r\n");
                        continue;
                    }

                    hname_upload_stat_box.AppendText("Connection established. Starting network config upload to " + element.GetIpAddress() + "\r\n");
                    
                    /*
                    foreach (ConfigElement config in configs)
                        config.SetAsicIp(_current_ip).GenerateWorkerName(_current_ip);
                    */
                    var result = connector.UploadNetworkConfig(element, false);
                    
                    foreach (String line in result)
                        hname_upload_stat_box.AppendText(line + "\r\n");
                }
                catch (Exception exc)
                {
                    var errorsAsString = "Uncatched error has occured while processing " + element.GetIpAddress() + ". Error was: " + exc.Message + ". Stack trace: " + exc.StackTrace + "\r\n";
                    // MessageBox.Show(errors_as_string, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    
                    hname_upload_stat_box.AppendText(errorsAsString);
                }
            }
            button.Enabled = true;
        }
    }
}
