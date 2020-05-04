using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.UI.Notifications;
using Watchify.CommandLine;

namespace Watchify.AutoRun
{
	public class ProcessManager : IDisposable
	{
		private Process _buildProcess;
		private CommandLineOptions _options;

		public async Task<ProcessManager> Start(CommandLineOptions options)
		{
			_options = options;

			var args = new[]
			{
				"watch",
				"--verbose",
				"--project",
				_options.ProjectDir.FullName,
				"run",
				"-- --Logging:LogLevel:Default=Debug"
			};
			_buildProcess = new Process
			{
				StartInfo =
				{
					UseShellExecute = false,
					RedirectStandardOutput = true,
					RedirectStandardError = true,
					WindowStyle = ProcessWindowStyle.Hidden,
					CreateNoWindow = true,
					FileName = "dotnet",
					Arguments = string.Join(" ", args)
				},
				EnableRaisingEvents = true,
			};

			try
			{
				await Task.Run(() =>
				{
					_buildProcess.Exited += (s, a) => { ShowToast("Watchify exited"); };
					_buildProcess.OutputDataReceived += (s, a) => { WriteOutput(a.Data); };
					_buildProcess.ErrorDataReceived += (s, a) => { WriteError(a.Data); };

					_buildProcess.Start();
					_buildProcess.BeginOutputReadLine();
					_buildProcess.BeginErrorReadLine();
				});

				return this;
			}
			catch (Exception ex)
			{
				WriteError("Failed to start process: " + ex.Message);
				return null;
			}
		}

		private bool _hasReceivedFirstOutput;
		private bool _isRestarting;
		private bool _isStarting;

		private void WriteOutput(string output)
		{
			if (output == null)
				return;

			if (output.StartsWith("watch") && !_isRestarting)
			{
				if (!_hasReceivedFirstOutput)
				{
					WriteInfo("Starting up...");
					WriteDebug($"Running (process: {_buildProcess.Id})");
					_hasReceivedFirstOutput = true;
				}

				switch (output.ToLowerInvariant().Trim())
				{
					case "watch : started":
						_isStarting = true;
						break;
					case "watch : exited":
						ShowToast("Watchify: Restarting...");
						WriteInfo("Restarting");
						_isRestarting = true;
						break;
				}
			}
			else if (_isStarting || _isRestarting)
			{
				if (output.StartsWith("info:"))
				{
					ShowToast("Watchify: Ready!");
					WriteInfo("Ready!");
					_isRestarting = false;
					_isStarting = false;
				}
			}

			WriteDebug(output);
		}

		private static void WriteError(string error)
		{
			if (error == null || string.IsNullOrWhiteSpace(error))
				return;

			if (error.ToLowerInvariant().StartsWith("watch : exited with error code"))
			{
				ShowToast("Watchify: Error building and running the app");
			}

			Console.ForegroundColor = ConsoleColor.DarkRed;
			Console.WriteLine(error);
			Console.ResetColor();
		}

		private static void WriteInfo(string message)
		{
			Console.ForegroundColor = ConsoleColor.DarkGreen;
			Console.WriteLine(message);
			Console.ResetColor();
		}

		private void WriteDebug(string message)
		{
			if (!_options.IsVerboseLoggingEnabled)
				return;
			Console.ForegroundColor = ConsoleColor.DarkYellow;
			Console.WriteLine(message);
			Console.ResetColor();
		}

		private static void ShowToast(string message)
		{
			var toastTemplate = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastText01);

			var textNode = toastTemplate.GetElementsByTagName("text");
			textNode.Item(0).InnerText = message;

			var notifier = ToastNotificationManager.CreateToastNotifier("Watchify");
			var notification = new ToastNotification(toastTemplate)
			{
				ExpirationTime = DateTimeOffset.Now.AddSeconds(2)
			};
			notifier.Show(notification);
		}

		public void Stop()
		{
			if (_buildProcess == null)
				return;

			_buildProcess.Kill();
			_buildProcess = null;
		}

		public void Dispose()
		{
			Stop();
		}

	}
}