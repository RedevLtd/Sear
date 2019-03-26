
using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Xml.Serialization;
using Newtonsoft.Json;
using SearAlertingServiceCore.Actions;

namespace SearAlertingServiceCore
{
    class Program
    {
        private static readonly log4net.ILog _logger = LogManager.GetLogger(typeof(Program));
        private static readonly AutoResetEvent waitHandle = new AutoResetEvent(false);

        static void Main(string[] args)
        {
            System.IO.Directory.SetCurrentDirectory(System.AppDomain.CurrentDomain.BaseDirectory);
            SearConfig config = SearConfig.ReadConfig();

            // set up logger
            var repo = log4net.LogManager.CreateRepository(Assembly.GetEntryAssembly(), typeof(log4net.Repository.Hierarchy.Hierarchy));
            log4net.Config.XmlConfigurator.Configure(repo, new FileInfo("log4net.config"));

            _logger.Info("SearAlertingService Starting");           
            _logger.InfoFormat("Looking for Alert configs in: {0}", config.AlertsFolderPath);

            List <Alert> alerts = ReadAlerts(config.AlertsFolderPath);

            if (alerts.Count == 0)
                _logger.Error("Found no valid Alert rules. Exiting....");

            SearClient sc = new SearClient(config, alerts);
            sc.Start();

            Console.CancelKeyPress += (o, e) =>
            {
                Console.WriteLine("Exit");
                // Allow the main thread to continue and exit...
                waitHandle.Set();
            };

            waitHandle.WaitOne();

            sc.Stop();
        }

        /// <summary>
        /// Attempts to read in alert configs for the specified directory
        /// </summary>
        /// <param name="alertsFolderPath"></param>
        /// <returns></returns>
        private static List<Alert> ReadAlerts(string alertsFolderPath)
        {
            List<Alert> alerts = new List<Alert>();
            foreach (var file in Directory.GetFiles(alertsFolderPath).Where(t => t.ToLower().EndsWith(".json")))
            {
                try
                {
                    Alert alert = JsonConvert.DeserializeObject<Alert>(File.ReadAllText(file),
                        new JsonSerializerSettings() {TypeNameHandling = TypeNameHandling.Auto});

                    _logger.InfoFormat("Adding Alert: {0}, Interval: {1}, ActionType: {2}", alert.Name, alert.Interval,
                        alert.Actions[0].ActionType);
                    alerts.Add(alert);

                }
                catch (Exception ex)
                {
                    _logger.Error("Error reading alert config. Error: " + ex.Message, ex);
                }
            }

            return alerts;
        }
    }
}
