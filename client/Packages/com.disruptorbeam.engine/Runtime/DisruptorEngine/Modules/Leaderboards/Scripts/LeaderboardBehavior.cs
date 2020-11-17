using System.Collections;
using DisruptorBeam.Content;
using DisruptorBeam.UI.Scripts;
using UnityEngine;

namespace DisruptorBeam.Modules.Leaderboards {


   [HelpURL(ContentConstants.URL_FEATURE_LEADERBOARD_FLOW)]
   public class LeaderboardBehavior : MonoBehaviour {

        public MenuManagementBehaviour MenuManager;
        public LeaderboardRef Leaderboard;

        public void Toggle(bool leaderboardDesiredState) {

            if (!leaderboardDesiredState && MenuManager.IsOpen){

                MenuManager.CloseAll();
            }
            else if (leaderboardDesiredState && !MenuManager.IsOpen){

                MenuManager.Show<LeaderboardMainMenu>();
            }
        }
    }
}