using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.IO;

/// <summary>
/// Editor script to set up the main scene with all required components.
/// Can be run in batchmode for automated builds.
/// </summary>
public class SceneSetup
{
    [MenuItem("Tools/Setup Main Scene")]
    public static void SetupMainScene()
    {
        Debug.Log("=== Starting Scene Setup ===");

        // Create a new scene
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        // Load the robot prefab
        string fbxPath = "Assets/Models/Robot_Toy.fbx";
        GameObject robotPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(fbxPath);

        if (robotPrefab == null)
        {
            Debug.LogError("Failed to load robot prefab from: " + fbxPath);
            return;
        }

        // Instantiate robot
        GameObject robot = (GameObject)PrefabUtility.InstantiatePrefab(robotPrefab);
        robot.name = "Robot_Toy";
        robot.transform.position = Vector3.zero;
        robot.transform.rotation = Quaternion.identity;
        Debug.Log("Robot instantiated");

        // Apply material with textures
        Material robotMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Models/Robot_Toy.mat");
        if (robotMaterial != null)
        {
            foreach (Renderer renderer in robot.GetComponentsInChildren<Renderer>())
            {
                renderer.sharedMaterial = robotMaterial;
            }
            Debug.Log("Applied textured material to robot");
        }
        else
        {
            Debug.LogWarning("Robot material not found");
        }

        // Add colliders to all mesh renderers
        AddCollidersToRobot(robot);

        // Find torso and add controller
        Transform torso = FindChildByName(robot.transform, "Robot_Torso");
        if (torso != null)
        {
            torso.gameObject.tag = "Torso";
            var robotController = torso.gameObject.AddComponent<RobotController>();
            robotController.allRenderers = robot.GetComponentsInChildren<Renderer>();
            Debug.Log("RobotController added to torso");
        }
        else
        {
            Debug.LogWarning("Could not find Robot_Torso");
        }

        // Find right arm and add controller
        Transform rightArm = FindChildByName(robot.transform, "Robot_Upperarm_Right");
        if (rightArm != null)
        {
            rightArm.gameObject.tag = "Arm";
            var armController = rightArm.gameObject.AddComponent<ArmController>();
            Debug.Log("ArmController added to right arm");
        }
        else
        {
            Debug.LogWarning("Could not find Robot_Upperarm_Right");
        }

        // Create status display object
        GameObject statusObj = new GameObject("RobotStatus");
        var statusUI = statusObj.AddComponent<RobotStatus>();
        Debug.Log("RobotStatus created");

        // Link arm controller to status
        if (rightArm != null)
        {
            var armCtrl = rightArm.GetComponent<ArmController>();
            if (armCtrl != null)
            {
                armCtrl.statusUI = statusUI;
            }
        }

        // Setup camera - position very close so robot fills ~80% of view
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            mainCamera.transform.position = new Vector3(0.1f, 0.18f, -0.28f);  // Even closer
            mainCamera.transform.LookAt(robot.transform.position + Vector3.up * 0.12f);
            mainCamera.nearClipPlane = 0.01f;  // Allow close-up

            // Add camera controller
            var camController = mainCamera.gameObject.AddComponent<CameraController>();
            camController.target = robot.transform;
            Debug.Log("CameraController added to main camera");
        }

        // Setup lighting
        var lights = Object.FindObjectsOfType<Light>();
        foreach (var light in lights)
        {
            if (light.type == LightType.Directional)
            {
                light.transform.rotation = Quaternion.Euler(50, -30, 0);
                light.intensity = 1.2f;
            }
        }

        // Ensure Scenes folder exists
        if (!Directory.Exists("Assets/Scenes"))
        {
            Directory.CreateDirectory("Assets/Scenes");
        }

        // Save scene
        string scenePath = "Assets/Scenes/MainScene.unity";
        EditorSceneManager.SaveScene(scene, scenePath);
        Debug.Log("Scene saved to: " + scenePath);

        // Add scene to build settings
        AddSceneToBuildSettings(scenePath);

        Debug.Log("=== Scene Setup Complete ===");
    }

    static void AddCollidersToRobot(GameObject robot)
    {
        // Add mesh colliders to all parts with mesh renderers
        MeshRenderer[] renderers = robot.GetComponentsInChildren<MeshRenderer>();
        foreach (var renderer in renderers)
        {
            MeshFilter meshFilter = renderer.GetComponent<MeshFilter>();
            if (meshFilter != null && meshFilter.sharedMesh != null)
            {
                // Add mesh collider if not already present
                if (renderer.GetComponent<Collider>() == null)
                {
                    MeshCollider collider = renderer.gameObject.AddComponent<MeshCollider>();
                    collider.sharedMesh = meshFilter.sharedMesh;
                }
            }
        }
        Debug.Log("Added colliders to " + renderers.Length + " mesh parts");
    }

    static Transform FindChildByName(Transform parent, string exactName)
    {
        foreach (Transform child in parent.GetComponentsInChildren<Transform>())
        {
            if (child.name == exactName)
                return child;
        }
        return null;
    }

    static void AddSceneToBuildSettings(string scenePath)
    {
        var scenes = EditorBuildSettings.scenes;
        foreach (var s in scenes)
        {
            if (s.path == scenePath) return; // Already added
        }

        var newScenes = new EditorBuildSettingsScene[scenes.Length + 1];
        scenes.CopyTo(newScenes, 0);
        newScenes[scenes.Length] = new EditorBuildSettingsScene(scenePath, true);
        EditorBuildSettings.scenes = newScenes;
        Debug.Log("Added scene to build settings");
    }

    [MenuItem("Tools/Debug FBX Hierarchy")]
    public static void DebugFBXHierarchy()
    {
        string fbxPath = "Assets/Models/Robot_Toy.fbx";
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(fbxPath);

        if (prefab == null)
        {
            Debug.LogError("FBX not found at: " + fbxPath);
            return;
        }

        Debug.Log("=== FBX Hierarchy ===");
        PrintHierarchy(prefab.transform, 0);
    }

    static void PrintHierarchy(Transform t, int depth)
    {
        string indent = new string(' ', depth * 2);
        Debug.Log(indent + "- " + t.name);
        foreach (Transform child in t)
        {
            PrintHierarchy(child, depth + 1);
        }
    }
}
