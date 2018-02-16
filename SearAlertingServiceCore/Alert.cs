using System;
using System.Collections.Generic;
using System.Text;

namespace SearAlertingServiceCore
{
    public class Alert
    {
        public string Name { get; set; }
        public string Interval { get; set; }
        public string Host { get; set; }
        public string Index { get; set; }
        public string Query { get; set; }
        public HitType HitType { get; set; }
        public int Hits { get; set; }
        public ActionType ActionType { get; set; }
        public string ActionConfig { get; set; }
        public string Link { get; set; }
        public string MessagePrefix { get; set; }
        public bool HasTriggered { get; set; }
        public bool AlertOnImproved { get; set; }       


        public bool Triggered(long resultHits)
        {
            if (HitType == HitType.Higher)
                return resultHits > Hits;
            else if (HitType == HitType.Lower)
                return resultHits < Hits;

            return false;
        }

    }

    public enum ActionType
    {
        Log,
        Slack,
        Email
    }

    public enum HitType
    {
        Higher,
        Lower
    }
}
