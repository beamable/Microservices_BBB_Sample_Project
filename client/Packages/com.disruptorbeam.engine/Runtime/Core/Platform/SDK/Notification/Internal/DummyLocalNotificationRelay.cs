using System;
using System.Collections.Generic;

namespace Core.Platform.SDK.Notification.Internal
{
   /// <summary>
   /// Dummy local notification handler, for use as a stand-in within
   /// Editor and other contexts where OS notifications are not available.
   /// </summary>
   public class DummyLocalNotificationRelay : ILocalNotificationRelay
   {
      public void CreateNotificationChannel(string id, string name, string description)
      {
         Core.Spew.NotificationLogger.LogFormat("[DummyLocalNote] Create notification channel. id={0}, name={1}, description={2}.", id, name, description);
      }

      public void ScheduleNotification(string channel, string key, string title, string message, DateTime when, Dictionary<string, string> data)
      {
         Core.Spew.NotificationLogger.LogFormat("[DummyLocalNote] Schedule notification. channel={0}, key={1}, title={2}, message={3}.", channel, key, title, message);
      }

      public void CancelNotification(string key)
      {
         Core.Spew.NotificationLogger.LogFormat("[DummyLocalNote] Cancel notification. key={0}.", key);
      }

      public void ClearDeliveredNotifications()
      {
         Core.Spew.NotificationLogger.LogFormat("[DummyLocalNote] Cancel all notifications.");
      }
   }
}
