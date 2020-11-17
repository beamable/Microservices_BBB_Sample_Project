using System.Collections.Generic;
using Beamable.Common.Content;

namespace DisruptorBeam.Content
{
   [ContentType("announcements")]
   [System.Serializable]
   public class AnnouncementContent : ContentObject
   {
      public string channel;
      public string title;
      public string summary;
      public string body;
      public string start_date;
      public string end_date;
      public List<AnnouncementAttachment> attachments;
   }

   [System.Serializable]
   public class AnnouncementAttachment
   {
      public string symbol;
      public int count;
      public string type;
   }
}