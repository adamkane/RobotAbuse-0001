using UnityEditor;
using UnityEngine;
using System.Linq;

public class FBXValidator
{
    [MenuItem("Tools/Validate Robot FBX")]
    public static void ValidateRobotFBX()
    {
        string[] possiblePaths = {
            "Assets/Models/Robot_Toy.fbx",
            "Assets/Models/source/Robot_Toy.fbx",
            "Assets/Models/Robot_Toy/Robot_Toy.fbx"
        };

        GameObject prefab = null;
        string foundPath = null;

        foreach (var path in possiblePaths)
        {
            prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab != null) { foundPath = path; break; }
        }

        if (prefab == null)
        {
            Debug.LogError("FAIL: FBX not found. Searched: " + string.Join(", ", possiblePaths));
            Debug.LogError("ACTION: List Assets/Models/ contents and update path");
            return;
        }
        Debug.Log("PASS: FBX found at " + foundPath);

        // Check 1: Has child transforms (not a flat mesh)
        Transform[] children = prefab.GetComponentsInChildren<Transform>();
        Debug.Log("INFO: FBX hierarchy has " + children.Length + " transforms:");
        foreach (var t in children)
            Debug.Log("  - " + t.name + " (parent: " + (t.parent ? t.parent.name : "none") + ")");

        if (children.Length < 3)
        {
            Debug.LogError("FAIL: FBX has insufficient hierarchy (" + children.Length + " parts)");
            Debug.LogError("ACTION: Model needs separate meshes for torso and arm. Cannot proceed.");
            return;
        }
        Debug.Log("PASS: Hierarchy has sufficient parts (" + children.Length + " transforms)");

        // Check 2: Has MeshRenderers or SkinnedMeshRenderers
        MeshRenderer[] renderers = prefab.GetComponentsInChildren<MeshRenderer>();
        SkinnedMeshRenderer[] skinnedRenderers = prefab.GetComponentsInChildren<SkinnedMeshRenderer>();
        int totalRenderers = renderers.Length + skinnedRenderers.Length;

        if (totalRenderers == 0)
        {
            Debug.LogError("FAIL: No renderers found. Model appears empty.");
            return;
        }
        Debug.Log("PASS: Found " + renderers.Length + " MeshRenderers, " + skinnedRenderers.Length + " SkinnedMeshRenderers");

        // Check 3: Look for arm-like parts
        string[] armKeywords = {"arm", "hand", "shoulder", "upperarm", "forearm", "limb"};
        var armParts = children.Where(t =>
            armKeywords.Any(k => t.name.ToLower().Contains(k))).ToList();

        if (armParts.Count == 0)
        {
            Debug.LogWarning("WARN: No transforms with arm-related names found");
            Debug.LogWarning("ACTION: Review hierarchy above and identify which part can serve as detachable arm");
        }
        else
        {
            Debug.Log("PASS: Found " + armParts.Count + " arm-related parts:");
            foreach (var arm in armParts)
                Debug.Log("  -> " + arm.name);
        }

        // Check 4: Look for torso/body parts
        string[] torsoKeywords = {"torso", "body", "chest", "spine", "hip", "pelvis"};
        var torsoParts = children.Where(t =>
            torsoKeywords.Any(k => t.name.ToLower().Contains(k))).ToList();

        if (torsoParts.Count == 0)
        {
            Debug.LogWarning("WARN: No transforms with torso-related names found");
            Debug.LogWarning("ACTION: The root or main mesh will likely serve as torso");
        }
        else
        {
            Debug.Log("PASS: Found " + torsoParts.Count + " torso-related parts:");
            foreach (var torso in torsoParts)
                Debug.Log("  -> " + torso.name);
        }

        // Check 5: Verify model bounds
        MeshFilter[] meshFilters = prefab.GetComponentsInChildren<MeshFilter>();
        if (meshFilters.Length > 0)
        {
            Bounds bounds = new Bounds();
            foreach (var mf in meshFilters)
            {
                if (mf.sharedMesh != null)
                    bounds.Encapsulate(mf.sharedMesh.bounds);
            }
            Debug.Log("INFO: Model bounds size: " + bounds.size);
        }

        Debug.Log("=== FBX VALIDATION COMPLETE ===");
        Debug.Log("NEXT: Document your Torso and Arm transform names before proceeding to Step 5");
    }
}
