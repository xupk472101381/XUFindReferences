using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace XUFindRef
{
    [InitializeOnLoad]
    public class XUFindReferencesCache
    {
        private static XUFindReferencesCache mInstance;
        public static XUFindReferencesCache GetInstance()
        {
            return mInstance;
        }

        private string CACHE_PATH = Application.dataPath.Replace("Assets", "Library/XUFindReferencesCache");
        private string CACHE_KEY_FORWARD = "forward";
        private string CACHE_KEY_REVERSE = "reverse";

        private Dictionary<string, List<string>> referencesForward = new Dictionary<string, List<string>>();//引用的GUID
        private Dictionary<string, List<string>> referencesReverse = new Dictionary<string, List<string>>();//被引用的GUID

        static XUFindReferencesCache()
        {
            //if (mInstance != null)
            //    return;

            mInstance = new XUFindReferencesCache();
            if (File.Exists(mInstance.CACHE_PATH))
            {
                mInstance.ReadCache();
            }
            else
            {
                mInstance.CreateCache();
            }
        }

        private void ReadCache()
        {
            Dictionary<string, object> cacheJson = Json.Deserialize(File.ReadAllText(CACHE_PATH)) as Dictionary<string, object>;
            {
                referencesForward = new Dictionary<string, List<string>>();
                Dictionary<string, object> forward = cacheJson[CACHE_KEY_FORWARD] as Dictionary<string, object>;
                foreach (var refFor in forward)
                {
                    referencesForward.Add(refFor.Key, new List<string>());
                    List<object> dependencies = refFor.Value as List<object>;
                    foreach (var dependencie in dependencies)
                    {
                        if (referencesForward[refFor.Key].Contains(dependencie.ToString()) == false)
                        {
                            referencesForward[refFor.Key].Add(dependencie.ToString());
                        }
                    }
                }
            }
            {
                referencesReverse = new Dictionary<string, List<string>>();
                Dictionary<string, object> reverse = cacheJson[CACHE_KEY_REVERSE] as Dictionary<string, object>;
                foreach (var refRev in reverse)
                {
                    referencesReverse.Add(refRev.Key, new List<string>());
                    List<object> dependencies = refRev.Value as List<object>;
                    foreach (var dependencie in dependencies)
                    {
                        if (referencesReverse[refRev.Key].Contains(dependencie.ToString()) == false)
                        {
                            referencesReverse[refRev.Key].Add(dependencie.ToString());
                        }
                    }
                }
            }
        }

        private void CreateCache()
        {
            List<string> allFiles = new List<string>(Directory.GetFiles(Application.dataPath, "*.*", SearchOption.AllDirectories));
            for (int i = 0; i < allFiles.Count; i++)
            {
                string file = allFiles[i];
                string assetsPath = "Assets" + Path.GetFullPath(file).Replace(Path.GetFullPath(Application.dataPath), "").Replace('\\', '/');
                string guid = AssetDatabase.AssetPathToGUID(assetsPath);

                //正向存储
                if (referencesForward.ContainsKey(guid) == false)
                {
                    referencesForward.Add(guid, new List<string>());
                    string[] dependencies = AssetDatabase.GetDependencies(assetsPath);
                    if (dependencies != null && dependencies.Length > 0)
                    {
                        foreach (var dependencie in dependencies)
                        {
                            string dependencieGUID = AssetDatabase.AssetPathToGUID(dependencie);
                            if (referencesForward[guid].Contains(dependencieGUID) == false && dependencieGUID != guid)
                            {
                                referencesForward[guid].Add(dependencieGUID);
                            }
                        }
                    }
                }

                //逆向存储
                if (referencesReverse.ContainsKey(guid) == false)
                {
                    referencesReverse.Add(guid, new List<string>());
                }
            }

            //解析逆向引用
            foreach (var refForward in referencesForward)
            {
                foreach (var dependencieGUID in refForward.Value)
                {
                    if (referencesReverse.ContainsKey(dependencieGUID))
                    {
                        if (referencesReverse[dependencieGUID].Contains(refForward.Key) == false && refForward.Key != dependencieGUID)
                        {
                            referencesReverse[dependencieGUID].Add(refForward.Key);
                        }
                    }
                }
            }

            SaveCache();
        }

        public void SaveCache()
        {
            Dictionary<string, Dictionary<string, List<string>>> jsonObj = new Dictionary<string, Dictionary<string, List<string>>>();
            jsonObj.Add(CACHE_KEY_FORWARD, referencesForward);
            jsonObj.Add(CACHE_KEY_REVERSE, referencesReverse);
            string jsonStr = Json.Serialize(jsonObj);
            File.WriteAllText(CACHE_PATH, jsonStr);
        }

        public void CacheOne(string assetsPath)
        {
            string guid = AssetDatabase.AssetPathToGUID(assetsPath);
            if (string.IsNullOrEmpty(guid) == false)
            {
                //正向存储
                if (referencesForward.ContainsKey(guid) == false)
                {
                    referencesForward.Add(guid, new List<string>());
                }
                else
                {
                    referencesForward[guid].Clear();
                }
                string[] dependencies = AssetDatabase.GetDependencies(assetsPath);
                if (dependencies != null && dependencies.Length > 0)
                {
                    foreach (var dependencie in dependencies)
                    {
                        string dependencieGUID = AssetDatabase.AssetPathToGUID(dependencie);
                        if (referencesForward[guid].Contains(dependencieGUID) == false && dependencieGUID != guid)
                        {
                            referencesForward[guid].Add(dependencieGUID);
                        }
                    }
                }

                //逆向存储
                if (referencesReverse.ContainsKey(guid) == false)
                {
                    referencesReverse.Add(guid, new List<string>());
                }
                else
                {
                    referencesReverse[guid].Clear();
                }
                foreach (var refForward in referencesForward)
                {
                    foreach (var dependencieGUID in refForward.Value)
                    {
                        if (dependencieGUID == guid)
                        {
                            if (referencesReverse[dependencieGUID].Contains(refForward.Key) == false && refForward.Key != dependencieGUID)
                            {
                                referencesReverse[dependencieGUID].Add(refForward.Key);
                            }
                        }
                    }
                }
            }
        }

        public void RemoveOne(string assetsPath)
        {
            string guid = AssetDatabase.AssetPathToGUID(assetsPath);

            if (referencesForward.ContainsKey(guid))
            {
                referencesForward.Remove(guid);
            }
            foreach (var refForward in referencesForward)
            {
                refForward.Value.Remove(guid);
            }

            if (referencesReverse.ContainsKey(guid))
            {
                referencesReverse.Remove(guid);
            }
            foreach (var refReverse in referencesReverse)
            {
                refReverse.Value.Remove(guid);
            }
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// 刷新缓存数据
        /// </summary>
        public void RefreshCache()
        {
            referencesForward = new Dictionary<string, List<string>>();
            referencesReverse = new Dictionary<string, List<string>>();
            CreateCache();
        }

        /// <summary>
        /// 获取引用的资源GUID列表
        /// </summary>
        /// <param name="guid">实例ID</param>
        public List<string> GetDependenciesByGUID(string guid)
        {
            List<string> dependencies = new List<string>();
            if (referencesForward.ContainsKey(guid))
            {
                dependencies.AddRange(referencesForward[guid]);
            }
            return dependencies;
        }

        /// <summary>
        /// 获取引用的资源GUID列表
        /// </summary>
        /// <param name="path">相对路径</param>
        public List<string> GetDependenciesByAssetsPath(string path)
        {
            string guid = AssetDatabase.AssetPathToGUID(path);
            return GetDependenciesByGUID(guid);
        }

        /// <summary>
        /// 获取被引用的资源GUID列表
        /// </summary>
        /// <param name="guid">实例ID</param>
        public List<string> GetReferencesByGUID(string guid)
        {
            List<string> references = new List<string>();
            if (referencesReverse.ContainsKey(guid))
            {
                references.AddRange(referencesReverse[guid]);
            }
            return references;
        }

        /// <summary>
        /// 获取被引用的资源GUID列表
        /// </summary>
        /// <param name="path">相对路径</param>
        public List<string> GetReferencesByAssetsPath(string path)
        {
            string guid = AssetDatabase.AssetPathToGUID(path);
            return GetReferencesByGUID(guid);
        }
    }

    public class XUFindReferencesPostprocessor : AssetPostprocessor
    {
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            foreach (string str in importedAssets)
            {
                XUFindReferencesCache.GetInstance().CacheOne(str);
            }
            foreach (string str in deletedAssets)
            {
                XUFindReferencesCache.GetInstance().RemoveOne(str);
            }
            foreach (string str in movedAssets)
            {
                XUFindReferencesCache.GetInstance().CacheOne(str);
            }
            foreach (string str in movedFromAssetPaths)
            {
                XUFindReferencesCache.GetInstance().CacheOne(str);
            }
            XUFindReferencesCache.GetInstance().SaveCache();
        }
    }
}