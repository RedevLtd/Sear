using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RazorLight;


namespace SearAlertingServiceCore.Web
{
    public class SearWeb
    {
        private static IRazorLightEngine _engine = new RazorLightEngineBuilder()
            .UseFilesystemProject(Directory.GetCurrentDirectory() + "/Web")
            .Build();

        public static string Index(List<Alert> alerts)
        {
            try
            {
                var result = _engine.CompileRenderAsync("Index.cshtml", alerts.OrderByDescending(t => t.HasTriggered).ToList()).Result;
                return result;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return $"Error Processing WebPage: {e.Message}";
            }
        }
    }
}
