using NUnit.Framework;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

/// <summary>
/// Quality Control tests for Robot Abuse Mini-Project.
/// These tests verify all visual and functional requirements are met.
/// Run via Unity Test Runner (Window > General > Test Runner > EditMode)
/// </summary>
[TestFixture]
public class QualityControlTests
{
    private const string SCENE_PATH = "Assets/Scenes/MainScene.unity";
    private const string FBX_PATH = "Assets/Models/Robot_Toy.fbx";
    private const string MATERIAL_PATH = "Assets/Models/Robot_Toy.mat";

    #region Scene Setup Tests

    [Test]
    public void QC_SceneExists()
    {
        var scene = AssetDatabase.LoadAssetAtPath<SceneAsset>(SCENE_PATH);
        Assert.IsNotNull(scene, "MainScene.unity must exist at " + SCENE_PATH);
    }

    [Test]
    public void QC_RobotModelExists()
    {
        var fbx = AssetDatabase.LoadAssetAtPath<GameObject>(FBX_PATH);
        Assert.IsNotNull(fbx, "Robot_Toy.fbx must exist at " + FBX_PATH);
    }

    [Test]
    public void QC_MaterialWithTexturesExists()
    {
        var material = AssetDatabase.LoadAssetAtPath<Material>(MATERIAL_PATH);
        Assert.IsNotNull(material, "Robot_Toy.mat must exist");

        // Check textures are assigned
        var mainTex = material.GetTexture("_MainTex");
        Assert.IsNotNull(mainTex, "Albedo texture must be assigned to material");
    }

    #endregion

    #region Camera Tests

    [Test]
    public void QC_CameraControllerExists()
    {
        EditorSceneManager.OpenScene(SCENE_PATH);
        var camera = Camera.main;
        Assert.IsNotNull(camera, "Main camera must exist");

        var controller = camera.GetComponent<CameraController>();
        Assert.IsNotNull(controller, "CameraController must be attached to main camera");
    }

    [Test]
    public void QC_CameraFOVIsReasonable()
    {
        EditorSceneManager.OpenScene(SCENE_PATH);
        var camera = Camera.main;
        Assert.IsNotNull(camera);

        // FOV should be between 40-70 for good framing
        Assert.GreaterOrEqual(camera.fieldOfView, 40f, "FOV should be at least 40°");
        Assert.LessOrEqual(camera.fieldOfView, 70f, "FOV should be at most 70°");
    }

    [Test]
    public void QC_RobotHeadIsInCameraView()
    {
        EditorSceneManager.OpenScene(SCENE_PATH);
        var camera = Camera.main;
        var robot = GameObject.Find("Robot_Toy");

        Assert.IsNotNull(camera, "Camera required");
        Assert.IsNotNull(robot, "Robot required");

        // Find head position (approximately 0.25 units above robot origin)
        Vector3 headPos = robot.transform.position + Vector3.up * 0.25f;

        // Check if head is in camera frustum
        Vector3 viewportPoint = camera.WorldToViewportPoint(headPos);

        // Viewport coordinates: (0,0) bottom-left, (1,1) top-right
        // Head should be in frame (between 0 and 1) and in front of camera (z > 0)
        Assert.Greater(viewportPoint.z, 0, "Head must be in front of camera");
        Assert.GreaterOrEqual(viewportPoint.x, 0.1f, "Head X must be in frame");
        Assert.LessOrEqual(viewportPoint.x, 0.9f, "Head X must be in frame");
        Assert.GreaterOrEqual(viewportPoint.y, 0.1f, "Head Y must be in frame (not cropped)");
        Assert.LessOrEqual(viewportPoint.y, 0.95f, "Head Y must have some margin from top");
    }

    [Test]
    public void QC_RobotFeetAreInCameraView()
    {
        EditorSceneManager.OpenScene(SCENE_PATH);
        var camera = Camera.main;
        var robot = GameObject.Find("Robot_Toy");

        Assert.IsNotNull(camera);
        Assert.IsNotNull(robot);

        // Robot origin is at feet
        Vector3 feetPos = robot.transform.position;
        Vector3 viewportPoint = camera.WorldToViewportPoint(feetPos);

        Assert.Greater(viewportPoint.z, 0, "Feet must be in front of camera");
        Assert.GreaterOrEqual(viewportPoint.y, 0.05f, "Feet must be visible (not below frame)");
    }

    #endregion

    #region Robot Component Tests

    [Test]
    public void QC_RobotHasRequiredComponents()
    {
        EditorSceneManager.OpenScene(SCENE_PATH);
        var robot = GameObject.Find("Robot_Toy");
        Assert.IsNotNull(robot, "Robot must exist in scene");

        // Check for renderers
        var renderers = robot.GetComponentsInChildren<Renderer>();
        Assert.Greater(renderers.Length, 0, "Robot must have renderers");

        // Check for colliders (for raycasting)
        var colliders = robot.GetComponentsInChildren<Collider>();
        Assert.Greater(colliders.Length, 0, "Robot must have colliders for interaction");
    }

    [Test]
    public void QC_TorsoHasRobotController()
    {
        EditorSceneManager.OpenScene(SCENE_PATH);
        var controllers = Object.FindObjectsOfType<RobotController>();
        Assert.Greater(controllers.Length, 0, "RobotController must exist for torso interaction");
    }

    [Test]
    public void QC_ArmHasArmController()
    {
        EditorSceneManager.OpenScene(SCENE_PATH);
        var controllers = Object.FindObjectsOfType<ArmController>();
        Assert.Greater(controllers.Length, 0, "ArmController must exist for arm detach/reattach");
    }

    [Test]
    public void QC_ArmControllerStartsAttached()
    {
        EditorSceneManager.OpenScene(SCENE_PATH);
        var armController = Object.FindObjectOfType<ArmController>();
        Assert.IsNotNull(armController);
        Assert.IsTrue(armController.IsAttached, "Arm must start in attached state");
    }

    #endregion

    #region UI Tests

    [Test]
    public void QC_StatusDisplayExists()
    {
        EditorSceneManager.OpenScene(SCENE_PATH);
        var status = Object.FindObjectOfType<RobotStatus>();
        Assert.IsNotNull(status, "RobotStatus component must exist for status display");
    }

    [Test]
    public void QC_StatusShowsAttachedByDefault()
    {
        EditorSceneManager.OpenScene(SCENE_PATH);
        var status = Object.FindObjectOfType<RobotStatus>();
        Assert.IsNotNull(status);
        Assert.AreEqual("Attached", status.GetStatus(), "Initial status must be 'Attached'");
    }

    #endregion

    #region Lighting Tests

    [Test]
    public void QC_SceneHasLighting()
    {
        EditorSceneManager.OpenScene(SCENE_PATH);
        var lights = Object.FindObjectsOfType<Light>();
        Assert.Greater(lights.Length, 0, "Scene must have at least one light");
    }

    [Test]
    public void QC_SceneHasMultipleLightsFor3PointSetup()
    {
        EditorSceneManager.OpenScene(SCENE_PATH);
        var lights = Object.FindObjectsOfType<Light>();
        // 3-point lighting: key, fill, rim
        Assert.GreaterOrEqual(lights.Length, 2, "Scene should have multiple lights for good lighting");
    }

    #endregion

    #region Ground/Environment Tests

    [Test]
    public void QC_GroundPlaneExists()
    {
        EditorSceneManager.OpenScene(SCENE_PATH);
        var ground = GameObject.Find("Ground");
        Assert.IsNotNull(ground, "Ground plane must exist for shadows and arm physics");
    }

    [Test]
    public void QC_GroundHasCollider()
    {
        EditorSceneManager.OpenScene(SCENE_PATH);
        var ground = GameObject.Find("Ground");
        Assert.IsNotNull(ground);

        var collider = ground.GetComponent<Collider>();
        Assert.IsNotNull(collider, "Ground must have collider for physics");
    }

    #endregion

    #region Build Settings Tests

    [Test]
    public void QC_SceneIsInBuildSettings()
    {
        var scenes = EditorBuildSettings.scenes;
        bool found = false;
        foreach (var scene in scenes)
        {
            if (scene.path == SCENE_PATH && scene.enabled)
            {
                found = true;
                break;
            }
        }
        Assert.IsTrue(found, "MainScene must be in build settings and enabled");
    }

    [Test]
    public void QC_ProductNameIsCorrect()
    {
        // Product name should follow naming convention
        string productName = PlayerSettings.productName;
        Assert.IsTrue(productName.StartsWith("RobotAbuse"),
            "Product name should start with 'RobotAbuse', got: " + productName);
    }

    [Test]
    public void QC_WebGLBuildTargetSupported()
    {
        // This test just verifies WebGL module is available
        bool webglSupported = BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.WebGL, BuildTarget.WebGL);
        Assert.IsTrue(webglSupported, "WebGL build target must be supported");
    }

    #endregion

    #region Aspect Ratio Tests

    [Test]
    public void QC_DefaultResolutionIsSquare()
    {
        int width = PlayerSettings.defaultWebScreenWidth;
        int height = PlayerSettings.defaultWebScreenHeight;

        Assert.AreEqual(width, height,
            string.Format("WebGL resolution should be square (1:1), got {0}x{1}", width, height));
    }

    #endregion
}
