//import java.lang.Math;
using System.Text;
using System;
namespace org.diracvideo.Jirac
{


    public class Unpack : IEquatable<Unpack>{
        private byte[] data; /* data array */
        private int i = 0;   /* index to next byte in array to be shifted
                                into shift register */
        private int r;       /* shift register */
        private int l = 0;   /* number of bits in shift register */
        private int s;       /* number of bytes in array */
        private int guard_bit; /* guard bit */

        public Unpack(byte[] d)
            : this(d, 0, d.Length)
        {
    
        }

        public Unpack(byte[] d, int b, int e) {
	        this.data = d;
	        this.i = b;
	        this.s = e;
	        ShiftIn();
        }
        
        public Unpack(Buffer b) : this(b.d, b.s, b.e) {
        }
    	    
        private  void ShiftIn() {
	        for(; l <= 24 && i < s; l += 8) {
	            r |= (data[i++]&0xff) << (24-l);
	        }
	        /* shift in guard bits.  FIXME guard bit might be 0. */
	        if (i == s) {
	            for(; l <= 24; l += 8) {
	                r |= 0xff << (24-l);
	            }
	        }
        }

        private  int ShiftOut(int n) {
            if (n > l) {
                throw new Exception("shifting out too many bits");
	        }
            if (n == 32) {
	            int v;
	            v = r;
	            l = 0;
	            r = 0;
	            return v;
	        } else {
	            int v;
	            v = (r >> (32 - n)) & ((1<<n) - 1);
	            l -= n;
	            r <<= n;
	            return v;
	        }
        }
        
        public void Align() {
	        r <<= (l & 7);
	        l -= (l & 7);
	        ShiftIn();
        }

        public int DecodeLit32() {
	        int v;
	        switch(l) {
	        case 0:
	        case 8:
	        case 16:
	        case 24:
	            ShiftIn();
                return Bits(32);
	        case 32:
	            v = r;
	            r = 0;
	            l = 0;
	            return v;
	        default:
	            return Bits(32);
	        }
        }

        /** bits
         * @param n number of bits to be decoded
         *
         * Decodes a number of bits from the input buffer.
         * Does not (generally) work when there are 32 bits left 
         * in the shift register (i.e. the shift register is full.
         * Therefore, use decodeLit32() for a literal 32 bit integer. */

        public int Bits(int n) {
            if (n < 0) 
                throw new Exception("n < 0");
            if (n > 32) 
                throw new Exception("n > 32");

	        if (n == 0) 
                return 0;
	        if (n > l) {
	            int t = l;
	            int v = ShiftOut(t) << (n - t);
	            ShiftIn();
	            return v | ShiftOut(n - t);
	        } 
	        return ShiftOut(n);
        }
        
        /** skip:
         * @param n number of bits to be skipped
         * 
         * This function is known not to work 100% correctly when given
         * a non-multiple-of-8 number of bits when it is not aligned.
         * Unfortunately, I have no idea how to fix it. */
        public void Skip(int n) { 
            if (n == 0) 
                return;
	        if(n < 32) {
	            Bits(n);
	        } else {
	            n -= l;
	            l = r = 0;
	            i += System.Math.Min(s - i - 1, n >> 3);
	            if(i < s - 1 && (n & 7) != 0) {
		        ShiftIn();
		        ShiftOut(n & 7);
	            }
	        }
	        /*	/b\ werk videoplayer, werk!!!   */
        }
        

        /** decodeUint:
         *
         * Decodes an exp-golomb encoded integer from the buffer. */
        public int DecodeUint() {
	        int v = 1;
	        while(Bits(1) == 0) {
	            v = (v << 1) | Bits(1);
	        }
	        return v-1;
        }
        
        public int DecodeSint() {
	        int v = DecodeUint();
	        return (v == 0 || Bits(1) == 0) ? v : -v;
        }

        public short DecodeSint(int qf, int qo) {
	        int m = DecodeUint();
	        if(m == 0) {
	            return (short)((qo + 2) >> 2);
	        } else {
	            m = (short)((m * qf + qo + 2)>>2);
	            return (short)((Bits(1) == 0) ? m : -m);
	        }
        }

        public int BitsLeft() {
	        return (s - i) * 8 + l;
        }

        /** bitsRead:
         *
         * Returns the read number of bits.
         * It assumes, which is not generally true,
         * that i was zero at initialization (that is,
         * we've read bits from the beginning of the buffer. */
        public int BitsRead() {
	        return i*8 - l;
        }
        
        public bool DecodeBool() {
	        return Bits(1) == 1;
        }

        /** getSubBuffer:
         * @param bytes length of sub buffer in bytes
         * 
         * Aligns current structure, returns a buffer starting at
         * the current byte to be read, and advances the index to
         * after the end of the taken subbuffer. Thus, destructive.
         */
        public Buffer GetSubBuffer(int bytes) {
	        Align();
	        int start = i - l/8;
	        Buffer buf = new Buffer(data, start, start + bytes);
	        Skip(bytes*8 + (l & 7));
	        return buf;
        }
        
        public bool Equals(Unpack u) {
	        bool same = true;
	        same = (same && u.BitsRead() == BitsRead());
	        same = (same && u.BitsLeft() == BitsLeft());
	        same = (same && u.data == data);
	        same = (same && Check() && u.Check());
	        return same;
        }

        public bool Check() {
	        int t = 0;
	        if (l == 0) {
	            return r == 0;
	        }
	        for(int j = -4; j < 0; j++) {
	            t = (t << 8) | (data[i+j]&0xff);
	        }
	        t <<= (32 - l);
	        return t == r;
        }

        public Unpack Clone() {
	        Unpack n = new Unpack(data);
	        n.i = this.i;
	        n.s = this.s;
	        n.l = this.l;
	        n.r = this.r;
	        return n;
        } 

        public override string ToString() {
	        StringBuilder b = new StringBuilder();
	        b.Append(String.Format("Register: %08X\n", r));
	        b.Append(String.Format("Bits left: %d\tIndex: %d", l,i));
	        return b.ToString();
        }
    }
}