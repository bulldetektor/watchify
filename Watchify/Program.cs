using Watchify.CommandLine;

// ReSharper disable ArgumentsStyleLiteral

namespace Watchify
{
	public class Program
	{
		private static int Main(string[] args)
		{
			var options = CommandLineOptions.Parse(args);

			if (options?.Command == null)
			{
				return 1;
			}

			return options.Command.Run();
		}
	}
}
