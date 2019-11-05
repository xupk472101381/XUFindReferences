using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace XUFindRef
{
    public class XUFindReferencesWindows : EditorWindow
    {
        private bool foldoutDep = true;
        private bool foldoutRef = true;

        private Vector2 scrollPos = Vector2.zero;

        void OnInspectorUpdate()
        {
            //开启窗口的重绘，不然窗口信息不会刷新
            Repaint();
        }

        void OnGUI()
        {
            float progress = XUFindReferencesCache.GetInstance().progress;
            if (progress < 1f)
            {
                EditorGUI.ProgressBar(new Rect(10, 10, this.maxSize.x - 20, 20), progress, "缓存数据生成中:" + ((int)(progress * 100)).ToString("D2") + "%");
                return;
            }

            if (GUILayout.Button("刷新缓存数据"))
            {
                XUFindReferencesCache.GetInstance().RefreshCache();
            }

            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (!string.IsNullOrEmpty(path))
            {
                scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
                {
                    List<string> cacheDependencies = XUFindReferencesCache.GetInstance().GetDependenciesByAssetsPath(path);
                    foldoutDep = EditorGUILayout.Foldout(foldoutDep, "我引用的资源列表(" + cacheDependencies.Count + ")");
                    if (foldoutDep)
                    {
                        foreach (var dependencie in cacheDependencies)
                        {
                            string assetsPath = AssetDatabase.GUIDToAssetPath(dependencie);
                            EditorGUILayout.ObjectField(AssetDatabase.LoadAssetAtPath(assetsPath, typeof(Object)), typeof(Object));
                        }
                    }
                }
                {
                    List<string> cacheReferences = XUFindReferencesCache.GetInstance().GetReferencesByAssetsPath(path);
                    foldoutRef = EditorGUILayout.Foldout(foldoutRef, "引用我的资源列表(" + cacheReferences.Count + ")");
                    if (foldoutRef)
                    {
                        foreach (var reference in cacheReferences)
                        {
                            string assetsPath = AssetDatabase.GUIDToAssetPath(reference);
                            EditorGUILayout.ObjectField(AssetDatabase.LoadAssetAtPath(assetsPath, typeof(Object)), typeof(Object));
                        }
                    }
                }
                EditorGUILayout.EndScrollView();
            }
        }
    }
}