using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;

namespace MqttSubscriber
{
    public class Program
    {
        public static string sourceName = "MqttSubscriber";
        public static IConfiguration configuration;
        public static ILogger logger;

        public Program(IConfiguration _configuration, ILogger _logger)
        {
            configuration = _configuration;
            logger = _logger;
        }

        public static void Main(string[] args)
        {
            using var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.SetMinimumLevel(LogLevel.Debug);
                builder.AddLog4Net("log4net.config");
            });
            logger = loggerFactory.CreateLogger<Program>();

            logger.LogInformation($"{sourceName} worker started at: {DateTimeOffset.Now}");

            configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: false).Build();

            CreateHostBuilder(args).Build().Run();
            logger.LogInformation($"{sourceName} worker stopped at: {DateTimeOffset.Now}");
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
            .UseWindowsService()
            .ConfigureServices((hostContext, services) =>
            {
                services.AddHostedService<Worker>();
            });
    }
}
