using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core.Coroutines;
using Core.Platform.SDK;
using Core.Platform.SDK.Announcements;
using Core.Platform.SDK.Auth;
using DisruptorBeam;
using DisruptorBeam.Modules.AccountManagement;
using DisruptorBeam.UI.Layouts;
using DisruptorBeam.UI.Scripts;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class AnnouncementMainMenu : MenuBase
{
   public AnnouncementSummary SummaryPrefab;
   public Transform AnnouncementList;
   private PlatformSubscription<AnnouncementQueryResponse> Subscription;
   private List<AnnouncementView> Announcements;

   protected async void Start()
   {
      var de = await DisruptorEngine.Instance;
      Subscription = de.AnnouncementService.Subscribe(announcements =>
      {
         Announcements = announcements.announcements;
         
         // Clear all data
         for (var i = 0; i < AnnouncementList.childCount; i++)
         {
            Destroy(AnnouncementList.GetChild(i).gameObject);
         }

         // Populate summaries
         foreach (var announcement in Announcements)
         {
            var summary = Instantiate(SummaryPrefab, AnnouncementList);
            summary.Apply(Manager, announcement);
         }
      });
   }

   private void OnDestroy()
   {
      Subscription.Unsubscribe();
   }

   public async void OnReadAll()
   {
      List<string> ids = new List<string>();
      foreach (var announcement in Announcements)
      {
         ids.Add(announcement.id);
      }

      var de = await DisruptorEngine.Instance;
      await de.AnnouncementService.MarkRead(ids);
   }
   
   public async void OnClaimAll()
   {
      List<string> ids = new List<string>();
      foreach (var announcement in Announcements)
      {
         ids.Add(announcement.id);
      }

      var de = await DisruptorEngine.Instance;
      await de.AnnouncementService.Claim(ids);
   }
   
   public async void OnDeleteAll()
   {
      List<string> ids = new List<string>();
      foreach (var announcement in Announcements)
      {
         ids.Add(announcement.id);
      }

      var de = await DisruptorEngine.Instance;
      await de.AnnouncementService.MarkDeleted(ids);
   }
}