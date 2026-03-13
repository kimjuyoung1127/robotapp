// Folder: Tests - EditMode unit tests; no scene required.
using NUnit.Framework;
using KineTutor3D.App.Fairino;

namespace KineTutor3D.Tests.EditMode
{
    [TestFixture]
    public class MockFairinoClientTests
    {
        private MockFairinoClient client;

        [SetUp]
        public void SetUp()
        {
            client = new MockFairinoClient();
        }

        [Test]
        public void InitialState_NotConnected()
        {
            Assert.IsFalse(client.IsConnected);
            Assert.IsFalse(client.IsEnabled);
        }

        [Test]
        public void Connect_ValidIp_Succeeds()
        {
            var result = client.Connect("192.168.58.2", 8080);
            Assert.IsTrue(result.IsSuccess);
            Assert.IsTrue(client.IsConnected);
        }

        [Test]
        public void Connect_EmptyIp_Fails()
        {
            var result = client.Connect("", 8080);
            Assert.IsFalse(result.IsSuccess);
            Assert.IsFalse(client.IsConnected);
        }

        [Test]
        public void Disconnect_AfterConnect_Succeeds()
        {
            client.Connect("192.168.58.2", 8080);
            var result = client.Disconnect();
            Assert.IsTrue(result.IsSuccess);
            Assert.IsFalse(client.IsConnected);
            Assert.IsFalse(client.IsEnabled);
        }

        [Test]
        public void Enable_WhenConnected_Succeeds()
        {
            client.Connect("192.168.58.2", 8080);
            var result = client.Enable();
            Assert.IsTrue(result.IsSuccess);
            Assert.IsTrue(client.IsEnabled);
        }

        [Test]
        public void Enable_WhenNotConnected_Fails()
        {
            var result = client.Enable();
            Assert.IsFalse(result.IsSuccess);
        }

        [Test]
        public void MoveJ_WhenEnabledWithValidJoints_Succeeds()
        {
            client.Connect("192.168.58.2", 8080);
            client.Enable();

            var joints = new double[] { 10, 20, 30, 40, 50, 60 };
            var result = client.MoveJ(joints, 30, 50);
            Assert.IsTrue(result.IsSuccess);
        }

        [Test]
        public void MoveJ_WhenNotEnabled_Fails()
        {
            client.Connect("192.168.58.2", 8080);

            var joints = new double[] { 10, 20, 30, 40, 50, 60 };
            var result = client.MoveJ(joints, 30, 50);
            Assert.IsFalse(result.IsSuccess);
        }

        [Test]
        public void MoveJ_WrongArrayLength_Fails()
        {
            client.Connect("192.168.58.2", 8080);
            client.Enable();

            var result = client.MoveJ(new double[] { 10, 20 }, 30, 50);
            Assert.IsFalse(result.IsSuccess);
        }

        [Test]
        public void ServoJ_WhenEnabledWithValidJoints_Succeeds()
        {
            client.Connect("192.168.58.2", 8080);
            client.Enable();

            var result = client.ServoJ(new double[] { 0, 0, 0, 0, 0, 0 });
            Assert.IsTrue(result.IsSuccess);
        }

        [Test]
        public void ReadState_WhenConnected_ReturnsState()
        {
            client.Connect("192.168.58.2", 8080);

            var result = client.ReadState();
            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(6, result.Value.JointPosDeg.Length);
            Assert.AreEqual(6, result.Value.TcpPose.Length);
        }

        [Test]
        public void ReadState_AfterMoveJ_ReflectsTarget()
        {
            client.Connect("192.168.58.2", 8080);
            client.Enable();

            var target = new double[] { 10, 20, 30, 40, 50, 60 };
            client.MoveJ(target, 30, 50);

            var state = client.ReadState();
            Assert.IsTrue(state.IsSuccess);
            for (int i = 0; i < 6; i++)
            {
                Assert.AreEqual(target[i], state.Value.JointPosDeg[i], 1e-10,
                    $"Joint {i} should reflect MoveJ target");
            }
        }

        [Test]
        public void ReadState_WhenNotConnected_Fails()
        {
            var result = client.ReadState();
            Assert.IsFalse(result.IsSuccess);
        }

        [Test]
        public void StopMotion_Succeeds()
        {
            var result = client.StopMotion();
            Assert.IsTrue(result.IsSuccess);
        }

        [Test]
        public void GetVersion_WhenConnected_ReturnsVersionInfo()
        {
            client.Connect("192.168.58.2", 8080);

            var result = client.GetVersion();
            Assert.IsTrue(result.IsSuccess);
            Assert.IsFalse(string.IsNullOrEmpty(result.Value.FirmwareVersion));
            Assert.IsFalse(string.IsNullOrEmpty(result.Value.SdkVersion));
        }

        [Test]
        public void GetVersion_WhenNotConnected_Fails()
        {
            var result = client.GetVersion();
            Assert.IsFalse(result.IsSuccess);
        }
    }
}
