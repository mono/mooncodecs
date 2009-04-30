//import java.awt.Dimension;

using org.diracvideo.Math;
namespace org.diracvideo.Jirac
{


    internal class SubBand {
        private int qi, level, stride, offset, orient, numX, numY;
        private Buffer buf;
        private Dimension frame, band;
        private Parameters par;

        public SubBand (Buffer b, int q, Parameters p) {
	        par = p;
	        qi = q;
	        buf = b;
        }
        
        public void CalculateSizes(int i, bool luma) {
	        level = (i-1)/3;
	        int shift = (par.transformDepth - level);
	        frame = luma ? par.iwtLumaSize : par.iwtChromaSize;
	        orient = (i - 1) % 3 + 1;
	        stride = (1 << shift);
	        offset = (orient == 0 ? 0 : 
		          (orient == 1 ? stride >> 1 :
		           (orient == 2 ? (frame.Width * stride) >> 1 :
		            (stride + frame.Width * stride) >> 1)));
	        numX = (orient == 0 ? par.horiz_codeblocks[0] :
		        par.horiz_codeblocks[level+1]);
	        numY = (orient == 0 ? par.vert_codeblocks[0] :
		        par.vert_codeblocks[level+1]);
	        band = new Dimension(frame.Width >> shift, frame.Height >> shift);
        }

        /* Maybe we should rewrite this namespace blocks.
         * I'm not sure */
        public void DecodeCoeffs(ref short[] c) {
	        if(buf == null)
	            return;
	        int[] bounds = {0,0,0};
	        if(par.no_ac) {
	            Unpack u = new Unpack(buf);
	            if(numX * numY == 1) {	
		        bounds[1] = c.Length;
		        bounds[2] = frame.Width;
		        DecodeCodeBlock(ref c,u,bounds);
		        return;
	            }
	            for(int y = 0; y < numY; y++) {
		        for(int x = 0; x < numX; x++) {
		            if(u.DecodeBool())
			        continue;
		            if(par.codeblock_mode_index != 0)
			        qi += u.DecodeSint();
		            CalculateBounds(bounds,x,y);
		            DecodeCodeBlock(ref c,u,bounds);
		        }
	        } 
	        } else {
	            Arithmetic a = new Arithmetic(buf);
	            if(numX * numY == 1) {
		        bounds[1] = c.Length;
		        bounds[2] = frame.Width;
		        DecodeCodeBlock(ref c, a, bounds);
		        return;
	            }
	            for(int y = 0; y < numY; y++) {
		            for(int x = 0; x < numX; x++) {
		                if(a.DecodeBool(Context.ZERO_CODEBLOCK))
			            continue;
		                if(par.codeblock_mode_index != 0)
			                qi += a.DecodeSint(Context.QUANTISER_CONT,
					               Context.QUANTISER_VALUE,
					               Context.QUANTISER_SIGN);
		                CalculateBounds(bounds, x, y);
		                DecodeCodeBlock(ref c, a, bounds);
		            }
	            } 
	        }
        }


        private void DecodeCodeBlock(ref short[] res, Unpack u,
				     int[] bounds) {
	        int qo = QuantOffset(qi);
	        int qf = QuantFactor(qi);
	        for(int i = bounds[0] + offset; i < bounds[1];
	            i += frame.Width * stride) {
	            for(int j = i; j < i + bounds[2]; j += stride) {
		        res[j] = u.DecodeSint(qf,qo);
	            }
	        }
        }

        private void CalculateBounds(int[] bounds, int blockX, int blockY) {
	        int shift = par.transformDepth - level;
	        int startX = ((band.Width * blockX)/numX) << shift;
	        int startY = ((band.Height * blockY)/numY) << shift;
	        bounds[0] = (frame.Width * startY) + startX;
	        int endX = ((band.Width * (blockX+1))/numX) << shift;
	        int endY = ((band.Height * (blockY+1))/numY) << shift;
	        bounds[1] = ((endY-1)*frame.Width) + endX;
	        bounds[2] = endX - startX;
        }

        private void DecodeCodeBlock(ref short[] c, Arithmetic a, int[] bounds) {
	        int qo = QuantOffset(qi);
	        int qf = QuantFactor(qi);
	        for(int i = bounds[0]+offset; i < bounds[1]; i += frame.Width*stride) {
	            DecodeLineGeneric(ref c, a, i, bounds[2], qf, qo);
	        }
        }

        private void DecodeLineGeneric(ref short[] c, Arithmetic a, int lineOffset,
				       int blockWidth, int qf, int qo) {
	        int y = (lineOffset - offset)/(stride*frame.Width);
	        int x = ((lineOffset)%(frame.Width))/stride;
	        int parentLine = (y/2)*(2*frame.Width*stride);
	        int parentOffset = (orient == 1 ? stride :
			            (orient == 2 ? stride*frame.Width : 
			             stride*frame.Width + stride));
	        for(int i = lineOffset; i < blockWidth + lineOffset; i += stride) {
	            bool zparent = true, znhood = true;
	            if(level > 0) {
		        zparent = (c[parentLine + parentOffset+(x/2)*stride*2] == 0);
	            } 
	            if(x > 0) znhood = (znhood && c[i-stride] == 0);
	            if(y > 0) znhood = (znhood && c[i-stride*frame.Width] == 0);
	            if(x > 0 && y > 0) 
		        znhood = (znhood && c[i-stride-stride*frame.Width] == 0);
	            int cont = 0, sign = 0;
	            if(zparent) {
		        cont = (znhood ? Context.ZPZN_F1 : Context.ZPNN_F1);
	            } else {
		        cont = (znhood ? Context.NPZN_F1 : Context.NPNN_F1);
	            }
	            int v = a.DecodeUint(cont, Context.COEFF_DATA);
	            if(orient == 1 && y > 0) {
		        sign = c[i - frame.Width*stride];
	            } else if(orient == 2 && x > 0) {
		        sign = c[i - stride];
	            }
	            sign = (sign > 0 ? Context.SIGN_POS : 
		            (sign < 0 ? Context.SIGN_NEG : Context.SIGN_ZERO));
	            if(v > 0) {
		        v = (v * qf + qo + 2) >> 2;
		        v = (a.DecodeBool(sign) ? -v : v);
	            } 
	            c[i] = (short)v;
	            x++;
	        }
        }

        public void IntraDCPredict(ref short[] c) {
	        int predict = 0;
	        for(int i = offset; i < c.Length; i += frame.Width * stride) {
	            for(int j = i; j < i + frame.Width; j += stride) {
		        if(j > i && i > 0) {
		            predict = Util.Mean(c[j - stride], 
					        c[j - frame.Width*stride],
					        c[j - frame.Width*stride - stride]);
		        } else if(j > i && i == 0) {
		            predict = c[j-stride];
		        } else if(j == i && i > 0) {
		            predict = c[j-stride*frame.Width];
		        } else {
		            predict = 0;
		        }
		        c[j] += (short)predict;
	            }
	        }
        }

        private int QuantFactor(int qi) {	    
	        int mybase = (1 << (qi >> 2));
	        switch(qi & 0x3) {
	        case 0:
                return mybase << 2;
	        case 1:
	            return (503829 * mybase + 52958)/105917;
	        case 2:
	            return (665857 * mybase + 58854)/117708;
	        case 3:
	        default:
	            return (440253 * mybase + 32722)/65444;
	        }
        }

        private int QuantOffset(int qi) {
	        if(qi == 0) {
	            return 0;
	        } else {
	            if(par.is_intra) {
		        if(qi == 1) {
		            return 2;
		        } else {
		            return (QuantFactor(qi) + 1) / 2;
		        }
	            } else {
		        return (QuantFactor(qi) * 3 + 4) / 8;
	            }
	        }
        }
    }

}