using System.Collections;
using Core.Coroutines;
using Core.Platform.SDK;
using Core.Service;
using UnityEngine;

namespace DisruptorBeam
{
   public static class PromiseExtensions
   {

      public static Promise<T> WaitForSeconds<T>(this Promise<T> promise, float seconds)
      {
         var result = new Promise<T>();
         IEnumerator Wait()
         {
            yield return Yielders.Seconds(seconds);
            promise.Then(x => result.CompleteSuccess(x));
         };

         ServiceManager.Resolve<CoroutineService>().StartCoroutine(Wait());

         return result;
      }

      public static CustomYieldInstruction ToYielder<T>(this Promise<T> self)
      {
         return new PromiseYieldInstruction<T>(self);
      }
   }

   public class PromiseYieldInstruction<T> : CustomYieldInstruction
   {
      private readonly Promise<T> _promise;

      public PromiseYieldInstruction(Promise<T> promise)
      {
         _promise = promise;
      }

      public override bool keepWaiting => !_promise.IsCompleted;
   }
}