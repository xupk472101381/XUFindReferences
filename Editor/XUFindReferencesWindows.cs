using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace XUFindRef
{
    public class XUFindReferencesWindows : EditorWindow
    {
        private bool foldoutDep = true;
        private bool foldoutRef = true;

        void OnGUI()
        {
            if (GUILayout.Button("刷新缓存数据"))
            {
                XUFindReferencesCache.GetInstance().RefreshCache();
            }

            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (!string.IsNullOrEmpty(path))
            {
                {
                    foldoutDep = EditorGUILayout.Foldout(foldoutDep, "我引用的资源列表");
                    if (foldoutDep)
                    {
                        List<string> cacheDependencies = XUFindReferencesCache.GetInstance().GetDependenciesByAssetsPath(path);
                        foreach (var dependencie in cacheDependencies)
                        {
                            string assetsPath = AssetDatabase.GUIDToAssetPath(dependencie);
                            EditorGUILayout.ObjectField(AssetDatabase.LoadAssetAtPath(assetsPath, typeof(Object)), typeof(Object));
                        }
                    }
                }
                {
                    foldoutRef = EditorGUILayout.Foldout(foldoutRef, "引用我的资源列表");
                    if (foldoutRef)
                    {
                        List<string> cacheReferences = XUFindReferencesCache.GetInstance().GetReferencesByAssetsPath(path);
                        foreach (var reference in cacheReferences)
                        {
                            string assetsPath = AssetDatabase.GUIDToAssetPath(reference);
                            EditorGUILayout.ObjectField(AssetDatabase.LoadAssetAtPath(assetsPath, typeof(Object)), typeof(Object));
                        }
                    }
                }
            }
        }
    }
}