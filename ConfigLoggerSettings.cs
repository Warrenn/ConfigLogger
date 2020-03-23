using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace ConfigLogger
{
    internal class ConfigLoggerSettings
    {
        public static ConfigLoggerSettings Default =>
            new ConfigLoggerSettings
            {
                HashKeys = new List<Predicate<string>>
                {
                    key => (key ?? "").ToUpper().Contains("PASSWORD"),
                    key => (key ?? "").ToUpper().Contains("SECRET"),
                    key => (key ?? "").ToUpper().Contains("CONNECTIONSTRING")
                },
                LogFunction = (key, value) => Trace.TraceInformation($"{key} = {value}")
            };

        public ConfigLoggerSettings()
        {
            Transforms = new Dictionary<Predicate<string>, Func<string, string>>();
            HashKeys = new List<Predicate<string>>();
            LogFunction = (s, s1) => { };
        }

        public Dictionary<Predicate<string>, Func<string, string>> Transforms { get; set; }
        public IList<Predicate<string>> HashKeys { get; set; }
        public Action<string, string> LogFunction { get; set; }
    }
}
