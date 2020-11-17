using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Beamable.Common;

namespace DisruptorBeam.Content.Validation
{
   [Agnostic]
   public class ContentValidationException : Exception
   {
      public MemberInfo Info { get; }

      public ContentValidationException(MemberInfo info) : base(generateMessage(info, ""))
      {
         Info = info;
      }

      public ContentValidationException(MemberInfo info, string message) : base(generateMessage(info, $" {message}"))
      {
         Info = info;
      }

      private static string generateMessage(MemberInfo info, string message)
      {
         return $"Error with '{info.MemberType} {info.Name}' on {info.DeclaringType}.{message}";
      }
   }

   public class ContentNameValidationException : Exception
   {
      public static Dictionary<char, string> INVALID_CHARACTERS = new Dictionary<char, string>
      {
         {' ', "spaces"},
         {'/', "forward slash"},
         {'\\', "back slashes"},
         {'|', "pipes"},
         {':', "colons"},
         {'*', "asterisks"},
         {'?', "question marks"},
         {'"', "double quotes"},
         {'<', "less than symbols"},
         {'>', "greater than symbols"},
      };

      public static char[] INVALID_CHARS = INVALID_CHARACTERS.Keys.ToArray();

      public char InvalidChar { get; }
      public int InvalidCharPosition { get; }
      public string Name { get; }
      public string InvalidCharName { get; }
      public ContentNameValidationException(char invalidChar, string invalidCharName, int position, string name)
         : base($"Content name=[{name}] cannot contain {invalidCharName} at position=[{position}]")
      {
         InvalidChar = invalidChar;
         InvalidCharName = invalidCharName;
         InvalidCharPosition = position;
         Name = name;
      }

      public static bool HasNameValidationErrors(string contentName, out List<ContentNameValidationException> errors)
      {
         errors = new List<ContentNameValidationException>();
         for (var i = 0; i < contentName.Length; i++)
         {
            var badCharIndex = Array.IndexOf(INVALID_CHARS, contentName[i]);
            if (badCharIndex >= 0)
            {
               var invalidChar = INVALID_CHARS[badCharIndex];
               errors.Add(new ContentNameValidationException(
                  invalidChar,
                  INVALID_CHARACTERS[invalidChar],
                  i,
                  contentName));
            }
         }

         return errors.Count > 0;
      }
   }
}