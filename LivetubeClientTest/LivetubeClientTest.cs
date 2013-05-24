using System;
using NUnit.Framework;
using System.Net;
using Moq;
using System.Text;
using System.Reflection;

namespace Hakusai.Livetube.Test
{
    public class LivetubeClientTest
    {
        public sealed class MockWebClientFactory : IWebClientFactory
        {
            public void Dispose()
            {}

            public IWebClient CreateWebClient()
            {
                return MockClient.Object;
            }

            public Mock<IWebClient> MockClient = new Mock<IWebClient>();
        }

        [Test]
        public void DefaultConstructorTest()
        {
            var livetube = new LivetubeClient();
            FieldInfo  field = livetube.GetType().GetField("_webClient", BindingFlags.GetField | BindingFlags.SetField | BindingFlags.NonPublic | BindingFlags.Instance);
            Object o = field.GetValue(livetube);
            Type t = o.GetType();
            Assert.AreEqual(typeof(WebClientWrapper), field.GetValue(livetube).GetType());

            livetube.Dispose();
            // 通常DefaultInstanceで生成されるためDispose()されない。とりあえずここで試す。
            new WebClientWrapperFactory().Dispose();
        }

        [Test]
        public void LoginTest()
        {
            // モックの設定
            var factory = new MockWebClientFactory();
            var mock = factory.MockClient;
            WebHeaderCollection reqHead = new WebHeaderCollection();
            mock.Setup(client => client.Headers).Returns(reqHead);
            WebHeaderCollection resHead = new WebHeaderCollection();
            resHead.Add("Set-Cookie", "SomeCookie");
            mock.Setup(client => client.ResponseHeaders).Returns(resHead);

            // 実際の呼び出し
            LivetubeClient livetube = new LivetubeClient(factory);
            Assert.True(livetube.Login("hakusai", "password"));

            // 結果の確認
            Assert.AreEqual("application/x-www-form-urlencoded", reqHead["Content-Type"]);
            byte[] bytes = Encoding.ASCII.GetBytes("user=hakusai&password=password");
            mock.Verify(client => client.UploadData("http://livetube.cc/login", "POST", bytes));

            livetube.Dispose();
        }

        [Test]
        public void LoginTest2_twice()
        {
            // モックの設定
            var factory = new MockWebClientFactory();
            var mock = factory.MockClient;
            WebHeaderCollection reqHead = new WebHeaderCollection();
            mock.Setup(client => client.Headers).Returns(reqHead);
            WebHeaderCollection resHead = new WebHeaderCollection();
            resHead.Add("Set-Cookie", "SomeCookie");
            mock.Setup(client => client.ResponseHeaders).Returns(resHead);

            // 実際の呼び出し
            LivetubeClient livetube = new LivetubeClient(factory);
            Assert.True(livetube.Login("hakusai", "password"));
            reqHead.Clear();
            resHead.Add("Set-Cookie", "SomeCookie");
            Assert.True(livetube.Login("hakusai", "password"));

            // 結果の確認
            Assert.AreEqual("application/x-www-form-urlencoded", reqHead["Content-Type"]);
            byte[] bytes = Encoding.ASCII.GetBytes("user=hakusai&password=password");
            mock.Verify(client => client.UploadData("http://livetube.cc/login", "POST", bytes));
            mock.Verify(client => client.DownloadString("http://livetube.cc/logoff"));

            livetube.Dispose();
        }

        [Test]
        public void LogoffTest()
        {
            // モックの設定
            var factory = new MockWebClientFactory();
            var mock = factory.MockClient;
            WebHeaderCollection reqHead = new WebHeaderCollection();
            mock.Setup(client => client.Headers).Returns(reqHead);
            WebHeaderCollection resHead = new WebHeaderCollection();
            resHead.Add("Set-Cookie", "SomeCookie");
            mock.Setup(client => client.ResponseHeaders).Returns(resHead);

            // 実際の呼び出し
            LivetubeClient livetube = new LivetubeClient(factory);
            Assert.True(livetube.Login("hakusai", "password"));

            reqHead.Clear();
            resHead.Clear();
            livetube.Logoff();

            // 結果の確認
            Assert.AreEqual("SomeCookie", reqHead["Cookie"]);
            mock.Verify(client => client.DownloadString("http://livetube.cc/logoff"));

            livetube.Dispose();
        }

        [Test]
        public void FindCurrentBroadcastingTest()
        {
            // モックの設定
            var factory = new MockWebClientFactory();
            var mock = factory.MockClient;
            mock.Setup(client => client.DownloadString("http://livetube.cc/hakusai/")).Returns("<html>\r\n<a href=\"/hakusai/sometitle\"><img src=\"/images/loading.gif\">実行中</a></html>\r\n");
            WebHeaderCollection reqHead = new WebHeaderCollection();
            mock.Setup(client => client.Headers).Returns(reqHead);
            WebHeaderCollection resHead = new WebHeaderCollection();
            mock.Setup(client => client.ResponseHeaders).Returns(resHead);

            // 実際の呼び出し
            LivetubeClient livetube = new LivetubeClient(factory);
            Assert.AreEqual("http://livetube.cc/hakusai/sometitle", livetube.FindCurrentBroadcasting("http://livetube.cc/hakusai/"));
        }

        [Test]
        public void FindStreamTest()
        {
            // モックの設定
            var factory = new MockWebClientFactory();
            var mock = factory.MockClient;
            mock.Setup(client => client.DownloadString("http://livetube.cc/hakusai/somesbroadcast")).Returns("<html>\r\nvar comment_entry_id = \"somestreamid\";\r\n</html>\r\n");
            WebHeaderCollection reqHead = new WebHeaderCollection();
            mock.Setup(client => client.Headers).Returns(reqHead);
            WebHeaderCollection resHead = new WebHeaderCollection();
            resHead.Add("Set-Cookie", "SomeCookie");
            mock.Setup(client => client.ResponseHeaders).Returns(resHead);

            // 実際の呼び出し
            LivetubeClient livetube = new LivetubeClient(factory);
            Assert.True(livetube.Login("hakusai", "password"));

            reqHead.Clear();
            resHead.Clear();
            Assert.AreEqual("somestreamid", livetube.FindStream("http://livetube.cc/hakusai/somesbroadcast"));

            reqHead.Clear();
            resHead.Clear();
            livetube.Logoff();

            // 結果の確認
            Assert.AreEqual("SomeCookie", reqHead["Cookie"]);
            mock.Verify(client => client.DownloadString("http://livetube.cc/logoff"));

            livetube.Dispose();
        }

        [Test]
        public void PostCommentTest()
        {
            // モックの設定
            var factory = new MockWebClientFactory();
            var mock = factory.MockClient;
            mock.Setup(client => client.DownloadString("http://livetube.cc/hakusai/somesbroadcast")).Returns("<html>\r\nvar comment_entry_id = \"somestreamid\";\r\n</html>\r\n");
            WebHeaderCollection reqHead = new WebHeaderCollection();
            mock.Setup(client => client.Headers).Returns(reqHead);
            WebHeaderCollection resHead = new WebHeaderCollection();
            resHead.Add("Set-Cookie", "SomeCookie");
            mock.Setup(client => client.ResponseHeaders).Returns(resHead);

            // 実際の呼び出し
            LivetubeClient livetube = new LivetubeClient(factory);
            Assert.True(livetube.Login("hakusai", "password"));

            reqHead.Clear();
            resHead.Clear();
            var streamid = livetube.FindStream("http://livetube.cc/hakusai/somesbroadcast");

            reqHead.Clear();
            resHead.Clear();
            livetube.PostComment(streamid, "hakusai", "コメント");

            // 結果の確認
            Assert.AreEqual("application/x-www-form-urlencoded", reqHead["Content-Type"]);
            Assert.AreEqual("SomeCookie", reqHead["Cookie"]);
            byte[] bytes = Encoding.ASCII.GetBytes("name=hakusai&c=%E3%82%B3%E3%83%A1%E3%83%B3%E3%83%88");
            mock.Verify(client => client.UploadData("http://livetube.cc/stream/somestreamid.comments", "POST", bytes));

            reqHead.Clear();
            resHead.Clear();
            livetube.Logoff();

            livetube.Dispose();
        }
    }

    public class LivetubeClientFactoryTest
    {
        [Test]
        public void ConstructorTest()
        {
            using (var factory = new LivetubeClientFactory())
            {
                var livetube = factory.CreateLivetubeClient();
                var m = livetube.GetType().GetField("_webClient", BindingFlags.GetField | BindingFlags.NonPublic | BindingFlags.Instance);
                var t = m.GetValue(livetube).GetType();
                Assert.AreEqual(typeof(WebClientWrapper), t);
            }
        }

        [Test]
        public void ConstructorTest2()
        {
            var mockFactory = new Mock<IWebClientFactory>();
            var mockWebClient = new Mock<IWebClient>();
            mockFactory.Setup(f => f.CreateWebClient()).Returns(mockWebClient.Object);

            using (var factory = new LivetubeClientFactory(mockFactory.Object))
            {
                var livetube = factory.CreateLivetubeClient();
                var m = livetube.GetType().GetField("_webClient", BindingFlags.GetField | BindingFlags.NonPublic | BindingFlags.Instance);
                var t = m.GetValue(livetube).GetType();
                Assert.AreEqual(mockWebClient.Object.GetType(), t);
            }
        }
    }
}
