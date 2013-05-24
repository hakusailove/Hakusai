using CaveTube.CaveTubeClient;
using System;
using System.Threading;

namespace Hakusai.Cavetube
{
    /// <summary>
    /// Cavetube関連の名前空間
    /// </summary>
    /// <remarks>
    /// CavetuveLibrary本体を使うにあたりなんとなく用意したクラス。
    /// </remarks>
    /// <example><code>
    /// using (ICavetubeAuthManager auth = new CavetubeAuthManager(user, password))
    /// {
    ///     ...
    ///     client.PostComment(user, message, auth.GetApiKey()); // 初回はログインして取得
    ///     ...
    ///     client.PostComment(user, message, auth.GetApiKey()); // 2回目以降は保持しているAPIキー
    /// } // ここでログアウト
    /// </code></example>
    [System.Runtime.CompilerServices.CompilerGeneratedAttribute]
    class NamespaceDoc { }

    /// <summary>
    /// CavetubeAuthのinterface(staticだったけど無理矢理ラップ)
    /// </summary>
    public interface ICavetubeAuth
    {
        /// <summary>
        /// 指定されたユーザーを指定されたパスワードで認証しログインする
        /// </summary>
        /// <remarks>
        /// 実際には設定ファイルの開発キーも使用される(はず)
        /// </remarks>
        /// <param name="userId">認証するユーザー</param>
        /// <param name="password">パスワード</param>
        /// <returns>APIキー</returns>
        string Login(string userId, string password);

        /// <summary>
        /// 指定されたユーザーを指定されたパスワードで認証しログアウトする
        /// </summary>
        /// <remarks>
        /// 実際には設定ファイルの開発キーも使用される(かも)。
        /// なんにしてもなんでログアウトに認証が必要なのか不明。
        /// </remarks>
        /// <param name="userId">認証するユーザー</param>
        /// <param name="password">パスワード</param>
        /// <returns>成功したら真</returns>
        bool Logout(string userId, string password);
    }

    /// <summary>
    /// <see cref="ICavetubeAuth"/>実装クラスのインスタンスを生成するファクトリクラスのインターフェース
    /// </summary>
    public interface ICavetubeAuthFactory : IDisposable
    {
        /// <summary>
        /// <see cref="ICavetubeAuth"/>実装クラスのインスタンスを生成して返す
        /// </summary>
        /// <returns><see cref="ICavetubeAuth"/>実装クラスのインスタンス</returns>
        ICavetubeAuth CreateCavetubeAuth();
    }

    /// <summary>
    /// <see cref="ICavetubeAuth"/>を実装したCavetubeAuthのラッパークラス
    /// </summary>
    public class CavetubeAuthWrapper : ICavetubeAuth
    {
        /// <summary>
        /// <see cref="ICavetubeAuth.Login(string, string)"/>の説明参照
        /// </summary>
        /// <param name="userId"><see cref="ICavetubeAuth.Login(string, string)"/>の説明参照</param>
        /// <param name="password"><see cref="ICavetubeAuth.Login(string, string)"/>の説明参照</param>
        /// <returns><see cref="ICavetubeAuth.Login(string, string)"/>の説明参照</returns>
        public string Login(string userId, string password)
        {
            return CavetubeAuth.Login(userId, password);
        }

        /// <summary>
        /// <see cref="ICavetubeAuth.Logout(string, string)"/>の説明参照
        /// </summary>
        /// <param name="userId"><see cref="ICavetubeAuth.Logout(string, string)"/>の説明参照</param>
        /// <param name="password"><see cref="ICavetubeAuth.Logout(string, string)"/>の説明参照</param>
        /// <returns><see cref="ICavetubeAuth.Logout(string, string)"/>の説明参照</returns>
        public bool Logout(string userId, string password)
        {
            return CavetubeAuth.Logout(userId, password);
        }
    }

    /// <summary>
    /// <see cref="CavetubeAuthWrapper"/>のインスタンスを生成する<see cref="ICavetubeAuthFactory"/>を実装したクラス
    /// </summary>
    public sealed class CavetubeAuthFactory : ICavetubeAuthFactory
    {
        private static CavetubeAuthFactory _DefaultInstance = new CavetubeAuthFactory();

        /// <summary>
        /// デフォルトのstaticなインスタンス
        /// </summary>
        /// <remarks>別クラスのデフォルトコンストラクタで無条件にこのクラスのインスタンスが必要になるため用意した</remarks>
        public static CavetubeAuthFactory DefaultInstance
        {
            get { return _DefaultInstance; }
        }

        /// <summary>
        /// <see cref="CavetubeAuthWrapper"/>のインスタンスを生成して返す
        /// </summary>
        /// <returns><see cref="CavetubeAuthWrapper"/>のインスタンス</returns>
        public ICavetubeAuth CreateCavetubeAuth()
        {
            return new CavetubeAuthWrapper();
        }

        /// <summary>
        /// <see cref="IDisposable.Dispose"/>参照
        /// </summary>
        public void Dispose()
        { }
    }

    /// <summary>
    /// ログインしたらusingでログアウトできるような管理クラスのinterface
    /// </summary>
    /// <remarks><para>CavetubeAuthはユーティリティ的なクラスとして実装されてるようで、
    /// そのままではログインしたら必ずログアウトするといった作りにしくい。
    /// それを自動でするような簡単な仕組みを用意したということ。</para>
    /// <para>このinterfaceの対応factoryは固有の認証方法を認めないことになるので作らない。</para>
    /// </remarks>
    public interface ICavetubeAuthManager : IDisposable
    {
        /// <summary>
        /// APIキーを取得する
        /// </summary>
        /// <remarks>必要ならログインする</remarks>
        /// <returns>APIキー</returns>
        String GetApiKey();
    }

    /// <summary>
    /// <see cref="ICavetubeAuthManager"/>の実装クラス
    /// </summary>
    /// <remarks><see cref="ICavetubeAuthManager"/>の説明参照</remarks>
    public class CavetubeAuthManager : ICavetubeAuthManager
    {
        private String _user = null;
        private String _pass = null;
        private String _apikey = null;
        private ICavetubeAuth _auth = null;

        /// <summary>
        /// コンストラクタ
        /// この時点ではログインはしない
        /// </summary>
        /// <param name="user">ユーザー名</param>
        /// <param name="password">パスワード</param>
        public CavetubeAuthManager(String user, String password)
            : this(user, password, CavetubeAuthFactory.DefaultInstance)
        { }

        /// <summary>
        /// テスト用コンストラクタ
        /// </summary>
        /// <param name="user">ユーザー名</param>
        /// <param name="password">パスワード</param>
        /// <param name="factory">認証に必要なオブジェクトのファクトリオブジェクト</param>
        public CavetubeAuthManager(String user, String password, ICavetubeAuthFactory factory)
        {
            _user = user;
            _pass = password;
            _auth = factory.CreateCavetubeAuth();
        }

        /// <summary>
        /// <see cref="ICavetubeAuthManager.GetApiKey()"/>の実装
        /// </summary>
        /// <remarks><see cref="ICavetubeAuthManager.GetApiKey()"/>参照</remarks>
        /// <returns><see cref="ICavetubeAuthManager.GetApiKey()"/>参照</returns>
        public String GetApiKey()
        {
            if (_apikey == null)
            {
                _apikey = _auth.Login(_user, _pass);
            }
            return _apikey;
        }

        /// <summary>
        /// <see cref="IDisposable.Dispose()"/>の実装
        /// </summary>
        /// <remarks>ログインしてたらログアウトする</remarks>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private bool disposed = false;

        /// <summary>
        /// 典型的なDisposeの実装
        /// </summary>
        /// <param name="disposing"><see cref="Dispose()"/>中なら真</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    if (_apikey != null)
                    {
                        _auth.Logout(_user, _pass);
                        _apikey = null;
                    }
                }
                disposed = true;
            }
        }

        /// <summary>
        /// デストラクタ
        /// </summary>
        ~CavetubeAuthManager()
        {
            Dispose(false);
        }
    }

    /// <summary>
    /// CaveTube.CaveTubeClient.Summaryを元に必要なアクセスだけ定義したインターフェース
    /// </summary>
    public interface ISummary
    {
        /// <summary>
        /// 部屋ID
        /// </summary>
        String RoomId { get; set; }
    }

    /// <summary>
    /// <see cref="ISummary"/>の実装クラス
    /// </summary>
    /// <remarks>CaveTube.CaveTubeClient.Summaryのラッパー</remarks>
    public class SummaryWrapper : ISummary
    {
        private Summary _summary;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <remarks>空のSummaryWrapperを作成</remarks>
        public SummaryWrapper() { }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <remarks>ラップする対象を持つコンストラクタ</remarks>
        /// <param name="summary">CaveTube.CaveTubeClient.Summaryのインスタンス</param>
        public SummaryWrapper(Summary summary) { _summary = summary; }

        /// <summary>
        /// 本体のRoomIDのsetterがinternalなため
        /// </summary>
        private String _dummyRoomID;
        
        /// <summary>
        /// 部屋ID
        /// </summary>
        /// <remarks>Summaryインスタンスをラップしている場合は書き込み不可です</remarks>
        public String RoomId
        {
            get
            {
                if (_summary != null)
                {
                    return _summary.RoomId;
                }
                else
                {
                    return _dummyRoomID;
                }
            }
            set
            {
                if (_summary == null)
                {
                    _dummyRoomID = value;
                }
                else
                {
                    throw new NotSupportedException();
                }
            }
        }
    }

    /// <summary>
    /// CaveTube.CaveTubeClient.CavetubeClientを元に必要なアクセスだけ定義したインターフェース
    /// </summary>
    public interface ICavetubeClient : IDisposable
    {
        /// <summary>
        /// Joinしたときに発生するイベント
        /// </summary>
        /// <remarks>Actionの引数は部屋ID</remarks>
        event Action<string> OnJoin;

        /// <summary>
        /// Leaveしたときに発生するイベント
        /// </summary>
        /// <remarks>Actionの引数は部屋ID</remarks>
        event Action<string> OnLeave;

        /// <summary>
        /// 指定した放送中URLの部屋にJoinする
        /// </summary>
        /// <param name="liveUrl">放送中URL</param>
        void JoinRoom(string liveUrl);

        /// <summary>
        /// Joinした部屋からLeaveする
        /// </summary>
        void LeaveRoom();

        /// <summary>
        /// コメントサーバーに接続する
        /// </summary>
        void Connect();

        /// <summary>
        /// (Web APIを使って)指定した放送中URLのサマリを取得する
        /// </summary>
        /// <param name="liveUrl">放送中URL</param>
        /// <returns>放送中URLのサマリ</returns>
        ISummary GetSummary(string liveUrl);

        /// <summary>
        /// コメントをポストする
        /// </summary>
        /// <remarks>APIキーを指定すると認証コメントになる</remarks>
        /// <param name="name">ユーザー名</param>
        /// <param name="message">メッセージ</param>
        /// <param name="apiKey">APIキー</param>
        void PostComment(string name, string message, string apiKey = "");
    }

    /// <summary>
    /// <see cref="ICavetubeClient"/>のインスタンスを生成するファクトリインターフェース
    /// </summary>
    public interface ICavetubeClientFactory : IDisposable
    {
        /// <summary>
        /// <see cref="ICavetubeClient"/>のインスタンスを生成する
        /// </summary>
        /// <returns><see cref="ICavetubeClient"/>のインスタンス</returns>
        ICavetubeClient CreateCavetubeClient();
    }

    /// <summary>
    /// <see cref="ICavetubeClient"/>を実装するCaveTube.CaveTubeClient.CavetubeClientのラッパー
    /// </summary>
    public class CavetubeClientWrapper : ICavetubeClient
    {
        // CavetubeClientがなぜかsealedだったので継承できずに手書きwrapper最悪
        private CavetubeClient _client = new CavetubeClient();

        /// <summary>
        /// <see cref="ICavetubeClient.OnJoin"/>の説明参照
        /// </summary>
        public event Action<string> OnJoin;

        /// <summary>
        /// <see cref="ICavetubeClient.OnLeave"/>の説明参照
        /// </summary>
        public event Action<string> OnLeave;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public CavetubeClientWrapper()
        {
            _client.OnJoin += (e) =>
            {
                OnJoin(e);
            };

            _client.OnLeave += (e) =>
            {
                OnLeave(e);
            };
        }

        /// <summary>
        /// <see cref="ICavetubeClient.JoinRoom(string)"/>の説明参照
        /// </summary>
        /// <param name="liveUrl"><see cref="ICavetubeClient.JoinRoom(string)"/>の説明参照</param>
        public void JoinRoom(string liveUrl)
        {
            _client.JoinRoom(liveUrl);
        }

        /// <summary>
        /// <see cref="ICavetubeClient.LeaveRoom()"/>の説明参照
        /// </summary>
        public void LeaveRoom()
        {
            _client.LeaveRoom();
        }

        /// <summary>
        /// <see cref="ICavetubeClient.Connect()"/>の説明参照
        /// </summary>
        public void Connect()
        {
            _client.Connect();
        }

        /// <summary>
        /// <see cref="IDisposable.Dispose()"/>の説明参照
        /// </summary>
        public void Dispose()
        {
            _client.Dispose();
        }

        /// <summary>
        /// <see cref="ICavetubeClient.GetSummary(string)"/>の説明参照
        /// </summary>
        /// <param name="liveUrl"><see cref="ICavetubeClient.GetSummary(string)"/>の説明参照</param>
        /// <returns><see cref="ICavetubeClient.GetSummary(string)"/>の説明参照</returns>
        public ISummary GetSummary(string liveUrl)
        {
            return new SummaryWrapper(_client.GetSummary(liveUrl));
        }

        /// <summary>
        /// <see cref="ICavetubeClient.PostComment(string, string, string)"/>の説明参照
        /// </summary>
        /// <param name="name"><see cref="ICavetubeClient.PostComment(string, string, string)"/>の説明参照</param>
        /// <param name="message"><see cref="ICavetubeClient.PostComment(string, string, string)"/>の説明参照</param>
        /// <param name="apiKey"><see cref="ICavetubeClient.PostComment(string, string, string)"/>の説明参照</param>
        public void PostComment(string name, string message, string apiKey = "")
        {
            _client.PostComment(name, message, apiKey);
        }
    }

    /// <summary>
    /// <see cref="CavetubeClientWrapper"/>のインスタンスを生成する<see cref="ICavetubeClientFactory"/>を実装するクラス
    /// </summary>
    public class CavetubeClientFactory : ICavetubeClientFactory
    {
        /// <summary>
        /// <see cref="ICavetubeClientFactory"/>の説明参照
        /// </summary>
        /// <returns><see cref="ICavetubeClientFactory"/>の説明参照</returns>
        public ICavetubeClient CreateCavetubeClient()
        {
            return new CavetubeClientWrapper();
        }

        /// <summary>
        /// <see cref="IDisposable.Dispose()"/>の説明参照
        /// </summary>
        public void Dispose()
        {}
    }

    /// <summary>
    /// 実装を括っただけのユーティリティクラス
    /// </summary>
    public class CavetubeUtility
    {
        /// <summary>
        /// 指定された部屋にJoinして指定されたActionを行ってからLeaveする
        /// </summary>
        /// <param name="client">ICavetubeClientのインスタンス</param>
        /// <param name="roomId">Joinする部屋のID</param>
        /// <param name="action">実施するアクション</param>
        public static void Joinning(ICavetubeClient client, String roomId, Action action)
        {
            // Joinは待たないと部屋名が設定されうちにPostしようとして失敗するので待つ
            using (ManualResetEvent sig = new ManualResetEvent(false))
            {
                client.OnJoin += s =>
                {
                    sig.Set();
                };
                client.JoinRoom(roomId);
                sig.WaitOne();
            }

            action();

            // Leaveも待たないとぶち切れそうなので待つ
            using (ManualResetEvent sig = new ManualResetEvent(false))
            {
                client.OnLeave += s =>
                {
                    sig.Set();
                };
                client.LeaveRoom();
                sig.WaitOne();
            }
        }
    }
}