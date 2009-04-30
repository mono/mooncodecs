//import java.awt.Dimension;
using System;
using System.Text;
using org.diracvideo.Math;
namespace org.diracvideo.Jirac
{


    internal class Parameters {
        /* static array of wavelets */ 
        private static Wavelet[] wavs = {
	        new DeslauriesDebuc9_7(),
	        new LeGall5_3(),
	        new DeslauriesDebuc13_7(),
	        new HaarNoShift(),
	        new HaarSingleShift(),
	        new Fidelity(),
	        new Daubechies9_7(),
	        new Wavelet()
        };
        
        /* Wavelet transform parameters */
        public int transformDepth = 4, wavelet_index;
        public int codeblock_mode_index = 0;
        public bool no_ac, is_ref, is_lowdelay, is_intra;
        public int[] horiz_codeblocks = new int[7],
	    vert_codeblocks = new int[7];
        public int num_refs;
        public Dimension iwtLumaSize, iwtChromaSize;
        /* Motion prediction parametrs */
        public int xblen_luma, yblen_luma, 
	    xbsep_luma, ybsep_luma;
        public int x_num_blocks, y_num_blocks,
	    x_offset, y_offset;
        public bool have_global_motion;
        public int picture_prediction_mode, mv_precision;
        public int picture_weight_bits = 1, 
	    picture_weight_1 = 1,picture_weight_2 = 1;
        public Global[] global = new Global[2];

        public Parameters(int c) {
	    no_ac = !((c & 0x48) == 0x8);
	    num_refs = (c & 0x3);
	    is_ref = (c & 0x0c) == 0x0c;
	    is_lowdelay = ((c & 0x88) == 0x88);
	    is_intra = (num_refs == 0);
        }

        public void CalculateIwtSizes(VideoFormat format) {
	        int[] size = new int[]{0,0};
	        format.GetPictureLumaSize(ref size);
	        iwtLumaSize = new Dimension(Util.RoundUpPow2(size[0], transformDepth),
				            Util.RoundUpPow2(size[1], transformDepth));
	        format.GetPictureChromaSize(ref size);
	        iwtChromaSize = new Dimension(Util.RoundUpPow2(size[0], transformDepth),
				              Util.RoundUpPow2(size[1], transformDepth));
        }

        public void VerifyBlockParams()  {
	        bool ok = true;
	        ok = ok && xblen_luma >= 0;
	        ok = ok && yblen_luma >= 0;
	        ok = ok && xbsep_luma >= 0;
	        ok = ok && ybsep_luma >= 0;
	        if(!ok) {
	            throw new Exception("Block Paramters incorrect");
	        }
        }

        public void SetBlockParams(int i)
	     {
	        switch(i) {
	        case 1:
	            xblen_luma = yblen_luma = 8;
	            xbsep_luma = ybsep_luma = 4;
	            break;
	        case 2:
	            xblen_luma = yblen_luma = 12;
	            xbsep_luma = ybsep_luma = 8;
	            break;
	        case 3:
	            xblen_luma = yblen_luma = 16;
	            xbsep_luma = ybsep_luma = 12;
	            break;
	        case 4:
	            xblen_luma = yblen_luma = 24;
	            xbsep_luma = ybsep_luma = 16;
	            break;
	        default:
	            throw new Exception("Unsupported Block Parameters index");
	        }

        }

        public void CalculateMCSizes() {
	        x_num_blocks = 4*Util.DivideRoundUp(iwtLumaSize.Width, 4*xbsep_luma);
	        y_num_blocks = 4*Util.DivideRoundUp(iwtLumaSize.Height, 4*ybsep_luma);
	        x_offset = (xblen_luma - xbsep_luma)/2;
	        y_offset = (yblen_luma - ybsep_luma)/2;
        }
        
        public Wavelet GetWavelet() {
	        return wavs[wavelet_index];
        }

        public override string ToString() {
	        StringBuilder sb = new StringBuilder();
	        sb.Append("\nParameters:\n");
	        sb.Append(String.Format("Transform depth: %d\n", transformDepth));
	        sb.Append(String.Format("Using ac: %c\n", (no_ac ? 'n' : 'y')));
	        sb.Append(String.Format("Is ref: %c", (is_ref ? 'y' : 'n')));
	        for(int i = 0; i < transformDepth; i++) {
	            sb.Append(String.Format("\nHorizBlocks: %d\tVertBlocks: %d",
				            horiz_codeblocks[i], vert_codeblocks[i]));
	        }
	        sb.Append(String.Format("\nxblen: %d\t yblen: %d", xblen_luma, yblen_luma));
	        sb.Append(String.Format("\nxbsep: %d\t ybsep: %d", xbsep_luma, ybsep_luma));
	        return sb.ToString();
        }
    }    
}