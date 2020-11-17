using UnityEngine;

namespace Core.Coroutines
{
   public class CompletedYieldInstruction : CustomYieldInstruction
   {
      public static readonly CompletedYieldInstruction Instance = new CompletedYieldInstruction();

      public override bool keepWaiting
      {
         get { return false; }
      }
   }
}