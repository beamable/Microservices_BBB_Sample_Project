using Beamable.Common.Content;
using DisruptorBeam.Content;

namespace Beamable.Server.Content
{
   public class MicroserviceContentSerializer : ContentSerializer<IContentObject>
   {
      protected override TContent CreateInstance<TContent>()
      {
         return new TContent();
      }
   }
}