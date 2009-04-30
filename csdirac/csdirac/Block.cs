//import java.awt.Point;
//import java.awt.Dimension;
using org.diracvideo.Math;
using System;

namespace org.diracvideo.Jirac
{


    /** Block
     *
     * Has methods for getting the correct positions
     * of elements in the data array. I would call it
     * Frame but that conflicts with java.awt.Frame. */

    public class Block : IEquatable<Block> {
        public short[] d;
        public Point p;
        public Dimension s, o;
        /** Default Block constructor 
         * 
         * @param d is the data of the frame
         * @param p is the place where the frame should start
         * @param s is the dimension of the frame
         * @param o is the dimension of the outer frame **/
        
        public Block(short[] d, Point p, Dimension s, Dimension o) {
	        this.s = s;
	        this.d = d;
	        this.p = p;
	        this.o = o;
        }
        /**
         * Creates a Block consisting of the entire frame.
         *
         * @param width is the width of the frame
         * @param d is the data of the frame **/

        public Block(short[] d, int width) {
	        this.d = d;
	        this.p = new Point(0,0);
	        this.s = this.o = new Dimension(width, d.Length / width);
        }
        
        /**
         * Creates a Block with dimension d namespace a 
         * newly allocated array for the frame **/
        public Block(Dimension d) {
	        this.d = new short[d.Width * d.Height];
	        this.o = this.s = d;
	        this.p = new Point(0,0);
        }

        public Block Sub(Point off, Dimension sub) {
	        Point pnt = new Point(p.X + off.X, p.Y + off.Y);
	        return new Block(d, pnt, sub, o);
        }

        /** @return the index to the start of the frame **/
        public int Start() {
	        return (p.Y*o.Width) + p.X;
        }

        /** @return the index to the end of the frame (last element + 1) **/
        public int End() {
            return Line(s.Height - 1) + s.Width;
        }
        
        /** @return the line of the frame with index n **/
        public int Line(int n) {
	        return (n+p.Y)*o.Width + p.X;
        }

        /** The index of a point
         * @return the index for a general point in the frame **/
        public int Index(int x, int y) {
            return Line(y) + x;
        }

        /** Pixel at a given point, unchecked */
        public short Pixel(int x, int y) {
	        return d[(y + p.Y)*o.Width + (p.X + x)];
        }

        /** Pixel at a given point, checked */
        public short Real(int x, int y) {
            return Pixel(Util.Clamp(x, 0, s.Width - 1),
		         Util.Clamp(y, 0, s.Height - 1));
        }

        public void Set(int x, int y, short v) {
	        d[(y+p.Y)*o.Width + (p.X + x)] = v;
        }
        
        public void Set(int x, int y, int v) {
	        Set(x, y, (short)v);
        }

        public void AddTo(Block o) {
	        int height = System.Math.Min(s.Height, o.s.Height);
	        int width = System.Math.Min(s.Width, o.s.Width);
	        for(int y = 0; y < height; y++) {
	            int a = Line(y);
		        int b = o.Line(y);
	            for(int x = 0; x < width; x++) {
		        o.d[b+x] += d[a+x];
	            }
	        }
        }

        /** upsample a block
         * block should be `real'
         * @return the upsampled block
         * @see the dirac specification section 15.8.11 */
        public Block UpSample() {
	        Block r = new Block(new Dimension(2*s.Width, 2*s.Height));
	        short[] taps = new short[] {21, -7, 3, -1};
	        for(int y = 0; y < s.Height - 1; y++) { /* vertical upsampling */
	            for(int x = 0; x < s.Width - 1; x++) {
		        r.Set(x*2, y*2, Pixel(x,y)); /* the copying part */
	            }
	            for(int x = 0; x < s.Width - 1; x++) {
		        short val = 16;
		        for(int i = 0; i < 4; i++) {
                    val += (short)(taps[i] * Pixel(x, System.Math.Max(0, y - i)));
                    val += (short)(taps[i] * Pixel(x, System.Math.Min(s.Height - 1, y + i)));
		        }
		        r.Set(x*2, y*2 + 1, (short)(val >> 5));
	            }
	        }
	        for(int y = 0; y < s.Height - 1; y++) {
	            for(int x = 0; x < s.Width - 1; x++) {
		        short val = 16;
		        for(int i = 0; i < 4; i++) {
                    val += (short)(taps[i] * Pixel(System.Math.Max(0, x - i), y));
                    val += (short)(taps[i] * Pixel(System.Math.Min(x + i, s.Width - 1), y));
		        }
		        r.Set(x*2 + 1, y*2, (short) (val >> 5));
		        val = 16;
		        for(int i = 0; i < 4; i++) {
                    int xdown = System.Math.Max(0, (x - i) * 2);
                    int xup = System.Math.Min(2 * s.Width - 2, (x + i) * 2);
		            val += (short)(taps[i]*r.Pixel(xdown, y*2+1));
		            val += (short)(taps[i]*r.Pixel(xup, y*2+1));
		        }
		        r.Set(x*2 + 1, y*2 + 1, (short) (val >> 5));
	            }
	        }
	        return r;
        }

        public void ShiftOut(int b, int a) {
	        for(int y = 0; y < s.Height; y++) {
	            int line = Line(y);
	            for(int x = 0; x < s.Width; x++) {
		        d[line + x] += (short)a;
		        d[line + x] >>= b;
	            }
	        }
        }

        public void Clip(int b) {
	        int l = -(1 << b), h = (1 << b) - 1;
	        for(int y = 0; y < s.Height; y++) {
	            int line = Line(y);
	            for(int x = 0; x < s.Width; x++) 
		        d[line+x] = (short)Util.Clamp(d[line+x], l, h);
	        }
        }

        /** A test method which fills the block with a checkers pattern 
         * @param m the size of the blocks  */
        public void Checkers(int m) {
	        m = (1 << m);
	        for(int i = 0; i < s.Height; i++) {
	            for(int j = 0; j < s.Width; j++) {
		        Set(i,j,(short)(((i&m)^(j&m))*255));
	            }
	        }
        }

        /** a method to test for the equality of two blocks
         * Two blocks are found to be equal if each of their
         * points are equal */
        public bool Equals(Block o) {
	        if(s.Width != o.s.Width)
	            return false;
	        if(s.Height != o.s.Height) 
	            return false;
	        for(int i = 0; i < s.Height; i++)
	            for(int j = 0; j < s.Width; j++) 
		        if(Pixel(i,j) != o.Pixel(i,j))
		            return false;
	        return true;
        }
    }
}