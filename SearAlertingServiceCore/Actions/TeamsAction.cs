using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;
using System.Text;

namespace SearAlertingServiceCore.Actions
{
    public class TeamsAction : Action
    {
        public string TeamsUrl { get; set; }

        public TeamsAction()
        {
            base.ActionType = "Teams";
        }

        public override void Execute(string message, string name, bool failure = true)
        {
            base.Execute(message, name);

            TeamsPayload payload = new TeamsPayload()
            {
                summary = $"Alert: {name} has {(failure ? "Triggered" : "Reset")}",
                title = $"Alert: {name} has {(failure ? "Triggered" : "Reset")}",
                themeColor = failure ? "FF0000" : "0075FF",
                sections = new Section[]
                {
                    new Section()
                    {
                        activitySubtitle = DateTime.UtcNow.ToString("dd-MMM-yy hh:mm"),
                        activityTitle = message,
                        activityImage = failure ? "https://emojipedia-us.s3.dualstack.us-west-1.amazonaws.com/thumbs/120/google/274/fire_1f525.png" 
                            : "https://emojipedia-us.s3.dualstack.us-west-1.amazonaws.com/thumbs/120/twitter/281/smiling-face-with-sunglasses_1f60e.png"
                    }
                },
                potentialAction = new Potentialaction[]
                {
                    new Potentialaction()
                    {
                        name = "View in Kibana",
                        type = "OpenUri",
                        targets = new Target[]
                        {
                            new Target()
                            {
                                os = "default",
                                uri = Link
                            }
                        }
                    }
                }
            };

            using (WebClient client = new WebClient())
            {
                var data = JsonConvert.SerializeObject(payload);
                client.Headers.Add("Content-Type", "application/json");
                client.UploadString(TeamsUrl, data);
            }
        }
    }

    public class TeamsPayload
    {
        public string summary { get; set; }
        public string themeColor { get; set; }
        public string title { get; set; }
        public Section[] sections { get; set; }
        public Potentialaction[] potentialAction { get; set; }
    }

    public class Section
    {
        public string activityTitle { get; set; }
        public string activitySubtitle { get; set; }
        public string activityImage { get; set; }
        public string text { get; set; }
    }

    public class Potentialaction
    {
        [JsonPropertyAttribute("@type")]
        public string type { get; set; }
        public string name { get; set; }
        public Target[] targets { get; set; }
    }

    public class Target
    {
        public string os { get; set; }
        public string uri { get; set; }
    }


}
