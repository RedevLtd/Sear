using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;
using System.Text;

namespace SearAlertingServiceCore.Actions
{
    public static class SlackAction
    {
        public static bool Execute(string slackUrl, string message, string link, bool fail = true)
        {
            Payload payload = new Payload()
            {
                text = String.Format("{0}\r\n\r\n<{1}|Click Here> for details!", message, link),
                username = "Sear Alert Bot",
                icon_emoji = fail ? ":fire:" : ":sunglasses:"
            };

            using (WebClient client = new WebClient())
            {
                NameValueCollection data = new NameValueCollection();
                data["payload"] = JsonConvert.SerializeObject(payload);
                client.UploadValues(slackUrl, "POST", data);
            }

            return true;
        }
    }

    public class SlackConfig
    {
        public string SlackUrl { get; set; }
    }

    public class Payload
    {                    
        public string username { get; set; }        
        public string text { get; set; }
        public string icon_emoji { get; set; }
    }
}
