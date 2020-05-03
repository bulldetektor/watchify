using System;
using System.IO;
using Microsoft.Extensions.CommandLineUtils;

namespace Watchify.CommandLine
{
	public class CommandLineOptions
	{
		public ICommand Command { get; set; }
		public DirectoryInfo ProjectDir { get; private set; }

		public static CommandLineOptions Parse(string[] args)
		{
			var options = new CommandLineOptions();

			var app = new CommandLineApplication(throwOnUnexpectedArg: false)
			{
				Name = "watchify",
				FullName = "Watch and run dotnet core projects with visual feedback"
			};
			app.HelpOption(Constants.HelpOptions);

			var projectDirInput = app.Option(
				"-p|--project", 
				"Path to directory containing the project you want to run and watch (defaults to current directory)",
				CommandOptionType.SingleValue);

			RootCommand.Configure(app, options);

			var result = app.Execute(args);
				
			options.ProjectDir = GetProjectDir(projectDirInput);

			return result != 0 ? null : options;
		}

		private static DirectoryInfo GetProjectDir(CommandOption projectDirInput)
		{
			DirectoryInfo projectDir;
			if (projectDirInput.HasValue() && projectDirInput.Value().Trim() != ".")
			{
				projectDir = new DirectoryInfo(projectDirInput.Value());
				if (!projectDir.Exists)
				{
					throw new Exception($"Directory not found: {projectDirInput}");
				}
			}
			else
			{
				projectDir = new DirectoryInfo(Environment.CurrentDirectory);
			}

			return projectDir;
		}

	}
}