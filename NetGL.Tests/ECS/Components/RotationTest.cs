using Microsoft.VisualStudio.TestTools.UnitTesting;
using NetGL.ECS;
using OpenTK.Mathematics;

namespace NetGL.Tests.ECS.Components;

[TestClass]
public class RotationTest {

    [TestMethod]
    public void TestRotation() {
        Assert.AreEqual(Rotation.Direction.Forward.forward, Vector3.UnitZ);
        Assert.AreEqual(Rotation.Direction.Forward.right, Vector3.UnitX);
        Assert.AreEqual(Rotation.Direction.Forward.up, Vector3.UnitY);

        Rotation rot = Rotation.Direction.Forward;
        Assert.AreEqual(rot.yaw_pitch_roll.X, 0);
        Assert.AreEqual(rot.yaw_pitch_roll.Y, 0);
        Assert.AreEqual(rot.yaw_pitch_roll.Z, 0);

        Assert.AreEqual(rot, Rotation.Direction.Forward);
        Assert.AreEqual(rot.right, Vector3.UnitX);
        Assert.AreEqual(rot.left, -Vector3.UnitX);

        rot.yaw(90);
        Assert.AreEqual(rot, Rotation.Direction.Right);
    }
}