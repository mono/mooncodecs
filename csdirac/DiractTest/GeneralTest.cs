using org.diracvideo.Jirac;
using NUnit.Framework;
using System;
using org.diracvideo.Math;
namespace org.diracvideo.Jirac.Test
{
    [TestFixture]
    public class GeneralTest {

        [Test]
        private void testColourSpace() {
	        ColourSpace col = new ColourSpace(0,null);
	        Console.WriteLine(col);
        }
        [Test]
        public void testShift()
        {
	        int i = 16;
	        Console.WriteLine("%d %d %d\n",  i >> 0, i >> 1, i >> 2);
        }
        [Test]
        public void testShort()
        {
            //OD: removed
	        /*int s = (short)(0x8000);
	        Console.WriteLine("%d %d %d\n", s, s >> 2, s >> 2);
             */
	        int i = 0x8000;
	        Console.WriteLine("%d %d %d\n", i, i >> 2, i >> 2);
        }
        [Test]
        public void testLevel()
        {
	        for(int n = 1; n < 8; n++) {
	            Console.WriteLine("Testing with TransformDepth = %d\n", n);
	            for(int i = 0; i < 3*n+1; i++) {
		        Console.WriteLine("Number: %d\tLevel: %d\tOrient: %d\n", 
				          i, (i-1)/3, (i-1) % 3 + 1);
	            } 
	        }
        }
        [Test]
        public void testDivision()
        {
	        for(int i = 0; i < 1000; i++) {
	            for(int j = 1; j < 1000; j++) {
		            int d = i/j;
		            if(d*j > i) {
		                Console.WriteLine("Divison error");
		            } else if(d*j != i) {
		                Console.WriteLine("Inexact division");
		            }
	            }
	        }
        }
        [Test]
        public void testBlockDimensions()
        {
	        Dimension frame = new Dimension(320,240);
	        for(int numY = 1; numY < 10; numY ++) {
	            for(int numX = 1; numX < 10; numX++) {
		        Console.WriteLine("numX: %d\tnumY: %d\n", numX, numY);
		        Dimension block = new Dimension(frame.Width / numX,
						        frame.Height / numY);
		        for(int i = 0; i < numY; i++)
		            for(int j = 0; j < numX; j++) {
			        int testStart = (block.Width*j) +
			            (frame.Width*block.Height*i);
			        int testEnd = testStart + block.Width +
			            (frame.Width*(block.Height-1));
			        int specX = (frame.Width * j)/numX;
			        int specY = (frame.Height *i)/numY;
			        int specStart = (frame.Width*specY) + specX;
			        int specEndX = (frame.Width * (j+1))/numX;
			        int specEndY = (frame.Height *(i+1))/numY;
			        int specEnd = (frame.Width*(specEndY - 1)) + specEndX;
			        if(specEnd != testEnd ||
			           specStart != testStart) {
			            Console.WriteLine("Spec:\t\tTest\n%d\t\t%d\n",
					              specEnd, testEnd);
			            Console.WriteLine("%d\t\t%d\n", specStart,
					              testStart);
			        }
		            }
	            }
	        }
        }
    }
}