namespace org.diracvideo.Jirac
{


    internal class Context {

        public const short  
	    ZERO_CODEBLOCK = 0,
	    QUANTISER_CONT = 1,
	    QUANTISER_VALUE = 2,
	    QUANTISER_SIGN = 3,
	    ZPZN_F1 = 4,
	    ZPNN_F1 = 5,
	    ZP_F2 = 6,
	    ZP_F3 = 7,
	    ZP_F4 = 8,
	    ZP_F5 = 9,
	    ZP_F6p = 10,
	    NPZN_F1 = 11,
	    NPNN_F1 = 12,
	    NP_F2 = 13,
	    NP_F3 = 14,
	    NP_F4 = 15,
	    NP_F5 = 16,
	    NP_F6p = 17,
	    SIGN_POS = 18,
	    SIGN_NEG = 19,
	    SIGN_ZERO = 20,
	    COEFF_DATA = 21,
	    SB_F1 = 22,
	    SB_F2 = 23, 
	    SB_DATA = 24,
	    BLOCK_MODE_REF1 = 25,
	    BLOCK_MODE_REF2 = 26,
	    GLOBAL_BLOCK = 27,
	    LUMA_DC_CONT_BIN1 = 28,
	    LUMA_DC_CONT_BIN2 = 29,
	    LUMA_DC_VALUE = 30,
	    LUMA_DC_SIGN = 31,
	    CHROMA1_DC_CONT_BIN1 = 32,
	    CHROMA1_DC_CONT_BIN2 = 33,
	    CHROMA1_DC_VALUE = 34,
	    CHROMA1_DC_SIGN = 35,
	    CHROMA2_DC_CONT_BIN1 = 36,
	    CHROMA2_DC_CONT_BIN2 = 37,
	    CHROMA2_DC_VALUE = 38,
	    CHROMA2_DC_SIGN = 39,
	    MV_REF1_H_CONT_BIN1 = 40,
	    MV_REF1_H_CONT_BIN2 = 41,
	    MV_REF1_H_CONT_BIN3 = 42,
	    MV_REF1_H_CONT_BIN4 = 43,
	    MV_REF1_H_CONT_BIN5 = 44,
	    MV_REF1_H_VALUE = 45,
	    MV_REF1_H_SIGN = 46,
	    MV_REF1_V_CONT_BIN1 = 47,
	    MV_REF1_V_CONT_BIN2 = 48,
	    MV_REF1_V_CONT_BIN3 = 49,
	    MV_REF1_V_CONT_BIN4 = 50,
	    MV_REF1_V_CONT_BIN5 = 51,
	    MV_REF1_V_VALUE = 52,
	    MV_REF1_V_SIGN = 53,
	    MV_REF2_H_CONT_BIN1 = 54,
	    MV_REF2_H_CONT_BIN2 = 55,
	    MV_REF2_H_CONT_BIN3 = 56,
	    MV_REF2_H_CONT_BIN4 = 57,
	    MV_REF2_H_CONT_BIN5 = 58,
	    MV_REF2_H_VALUE = 59,
	    MV_REF2_H_SIGN = 60,
	    MV_REF2_V_CONT_BIN1 = 61,
	    MV_REF2_V_CONT_BIN2 = 62,
	    MV_REF2_V_CONT_BIN3 = 63,
	    MV_REF2_V_CONT_BIN4 = 64,
	    MV_REF2_V_CONT_BIN5 = 65, 
	    MV_REF2_V_VALUE = 66,
	    MV_REF2_V_SIGN = 67,
	    LAST = 68;
    }


    public class Arithmetic {
        private int offset, size, code, low, range, cntr;
        private byte[] data;
        private byte shift;
        private int[] probabilities;
        private short[] lut;
        /* static lookup tables */
        private readonly static short[] LUT = new short[] {
	    0,    2,    5,    8,   11,   15,   20,   24,
	    29,   35,   41,   47,   53,   60,   67,   74,
	    82,   89,   97,  106,  114,  123,  132,  141,
	    150,  160,  170,  180,  190,  201,  211,  222,
	    233,  244,  256,  267,  279,  291,  303,  315,
	    327,  340,  353,  366,  379,  392,  405,  419,
	    433,  447,  461,  475,  489,  504,  518,  533,
	    548,  563,  578,  593,  609,  624,  640,  656,
	    672,  688,  705,  721,  738,  754,  771,  788,
	    805,  822,  840,  857,  875,  892,  910,  928,
	    946,  964,  983, 1001, 1020, 1038, 1057, 1076,
	    1095, 1114, 1133, 1153, 1172, 1192, 1211, 1231,
	    1251, 1271, 1291, 1311, 1332, 1352, 1373, 1393,
	    1414, 1435, 1456, 1477, 1498, 1520, 1541, 1562,
	    1584, 1606, 1628, 1649, 1671, 1694, 1716, 1738,
	    1760, 1783, 1806, 1828, 1851, 1874, 1897, 1920,
	    1935, 1942, 1949, 1955, 1961, 1968, 1974, 1980,
	    1985, 1991, 1996, 2001, 2006, 2011, 2016, 2021,
	    2025, 2029, 2033, 2037, 2040, 2044, 2047, 2050,
	    2053, 2056, 2058, 2061, 2063, 2065, 2066, 2068,
	    2069, 2070, 2071, 2072, 2072, 2072, 2072, 2072,
	    2072, 2071, 2070, 2069, 2068, 2066, 2065, 2063,
	    2060, 2058, 2055, 2052, 2049, 2045, 2042, 2038,
	    2033, 2029, 2024, 2019, 2013, 2008, 2002, 1996,
	    1989, 1982, 1975, 1968, 1960, 1952, 1943, 1934,
	    1925, 1916, 1906, 1896, 1885, 1874, 1863, 1851,
	    1839, 1827, 1814, 1800, 1786, 1772, 1757, 1742,
	    1727, 1710, 1694, 1676, 1659, 1640, 1622, 1602,
	    1582, 1561, 1540, 1518, 1495, 1471, 1447, 1422,
	    1396, 1369, 1341, 1312, 1282, 1251, 1219, 1186,
	    1151, 1114, 1077, 1037,  995,  952,  906,  857,
	    805,  750,  690,  625,  553,  471,  376,  255
        };

        private readonly static int[] next_list = new int[] {
	        0, Context.QUANTISER_CONT,0,0,
	        Context.ZP_F2, Context.ZP_F2,
	        Context.ZP_F3, Context.ZP_F4,
	        Context.ZP_F5, Context.ZP_F6p,
	        Context.ZP_F6p,	Context.NP_F2,
	        Context.NP_F2, Context.NP_F3,
	        Context.NP_F4, Context.NP_F5,
	        Context.NP_F6p,	Context.NP_F6p,
	        0, 0, 0, 0,Context.SB_F2,
	        Context.SB_F2, 0, 0, 0,	0,
	        Context.LUMA_DC_CONT_BIN2,
	        Context.LUMA_DC_CONT_BIN2,
	        0, 0, Context.CHROMA1_DC_CONT_BIN2,
	        Context.CHROMA1_DC_CONT_BIN2,
	        0, 0, Context.CHROMA2_DC_CONT_BIN2,
	        Context.CHROMA2_DC_CONT_BIN2,
	        0, 0, Context.MV_REF1_H_CONT_BIN2,
	        Context.MV_REF1_H_CONT_BIN3,
	        Context.MV_REF1_H_CONT_BIN4,
	        Context.MV_REF1_H_CONT_BIN5,
	        Context.MV_REF1_H_CONT_BIN5,
	        0, 0, Context.MV_REF1_V_CONT_BIN2,
	        Context.MV_REF1_V_CONT_BIN3,
	        Context.MV_REF1_V_CONT_BIN4,
	        Context.MV_REF1_V_CONT_BIN5,
	        Context.MV_REF1_V_CONT_BIN5,
	        0, 0, Context.MV_REF2_H_CONT_BIN2,
	        Context.MV_REF2_H_CONT_BIN3,
	        Context.MV_REF2_H_CONT_BIN4,
	        Context.MV_REF2_H_CONT_BIN5,
	        Context.MV_REF2_H_CONT_BIN5,
	        0, 0, Context.MV_REF2_V_CONT_BIN2,
	        Context.MV_REF2_V_CONT_BIN3,
	        Context.MV_REF2_V_CONT_BIN4,
	        Context.MV_REF2_V_CONT_BIN5,
	        Context.MV_REF2_V_CONT_BIN5,
	        0, 0, 0
            };

        public Arithmetic(Buffer b) {
	        data = b.d;
	        offset = b.s;
	        size = b.e;
	        DecodeInit();
        }

        public Arithmetic(byte[] d)
            : this(new Buffer(d))
        {
        }

        private void DecodeInit() {
	        cntr = 0;
	        low = 0;
	        range = 0xffff;
	        if(size - offset > 1) {
	            code = ((data[offset]&0xff)<<8) | (data[offset+1]&0xff);
		        } else if(size - offset > 0) {
	            code = ((data[offset]&0xff)<<8) | 0xff;
	        } else {
	            code = 0xffff;
	        }
	        offset += 2;
	        shift = (size - offset > 0 ? data[offset] : (byte)0xff);
	        probabilities = new int[Context.LAST];
	        lut = new short[512];
	        for(int i = 0; i < Context.LAST; i++) {
	            probabilities[i] = 0x8000;
	        }
	        for(int i = 0; i < 256; i++) {
	            lut[i] = Arithmetic.LUT[255-i];
	            lut[256+i] = (short)(-Arithmetic.LUT[i]);
	        }
        }

        public int DecodeUint(int cont, int val) {
	        int v = 1;
	        while(!DecodeBool(cont)) {
	            cont = Arithmetic.next_list[cont];
	            v = (v << 1) | DecodeBit(val);
	        }
	        return v-1;
        }

        public int DecodeSint(int cont, int val, int sign) {
	        int v = DecodeUint(cont, val);
	        return (v == 0 || DecodeBool(sign)) ? -v : v;
        }

        public int DecodeBit(int context) {
	        return (DecodeBool(context) ? 1 : 0);
        }

        public bool DecodeBool(int context) {
	        bool v;
	        int range_times_prob, lut_index;
	        range_times_prob =
	            (range * probabilities[context]) >> 16;
	        v = (code - low >= range_times_prob);
	        lut_index = probabilities[context] >> 8 | (v ? 256 : 0);
	        probabilities[context] += lut[lut_index];
	        if(v) {
	            low += range_times_prob;
	            range -= range_times_prob;
	        } else {
	            range = range_times_prob;
	        }

	        while(range <= 0x4000) {
	            low <<= 1;
	            range <<= 1;
	            code <<= 1;
	            code |= (shift >> (7-cntr))&1;
	            cntr++;
	            if(cntr == 8) {
		        offset++;
		        if(offset < size) {
		            shift = data[offset];
		        } else {
		            shift = (byte)0xff;
		        }
		        low &= 0xffff;
		        code &= 0xffff;
		        if(code < low) {
		            code |= (1<<16);
		        }
		        cntr = 0;
	            }
	        }
	        return v;
        }

        /* an estimate of how many bits there are left */
        public int BytesLeft() { 
	        return (size - offset);
        }
    }
}