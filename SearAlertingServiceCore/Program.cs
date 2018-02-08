
using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Xml.Serialization;

namespace SearAlertingServiceCore
{
    class Program
    {
        private static readonly log4net.ILog _logger = LogManager.GetLogger(typeof(Program));
        private static readonly AutoResetEvent waitHandle = new AutoResetEvent(false);

        static void Main(string[] args)
        {
            System.IO.Directory.SetCurrentDirectory(System.AppDomain.CurrentDomain.BaseDirectory);
            List<Alert> alerts = new List<Alert>();

            // set up logger
            var repo = log4net.LogManager.CreateRepository(Assembly.GetEntryAssembly(), typeof(log4net.Repository.Hierarchy.Hierarchy));
            log4net.Config.XmlConfigurator.Configure(repo, new FileInfo("log4net.config"));

            _logger.Info("SearAlertingService Starting");

            string alertsFolder = string.IsNullOrWhiteSpace(args[0]) ? "Alerts" : args[0];
            _logger.InfoFormat("Looking for Alert configs in: {0}", alertsFolder);

            foreach (var file in Directory.GetFiles(alertsFolder))
            {
                try
                {
                    if (file.EndsWith(".xml"))
                    {
                        XmlSerializer serializer = new XmlSerializer(typeof(Alert));
                        StreamReader reader = new StreamReader(file);
                        var alert = (Alert)serializer.Deserialize(reader);
                        reader.Close();

                        _logger.InfoFormat("Adding Alert: {0}, Interval: {1}, ActionType: {2}", alert.Name, alert.Interval, alert.ActionType);

                        alerts.Add(alert);
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error("Error reading alert config. Error: " + ex.Message, ex);
                }
            }

            if (alerts.Count == 0)
            {
                _logger.Error("Found no valid Alert rules. Exiting....");
                Environment.ExitCode = -1;
                return;
            }

            SearClient sc = new SearClient(alerts);
            sc.Start();

            Console.CancelKeyPress += (o, e) =>
            {
                Console.WriteLine("Exit");

                // Allow the manin thread to continue and exit...
                waitHandle.Set();
            };

            waitHandle.WaitOne();

            sc.Stop();
        }
    }
}
