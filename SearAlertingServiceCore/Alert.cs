using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Action = SearAlertingServiceCore.Actions.Action;

namespace SearAlertingServiceCore
{
    public class Alert
    {
        private readonly log4net.ILog _logger = LogManager.GetLogger(typeof(Alert));

        public string Name { get; set; }
        public string Interval { get; set; }
        public string Host { get; set; }
        public string Index { get; set; }
        public string AdvancedQuery { get; set; }
        public SimpleQuery SimpleQuery { get; set; }
        public HitType HitType { get; set; }
        public int Hits { get; set; }
        public List<Action> Actions { get; set; }
        public bool HasTriggered { get; set; }
        public DateTime? WhenTriggered { get; set; }
        public bool AlertOnImproved { get; set; }  

        /// <summary>
        /// Checks if this alert should trigger based on the hits
        /// </summary>
        /// <param name="resultHits"></param>
        /// <returns></returns>
        public bool Triggered(long resultHits)
        {
            if (HitType == HitType.Higher)
                return resultHits > Hits;
            else if (HitType == HitType.Lower)
                return resultHits < Hits;

            return false;
        }

        /// <summary>
        /// Execute the alert
        /// </summary>
        /// <param name="resultHits"></param>
        /// <param name="failure"></param>
        public void ExecuteAlert(long resultHits, bool failure = true)
        {
            try
            {
                if (failure)
                {
                    string message = string.Format("Alert: {0} Triggered!\r\n\r\n Hits: {1} exceeded the threshold {2}", Name, resultHits, Hits);

                    // reset after a day, as alert has been ignored.
                    if (WhenTriggered < DateTime.UtcNow.AddDays(-1))
                    {
                        WhenTriggered = DateTime.UtcNow;
                        Actions.Select(t => { t.SetHasExecuted(false); return t;}).ToList(); // reset all actions
                    }

                    // get every action where the timespan for escalation is less than the current triggered period
                    // i.e. an action might want to be triggered if the current failure has been triggered for over 60mins
                    var actions = Actions.Where(t => (DateTime.UtcNow - WhenTriggered.Value).TotalMinutes >= t.EscalationTimeSpan && t.GetHasExecuted() == false);

                    foreach (var action in actions)
                        action.Execute(message, Name);
                }
                else
                {
                    if (HasTriggered && AlertOnImproved) // if the alert was previously triggered and flag set to alert on improved
                    {
                        _logger.InfoFormat("Alert {0} has improved", Name);

                        string message = string.Format("Alert: {0} has Improved!\r\n\r\n Hits: {1} now within the threshold {2}", Name, resultHits, Hits);

                        foreach (var action in Actions)
                            action.Execute(message, Name, false);
                    }

                    WhenTriggered = null;
                    HasTriggered = false;
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"ExecuteAlert() Alert: {Name} failed to trigger. Error: {ex.Message}", ex);
            }
        }

    }

    public class SimpleQuery
    {
        public string SearchQuery { get; set; }
        public string TimeSpan { get; set; }

        public override string ToString()
        {
            return $"Query: {SearchQuery}, TimeSpan: {TimeSpan}";
        }
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum HitType
    {
        Higher,
        Lower
    }
}
