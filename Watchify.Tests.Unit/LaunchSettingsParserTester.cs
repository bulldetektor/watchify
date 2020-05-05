using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Watchify.AutoRun;
using Xunit;

namespace Watchify.Tests.Unit
{
	public class LaunchSettingsParserTester
	{
		private readonly LaunchSettingsParser _parser;

		public LaunchSettingsParserTester()
		{
			_parser = new LaunchSettingsParser();
		}

		[Fact]
		public async Task Gets_the_application_url()
		{
			var testFilePath = Directory.EnumerateFiles(Environment.CurrentDirectory, "launchSettings.testprofiles.json", SearchOption.AllDirectories).First();
			var testProfiles = new FileInfo(testFilePath);

			var actual = await _parser.GetApplicationUrl(testProfiles);

			actual.Should().Be("https://localhost:5001");
		}
	}
}
