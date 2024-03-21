using System.Collections.Generic;
using HietakissaUtils.QOL;
using HietakissaUtils;
using UnityEngine;

public class TreeNavMeshFix : MonoBehaviour
{
    [SerializeField] Terrain terrain;
    [SerializeField] Transform treeParent;

    readonly List<GameObject> treeCopies = new List<GameObject>();


    [ContextMenu("Create Tree Copies")]
    void CreateTreeCopies()
    {
        Debug.Log($"prototype length: {terrain.terrainData.treePrototypes.Length}");
        for (int prototypeIndex = 0; prototypeIndex < terrain.terrainData.treePrototypes.Length; prototypeIndex++)
        {
            TreePrototype tree = terrain.terrainData.treePrototypes[prototypeIndex];

            List<TreeInstance> treeInstances = new List<TreeInstance>();
            foreach (TreeInstance treeInstance in terrain.terrainData.treeInstances)
            {
                if (treeInstance.prototypeIndex == prototypeIndex) treeInstances.Add(treeInstance);
            }


            for (int instanceIndex = 0; instanceIndex < treeInstances.Count; instanceIndex++)
            {
                CreateCopyForInstance(tree, treeInstances[instanceIndex]);
            }
        }
    }

    [ContextMenu("Delete Tree Copies")]
    void DeleteTreeCopies()
    {
        if (treeCopies.Count == 0) treeParent.DestroyChildrenImmediate();
        else
        {
            foreach (GameObject tree in treeCopies) QOL.Destroy(tree);

            treeCopies.Clear();
        }
    }

    void CreateCopyForInstance(TreePrototype tree, TreeInstance instance)
    {
        CapsuleCollider capsule = tree.prefab.GetComponent<CapsuleCollider>();
        if (!capsule) return;

        Vector3 capsuleSize = new Vector3(capsule.radius * 2f, capsule.height * 0.5f, capsule.radius * 2f);
        Vector3 posOnTerrain = Vector3.Scale(instance.position, terrain.terrainData.size);


        GameObject copy = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        copy.name = tree.prefab.name + treeCopies.Count;
        copy.layer = terrain.preserveTreePrototypeLayers ? tree.prefab.layer : terrain.gameObject.layer;

        copy.transform.position = terrain.GetPosition() + posOnTerrain + Vector3.up * capsule.height * 0.5f;
        copy.transform.localRotation = Quaternion.Euler(0, Mathf.Rad2Deg * instance.rotation, 0);
        copy.transform.localScale = capsuleSize;

        copy.transform.parent = treeParent;
        copy.isStatic = true;

        treeCopies.Add(copy);
    }
}
