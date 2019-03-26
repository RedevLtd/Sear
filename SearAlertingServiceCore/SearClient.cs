using log4net;
using Newtonsoft.Json;
using SearAlertingServiceCore.Actions;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SearAlertingServiceCore.Web;

namespace SearAlertingServiceCore
{
    public class SearClient
    {
        public bool stopped = true;

        private readonly SearConfig _config;
        private List<Alert> _alerts;
        private readonly log4net.ILog _logger = LogManager.GetLogger(typeof(SearClient));
        private WebServer _webServer;

        public SearClient(SearConfig config, List<Alert> alerts)
        {
            _config = config;
            _alerts = alerts;
            _logger.InfoFormat("Sear Client setup with {0} alerts", _alerts.Count);

            _webServer = new WebServer(SendResponse, "http://*:8080/");
        }

        public string SendResponse(HttpListenerRequest request)
        {
            return SearWeb.Index(_alerts);
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
            _webServer.Run();
            stopped = false;
            Task.Run(() => { Run(); });
        }

        /// <summary>
        /// Executes this alert
        /// </summary>
        /// <param name="alert"></param>
        public void ExecuteAlert(Alert alert)
        {
            try
            {
                long resultHits = PerformElasticRequest(alert);

                if (alert.Triggered(resultHits)) // check if triggered
                {
                    // execute Action
                    _logger.InfoFormat("\r\nAlert Triggered!\r\n------------------------------------\r\n{0} - Hits: {1}\r\n", alert.Name, resultHits);
                    alert.HasTriggered = true;

                    if(alert.WhenTriggered == null)
                        alert.WhenTriggered = DateTime.UtcNow;

                    alert.ExecuteAlert(resultHits, true);
                }
                else // we didnt trigger
                {
                    _logger.DebugFormat("\r\nRan Alert: {0}. Did not trigger. ActualValue: {1}, TriggerValue: {2}", alert.Name, resultHits, alert.Hits);
                    alert.HasTriggered = false;
                    alert.WhenTriggered = null;
                    alert.ExecuteAlert(resultHits, false);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(String.Format("Execute() Failed. Alert: {0}. Error: {1}", alert.Name, ex.Message), ex);
            }
        }

        public void Stop()
        {
            _webServer.Stop();
            stopped = true;
        }

        /// <summary>
        /// performs the ES query and returns the number of hits
        /// </summary>
        /// <param name="alert"></param>
        /// <returns></returns>
        public long PerformElasticRequest(Alert alert)
        {
            using (WebClient client = new WebClient())
            {
                // make elasticsearch request
                client.Headers.Add("Content-Type", "application/json");

                string query = alert.AdvancedQuery;
                if (alert.SimpleQuery != null)
                    query = BuildSimpleQuery(alert.SimpleQuery);
                

                var result = client.UploadString(alert.Host + "/" + alert.Index + "/_search", query);
                var json = JsonConvert.DeserializeObject<dynamic>(result);

                return json.hits.total;
            }
        }

        /// <summary>
        /// Builds up an ES json query string using the simple query variables
        /// </summary>
        /// <param name="simpleQuery"></param>
        /// <returns></returns>
        private string BuildSimpleQuery(SimpleQuery simpleQuery)
        {
            string query = @"{
            ""query"": {
                ""bool"": {
                    ""must"": [
                    {
                        ""query_string"": {
                            ""query"": """ + simpleQuery.SearchQuery + @""",
                            ""analyze_wildcard"": true,
                            ""default_field"": ""*""
                        }
                    },
                    {
                        ""range"": {
                            ""@timestamp"": {
                                ""gte"": """ + simpleQuery.TimeSpan + @""",
                                ""lte"": ""now""
                            }
                        }
                    }
                    ],
                    ""filter"": [],
                    ""should"": [],
                    ""must_not"": []
                }
            }
        }";

            return query;
        }
    }
}
