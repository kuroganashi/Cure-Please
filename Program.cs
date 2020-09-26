namespace CurePlease
{
	using System;
	using System.IO;
	using System.Windows.Forms;
  using Serilog;
  using Serilog.Events;

  internal static class Program
	{

		/// <summary>
		/// The main entry point for the application. 
		/// </summary>
		[STAThread]
		private static void Main()
		{
			ConfigureLogger();
			Application.ThreadException += Application_ThreadException;
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new Form1());
		}

    private static void ConfigureLogger()
    {
			var guid = Guid.NewGuid().ToString("n");

			Log.Logger = new LoggerConfiguration()
				.MinimumLevel.Debug()
				.MinimumLevel.Override("CurePlease", LogEventLevel.Verbose)
				.WriteTo.Sink(new UiLogSink(), LogEventLevel.Verbose)
				.WriteTo.File($"logs\\debug.{guid}.log", 
					restrictedToMinimumLevel: LogEventLevel.Information,
					flushToDiskInterval: TimeSpan.FromSeconds(2),
					shared: true)
				.WriteTo.Debug()
				.CreateLogger();
    }

    private static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
		{
			Log.Fatal(e.Exception, "Unhandled thread exception");
			File.AppendAllText("errors.log", $"{e.Exception}\r\n");
			Application.Exit();
		}
	}
}
