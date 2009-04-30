using System.Text;
using System;
using org.diracvideo.Math;
//import java.awt.Dimension;
namespace org.diracvideo.Jirac
{


    public class ColourSpace {
        private VideoFormat format;
        int[][] rgb_table, matrix;
        public ColourSpace(int colourmode, VideoFormat fmt) {
	    format = fmt;
	    SetupTables();
        }

        private void SetupTables() {
            matrix = new int[3][];
            
            for (int i = 0; i < 3; i++)
                matrix[i] = new int[3];

            rgb_table = new int[9][];
            for (int j = 0; j < 9; j++)
                rgb_table[j] = new int[255];
	        SetupMatrix(0.2990, 0.1140);
	        for(int c = 0; c < 3; c++) {
	            for(int i = 0; i < 255; i++) {
		        rgb_table[3*c][i] = (matrix[0][c]*i)>>16;
		        rgb_table[3*c+1][i] = (matrix[1][c]*(i-128))>>16;
		        rgb_table[3*c+2][i] = (matrix[2][c]*(i-128))>>16;
	            }
	        }
        }
        
        private void SetupMatrix(double kr, double kb) {
	        double kg = 1.0 - kr - kb;
	        int factor = 1 << 16;
	        matrix[0][0] = matrix[1][0] = matrix[2][0] = factor;
	        matrix[0][2] = (int)(2*(1-kr)*factor);
	        matrix[1][1] = (int)((-2*kb*(1-kb)/kg)*factor);
	        matrix[1][2] = (int)((-2*kr*(1-kr)/kg)*factor);
	        matrix[2][1] = (int)(2*(1-kb)*factor);
        }



        public int Clamp(int y) {
	        return Util.Clamp(y+128,0,255);
        }

        public void Convert(Block[] yuv, ref int[] rgb) {
	        short[] Y = yuv[0].d, U = yuv[1].d, V = yuv[2].d;
	        int xShift = format.ChromaHShift(), yShift = format.ChromaVShift();
	        Dimension lum = yuv[0].s;
	        for(int y = 0; y < lum.Height; y++) {
	            int yLine = yuv[0].Line(y);
	            int uvLine = yuv[1].Line(y >> yShift);
	            int rgbLine = y*lum.Width;
	            for(int x = 0; x < lum.Width; x++) 
		            rgb[rgbLine+x] = Clamp(Y[yLine+x])*0x010101;
	        }
        }

        public override string ToString() {
	        StringBuilder sb = new StringBuilder();
	        sb.Append("org.diracvideo.Jirac.ColourSpace");
	        for(int i = 0; i < 3; i++) {
	            sb.Append(String.Format("\n%d\t%d\t%d", matrix[i][0],
				            matrix[i][1], matrix[i][2]));
	        }
	        return sb.ToString();
        }
    }
}