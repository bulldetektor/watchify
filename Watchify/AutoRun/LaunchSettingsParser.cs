using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Watchify.AutoRun
{
	public class LaunchSettingsParser
	{
		public async Task<string> GetApplicationUrl(DirectoryInfo projectDir, string launchProfileName = "Development")
		{
			var propsDir = new DirectoryInfo(Path.Join(projectDir.FullName, "Properties"));
			if (!propsDir.Exists) return null;

			var launchSettingsFile = propsDir.GetFiles("launchSettings.json").FirstOrDefault();
			if (launchSettingsFile == null) return null;

			return await GetApplicationUrl(launchSettingsFile, launchProfileName);
		}

		public async Task<string> GetApplicationUrl(FileInfo launchSettingsFile, string launchProfileName = "Development")
		{
			string appUrl = null;
			try
			{
				if (launchSettingsFile == null) return null;

				await using var fileStream = File.OpenRead(launchSettingsFile.FullName);
				var launchProfileContent = await JsonDocument.ParseAsync(fileStream);
				if (!launchProfileContent.RootElement.TryGetProperty("profiles", out var profiles)) return null;

				if (profiles.TryGetProperty(launchProfileName, out var actualProfile))
				{
					if (actualProfile.TryGetProperty("launchUrl", out var launchUrl))
					{
						appUrl = launchUrl.GetString();
					}
					else if (actualProfile.TryGetProperty("environmentVariables", out var envVars))
					{
						if (envVars.TryGetProperty("ASPNETCORE_URLS", out var aspnetCoreUrls))
						{
							appUrl = aspnetCoreUrls.GetString();
						}
					}
				}
				else
				{
					foreach (var jsonProperty in profiles.EnumerateObject())
					{
						if (jsonProperty.Value.TryGetProperty("applicationUrl", out var au)
						    && jsonProperty.Value.TryGetProperty("environmentVariables", out var ev)
						    && ev.TryGetProperty("ASPNETCORE_ENVIRONMENT", out var evEnv)
						    && evEnv.GetString() == launchProfileName)
						{
							appUrl = au.GetString();
							break;
						}
					}

					if (string.IsNullOrEmpty(appUrl))
						return null;
				}
			}
			catch (Exception exception)
			{
				Console.WriteLine($"Failed to load launch setttings: {exception.Message}");
				return null;
			}

			return appUrl?.Split(';', StringSplitOptions.RemoveEmptyEntries)[0];
		}
	}
}
