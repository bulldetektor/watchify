using System;
using System.Diagnostics;
using Windows.UI.Notifications;
using Watchify.CommandLine;

namespace Watchify.AutoRun
{
	public class ProcessManager : IDisposable
	{
		private Process _buildProcess;
		private CommandLineOptions _options;

		public ProcessManager Start(CommandLineOptions options)
		{
			_options = options;

			var args = new[]
			{
				"watch",
				"--project",
				_options.ProjectDir.FullName,
				"run"
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
				_buildProcess.Exited += (s, a) => { ShowToast("Watchify exited"); };
				_buildProcess.OutputDataReceived += (s, a) => { WriteProcessOutput(a.Data); };
				_buildProcess.ErrorDataReceived += (s, a) => { WriteError(a.Data); };

				_buildProcess.Start();
				_buildProcess.BeginOutputReadLine();
				_buildProcess.BeginErrorReadLine();

				return this;
			}
			catch (Exception ex)
			{
				WriteError("Failed to start process: " + ex.Message);
				return null;
			}
		}

		private bool _hasReceivedFirstOutput;
		private bool _isWaitingForStart;

		private void WriteProcessOutput(string output)
		{
			if (output == null)
				return;

			if (_isWaitingForStart)
			{
				WriteInfo("App starting up...");
				ShowToast("Watchify: App is starting up!");
				_isWaitingForStart = false;
				_hasReceivedFirstOutput = false;
			}

			if (output.StartsWith("watch") && !_hasReceivedFirstOutput)
			{
				WriteDebug($"Running (process: {_buildProcess.Id})");
				_hasReceivedFirstOutput = true;
				_isWaitingForStart = true;
			}

			WriteDebug(output);
		}

		private static void WriteError(string error)
		{
			if (error == null)
				return;

			ShowToast($"This means trouble: {error}");
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