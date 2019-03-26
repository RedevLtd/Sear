using log4net;

namespace SearAlertingServiceCore.Actions
{
    public class Action
    {
        public string ActionType {get; set; }
        public long EscalationTimeSpan { get; set; } // how many minutes since it was first triggered
        public string Link { get; set; }
        private readonly log4net.ILog _logger = LogManager.GetLogger(typeof(Action));
        private bool _hasExecuted { get; set; }

        public virtual void Execute(string message, string name, bool failure = true)
        {
            _logger.DebugFormat("{0} Action has been triggered. Escalation TimeSpan: {1} mins", ActionType, EscalationTimeSpan);
            _hasExecuted = true;
        }

        public bool GetHasExecuted()
        {
            return _hasExecuted;
        }

        public void SetHasExecuted(bool value)
        {
            _hasExecuted = value;
        }

        public override string ToString()
        {
            return $"{ActionType}  - EscalationTimeSpan: {EscalationTimeSpan} mins, Link: {Link}";
        }
    }
}
