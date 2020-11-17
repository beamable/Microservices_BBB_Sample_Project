using System;
using System.Threading;
using Beamable.Common;
using Serilog;

namespace Core.Server.Common
{
   public class BeamableSerilogProvider : BeamableLogProvider
   {
      public static AsyncLocal<ILogger> LogContext = new AsyncLocal<ILogger>();

      public static BeamableSerilogProvider Instance => BeamableLogProvider.Provider as BeamableSerilogProvider;

      public override void Info(string message)
      {
         LogContext.Value.Information(message);
      }

      public override void Info(string message, params object[] args)
      {
         LogContext.Value.Information(message, args);
      }

      public override void Warning(string message)
      {
         LogContext.Value.Warning(message);
      }

      public override void Error(Exception ex)
      {
         LogContext.Value.Error(ex, "[Exception]");
      }

      public void Debug(string message, params object[] args)
      {
         LogContext.Value.Debug(message, args);
      }
   }
}