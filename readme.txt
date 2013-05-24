■ライセンス
このディレクトリ以下にある配布ファイルは原則MITライセンスです。
サブディレクトリにある同名のファイルでライセンスが指定されている場合は上記の限りではありません。
MITライセンスについてはlicense.txtをご覧ください。

■はくさいクラスライブラリのソース一式
ここで配布しているものです。
内容的には素人が書いた.NETのクラスライブラリとアプリケーションです。
自分で書いたものを保存するため、一部分でも有益なら他人が見て使えるようにするために
公開させてもらっています。これ以上くどくど説明するほど有益なものではありません。

■ビルドについて
Visual Studio 2012 Express Desktopを使っています。
基本はHakusai.slnをダブルクリックしてビルドするだけですが、
最初は参照しているライブラリを各自ダウンロードして参照設定しないといけません。

☆CaveTube.CavetubeClient
Cavetubeにアクセスするのに使っているライブラリです。
http://gae.cavelis.net/howto/#other_talk
ここからCaveTalkをダウンロードしてzipをextlibディレクトリに展開し、
中に含まれるCaveTube.CavetubeClient.dllに必要なプロジェクトから参照設定をしてください。

☆NLog
ログを出力するのに使っているライブラリです。
http://nlog-project.org/
NuGetでインストールするのが楽です。必要なプロジェクトから参照設定をしてください。

＝＝ もしテストプロジェクトもビルドしたい場合はさらに以下も必要です

☆NUnit
単体テストを実施するのに使っているフレームです。
http://www.nunit.org/
ダウンロードしてインストールしてください。

☆Moq
単体テストで使っているMockライブラリです。
https://code.google.com/p/moq/
ここからダウンロードしてzipをextlibディレクトリに展開し必要なプロジェクトから
参照設定をしてください。

＝＝ もし単体テストのビルドだけでなく(デバッグ)実行もしたい場合
プロジェクトのディレクトリにある、*.csproj.userを手書きで書き換える
必要があるかもしれません。インストールしたパスやバージョンが違えば合わせてください。
Visual StudioがExpressでない人は逆にこのファイル自体が必要ないかもしれません。

＝＝ もし単体テストの実行だけでなくカバレッジも取りたい場合
☆OpenCover
カバレッジを取るのに使えるソフトです。
http://opencover.codeplex.com/
ここからダウンロードしてインストールしてください

☆ReportGenerator
上記OpenCoverは結果をXMLで吐くだけなので、それをHTMLに整形するツールです
http://reportgenerator.codeplex.com/
ここからダウンロードしてOpenCoverのインストール先に展開してください

各テストプロジェクト(*Test.csproj)はreport.cmdというファイルを含んでおり
これで上記ツール2つを使ってカバレッジが測定できるようにしています。
パスなどが違うと動かないので環境に合わせて修正してください。
これらのファイルはbin/Debugなどにコピーされますので、コピーされたものを
ダブルクリックして実行してください。htmlディレクトリに結果が生成されます。

＝＝ もしソースコード内のコメントからドキュメントも生成したい場合
以下のソフトをインストールしてください。

☆Sandcastle Help File Builder
http://shfb.codeplex.com/

☆日本語リソース
https://code.google.com/p/sandcastle-help-file-builder-japanese-help-file-pack/

Hakusai.shfbprojをダブルクリックし、Documentation>Build Projectで、
Helpディレクトリに生成されます。
