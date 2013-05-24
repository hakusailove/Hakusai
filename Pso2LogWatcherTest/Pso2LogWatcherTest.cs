using System;
using NUnit.Framework;
using System.IO;
using System.Text;
using System.Threading;

namespace Hakusai.Pso2.Test
{
    public class Pso2LogWatcherTest
    {
        [Test]
        public void DefaultLogDirGetTest()
        {
            string PSO2_LOG_DIR = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.Personal),
                Path.Combine(new string[] { "SEGA", "PHANTASYSTARONLINE2", "log" }));
            Assert.AreEqual(PSO2_LOG_DIR, Pso2LogWatcher.DefaultLogDir);
        }

        enum StartTestState : int
        {
            WaitingAssertRow1,
            AssertedRow1,
            AssertedRow2,
            AssertedRow3
        };

        [Test]
        public void StartTest_FirstTime()
        {
            String dir = Path.GetRandomFileName();
            // まあ確率的に例外とかないだろう
            Directory.CreateDirectory(dir);
            try
            {
                String[][] expectedRows = new String[][]{
                    new String[]{"2013-05-17T20:07:48", "9", "GUILD", "12133620", "白 菜", "テスト"},
                    new String[]{"2013-05-17T20:07:49", "10", "PUBLIC", "12133621", "白 菜2", "テスト2"},
                    new String[]{"2013-05-17T20:07:50", "11", "PARTY", "12133622", "白 菜3", "テスト3"}
                };
                int row = 0;

                using (IPso2LogWatcherFactory factory = new Pso2LogWatcherFactory())
                using (IPso2LogWatcher watcher = factory.CreatePso2LogWatcher())
                {
                    StartTestState[] state = new StartTestState[] { StartTestState.WaitingAssertRow1 };
                    watcher.Pso2LogEvent += (s, e) =>
                    {
                        string[] expectedCols = expectedRows[row++];
                        int col = 0;
                        Assert.AreEqual(expectedCols[col++], e.Time);
                        Assert.AreEqual(expectedCols[col++], e.MessageID);
                        Assert.AreEqual(expectedCols[col++], e.SendTo);
                        Assert.AreEqual(expectedCols[col++], e.FromID);
                        Assert.AreEqual(expectedCols[col++], e.From);
                        Assert.AreEqual(expectedCols[col++], e.Message);

                        lock (state)
                        {
                            state[0]++;
                            Monitor.Pulse(state);
                        }
                    };
                    watcher.Start(dir);
                    using (TextWriter log = 
                        new StreamWriter(
                            new FileStream(
                                Path.Combine(dir, "ChatLog20130519_00.txt"),
                                FileMode.CreateNew,
                                FileAccess.Write,
                                FileShare.Delete | FileShare.ReadWrite
                            ),
                            Encoding.Unicode
                        )
                    )
                    {
                        log.WriteLine("2013-05-17T20:07:48\t9\tGUILD\t12133620\t白 菜\tテスト");
                        log.Flush();

                        lock (state)
                        {
                            if (state[0] != StartTestState.AssertedRow1)
                            {
                                Monitor.Wait(state, 3000);
                            }
                            Assert.AreEqual(StartTestState.AssertedRow1, state[0]);
                        }
                        log.WriteLine("2013-05-17T20:07:49\t10\tPUBLIC\t12133621\t白 菜2\tテスト2");
                        log.Flush();
                    }
                    lock (state)
                    {
                        if (state[0] != StartTestState.AssertedRow2)
                        {
                            Monitor.Wait(state, 3000);
                        }
                        Assert.AreEqual(StartTestState.AssertedRow2, state[0]);
                    }
                    using (TextWriter log =
                        new StreamWriter(
                            new FileStream(
                                Path.Combine(dir, "ChatLog20130520_00.txt"),
                                FileMode.CreateNew,
                                FileAccess.Write,
                                FileShare.Delete | FileShare.ReadWrite
                            ),
                            Encoding.Unicode
                        )
                    )
                    {
                        log.WriteLine("2013-05-17T20:07:50\t11\tPARTY\t12133622\t白 菜3\tテスト3");
                        log.Flush();
                    }
                    lock (state)
                    {
                        if (state[0] != StartTestState.AssertedRow3)
                        {
                            Monitor.Wait(state, 3000);
                        }
                        Assert.AreEqual(StartTestState.AssertedRow3, state[0]);
                    }
                    watcher.Stop();
                }
            }
            finally
            {
                Directory.Delete(dir, true);
            }
        }

        enum StartTestState2 : int
        {
            WaitingAssertRow1,
            AssertedRow1,
        };
        [Test]
        public void StartTest2_Twice()
        {
            String dir = Path.GetRandomFileName();
            // まあ確率的に例外とかないだろう
            Directory.CreateDirectory(dir);
            try
            {
                String[][] expectedRows = new String[][]{
                    new String[]{"2013-05-17T20:07:48", "9", "GUILD", "12133620", "白 菜", "テスト"},
                };
                int row = 0;

                using (IPso2LogWatcherFactory factory = new Pso2LogWatcherFactory())
                using (IPso2LogWatcher watcher = factory.CreatePso2LogWatcher())
                {
                    StartTestState2[] state = new StartTestState2[] { StartTestState2.WaitingAssertRow1 };
                    watcher.Pso2LogEvent += (s, e) =>
                    {
                        string[] expectedCols = expectedRows[row++];
                        int col = 0;
                        Assert.AreEqual(expectedCols[col++], e.Time);
                        Assert.AreEqual(expectedCols[col++], e.MessageID);
                        Assert.AreEqual(expectedCols[col++], e.SendTo);
                        Assert.AreEqual(expectedCols[col++], e.FromID);
                        Assert.AreEqual(expectedCols[col++], e.From);
                        Assert.AreEqual(expectedCols[col++], e.Message);

                        lock (state)
                        {
                            state[0]++;
                            Monitor.Pulse(state);
                        }
                    };
                    watcher.Start(dir);
                    using (TextWriter log =
                        new StreamWriter(
                            new FileStream(
                                Path.Combine(dir, "ChatLog20130519_00.txt"),
                                FileMode.CreateNew,
                                FileAccess.Write,
                                FileShare.Delete | FileShare.ReadWrite
                            ),
                            Encoding.Unicode
                        )
                    )
                    {
                        log.WriteLine("2013-05-17T20:07:48\t9\tGUILD\t12133620\t白 菜\tテスト");
                        log.Flush();
                    }
                    lock (state)
                    {
                        if (state[0] != StartTestState2.AssertedRow1)
                        {
                            Monitor.Wait(state, 3000);
                        }
                        Assert.AreEqual(StartTestState2.AssertedRow1, state[0]);
                    }
                    watcher.Stop();
                    Assert.Throws<ObjectDisposedException>(() =>
                    {
                        watcher.Start(dir);
                    });
                    watcher.Stop();
                }
            }
            finally
            {
                Directory.Delete(dir, true);
            }
        }

        [Test]
        public void StartTest3_StartWhileStart()
        {
            String dir = Path.GetRandomFileName();
            // まあ確率的に例外とかないだろう
            Directory.CreateDirectory(dir);
            try
            {
                using (IPso2LogWatcherFactory factory = new Pso2LogWatcherFactory())
                using (IPso2LogWatcher watcher = factory.CreatePso2LogWatcher())
                {
                    watcher.Pso2LogEvent += (s, e) =>
                    {
                    };
                    watcher.Start(dir);
                    Assert.Throws<InvalidOperationException>(() =>
                    {
                        watcher.Start(dir);
                    });
                    watcher.Stop();
                }
            }
            finally
            {
                Directory.Delete(dir, true);
            }
        }

        enum StartTestState4 : int
        {
            WaitingAssertRow1,
            AssertedRow1,
            AssertedRow2
        };

        [Test]
        public void StartTest4_SecondTime()
        {
            String dir = Path.GetRandomFileName();
            // まあ確率的に例外とかないだろう
            Directory.CreateDirectory(dir);
            try
            {
                String[][] expectedRows = new String[][]{
                    new String[]{"2013-05-17T20:07:49", "10", "PUBLIC", "12133621", "白 菜2", "テスト2"},
                    new String[]{"2013-05-17T20:07:50", "11", "PARTY", "12133622", "白 菜3", "テスト3"}
                };
                int row = 0;

                using (IPso2LogWatcherFactory factory = new Pso2LogWatcherFactory())
                using (IPso2LogWatcher watcher = factory.CreatePso2LogWatcher())
                {
                    StartTestState4[] state = new StartTestState4[] { StartTestState4.WaitingAssertRow1 };
                    watcher.Pso2LogEvent += (s, e) =>
                    {
                        string[] expectedCols = expectedRows[row++];
                        int col = 0;
                        Assert.AreEqual(expectedCols[col++], e.Time);
                        Assert.AreEqual(expectedCols[col++], e.MessageID);
                        Assert.AreEqual(expectedCols[col++], e.SendTo);
                        Assert.AreEqual(expectedCols[col++], e.FromID);
                        Assert.AreEqual(expectedCols[col++], e.From);
                        Assert.AreEqual(expectedCols[col++], e.Message);

                        lock (state)
                        {
                            state[0]++;
                            Monitor.Pulse(state);
                        }
                    };
                    using (TextWriter log =
                        new StreamWriter(
                            new FileStream(
                                Path.Combine(dir, "ChatLog20130519_00.txt"),
                                FileMode.CreateNew,
                                FileAccess.Write,
                                FileShare.Delete | FileShare.ReadWrite
                            ),
                            Encoding.Unicode
                        )
                    )
                    {
                        log.WriteLine("2013-05-17T20:07:48\t9\tGUILD\t12133620\t白 菜\tテスト");
                        log.Flush();

                        watcher.Start(dir);

                        log.WriteLine("2013-05-17T20:07:49\t10\tPUBLIC\t12133621\t白 菜2\tテスト2");
                        log.Flush();
                    }
                    lock (state)
                    {
                        if (state[0] != StartTestState4.AssertedRow1)
                        {
                            Monitor.Wait(state, 3000);
                        }
                        Assert.AreEqual(StartTestState4.AssertedRow1, state[0]);
                    }
                    using (TextWriter log =
                        new StreamWriter(
                            new FileStream(
                                Path.Combine(dir, "ChatLog20130520_00.txt"),
                                FileMode.CreateNew,
                                FileAccess.Write,
                                FileShare.Delete | FileShare.ReadWrite
                            ),
                            Encoding.Unicode
                        )
                    )
                    {
                        log.WriteLine("2013-05-17T20:07:50\t11\tPARTY\t12133622\t白 菜3\tテスト3");
                        log.Flush();
                    }
                    lock (state)
                    {
                        if (state[0] != StartTestState4.AssertedRow2)
                        {
                            Monitor.Wait(state, 3000);
                        }
                        Assert.AreEqual(StartTestState4.AssertedRow2, state[0]);
                    }
                    watcher.Stop();
                }
            }
            finally
            {
                Directory.Delete(dir, true);
            }
        }

        enum StartTestState5 : int
        {
            WaitingAssertRow1,
            AssertedRow1,
            AssertedRow2
        };

        [Test]
        public void StartTest5_NotepadLikeCreateFile()
        {
            String dir = Path.GetRandomFileName();
            // まあ確率的に例外とかないだろう
            Directory.CreateDirectory(dir);
            try
            {
                String[][] expectedRows = new String[][]{
                    new String[]{"2013-05-17T20:07:49", "10", "PUBLIC", "12133621", "白 菜2", "テスト2"},
                    new String[]{"2013-05-17T20:07:50", "11", "PARTY", "12133622", "白 菜3", "テスト3"}
                };
                int row = 0;

                using (IPso2LogWatcherFactory factory = new Pso2LogWatcherFactory())
                using (IPso2LogWatcher watcher = factory.CreatePso2LogWatcher())
                {
                    StartTestState5[] state = new StartTestState5[] { StartTestState5.WaitingAssertRow1 };
                    watcher.Pso2LogEvent += (s, e) =>
                    {
                        string[] expectedCols = expectedRows[row++];
                        int col = 0;
                        Assert.AreEqual(expectedCols[col++], e.Time);
                        Assert.AreEqual(expectedCols[col++], e.MessageID);
                        Assert.AreEqual(expectedCols[col++], e.SendTo);
                        Assert.AreEqual(expectedCols[col++], e.FromID);
                        Assert.AreEqual(expectedCols[col++], e.From);
                        Assert.AreEqual(expectedCols[col++], e.Message);

                        lock (state)
                        {
                            state[0]++;
                            Monitor.Pulse(state);
                        }
                    };
                    using (TextWriter log =
                        new StreamWriter(
                            new FileStream(
                                Path.Combine(dir, "ChatLog20130519_00.txt"),
                                FileMode.CreateNew,
                                FileAccess.Write,
                                FileShare.Delete | FileShare.ReadWrite
                            ),
                            Encoding.Unicode
                        )
                    )
                    {
                        log.WriteLine("2013-05-17T20:07:48\t9\tGUILD\t12133620\t白 菜\tテスト");
                        log.Flush();

                        watcher.Start(dir);

                        log.WriteLine("2013-05-17T20:07:49\t10\tPUBLIC\t12133621\t白 菜2\tテスト2");
                        log.Flush();
                    }
                    lock (state)
                    {
                        if (state[0] != StartTestState5.AssertedRow1)
                        {
                            Monitor.Wait(state, 3000);
                        }
                        Assert.AreEqual(StartTestState5.AssertedRow1, state[0]);
                    }
                    File.Create(Path.Combine(dir, "ChatLog20130520_00.txt")).Close();
                    File.Delete(Path.Combine(dir, "ChatLog20130520_00.txt"));
                    Thread.Sleep(100);
                    using (TextWriter log =
                        new StreamWriter(
                            new FileStream(
                                Path.Combine(dir, "ChatLog20130520_00.txt"),
                                FileMode.CreateNew,
                                FileAccess.Write,
                                FileShare.Delete | FileShare.ReadWrite
                            ),
                            Encoding.Unicode
                        )
                    )
                    {
                        log.WriteLine("2013-05-17T20:07:50\t11\tPARTY\t12133622\t白 菜3\tテスト3");
                        log.Flush();
                    }
                    lock (state)
                    {
                        if (state[0] != StartTestState5.AssertedRow2)
                        {
                            Monitor.Wait(state, 3000);
                        }
                        Assert.AreEqual(StartTestState5.AssertedRow2, state[0]);
                    }
                    watcher.Stop();
                }
            }
            finally
            {
                Directory.Delete(dir, true);
            }
        }

        [Test]
        public void DestructorTest()
        {
            using (IPso2LogWatcherFactory factory = new Pso2LogWatcherFactory())
            {
                // 意図的にリークさせる
                factory.CreatePso2LogWatcher();
            }
            // ファイナライザが呼ばれることを期待(using使うとDisposeで呼ばれず、カバレッジを上げるためだけに実施している)
            GC.Collect();
        }
    }
}