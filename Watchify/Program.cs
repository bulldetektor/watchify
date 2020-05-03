using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Watchify.AutoRun;
using Watchify.CommandLine;

namespace Watchify
{
	public class Program
	{
		private static int Main(string[] args)
		{
			var services = new ServiceCollection();

			services.AddLogging(loggingBuider =>
			{
				loggingBuider.AddConsole();
			});


			services.AddScoped<ConsoleApplication>();
			services.AddScoped<CommandLineOptions>();
			services.AddScoped<RootCommand>();
			services.AddScoped<ICommand, AutoRunner>();

			var options = services
				.BuildServiceProvider()
				.GetRequiredService<CommandLineOptions>()
				.Parse(args);

			return options?.Command?.Run() ?? 1;
		}
	}
}
