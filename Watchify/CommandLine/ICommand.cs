namespace Watchify.CommandLine
{
	public interface ICommand
	{
		string Name { get; }
		string Description { get; }

		ICommand Configure(CommandLineOptions options);
		int Run();
	}
}