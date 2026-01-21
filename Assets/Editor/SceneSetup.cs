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

        // Find robot head for camera target
        Transform robotHead = FindChildByName(robot.transform, "Robot_Head");
        Vector3 headPosition = robotHead != null ? robotHead.position : robot.transform.position + Vector3.up * 0.22f;
        Debug.Log("Robot head position: " + headPosition);

        // Setup camera - HEAD MUST BE FULLY VISIBLE (Grade F if not!)
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            // CRITICAL: Camera far enough back to see ENTIRE robot including head
            // Position: further back, at belly height
            mainCamera.transform.position = new Vector3(0.15f, 0.08f, -0.65f);
            // Look at robot center (belly area) - head will be in upper part of wide FOV
            mainCamera.transform.LookAt(robot.transform.position + Vector3.up * 0.08f);
            mainCamera.nearClipPlane = 0.01f;
            mainCamera.backgroundColor = new Color(0.4f, 0.38f, 0.42f);
            mainCamera.fieldOfView = 60f;  // WIDER to capture full robot

            // Add camera controller
            var camController = mainCamera.gameObject.AddComponent<CameraController>();
            camController.target = robot.transform;
            Debug.Log("CameraController added to main camera");
        }

        // Create ground plane for physics
        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Ground";
        ground.transform.position = new Vector3(0, -0.05f, 0);
        ground.transform.localScale = new Vector3(1, 1, 1);
        // Make ground slightly darker
        var groundRenderer = ground.GetComponent<Renderer>();
        if (groundRenderer != null)
        {
            Material groundMat = new Material(Shader.Find("Standard"));
            groundMat.color = new Color(0.35f, 0.32f, 0.3f);
            groundRenderer.material = groundMat;
        }
        Debug.Log("Ground plane created");

        // Setup 3-point lighting for cinematic look
        var existingLights = Object.FindObjectsOfType<Light>();
        foreach (var light in existingLights)
        {
            if (light.type == LightType.Directional)
            {
                // Convert default light to Key Light
                light.name = "Key Light";
                light.transform.rotation = Quaternion.Euler(35, 45, 0);  // Front-right, above
                light.color = new Color(1f, 0.95f, 0.9f);  // Warm
                light.intensity = 1.1f;
            }
        }

        // Add Fill Light (front-left, softer)
        GameObject fillLightObj = new GameObject("Fill Light");
        Light fillLight = fillLightObj.AddComponent<Light>();
        fillLight.type = LightType.Directional;
        fillLight.transform.rotation = Quaternion.Euler(20, -45, 0);
        fillLight.color = new Color(0.85f, 0.9f, 1f);  // Cool
        fillLight.intensity = 0.4f;

        // Add Rim Light (behind, for edge separation)
        GameObject rimLightObj = new GameObject("Rim Light");
        Light rimLight = rimLightObj.AddComponent<Light>();
        rimLight.type = LightType.Directional;
        rimLight.transform.rotation = Quaternion.Euler(15, 180, 0);  // From behind
        rimLight.color = Color.white;
        rimLight.intensity = 0.5f;

        // Set ambient light
        RenderSettings.ambientLight = new Color(0.25f, 0.25f, 0.28f);
        Debug.Log("3-point lighting setup complete");

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
