using System.Collections;
using Core.Platform.SDK;
using Core.Platform.SDK.Auth;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Core.Platform.Tests.Auth.AuthServiceTests
{
   public class IsEmailAvailableTests : AuthServiceTestBase
   {
      [UnityTest]
      public IEnumerator MakesWebCall()
      {
         var email = "test+ext@test.com";
         
         _requester.MockRequest<AvailabilityResponse>(Method.GET, $"{ROUTE}/available?email=test%2bext%40test.com")
            .WithNoAuthHeader()
            .WithResponse(new AvailabilityResponse(){ available = true});

         yield return _service.IsEmailAvailable(email).AsYield();
         
         Assert.AreEqual(true, _requester.AllMocksCalled);
      }
      
   }
}