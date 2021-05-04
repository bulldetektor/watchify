using System;
using System.Linq;
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

			Console.WriteLine("Args: " + string.Join(',', args));
			var options = serviceProvider
				.GetRequiredService<CommandLineOptions>()
				.Parse(args);

			return options?.Command?.Run() ?? 1;
		}

		private static IServiceCollection ConfigureServices()
		{
			var services = new ServiceCollection()
				.AddScoped<ConsoleApplication>()
				.AddScoped<CommandLineOptions>()
				.AddScoped<RootCommand>()
				.AddScoped<ICommand, AutoRunner>()
				.AddScoped<RunWatchProcessManager>()
				.AddScoped<ProxyServerProcessManager>()
				.AddScoped<LaunchSettingsParser>();

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
