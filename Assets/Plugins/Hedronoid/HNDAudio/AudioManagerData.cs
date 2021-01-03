using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using UnityEngine.Audio;
using System.Collections.Generic;
using Hedronoid;
using Hedronoid.Events;
using System.Linq;


namespace Hedronoid.Audio
{
    /// <summary>
    /// Prefab with all audio data expect actual audio clips.
    /// This contains all definition of hooks, categories, links to mixer groups.
    /// </summary>
    [Serializable]
    public class AudioManagerData : HNDGameObject
    {
        [SerializeField]
        private AudioManagerCategory m_TreeData;

        // This is not a tree :)
        [SerializeField]
        public List<AudioManagerCategory> m_SavedTree;
        public List<AudioManagerCategory> SavedTree
        {
            get { return m_SavedTree; }
            set { m_SavedTree = value; }
        }

        public AudioManagerCategory TreeData
        {
            get { return m_TreeData; }
            set { m_TreeData = value; }
        }

#if UNITY_EDITOR
        public static AudioManagerData LoadInstanceData()
        {
            HNDAudioPreferences p = HNDAudioPreferences.Instance;

#if UNITY_2018_3_OR_NEWER
            var go = PrefabUtility.LoadPrefabContents(p.AudioManagerDataPath);
            AudioManagerData data = go.GetComponent<AudioManagerData>();
#else
            AudioManagerData data = AssetDatabase.LoadAssetAtPath<AudioManagerData>(p.AudioManagerDataPath);
#endif

            if (data == null) return null;

            data.ReconstructTreeChildren();
            return data;
        }
#endif

        public void InitTree()
        {
            int uniqueId = 1;
            m_TreeData = new AudioManagerCategory("Master", uniqueId++);
            m_TreeData.Add(new AudioManagerCategory("Music", uniqueId++));
            m_TreeData.Add(new AudioManagerCategory("SFX", uniqueId++));
            m_TreeData.Add(new AudioManagerCategory("Characters", uniqueId++));
            m_TreeData.Add(new AudioManagerCategory("VO", uniqueId++));
            m_SavedTree = new List<AudioManagerCategory>();
        }

        public void ReconstructTreeChildren()
        {
            Dictionary<int, AudioManagerCategory> lookup = new Dictionary<int, AudioManagerCategory>();
            AudioManagerCategory root = null;

            //Build look up dictionary, clear non-serialized data
            for (int i = 0; i < m_SavedTree.Count; i++)
            {
                var category = m_SavedTree[i];
                lookup[category.UniqueID] = category;
                category.ReconstructClear();

                if (category.ParentUniqueId < 0)
                {
                    root = category;
                }
            }

            //Use parentUniqueId to reconstruct parent relations
            for (int i = 0; i < m_SavedTree.Count; i++)
            {
                var category = m_SavedTree[i];
                if (category.ParentUniqueId >= 0)
                {

                    if (!lookup.ContainsKey(category.ParentUniqueId))
                    {
                        D.AudioError("No parent found for category: " + category.UniqueID + " - " + category.ID);
                    }

                    var parentCategory = lookup[category.ParentUniqueId];
                    category.Parent = parentCategory;
                    parentCategory.ReconstructAdd(category);
                }
            }

            //Use orderIdx to reconstruct children order
            for (int i = 0; i < m_SavedTree.Count; i++)
            {
                var category = m_SavedTree[i];
                if (category.Children != null && category.Children.Count > 0)
                {
                    var newChildren = new Dictionary<string, AudioManagerCategory>();

                    var sortedChildrenList = category.Children.Values.OrderBy(c => c.OrderIdx).ToList();
                    for (int j = 0; j < sortedChildrenList.Count; j++)
                    {
                        var childCategory = sortedChildrenList[j];
                        newChildren.Add(childCategory.ID, childCategory);
                    }

                    category.Children = newChildren;
                }
            }

            if (m_SavedTree.Count > 0)
            {
                m_TreeData = root;
            }

            D.AudioLog("Audio data loaded!");
        }

#if UNITY_EDITOR
        public void SaveTree()
        {
            if (m_TreeData == null) InitTree();

#if UNITY_2018_3_OR_NEWER
            var go = PrefabUtility.LoadPrefabContents(HNDAudioPreferences.Instance.AudioManagerDataPath);
            AudioManagerData data = go.GetComponent<AudioManagerData>();

            data.TreeData = m_TreeData;
            data.SavedTree = new List<AudioManagerCategory>();
            data.SaveTree(m_TreeData);

            PrefabUtility.SaveAsPrefabAsset(go, HNDAudioPreferences.Instance.AudioManagerDataPath);
#else
            m_SavedTree = new List<AudioManagerCategory>();
            SaveTree(m_TreeData);
#endif
        }
#endif

        public void SaveTree(AudioManagerCategory node)
        {
            if (node.Parent != null)
            {
                node.ParentUniqueId = node.Parent.UniqueID;

                int orderIdx = -1;
                var siblingList = node.Parent.Children.Values.ToList();
                for (int i = 0; i < siblingList.Count; i++)
                {
                    if (siblingList[i] == node)
                    {
                        orderIdx = i;
                        break;
                    }
                }
                node.OrderIdx = orderIdx;
            }
            else
            {
                node.ParentUniqueId = -1;
                node.OrderIdx = -1;
            }

            m_SavedTree.Add(node);
            //node.NextAllowedAudioDelay = -1;
            if (node.Children != null && node.Children.Count > 0)
                foreach (KeyValuePair<string, AudioManagerCategory> entry in node.Children)
                {
                    SaveTree(entry.Value);
                }
        }

        public int CreateNewUniqueId()
        {
            int randomId = UnityEngine.Random.Range(1, Int32.MaxValue);
            if (m_TreeData != null)
            {
                while (m_TreeData.FindCategoryWithUniqueId(randomId) != null)
                {
                    randomId = UnityEngine.Random.Range(1, Int32.MaxValue);
                }
            }
            return randomId;
        }

        public AudioManagerCategory CreateAudioManagerCategory(string name)
        {
            AudioManagerCategory c = new AudioManagerCategory(name, CreateNewUniqueId());
            return c;
        }
    }

}