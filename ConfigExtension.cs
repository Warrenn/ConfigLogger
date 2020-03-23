using System;
using System.Diagnostics;
using System.Reflection;
using Microsoft.Extensions.Configuration;

namespace ConfigLogger
{
    public static class ConfigExtension
    {
        public static void LogValues(this IConfiguration config, Action<string, string> logFunction)
        {
            ConfigLogger.LogValues(config, logFunction);
        }

        public static void LogValues(this IConfiguration config)
        {
            ConfigLogger.LogValues(config);
        }
    }
}
