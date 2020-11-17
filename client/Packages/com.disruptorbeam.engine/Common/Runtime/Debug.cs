using System;

namespace Beamable.Common
{
   public abstract class BeamableLogProvider
   {

      private static BeamableLogProvider _provider;

      public static BeamableLogProvider Provider
      {
         get
         {
            if (_provider == null)
            {
               #if UNITY_EDITOR || UNITY_ENGINE
               Register(new BeamableLogUnityProvider());
               #endif
            }
            return _provider;
         }
         private set { _provider = value; }
      }
      public static void Register(BeamableLogProvider provider)
      {
         if (_provider != null)
         {
            throw new Exception("Cannot register multiple beamable log providers.");
         }

         _provider = provider;
      }

      public abstract void Info(string message);
      public abstract void Info(string message, params object[] args);
      public abstract void Warning(string message);
      public abstract void Error(Exception ex);
   }

   #if UNITY_EDITOR || UNITY_ENGINE
   public class BeamableLogUnityProvider : BeamableLogProvider
   {
      public override void Info(string message)
      {
         UnityEngine.Debug.Log(message);
      }

      public override void Info(string message, params object[] args)
      {
         UnityEngine.Debug.Log(string.Format(message, args));
      }

      public override void Warning(string message)
      {
         UnityEngine.Debug.LogWarning(message);
      }

      public override void Error(Exception ex)
      {
         UnityEngine.Debug.LogException(ex);
      }
   }
   #endif

   /// <summary>
   /// The Beamable Debug is a simple mock of the UnityEngine Debug class.
   /// The intention is not to replicate the entire set of functionality from Unity's Debug class,
   /// but to provide an easy reflexive log solution for dotnet core code.
   /// </summary>
   public class Debug
   {
      public static void Log(string info)
      {
         BeamableLogProvider.Provider.Info(info);
      }

      public static void Log(string info, params object[] args)
      {
         BeamableLogProvider.Provider.Info(info, args);
      }

      public static void LogWarning(string warning)
      {
         BeamableLogProvider.Provider.Warning(warning);

      }

      public static void LogException(Exception ex)
      {
         BeamableLogProvider.Provider.Error(ex);
      }

      public static void LogError(Exception ex)
      {
         BeamableLogProvider.Provider.Error(ex);
      }
   }
}