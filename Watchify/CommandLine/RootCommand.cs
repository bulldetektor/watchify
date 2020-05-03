using System.Collections.Generic;

namespace Watchify.CommandLine
{
	public class RootCommand : ICommand
	{
		public string Name => "root";
		public string Description => "root description";


		public RootCommand(ConsoleApplication app, IEnumerable<ICommand> commands)
		{
			_app = app;
			_commands = commands;
		}

		private readonly ConsoleApplication _app;
		private readonly IEnumerable<ICommand> _commands;

		
		public ICommand Configure(CommandLineOptions options)
		{
			foreach (var command in _commands)
			{
				_app.Command(command.Name, app =>
				{
					app.Description = command.Description;
					app.HelpOption(Constants.HelpOptions);

					app.OnExecute(() =>
					{
						options.Command = command.Configure(options);
						return 0;
					});

				});
			}

			_app.OnExecute(() =>
			{
				options.Command = this;

				return 0;
			});

			return this;
		}


		public int Run()
		{
			_app.ShowHelp();
			return 1;
		}
	}
}