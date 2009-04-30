//import java.awt.Dimension;
//import java.awt.Image;
//import java.awt.image.BufferedImage;

using System;
using System.Text;
using org.diracvideo.Math;
namespace org.diracvideo.Jirac
{


    public class Picture {
        private Buffer buf;
        private Wavelet wav;
        private Decoder dec;
        private Parameters par;
        private int code;
        private int[] refs;
        private int[] pixels;
        private int retire;
        private bool zero_residual = false;
        private SubBand[][] coeffs;
        private Block[] iwt_frame, mc_frame;
        private Motion motion;
        private Buffer[] motion_buffers;
        public Decoder.Status status;
        public Exception error = null;
        public  int num;
        private object obj = new object();


        /** Picture:
         * @param b payload buffer
         * @param d decoder of the picture 
         *
         * The b buffer should only point to the payload data section of 
         * the picture (not the header). The only methods that would ever need 
         * to be called are parse(), decode(), and getImage(). However,
         * one should check wether the error variable is set before and after
         * calling a method. One should not call them in any other order than that
         * just specified. Each can be called without arguments and should not be 
         * called twice. */

        public Picture(Buffer b, Decoder d) {
	        num = b.GetInt(13);
	        code = b.GetByte(4);
	        buf = b;
	        dec = d;
	        par = new Parameters(code);
	        coeffs = new SubBand[3][];
            coeffs[0] = new SubBand[19];
            coeffs[1] = new SubBand[19];
            coeffs[2] = new SubBand[19];
	        motion_buffers = new Buffer[9];
	        status = Decoder.Status.NULL;
        }

        public bool Decodable() {
	    if(!par.is_intra)
	        for(int i = 0; i < refs.Length; i++)
		    if(!dec.refs.Has(refs[i])) return false;
	    return true;
        }

        public void Parse() {
            lock(obj)
            {
	            if(status != Decoder.Status.NULL)
	                return;
	            status = Decoder.Status.WAIT;
	            try {
	                Unpack u = new Unpack(buf);
	                u.Skip(136); /* 17 * 8 */
	                ParseHeader(u);
	                par.CalculateIwtSizes(dec.format);
	                if(!par.is_intra) {
		                u.Align();
		                ParsePredictionParameters(u);
		                par.CalculateMCSizes();
		                u.Align();
		                ParseBlockData(u);
	                }
	                u.Align();
	                if(!par.is_intra) {
		                zero_residual = u.DecodeBool();
	                }
	                if(!zero_residual) {
		                ParseTransformParameters(u);
		                u.Align();
		                if(par.is_lowdelay) {
		                    ParseLowDelayTransformData(u);
		                } else {
		                    ParseTransformData(u);
		                }
	                }
	                status = Decoder.Status.OK;
	            } catch(Exception e) {
	                error = e;
	                status = Decoder.Status.ERROR;
	            }
	            buf = null;
            }
        }

        private void ParseHeader(Unpack u)  {
	        u.Align();
	        if(!par.is_intra) {
	            refs = new int[par.num_refs];
	            for(int i = 0; i < par.num_refs; i++)
		        refs[i] = num + u.DecodeSint();
	        }
	        if(par.is_ref) 
	            retire = u.DecodeSint();
        }
        
        private void ParsePredictionParameters(Unpack u)  {
	        int index = u.DecodeUint();
	        if(index == 0) {
	            par.xblen_luma = u.DecodeUint();
	            par.yblen_luma = u.DecodeUint();
	            par.xbsep_luma = u.DecodeUint();
	            par.ybsep_luma = u.DecodeUint();
	            par.VerifyBlockParams();
	        } else {
	            par.SetBlockParams(index);
	        }
	        par.mv_precision = u.DecodeUint();
	        if(par.mv_precision > 3) {
	            throw new Exception("mv_precision greater than supported");
	        }
	        par.have_global_motion = u.DecodeBool();
	        if(par.have_global_motion) {
	            for(int i = 0; i < par.num_refs; i++) {
		            Global gm = par.global[i];
		            if(u.DecodeBool()) {
		                gm.b0 = u.DecodeSint();
		                gm.b1 = u.DecodeSint();
		            } else {
		                gm.b0 = gm.b1 = 0;
		            }
		            if(u.DecodeBool()) {
		                gm.a_exp = u.DecodeUint();
		                gm.a00 = u.DecodeSint();
		                gm.a01 = u.DecodeSint();
		                gm.a10 = u.DecodeSint();
		                gm.a11 = u.DecodeSint();
		            } else {
		                gm.a_exp = 0;
		                gm.a00 = gm.a11 = 1;
		                gm.a10 = gm.a01 = 0;
		            }
		            if(u.DecodeBool()) {
		                gm.c_exp = u.DecodeUint();
		                gm.c0 = u.DecodeSint();
		                gm.c1 = u.DecodeSint();
		            } else {
		                gm.c_exp = gm.c0 = gm.c1 = 0;
		            }
	            }
	        }
	        par.picture_prediction_mode = u.DecodeUint();
	        if(par.picture_prediction_mode != 0) {
	            throw new Exception("Unsupported picture prediction mode");
	        }
	        if(u.DecodeBool()) {
	            par.picture_weight_bits = u.DecodeUint();
	            par.picture_weight_1 = u.DecodeSint();
	            if(par.num_refs > 1) {
		            par.picture_weight_2 = u.DecodeSint();
	            }
	        }
        }

        private void ParseBlockData(Unpack u)  {
	        for(int i = 0; i < 9; i++) {
	            if(par.num_refs < 2 && (i == 4 || i == 5))
		        continue;
	            int l = u.DecodeUint();
	            motion_buffers[i] = u.GetSubBuffer(l);
	        }
        }

        private void ParseTransformParameters(Unpack u)  {
	        par.wavelet_index = u.DecodeUint();
	        wav = par.GetWavelet();
	        par.transformDepth = u.DecodeUint();
	        if(!par.is_lowdelay) {
	            if(u.DecodeBool()) {
		        for(int i = 0; i < par.transformDepth + 1; i++) {
		            par.horiz_codeblocks[i] = u.DecodeUint();
		            par.vert_codeblocks[i] = u.DecodeUint();
		        }
		        par.codeblock_mode_index = u.DecodeUint();
	            } else {
		        for(int i = 0; i < par.transformDepth + 1; i++) {
		            par.horiz_codeblocks[i] = 1;
		            par.vert_codeblocks[i] = 1;
		        }
	            }
	        } else {
	            throw new Exception("Unhandled stream type");
	        }
        }

        private void ParseTransformData(Unpack u)  {
	        int q,l;
	        Buffer b;
	        for(int c = 0; c < 3; c++) {
	            for(int i = 0; i < 1+3*par.transformDepth; i++) {
		        u.Align();
		        l = u.DecodeUint();
		        if( l != 0) {
		            q = u.DecodeUint();
		            b = u.GetSubBuffer(l);
		        } else {
		            q = 0;
		            b = null;
		        }
		        coeffs[c][i] = new SubBand(b,q,par);
		        coeffs[c][i].CalculateSizes(i, c == 0);
	            }
	        }
        }


        private void ParseLowDelayTransformData(Unpack u) {
	        System.Console.WriteLine("parseLowDelayTransformData()");
        }

        /** synchronized decoding
         *
         * Decodes the picture. */
        public void Decode() {
            lock (obj)
            {
                if (status != Decoder.Status.OK)
                    return;
                status = Decoder.Status.WAIT;
                if (!zero_residual)
                {
                    InitializeIwtFrames();
                    DecodeWaveletTransform();
                }
                if (!par.is_intra)
                {
                    InitializeMCFrames();
                    DecodeMotionCompensate();
                    if (zero_residual)
                        iwt_frame = mc_frame;
                    else for (int i = 0; i < 3; i++)
                            mc_frame[i].AddTo(iwt_frame[i]);
                }
                if (par.is_ref)
                {
                    if (retire != 0)
                        dec.refs.Remove(retire + num);
                    dec.refs.Add(num, iwt_frame);
                }
                CreateImage();
                status = Decoder.Status.DONE;
            }
        }

        
        private void DecodeWaveletTransform() {
	        for(int c = 0; c < 3; c++) {
	            short[] res = iwt_frame[c].d;
	            coeffs[c][0].DecodeCoeffs(ref res);
	            coeffs[c][0].IntraDCPredict(ref res);
	            for(int i = 0; i < par.transformDepth; i++) {
		            coeffs[c][3*i+1].DecodeCoeffs(ref res);
		            coeffs[c][3*i+2].DecodeCoeffs(ref res);
		            coeffs[c][3*i+3].DecodeCoeffs(ref res);
	            } 
	            wav.Inverse(iwt_frame[c], par.transformDepth);  
	        }
        }

        private void DecodeMotionCompensate() {
	        Block[][] frames = new Block[par.num_refs][];
	        bool upscaled = par.mv_precision > 0;
	        for(int i = 0; i < par.num_refs; i++) 
	            frames[i] = dec.refs.Get(refs[i],upscaled);
	        motion = new Motion(par, motion_buffers, frames);
	        motion.Decode();
	        motion.Render(mc_frame, dec.format);
        }


        private void InitializeIwtFrames() {
	        iwt_frame = new Block[3];
	        iwt_frame[0] = new Block(par.iwtLumaSize);
	        iwt_frame[1] = new Block(par.iwtChromaSize);
	        iwt_frame[2] = new Block(par.iwtChromaSize);
        }
        
        private void InitializeMCFrames() {
	        Dimension luma = new Dimension(dec.format.width, dec.format.height);
	        Dimension chro = new Dimension(dec.format.width >> dec.format.ChromaHShift(),
				               dec.format.height >> dec.format.ChromaVShift());
	        mc_frame = new Block[3];
	        mc_frame[0] = new Block(luma);
	        mc_frame[1] = new Block(chro);
	        mc_frame[2] = new Block(chro);
        }

        private void CreateImage() {
                pixels = new int[dec.format.width * dec.format.height];
	        dec.format.colour.Convert(iwt_frame, ref pixels);
        }

        /** getImage returns the displayable image of the picture
         *
         * Returns a black image when error != null. Does no work
         * when there already is an image created - can be called
         * multiple times. **/
        public int[] GetImage() {
	        if(pixels == null) {
	            CreateImage();
	        }
	        return pixels;
        }

        public override String ToString() {
	        StringBuilder b = new StringBuilder();	   
	        b.Append(String.Format("Picture number: %d with code %02X",
			               num, code));
            b.Append(par.ToString());
	        if(status == Decoder.Status.OK) {
	            if(!par.is_intra) {
                    b.Append(String.Format("\nHas %d reference(s)",
				               par.num_refs));
	            }
	            if(par.is_ref) {
                    b.Append("\nIs a reference");
	            }
	            for(int i = 0; i < par.num_refs; i++) {
                    b.Append(String.Format("\n\treference[%d]: %d", 
				               i, refs[i]));
	            }
	        } else {
                b.Append(String.Format("Picture ERROR: %s", error.ToString()));
	        }
	        return b.ToString();
        }

    }
}
