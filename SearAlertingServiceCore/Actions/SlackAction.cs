using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;
using System.Text;

namespace SearAlertingServiceCore.Actions
{
    public class SlackAction : Action
    {
        public string MessagePrefix { get; set; }
        public string SlackUrl { get; set; }

        public SlackAction()
        {
            base.ActionType = "Slack";
        }

        public override void Execute(string message, string name, bool failure = true)
        {
            base.Execute(message, name);

            Payload payload = new Payload()
            {
                text = String.Format("{0} {1}\r\n\r\n<{2}|Click Here> for details!", MessagePrefix, message, Link),
                username = "Sear Alert Bot",
                icon_emoji = failure ? ":fire:" : ":sunglasses:"
            };

            using (WebClient client = new WebClient())
            {
                NameValueCollection data = new NameValueCollection();
                data["payload"] = JsonConvert.SerializeObject(payload);
                client.UploadValues(SlackUrl, "POST", data);
            }
        }
    }

    public class Payload
    {                    
        public string username { get; set; }        
        public string text { get; set; }
        public string icon_emoji { get; set; }
    }
}
