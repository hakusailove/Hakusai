using System;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace LivetubeCommentReaderSample
{
    /// <summary>
    /// Livetubeからコメントを読んで表示するだけのサンプルプログラムを格納してます。
    /// </summary>
    /// <remarks>
    /// そのうち削除予定。
    /// </remarks>
    [System.Runtime.CompilerServices.CompilerGeneratedAttribute]
    class NamespaceDoc
    {
    }

    /// <summary>
    /// MonoのM$互換ライブラリに<see cref="System.Xml.Resolvers.XmlPreloadedResolver"/>がないのでダミーを作った
    /// </summary>
	public class DummyXmlResolver : XmlResolver
	{
		protected Stream _stream = null;
		protected Uri _uri = null;
		
        /// <summary>
        /// 派生元の説明参照
        /// </summary>
		public override ICredentials Credentials
		{
			set {} // どうせファイルから取るから必要ない
		}

        /// <summary>
        /// 派生元の説明参照
        /// </summary>
        public override Object GetEntity(
			Uri absoluteUri,
			string role,
			Type ofObjectToReturn
		)
		{
			Object result = null;
			if (_uri.Equals(absoluteUri)) {
				if (ofObjectToReturn == typeof(Stream)) {
					result = _stream;
				}
			}
			return result;
		}

        /// <summary>
        /// <see cref="System.Xml.Resolvers.XmlPreloadedResolver"/>の説明参照
        /// </summary>
        public void Add(
			Uri uri,
			Stream value
		)
		{
			_uri = uri;
			var bytes = new byte[value.Length];
			value.Read(bytes, 0, bytes.Length);
			_stream = new MemoryStream(bytes);
		}
	}

    /// <summary>
    /// Livetubeからコメントを読んで表示するだけのサンプルプログラム
    /// </summary>
    /// <remarks>
    /// 現在のはくさいライブラリの機能では実現できないので入れています。
    /// そのうち削除予定。
    /// </remarks>
    public class Program
    {
        static void Main(string[] args)
        {
            var cancelSource = new CancellationTokenSource();
            CancellationToken token = cancelSource.Token;

            Task job = Task.Factory.StartNew(() =>
            {
                using (var web = new WebClient() { Encoding = Encoding.UTF8 })
                {
					token.Register(() => {
						web.CancelAsync();
					});
                    int count = 0;
                    var html = web.DownloadString("http://livetube.cc/ユーザー/タイトル");

                    // ストリームのURLを取得
                    Match m = new Regex("var comment_entry_id = \"([^\"]*)\";").Match(html);
                    var stream_id = "";
                    if (m.Success && (m.Groups.Count > 1))
                    {
                        stream_id = m.Groups[1].Value;
                    }

                    // HTMLのパースにXMLパーサーを使う
                    // XMLの文字実体参照は5つしかないのでXHTMLとしてパースする
                    // ただしW3Cにアクセスしにいくと迷惑なのでDTDをここからダウンロードして保存しておく
                    // www.w3.org/TR/xhtml11/DTD/xhtml11-flat.dtd
                    FileInfo dtdFile = new FileInfo("xhtml11-flat.dtd");
                    Uri dtdPublicUri = new Uri("http://www.w3.org/TR/xhtml11/DTD/xhtml11-flat.dtd");
                    var xpr = new DummyXmlResolver();
                    using (FileStream fs = dtdFile.Open(FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        xpr.Add(dtdPublicUri, fs);
                    }
                    
                    var nameRegex = new Regex("^\\s*\\d+\\s:\\s*(.*)\\s$");
                    try
                    {
                        while (true)
                        {
                            // 必要なコメントをダウンロードする(中身はHTML要素)
                            var url = String.Format("http://livetube.cc/stream/{0}.comments.{1}", stream_id, count);
                            using (var observable = new ManualResetEvent(false))
                            {
                                DownloadStringCompletedEventHandler action = (sender, e) =>
                                {
                                    html = e.Result;
                                    observable.Set();
                                };
                                web.DownloadStringCompleted += action;
                                web.DownloadStringAsync(new Uri(url));
                                observable.WaitOne();
								token.ThrowIfCancellationRequested();
                                web.DownloadStringCompleted -= action;
                            }

                            // HTML要素に追加してXHTMLなHTML文書に
                            var head = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.1//EN\" \"http://www.w3.org/TR/xhtml11/DTD/xhtml11-flat.dtd\">\r\n<html><body>\r\n";
                            var tail = "</body></html>";
                            html = head + html + tail;

                            // XHTML文書としてパース
                            var doc = new XmlDocument();
                            doc.XmlResolver = xpr;
                            doc.LoadXml(html);
                            XmlNamespaceManager nm = new XmlNamespaceManager(doc.NameTable);
                            nm.AddNamespace("p", doc.DocumentElement.NamespaceURI);

                            // XPathを使って名前とコメントを抽出
                            foreach (XmlNode div in doc.SelectNodes("/p:html/p:body/p:div", nm))
                            {
                                String name = "";
                                String comment = "";
                                var nodes = div.SelectNodes("p:div/text()", nm);
                                if (nodes != null)
                                {
                                    name = nameRegex.Replace(nodes[0].InnerText, "$1");
                                }
                                nodes = div.SelectNodes("p:div/p:a/p:span", nm);
                                if (nodes != null && nodes.Count > 0)
                                {
                                    name = nodes[0].InnerText;
                                }
                                nodes = div.SelectNodes("p:div/p:span", nm);
                                comment = nodes[1].InnerText;
                                Console.WriteLine("{0}: {1}", name, comment);
                                ++count;
                            }
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        Console.WriteLine("//// 停止要求を受け停止");
                    }
                }
            }, token);

            // 上記をタスクとして実行しておき、Enterキーで中止指示
            Console.ReadLine();
            cancelSource.Cancel();

            // 終了を待つ
            job.Wait();
        }
    }
}
