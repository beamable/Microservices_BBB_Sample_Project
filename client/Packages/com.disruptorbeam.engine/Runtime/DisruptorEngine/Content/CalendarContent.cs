using System.Collections.Generic;
using Beamable.Common.Content;
using Core.Platform.SDK.Calendars;

namespace DisruptorBeam.Content
{
   [ContentType("calendars")]
   public class CalendarContent : ContentObject
   {
      public OptionalString start_date;
      public OptionalString requirement;
      public List<RewardCalendarDay> days;
   }

   [System.Serializable]
   public class CalendarRef : ContentRef<CalendarContent> {}
}