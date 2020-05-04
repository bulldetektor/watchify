using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Core;
using Watchify.AutoRun;
using Watchify.CommandLine;

namespace Watchify
{
	public class Program
	{
		private static int Main(string[] args)
		{
			var serviceProvider = ConfigureServices().BuildServiceProvider();

			var options = serviceProvider
				.GetRequiredService<CommandLineOptions>()
				.Parse(args);

			return options?.Command?.Run() ?? 1;
		}

		private static ServiceCollection ConfigureServices()
		{
			var services = new ServiceCollection();

			services.AddScoped<ConsoleApplication>();
			services.AddScoped<CommandLineOptions>();
			services.AddScoped<RootCommand>();
			services.AddScoped<ICommand, AutoRunner>();
			services.AddScoped<ProcessManager>();

			ConfigureLogging(services);
			
			return services;
		}

		private static void ConfigureLogging(IServiceCollection services)
		{
			services.AddLogging(loggingBuider =>
			{
				loggingBuider
					.AddConsole()
					.AddDebug()
					.AddSerilog();
			});

			var logLevelSwitch = new LoggingLevelSwitch();
			services.AddSingleton(logLevelSwitch);
			Log.Logger = new LoggerConfiguration()
				.WriteTo.Console()
				.MinimumLevel.ControlledBy(logLevelSwitch)
				.Enrich.FromLogContext()
				.CreateLogger();
		}
	}
}
