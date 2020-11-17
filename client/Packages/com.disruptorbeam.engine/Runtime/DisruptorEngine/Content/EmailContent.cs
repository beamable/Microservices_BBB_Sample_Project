using Beamable.Common;
using Beamable.Common.Content;

namespace DisruptorBeam.Content
{
   [ContentType("emails")]
   [System.Serializable]
   [Agnostic]
   public class EmailContent : ContentObject
   {
      public string subject, body;
   }

   [System.Serializable]
   [Agnostic]
   public class EmailRef : ContentRef<EmailContent> {}
}