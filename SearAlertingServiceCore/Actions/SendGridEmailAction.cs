using System;
using System.Collections.Generic;
using System.Text;
using log4net;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace SearAlertingServiceCore.Actions
{
    public class SendGridEmailAction : Action
    {
        public string SmtpServer { get; set; }
        public string SendGridApiKey { get; set; }
        public string From { get; set; }
        public string To { get; set; }

        private readonly log4net.ILog _logger = LogManager.GetLogger(typeof(SendGridEmailAction));

        public SendGridEmailAction()
        {
            base.ActionType = "SendGrid Email";
        }

        public override void Execute(string message, string name, bool failure = true)
        {
            try
            {
                base.Execute(message, name, failure);

                SendGrid.SendGridClient client = new SendGridClient(SendGridApiKey);
                var tos = new List<EmailAddress>();

                foreach (var item in To.Split(';'))
                    tos.Add(new EmailAddress(item));

                var email = new SendGridMessage();

                email.SetFrom(From);
                email.SetSubject($"Sear Alert - {name} " + (failure ? "Triggered!" : "Resolved!"));
                email.AddTos(tos);

                email.AddContent(MimeType.Text, message);

                var result = client.SendEmailAsync(email).Result;
            }
            catch (Exception ex)
            {
               _logger.Error("Failed to send email. Error: " + ex.Message, ex);
            }
        }

        public override string ToString()
        {
            return $"{base.ToString()}, To: {To}, From: {From}";
        }
    }
}
