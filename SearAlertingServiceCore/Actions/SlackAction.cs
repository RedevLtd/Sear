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
                data["payload"] = "{ \"text\": \"" + message + "\r\n\r\n<" + link + "|Click here> for details!\", \"username\": \"Sear Alert Bot\"}";
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
