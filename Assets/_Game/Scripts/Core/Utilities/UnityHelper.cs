using UnityEngine;

public static class UnityHelper
{
    public static TView InstantiatieView<TView>(GameObject prefab, Transform parent)
    {
        var go = GameObject.Instantiate(prefab, parent) as GameObject;
        return go.GetComponent<TView>();
    }

    public static TView InstantiatieViewAt<TView>(GameObject prefab, Transform parent, int index)
    {
        if (parent.childCount > index)
        {
            return parent.GetChild(index).GetComponent<TView>();
        }
        else
        {
            return UnityHelper.InstantiatieView<TView>(prefab, parent);
        }
    }
}