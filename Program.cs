namespace CurePlease
{
	using System;
	using System.IO;
	using System.Windows.Forms;

	internal static class Program
	{
		/// <summary>
		/// The main entry point for the application. 
		/// </summary>
		[STAThread]
		private static void Main()
		{
			Application.ThreadException += Application_ThreadException;
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new Form1());
		}

		private static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
		{
			File.AppendAllText("errors.log", $"{e.Exception}\r\n");
			Application.Exit();
		}
	}
}
