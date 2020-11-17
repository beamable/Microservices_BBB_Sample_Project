using System.Collections;
using DisruptorBeam.UI.Scripts;
using UnityEngine;

namespace DisruptorBeam.Modules.Announcements {

    public class AnnouncementBehavior : MonoBehaviour {

        public MenuManagementBehaviour MenuManager;

        public void Toggle(bool announcementDesiredState) {

            if (!announcementDesiredState && MenuManager.IsOpen){

                MenuManager.CloseAll();
            }
            else if (announcementDesiredState && !MenuManager.IsOpen){

                MenuManager.Show<AnnouncementMainMenu>();
            }
        }
    }
}