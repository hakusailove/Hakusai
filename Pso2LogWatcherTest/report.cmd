REM OpenCoverでカバレッジを取りつつNUnitを実行し、結果をReportGeneratorでHTMLにするバッチ
REM このファイルは環境によって修正が必要です
REM コンソール上ではこのコメントは文字化けしますがVisual Studio上での編集の都合上UTF-8にしています

REM 環境設定
set OPENCOVDIR=%LOCALAPPDATA%\Apps\OpenCover
set OPENCOVER=%OPENCOVDIR%\OpenCover.Console.exe
set NUNITCONS=%ProgramFiles(x86)%\NUnit 2.6.2\bin\nunit-console.exe
set REPORTGEN=%OPENCOVDIR%\ReportGenerator.exe
set COVXML=coverage.xml
set TARGETDIR=html

REM 対象設定
set TARGET=Pso2LogWatcherTest.dll

"%OPENCOVER%" -register:user -target:"%NUNITCONS%" -targetargs:"%TARGET%" "-filter:+[Pso2LogWatcher]* -[*]*.NamespaceDoc" -hideskipped:All -output:"%COVXML%"
"%REPORTGEN%" "-reports:%COVXML%" "-targetdir:%TARGETDIR%" -filters:+Pso2LogWatcher