using System;
using Core.Platform.SDK;
using Core.Platform.SDK.Auth;

namespace Packages.DisruptorEngine.Runtime.Tests.DisruptorEngine
{
   public class MockAuthService : IAuthService
   {
      public Func<string, Promise<bool>> IsEmailAvailableDelegate;
      public Func<string, string, Promise<User>> RegisterDbCredentialsDelegate;
      public Func<string, string, bool, Promise<TokenResponse>> LoginDelegate;
      public Func<TokenResponse, Promise<User>> GetUserDelegate;
      public Func<Promise<User>> GetCurrentUserDelegate;
      
      
      public Promise<User> GetUser()
      {
         return GetCurrentUserDelegate();
      }

      public Promise<User> GetUser(TokenResponse token)
      {
         return GetUserDelegate(token);
      }

      public Promise<bool> IsEmailAvailable(string email)
      {
         return IsEmailAvailableDelegate(email);
      }

      public Promise<bool> IsThirdPartyAvailable(AuthThirdParty thirdParty, string token)
      {
         throw new NotImplementedException();
      }

      public Promise<TokenResponse> CreateUser()
      {
         throw new System.NotImplementedException();
      }

      public Promise<TokenResponse> LoginRefreshToken(string refreshToken)
      {
         throw new System.NotImplementedException();
      }

      public Promise<TokenResponse> Login(string username, string password, bool mergeGamerTagToAccount = true)
      {
         return LoginDelegate(username, password, mergeGamerTagToAccount);
      }

      public Promise<TokenResponse> LoginThirdParty(AuthThirdParty thirdParty, string thirdPartyToken, bool includeAuthHeader = true)
      {
         throw new System.NotImplementedException();
      }

      public Promise<User> RegisterDBCredentials(string email, string password)
      {
         return RegisterDbCredentialsDelegate(email, password);
      }

      public Promise<User> RegisterThirdPartyCredentials(AuthThirdParty thirdParty, string accessToken)
      {
         throw new System.NotImplementedException();
      }

      public Promise<EmptyResponse> IssueEmailUpdate(string newEmail)
      {
         throw new System.NotImplementedException();
      }

      public Promise<EmptyResponse> ConfirmEmailUpdate(string code, string password)
      {
         throw new System.NotImplementedException();
      }

      public Promise<EmptyResponse> IssuePasswordUpdate(string email)
      {
         throw new System.NotImplementedException();
      }

      public Promise<EmptyResponse> ConfirmPasswordUpdate(string code, string newPassword)
      {
         throw new System.NotImplementedException();
      }

      public Promise<CustomerRegistrationResponse> RegisterDisruptorEngineCustomer(string email, string password, string projectName)
      {
         throw new System.NotImplementedException();
      }
   }
}