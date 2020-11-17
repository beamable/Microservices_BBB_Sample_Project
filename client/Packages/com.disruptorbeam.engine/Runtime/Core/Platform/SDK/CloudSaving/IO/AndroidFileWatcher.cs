using System.IO;
using UnityEngine;

namespace Core.Platform.SDK.CloudSaving.IO
{
   // TODO: Going to need to write an Android plugin to do this...

   public class AndroidFileWatcher : IBeamableFileWatcher
   {
#pragma warning disable 0169, 0649, 67
      private readonly AndroidJavaClass _watcher;
      public AndroidFileWatcher()
      {
         // _watcher = new AndroidJavaClass("android.os.FileObserver");
      }

      public bool EnableRaisingEvents { get; set; }
      public string Path { get; set; }
      public bool IncludeSubdirectories { get; set; }
      public NotifyFilters NotifyFilter { get; set; }
      public event FileSystemEventHandler Changed;
      public event FileSystemEventHandler Created;
      public event RenamedEventHandler Renamed;
#pragma warning restore 0169, 0649, 67
   }
}