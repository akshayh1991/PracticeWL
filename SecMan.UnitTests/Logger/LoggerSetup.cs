using Microsoft.Extensions.Configuration;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecMan.UnitTests.Logger
{
    public class LoggerSetup
    {
        public static void Initialize()
        {

            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();

            // Get log file path from configuration
            string? logFilePath = configuration["UnitTestCasesPath:Path"];

            // Configure Serilog to write to a file
            Log.Logger = new LoggerConfiguration()
                .WriteTo.File(logFilePath,
                              rollingInterval: RollingInterval.Day,
                              fileSizeLimitBytes: 10_000_000) // Optional: limit file size to 10 MB
                .CreateLogger();
        }
    }
}
