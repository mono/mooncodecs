//import java.awt.Dimension;
//import java.awt.Point;

using System;
using org.diracvideo.Math;
namespace org.diracvideo.Jirac
{


    /** Motion
     *
     * An ill-named class representing an object
     * which does motion compensation prediction
     * on a picture. **/
    class Motion {
        Parameters par;
        Vector[] vecs;
        Block[][] refs;
        Arithmetic[] ar;
        int xbsep, ybsep, xblen, yblen, xoffset, yoffset;
        int chroma_h_shift, chroma_v_shift;
        short[] weight_x, weight_y, obmc;
        Block[] tmp_ref;
        Block block;

        static int ARITH_SUPERBLOCK = 0;
        static int ARITH_PRED_MODE = 1;
        static int ARITH_REF1_X = 2;
        static int ARITH_REF1_Y = 3;
        static int ARITH_REF2_X = 4;
        static int ARITH_REF2_Y = 5;
        static int ARITH_DC_0 = 6;
        static int ARITH_DC_1 = 7;
        static int ARITH_DC_2 = 8;

        public Motion(Parameters p, Buffer[] bufs, Block[][] frames) {
	        par = p;
	        refs = frames;
	        vecs = new Vector[par.x_num_blocks * par.y_num_blocks];
	        tmp_ref = new Block[refs.Length];
	        ar = new Arithmetic[9];
	        for(int i = 0; i < 9; i++) 
	            if(bufs[i] != null) ar[i] = new Arithmetic(bufs[i]);
        }
        

        public void Decode() {
	        for(int y = 0; y < par.y_num_blocks; y += 4)
	            for(int x = 0; x < par.x_num_blocks; x += 4)
		        DecodeMacroBlock(x,y);	
        }

        private void DecodeMacroBlock(int x, int y) {
	        int split = SplitPrediction(x,y);
	        Vector mv = GetVector(x,y);
	        mv.split = (split + ar[ARITH_SUPERBLOCK].DecodeUint(Context.SB_F1, Context.SB_DATA))%3;
	        switch(mv.split) {
	        case 0:
	            DecodePredictionUnit(mv, x, y);
	            for(int i = 0; i < 4; i++) 
		        for(int j = 0; j < 4; j++) 
		            SetVector(mv, x + j, y + i);
	            break;
	        case 1:
	            for(int i = 0; i < 4; i += 2) 
		        for(int j = 0; j < 4; j += 2) {
		            mv = GetVector(x + j, y + i);
		            mv.split = 1;
		            DecodePredictionUnit(mv, x + j, y + i);
		            SetVector(mv, x + j + 1, y + i);
		            SetVector(mv, x + j, y + i + 1);
		            SetVector(mv, x + j + 1, y + i + 1);
		        }
	            break;
	        case 2:
	            for(int i = 0; i < 4; i++) 
		        for(int j = 0; j < 4; j++) {
		            mv = GetVector(x + j, y + i);
		            mv.split = 2;
		            DecodePredictionUnit(mv, x + j, y + i);
		        }
	            break;
	        default:
	            throw new Exception("Unsupported splitting mode");
	        }
        }

        private void DecodePredictionUnit(Vector mv, int x, int y) {
	        mv.pred_mode = ModePrediction(x,y);
	        mv.pred_mode ^= ar[ARITH_PRED_MODE].DecodeBit(Context.BLOCK_MODE_REF1);
	        if(par.num_refs > 1) {
	            mv.pred_mode ^= (ar[ARITH_PRED_MODE].DecodeBit(Context.BLOCK_MODE_REF2) << 1);
	        }
	        if(mv.pred_mode == 0) {
	            int[] pred = new int[3];
	            DcPrediction(x,y,pred);
	            mv.dc[0] = pred[0] + 
		        ar[ARITH_DC_0].DecodeSint(Context.LUMA_DC_CONT_BIN1,
					          Context.LUMA_DC_VALUE,
					          Context.LUMA_DC_SIGN);
	            mv.dc[1] = pred[1] + 
		        ar[ARITH_DC_1].DecodeSint(Context.CHROMA1_DC_CONT_BIN1,
					          Context.CHROMA1_DC_VALUE,
					          Context.CHROMA1_DC_SIGN);
	            mv.dc[2] = pred[2] + 
		        ar[ARITH_DC_2].DecodeSint(Context.CHROMA2_DC_CONT_BIN1,
					          Context.CHROMA2_DC_VALUE,
					          Context.CHROMA2_DC_SIGN);
	        } else {
	            int pred_x, pred_y;
	            if(par.have_global_motion) {
		        int pred = GlobalPrediction(x,y);
		        pred ^= ar[ARITH_SUPERBLOCK].DecodeBit(Context.GLOBAL_BLOCK);
		        mv.namespace_global = (pred == 0 ? false : true);
	            } else {
		        mv.namespace_global = false;
	            }
	            if(!mv.namespace_global) {
		        if((mv.pred_mode & 1) != 0) {
		            VectorPrediction(mv,x,y,1);
		            mv.dx[0] += 
			        ar[ARITH_REF1_X].DecodeSint(Context.MV_REF1_H_CONT_BIN1,
						            Context.MV_REF1_H_VALUE, 
						            Context.MV_REF1_H_SIGN);
		            mv.dy[0] +=
			        ar[ARITH_REF1_Y].DecodeSint(Context.MV_REF1_V_CONT_BIN1, 
						            Context.MV_REF1_V_VALUE,
						            Context.MV_REF1_V_SIGN);
		        }
		        if((mv.pred_mode & 2) != 0) {
		            VectorPrediction(mv, x, y, 2);
		            mv.dx[1] += ar[ARITH_REF2_X].DecodeSint(Context.MV_REF2_H_CONT_BIN1,
							            Context.MV_REF2_H_VALUE, 
							            Context.MV_REF2_H_SIGN);
		            mv.dy[1] += ar[ARITH_REF2_Y].DecodeSint(Context.MV_REF2_V_CONT_BIN1, 
						         Context.MV_REF2_V_VALUE,
						         Context.MV_REF2_V_SIGN);

		        }
	            } 
	        }
	        mv.namespace_global = false;
	        mv.dx[0] = 0;
	        mv.dy[0] = 0;
	        mv.dx[1] = 0;
	        mv.dy[1] = 0;
        }

        public void Render(Block[] outBlocks, VideoFormat f) {
	        for(int k = 0; k < outBlocks.Length; k++) {
	            InitializeRender(k,f);
	            block = new Block(new Dimension(xblen, yblen));
	            for(int i = 0; i < par.num_refs; i++)  
		        tmp_ref[i] = refs[i][k];
	            for(int j = 0; j < par.y_num_blocks; j++)
		        for(int i = 0; i < par.x_num_blocks; i++) {
		            PredictBlock(outBlocks[k], i, j, k);
		            AccumulateBlock(outBlocks[k], i*xbsep - xoffset, 
				            j*ybsep - yoffset);
		        }
	            outBlocks[k].ShiftOut(6,0);
	            outBlocks[k].Clip(7);
	        }
        }

        private void InitializeRender(int k, VideoFormat f) {
	        chroma_h_shift = f.ChromaHShift();
	        chroma_v_shift = f.ChromaVShift();
	        yblen = par.yblen_luma;
	        xblen = par.xblen_luma;
	        ybsep = par.ybsep_luma;
	        xbsep = par.xbsep_luma;
	        if(k != 0) {
	            yblen >>= chroma_v_shift;
	            ybsep >>= chroma_v_shift;
	            xbsep >>= chroma_h_shift;
	            xblen >>= chroma_h_shift;
	        }
	        yoffset = (yblen - ybsep) >> 1;
	        xoffset = (xblen - xbsep) >> 1;
	        /* initialize obmc weight */
	        weight_y = new short[yblen];
	        weight_x = new short[xblen];
	        obmc = new short[xblen*yblen];
	        for(int i = 0; i < xblen; i++) {
	            short wx;
	            if(xoffset == 0) {
		        wx = 8;
	            } else if( i < 2*xoffset) {
		        wx = Util.GetRamp(i, xoffset);
	            } else if(xblen - 1 - i < 2*xoffset) {
		        wx = Util.GetRamp(xblen - 1 - i, xoffset);
	            } else {
		        wx = 8;
	            }
	            weight_x[i] = wx;
	        }
	        for(int j = 0; j < yblen; j++) {
	            short wy;
	            if(yoffset == 0) {
		        wy = 8;
	            } else if(j < 2*yoffset) {
		        wy = Util.GetRamp(j, yoffset);
	            } else if(yblen - 1 - j < 2*yoffset) {
		        wy = Util.GetRamp(yblen - 1 - j, yoffset);
	            } else {
		        wy = 8;
	            }
	            weight_y[j] = wy;
	        }
        }

        private void DumpWeights() {
	        System.Console.WriteLine("weight_x");
	        for(int i = 0; i < xblen; i++)
	            Console.WriteLine("%d ", weight_x[i]);
	        System.Console.WriteLine("\nweight_y");
	        for(int i = 0; i < yblen; i++)
	            Console.WriteLine("%d ", weight_y[i]);
	        System.Console.WriteLine("");
        }

        private void PredictBlock(Block b, int i, int j, int k) {
	        int xstart = (i*xbsep) - xoffset, 
	            ystart = (j*ybsep) - yoffset;
	        Vector mv = GetVector(i,j);
	        if(mv.pred_mode == 0) {
	            for(int q = 0; j < yblen; j++) 
		        for(int p = 0; i < xblen; i++)
		            block.Set(p, q, (mv.dc[k]));
	        } 
	        if(k != 0 && !mv.namespace_global)
	            mv = mv.Scale(chroma_h_shift, chroma_v_shift); 
	        for(int q = 0; q < yblen; q++) {
	            int y = ystart + q;
	            if(y < 0 || y > b.s.Height - 1) continue;
	            for(int p = 0; p < xblen; p++) {
		        int x = xstart + p;
		        if(x < 0 || x > b.s.Width - 1) continue;
		        block.Set(p,q, PredictPixel(mv, x, y, k));
	            }
	        }
        }

        private short PredictPixel(Vector mv, int x,  int y, int k) {
	        if(mv.namespace_global) {
	            for(int i = 0; i < par.num_refs; i++) {
		        par.global[i].GetVector(mv, x, y, i);
	            }
	            if(k != 0) 
		        mv = mv.Scale(chroma_h_shift, chroma_v_shift);
	        }
	        short weight = (short)(par.picture_weight_1 + par.picture_weight_2);
	        short val = 0;
	        int px, py;
	        switch(mv.pred_mode) {
	        case 1:
	            px = (x << par.mv_precision) + mv.dx[0];
	            py = (y << par.mv_precision) + mv.dy[0];
	            val = (short)(weight*PredictSubPixel(0, px, py));
	            break;
	        case 2:
	            px = (x << par.mv_precision) + mv.dx[1];
	            py = (y << par.mv_precision) + mv.dy[1];
	            val = (short)(weight*PredictSubPixel(1, px, py));
	            break;
	        case 3:
	            px = (x << par.mv_precision) + mv.dx[0];
	            py = (y << par.mv_precision) + mv.dy[0];
	            val = (short)(par.picture_weight_1*PredictSubPixel(0, px, py));
	            px = (x << par.mv_precision) + mv.dx[1];
	            py = (x << par.mv_precision) + mv.dy[1];
	            val += (short)(par.picture_weight_2*PredictSubPixel(1, px, py));
                break;
	        default:
	            break;
	        }
	        return (short)Util.RoundShift(val, par.picture_weight_bits);
        }

        private short PredictSubPixel(int reference, int px, int py) {
	    if(par.mv_precision < 2) { 
	        return tmp_ref[reference].Real(px, py); 
	    }
	    int prec = par.mv_precision;
	    int add = 1 << (prec - 1);
	    int hx = px >> (prec-1);
	    int hy = py >> (prec-1);
	    int rx = px - (hx << (prec-1));
	    int ry = py - (hy << (prec-1));
	    int w00,w01, w10, w11;
	    w00 = (add - rx)*(add - ry);
	    w01 = (add - rx)*ry;
	    w10 = rx*(add - ry);
	    w11 = rx*ry;
	    int val = w00*tmp_ref[reference].Real(hx, hy) + 
	        w01*tmp_ref[reference].Real(hx + 1, hy) +
	        w10*tmp_ref[reference].Real(hx, hy + 1) + 
	        w11*tmp_ref[reference].Real(hx + 1, hy + 1);
	    return (short)((val + (1 << (2*prec-3))) >> (2*prec - 2));
        }
        

        private void AccumulateBlock(Block b, int x, int y) {
	        if(!Edge(x,y)) {
	            for(int q = 0; q < yblen; q++) {
		        if(q + y < 0 || q + y >= b.s.Height) continue;
		        int outLine = b.Index(x, y + q);
		        int inLine = block.Line(q);	
		        for(int p = 0; p < xblen; p++) {
		            if(p + x < 0 || p + x >= b.s.Width) continue;
		            b.d[outLine + p] += 
			        (short)(weight_x[p]*weight_y[q]*block.d[inLine+p]);
		        }
	            }
	        } else {
	            int w_x, w_y;
	            for(int q = 0; q < yblen; q++) {
		            if(q + y < 0 || q + y >= b.s.Height) continue;
		            if((y < 0 && q < 2*yoffset) || 
		                   (y >= par.y_num_blocks*ybsep - yoffset &&
			            yblen - 1 - q < 2*yoffset)) 
			            w_y = 8;
		                else
			            w_y = weight_y[q];
		            int outLine = b.Index(x, y + q);
		            int inLine = block.Line(q);
		            for(int p = 0; p < xblen; p++) {
		                if(p + x < 0 || p + x >= b.s.Width) continue;
		                if((x < 0 && p < 2*xoffset) || 
		                   (x >= par.x_num_blocks*xbsep - xoffset &&
			            xblen - 1 - p < 2*xoffset)) 
			            w_x = 8;
		                else
			            w_x = weight_x[p];
		                b.d[outLine + p] += 
			            (short)(w_x*w_y*block.d[inLine+p]);
		            }
	            }
    		
	        }
    	
        }

        private bool Edge(int x, int y) {
	        return (x < 0 || x >= par.x_num_blocks*xbsep - xoffset) 
	            || (y < 0 || y >= par.y_num_blocks*ybsep - yoffset);
    	
        }
        
        private int SplitPrediction(int x, int y) {
	        if(y == 0) {
	            if(x == 0) {
		        return 0;
	            } else {
		        return vecs[x-4].split;
	            }
	        } else {
	            if(x == 0) {
		            return GetVector(0, y - 4).split;
	            } else {
		            int sum = 0;
		            sum += GetVector(x, y - 4).split;
		            sum += GetVector(x - 4, y).split;
		            sum += GetVector(x - 4, y - 4).split;
		            return (sum+1)/3;
	            }
	        }
        }

        private int ModePrediction(int x, int y) {
	        if(y == 0) {
	            if(x == 0) {
		            return 0;
	            } else {
		            return vecs[x - 1].pred_mode;
	            }
	        } else {
	            if(x == 0) {
		            return GetVector(0, y - 1).pred_mode;
	            } else {
		            int a,b,c;
		            a = GetVector(x - 1, y).pred_mode;
		            b = GetVector(x, y - 1).pred_mode;
		            c = GetVector(x - 1, y - 1).pred_mode;
		            return (a&b)|(b&c)|(c&a);
	            }
	        }
        }

        private int GlobalPrediction(int x, int y) {
	        if(x == 0 && y == 0) {
	            return 0;
	        }
	        if(y == 0) {
	            return vecs[x-1].namespace_global ? 1 : 0;
	        }
	        if(x == 0) {
	            return GetVector(0, y-1).namespace_global ? 1 : 0;
	        }
	        int sum = 0;
	        sum += GetVector(x - 1, y).namespace_global ? 1 : 0;
	        sum += GetVector(x, y - 1).namespace_global ? 1 : 0;
	        sum += GetVector(x - 1, y - 1).namespace_global ? 1 : 0;
	        return (sum >= 2) ? 1 : 0;
        }
        
        private void VectorPrediction(Vector mv, int x, int y, int mode) {
	        int n = 0;
            int[] vx = new int[3];
            int[] vy = new int[3];
	        if(x > 0) {
	            Vector ov = GetVector(x-1, y);
	            if(!ov.namespace_global && 
	               (ov.pred_mode & mode) != 0) {
		        vx[n] = ov.dx[mode-1];
		        vy[n] = ov.dx[mode-1];
		        n++;
	            }
	        }
	        if(y > 0) {
	            Vector ov = GetVector(x, y-1);
	            if(!ov.namespace_global &&
	               (ov.pred_mode & mode) != 0) {
		        vx[n] = ov.dx[mode-1];
		        vy[n] = ov.dx[mode-1];
		        n++;
	            }
	        }
	        if(x > 0 && y > 0) {
	            Vector ov = GetVector(x - 1, y - 1);
	            if(!ov.namespace_global &&
	               (ov.pred_mode & mode) != 0) {
		        vx[n] = ov.dx[mode-1];
		        vy[n] = ov.dy[mode-1];
		        n++;
	            }
	        }
	        switch(n) {
	        case 0:
	            mv.dx[mode-1] = 0;
	            mv.dy[mode-1] = 0;
	            break;
	        case 1:
	            mv.dx[mode-1] = vx[0];
	            mv.dy[mode-1] = vy[0];
	            break;
	        case 2:	  
	            mv.dx[mode-1] = (vx[0] + vx[1] + 1) >> 1;
	            mv.dy[mode-1] = (vy[0] + vy[1] + 1) >> 1;
	            break;
	        case 3:
	            mv.dx[mode-1] = Util.Median(vx);
	            mv.dy[mode-1] = Util.Median(vy);
	            break;
	        }
        }

        private void DcPrediction(int x, int y, int[] pred) {
	        for(int i = 0; i < 3; i++) {
	            int sum = 0, n = 0;
	            if(x > 0) {
		        Vector ov = GetVector(x - 1, y);
		        if(ov.pred_mode == 0) {
		            sum += ov.dc[i];
		            n++;
		        }
	            }
	            if(y > 0) {
		        Vector ov = GetVector(x, y - 1);
		        if(ov.pred_mode == 0) {
		            sum += ov.dc[i];
		            n++;
		        }
	            }
	            if(x > 0 && y > 0) {
		        Vector ov = GetVector(x - 1, y - 1);
		        if(ov.pred_mode == 0) {
		            sum += ov.dc[i];
		            n++;
		        }
	            }
	            switch(n) {
	            case 0:
		            pred[i] = 0;
		            break;
	            case 1:
		            pred[i] = sum;
		            break;
	            case 2:
		            pred[i] = (sum+1)>>1;
		            break;
	            case 3:
		            pred[i] = (sum+1)/3;
		            break;
	            }
	        }
        }

        private  Vector GetVector(int x, int y) {
	        int pos = x + y*par.x_num_blocks;
	        if(vecs[pos] == null) {
	            vecs[pos] = new Vector();
	        }
	        return vecs[pos];
        }

        private  void SetVector(Vector mv, int x, int y) {
	        vecs[x + y*par.x_num_blocks] = mv;
        }

    }

}