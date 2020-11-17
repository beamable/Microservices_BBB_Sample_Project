﻿using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

namespace Core.Platform.SDK.Caches
{
    public class OfflineCache
    {
        private OfflineCache()
        {
        }

        private const string _offlineCacheRoot = "beamable";
        private const string _offlineCacheDir = "cache";
        private const string _offlineCacheExtension = ".json";

        private static OfflineCache _instance = new OfflineCache();
        private Dictionary<string, object> _offlineCacheData = new Dictionary<string, object>();

        private readonly static string _cid = Config.ConfigDatabase.GetString("cid");
        private readonly static string _pid = Config.ConfigDatabase.GetString("pid");
        private readonly static string _offlineCacheRootDir = Path.Combine(Application.persistentDataPath, _offlineCacheRoot, _offlineCacheDir, _cid, _pid, Application.version);
        private readonly MD5 _md5 = MD5.Create();

        public static Promise<T> Get<T>(string key, AccessToken token)
        {
            return _instance.Read<T>(_instance.GetHash(key + token.RefreshToken));
        }

        public static void Set<T>(string key, object data, AccessToken token)
        {
            _instance.Update(_instance.GetHash(key + token.RefreshToken), data);
        }

        public static void Merge<TKey, TValue>(string key, AccessToken token, Dictionary<long, Dictionary<TKey, TValue>> data)
        {
            _instance.Merge(key + token.RefreshToken, data);
        }

        public static Promise<Dictionary<long, TDict>> RecoverDictionary<TDict>(Exception ex, string key,
            AccessToken token,
            List<long> gamerTags)
        {
            return _instance.HandleDictionaryCase<TDict>(ex, key + token.RefreshToken, gamerTags);
        }

        public static void FlushInvalidCache()
        {
            _instance.DeleteCache();
        }

        private void DeleteCache()
        {
            if (Directory.Exists(Directory.GetParent(_offlineCacheRootDir).ToString()))
            {
                string[] dirs = Directory.GetDirectories(Directory.GetParent(_offlineCacheRootDir).ToString());
                foreach (string dir in dirs)
                {
                    if (Path.GetFileName(dir) != Application.version)
                    {
                        Debug.Log("Clearing cache for :" + dir);
                        Directory.Delete(dir, true);
                    }
                }
            }
        }

        private Promise<Dictionary<long, TDict>> HandleDictionaryCase<TDict>(Exception ex, string key, List<long> gamerTags)
        {
            if (ex is NoConnectivityException)
            {
                return Read<Dictionary<long, TDict>>(key).Map(data =>
                {
                    var output = new Dictionary<long, TDict>();
                    foreach (var gamerTag in gamerTags)
                    {
                        if (data.ContainsKey(gamerTag))
                        {
                            output.Add(gamerTag, data[gamerTag]);
                        }
                        else
                        {
                            Debug.LogError("No cached data for " + gamerTag);
                            throw ex;
                        }
                    }
                    return output;
                });
            }
            else
            {
                throw ex;
            }
        }
        private void Merge<TKey, TValue>(string key, Dictionary<long, Dictionary<TKey, TValue>> nextData)
        {
            Read<OfflineUserCache>(key)
                .RecoverWith(err => Promise<OfflineUserCache>.Successful(new OfflineUserCache()))
                .Then(currentData =>
            {
                var result = new Dictionary<long, Dictionary<TKey, TValue>>();
                var currentDictionary = CacheToDict<TKey, TValue> (currentData); //Convert offlinecache data to dictionary

                //Take the Union of the data
                var dictionaries = new Dictionary<long, Dictionary<TKey, TValue>>[] { currentDictionary, nextData };
                foreach (Dictionary<long, Dictionary<TKey, TValue>> dict in dictionaries)
                {
                    result = result.Union(dict)
                        .GroupBy(g => g.Key)
                        .ToDictionary(pair => pair.Key, pair => pair.First().Value);
                }                
                
                //Write updated offlinecache to disk
                Update(key, DictToCache<TKey, TValue>(result));
            });
        }
        public static Dictionary<K, V> Merge<K, V>(IEnumerable<Dictionary<K, V>> dictionaries)
        {
            Dictionary<K, V> result = new Dictionary<K, V>();
            foreach (Dictionary<K, V> dict in dictionaries)
            {
                result = result.Union(dict)
                    .GroupBy(g => g.Key)
                    .ToDictionary(pair => pair.Key, pair => pair.First().Value);
            }
            return result;
        }

        private void Update<T>(string key, T data)
        {
            if (_offlineCacheData.ContainsKey(key))
            {
                if (!_offlineCacheData[key].Equals(data))
                {
                    _offlineCacheData[key] = data;
                    WriteCacheToDisk(key, data);
                }
            }
            else
            {
                _offlineCacheData.Add(key, data);
                WriteCacheToDisk(key, data);
            }
        }

        private Promise<T> Read<T>(string key)
        {
            Promise<T> _localCacheResponse = new Promise<T>();

            if (_offlineCacheData.TryGetValue(key, out var _cacheFromMemory))
            {
                _localCacheResponse.CompleteSuccess((T)_cacheFromMemory);
            }
            else
            {
                if (!File.Exists(GetFullPathForKey(key)))
                {
                    _localCacheResponse.CompleteError(new NoConnectivityException(key + " is not cached and requires internet connectivity."));
                    return _localCacheResponse;
                }
                _offlineCacheData.Add(key, ReadCacheFromDisk<T>(key));
                _localCacheResponse.CompleteSuccess((T)_offlineCacheData[key]);
            }

            return _localCacheResponse;
        }

        private string GetFullPathForKey(string key)
        {
            return Path.Combine(_offlineCacheRootDir, key) + _offlineCacheExtension;
        }

        private void WriteCacheToDisk(string key, object data)
        {
            Directory.CreateDirectory(_offlineCacheRootDir);
            File.WriteAllText(GetFullPathForKey(key), JsonUtility.ToJson(data));
        }
        private object ReadCacheFromDisk<T>(string key)
        {
            return JsonUtility.FromJson<T>(File.ReadAllText(GetFullPathForKey(key)));
        }

        protected string GetHash(string input)
        {
            byte[] data = _md5.ComputeHash(Encoding.UTF8.GetBytes(input));
            var sBuilder = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }
            return sBuilder.ToString();
        }

        /// <summary>
        /// Unpacks the offlineCache object to a generic Dictionary.
        /// </summary>
        /// <param name="offlineUsers">Offline cache object to be unpacked</param>
        /// <returns></returns>
        private Dictionary<long, Dictionary<TKey, TValue>> CacheToDict<TKey, TValue>(OfflineUserCache offlineUsers)
        {
            Dictionary<long, Dictionary<TKey, TValue>> output = new Dictionary<long, Dictionary<TKey, TValue>>();

            foreach (OfflineUser user in offlineUsers.cache)
            {
                Dictionary<TKey, TValue> userStats = new Dictionary<TKey, TValue>();
                for (int i = 0; i < user.offlineDataList.keys.Count; i++)
                {
                    userStats.Add(
                        (TKey)Convert.ChangeType(user.offlineDataList.keys[i], typeof(TKey)),
                        (TValue)Convert.ChangeType(user.offlineDataList.values[i], typeof(TValue)));
                }
                output.Add(user.dbid, userStats);
            }

            return output;
        }

        /// <summary>
        /// Packs dictionary for storage in cache as a OfflineUserCache object
        /// </summary>
        /// <param name="inputDict">dctionary to be packed</param>
        /// <returns>packed dictionary, as cache object</returns>
        private OfflineUserCache DictToCache<TKey, TValue>(Dictionary<long, Dictionary<TKey, TValue>> inputDict)
        {
            OfflineUserCache newOfflineUsers = new OfflineUserCache();
            int tempIndex = 0;
            foreach (KeyValuePair<long, Dictionary<TKey, TValue>> user in inputDict)
            {
                newOfflineUsers.cache.Add(new OfflineUser());
                newOfflineUsers.cache[tempIndex].dbid = user.Key; //store dbid
                foreach (KeyValuePair<TKey, TValue> stat in user.Value)
                {
                    newOfflineUsers.cache[tempIndex].offlineDataList.keys.Add(stat.Key.ToString());
                    newOfflineUsers.cache[tempIndex].offlineDataList.values.Add(stat.Value.ToString());
                }
                tempIndex++;
            }

            return newOfflineUsers;
        }

    }
    [Serializable]
    //OfflineUserCache
    public class OfflineUserCache
    {
        public List<OfflineUser> cache;

        public OfflineUserCache()
        {
            cache = new List<OfflineUser>();
        }
    }

    [Serializable]
    //OfflineUserList
    public class OfflineUser
    {
        public long dbid;
        public OfflineUserData offlineDataList;

        public OfflineUser()
        {
            dbid = 0;
            offlineDataList = new OfflineUserData();
        }
    }

    [Serializable]
    //OfflineUserData
    public class OfflineUserData
    {
        public List<string> keys;
        public List<string> values;

        public OfflineUserData()
        {
            keys = new List<string>();
            values = new List<string>();
        }
    }
}