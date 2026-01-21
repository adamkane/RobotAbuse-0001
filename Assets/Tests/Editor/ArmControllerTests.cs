using NUnit.Framework;
using UnityEngine;

/// <summary>
/// Edit Mode unit tests for the ArmController.
/// These tests verify the arm attachment state logic.
/// </summary>
[TestFixture]
public class ArmControllerTests
{
    private GameObject testArm;
    private ArmController armController;

    [SetUp]
    public void SetUp()
    {
        // Create a test arm object
        testArm = new GameObject("TestArm");
        armController = testArm.AddComponent<ArmController>();
    }

    [TearDown]
    public void TearDown()
    {
        // Clean up
        if (testArm != null)
        {
            Object.DestroyImmediate(testArm);
        }
    }

    /// <summary>
    /// Test that the arm starts in the attached state.
    /// </summary>
    [Test]
    public void ArmController_StartsAttached()
    {
        // Assert
        Assert.IsTrue(armController.IsAttached,
            "Arm should start in attached state");
    }

    /// <summary>
    /// Test that SetAttached(false) changes state to detached.
    /// </summary>
    [Test]
    public void ArmController_CanBeSetToDetached()
    {
        // Act
        armController.SetAttached(false);

        // Assert
        Assert.IsFalse(armController.IsAttached,
            "Arm should be detached after SetAttached(false)");
    }

    /// <summary>
    /// Test that SetAttached(true) changes state to attached.
    /// </summary>
    [Test]
    public void ArmController_CanBeReattached()
    {
        // Arrange - first detach
        armController.SetAttached(false);
        Assert.IsFalse(armController.IsAttached);

        // Act - then reattach
        armController.SetAttached(true);

        // Assert
        Assert.IsTrue(armController.IsAttached,
            "Arm should be attached after SetAttached(true)");
    }

    /// <summary>
    /// Test that demonstrates the test CAN fail under specific conditions.
    /// This test would fail if we expected wrong initial state.
    /// </summary>
    [Test]
    public void ArmController_InitialStateIsCorrect()
    {
        // This test verifies expected behavior
        // It would fail if:
        // - The default IsAttached value was false instead of true
        // - The ArmController constructor set isAttached = false

        // The arm should NOT start detached
        Assert.IsFalse(armController.IsAttached == false && armController.IsAttached == true,
            "Logic error - arm cannot be both attached and detached");

        // Initial state must be attached
        Assert.IsTrue(armController.IsAttached,
            "Initial state should be attached");
    }

    /// <summary>
    /// Test state toggle behavior.
    /// </summary>
    [Test]
    public void ArmController_StateToggleWorks()
    {
        // Start attached
        Assert.IsTrue(armController.IsAttached);

        // Toggle to detached
        armController.SetAttached(!armController.IsAttached);
        Assert.IsFalse(armController.IsAttached);

        // Toggle back to attached
        armController.SetAttached(!armController.IsAttached);
        Assert.IsTrue(armController.IsAttached);
    }
}
