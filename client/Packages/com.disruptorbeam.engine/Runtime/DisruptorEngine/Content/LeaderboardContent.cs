using Beamable.Common;
using Beamable.Common.Content;

namespace DisruptorBeam.Content
{
    [ContentType("leaderboards")]
    [System.Serializable]
    [Agnostic]
    public class LeaderboardContent : ContentObject
    {
        public ClientPermissions permissions;
    }

    [System.Serializable]
    [Agnostic]
    public class ClientPermissions
    {
        public bool write_self;
    }

    [System.Serializable]
    [Agnostic]
    public class LeaderboardLink : ContentLink<LeaderboardContent> {}

    [System.Serializable]
    [Agnostic]
    public class LeaderboardRef : ContentRef<LeaderboardContent> {}
}