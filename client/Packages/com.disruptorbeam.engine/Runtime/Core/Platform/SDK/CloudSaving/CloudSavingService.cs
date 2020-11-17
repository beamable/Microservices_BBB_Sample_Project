using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using UnityEngine.Networking;
using UnityEngine;
using Core.Coroutines;
using System.Collections;
using System.Linq;
using System.Collections.Concurrent;
using Core.Platform.SDK.CloudSaving.IO;

namespace Core.Platform.SDK.CloudSaving
{
   public class CloudSavingService : PlatformSubscribable<ManifestResponse, ManifestResponse>
   {
      private const string ServiceName = "cloudsaving";
      private string _manifestPath;
      private ManifestResponse _localManifest;
      private PlatformService _platform;
      private PlatformRequester _requester;
      private WaitForSeconds _delay;
      private CoroutineService _coroutineService;
      private ConcurrentDictionary<string, string> _pendingUploads = new ConcurrentDictionary<string, string>();
      private ConcurrentDictionary<string, string> _previouslyDownloaded = new ConcurrentDictionary<string, string>();
      private List<KeyValuePair<string, string>> _downloadMap = new List<KeyValuePair<string, string>>();

      private IEnumerator _reinitializeUserDataCoroutine;
      private IEnumerator _batchFilesForUploadCoroutine;

#if UNITY_ANDROID || UNITY_IOS
      // For whatever reason, it seems as though Unity has an open issue with the .Net file watcher
      // on Android. For now, to ensure that our customers don't have to deal with any weirdness, lets
      // use a little wrapper around our small Android plugin.
      private readonly IBeamableFileWatcher _watcher = new AndroidFileWatcher();
#else
      private readonly IBeamableFileWatcher _watcher = new DotNetFileWatcher();
#endif

      private int _retryCounter = 0;

      public Action<ManifestResponse> updateReceived;
      public Action<CloudSavingError> OnError;

      private bool _allowEvents = true;
      private ConnectivityService _connectivityService;

      private LocalCloudDataPath localCloudDataPath => new LocalCloudDataPath(_platform);

      public string LocalCloudDataFullPath => localCloudDataPath.fullPath;

      public CloudSavingService(PlatformService platform, PlatformRequester requester,
         CoroutineService coroutineService) : base(platform, requester, ServiceName)
      {
         _platform = platform;
         _requester = requester;
         _delay = new WaitForSeconds(3);
         _coroutineService = coroutineService;
         _connectivityService = new ConnectivityService(_coroutineService);
      }

      public void Init(object user = null)
      {
         Initialize(user);
      }

      private Promise<Unit> Initialize(object user = null)
      {
         StopRoutines();
         return SetLocalManifest().Then(init =>
         {
            ListUserCloudData().Then(resp =>
               {
                  if (resp.replacement)
                  {
                     _reinitializeUserDataCoroutine = ReinitializeUserData();
                     _coroutineService.StartCoroutine(_reinitializeUserDataCoroutine);
                  }
                  else
                  {
                     _batchFilesForUploadCoroutine = BatchFilesForUpload();
                     _coroutineService.StartCoroutine(_batchFilesForUploadCoroutine);
                  }
               }
            ).Then(manifest =>
            {
               _platform.Notification.Subscribe(String.Format("{0}.datareplaced", "cloudsaving"), OnReplacedNtf);
               Subscribe(cb =>
               {
                  //Start listening for events.
               });
            }).Error(_ =>
               {
                  _retryCounter++;
                  if (_retryCounter >= 10)
                  {
                     Promise<Unit>.Failed(new Exception("Manifest not found, possibly no internet."));
                  }

                  Initialize();
               }
            ).RecoverWith(recover =>
               {
                  return Promise<ManifestResponse>.Failed(
                     new NoConnectivityException("No internet, cannot create or fetch manifest."));
               }
            );
         }).RecoverWith(error => { return Initialize(); }
         );
      }

      private void InvokeError(string reason, Exception inner)
      {
         OnError?.Invoke(new CloudSavingError(reason, inner));
      }

      private Action<Exception> ProvideErrorCallback(string methodName)
      {
         return (ex) => { InvokeError($"{methodName} Failed: {ex?.Message ?? "unknown reason"}", ex); };
      }

      public Promise<ManifestResponse> ListUserCloudData()
      {
         ListObjectsRequest listObjectsRequest = new ListObjectsRequest(_platform.User.id);

         return ListObjectsFromS3(listObjectsRequest).RecoverWith(error =>
         {
            if (error is PlatformRequesterException notFound && notFound.Status == 404)
            {
               return InitializeCloudData().FlatMap(_ =>
                  ListObjectsFromS3(listObjectsRequest)
               );
            }
            else
            {
               return Promise<ManifestResponse>.Failed(
                  new NoConnectivityException("No internet, cannot create or fetch manifest."));
            }
         });
      }

      public Promise<ManifestResponse> ForceUploadUserData()
      {
         return UploadUserData(_pendingUploads);
      }

      public IEnumerator ReinitializeUserData()
      {
         _allowEvents = false;
         yield return new WaitForSecondsRealtime(3);
         yield return DeleteLocalUserData().Then(userDeleted =>
            DownloadUserData(force: true).Then(_ => { _watcher.EnableRaisingEvents = true; }
            ).Error(_ =>
               _watcher.EnableRaisingEvents = true
            )
         ).Then(allowevent => _allowEvents = true);
      }

      private void StopRoutines()
      {
         _coroutineService.StopCoroutine(_batchFilesForUploadCoroutine);
         _coroutineService.StopCoroutine(WatchFiles());
      }

      private Promise<Unit> SetLocalManifest()
      {
         try
         {
            _manifestPath = Path.Combine(localCloudDataPath.root, "beamable", "tmp", localCloudDataPath.prefix,
               "cloudDataManifest.json");
         }
         catch (Exception e)
         {
            Debug.Log(e);
            return Promise<Unit>.Failed(e);
         }

         if (File.Exists(_manifestPath))
         {
            _localManifest = _localManifest == null
               ? JsonUtility.FromJson<ManifestResponse>(File.ReadAllText(_manifestPath))
               : _localManifest;
         }

         return Promise<Unit>.Successful(PromiseBase.Unit);
      }

      private Promise<ManifestResponse> UploadUserData(ConcurrentDictionary<string, string> pendingUploads)
      {
         var (upload, uploadMap) = GenerateUploadObjectRequestWithMapping(pendingUploads);
         //TODO: We may want to handle this better, so we don't create empty manifests
         if (upload.request.Count <= 0)
         {
            return Promise<ManifestResponse>.Failed(new Exception("Upload is empty"));
         }

         return HandleRequest(upload,
               uploadMap,
               Method.PUT,
               "/data/uploadURL"
            ).FlatMap(_ => CommitManifest(upload))
            .RecoverWith(_ =>
               UploadUserData(pendingUploads)
            ).Error(ProvideErrorCallback(nameof(UploadUserData)));
      }

      private (UploadObjectsRequest, List<KeyValuePair<string, string>>) GenerateUploadObjectRequestWithMapping(
         ConcurrentDictionary<string, string> pendingUploads)
      {
         var uploadRequest = new List<UploadObjectRequest>();
         var uploadMap = new List<KeyValuePair<string, string>>();
         var allFilesAndDirectories = new List<string>();

         if (pendingUploads != null && pendingUploads.Count > 0)
         {
            foreach (var item in pendingUploads)
            {
               allFilesAndDirectories.Add(Path.Combine(LocalCloudDataFullPath, item.Key));
            }
         }

         foreach (var fullPathToFile in allFilesAndDirectories)
         {
            var objectKey = StripPath(fullPathToFile, LocalCloudDataFullPath);
            uploadMap.Add(new KeyValuePair<string, string>(objectKey, fullPathToFile));
            var contentInfo = new FileInfo(fullPathToFile);
            var contentLength = contentInfo.Length;
            var lastModified = long.Parse(contentInfo.LastWriteTime.ToString("yyyyMMddHHmmss"));

            var uploadObjectRequest = new UploadObjectRequest(objectKey,
               (int) contentLength,
               GenerateChecksum(fullPathToFile),
               null,
               _platform.User.id,
               lastModified);

            _previouslyDownloaded[objectKey] = GenerateChecksum(fullPathToFile);

            uploadRequest.Add(uploadObjectRequest);
         }

         return (new UploadObjectsRequest(uploadRequest), uploadMap);
      }

      private Promise<Unit> WriteManifestToDisk(ManifestResponse response)
      {
         try
         {
            Directory.CreateDirectory(Path.GetDirectoryName(_manifestPath));
            _localManifest = response;
            File.WriteAllText(_manifestPath, JsonUtility.ToJson(_localManifest, true));
            return Promise<Unit>.Successful(PromiseBase.Unit);
         }
         catch (Exception ex)
         {
            return Promise<Unit>.Failed(ex).Error(ProvideErrorCallback(nameof(WriteManifestToDisk)));
         }
      }

      private Promise<Unit> DownloadUserData(ManifestResponse request = null, bool force = false)
      {
         if (request == null)
         {
            _watcher.EnableRaisingEvents = false;
            return ListUserCloudData().FlatMap(r => DownloadUserData(r, force));
         }

         var _downloadRequest = GenerateDownloadRequest(request, force);
         return HandleRequest(new GetObjectsRequest(_downloadRequest),
               _downloadMap,
               Method.GET,
               "/data/downloadURL"
            )
            .Error(ProvideErrorCallback(nameof(DownloadUserData)))
            .Map(_ => PromiseBase.Unit).Then(__ =>
            {
               if (request.replacement)
               {
                  var _upload = new UploadObjectsRequest(new List<UploadObjectRequest>());
                  foreach (var r in request.manifest)
                  {
                     _upload.request.Add(new UploadObjectRequest(r.key,
                        (int) r.size,
                        r.eTag,
                        null,
                        _platform.User.id,
                        r.lastModified)
                     );
                  }

                  CommitManifest(_upload).Then(_ =>
                     updateReceived?.Invoke(request)
                  );
               }
               else
               {
                  if (_downloadRequest.Count > 0)
                  {
                     WriteManifestToDisk(request).Then(_ =>
                        updateReceived?.Invoke(request)
                     );
                  }
               }
            });
      }

      private List<GetObjectRequest> GenerateDownloadRequest(ManifestResponse request = null, bool force = false)
      {
         List<GetObjectRequest> _downloadRequest = new List<GetObjectRequest>();
         string _objectFullName;
         if (force)
         {
            foreach (var resposneObj in request.manifest)
            {
               _previouslyDownloaded[resposneObj.key] = resposneObj.eTag;
            }
         }
         else
         {
            _previouslyDownloaded = DiffManifest(request);
         }

         foreach (var _s3Object in _previouslyDownloaded)
         {
            _downloadRequest.Add(new GetObjectRequest(_s3Object.Value));
            _objectFullName = Path.Combine(LocalCloudDataFullPath, _s3Object.Key);
            _downloadMap.Add(new KeyValuePair<string, string>(_s3Object.Value, _objectFullName));
         }

         return _downloadRequest;
      }

      private Promise<ManifestResponse> InitializeCloudData()
      {
         _pendingUploads.Clear();
         Directory.CreateDirectory(LocalCloudDataFullPath);
         foreach (var filename in Directory.EnumerateFiles(LocalCloudDataFullPath, "*.*", SearchOption.AllDirectories))
         {
            SetPendingUploads(filename);
         }

         if (_pendingUploads.Count > 0 && _localManifest != null)
         {
            return UploadUserData(_pendingUploads);
         }
         else
         {
            var upload = new UploadObjectsRequest(new List<UploadObjectRequest>());
            return CommitManifest(upload);
         }
      }

      private ConcurrentDictionary<string, string> DiffManifest(ManifestResponse response)
      {
         ConcurrentDictionary<string, string> _newManifestDict = new ConcurrentDictionary<string, string>();

         if (_localManifest != null)
         {
            foreach (var _s3Object in _localManifest.manifest)
            {
               _previouslyDownloaded[_s3Object.key] = _s3Object.eTag;
            }

            foreach (var _s3Object in response.manifest)
            {
               if (!_previouslyDownloaded.ContainsKey(_s3Object.key) ||
                   !_previouslyDownloaded[_s3Object.key].Equals(_s3Object.eTag))
               {
                  _newManifestDict[_s3Object.key] = _s3Object.eTag;
               }
            }
         }
         else
         {
            foreach (var _s3Object in response.manifest)
            {
               _newManifestDict[_s3Object.key] = _s3Object.eTag;
            }
         }

         WriteManifestToDisk(response)
            .Map(_ => PromiseBase.Unit);

         return _newManifestDict;
      }

      private Promise<List<Unit>> HandleRequest<T>(T request, List<KeyValuePair<string, string>> map, Method method,
         string endpoint)
      {
         var _promiseList = new HashSet<Promise<Unit>>();
         var _failedPromiseList = new HashSet<Promise<Unit>>();
         return GetPresignedURL(request, endpoint).FlatMap(_presignedURLS =>
         {
            foreach (var response in _presignedURLS.response)
            {
               foreach (var filepath in map)
               {
                  if (filepath.Key == response.objectKey)
                  {
                     try
                     {
                        _promiseList.Add(GetObjectFromS3(filepath.Value, response, method, response.objectKey));
                     }
                     catch
                     {
                        _failedPromiseList.Add(GetObjectFromS3(filepath.Value, response, method, response.objectKey));
                     }
                  }
               }
            }

            return Promise.Sequence(_promiseList.ToList())
               .Then(_ =>
               {
                  if (_failedPromiseList.Count > 0)
                     Promise.Sequence(_failedPromiseList.ToList());
               });
         });
      }

      private Promise<Unit> GetObjectFromS3(string fullPathToFile, PreSignedURL url, Method method, string _key)
      {
         return MakeRequestToS3(
            BuildS3Request(method, url.url, fullPathToFile)
            , _key).Map(_ => PromiseBase.Unit);
      }

      private Promise<EmptyResponse> MakeRequestToS3(UnityWebRequest request, string _key)
      {
         var result = new Promise<EmptyResponse>();
         var op = request.SendWebRequest();
         op.completed += _ => HandleResponse(result, request, _key);
         return result;
      }

      private void HandleResponse(Promise<EmptyResponse> promise, UnityWebRequest request, string _key)
      {
         request.Dispose();
         promise.CompleteSuccess(new EmptyResponse());
      }

      private UnityWebRequest BuildS3Request(Method method, string uri, string fileName)
      {
         UnityWebRequest request;

         if (fileName != null && method == Method.GET)
         {
            request = new UnityWebRequest(uri)
            {
               downloadHandler = new DownloadHandlerFile(fileName),

               method = method.ToString()
            };
         }
         else
         {
            var upload = new UploadHandlerRaw(File.ReadAllBytes(fileName))
            {
               contentType = "application/octet-stream"
            };
            request = new UnityWebRequest(uri)
            {
               uploadHandler = upload,
               method = method.ToString()
            };
         }

         return request;
      }

      private Promise<ManifestResponse> ListObjectsFromS3(ListObjectsRequest request)
      {
         return _requester.Request<ManifestResponse>(
               Method.GET,
               string.Format($"/basic/cloudsaving?playerId={request.playerId}"),
               null)
            .Error(ProvideErrorCallback(nameof(ListObjectsFromS3)));
      }

      private Promise<URLResponse> GetPresignedURL<T>(T request, string endpoint)
      {
         return _requester.Request<URLResponse>(
               Method.POST,
               string.Format($"/basic/cloudsaving{endpoint}"),
               request)
            .Error(ProvideErrorCallback(nameof(GetPresignedURL)));
      }

      private Promise<ManifestResponse> CommitManifest<UploadObjectsRequest>(UploadObjectsRequest request)
      {
         return _requester.Request<ManifestResponse>(
            Method.PUT,
            string.Format($"/basic/cloudsaving/data/commitManifest"),
            request).Then(res =>
         {
            res.replacement = false;
            WriteManifestToDisk(res)
               .Map(_ => PromiseBase.Unit);
         }).Error(ProvideErrorCallback(nameof(CommitManifest)));
      }

      private string StripPath(string key, string path)
      {
         return key.Remove(0, path.Length + 1).Replace(@"\", "/");
      }

      private IEnumerator BatchFilesForUpload()
      {
         yield return DownloadUserData().Map(_ =>
            InitializeCloudData().Then(_manifest =>
            {
               _coroutineService.StartCoroutine(WatchFiles());
            })
         );

         while (true)
         {
            yield return _delay;
            if (_connectivityService.HasConnectivity)
            {
               if (_pendingUploads.Count > 0)
               {
                  ConcurrentDictionary<string, string> _upload =
                     new ConcurrentDictionary<string, string>(_pendingUploads);
                  _pendingUploads.Clear();
                  yield return UploadUserData(_upload);
               }
            }
         }
      }

      private IEnumerator WatchFiles()
      {
         yield return _delay;
         try
         {
            _watcher.Path = LocalCloudDataFullPath;
            _watcher.IncludeSubdirectories = true;
            _watcher.NotifyFilter = NotifyFilters.LastWrite
                                   | NotifyFilters.FileName
                                   | NotifyFilters.DirectoryName;

            _watcher.Changed += OnLocalCloudDataChanged;
            _watcher.Created += OnLocalCloudDataChanged;
            _watcher.Renamed += OnLocalCloudDataRenamed;
            _watcher.EnableRaisingEvents = true;
         }
         catch (Exception e)
         {
            Debug.Log(e);
            Initialize();
         }
      }

      private void OnLocalCloudDataChanged(object sender, FileSystemEventArgs e)
      {
         SetPendingUploads(e.FullPath);
      }

      private void OnLocalCloudDataRenamed(object sender, RenamedEventArgs e)
      {
         SetPendingUploads(e.FullPath);
      }

      private void SetPendingUploads(string filePath)
      {
         var checksumNotEqual = _previouslyDownloaded.ContainsKey(filePath) &&
                                 !_previouslyDownloaded[filePath].Equals(GenerateChecksum(filePath));
         var missingKey = !_previouslyDownloaded.ContainsKey(filePath);
         var fileLengthNotZero = new FileInfo(filePath).Length > 0;
         if (checksumNotEqual || missingKey || fileLengthNotZero)
         {
            _pendingUploads[filePath] = GenerateChecksum(filePath);
         }
      }

      private Promise<Unit> DeleteLocalUserData()
      {
         _watcher.EnableRaisingEvents = false;
         _pendingUploads.Clear();
         _localManifest = null;
         _previouslyDownloaded.Clear();

         if (File.Exists(_manifestPath))
         {
            File.Delete(_manifestPath);
         }

         if (Directory.Exists(LocalCloudDataFullPath))
         {
            Directory.Delete(LocalCloudDataFullPath, true);
         }

         Directory.CreateDirectory(LocalCloudDataFullPath);
         return Promise<Unit>.Successful(PromiseBase.Unit);
      }

      private IEnumerator DownloadFiles(ManifestResponse data)
      {
         yield return new WaitForSecondsRealtime(3);
         yield return DownloadUserData(data).Then(_ =>
         {
            Notify(data);
            _downloadMap.Clear();
            _allowEvents = true;
         });
      }

      protected override string CreateRefreshUrl(string scope = null)
      {
         return "/basic/cloudsaving";
      }

      protected string GenerateChecksum(string content)
      {
         try
         {
            using (var md5 = MD5.Create())
            {
               using (var stream = File.OpenRead(content))
               {
                  return BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", string.Empty);
               }
            }
         }
         catch
         {
            //Waiting for release.
            return GenerateChecksum(content);
         }
      }

      protected void OnReplacedNtf(object data)
      {
         _coroutineService.StartCoroutine(ReinitializeUserData());
      }

      protected override void OnRefresh(ManifestResponse data)
      {
         if (_allowEvents && !data.replacement)
         {
            if (JsonUtility.ToJson(_localManifest) != JsonUtility.ToJson(data))
            {
               _allowEvents = false;
               _coroutineService.StartCoroutine(DownloadFiles(data));
            }
         }
      }
   }


   [Serializable]
   public class S3Object
   {
      public string bucketName;
      public string key;
      public long size;
      public DateTime lastModified;
      public Owner owner;
      public string storageClass;
      public string eTag;
   }

   [Serializable]
   public class Owner
   {
      public string displayName;
      public string id;
   }

   [Serializable]
   public class PreSignedURL
   {
      public string objectKey;
      public string url;
   }

   [Serializable]
   public class URLResponse
   {
      public List<PreSignedURL> response;

      public URLResponse(List<PreSignedURL> response)
      {
         this.response = response;
      }
   }

   [Serializable]
   public class UploadObjectsRequest
   {
      public List<UploadObjectRequest> request;

      public UploadObjectsRequest(List<UploadObjectRequest> request)
      {
         this.request = request;
      }
   }

   [Serializable]
   public class UploadObjectRequest
   {
      public string objectKey;
      public int sizeInBytes;
      public string checksum;
      public List<MetadataPair> metadata;
      public long playerId;
      public long lastModified;

      public UploadObjectRequest(string objectKey, int sizeInBytes, string checksum, List<MetadataPair> metadata,
         long playerId, long lastModified)
      {
         this.objectKey = objectKey;
         this.sizeInBytes = sizeInBytes;
         this.checksum = checksum;
         this.metadata = metadata;
         this.playerId = playerId;
         this.lastModified = lastModified;
      }
   }

   [Serializable]
   public class MetadataPair
   {
      public string key;
      public string value;

      public MetadataPair(string key, string value)
      {
         this.key = key;
         this.value = value;
      }
   }

   [Serializable]
   public class ListObjectsRequest
   {
      public long playerId;

      public ListObjectsRequest(long playerId)
      {
         this.playerId = playerId;
      }
   }

   [Serializable]
   public class GetObjectRequest
   {
      public string objectKey;

      public GetObjectRequest(string objectKey)
      {
         this.objectKey = objectKey;
      }
   }

   [Serializable]
   public class GetObjectsRequest
   {
      public List<GetObjectRequest> request;

      public GetObjectsRequest(List<GetObjectRequest> request)
      {
         this.request = request;
      }
   }

   [Serializable]
   public class ManifestResponse
   {
      public string id;
      public List<CloudSavingManifestEntry> manifest;
      public bool replacement = false;
   }

   [Serializable]
   public class CloudSavingManifestEntry
   {
      public string bucketName;
      public string key;
      public int size;
      public long lastModified;
      public string eTag;

      public CloudSavingManifestEntry(string bucketName, string key, int size, long lastModified, string eTag)
      {
         this.bucketName = bucketName;
         this.key = key;
         this.size = size;
         this.lastModified = lastModified;
         this.eTag = eTag;
      }
   }

   [Serializable]
   public class LocalCloudDataPath
   {
      public string root;
      public string prefix;
      public string fullPath;

      public LocalCloudDataPath(PlatformService platformService)
      {
         platformService.OnReady.Then(plat =>
         {
            root = Application.persistentDataPath;
            prefix = Path.Combine(platformService.Cid, platformService.Pid, platformService.User.id.ToString());
            fullPath = Path.Combine(root, prefix);
         });
      }
   }
}