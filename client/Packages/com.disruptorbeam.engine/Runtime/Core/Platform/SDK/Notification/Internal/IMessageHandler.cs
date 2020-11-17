using UnityEngine;

namespace Core.Platform.SDK.Notification.Internal
{
   public interface IMessageHandler<T>
   {
      int ChannelHistory();
      bool ShouldHandleMessage(T message);
      void OnMessageReceived(MonoBehaviour owner, T message);
   }
}