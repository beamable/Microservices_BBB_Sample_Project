using System;
using Beamable.Common;
using Beamable.Server;
using Core.Server.Common;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting.Compact;

namespace Beamable.Server
{
   public static class MicroserviceBootstrapper
   {
      public static LoggingLevelSwitch LogLevel;

      private static void ConfigureLogging()
      {
         BeamableLogProvider.Register(new BeamableSerilogProvider());
         BeamableSerilogProvider.LogContext.Value = Log.Logger;

         // TODO pull "LOG_LEVEL" into a const?
         var logLevel = Environment.GetEnvironmentVariable("LOG_LEVEL") ?? "debug";
         var envLogLevel = (LogEventLevel)Enum.Parse(typeof(LogEventLevel), logLevel, true);

         // The LoggingLevelSwitch _could_ be controlled at runtime, if we ever wanted to do that.
         LogLevel = new LoggingLevelSwitch {MinimumLevel = envLogLevel};

         // https://github.com/serilog/serilog/wiki/Configuration-Basics
         Log.Logger = new LoggerConfiguration()
            .MinimumLevel.ControlledBy(LogLevel)
            .WriteTo.Console(new RenderedCompactJsonFormatter())
            .CreateLogger();
      }

      public static void Start<TMicroService>() where TMicroService : Microservice, new()
      {
         ConfigureLogging();

         var beamableService = new BeamableMicroService();
         var args = new EnviornmentArgs();

         var factory = new ServiceFactory<TMicroService>(() => new TMicroService());
         beamableService.Start(factory, args);
      }
   }
}