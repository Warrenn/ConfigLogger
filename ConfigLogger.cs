using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace ConfigLogger
{
    public static class ConfigLogger
    {
        public class FluentClass
        {
            internal ConfigLoggerSettings Settings { get; set; }
        }

        public static FluentClass LogFunction(Action<string, string> logFunction)
        {
            var settings = ConfigLoggerSettings.Default;
            settings.LogFunction = logFunction;
            return new FluentClass { Settings = settings };
        }

        public static FluentClass LogFunction(this FluentClass fluent, Action<string, string> logFunction)
        {
            fluent.Settings.LogFunction = logFunction;
            return fluent;
        }

        public static FluentClass HashKey(string hashKey)
        {
            var settings = ConfigLoggerSettings.Default;
            settings.HashKeys.Add(key => string.Equals(key, hashKey, StringComparison.InvariantCultureIgnoreCase));
            return new FluentClass { Settings = settings };
        }

        public static FluentClass HashKey(this FluentClass fluent, string hashKey)
        {
            fluent.Settings.HashKeys.Add(key => string.Equals(key, hashKey, StringComparison.InvariantCultureIgnoreCase));
            return fluent;
        }

        public static FluentClass HashCondition(Predicate<string> predicate)
        {
            var settings = ConfigLoggerSettings.Default;
            settings.HashKeys.Add(predicate);
            return new FluentClass { Settings = settings };
        }

        public static FluentClass HashCondition(this FluentClass fluent, Predicate<string> predicate)
        {
            fluent.Settings.HashKeys.Add(predicate);
            return fluent;
        }

        public static FluentClass Transform(string hashKey, Func<string, string> transform)
        {
            var settings = ConfigLoggerSettings.Default;
            settings.Transforms.Add(key => string.Equals(key, hashKey, StringComparison.InvariantCultureIgnoreCase), transform);
            return new FluentClass { Settings = settings };
        }

        public static FluentClass Transform(this FluentClass fluent, string hashKey, Func<string, string> transform)
        {
            fluent.Settings.Transforms.Add(key => string.Equals(key, hashKey, StringComparison.InvariantCultureIgnoreCase), transform);
            return fluent;
        }

        public static void LogValues(IConfiguration config, Action<string, string> logFunction)
        {
            var settings = ConfigLoggerSettings.Default;
            settings.LogFunction = logFunction;
            InternalLogValues(settings, config);
        }

        public static void LogValues(IConfiguration config)
        {
            var settings = ConfigLoggerSettings.Default;
            InternalLogValues(settings, config);
        }

        public static void LogValues(this FluentClass fluent, IConfiguration config, Action<string, string> logFunction)
        {
            fluent.Settings.LogFunction = logFunction;
            InternalLogValues(fluent.Settings, config);
        }

        public static void LogValues(this FluentClass fluent, IConfiguration config)
        {
            InternalLogValues(fluent.Settings, config);
        }

        public static void LogAssemblyAndVersion(Action<string, string> logFunction = null)
        {
            logFunction = logFunction ?? ((a, v) => Trace.TraceInformation($"Assembly: {a}; Version={v}"));
            var assembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
            var fullname = assembly.FullName;
            var version = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
            logFunction(fullname, version);
        }

        private static void InternalLogValues(ConfigLoggerSettings settings, IConfiguration config)
        {
            var logFunction = settings.LogFunction;
            var hashKeys = config["HASH_KEYS"];
            foreach (var key in hashKeys.Split(';'))
            {
                settings.HashKeys.Add(s => string.Equals(s, key, StringComparison.InvariantCultureIgnoreCase));
            }
            foreach (var predicate in settings.HashKeys)
            {
                settings.Transforms.Add(predicate, HashFunction);
            }

            foreach (var valuePair in config.AsEnumerable())
            {
                var transform = settings.Transforms
                                    .FirstOrDefault(kv => kv.Key(valuePair.Key)).Value ??
                                ((v) => v);
                var value = transform(valuePair.Value);
                logFunction(valuePair.Key, value);
            }
        }

        private static string HashFunction(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return "";
            using (var sha1 = new SHA1CryptoServiceProvider())
            {
                var byteValue = Encoding.ASCII.GetBytes(value);
                var hashBytes = sha1.ComputeHash(byteValue);
                return $"SHA1(base64){Convert.ToBase64String(hashBytes)}";
            }
        }


    }
}
