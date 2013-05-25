using CaveTube.CaveTubeClient;
using System;
using System.Threading;

namespace CavetubeCommentReaderSample
{
    /// <summary>
    /// Cavetubeからコメントを読んで表示するだけのサンプルプログラムを格納してます。
    /// </summary>
    /// <remarks>
    /// そのうち削除予定。
    /// </remarks>
    [System.Runtime.CompilerServices.CompilerGeneratedAttribute]
    class NamespaceDoc
    {
    }

    /// <summary>
    /// Cavetubeからコメントを読んで表示するだけのサンプルプログラム
    /// </summary>
    /// <remarks>
    /// 現在のはくさいライブラリの機能では実現できないので
    /// CavetubeClientライブラリを直に呼んで表示させています。
    /// そのうち削除予定。
    /// </remarks>
    public class Program
    {
        static void Main(string[] args)
        {
            var uri = new Uri("http://gae.cavelis.net/放送");
            using (var _client = new CavetubeClient())
            {
                using (var e = new ManualResetEvent(false))
                {
                    _client.Connect();
                    _client.OnJoin += (s) =>
                    {
                        e.Set();
                    };
                    _client.JoinRoom(_client.GetSummary(uri.AbsoluteUri).RoomId);
                    e.WaitOne();
                }
                _client.OnNewMessage += (message) =>
                {
                    Console.WriteLine("{0}: {1}", message.Name, message.Comment);
                };
                Console.ReadLine();
            }
        }
    }
}
