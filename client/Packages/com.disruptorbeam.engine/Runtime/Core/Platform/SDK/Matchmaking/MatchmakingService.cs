using System;
using System.Collections.Generic;

namespace Core.Platform.SDK.Matchmaking
{
    public class MatchmakingService
    {
        private PlatformRequester _requester;
        public MatchmakingService (PlatformRequester requester)
        {
            _requester = requester;
        }

        public Promise<MatchmakingResponse> Match () {
            return _requester.Request<MatchmakingResponse>(
                Method.POST, 
                $"/object/matchmaking/global/match"
            );
        }
    }
    
    [Serializable]
    public class MatchmakingResponse
    {
        public string game;
        public int ticksRemaining;
        public List<long> players;
    }
}