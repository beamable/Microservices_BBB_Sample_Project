using System;
using Beamable.Common.Api.Auth;
using Core.Platform.SDK;

namespace Beamable.Common.Api
{
   public enum Method
   {
      GET = 1,
      POST = 2,
      PUT = 3,
      DELETE = 4
   }

   [Serializable]
   public class EmptyResponse
   {
   }

   public interface IBeamableRequester
   {
      Promise<T> Request<T>(
         Method method,
         string uri,
         object body = null,
         bool includeAuthHeader = true,
         Func<string, T> parser = null,
         bool useCache = false
      );

      IBeamableRequester WithAccessToken(TokenResponse tokenResponse);

      string EscapeURL(string url);
   }
}