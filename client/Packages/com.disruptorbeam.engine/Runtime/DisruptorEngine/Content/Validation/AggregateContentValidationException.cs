using System;
using System.Collections.Generic;

namespace DisruptorBeam.Content.Validation
{
   public class AggregateContentValidationException : AggregateException
   {
      public AggregateContentValidationException(IEnumerable<ContentValidationException> exs) : base(exs) {}
   }
}