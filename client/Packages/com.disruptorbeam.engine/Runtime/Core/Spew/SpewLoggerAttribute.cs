using System;
using System.Diagnostics;

namespace Core.Spew
{
   /// <summary>
   /// Conditional attribute to add Spew.
   /// </summary>
   [Conditional("UNITY_EDITOR")]
   [AttributeUsage(AttributeTargets.Class)]
   public sealed class SpewLoggerAttribute : Attribute {}
}
