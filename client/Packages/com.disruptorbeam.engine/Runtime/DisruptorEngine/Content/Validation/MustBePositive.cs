using System.Reflection;

namespace DisruptorBeam.Content.Validation
{
   public sealed class MustBePositive : ValidationAttribute
   {
      private const string NUMERIC_TYPE = "Value must be a numeric type.";
      private const string LESS_THAN_ZERO = "Value is less than 0.";
      public override void Validate(FieldInfo field, ContentObject obj)
      {
         var fieldType = field.FieldType;
         if (!IsNumericType(fieldType))
         {
            throw new ContentValidationException(field, NUMERIC_TYPE);
         }

         var value = field.GetValue(obj);

         // XXX: Eww.. Would be nice if we could use the `dynamic` keyword.
         if (fieldType == typeof(sbyte))
         {
            if ((sbyte)value < 0)
            {
               throw new ContentValidationException(field, LESS_THAN_ZERO);
            }
         }
         else if (fieldType == typeof(short))
         {
            if ((short)value < 0)
            {
               throw new ContentValidationException(field, LESS_THAN_ZERO);
            }
         }
         else if (fieldType == typeof(int))
         {
            if ((int)value < 0)
            {
               throw new ContentValidationException(field, LESS_THAN_ZERO);
            }
         }
         else if (fieldType == typeof(long))
         {
            if ((long)value < 0)
            {
               throw new ContentValidationException(field, LESS_THAN_ZERO);
            }
         }
         else if (fieldType == typeof(float))
         {
            if ((float)value < 0)
            {
               throw new ContentValidationException(field, LESS_THAN_ZERO);
            }
         }
         else if (fieldType == typeof(double))
         {
            if ((double)value < 0)
            {
               throw new ContentValidationException(field, LESS_THAN_ZERO);
            }
         }
         else if (fieldType == typeof(decimal))
         {
            if ((decimal)value < 0)
            {
               throw new ContentValidationException(field, LESS_THAN_ZERO);
            }
         }
      }
   }
}