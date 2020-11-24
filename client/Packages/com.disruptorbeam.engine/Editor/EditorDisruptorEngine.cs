using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Core.Config;
using Core.Platform.SDK;
using Core.Platform.SDK.Auth;
using DisruptorBeam.Editor.Environment;
using UnityEditor;
using DisruptorBeam.Editor.Content;
using UnityEditor.AddressableAssets;
using UnityEngine;

namespace DisruptorBeam.Editor
{
   public class EditorDisruptorEngine
   {
      private static Promise<EditorDisruptorEngine> _instance;
      public PlatformRequester Requester => _requester;
      public static Promise<EditorDisruptorEngine> Instance
      {
         get
         {
            if (_instance == null)
            {
               var de = new EditorDisruptorEngine();
               _instance = de.Initialize().Error(err =>
               {
                  Debug.LogError(err);
                  de.Logout();
                  _instance = null;
               });
            }

            return _instance;
         }
      }

      // Services
      private AccessTokenStorage _accessTokenStorage;
      private PlatformRequester _requester;
      public AuthService AuthService;
      public ContentIO ContentIO;
      public ContentPublisher ContentPublisher;
      public event Action<User> OnUserChange;

      // Info
      public string Cid
      {
         get => _requester.Cid;
         set => _requester.Cid = value;
      }

      public string Pid
      {
         get => _requester.Pid;
         set => _requester.Pid = value;
      }

      public string Host => _requester.Host;

      public User User;
      public AccessToken Token => _requester.Token;

      private Promise<EditorDisruptorEngine> Initialize()
      {
         // Register services
         BeamableEnvironment.ReloadEnvironment();
         _accessTokenStorage = new AccessTokenStorage("editor.");
         _requester = new PlatformRequester(BeamableEnvironment.ApiUrl, _accessTokenStorage, null);
         AuthService = new AuthService(_requester);
         ContentIO = new ContentIO(_requester);
         ContentPublisher = new ContentPublisher(_requester, ContentIO);

         if (!ConfigDatabase.HasConfigFile(ConfigDatabase.GetConfigFileName()))
         {
            ApplyConfig("", "", BeamableEnvironment.ApiUrl);
            return Promise<EditorDisruptorEngine>.Successful(this);
         }

         ConfigDatabase.Init();
         ApplyConfig(
            ConfigDatabase.GetString("cid"),
            ConfigDatabase.GetString("pid"),
            ConfigDatabase.GetString("platform")
         );

         return _accessTokenStorage.LoadTokenForRealm(Cid, Pid).FlatMap(token =>
         {
            if (token == null)
               return Promise<EditorDisruptorEngine>.Successful(this);
            return InitializeWithToken(token).Error(err => { Logout(); });
         });
      }

      public void Logout()
      {
         _requester.DeleteToken();
         User = null;
         OnUserChange?.Invoke(null);
         BeamableEnvironment.ReloadEnvironment();
      }

      public bool HasDependencies()
      {
         var hasAddressables = null != AddressableAssetSettingsDefaultObject.GetSettings(false);
         var hasTextmeshPro = TextMeshProImporter.EssentialsLoaded;

         return hasAddressables && hasTextmeshPro;
      }

      public Promise<Unit> CreateDependencies()
      {
         // import addressables...
         AddressableAssetSettingsDefaultObject.GetSettings(true);

         return TextMeshProImporter.ImportEssentials().Then(_ =>
         {
            AssetDatabase.Refresh();
         });
      }

      public void SaveConfig(string cid, string pid, string host = null)
      {
         if (string.IsNullOrEmpty(host))
         {
            host = BeamableEnvironment.ApiUrl;
         }

         var config = new ConfigData()
         {
            cid = cid,
            pid = pid,
            platform = host,

            //Use dev URL - srivello
            socket = "wss://thorium-dev.disruptorbeam.com/socket"
         };

         var asJson = JsonUtility.ToJson(config, true);
         Directory.CreateDirectory("Assets/DisruptorEngine/Resources/");
         string path = "Assets/DisruptorEngine/Resources/config-defaults.txt";
         File.WriteAllText(path, asJson);
         AssetDatabase.Refresh();
         ApplyConfig(cid, pid, host);
      }

      public Promise<EditorDisruptorEngine> ApplyToken(TokenResponse tokenResponse)
      {
         var token = new AccessToken(_accessTokenStorage, Cid, Pid, tokenResponse.access_token,
            tokenResponse.refresh_token, tokenResponse.expires_in);
         return token.Save().FlatMap(_ => InitializeWithToken(token));
      }

      public Promise<string> GetRealmSecret()
      {
         // TODO this will only work if the current user is an admin.

         return _requester.Request<CustomerResponse>(Method.GET, "/basic/realms/customer").Map(resp =>
         {
            var matchingProject = resp.customer.projects.FirstOrDefault(p => p.name.Equals(Pid));
            return matchingProject?.secret ?? "";
         });
      }

      private void ApplyConfig(string cid, string pid, string host)
      {
         Cid = cid;
         Pid = pid;
         _requester.Host = host;
      }


      private Promise<EditorDisruptorEngine> InitializeWithToken(AccessToken token)
      {
         _requester.Token = token;
         return AuthService.GetUser().Map(user =>
         {
            User = user;
            OnUserChange?.Invoke(user);
            return this;
         });
      }


   }

   [System.Serializable]
   public class ConfigData
   {
      public string cid, pid, platform;
      public string socket;
   }

   [System.Serializable]
   public class CustomerResponse
   {
      public CustomerDTO customer;
   }

   [System.Serializable]
   public class CustomerDTO
   {
      public List<ProjectDTO> projects;
   }

   [System.Serializable]
   public class ProjectDTO
   {
      public string name;
      public string secret;
   }
}