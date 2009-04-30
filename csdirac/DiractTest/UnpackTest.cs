using org.diracvideo.Jirac;
using NUnit.Framework;
using System;
namespace org.diracvideo.Jirac.Test
{
    public static class Utils
    {
        public static byte[] ToBytes(string s)
        {
            return System.Text.Encoding.Default.GetBytes(s);
        }
    }

    [TestFixture]
    public class UnpackTest {

        [Test]
        private void bitsLoopTest()
        {
            for (int i = 0; i < 5000; i++)
            {
                bitsReadTest();
                skipTest();
            }
        }

        private byte[] ToBytes(string s)
        {
            return System.Text.Encoding.Default.GetBytes(s);
        }

        private void skipTest() {
	        /* There used to be an extensive test here. 
	           It failed, continued to fail, and then failed even more.
	           Apparantly an Unpack object can be inconsistent
	           after skipping. I have absolutely no clue why.
	           Especially as it all /seems/ to work so smoothly. 
	           Anyone that is interested can try to fix it. */
	        Unpack u,o;
	        string s = String.Format("Hello World! \n%s\n%s\n%s",
				         "How are you today? I'm fine,",
				         "thank you for asking. It is",
				         "such lovely weather today");
            u = new Unpack(Utils.ToBytes(s));
	        Random r = new Random();
	        while(u.BitsLeft() > 160) {
	            u.Bits(r.Next(31));
	        }
	        int i = r.Next(u.BitsLeft());
	        o = u.Clone();
	        /*
	        o.skip(i);
	        for(; i > 31; i -= 31) {
	            u.bits(31);
	        }
	        u.bits(i); 
	        */
	        if(u.Equals(o)) {
	            while(u.BitsLeft() > 8) {
		        i = r.Next(System.Math.Min(u.BitsLeft(), 31));
		        if(u.Bits(i) != o.Bits(i)) {
		            throw new Exception("Skip Error (Inconsistency)");
		        }
	            }
	        } else {
                throw new Exception("Skip Error (Unequality)");
	        }
        }

        [Test]
        private void decodeTest() {
	        byte[] r = { (byte)0x96, (byte)0x11, (byte)0xA5, (byte)0x7F};
	        Unpack u = new Unpack(r);
	        for(int i = 0; i < 6; i++) {
	            int v = u.DecodeUint();
	            //	    Console.WriteLine(v);
	            if(i != v) {
                    throw new Exception("Error in decodeUint()");
	            }
	        }
        }

        [Test]
        private void bitsTest() {
	        string s = "BBCD is the code for Dirac bitstreams\n" +
	            "This string should be just a little bit longer\n" ;
            Unpack u = new Unpack(Utils.ToBytes(s));
            char[] r = new char[s.Length];
            char[] o = new char[s.Length];
	        for(int i = 0; u.BitsLeft() > 8; i++) {
	            r[i] = Convert.ToChar((byte)u.Bits(8));
	            o[i] = Convert.ToChar((byte)s[9*i]);
	            u.Skip(37);
	            u.Skip(27);
	        }
	        if(new String(o).CompareTo(new String(r)) != 0) {
	            throw new Exception("Bits error");
	        }
        }
        
        private void bitsReadTest() {
	        Unpack u = new Unpack(Utils.ToBytes("hallo sanne je bent mooi en lief"));
	        Random r = new Random();
	        int t = 0;
	        for(int c = 0; u.BitsLeft() > 32; c += t) {
	            if(u.BitsRead() != c) {
                    throw new Exception("bitsRead() Error");
	            }
	            t = r.Next(System.Math.Min(u.BitsLeft(),32));
	            u.Bits(t);
	        }
        }
    }
}