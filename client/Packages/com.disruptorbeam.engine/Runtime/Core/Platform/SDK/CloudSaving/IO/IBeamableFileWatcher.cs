using System.IO;

namespace Core.Platform.SDK.CloudSaving.IO
{
   public interface IBeamableFileWatcher
   {
      bool EnableRaisingEvents { get; set; }
      string Path { get; set; }
      bool IncludeSubdirectories { get; set; }
      NotifyFilters NotifyFilter { get; set; }

      event FileSystemEventHandler Changed;

      event FileSystemEventHandler Created;

      event RenamedEventHandler Renamed;
   }
}