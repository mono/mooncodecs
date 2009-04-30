namespace org.diracvideo.Jirac
{

    /** Buffer
     *
     * Buffer represents a one-dimensional array,
     */
     public class Buffer {
        public  int s,e;
        public  byte[] d;

        public Buffer(byte[] d, int s, int e) {
	    this.s = Util.Clamp(s,0,d.Length);
	    this.e = Util.Clamp(e,s,d.Length);
	    this.d = d;
        }

        public Buffer(byte[] d, int b) : this(d, b, d.Length) { }
	    
        public Buffer(byte[] d) : this(d, 0, d.Length) { }


        /** Get a subbuffer
         * 
         * Parameters are relative to the beginning of the block
         * 
         * @param b begin of block 
         * @param e end of block */

        public Buffer Sub(int b, int e) {
	    return new Buffer(this.d, this.s + b, this.s + e);
        }

        public Buffer Sub(int b) {
	    return new Buffer(this.d, this.s + b, this.e);
        }
        
        public byte GetByte(int i) {
	    return d[i + s];
        }

        public int GetInt(int i) {
	    int r = 0;
	    for(int j = i + s; j < i + s + 4; j++)
	        r = (r << 8) | (d[j]&0xff);
	    return r;
        }

        public int Size() {
	    return e - s;
        }

        public Buffer Cat(Buffer o) {
	    byte[] n = new byte[o.Size() + Size()];
	    System.Array.Copy(this.d, this.s, n, 0, e - s);
        System.Array.Copy(o.d, o.s, n, e - s, o.e - o.s);
	    return new Buffer(n);
        }
    }

}