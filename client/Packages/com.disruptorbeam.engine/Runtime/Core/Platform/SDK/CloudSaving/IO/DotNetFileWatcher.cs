using System.IO;

namespace Core.Platform.SDK.CloudSaving.IO
{
   /// <summary>
   /// Pass through class implementing the IBeamableFileWatcher facade.
   /// </summary>
   public class DotNetFileWatcher : IBeamableFileWatcher
   {
      private readonly FileSystemWatcher _watcher;
      public DotNetFileWatcher()
      {
         _watcher = new FileSystemWatcher();
      }

      public bool EnableRaisingEvents
      {
         get => _watcher.EnableRaisingEvents;
         set => _watcher.EnableRaisingEvents = value;
      }

      public string Path
      {
         get => _watcher.Path;
         set => _watcher.Path = value;
      }

      public bool IncludeSubdirectories
      {
         get => _watcher.IncludeSubdirectories;
         set => _watcher.IncludeSubdirectories = value;
      }

      public NotifyFilters NotifyFilter
      {
         get => _watcher.NotifyFilter;
         set => _watcher.NotifyFilter = value;
      }

      public event FileSystemEventHandler Changed
      {
         add => _watcher.Changed += value;
         remove => _watcher.Changed -= value;
      }

      public event FileSystemEventHandler Created
      {
         add => _watcher.Created += value;
         remove => _watcher.Created -= value;
      }

      public event RenamedEventHandler Renamed
      {
         add => _watcher.Renamed += value;
         remove => _watcher.Renamed -= value;
      }
   }
}