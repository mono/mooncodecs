using org.diracvideo.Jirac;
using System;
using System.IO;
using NUnit.Framework;
using System.Collections.Generic;
namespace org.diracvideo.Jirac.Test
{
    /** A test for the arithmetic decoder.
     *
     * Decodes a arithmetic encoded file from arith_file */

    [TestFixture]
    public class ArithTest {
        [Test]
        public void main() {
            //TODO dialogue to ask string
            string[] a = new string[0];
            FileStream input = TryOpen(a);
            byte[] d = new byte[input.Length];
            input.Read(d, 0, (int)input.Length);
            input.Close();
            input.Dispose();
            Arithmetic ar = new Arithmetic(d);
            TestArithmetic(ar);
        }

        private FileStream TryOpen(String[] a)
        {
	        for(int i = 0; i < a.Length; i++) {
	            if (File.Exists(a[i]))
                {
                    return File.Open(a[i], FileMode.Open);
	            }
	        }
            List<string> files = new List<string>();
	        
	        foreach(string f in Directory.GetFiles(".")) {
                if(f.Length == f.LastIndexOf(".sundae") + 7 && File.Exists(f))
                    return new FileStream(f, FileMode.Open);
	        }
	        Console.WriteLine("No arith file was found");
	        Environment.Exit(0);
	        return null;
        }

        private void TestArithmetic(Arithmetic a) {
	        byte[] d = new byte[a.BytesLeft()];
	        for(int i = 0; i < d.Length; i++) {
	            byte c = 0;
	            for(int j = 0; j < 8; j++) {
		        c = (byte)((c << 1) | a.DecodeBit(j));
	            }
	            d[i] = c;
	        }
	        String s = System.Text.Encoding.Default.GetString(d);
	        Console.WriteLine(s);
        }
    }
}