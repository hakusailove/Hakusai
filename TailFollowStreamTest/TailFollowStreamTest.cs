using NUnit.Framework;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Hakusai.IO.Test
{
    public class TailFollowStreamTest
    {
        [Test]
        public void ConstructorTest1_FromStart()
        {
            using (Stream s = new MemoryStream())
            using (TextWriter writer = new StreamWriter(s))
            {
                writer.WriteLine("こんにちは");
                writer.Flush();
                s.Seek(0, SeekOrigin.Begin);
                using (TailFollowStream tail = new TailFollowStream(s))
                using (TextReader reader = new StreamReader(tail))
                {
                    Assert.AreEqual("こんにちは", reader.ReadLine());
                }
            }
        }

        [Test]
        public void ConstructorTest2_FromEnd()
        {
            var path = Path.GetTempFileName();
            try
            {
                using (Stream ofs = new FileStream(path, FileMode.Open, FileAccess.Write, FileShare.Read))
                using (TextWriter writer = new StreamWriter(ofs))
                {
                    writer.WriteLine("こんにちは");
                    writer.Flush();
                    using (Stream ifs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Delete | FileShare.ReadWrite))
                    using (TailFollowStream tail = new TailFollowStream(ifs, true))
                    using (TextReader reader = new StreamReader(tail))
                    {
                        var job = Task<bool>.Factory.StartNew(() =>
                        {
                            Assert.AreEqual("さようなら", reader.ReadLine());
                            return true;
                        });
                        writer.WriteLine("さようなら");
                        writer.Flush();
                        job.Wait();
                        Assert.True(job.Result);
                    }
                }
            }
            finally
            {
                File.Delete(path);
            }
        }

        [Test]
        public void ConstructorTest3_IllegalNotReadable()
        {
            var path = Path.GetTempFileName();
            try
            {
                Exception ex = Assert.Throws<ArgumentException>(() =>
                {
                    using (Stream ofs = new FileStream(path, FileMode.Open, FileAccess.Write, FileShare.ReadWrite))
                    using (TextWriter writer = new StreamWriter(ofs))
                    {
                        using (Stream ifs = new FileStream(path, FileMode.Open, FileAccess.Write, FileShare.Delete | FileShare.ReadWrite))
                        using (TailFollowStream tail = new TailFollowStream(ifs, true))
                        using (TextReader reader = new StreamReader(tail))
                        {
                        }
                    }
                });
                Assert.AreEqual("不適切なストリームが指定されました。", ex.Message);
            }
            finally
            {
                File.Delete(path);
            }
        }

        [Test]
        public void WriteTest()
        {
            using (Stream s = new MemoryStream())
            {
                using (TailFollowStream tail = new TailFollowStream(s))
                {
                    Assert.Throws<NotSupportedException>(() =>
                        {
                            tail.Write(null, 0, 0);
                        });
                }
            }
        }

        [Test]
        public void Read1_AfterDispose()
        {
            using (Stream s = new MemoryStream())
            {
                using (TailFollowStream tail = new TailFollowStream(s))
                {
                    tail.Close();
                    Exception ex = Assert.Throws<ObjectDisposedException>(() =>
                    {
                        tail.Read(null, 0, 0);
                    });
                    Assert.AreEqual("破棄されたオブジェクトにアクセスできません。\r\nオブジェクト名 'Hakusai.IO.TailFollowStream' です。", ex.Message);
                }
            }
        }

        [Test]
        public void Read2_Normal()
        {
            var path = Path.GetTempFileName();
            try
            {
                using (Stream ofs = new FileStream(path, FileMode.Open, FileAccess.Write, FileShare.Read))
                using (TextWriter writer = new StreamWriter(ofs))
                {
                    writer.WriteLine("こんにちは");
                    writer.Flush();
                    using (Stream ifs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Delete | FileShare.ReadWrite))
                    using (TailFollowStream tail = new TailFollowStream(ifs))
                    using (TextReader reader = new StreamReader(tail))
                    {
                        var job = Task<bool>.Factory.StartNew(() =>
                        {
                            Assert.AreEqual("こんにちは", reader.ReadLine());
                            Assert.AreEqual("さようなら", reader.ReadLine());
                            Assert.IsNull(reader.ReadLine());
                            return true;
                        });
                        Thread.Sleep(1000);
                        writer.WriteLine("さようなら");
                        writer.Flush();
                        Thread.Sleep(1000);
                        reader.Close();
                        job.Wait();
                        Assert.True(job.Result);
                    }
                }
            }
            finally
            {
                File.Delete(path);
            }
        }

        private class WaitableReadFileStream : FileStream
        {
            public WaitableReadFileStream(
                string path,
                FileMode mode,
                FileAccess access,
                FileShare share
            ) :
                base(path, mode, access, share)
            {
            }
            private int _readWait = 0;
            public int ReadWait
            {
                get { return _readWait; }
                set { _readWait = value; }
            }

            public override int Read(byte[] array, int offset, int count)
            {
                if (_readWait > 0)
                {
                    Thread.Sleep(_readWait);
                }
                return base.Read(array, offset, count);
            }
        }

        [Test]
        public void Read3_StopWhileInternalRead()
        {
            var path = Path.GetTempFileName();
            try
            {
                using (Stream ofs = new FileStream(path, FileMode.Open, FileAccess.Write, FileShare.Read))
                using (TextWriter writer = new StreamWriter(ofs))
                {
                    writer.WriteLine("こんにちは");
                    writer.Flush();
                    using (WaitableReadFileStream ifs = new WaitableReadFileStream(path, FileMode.Open, FileAccess.Read, FileShare.Delete | FileShare.ReadWrite))
                    using (TailFollowStream tail = new TailFollowStream(ifs))
                    using (TextReader reader = new StreamReader(tail))
                    {
                        var job = Task<bool>.Factory.StartNew(() =>
                        {
                            Assert.AreEqual("こんにちは", reader.ReadLine());
                            Assert.AreEqual("さようなら", reader.ReadLine());
                            ifs.ReadWait = 1000;
                            Assert.IsNull(reader.ReadLine());
                            return true;
                        });
                        writer.WriteLine("さようなら");
                        writer.Flush();
                        Thread.Sleep(500);
                        reader.Close();
                        job.Wait();
                        Assert.True(job.Result);
                    }
                }
            }
            finally
            {
                File.Delete(path);
            }
        }

        [Test]
        public void SetLengthTest()
        {
            using (Stream s = new MemoryStream())
            using (TailFollowStream tail = new TailFollowStream(s))
            {
                Assert.Throws<NotSupportedException>(() =>
                {
                    tail.SetLength(0);
                });
            }
        }

        [Test]
        public void FlushTest()
        {
            using (Stream s = new MemoryStream())
            using (TailFollowStream tail = new TailFollowStream(s))
            {
                Assert.Throws<NotSupportedException>(() =>
                {
                    tail.Flush();
                });
            }
        }

        [Test]
        public void SeekTest()
        {
            using (Stream s = new MemoryStream())
            using (TailFollowStream tail = new TailFollowStream(s))
            {
                Assert.Throws<NotSupportedException>(() =>
                {
                    tail.Seek(0, SeekOrigin.Begin);
                });
            }
        }

        [Test]
        public void PositionGetTest()
        {
            using (Stream s = new MemoryStream())
            using (TailFollowStream tail = new TailFollowStream(s))
            {
                Assert.Throws<NotSupportedException>(() =>
                {
                    long len = tail.Position;
                });
            }
        }

        [Test]
        public void PositionSetTest()
        {
            using (Stream s = new MemoryStream())
            using (TailFollowStream tail = new TailFollowStream(s))
            {
                Assert.Throws<NotSupportedException>(() =>
                {
                    tail.Position = 0;
                });
            }
        }

        [Test]
        public void LengthGetTest()
        {
            using (Stream s = new MemoryStream())
            using (TailFollowStream tail = new TailFollowStream(s))
            {
                Assert.Throws<NotSupportedException>(() =>
                {
                    long len = tail.Length;
                });
            }
        }

        [Test]
        public void CanWriteGetTest()
        {
            using (Stream s = new MemoryStream())
            using (TailFollowStream tail = new TailFollowStream(s))
            {
                Assert.False(tail.CanWrite);
            }
        }

        [Test]
        public void CanSeekGetTest()
        {
            using (Stream s = new MemoryStream())
            using (TailFollowStream tail = new TailFollowStream(s))
            {
                Assert.False(tail.CanSeek);
            }
        }
    }
}

