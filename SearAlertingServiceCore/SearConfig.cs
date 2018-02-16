using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace SearAlertingServiceCore
{
    public class SearConfig
    {
        public string AlertsFolderPath { get; set; } = "Alerts";       

        private const string CONFIG_FILE_PATH = "SearConfig.config";
        private static readonly log4net.ILog _logger = LogManager.GetLogger(typeof(SearConfig));

        public static SearConfig ReadConfig()
        {
            try
            {
                if (File.Exists(CONFIG_FILE_PATH))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(SearConfig));
                    StreamReader reader = new StreamReader(CONFIG_FILE_PATH);
                    SearConfig config = (SearConfig)serializer.Deserialize(reader);
                    reader.Close();

                    return config;
                }
            }
            catch(Exception ex)
            {
                _logger.Error("Error reading SearConfig. Error: " + ex.Message, ex);
            }

            return new SearConfig();
        }
    }
}
