using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Notifications;
using IWshRuntimeLibrary;

namespace Watchify
{
	public class Program
	{
		private static Process _buildProcess;
		private const string AppId = "70d199a3-71b3-46ed-8457-b0b4df6a3cb9";

		private static async Task Main(string[] args)
		{
			Console.WriteLine("Initializing...");

			Console.CancelKeyPress += (s, a) => KillProcess();

			StartProcess();

			_buildProcess.Exited += (s, a) =>
			{
				ShowToast("Watchify exited");
			};
			_buildProcess.OutputDataReceived += (s, a) => { WriteProcessOutput(a.Data); };
			_buildProcess.ErrorDataReceived += (s, a) => { WriteProcessError(a.Data); };

			Console.WriteLine("Press X to exit...");

			while (true)
			{
				var userInput = Console.ReadKey(intercept: true);
				if (userInput.Key == ConsoleKey.X)
				{
					_buildProcess.Kill();
					break;
				}
			}
			
			await Task.Delay(1000);
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

		private static bool _hasReceivedFirstOutput;
		private static bool _isWaitingForStart;

		private static void WriteProcessOutput(string output)
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

		private static void KillProcess()
		{
			_buildProcess.Kill();
		}

		private static void StartProcess()
		{
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
					ArgumentList = {"watch", "--project", @"C:\data\kode\carweb\commonbuild\Microservices\vehicleservice\src\VehicleService", "run"}
				},
				EnableRaisingEvents = true,

			};

			try
			{
				_buildProcess.Start();
				_buildProcess.BeginOutputReadLine();
				_buildProcess.BeginErrorReadLine();
			}
			catch (Exception ex)
			{
				Console.WriteLine("Failed to start process: " + ex.Message);
			}
		}

		private static void ShowToast(string message)
		{
			var toastTemplate = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastText01);

			var textNode = toastTemplate.GetElementsByTagName("text");
			textNode.Item(0).InnerText = message;

			var notifier = ToastNotificationManager.CreateToastNotifier("Watchify");
			var notification = new ToastNotification(toastTemplate);
			notification.ExpirationTime = DateTimeOffset.Now.AddSeconds(2);
			notifier.Show(notification);
		}

		internal static bool TryCreateShortcut()
		{
			try
			{
				var startMenuDir = Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu);
				startMenuDir = Path.Combine(startMenuDir, "Programs", "Watchify");
				if (!Directory.Exists(startMenuDir))
					Directory.CreateDirectory(startMenuDir);

				var shortcutLocation = Path.Combine(startMenuDir, "Watchify.lnk");
				var shell = new WshShell();
				var shortcut = (WshShortcut)shell.CreateShortcut(shortcutLocation);
				shortcut.Description = "Watch dotnet core builds";
				shortcut.TargetPath = Assembly.GetEntryAssembly().Location.Replace(".dll", ".exe");
				//shortcut.FullName = "Bulldetektor.Watchify";

				shortcut.Save();

				return true;
			}
			catch (Exception ex)
			{
				Console.WriteLine("Failed to create shortcut: " + ex.Message);
				return false;
			}
		}
	}


}
