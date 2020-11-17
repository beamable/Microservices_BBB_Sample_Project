using System;
using System.Collections.Generic;
using Beamable.Common.Content;
using Core.Platform.SDK;
using DisruptorBeam.Content;
using DisruptorBeam.Editor.Content;
using Manifest = DisruptorBeam.Editor.Content.Manifest;

namespace DisruptorBeam.Editor.Tests.EditorDisruptorEngine.Content.ContentIOTests
{
   public class MockContentIO : IContentIO
   {
      public Func<Promise<Manifest>> FetchManifestResult = () => null;
      public Func<IEnumerable<ContentObject>> FindAllResult = () => null;
      public Func<IContentObject, string> ChecksumResult = c => "";


      public Promise<Manifest> FetchManifest()
      {
         return FetchManifestResult();
      }

      public IEnumerable<ContentObject> FindAll()
      {
         return FindAllResult();
      }

      public string Checksum(IContentObject content)
      {
         return ChecksumResult(content);
      }
   }
}