using System;
using System.Collections.Specialized;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace Hakusai.Livetube
{
    /// <summary>
    /// Livetube関連の名前空間
    /// </summary>
    /// <remarks>
    /// <para>LivetubeClientの使い方を例で示します。</para>
    /// <para>プロジェクト設定でLivetubeClient.dllへの参照追加が必要です</para>
    /// </remarks>
    /// <example><code>
    /// // 以下ではユーザーhakusaiのページから“現在放送中”のURLを探し出し、そのストリームにコメントしています
    /// // URL・ユーザー名・パスワードは自分のものをお使いください。
    /// 
    /// using (var factory = new Hakusai.Livetube.LivetubeClientFactory())
    /// using (var livetube = factory.CreateLivetubeClient())
    /// {
    ///     var user_url = "http://livetube.cc/hakusai/";                   // ユーザーページのURL
    ///     var broadcast_url = livetube.FindCurrentBroadcasting(user_url); // 放送中のURLを探す
    ///     var stream_id = livetube.FindStream(broadcast_url);             // ストリームのIDを探す
    ///
    ///     livetube.PostComment(stream_id, "hakusai", "テストコメント"); // コメントする(匿名コメント)
    ///     
    ///     livetube.Login("hakusai", "password");                        // ログインする
    ///     livetube.PostComment(stream_id, "hakusai", "テストコメント"); // コメントする(認証コメント)
    ///     livetube.Logoff();　　　　　　　　　　　　　　　　　　　　　 // ログオフする
    /// }
    /// </code></example>
    [System.Runtime.CompilerServices.CompilerGeneratedAttribute]
    class NamespaceDoc
    {
    }

    /// <summary>
    /// WebClientでこのライブラリ必要なメソッドのみ抽出したInterface(テスト用に作成)
    /// </summary>
    public interface IWebClient : IDisposable
    {
        /// <summary>
        /// <see cref="P:System.Net.WebClient.Headers"/>参照
        /// </summary>
        WebHeaderCollection Headers { get; set; }
        /// <summary>
        /// <see cref="P:System.Net.WebClient.ResponseHeaders"/>参照
        /// </summary>
        WebHeaderCollection ResponseHeaders { get; }
        /// <summary>
        /// <see cref="WebClient.UploadData(string, string, byte[])"/>参照
        /// </summary>
        /// <param name="address"><see cref="WebClient.UploadData(String, String, Byte[])"/>参照</param>
        /// <param name="method"><see cref="WebClient.UploadData(String, String, Byte[])"/>参照</param>
        /// <param name="data"><see cref="WebClient.UploadData(string, string, byte[])"/>参照</param>
        /// <returns><see cref="WebClient.UploadData(string, string, byte[])"/>参照</returns>
        byte[] UploadData(string address, string method, byte[] data);
        /// <summary>
        /// <see cref="System.Net.WebClient.DownloadString(string)"/>参照
        /// </summary>
        /// <param name="address"><see cref="System.Net.WebClient.DownloadString(string)"/>参照</param>
        /// <returns><see cref="System.Net.WebClient.DownloadString(string)"/>参照</returns>
        string DownloadString(string address);
    }

    /// <summary>
    /// WebClientをIWebClientを実装する形にラップしたもの(テスト用に作成)
    /// </summary>
    /// <remarks>実際中身は空でWebClientそのもの</remarks>
    public class WebClientWrapper : WebClient, IWebClient
    {
    }

    /// <summary>
    /// IWebClientの生成インターフェース
    /// </summary>
    /// <remarks>生成したオブジェクトの破棄はユーザー側の責任</remarks>
    public interface IWebClientFactory : IDisposable
    {
        /// <summary>
        /// IWebClientを実装したオブジェクトを返す
        /// </summary>
        /// <remarks>生成したオブジェクトの破棄はユーザー側の責任</remarks>
        /// <returns>生成したオブジェクト</returns>
        IWebClient CreateWebClient();
    }

    /// <summary>
    /// WebClientWrapperを生成する
    /// </summary>
    /// <remarks>encodingはデフォルトをANSIからUTF-8に変更しています</remarks>
    public class WebClientWrapperFactory : IWebClientFactory
    {
        private static WebClientWrapperFactory _defaultInstance = new WebClientWrapperFactory();
        /// <summary>
        /// デフォルトのインスタンス
        /// </summary>
        public static WebClientWrapperFactory DefaultInstance { get { return _defaultInstance; } }
        private Encoding _encoding;
        /// <summary>
        /// コンストラクタ(EncodingはUTF-8)
        /// </summary>
        public WebClientWrapperFactory() : this(Encoding.UTF8) { }
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="encoding">生成するWebClientWrapperのEncodingに設定する値</param>
        public WebClientWrapperFactory(Encoding encoding) { _encoding = encoding; }

        /// <summary>
        /// IWebClientを実装したオブジェクト(WebClientWrapper)を返す
        /// </summary>
        /// <remarks>生成したオブジェクトの破棄はユーザー側の責任</remarks>
        /// <returns>生成したオブジェクト</returns>
        public IWebClient CreateWebClient()
        {
            WebClientWrapper client = new WebClientWrapper();
            client.Encoding = _encoding;
            return client;
        }

        /// <summary>
        /// Dispose()の実装(このクラスでは何もしない)
        /// </summary>
        public void Dispose()
        {}
    }

    /// <summary>
    /// Libetubeのコメント操作を行うライブラリのインターフェース
    /// </summary>
    public interface ILivetubeClient : IDisposable
    {
        /// <summary>
        /// ログインする
        /// </summary>
        /// <param name="user">ユーザー名</param>
        /// <param name="password">パスワード</param>
        /// <returns>ログインに成功したら真</returns>
        bool Login(string user, string password);

        /// <summary>
        /// ログオフする
        /// </summary>
        void Logoff();

        /// <summary>
        /// コメントを送る(ログインしていてユーザー名が一致すれば認証コメントになる)
        /// </summary>
        /// <param name="stream">ストリーム</param>
        /// <param name="user">ユーザー名</param>
        /// <param name="comment">コメント</param>
        void PostComment(string stream, string user, string comment);

        /// <summary>
        /// 指定した配信URLからストリームを識別するIDを見つけて返す
        /// </summary>
        /// <param name="url">配信URL</param>
        /// <returns>ストリームを識別するID</returns>
        string FindStream(string url);

        /// <summary>
        /// 指定した配信者URLから現配信URLを見つけて返す
        /// </summary>
        /// <param name="userurl">配信者URL</param>
        /// <returns>現配信URL</returns>
        string FindCurrentBroadcasting(string userurl);
    }

    /// <summary>
    /// ILivetubeClientの生成インターフェース
    /// </summary>
    /// <remarks>生成したオブジェクトの破棄はユーザー側の責任</remarks>
    public interface ILivetubeClientFactory : IDisposable
    {
        /// <summary>
        /// ILivetubeClientを実装したオブジェクトを返す
        /// </summary>
        /// <remarks>生成したオブジェクトの破棄はユーザー側の責任</remarks>
        /// <returns>生成したオブジェクト</returns>
        ILivetubeClient CreateLivetubeClient();
    }

    /// <summary>
    /// Libetubeのコメント操作を行うライブラリ
    /// </summary>
    public class LivetubeClient : ILivetubeClient, IDisposable
    {
        private static readonly string SERVER = "livetube.cc";
        private static readonly string LOGIN_URL = string.Format("http://{0}/login", SERVER);
        private static readonly string LOGOFF_URL = string.Format("http://{0}/logoff", SERVER);
        private static readonly Regex STREAM_REGEX = new Regex("var comment_entry_id = \"([^\"]*)\";");
        private static readonly Regex BROADCAST_REGEX = new Regex("<a href=\"([^\"]*)\"><img src=\"/images/loading.gif\"[^>]*>実行中</a>");


        /// <summary>
        /// ログイン状態維持のため用意されたクッキー文字列の保存先
        /// </summary>
        protected string _cookie = null;
        /// <summary>
        /// 接続維持のため用意されたWebClientの格納先
        /// </summary>
        private IWebClient _webClient = null;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <remarks>WebClientWrapperFactoryを使用するデフォルトのコンストラクタ</remarks>
        public LivetubeClient()
            : this(WebClientWrapperFactory.DefaultInstance)
        { }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="factory">使用するIWebClientFactoryオブジェクト</param>
        public LivetubeClient(IWebClientFactory factory)
        {
            _webClient = factory.CreateWebClient();
        }

        private string PostUrl(string url, NameValueCollection keyValues)
        {
            if (_cookie != null)
            {
                _webClient.Headers.Add("Cookie", _cookie);
            }
            _webClient.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
            byte[] bytes = Encoding.ASCII.GetBytes(GetQueryString(keyValues));

            _webClient.UploadData(url, "POST", bytes);
                
            return _webClient.ResponseHeaders.Get("Set-Cookie");
        }

        private string GetUrlByString(string url)
        {
            if (_cookie != null)
            {
                _webClient.Headers.Add("Cookie", _cookie);
            }
            _webClient.Headers.Add("Accept-Charset", "Shift_JIS,utf-8;q=0.7,*;q=0.3");
            return _webClient.DownloadString(url);
        }

        private string GetQueryString(NameValueCollection keyValues)
        {
            StringBuilder buf = new StringBuilder();
            foreach (string key in keyValues)
            {
                if (buf.Length > 0)
                {
                    buf.Append("&");
                }
                buf.Append(key);
                buf.Append("=");
                buf.Append(Uri.EscapeDataString(keyValues[key]));
            }
            return buf.ToString();
        }

        /// <summary>
        /// ログインする
        /// </summary>
        /// <param name="user">ユーザー名</param>
        /// <param name="password">パスワード</param>
        /// <returns>ログインに成功したら真</returns>
        public bool Login(string user, string password)
        {
            if (_cookie != null)
            {
                Logoff();
            }

            NameValueCollection keyValues = new NameValueCollection();
            keyValues["user"] = user;
            keyValues["password"] = password;

            _cookie = PostUrl(LOGIN_URL, keyValues);
            return _cookie != null;
        }

        /// <summary>
        /// ログオフする
        /// </summary>
        public void Logoff()
        {
            if (_cookie != null)
            {
                GetUrlByString(LOGOFF_URL);
                _cookie = null;
            }
        }

        /// <summary>
        /// コメントを送る(ログインしていてユーザー名が一致すれば認証コメントになる)
        /// </summary>
        /// <param name="stream">ストリーム</param>
        /// <param name="user">ユーザー名</param>
        /// <param name="comment">コメント</param>
        public void PostComment(string stream, string user, string comment)
        {
            string commentUrl = string.Format("http://{0}/stream/{1}.comments", SERVER, stream);
            NameValueCollection keyValues = new NameValueCollection();
            keyValues["name"] = user;
            keyValues["c"] = comment;
            PostUrl(commentUrl, keyValues);
        }

        /// <summary>
        /// 指定した配信URLからストリームを識別するIDを見つけて返す
        /// </summary>
        /// <param name="url">配信URL</param>
        /// <returns>ストリームを識別するID</returns>
        public string FindStream(string url)
        {
            string stream = null;
            string html = GetUrlByString(url);
            Match m = STREAM_REGEX.Match(html);
            if (m.Success)
            {
                if (m.Groups.Count > 1)
                {
                    stream = m.Groups[1].Value;
                }
            }
            return stream;
        }

        /// <summary>
        /// 指定した配信者URLから現配信URLを見つけて返す
        /// </summary>
        /// <param name="userurl">配信者URL</param>
        /// <returns>現配信URL</returns>
        public string FindCurrentBroadcasting(string userurl)
        {
            string broadcast = null;
            string html = GetUrlByString(userurl);
            Match m = BROADCAST_REGEX.Match(html);
            if (m.Success)
            {
                if (m.Groups.Count > 1)
                {
                    broadcast = m.Groups[1].Value;
                    Uri broadcasturi = null;
                    Uri.TryCreate(new Uri(userurl), broadcast, out broadcasturi);
                    if (broadcasturi != null)
                        broadcast = broadcasturi.AbsoluteUri;
                }
            }
            return broadcast;
        }

        /// <summary>
        /// IDisposableのDispose実装
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private bool _disposed = false;

        /// <summary>
        /// Dispose処理の本体
        /// </summary>
        /// <remarks>実装方法の詳細はMSDN参照</remarks>
        /// <param name="disposing">Disposeから呼ばれていれば真</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // マネージリソースの開放
                    Logoff();
                    if (_webClient != null)
                    {
                        _webClient.Dispose();
                        _webClient = null;
                    }
                }
                // アンマネージリソースは特にないけどあればここで開放するらしい
                // null入れ
                _disposed = true;
            }
        }

        /// <summary>
        /// デストラクタ
        /// </summary>
        ~LivetubeClient()
        {
            Dispose(false);
        }
    }

    /// <summary>
    /// LivetubeClientの生成クラス
    /// </summary>
    /// <remarks>生成したオブジェクトの破棄はユーザー側の責任</remarks>
    public sealed class LivetubeClientFactory : ILivetubeClientFactory
    {
        private IWebClientFactory _webClientFactory;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <remarks>デフォルトのWebClientWrapperを使用するLivetubeClientを生成する</remarks>
        public LivetubeClientFactory()
            : this(WebClientWrapperFactory.DefaultInstance)
        { }

        /// <summary>
        /// IWebClientFactoryを外から選択可能なコンストラクタ
        /// </summary>
        /// <param name="factory">LivetubeClientが使用するIWebClientFactory</param>
        public LivetubeClientFactory(IWebClientFactory factory)
        {
            _webClientFactory = factory;
        }

        /// <summary>
        /// ILivetubeClientを実装したオブジェクト(LivetubeClient)を返す
        /// </summary>
        /// <remarks>生成したオブジェクトの破棄はユーザー側の責任</remarks>
        /// <returns>生成したオブジェクト</returns>
        public ILivetubeClient CreateLivetubeClient()
        {
            return new LivetubeClient(_webClientFactory);
        }

        /// <summary>
        /// 基底クラスの説明参照
        /// </summary>
        public void Dispose() { }
    }
}
