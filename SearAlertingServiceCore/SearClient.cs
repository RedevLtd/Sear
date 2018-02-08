using log4net;
using Newtonsoft.Json;
using SearAlertingServiceCore.Actions;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SearAlertingServiceCore
{
    public class SearClient
    {
        public bool stopped = true;

        private static List<Alert> _alerts;
        private static readonly log4net.ILog _logger = LogManager.GetLogger(typeof(SearClient));

        public SearClient(List<Alert> alerts)
        {
            _alerts = alerts;
            _logger.InfoFormat("Sear Client setup with {0} alerts", _alerts.Count);
        }


        /// <summary>
        /// run every minute, check for any alerts due to be ran
        /// </summary>
        private void Run()
        {
            while (stopped == false)
            {
                _logger.DebugFormat("{0} - Check Interval", DateTime.Now.ToString("yyyy-MMM-dd HH:mm:ss"));

                foreach (var alert in _alerts)
                {
                    DateTime nextRun = NCrontab.CrontabSchedule.Parse(alert.Interval).GetNextOccurrence(DateTime.Now.AddMinutes(-1));

                    if (new DateTime(nextRun.Year, nextRun.Month, nextRun.Day, nextRun.Hour, nextRun.Minute, 0) <= new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, 0))
                        ExecuteAlert(alert);
                }

                Thread.Sleep(60 * 1000);
            }

            _logger.Info("Service Stopped");
        }

        public void Start()
        {
            stopped = false;
            Task.Run(() => { Run(); });
        }


        public void ExecuteAlert(Alert alert)
        {
            try
            {
                using (WebClient client = new WebClient())
                {
                    // make elasticsearch request
                    client.Headers.Add("Content-Type", "application/json");
                    var result = client.UploadString(alert.Host + "/" + alert.Index + "/_search", alert.Query);
                    var json = JsonConvert.DeserializeObject<dynamic>(result);

                    long resultHits = json.hits.total;

                    if (alert.HitType == HitType.Higher ? resultHits > alert.Hits : resultHits < alert.Hits) // check if triggered
                    {
                        // execute Action
                        _logger.Info("Alert Triggered!");
                        _logger.Info("------------------------------------");
                        _logger.InfoFormat("{0} - Hits: {1}\r\n", alert.Name, resultHits);

                        string message = string.Format("{0}Alert: {1} Triggered!\r\n\r\n Hits: {2} exceeded the threshold {3}", alert.MessagePrefix, alert.Name, resultHits, alert.Hits);

                        if (alert.Triggered) // dont want to alert again
                        {
                            _logger.Debug("Already triggered, not sending alerts");
                            return;
                        }

                        if (alert.ActionType == ActionType.Slack)
                            SlackAction.Execute(alert.ActionConfig, message, alert.Link);

                        alert.Triggered = true;
                    }
                    else
                    {
                        _logger.DebugFormat("Ran Alert: {0}. Did not trigger. ActualValue: {1}, TriggerValue: {2}", alert.Name, resultHits, alert.Hits);
                        alert.Triggered = false;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(String.Format("Execute() Failed. Alert: {0}. Error: {1}", alert.Name, ex.Message), ex);
            }
        }

        public void Stop()
        {
            stopped = true;
        }

    }
}
