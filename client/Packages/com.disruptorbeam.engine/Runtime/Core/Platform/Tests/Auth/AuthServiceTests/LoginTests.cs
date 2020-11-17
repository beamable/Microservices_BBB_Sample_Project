using System.Collections;
using Core.Platform.SDK;
using Core.Platform.SDK.Auth;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Core.Platform.Tests.Auth.AuthServiceTests
{
   public class LoginTests : AuthServiceTestBase
   {

      [UnityTest]
      public IEnumerator MakesWebCall()
      {
         var password = "password";
         var email = "test@test.com";
         var merge = false;
         var result = new TokenResponse();
         
         var req = _requester.MockRequest<TokenResponse>(Method.POST, $"{TOKEN_URL}")
            .WithNoAuthHeader(merge)
            .WithFormField("username", email)
            .WithFormField("grant_type", "password")
            .WithFormField("password", password)
            .WithResponse(result);

         yield return _service.Login(email, password, merge)
            .Then(response => Assert.AreEqual(result, response))
            .AsYield();
         
         
         Assert.AreEqual(1, req.CallCount);
      }
      
   }
}