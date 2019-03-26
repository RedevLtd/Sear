using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace SearAlertingServiceCore.Actions
{
    public class AqlSmsAction : Action
    {
        public string AqlUrl { get; set; }
        public string AqlToken { get; set; }
        public List<string> Numbers { get; set; }

        public AqlSmsAction()
        {
            base.ActionType = "AQL SMS";
        }

        public override void Execute(string message, string name, bool failure = true)
        {
            base.Execute(message, name, failure);

            string numbers = String.Join(",", Numbers.Select(t => "\"" + t + "\""));
            string data = "{\"originator\" : \"SEAR\",\"destinations\" : [" + numbers + "], \"message\": \"" + message + "\"}";

            using (WebClient client = new WebClient())
            {
                client.Headers.Add("content-type", "application/json");
                client.Headers.Add("x-auth-token", AqlToken);
                client.UploadString(AqlUrl, "POST", data);
            }
        }

        public override string ToString()
        {
            return $"{base.ToString()}, Numbers: {String.Join(",",Numbers)}";
        }
    }
}
