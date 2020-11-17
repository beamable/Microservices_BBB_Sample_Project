using DisruptorBeam;
using DisruptorBeam.Modules.Leaderboards;
using DisruptorBeam.UI.Scripts;
using UnityEngine;

public class LeaderboardMainMenu : MenuBase
{
   public LeaderboardItem LeaderboardItemPrefab;
   public LeaderboardBehavior LeaderboardBehavior;
   public Transform LeaderboardEntries;

   protected async void OnEnable()
   {
      var de = await DisruptorEngine.Instance;
      await de.LeaderboardService.GetBoard(LeaderboardBehavior.Leaderboard.Id, 0, 50).Then( board =>
      {
         // Clear all data
         for (var i = 0; i < LeaderboardEntries.childCount; i++)
         {
            Destroy(LeaderboardEntries.GetChild(i).gameObject);
         }

         // Populate lines
         foreach (var rank in board.rankings)
         {
            var leaderboardItem = Instantiate(LeaderboardItemPrefab, LeaderboardEntries);
            leaderboardItem.Apply(rank);
         }
      }).Error(err => Debug.LogError(err.Message + " Please check LeaderboardFlow GameObject has Leaderboard field set."));
   }
}