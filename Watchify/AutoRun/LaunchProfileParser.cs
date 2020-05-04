using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Watchify.AutoRun
{
	public class LaunchProfileParser
	{
		public async Task<string> GetApplicationUrl(DirectoryInfo projectDir, string launchProfileName = "Development")
		{
			var propsDir = new DirectoryInfo(Path.Join(projectDir.FullName, "Properties"));
			if (!propsDir.Exists) return null;
			
			var launchProfileFile = propsDir.GetFiles("launchSettings.json").FirstOrDefault();
			if (launchProfileFile == null) return null;
			
			var launchProfileContent = await JsonDocument.ParseAsync(File.OpenRead(launchProfileFile.FullName));
			if (!launchProfileContent.RootElement.TryGetProperty("profiles", out var profiles)) return null;

			if (!profiles.TryGetProperty(launchProfileName, out var actualProfile)) return null;
			
			if (actualProfile.TryGetProperty("launchUrl", out var launchUrl))
			{
				return launchUrl.GetString();
			}
			if (actualProfile.TryGetProperty("environmentVariables", out var envVars))
			{
				if (envVars.TryGetProperty("ASPNETCORE_URLS", out var aspnetCoreUrls))
				{
					return aspnetCoreUrls.GetString();
				}
			}

			return null;
		}
	}
}
