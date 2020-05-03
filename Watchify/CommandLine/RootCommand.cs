using Microsoft.Extensions.CommandLineUtils;
using Watchify.AutoRun;

namespace Watchify.CommandLine
{
	public class RootCommand : ICommand
	{
		private readonly CommandLineApplication _app;

		public RootCommand(CommandLineApplication app)
		{
			_app = app;
		}

		public static void Configure(CommandLineApplication app, CommandLineOptions options)
		{
			app.Command("run", c => AutoRunner.Configure(c, options));

			app.OnExecute(() =>
			{
				options.Command = new RootCommand(app);

				return 0;
			});
		}

		public int Run()
		{
			_app.ShowHelp();

			return 1;
		}
	}
}