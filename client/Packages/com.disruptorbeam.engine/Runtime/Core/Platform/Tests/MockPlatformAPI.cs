using System;
using System.Collections.Generic;
using System.Linq;
using Core.Platform.SDK;
using Core.Platform.SDK.Auth;
using Core.Serialization;
using UnityEngine;
using AccessToken = Core.Platform.SDK.AccessToken;

namespace Core.Platform.Tests
{
   public abstract class MockPlatformRouteBase
   {
      public Method Method;
      public string Uri;
      public bool IncludeAuthHeader = true;
      public string Token;
      protected object _response;


      public int CallCount { get; protected set; }
      public bool Called => CallCount > 0;

      public abstract Promise<T> Invoke<T>(object body, bool includeAuth, AccessToken token);
      public abstract bool MatchesRequest<T>(Method method, string uri, string token, object body, bool includeAuthHeader);

   }

   public class MockPlatformRoute<T> : MockPlatformRouteBase
   {
      private delegate bool BodyMatcher(object body);

      private List<BodyMatcher> _matchers = new List<BodyMatcher>();

      public MockPlatformRoute<T> WithResponse(T response)
      {
         _response = response;
         return this;
      }

      public MockPlatformRoute<T> WithNoAuthHeader(bool use = false)
      {
         IncludeAuthHeader = use;
         return this;
      }

      public MockPlatformRoute<T> WithToken(string token)
      {
         Token = token;
         return this;
      }

      public MockPlatformRoute<T> WithFormField(string name, string value)
      {
         _matchers.Add(body =>
         {
            if (body is WWWForm form)
            {
               var actual = form.GetField(name);
               return actual.Equals(value);
            }

            return false;
         });
         return this;
      }

      public override Promise<T1> Invoke<T1>(object body, bool includeAuth, AccessToken token)
      {
         CallCount ++ ;
         return Promise<T1>.Successful((T1)_response);
      }

      public override bool MatchesRequest<T1>(Method method, string uri, string token, object body, bool includeAuthHeader)
      {

         var tokenMatch = string.Equals(Token, token);
         var authMatch = includeAuthHeader == IncludeAuthHeader;
         var uriMatch = Uri.Equals(uri);
         var typeMatch = typeof(T1) == typeof(T);
         var methodMatch = Method.Equals(method);

         var simpleChecks = tokenMatch && authMatch && uriMatch && typeMatch && methodMatch;

         if (!simpleChecks)
         {
            return false;
         }

         return _matchers.All(matcher => matcher(body));
      }

   }

   public class MockPlatformAPI : IPlatformRequester
   {

      //private Dictionary<string, MockPlatformRouteBase> _routes = new Dictionary<string, MockPlatformRouteBase>();

      private List<MockPlatformRouteBase> _routes = new List<MockPlatformRouteBase>();

      public AuthService AuthService { get; set; }
      public AccessToken Token { get; set; }

      public MockPlatformAPI()
      {
         // brand new!
      }

      public MockPlatformAPI(MockPlatformAPI copy)
      {
         _routes = copy._routes; // shallow copy.
      }

      public Promise<T> RequestJson<T>(Method method, string uri, JsonSerializable.ISerializable body, bool includeAuthHeader = true)
      {
         throw new NotImplementedException();
      }

      public MockPlatformRoute<T> MockRequest<T>(Method method, string uri)
      {
         var route = new MockPlatformRoute<T>()
         {
            Method = method,
            Uri = uri
         };

         _routes.Add(route);

         return route;
      }

      public bool AllMocksCalled => _routes.All(mock => mock.Called);

      public Promise<T> Request<T>(Method method, string uri, object body = null, bool includeAuthHeader = true, Func<string, T> parser = null, bool useCache=false)
      {
         var route = _routes.FirstOrDefault(r => r.MatchesRequest<T>(method, uri, Token?.Token, body, includeAuthHeader));


         if (route != null)
         {
            return route.Invoke<T>(body, includeAuthHeader, Token);
         }
         else
         {
            throw new Exception($"No route mock available for call.");
         }
      }

      public Promise<T> RequestForm<T>(string uri, WWWForm form, bool includeAuthHeader = true)
      {
         return Request<T>(Method.POST, uri, form, includeAuthHeader);
      }

      public Promise<T> RequestForm<T>(string uri, WWWForm form, Method method, bool includeAuthHeader = true)
      {
         throw new NotImplementedException();
      }

      public IPlatformRequester WithAccessToken(TokenResponse token)
      {
         var clone = new MockPlatformAPI(this);
         clone.Token = new AccessToken(null, null, null, token.access_token, token.refresh_token, token.expires_in);
         return clone;
      }
   }
}