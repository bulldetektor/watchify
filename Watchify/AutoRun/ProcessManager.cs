using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.UI.Notifications;

namespace Watchify.AutoRun
{
	public abstract class ProcessManager : IDisposable
	{
		protected abstract string ProcessToStart { get; }
		protected abstract string[] ProcessArgs { get; }
		protected abstract ConsoleColor InfoColor { get; }
		protected abstract ConsoleColor ErrorColor { get; }
		protected abstract ConsoleColor DebugColor { get; }

		protected abstract void OnExit(object sender, EventArgs args);
		protected abstract void HandleOutput(string message);
		protected abstract void HandleError(string message);


		protected Process RunningProcess;
		private bool _isVerboseLoggingEnabled;

		public ProcessManager WithLogVerbosity(bool isVerboseLoggingEnabled)
		{
			_isVerboseLoggingEnabled = isVerboseLoggingEnabled;
			return this;
		}

		public async Task<ProcessManager> Start()
		{
			if (Uri.TryCreate(ProcessToStart, UriKind.Absolute, out _))
			{
				RunningProcess = new Process
				{
					StartInfo = new ProcessStartInfo(ProcessToStart)
					{
						UseShellExecute = true,
						Verb = "open"
					}
				};
			}
			else
			{
				RunningProcess = new Process
				{
					StartInfo =
				{
					UseShellExecute = false,
					RedirectStandardOutput = true,
					RedirectStandardError = true,
					WindowStyle = ProcessWindowStyle.Hidden,
					CreateNoWindow = true,
					FileName = ProcessToStart,
					Arguments = string.Join(" ", ProcessArgs)
				},
					EnableRaisingEvents = true,
				};
			}

			try
			{
				await Task.Run(() =>
				{
					RunningProcess.Exited += OnExit;
					RunningProcess.OutputDataReceived += (s, a) => { HandleOutput(a.Data); };
					RunningProcess.ErrorDataReceived += (s, a) => { HandleError(a.Data); };

					RunningProcess.Start();
					RunningProcess.BeginOutputReadLine();
					RunningProcess.BeginErrorReadLine();
				});

				return this;
			}
			catch (Exception ex)
			{
				HandleError("Failed to start process: " + ex.Message);
				return null;
			}
		}

		protected static void ShowToast(string message)
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


		protected void WriteInfo(string message)
		{
			Console.ForegroundColor = InfoColor;
			Console.WriteLine(message);
			Console.ResetColor();
		}

		protected void WriteError(string message)
		{
			if (string.IsNullOrWhiteSpace(message))
				return;

			Console.ForegroundColor = ErrorColor;
			Console.WriteLine(message);
			Console.ResetColor();
		}

		protected void WriteDebug(string message)
		{
			if (!_isVerboseLoggingEnabled)
				return;
			Console.ForegroundColor = DebugColor;
			Console.WriteLine(message);
			Console.ResetColor();
		}


		public virtual void Dispose()
		{
			if (RunningProcess == null)
				return;

			RunningProcess.Kill();
			RunningProcess = null;
		}
	}
}