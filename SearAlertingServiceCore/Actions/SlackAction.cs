using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;
using System.Text;

namespace SearAlertingServiceCore.Actions
{
    public static class SlackAction
    {
        public static bool Execute(string slackUrl, string message, string link)
        {
            using (WebClient client = new WebClient())
            {
                NameValueCollection data = new NameValueCollection();
                data["payload"] = String.Format("{ \"text\": \"{0}\r\n\r\n<{1}|Click here> for details!\", \"username\": \"Sear Alert Bot\"}", message, link);
                client.UploadValues(slackUrl, "POST", data);
            }

            return true;
        }
    }

    public class SlackConfig
    {
        public string SlackUrl { get; set; }
    }
}
