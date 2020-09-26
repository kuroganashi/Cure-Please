using System.IO;
using System.Reactive.Subjects;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting;
using Serilog.Formatting.Display;

namespace CurePlease
{
  public class UiLogSink : ILogEventSink
	{
		private static readonly ITextFormatter formatter = new MessageTemplateTextFormatter("{Timestamp:HH:mm:ss.fff} [{Level}] {Message}{Exception}", null);
		public static ISubject<string> Output { get; set; } = new BehaviorSubject<string>("Ready");

		public void Emit(LogEvent logEvent)
		{
			if (logEvent != null)
			{
				var writer = new StringWriter();
				formatter.Format(logEvent, writer);
				Output.OnNext(writer.ToString());
			}
		}
	}
}
