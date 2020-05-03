using Microsoft.Extensions.CommandLineUtils;

namespace Watchify.CommandLine
{
	public class ConsoleApplication : CommandLineApplication
	{
		public ConsoleApplication() : base(throwOnUnexpectedArg: true)
		{
			Name = "watchify";
			FullName = "Watch and run dotnet core projects with visual feedback";
			HelpOption(Constants.HelpOptions);
		}

	}
}