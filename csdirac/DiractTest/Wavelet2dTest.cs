using org.diracvideo.Jirac;
using System;
using NUnit.Framework;
using System.Text;
namespace org.diracvideo.Jirac.Test
{
    [TestFixture]
    public class Wavelet2dTest {
        private Wavelet wav;

        [SetUp]
        public void main() {
	        wav = new Wavelet();
        }

        [Test]
        public void synthesizeTest()
        {
	        short[] d = new short[16];
	        d[8] = 9;
	        d[12] = 5;
	        d[14] = 2;
	        d[15] = 1;
	        wav.Synthesize(d,4,0,d.Length);
	        wav.Synthesize(d,2,0,d.Length);
	        wav.Synthesize(d,1,0,d.Length);
	        Console.WriteLine(d);
        }

        [Test]
        public void interLeaveTest()
        {
	        short[] ll = new short[36];
	        short[] lh = new short[36];	
	        short[] hl = new short[36];
	        short[] hh = new short[36];
	        fill(ll,1);
	        fill(lh,2);
	        fill(hl,3);
	        fill(hh,4);
	        short[] frame = wav.Interleave(ll,lh,hl,hh,6);
	        //	Console.WriteLine(printable(frame,12));
        }

        [Test]
        public void twoDimensionTest()
        {
	        short[] ll = new short[36];
	        short[] other = new short[36];
	        fill(ll,1);
	        short[] frame = wav.Interleave(ll, other, other, other, 6);
	        wav.Inverse(frame, 12, 1);
	        Console.WriteLine(printable(frame,12));
        }

        private void fill(short[] arr, int v) {
	        short s = (short)v;
	        for(int i = 0; i < arr.Length; i++) {
	            arr[i] = s;
	        }
        }
        
        private String printable(short[] arr, int w) {
	        StringBuilder sb = new StringBuilder();
	        for(int i = 0; i < arr.Length; i++) {
	            sb.Append(String.Format("%c%d", (((i % w) == 0) ? '\n' : ' '),
				            arr[i]));
	        }
	        return sb.ToString();
        }

    }
}