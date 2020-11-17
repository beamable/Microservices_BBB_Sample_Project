using Beamable.Common;
using Beamable.Common.Api;
using Beamable.Common.Api.Auth;
using Core.Platform.SDK;

namespace Beamable.Server.Api
{
   public class ServerAuthApi : IMicroserviceAuthApi
   {
      public const string BASIC_SERVICE = "/basic/accounts";
      public const string OBJECT_SERVICE = "/object/accounts";

      public IBeamableRequester Requester { get; }
      public RequestContext Context { get; }

      public ServerAuthApi(IBeamableRequester requester, RequestContext context)
      {
         Requester = requester;
         Context = context;
      }

      public Promise<User> GetUser()
      {
         return Requester.Request<User>(Method.GET, $"{BASIC_SERVICE}/me");
         //return GetUser(Context.UserId);
      }

      public Promise<User> GetUser(long userId)
      {
         return Requester.Request<User>(Method.GET, $"{BASIC_SERVICE}", new
         {
            gamerTag = userId
         });
      }

      public Promise<User> GetUser(TokenResponse token)
      {
         throw new System.NotImplementedException();
      }

      public Promise<bool> IsEmailAvailable(string email)
      {
         return Requester.Request<AvailabilityResponse>(Method.GET, $"{BASIC_SERVICE}/available", new
         {
            email
         }).Map(res => res.available);
      }

      public Promise<bool> IsThirdPartyAvailable(AuthThirdParty thirdParty, string token)
      {
         throw new System.NotImplementedException();
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
         throw new System.NotImplementedException();
      }

      public Promise<TokenResponse> LoginThirdParty(AuthThirdParty thirdParty, string thirdPartyToken, bool includeAuthHeader = true)
      {
         throw new System.NotImplementedException();
      }

      public Promise<User> RegisterDBCredentials(string email, string password)
      {
         throw new System.NotImplementedException();
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