using NUnit.Framework;
using System;
using System.IO;

namespace Hakusai.Csv.Test
{
    public class CsvParserTest
    {
        [Test]
        public void ParseTest1()
        {
            StringReader sin = new StringReader("1,2,3,4,5\r\n6,7,8,9,0\r\n");
            String[,] answer = { { "1", "2", "3", "4", "5" }, { "6", "7", "8", "9", "0" } };
            int recno = 0;
            CsvParser.Parse(
                sin,
                (strings) =>
                {
                    Assert.AreEqual(5, strings.Length);
                    Assert.AreEqual(answer[recno, 0], strings[0]);
                    Assert.AreEqual(answer[recno, 1], strings[1]);
                    Assert.AreEqual(answer[recno, 2], strings[2]);
                    Assert.AreEqual(answer[recno, 3], strings[3]);
                    Assert.AreEqual(answer[recno, 4], strings[4]);
                    ++recno;
                });
        }

        [Test]
        public void ParseTest2_Quoted()
        {
            StringReader sin = new StringReader("\"1\",\"2\"\r\n\"6\",\"7\"\r\n");
            String[,] answer = { { "1", "2" }, { "6", "7" } };
            int recno = 0;
            CsvParser.Parse(
                sin,
                (strings) =>
                {
                    Assert.AreEqual(2, strings.Length);
                    Assert.AreEqual(answer[recno, 0], strings[0]);
                    Assert.AreEqual(answer[recno, 1], strings[1]);
                    ++recno;
                });
        }
        [Test]
        public void ParseTest3_OnlyLF()
        {
            StringReader sin = new StringReader("1,2\n\"6\",\"7\"\n");
            String[,] answer = { { "1", "2" }, { "6", "7" } };
            int recno = 0;
            CsvParser.Parse(
                sin,
                (strings) =>
                {
                    Assert.AreEqual(2, strings.Length);
                    Assert.AreEqual(answer[recno, 0], strings[0]);
                    Assert.AreEqual(answer[recno, 1], strings[1]);
                    ++recno;
                });
        }
        [Test]
        public void ParseTest4_QuotedSpecialChars()
        {
            StringReader sin = new StringReader("\"1,\n\r\",\"2,\n\r\"\r\n\"6,\n\r\",\"7,\n\r\"\r\n");
            String[,] answer = { { "1,\n\r", "2,\n\r" }, { "6,\n\r", "7,\n\r" } };
            int recno = 0;
            CsvParser.Parse(
                sin,
                (strings) =>
                {
                    Assert.AreEqual(2, strings.Length);
                    Assert.AreEqual(answer[recno, 0], strings[0]);
                    Assert.AreEqual(answer[recno, 1], strings[1]);
                    ++recno;
                });
        }
        [Test]
        public void ParseTest5_Escaped()
        {
            StringReader sin = new StringReader("\"\"\"1\"\"\",\"\"\"2\"\"\"\r\n\"\"\"6\"\"\",\"\"\"7\"\"\"\r\n");
            String[,] answer = { { "\"1\"", "\"2\"" }, { "\"6\"", "\"7\"" } };
            int recno = 0;
            CsvParser.Parse(
                sin,
                (strings) =>
                {
                    Assert.AreEqual(2, strings.Length);
                    Assert.AreEqual(answer[recno, 0], strings[0]);
                    Assert.AreEqual(answer[recno, 1], strings[1]);
                    ++recno;
                });
        }
        [Test]
        public void ParseTest6_IllegalEOF1()
        {
            StringReader sin = new StringReader("1,2\r\n6,7\r");
            String[,] answer = { { "1", "2" }, { "6", "7\r" } };
            int recno = 0;
            CsvParser.Parse(
                sin,
                (strings) =>
                {
                    Assert.AreEqual(2, strings.Length);
                    Assert.AreEqual(answer[recno, 0], strings[0]);
                    Assert.AreEqual(answer[recno, 1], strings[1]);
                    ++recno;
                });
        }
        [Test]
        public void ParseTest7_IllegalEOF2()
        {
            StringReader sin = new StringReader("1,2\r\n6,7");
            String[,] answer = { { "1", "2" }, { "6", "7" } };
            int recno = 0;
            CsvParser.Parse(
                sin,
                (strings) =>
                {
                    Assert.AreEqual(2, strings.Length);
                    Assert.AreEqual(answer[recno, 0], strings[0]);
                    Assert.AreEqual(answer[recno, 1], strings[1]);
                    ++recno;
                });
        }
        [Test]
        public void ParseTest8_IllegalEOF3()
        {
            StringReader sin = new StringReader("1,2\r\n6,");
            String[][] answer = { new String[] { "1", "2" }, new String[]{ "6" } };
            int recno = 0;
            CsvParser.Parse(
                sin,
                (strings) =>
                {
                    Assert.AreEqual(answer[recno].Length, strings.Length);
                    for (int i = 0; i < answer[recno].Length; ++i)
                    {
                        Assert.AreEqual(answer[recno][i], strings[i]);
                    }
                    ++recno;
                });
        }
        [Test]
        public void ParseTest9_IllegalEOF4()
        {
            StringReader sin = new StringReader("1,2\r\n6,\"");
            String[][] answer = { new String[] { "1", "2" }, new String[] { "6", "\"" } };
            int recno = 0;
            CsvParser.Parse(
                sin,
                (strings) =>
                {
                    Assert.AreEqual(answer[recno].Length, strings.Length);
                    for (int i = 0; i < answer[recno].Length; ++i)
                    {
                        Assert.AreEqual(answer[recno][i], strings[i]);
                    }
                    ++recno;
                });
        }
        [Test]
        public void ParseTest10_IllegalEOF5()
        {
            StringReader sin = new StringReader("1,2\r\n6,\"\"");
            String[][] answer = { new String[] { "1", "2" }, new String[] { "6" } };
            int recno = 0;
            CsvParser.Parse(
                sin,
                (strings) =>
                {
                    Assert.AreEqual(answer[recno].Length, strings.Length);
                    for (int i = 0; i < answer[recno].Length; ++i)
                    {
                        Assert.AreEqual(answer[recno][i], strings[i]);
                    }
                    ++recno;
                });
        }
        [Test]
        public void ParseTest11_IllegalEOF6()
        {
            StringReader sin = new StringReader("1,2\r\n6,7\"\"");
            String[][] answer = { new String[] { "1", "2" }, new String[] { "6", "7" } };
            int recno = 0;
            CsvParser.Parse(
                sin,
                (strings) =>
                {
                    Assert.AreEqual(answer[recno].Length, strings.Length);
                    for (int i = 0; i < answer[recno].Length; ++i)
                    {
                        Assert.AreEqual(answer[recno][i], strings[i]);
                    }
                    ++recno;
                });
        }
        [Test]
        public void ParseTest12_QuotedOnlyLF()
        {
            StringReader sin = new StringReader("\"1\",\"2\"\n\"6\",\"\"\n");
            String[][] answer = { new String[] { "1", "2" }, new String[] { "6", "" } };
            int recno = 0;
            CsvParser.Parse(
                sin,
                (strings) =>
                {
                    Assert.AreEqual(answer[recno].Length, strings.Length);
                    for (int i = 0; i < answer[recno].Length; ++i)
                    {
                        Assert.AreEqual(answer[recno][i], strings[i]);
                    }
                    ++recno;
                });
        }
        [Test]
        public void ParseTest12_Empty()
        {
            StringReader sin = new StringReader(",\r\n,\r\n");
            String[][] answer = { new String[] { "", "" }, new String[] { "", "" } };
            int recno = 0;
            CsvParser.Parse(
                sin,
                (strings) =>
                {
                    Assert.AreEqual(answer[recno].Length, strings.Length);
                    for (int i = 0; i < answer[recno].Length; ++i)
                    {
                        Assert.AreEqual(answer[recno][i], strings[i]);
                    }
                    ++recno;
                });
        }
        [Test]
        public void ParseTest13_MultipleQuoted()
        {
            StringReader sin = new StringReader("\"\"1\"\",\"\"2\"\"\r\n\"\"3\"\",\"\"4\"\"\r\n");
            String[][] answer = { new String[] { "1", "2" }, new String[] { "3", "4" } };
            int recno = 0;
            CsvParser.Parse(
                sin,
                (strings) =>
                {
                    Assert.AreEqual(answer[recno].Length, strings.Length);
                    for (int i = 0; i < answer[recno].Length; ++i)
                    {
                        Assert.AreEqual(answer[recno][i], strings[i]);
                    }
                    ++recno;
                });
        }
        [Test]
        public void ParseTest14_InternalQuoted()
        {
            StringReader sin = new StringReader("1\"\"2,3\"\"4\r\n5\"\"6,7\"\"8\r\n");
            String[][] answer = { new String[] { "12", "34" }, new String[] { "56", "78" } };
            int recno = 0;
            CsvParser.Parse(
                sin,
                (strings) =>
                {
                    Assert.AreEqual(answer[recno].Length, strings.Length);
                    for (int i = 0; i < answer[recno].Length; ++i)
                    {
                        Assert.AreEqual(answer[recno][i], strings[i]);
                    }
                    ++recno;
                });
        }
        [Test]
        public void ParseTest15_IllegalCR()
        {
            StringReader sin = new StringReader("12,34\r56,78\r");
            String[][] answer = { new String[] { "12", "34\r56", "78\r" } };
            int recno = 0;
            CsvParser.Parse(
                sin,
                (strings) =>
                {
                    Assert.AreEqual(answer[recno].Length, strings.Length);
                    for (int i = 0; i < answer[recno].Length; ++i)
                    {
                        Assert.AreEqual(answer[recno][i], strings[i]);
                    }
                    ++recno;
                });
        }
        [Test]
        public void ParseTest16_IllegalCR2()
        {
            StringReader sin = new StringReader("\r,\r\"1\"\r\r");
            String[][] answer = { new String[] { "\r", "\r1\r\r" } };
            int recno = 0;
            CsvParser.Parse(
                sin,
                (strings) =>
                {
                    Assert.AreEqual(answer[recno].Length, strings.Length);
                    for (int i = 0; i < answer[recno].Length; ++i)
                    {
                        Assert.AreEqual(answer[recno][i], strings[i]);
                    }
                    ++recno;
                });
        }
    }
}

