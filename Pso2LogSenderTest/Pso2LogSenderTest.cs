using System;
using NUnit.Framework;
using Hakusai.Pso2LogSender;
using Hakusai.Pso2;
using Hakusai.Cavetube;
using Moq;
using CaveTube.CaveTubeClient;
using System.Threading;
using Hakusai.Livetube;
using System.Reflection;

namespace Hakusai.Pso2LogSender.Test
{
    class MockCavetubeAuthFactory : ICavetubeAuthFactory
    {
        public Mock<ICavetubeAuth> MockCavetubeAuth = new Mock<ICavetubeAuth>();

        public ICavetubeAuth CreateCavetubeAuth()
        {
            return MockCavetubeAuth.Object;
        }

        public void Dispose()
        { }
    }

    class CavetubeAuthManagerTest
    {
        [Test]
        public void GetApiKeyTest()
        {
            var factory = new MockCavetubeAuthFactory();
            var mock = factory.MockCavetubeAuth;
            mock.Setup(a => a.Login(It.IsAny<string>(), It.IsAny<string>())).Returns("apikey");
            ICavetubeAuth auth = mock.Object;

            using (CavetubeAuthManager manager = new CavetubeAuthManager("hakusai", "password", factory))
            {
                Assert.AreEqual("apikey", manager.GetApiKey());
            }

            mock.Verify(a => a.Logout("hakusai", "password"), Times.Once());
        }

        [Test]
        public void ConstructorTest()
        {
            using (var manager = new CavetubeAuthManager("someone", "password"))
            {
                var auth = manager.GetType().GetField("_auth",
                    BindingFlags.GetField | BindingFlags.SetField
                    | BindingFlags.NonPublic | BindingFlags.Instance).GetValue(manager);
                Assert.AreEqual(typeof(CavetubeAuthWrapper), auth.GetType());
            }
        }

        [Test]
        public void DestructorTest()
        {
            // usingを使わずに意図的にリーク
            new CavetubeAuthManager("someone", "password");
            // GC起動(using使わないでファイナライザを呼びたいから)(カバレッジを上げたいだけ)
            GC.Collect();
        }
    }

    public class CavetubeAuthFactoryTest
    {
        [Test]
        public void DisposeTest()
        {
            using (var factory = new CavetubeAuthFactory())
            {
                var auth = factory.CreateCavetubeAuth();
                Assert.AreEqual(typeof(CavetubeAuthWrapper), auth.GetType());
            }
        }
    }

    public class CavetubeClientFactoryTest
    {
        [Test]
        public void CreateCavetubeClientTest()
        {
            using (var factory = new CavetubeClientFactory())
            using (var client = factory.CreateCavetubeClient())
            {
                Assert.AreEqual(typeof(CavetubeClientWrapper), client.GetType());
            }
        }
    }

    class MockCavetubeClientFactory : ICavetubeClientFactory
    {
        public Mock<ICavetubeClient> MockCavetubeClient = new Mock<ICavetubeClient>();

        public ICavetubeClient CreateCavetubeClient()
        {
            return MockCavetubeClient.Object;
        }

        public void Dispose()
        { }
    }

    class CavetubeUtilityTest
    {
        [Test]
        public void JoinningTest()
        {
            var factory = new MockCavetubeClientFactory();
            var mockClient = factory.MockCavetubeClient;
            mockClient.Setup(cl => cl.JoinRoom(It.IsAny<string>())).Raises(m => m.OnJoin += null, "roomid");
            mockClient.Setup(cl => cl.LeaveRoom()).Raises(m => m.OnLeave += null, "roomid");
            var client = factory.CreateCavetubeClient();
            bool check = false;
            CavetubeUtility.Joinning(
                client,
                "roomId",
                () => {
                    check = true;
                });
            Assert.IsTrue(check);
        }
    }

    class MockPso2LogWatcherFactory : IPso2LogWatcherFactory
    {
        public Mock<IPso2LogWatcher> MockPso2LogWatcher = new Mock<IPso2LogWatcher>();

        public IPso2LogWatcher CreatePso2LogWatcher()
        {
            return MockPso2LogWatcher.Object;
        }

        public void Dispose()
        { }
    }

    class MockLivetubeClientFactory : ILivetubeClientFactory
    {
        public Mock<ILivetubeClient> MockLivetubeClient = new Mock<ILivetubeClient>();

        public ILivetubeClient CreateLivetubeClient()
        {
            return MockLivetubeClient.Object;
        }

        public void Dispose()
        { }
    }

    class Pso2LogSenderTest
    {
        [Test]
        public void CavetubeMainTest()
        {
            var clientFactory = new MockCavetubeClientFactory();
            var mockClient = clientFactory.MockCavetubeClient;
            var authFactory = new MockCavetubeAuthFactory();
            var mockAuth = authFactory.MockCavetubeAuth;
            var pso2Factory = new MockPso2LogWatcherFactory();
            var mockPso2 = pso2Factory.MockPso2LogWatcher;

            var summary = new SummaryWrapper();
            summary.RoomId = "roomid";
            var manualEvent = new ManualResetEvent(false);
            var check = false;

            mockAuth.Setup(a => a.Login(It.IsAny<string>(), It.IsAny<string>())).Returns("apikey");
            mockClient.Setup(cl => cl.JoinRoom(It.IsAny<string>())).Raises(m => m.OnJoin += null, "roomid");
            mockClient.Setup(cl => cl.LeaveRoom()).Raises(m => m.OnLeave += null, "roomid");
            mockClient.Setup(cl => cl.GetSummary(It.IsAny<string>())).Returns(summary);
            mockClient.Setup(cl => cl.PostComment(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Callback<string, string, string>((name, message, apikey) => {
                    manualEvent.Set();
                    Assert.AreEqual("someone", name);
                    Assert.AreEqual("message", message);
                    Assert.AreEqual("apikey", apikey);
                });

            var msg = new Pso2LogEventArgs();
            msg.From = "someone";
            msg.SendTo = "GUILD";
            msg.Message = "message";
            mockPso2.Setup(pso2 => pso2.Start()).Raises(pso2 => pso2.Pso2LogEvent += null, msg);

            Pso2LogSender.CavetubeMain(authFactory, clientFactory, pso2Factory, () =>
            {
                manualEvent.WaitOne();
                check = true;
            });

            Assert.True(check);
        }

        [Test]
        public void CavetubeMainTest2_RoomIdNotFound()
        {
            var clientFactory = new MockCavetubeClientFactory();
            var mockClient = clientFactory.MockCavetubeClient;
            var authFactory = new MockCavetubeAuthFactory();
            var mockAuth = authFactory.MockCavetubeAuth;
            var pso2Factory = new MockPso2LogWatcherFactory();
            var mockPso2 = pso2Factory.MockPso2LogWatcher;

            var summary = new SummaryWrapper();
            //summary.RoomId = "roomid";
            var manualEvent = new ManualResetEvent(false);
            var check = true;

            mockAuth.Setup(a => a.Login(It.IsAny<string>(), It.IsAny<string>())).Returns("apikey");
            mockClient.Setup(cl => cl.JoinRoom(It.IsAny<string>())).Raises(m => m.OnJoin += null, "roomid");
            mockClient.Setup(cl => cl.LeaveRoom()).Raises(m => m.OnLeave += null, "roomid");
            mockClient.Setup(cl => cl.GetSummary(It.IsAny<string>())).Returns(summary);
            mockClient.Setup(cl => cl.PostComment(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Callback<string, string, string>((name, message, apikey) =>
                {
                    check = false;
                    manualEvent.Set();
                    Assert.AreEqual("someone", name);
                    Assert.AreEqual("message", message);
                    Assert.AreEqual("apikey", apikey);
                });

            var msg = new Pso2LogEventArgs();
            msg.From = "someone";
            msg.SendTo = "GUILD";
            msg.Message = "message";
            mockPso2.Setup(pso2 => pso2.Start()).Raises(pso2 => pso2.Pso2LogEvent += null, msg);

            var ex = Assert.Throws<ApplicationException>(() =>
            {
                Pso2LogSender.CavetubeMain(authFactory, clientFactory, pso2Factory, () =>
                {
                    check = false;
                    manualEvent.WaitOne();
                });
            });
            Assert.AreEqual("部屋IDの取得に失敗しました", ex.Message);

            Assert.True(check);
        }

        [Test]
        public void LivetubeMainTest()
        {
            var clientFactory = new MockLivetubeClientFactory();
            var mockClient = clientFactory.MockLivetubeClient;
            var pso2Factory = new MockPso2LogWatcherFactory();
            var mockPso2 = pso2Factory.MockPso2LogWatcher;

            var manualEvent = new ManualResetEvent(false);
            var check = false;

            mockClient.Setup(cl => cl.FindCurrentBroadcasting(It.IsAny<string>())).Returns("http://livetube.cc/someone/sometitle");
            mockClient.Setup(cl => cl.FindStream(It.IsAny<string>())).Returns("stream_id");
            mockClient.Setup(cl => cl.PostComment(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Callback<string, string, string>((stream, user, comment) =>
                {
                    manualEvent.Set();
                    Assert.AreEqual("stream_id", stream);
                    Assert.AreEqual("someone", user);
                    Assert.AreEqual("message", comment);
                });

            var msg = new Pso2LogEventArgs();
            msg.From = "someone";
            msg.SendTo = "GUILD";
            msg.Message = "message";
            mockPso2.Setup(pso2 => pso2.Start()).Raises(pso2 => pso2.Pso2LogEvent += null, msg);

            Pso2LogSender.LivetubeMain(clientFactory, pso2Factory, () =>
            {
                manualEvent.WaitOne();
                check = true;
            });

            Assert.True(check);
        }

        [Test]
        public void LivetubeMainTest2_BroadcastNotFound()
        {
            var clientFactory = new MockLivetubeClientFactory();
            var mockClient = clientFactory.MockLivetubeClient;
            var pso2Factory = new MockPso2LogWatcherFactory();
            var mockPso2 = pso2Factory.MockPso2LogWatcher;

            var manualEvent = new ManualResetEvent(false);
            var check = true;

            mockClient.Setup(cl => cl.FindCurrentBroadcasting(It.IsAny<string>())).Returns((String)null);
            mockClient.Setup(cl => cl.FindStream(It.IsAny<string>())).Returns("stream_id");
            mockClient.Setup(cl => cl.PostComment(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Callback<string, string, string>((stream, user, comment) =>
                {
                    check = false;
                    manualEvent.Set();
                    Assert.AreEqual("stream_id", stream);
                    Assert.AreEqual("someone", user);
                    Assert.AreEqual("message", comment);
                });

            var msg = new Pso2LogEventArgs();
            msg.From = "someone";
            msg.SendTo = "GUILD";
            msg.Message = "message";
            mockPso2.Setup(pso2 => pso2.Start()).Raises(pso2 => pso2.Pso2LogEvent += null, msg);

            var ex = Assert.Throws<ApplicationException>(() =>
            {
                Pso2LogSender.LivetubeMain(clientFactory, pso2Factory, () =>
                {
                    check = false;
                    manualEvent.WaitOne();
                });
            });
            Assert.AreEqual("現配信URLの取得に失敗しました", ex.Message);

            Assert.True(check);
        }

        [Test]
        public void LivetubeMainTest3_StreamIdNotFound()
        {
            var clientFactory = new MockLivetubeClientFactory();
            var mockClient = clientFactory.MockLivetubeClient;
            var pso2Factory = new MockPso2LogWatcherFactory();
            var mockPso2 = pso2Factory.MockPso2LogWatcher;

            var manualEvent = new ManualResetEvent(false);
            var check = true;

            mockClient.Setup(cl => cl.FindCurrentBroadcasting(It.IsAny<string>())).Returns("http://livetube.cc/someone/sometitle");
            mockClient.Setup(cl => cl.FindStream(It.IsAny<string>())).Returns((String)null);
            mockClient.Setup(cl => cl.PostComment(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Callback<string, string, string>((stream, user, comment) =>
                {
                    check = false;
                    manualEvent.Set();
                    Assert.AreEqual("stream_id", stream);
                    Assert.AreEqual("someone", user);
                    Assert.AreEqual("message", comment);
                });

            var msg = new Pso2LogEventArgs();
            msg.From = "someone";
            msg.SendTo = "GUILD";
            msg.Message = "message";
            mockPso2.Setup(pso2 => pso2.Start()).Raises(pso2 => pso2.Pso2LogEvent += null, msg);

            var ex = Assert.Throws<ApplicationException>(() =>
            {
                Pso2LogSender.LivetubeMain(clientFactory, pso2Factory, () =>
                {
                    check = false;
                    manualEvent.WaitOne();
                });
            });
            Assert.AreEqual("ストリームの取得に失敗しました", ex.Message);

            Assert.True(check);
        }
    }
}
