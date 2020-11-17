using System.Collections.Generic;
using Beamable.Common;
using Beamable.Common.Api;
using Core.Platform.SDK;

namespace Beamable.Server.Api
{
   public class MicroserviceStatsApi : IMicroserviceStatsApi
   {
      private const string OBJECT_SERVICE = "object/stats";

      public MicroserviceRequester Requester { get; }
      public RequestContext Context { get; }

      public MicroserviceStatsApi(MicroserviceRequester requester, RequestContext context)
      {
         Requester = requester;
         Context = context;
      }

      public Promise<string> GetProtectedPlayerStat(long userId, string stat)
      {
         return GetStats("game", "private", "player", userId, new string[]
            {
               stat
            }
            ).Map(res => res.GetValueOrDefault(stat));
      }

      public Promise<Dictionary<string, string>> GetProtectedPlayerStats(long userId, string[] stats)
      {
         return GetStats("game", "private", "player", userId, stats);
      }

      public Promise<EmptyResponse> SetProtectedPlayerStat(long userId, string key, string value)
      {
         return SetStats("game", "private", "player", userId, new Dictionary<string, string>
            {
               {key, value}
            });
      }

      public Promise<EmptyResponse> SetProtectedPlayerStats(long userId, Dictionary<string, string> stats)
      {
         return SetStats("game", "private", "player", userId, stats);
      }

      public Promise<EmptyResponse> SetStats(string domain, string access, string type, long userId, Dictionary<string, string> stats)
      {
         var key = $"{domain}.{access}.{type}.{userId}";
         return Requester.Request<EmptyResponse>(Method.POST, $"{OBJECT_SERVICE}/{key}", new StatUpdates(stats));
      }

      public Promise<Dictionary<string, string>> GetStats(string domain, string access, string type, long userId,
         string[] stats)
      {
         var key = $"{domain}.{access}.{type}.{userId}";
         var statString = string.Join(",", stats);
         return Requester.Request<StatsResponse>(Method.GET, $"{OBJECT_SERVICE}/{key}", new
         {
            stats = statString
         }).Map(res => res.stats);
      }

   }

#pragma warning disable 0649
   class StatsResponse
   {


      public long id;
      public Dictionary<string, string> stats;
   }



   class StatUpdates
   {
      public Dictionary<string, string> set;

      public StatUpdates(Dictionary<string, string> stats)
      {
         set = stats;
      }
   }
#pragma warning restore 0649

}