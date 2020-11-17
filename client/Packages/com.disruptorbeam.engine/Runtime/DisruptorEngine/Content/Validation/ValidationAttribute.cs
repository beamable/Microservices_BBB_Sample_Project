using System;
using System.Collections.Generic;
using System.Reflection;

namespace DisruptorBeam.Content.Validation
{
   [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
   public abstract class ValidationAttribute : Attribute
   {
      private static readonly List<Type> numericTypes = new List<Type>
      {
         typeof(byte),
         typeof(sbyte),
         typeof(short),
         typeof(ushort),
         typeof(int),
         typeof(uint),
         typeof(long),
         typeof(ulong),
         typeof(float),
         typeof(double),
         typeof(decimal)
      };

      protected static bool IsNumericType(Type type)
      {
         return numericTypes.Contains(type);
      }

      public abstract void Validate(FieldInfo field, ContentObject obj);
   }
}