using UnityEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityObject = UnityEngine.Object;


namespace Hedronoid.Audio
{
    public class AudioManagerTreeView : TreeView
    {
        private AudioManagerData m_Data = null;
        private AudioManagerCategory m_Root = null;

        public Action<AudioManagerCategory[]> OnSelectedCategory;

        const string k_GenericDragID = "GenericDragColumnDragging";

        public AudioManagerTreeView(TreeViewState state, AudioManagerData data, AudioManagerCategory root) : base(state)
        {
            showAlternatingRowBackgrounds = true;

            m_Data = data;
            m_Root = root;
            Reload();
        }

        protected override TreeViewItem BuildRoot()
        {
            return CreateTreeViewItem(m_Root);
        }

        protected override IList<TreeViewItem> BuildRows(TreeViewItem root)
        {
            var rows = GetRows() ?? new List<TreeViewItem>(200);
            rows.Clear();

            if (hasSearch)
            {
                Search(rows);
            }
            else
            {
                // Add root
                var childItem = CreateTreeViewItem(m_Root);
                root.AddChild(childItem);
                rows.Add(childItem);

                // Add rest of the tree
                AddChildrenRecursive(m_Root, root, rows);
            }

            SetupDepthsFromParentsAndChildren(root);
            return rows;
        }

        void AddChildrenRecursive(AudioManagerCategory category, TreeViewItem item, IList<TreeViewItem> rows)
        {
            int childCount = category.Count;
            var childrenList = category.Children.Values.ToList();
            item.children = new List<TreeViewItem>(childCount);
            for (int i = 0; i < childCount; ++i)
            {
                var child = childrenList[i];

                var childItem = CreateTreeViewItem(child);
                item.AddChild(childItem);
                rows.Add(childItem);

                if (child.Count > 0)
                {
                    if (IsExpanded(childItem.id))
                    {
                        AddChildrenRecursive(child, childItem, rows);
                    }
                    else
                    {
                        childItem.children = CreateChildListForCollapsedParent();
                    }
                }
            }
        }

        void Search(IList<TreeViewItem> rows)
        {
            const int kItemDepth = 0; // tree is flattened when searching

            var stack = new Stack<AudioManagerCategory>();
            foreach (var element in m_Root.Children.Values.ToList())
                stack.Push(element);

            while (stack.Count > 0)
            {
                var current = stack.Pop();
                // Matches search?
                if (searchString == ":m")
                {
                    if (current.IsLeaf() && (current.AudioData.Count == 0 || current.AudioData.Contains(null) || current.AudioData.Contains(string.Empty)))
                    {
                        rows.Add(new TreeViewItem(current.UniqueID, kItemDepth, current.ID));
                    }
                }
                else if (current.ID.IndexOf(searchString, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    rows.Add(new TreeViewItem(current.UniqueID, kItemDepth, current.ID));
                }

                if (current.Children != null && current.Children.Count > 0)
                {
                    var children = current.Children.Values.ToList();
                    foreach (var child in children)
                    {
                        stack.Push(child);
                    }
                }
            }
        }

        static TreeViewItem CreateTreeViewItem(AudioManagerCategory category)
        {
            // We can use the GameObject instanceID for TreeViewItem id, as it ensured to be unique among other items in the tree.
            // To optimize reload time we could delay fetching the transform.name until it used for rendering (prevents allocating strings 
            // for items not rendered in large trees)
            // We just set depth to -1 here and then call SetupDepthsFromParentsAndChildren at the end of BuildRootAndRows to set the depths.
            return new TreeViewItem(category.UniqueID, -1, category.ID);
        }


        protected override IList<int> GetAncestors(int id)
        {
            var category = GetAudioCategory(id);


            List<int> ancestors = new List<int>();
            while (category.Parent != null)
            {
                ancestors.Add(category.Parent.UniqueID);
                category = category.Parent;
            }

            return ancestors;
        }

        protected override IList<int> GetDescendantsThatHaveChildren(int id)
        {
            var stack = new Stack<AudioManagerCategory>();

            var start = GetAudioCategory(id);
            stack.Push(start);

            var parents = new List<int>();
            while (stack.Count > 0)
            {
                var current = stack.Pop();
                parents.Add(current.UniqueID);
                for (int i = 0; i < current.Count; ++i)
                {
                    var childList = current.Children.Values.ToList();
                    if (current.Count > 0)
                        stack.Push(childList[i]);
                }
            }

            return parents;
        }

        AudioManagerCategory GetAudioCategory(int id)
        {
            return m_Root.FindCategoryWithUniqueId(id); //TODO Optimize?
        }

        // Custom GUI

        protected override void RowGUI(RowGUIArgs args)
        {
            Event evt = Event.current;
            extraSpaceBeforeIconAndLabel = 18f;

            // GameObject isStatic toggle 
            var category = GetAudioCategory(args.item.id);
            if (category == null)
                return;

            var buttonRect = args.rowRect;
            // D.AudioLog("buttonRect: " +buttonRect + ", " + args.label);
            buttonRect.x += buttonRect.width - 38f;
            buttonRect.width = 16f;
            if (GUI.Button(buttonRect, "+"))
            {
                category.Add(m_Data.CreateAudioManagerCategory("test"));
                Reload();
            }
            buttonRect.x += 18f;
            if (GUI.Button(buttonRect, "-"))
            {
                category.RemoveFromTree();
                Reload();
            }

            base.RowGUI(args);
        }

        protected override void SearchChanged(string newSearch)
        {
            // D.AudioLog("SearchChanged - newSearch: "+ newSearch + ", state.selectedIDs: "+ string.Join(", ", state.selectedIDs.Select(x => x.ToString()).ToArray()));

            base.SearchChanged(newSearch);

            if (string.IsNullOrEmpty(newSearch))
            {
                SetSelection(state.selectedIDs, TreeViewSelectionOptions.RevealAndFrame);
            }
        }

        protected override void SelectionChanged(IList<int> selectedIds)
        {
            // D.AudioLog("SelectionChanged - selectedIds: "+ string.Join(", ", selectedIds.Select(x => x.ToString()).ToArray()) + ", state - lastClickedID: " + state.lastClickedID);

            if (selectedIds.Count > 0)
            {
                AudioManagerCategory[] cats = selectedIds.Select(i => GetAudioCategory(i)).ToArray();
                OnSelectedCategory(cats);
            }
        }

        protected override bool CanStartDrag(CanStartDragArgs args)
        {
            return true;
        }


        protected override void SetupDragAndDrop(SetupDragAndDropArgs args)
        {
            DragAndDrop.PrepareStartDrag();

            // var sortedDraggedIDs = SortItemIDsInRowOrder (args.draggedItemIDs);
            var draggedRows = GetRows().Where(item => args.draggedItemIDs.Contains(item.id)).ToList();
            DragAndDrop.SetGenericData(k_GenericDragID, draggedRows);
            DragAndDrop.objectReferences = new UnityEngine.Object[] { }; // this IS required for dragging to work

            string title = draggedRows.Count == 1 ? draggedRows[0].displayName : "< Multiple >";
            DragAndDrop.StartDrag(title);
        }

        protected override DragAndDropVisualMode HandleDragAndDrop(DragAndDropArgs args)
        {
            if (hasSearch) return DragAndDropVisualMode.Rejected;

            // Check if we can handle the current drag data (could be dragged in from other areas/windows in the editor)
            var draggedRows = DragAndDrop.GetGenericData(k_GenericDragID) as List<TreeViewItem>;
            if (draggedRows == null)
                return DragAndDropVisualMode.None;
            var categories = new List<AudioManagerCategory>(draggedRows.Count);
            foreach (var obj in draggedRows)
            {
                var category = GetAudioCategory(obj.id);
                categories.Add(category);
            }


            // Filter out any unnecessary transforms before the reparent operation
            // RemoveItemsThatAreDescendantsFromOtherItems (transforms);

            // Reparent
            if (args.performDrop)
            {
                D.AudioLog("HandleDragAndDrop - dragAndDropPosition: " + args.dragAndDropPosition + ", insertAtIndex: " + args.insertAtIndex + ", parentItem: " + args.parentItem + ", performDrop: " + args.performDrop);
                switch (args.dragAndDropPosition)
                {
                    case DragAndDropPosition.UponItem:
                    case DragAndDropPosition.BetweenItems:
                        AudioManagerCategory parent = args.parentItem != null ? GetAudioCategory(args.parentItem.id) : null;
                        if (parent == null) break;

                        if (!IsValidReparenting(parent, categories))
                            return DragAndDropVisualMode.None;

                        PerformParenting(parent, categories, args);

                        break;

                    case DragAndDropPosition.OutsideItems:
                        PerformParenting(m_Root, categories, args);

                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                Reload();
                SetSelection(categories.Select(c => c.UniqueID).ToList(), TreeViewSelectionOptions.RevealAndFrame);
            }

            return DragAndDropVisualMode.Move;
        }


        private void PerformParenting(AudioManagerCategory parent, List<AudioManagerCategory> categories, DragAndDropArgs args)
        {
            bool isChangingParent = categories.Any(c => c.Parent != parent);
            bool showWarning = isChangingParent && !Event.current.control;

            if (!showWarning || EditorUtility.DisplayDialog("Reparent categories?",
                "Are you sure you want to move " + categories.Count + " items to parent " + parent.ID + "?",
                "Reparent", "Cancel"))
            {
                foreach (var cat in categories)
                {
                    SetParent(parent, cat);
                }

                if (args.dragAndDropPosition == DragAndDropPosition.BetweenItems)
                {
                    int insertIndex = args.insertAtIndex;
                    for (int i = categories.Count - 1; i >= 0; i--)
                    {
                        var cat = categories[i];
                        insertIndex = GetAdjustedInsertIndex(parent, cat, insertIndex);
                        SetSiblingIndex(cat, insertIndex);
                    }
                }

                m_Data.SaveTree();
                m_Data.ReconstructTreeChildren();
                m_Root = m_Data.TreeData;
            }
        }


        //TODO Move to AudioManagerCategory.cs
        private void SetParent(AudioManagerCategory parent, AudioManagerCategory category)
        {
            // D.AudioLog("SetParent: "+ category.ID + ":" + category.UniqueID + " to " + parent.ID + ":" + parent.UniqueID);

            var oldParent = category.Parent;
            oldParent.Children.Remove(category.ID);

            category.Parent = parent;
            if (parent.Children == null) parent.Children = new Dictionary<string, AudioManagerCategory>();
            parent.Children.Add(category.ID, category);
        }

        //TODO Move to AudioManagerCategory.cs
        private int GetSiblingIndex(AudioManagerCategory category)
        {
            if (category.Parent == null) return -1;
            var parentChildren = category.Parent.Children.Values.ToList();
            for (int i = 0; i < parentChildren.Count; i++)
            {
                if (parentChildren[i] == category) return i;
            }
            return -1;
        }

        //TODO Move to AudioManagerCategory.cs
        //TODO Optimize
        private void SetSiblingIndex(AudioManagerCategory category, int insertIdx)
        {
            // D.AudioLog("SetSiblingIndex - category: " + category.ID + ", insertIdx: "+ insertIdx);

            if (category.Parent == null) return;
            var buffer = category.Parent.Children.Values.ToList();

            int childCount = buffer.Count;

            var newChildren = new Dictionary<string, AudioManagerCategory>();
            int idx = 0;

            while (buffer.Count > 0 || !newChildren.ContainsKey(category.ID))
            {
                if (idx == insertIdx)
                {
                    newChildren.Add(category.ID, category);
                    buffer.Remove(category);
                    idx++;
                }
                else
                {
                    var child = buffer[0];
                    buffer.Remove(child);
                    if (child != category)
                    {
                        newChildren.Add(child.ID, child);
                        idx++;
                    }
                }
            }

            category.Parent.Children = newChildren;
        }

        int GetAdjustedInsertIndex(AudioManagerCategory parent, AudioManagerCategory categoryToInsert, int insertIndex)
        {
            if (categoryToInsert.Parent == parent && GetSiblingIndex(categoryToInsert) < insertIndex)
                return --insertIndex;
            return insertIndex;
        }

        bool IsValidReparenting(AudioManagerCategory parent, List<AudioManagerCategory> categoriesToMove)
        {
            if (parent == null)
                return true;

            foreach (var cat in categoriesToMove)
            {
                //Is trying to move parent to self
                if (cat == parent)
                    return false;

                //Is tryin to parent a parent to child
                if (IsHoveredAChildOfDragged(parent, cat))
                    return false;

                //Is trying to move object with same name as already exists
                if (IsHoveredChildrenIdsTaken(parent, cat))
                    return false;
            }

            return true;
        }


        bool IsHoveredAChildOfDragged(AudioManagerCategory hovered, AudioManagerCategory dragged)
        {
            AudioManagerCategory cat = hovered.Parent;
            while (cat != null)
            {
                if (cat == dragged)
                    return true;
                cat = cat.Parent;
            }
            return false;
        }

        bool IsHoveredChildrenIdsTaken(AudioManagerCategory hovered, AudioManagerCategory dragged)
        {
            if (hovered.Children == null) return false;
            foreach (var child in hovered.Children)
            {
                if (child.Value != dragged && child.Value.ID == dragged.ID) return true;
            }

            return false;
        }

        bool ValidDrag(TreeViewItem parent, List<TreeViewItem> draggedItems)
        {
            TreeViewItem currentParent = parent;
            while (currentParent != null)
            {
                if (draggedItems.Contains(currentParent))
                    return false;
                currentParent = currentParent.parent;
            }
            return true;
        }

        static void RemoveItemsThatAreDescendantsFromOtherItems(List<Transform> transforms)
        {
            transforms.RemoveAll(t => IsDescendantOf(t, transforms));
        }

        static bool IsDescendantOf(Transform transform, List<Transform> transforms)
        {
            while (transform != null)
            {
                transform = transform.parent;
                if (transforms.Contains(transform))
                    return true;
            }
            return false;
        }
    }
}
