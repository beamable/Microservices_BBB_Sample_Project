using System;
using System.Collections.Generic;
using Core.Platform.SDK;
using Core.Platform.SDK.Announcements;
using Core.Platform.SDK.Auth;
using Core.Platform.SDK.Calendars;
using Core.Platform.SDK.Chat;
using Core.Platform.SDK.Commerce;
using Core.Platform.SDK.CloudSaving;
using Core.Platform.SDK.Inventory;
using Core.Platform.SDK.Leaderboard;
using Core.Platform.SDK.Matchmaking;
using Core.Platform.SDK.Payments;
using Core.Platform.SDK.Sim;
using Core.Platform.SDK.Stats;
using Core.Platform.SDK.Tournaments;
using DisruptorBeam;
using DisruptorBeam.Content;

namespace Packages.DisruptorEngine.Runtime.Tests.DisruptorEngine
{
   public class MockDisruptorEngine : IDisruptorEngine
   {
      public User User { get; set; }
      public AccessToken Token { get; }
      public AnnouncementsService AnnouncementService { get; set; }
      public MockAuthService MockAuthService { get; set; } = new MockAuthService();
      public IAuthService AuthService => MockAuthService;
      public CalendarsService CalendarsService { get; set; }
      public ChatService ChatService { get; set; }

      public CloudSavingService CloudSavingService { get; set; }
      public ContentService ContentService { get; set; }
      public GameRelayService GameRelayService { get; set; }
      public InventoryService InventoryService { get; set; }
      public LeaderboardService LeaderboardService { get; set; }
      public PlatformRequester Requester { get; set; }
      public StatsService Stats { get; }
      public CommerceService Commerce { get; }
      public MatchmakingService Matchmaking { get; }
      public Promise<PaymentDelegate> PaymentDelegate { get; }
      public ConnectivityService ConnectivityService { get;  }
      public ITournamentService Tournaments { get; }

      public event Action<User> OnUserChanged;
      public event Action<bool> OnConnectivityChanged;

      public Func<TokenResponse, Promise<Unit>> ApplyTokenDelegate;
      public Func<Promise<ISet<UserBundle>>> GetDeviceUsersDelegate;

      public void UpdateUserData(User user)
      {
         User = user;
         TriggerOnUserChanged(user);
      }

      public void TriggerOnUserChanged(User user)
      {
         OnUserChanged?.Invoke(user);
      }

      public void TriggerOnConnectivityChanged(bool isSuccessful)
      {
         OnConnectivityChanged?.Invoke(isSuccessful);
      }

      public Promise<ISet<UserBundle>> GetDeviceUsers()
      {
         return GetDeviceUsersDelegate();
      }

      public void RemoveDeviceUser(TokenResponse token)
      {
         throw new NotImplementedException();
      }

      public Promise<Unit> ApplyToken(TokenResponse response)
      {
         var promise = ApplyTokenDelegate(response);
         TriggerOnUserChanged(null);
         return promise;
      }
   }
}