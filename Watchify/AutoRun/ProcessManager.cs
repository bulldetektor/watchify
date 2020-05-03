using System;
using System.Diagnostics;
using System.IO;
using Windows.UI.Notifications;

namespace Watchify.AutoRun
{
	public class ProcessManager : IDisposable
	{
		private Process _buildProcess;

		public void Start(DirectoryInfo projectDir)
		{
			var args = new[]
			{
				"watch",
				"--project",
				projectDir.FullName,
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
				_buildProcess.ErrorDataReceived += (s, a) => { WriteProcessError(a.Data); };

				_buildProcess.Start();
				_buildProcess.BeginOutputReadLine();
				_buildProcess.BeginErrorReadLine();
			}
			catch (Exception ex)
			{
				Console.WriteLine("Failed to start process: " + ex.Message);
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
				Console.ForegroundColor = ConsoleColor.DarkGreen;
				Console.WriteLine("App starting up");
				ShowToast("Watchify: App is starting up!");
				_isWaitingForStart = false;
				_hasReceivedFirstOutput = false;
			}

			if (output.StartsWith("watch") && !_hasReceivedFirstOutput)
			{
				Console.WriteLine($"Running (process: {_buildProcess.Id})");
				ShowToast($"Watchify: {output.Substring("watch : ".Length)}");
				_hasReceivedFirstOutput = true;
				_isWaitingForStart = true;
			}

			Console.ForegroundColor = ConsoleColor.DarkYellow;
			Console.WriteLine(output);
			Console.ResetColor();
		}

		private static void WriteProcessError(string error)
		{
			if (error == null)
				return;

			ShowToast($"This means trouble: {error}");
			Console.ForegroundColor = ConsoleColor.DarkRed;
			Console.WriteLine(error);
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
			if(_buildProcess == null)
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