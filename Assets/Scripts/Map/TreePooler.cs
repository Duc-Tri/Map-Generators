using MapGenerator;
using System.Collections.Generic;
using UnityEngine;

// TODO : make generic, ie for all sorts of objects, props, etc.

// TODO : use ScriptableObject to set prefabs, etc.
public static class TreePooler
{
    static int n = 0;
    static List<GameObject> treePrefabs;
    static List<GameObject> treePool = new List<GameObject>();
    //static List<GameObject> treeActive = new List<GameObject>();

    public static bool SetTreePrefabs(List<GameObject> prefabs)
    {
        treePrefabs = prefabs;

        // sanity check
        foreach (GameObject prefab in treePrefabs)
        {
            if (prefab == null || !prefab.CompareTag(Const.TAG_TREE)) return false;
        }

        return true;
    }

    public static GameObject GetTree(Map map, int x, int y)
    {
        GameObject tree = null;

        if (treePool.Count > 0)
        {
            tree = treePool[0];
            treePool.RemoveAt(0);
            tree.name = "recycled_tree" + (n++);
        }
        else
        {
            tree = GameObject.Instantiate(treePrefabs[Random.Range(0, treePrefabs.Count)], map.transform);
            tree.name = "created_tree" + (n++);
        }

        tree.transform.position = new Vector3(x, 0, y);
        tree.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360f), 0);
        tree.transform.localScale = Vector3.one * Random.Range(0.8f, 1.2f);
        tree.SetActive(true);

        return tree;
    }

    public static void SaveTree(GameObject tree)
    {
        if (!tree.CompareTag(Const.TAG_TREE)) return;

        tree.name = "inactive_tree" + (n++);
        tree.SetActive(false);
        treePool.Add(tree);
    }

    public static void SaveAllTrees(Map map)
    {
        GameObject[] trees = GameObject.FindGameObjectsWithTag(Const.TAG_TREE);
        foreach (GameObject t in trees)
        {
            SaveTree(t);
        }

    }


}
