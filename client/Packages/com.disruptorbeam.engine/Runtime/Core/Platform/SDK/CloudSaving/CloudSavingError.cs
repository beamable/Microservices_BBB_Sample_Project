using System;

namespace Core.Platform.SDK.CloudSaving
{
   public class CloudSavingError : Exception
   {
      public CloudSavingError(string message, Exception inner) : base(message, inner)
      {
      }
   }
}