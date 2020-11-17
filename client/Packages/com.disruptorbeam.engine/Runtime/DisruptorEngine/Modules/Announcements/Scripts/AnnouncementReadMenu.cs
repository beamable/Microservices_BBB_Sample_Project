using Core.Platform.SDK.Announcements;
using DisruptorBeam;
using DisruptorBeam.UI.Scripts;
using TMPro;
using UnityEngine.UI;

public class AnnouncementReadMenu : MenuBase
{
   private AnnouncementView Announcement;
   public TextMeshProUGUI Text;
   public Button ButtonClaim;

   public void Apply(AnnouncementView view)
   {
      Announcement = view;
      string status = "";

      if (Announcement.isRead)
      {
         status += "IsRead  ";
      }
      if (Announcement.isClaimed)
      {
         status += "IsClaimed  ";
      }
      
      Text.text = status + "\n\n" + Announcement.body;
      
      ButtonClaim.gameObject.SetActive(Announcement.HasClaimsAvailable());
   }

   public async void OnClaim()
   {
      var de = await DisruptorEngine.Instance;
      await de.AnnouncementService.Claim(Announcement.id);
      Manager.GoBack();
   }
   
   public async void OnDelete()
   {
      var de = await DisruptorEngine.Instance;
      await de.AnnouncementService.MarkDeleted(Announcement.id);
      Manager.GoBack();
   }
}