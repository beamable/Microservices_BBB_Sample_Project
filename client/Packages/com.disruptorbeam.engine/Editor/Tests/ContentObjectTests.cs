using DisruptorBeam.Content;
using DisruptorBeam.Content.Validation;
using NUnit.Framework;

namespace DisruptorBeam.Editor.Tests
{
   public class ContentObjectTests
   {
      [Test]
      public void Validate_WellConfigured_Test()
      {
         var testContent = new TestContent(0);
         Assert.DoesNotThrow(testContent.Validate);

         testContent = new TestContent(20);
         Assert.DoesNotThrow(testContent.Validate);

         testContent = new TestContent(-32);
         Assert.Throws<AggregateContentValidationException>(testContent.Validate);
      }

      [Test]
      public void Validate_PoorlyConfigured_Test()
      {
         var content = new PoorlyConfiguredContent("boo!", -32);
         Assert.Throws<AggregateContentValidationException>(content.Validate);
      }
   }

   public class TestContent : ContentObject
   {
      [MustBePositive]
      public int number;

      public TestContent(int number)
      {
         this.number = number;
      }
   }

   public class PoorlyConfiguredContent : ContentObject
   {
      [MustBePositive]
      public string s;

      [MustBePositive]
      public int number;

      public PoorlyConfiguredContent(string s, int number)
      {
         this.s = s;
         this.number = number;
      }
   }
}
