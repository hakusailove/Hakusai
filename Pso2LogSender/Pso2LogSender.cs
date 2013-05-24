using NLog;
using System;
using System.Configuration;
using Hakusai.Cavetube;
using CaveTube.CaveTubeClient;
using Hakusai.Pso2;
using Hakusai.Livetube;
using System.Reflection;
using System.IO;

namespace Hakusai.Pso2LogSender
{
    /// <summary>
    /// PSO2のログの内容を監視して配信コメントとして送信するプログラム
    /// </summary>
    /// <remarks>
    /// <para>コマンドライン</para>
    /// <para>Pso2LogSender.exe [-live|-help]</para>
    /// <list type="table">
    /// <listheader><term>オプション</term><description>説明</description></listheader>
    /// <item><term>オプションなし</term><description>Cavetubeにコメントする</description></item>
    /// <item><term>-live</term><description>Livetubeにコメントする</description></item>
    /// <item><term>-help</term><description>簡単なオプションの説明が表示される</description></item>
    /// </list>
    /// <para>現状の実装では</para>
    /// <list type="bullet">
    /// <item><description>設定ファイル(Pso2LogSender.exe.config)のuserが現在配信中のストリームに
    /// コメントが流れる</description></item>
    /// <item><description>設定ファイル(Pso2LogSender.exe.config)のuserとpasswordを使って配信サイトに
    /// ログインする</description></item>
    /// <item><description>PSO2のチームチャットのみがそのまま配信コメントに流れる(ソロチーム仕様)
    /// </description></item>
    /// <item><description>ソロチームでない場合誰のチャットでもその名前でコメントに流れる
    /// </description></item>
    /// <item><description>設定ファイル(Pso2LogSender.exe.config)でpso2_userに書かれた名前は
    /// userに書かれた名前に変換される(認証コメント用)</description></item>
    /// </list>
    /// </remarks>
    [System.Runtime.CompilerServices.CompilerGeneratedAttribute]
    class NamespaceDoc
    {
    }

    /// <summary>
    /// PSO2のログの内容を監視して配信コメントとして送信するプログラム
    /// </summary>
    /// <remarks>名前空間<see cref="Hakusai.Pso2LogSender"/>の説明参照
    /// </remarks>
    public class Pso2LogSender
    {
        private static Logger _logger = LogManager.GetLogger("Pso2LogSender");
        private static readonly String PSO2USER = ConfigurationManager.AppSettings["pso2_user"] ?? string.Empty;
        private static readonly String USER = ConfigurationManager.AppSettings["user"] ?? string.Empty;
        private static readonly String PASSWORD = ConfigurationManager.AppSettings["password"] ?? string.Empty;

        private static void WaitConsole()
        {
            Console.WriteLine("Press \'q\' to quit.");
            while (Console.Read() != 'q') ;
        }

        private static void LogWatch(
            Action<string, string> postAction,
            Action waitAction,
            IPso2LogWatcherFactory pso2Factory)
        {
            using (IPso2LogWatcher watcher = pso2Factory.CreatePso2LogWatcher())
            {
                watcher.Pso2LogEvent += (sender, e) =>
                {
                    if (e.SendTo == "GUILD")
                    {
                        _logger.Debug("{0}: <{1}>{2}", e.From, e.SendTo, e.Message);
                        string person = e.From;
                        if (person == PSO2USER) person = USER;
                        postAction(person, e.Message);
                    }
                };
                watcher.Start();
                waitAction();
            }
        }

        /// <summary>
        /// Cavetube用Main
        /// </summary>
        public static void CavetubeMain()
        {
            CavetubeMain(
                new CavetubeAuthFactory(),
                new CavetubeClientFactory(),
                new Pso2LogWatcherFactory(),
                WaitConsole);
        }

        /// <summary>
        /// Cavetube用Main(テスト用に差し替え可能な形)
        /// </summary>
        /// <param name="authFactory">使用するICavetubeAuthのインスタンスを選択</param>
        /// <param name="clientFactory">使用するICavetubeCelientのインスタンスを選択</param>
        /// <param name="pso2Factory">使用するIPso2LogWatcherのインスタンスを選択</param>
        /// <param name="waitAction">プログラムの終了を待つためのActionを選択</param>
        public static void CavetubeMain(
            ICavetubeAuthFactory authFactory,
            ICavetubeClientFactory clientFactory,
            IPso2LogWatcherFactory pso2Factory,
            Action waitAction)
        {
            using (CavetubeAuthManager manager = new CavetubeAuthManager(USER, PASSWORD, authFactory))
            using (var client = clientFactory.CreateCavetubeClient())
            {
                client.Connect();

                // 部屋名取得はapi経由でいいのだろうか？
                // 特定ユーザーの配信をチェックするのはコメントサーバからできるかも
                String url = new Uri(String.Format("http://gae.cavelis.net/live/{0}", USER)).AbsoluteUri;
                ISummary summary = client.GetSummary(url);
                if (summary.RoomId == null)
                    throw new ApplicationException("部屋IDの取得に失敗しました");

                CavetubeUtility.Joinning(client, summary.RoomId, () =>
                {
                    _logger.Debug("Joinした");
                    _logger.Debug("API Key: {0}", manager.GetApiKey());

                    LogWatch(
                        (String user, String message) =>
                        {
                            client.PostComment(user, message, manager.GetApiKey());
                        },
                        waitAction,
                        pso2Factory
                    );
                });
            }
        }

        /// <summary>
        /// Livetube用Main
        /// </summary>
        public static void LivetubeMain()
        {
            LivetubeMain(new LivetubeClientFactory(), new Pso2LogWatcherFactory(), WaitConsole);
        }

        /// <summary>
        /// Livetube用Main(テスト用に差し替え可能な形)
        /// </summary>
        /// <param name="clientFactory">使用するILivetubeClientのインスタンスを選択</param>
        /// <param name="pso2Factory">使用するIPso2LogWatcherのインスタンスを選択</param>
        /// <param name="waitAction">プログラムの終了を待つためのActionを選択</param>
        public static void LivetubeMain(
            ILivetubeClientFactory clientFactory,
            IPso2LogWatcherFactory pso2Factory,
            Action waitAction)
        {
            using (ILivetubeClient livetube = clientFactory.CreateLivetubeClient())
            {
                string userurl = new Uri(String.Format("http://livetube.cc/{0}/", USER)).AbsoluteUri;
                string broadcastUrl = livetube.FindCurrentBroadcasting(userurl);
                if (broadcastUrl == null)
                    throw new ApplicationException("現配信URLの取得に失敗しました");
                string stream = livetube.FindStream(broadcastUrl);
                if (stream == null)
                    throw new ApplicationException("ストリームの取得に失敗しました");

                livetube.Login(USER, PASSWORD);

                LogWatch(
                    (string user, string message) =>
                    {
                        livetube.PostComment(stream, user, message);
                    },
                    waitAction,
                    pso2Factory
                );

                livetube.Logoff();
            }
        }

        static void Main(string[] args)
        {
            try
            {
                if ((args.Length > 0) && (args[0] == "-live"))
                {
                    LivetubeMain();
                }
                else if ((args.Length > 0) && (args[0] == "-help"))
                {
                    Console.WriteLine("{0} [-live]\n\t(no args):\tCavetube\n\t-live:\t\tLivetube",
                             Path.GetFileName(Assembly.GetEntryAssembly().Location));
                }
                else
                {
                    CavetubeMain();
                }
            }
            catch (Exception e)
            {
                _logger.Error("予期しないエラーで終了しました\n" + e.ToString());
            }
        }
    }
}