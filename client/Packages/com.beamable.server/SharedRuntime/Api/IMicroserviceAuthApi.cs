using Beamable.Common;
using Beamable.Common.Api.Auth;
using Core.Platform.SDK;

namespace Beamable.Server.Api
{
   public interface IMicroserviceAuthApi : IAuthApi
   {
      Promise<User> GetUser(long userId);
   }
}