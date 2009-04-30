namespace org.diracvideo.Jirac
{

    internal class Util {
        public static int RoundUpPow2(int x, int y) {
	    return (((x) + (1<<(y)) - 1)&((~0)<<(y)));
        }

        public static int RoundUpShift(int x, int y) {
	    return (((x) + (1<<(y)) - 1)>>(y));
        }

        public static int DivideRoundUp(int x, int y) {
	    return (x + y - 1)/y;
        }

        public static int RoundShift(int x, int y) {
	    return (((x) + (1<<((y)-1)))>>(y));
        }

        public static int Clamp(int i, int l, int h) {
	    return (i < l ? l : i > h ? h : i);
        }

        public static int Clamp(double i, int l, int h) {
	    int v = (int)(i+0.5);
	    return Clamp(v,l,h);
        }

        public static int Median(int[] a) {
	    if(a[0] < a[1]) {
	        if(a[1] < a[2]) return a[1];
	        if(a[2] < a[0]) return a[0];
	        return a[2];
	    } else {
	        if(a[0] < a[2]) return a[0];
	        if(a[2] < a[1]) return a[1];
	        return a[2];
	    }
        }

        public static int Mean(params int[] numbers) {
	        int s = 0;
	        for(int i = 0; i < numbers.Length; i++)
	            s += numbers[i];
	        return s/numbers.Length;
        }

        public static short GetRamp(int x, int offset) {
	    if(offset == 1) {
	        if(x == 0) return 3;
	        return 5;
	    }
	    return (short)(1 + (6 * x + offset - 1)/(2*offset - 1));
        }
    }

}