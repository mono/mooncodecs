using System;
namespace org.diracvideo.Jirac
{

    public class VideoFormat : IEquatable<VideoFormat>
    {
        public int index;
        public int width, height;
        public int chroma_format;
        
        public int version_major, version_minor, profile, level;
        public bool interlaced, top_field_first;
        public int frame_rate_numerator, frame_rate_denominator,
	    aspect_ratio_numerator, aspect_ratio_denominator;
        public int clean_width, clean_height, left_offset, top_offset;
        public int luma_offset, luma_excursion, chroma_offset, chroma_excursion;
        public int transfer_function, colour_primaries, colour_matrix;
        public int interlaced_coding;
        public ColourSpace colour;

        private readonly static int[][] defaultFormats = new int[][] {
	        new int[]{ 0, 640, 480, 0420, 0, 0, 24000, 1001, /*custom */
	          1, 1, 640, 480, 0, 0, 0, 255, 128, 255, 0, 0, 0 },
	        new int[]{ 1,  176, 120, 0420,0,0, 15000, 1001, 10, /* QSIF525 */
	           11, 176, 120, 0, 0, 0, 255, 128, 255, 1, 1, 0 },
	        new int[]{ 2, 176, 144, 0420, 0, 1, 25, 2, 12, 11, /* QCIF */
	          176, 144, 0, 0, 0, 255, 128, 255, 2, 1, 0 },
	        new int[]{ 3, 352, 240, 0420, 0, 0, 15000, 1001, 10, /* SIF525 */
	          11, 352, 240, 0, 0,   0, 255, 128, 255, 1, 1, 0 },
	        new int[]{ 4, 352, 288, 0420, 0, 1, 25, 2, 12, 11,  /* CIF */
	          352, 288, 0, 0, 0, 255, 128, 255, 2, 1, 0 },
	        new int[]{ 5, 704, 480, 0420, 0, 0, 15000, 1001, 10, /* 4SIF525 */
	          11, 704, 480, 0, 0, 0, 255, 128, 255, 1, 1, 0 },
	        new int[]{ 6, 704, 576, 0420, 0, 1, 25, 2, 12, 11, /* 4CIF */
	          704, 576, 0, 0, 0, 255, 128, 255, 2, 1, 0 },
	        new int[]{ 7, 720, 480, 0422,  1, 0,  30000, 1001, 10, 11, /* SD480I-60 */
	          704, 480, 8, 0, 64, 876, 512, 896, 1, 1, 0 },
	        new int[]{ 8, 720, 576, 0422, 1, 1, 25, 1, 12, 11, 704, 576, /* SD576I-50 */
	          8, 0, 64, 876, 512, 896, 2, 1, 0 },
	        new int[]{ 9, 1280, 720, 0422, 0, 1, 60000, 1001, 1, 1, /* HD720P-60 */
	          1280, 720, 0, 0, 64, 876, 512, 896, 0, 0, 0 },
	        new int[]{ 10, 1280, 720, 0422, 0, 1, 50, 1, 1, 1, /* HD720P-50 */
	          1280, 720, 0, 0, 64, 876, 512, 896, 0, 0, 0 },
	        new int[]{ 11, 1920, 1080, 0422, 1, 1,30000, 1001, 1, 1, /* HD1080I-60 */
	          1920, 1080, 0, 0, 64, 876, 512, 896,  0, 0, 0 },
	        new int[]{ 12, 1920, 1080, 0422, 1, 1, 25, 1, 1, 1, /* HD1080I-50 */
	          1920, 1080, 0, 0, 64, 876, 512, 896, 0, 0, 0 },
	        new int[]{ 13, 1920, 1080, 0422, 0, 1, 60000, 1001, 1, 1, /* HD1080P-60 */
	          1920, 1080, 0, 0, 64, 876, 512, 896, 0, 0, 0 },
	        new int[]{ 14, 1920, 1080, 0422, 0, 1, 50, 1, 1, 1, /* HD1080P-50 */
	          1920, 1080, 0, 0, 64, 876, 512, 896,  0, 0, 0 },
	        new int[]{ 15, 2048, 1080, 0444, 0, 1, 24, 1, 1, 1, /* DC2K */
	          2048, 1080, 0, 0, 256, 3504, 2048, 3584, 3, 0, 0 },
	        new int[]{ 16, 4096, 2160, 0444, 0, 1, 24, 1, 1, 1, /* DC4K */
	          2048, 1536, 0, 0, 256, 3504, 2048, 3584,  3, 0, 0 }
            };

        private readonly static int[][] defaultFrameRates = new int[][]{
	        new int[]{ 0, 0 }, new int[]{ 24000, 1001 },
	        new int[]{ 24, 1 }, new int[]{ 25, 1 },
	        new int[]{ 30000, 1001 }, new int[]{ 30, 1 }, 
                new int[]{ 50, 1 }, new int[]{ 60000, 1001 },
                new int[]{ 60, 1 },  new int[]{ 15000, 1001 }, new int[]{ 25, 2 }
            };

        private readonly static int[][] defaultAspectRatios = new int[][]{
	        new int[]{ 0, 0 }, new int[]{ 1, 1 },
                new int[]{ 10, 11 }, new int[]{ 12, 11 },
                new int[]{ 40, 33 }, new int[]{ 16, 11 },  new int[]{ 4, 3 }
            };

        private readonly static int[][] defaultSignalRanges = new int[][]{
	        new int[]{ 0, 0, 0, 0 },
	        new int[]{ 0, 255, 128, 255 },
	        new int[]{ 16, 219, 128, 224 },
	        new int[]{ 64, 876, 512, 896, },
	        new int[]{ 256, 3504, 2048, 3584 }
            };

        private void SetDefaultVideoFormat(int i)  {
	        if (i >= VideoFormat.defaultFormats.Length) {
	            throw new Exception("Unsupported Video Format");
	        }
	        int[] a = VideoFormat.defaultFormats[i];
	        this.index = a[0];
	        this.width = a[1];
	        this.height = a[2];
	        this.chroma_format = a[3];
	        this.interlaced = ( a[4] == 0 ? false : true);
	        this.top_field_first = (a[5] == 0 ? false : true);
	        this.frame_rate_numerator = a[6];
	        this.frame_rate_denominator = a[7];
	        this.aspect_ratio_numerator = a[8];
	        this.aspect_ratio_denominator = a[9];
	        this.clean_width = a[10];
	        this.clean_height = a[11];
	        this.left_offset = a[12];
	        this.top_offset = a[13];
	        this.luma_offset = a[14];
	        this.luma_excursion = a[15];
	        this.chroma_offset = a[16];
	        this.chroma_offset = a[17];
        }

        private void SetDefaultFrameRate(int i) {
	        if(i >= VideoFormat.defaultFrameRates.Length) {
	            throw new Exception("Unsuported Frame Rate");
	        }
	        int[] a = VideoFormat.defaultFrameRates[i];
	        this.frame_rate_numerator = a[0];
	        this.frame_rate_denominator = a[1];
        }

        private void SetDefaultAspectRatio(int i)  {
	        if(i >= VideoFormat.defaultAspectRatios.Length) {
	            throw new Exception("Unsupported Aspect Ratio");
	        }
	        int[] a = VideoFormat.defaultAspectRatios[i];
	        this.aspect_ratio_numerator = a[0];
	        this.aspect_ratio_denominator = a[1];
        }

        private void SetDefaultSignalRange(int i)  {
	        if (i >= VideoFormat.defaultSignalRanges.Length) {
	            throw new Exception("Unsupported Signal Range");
	        }
	        int[] a = VideoFormat.defaultSignalRanges[i];
	        this.luma_offset = a[0];
	        this.luma_excursion = a[1];
	        this.chroma_offset = a[2];
	        this.chroma_excursion = a[3];
        }

        private void SetDefaultColourSpec(int i) {
	        colour = new ColourSpace(i, this);
        }

        /** VideoFormat:
         * @param b Buffer of a video format as packed in 
         * the dirac stream */

        public VideoFormat(Buffer b)  {
	        Unpack u = new Unpack(b);
	        u.Skip(104); /* 13 * 8 */
	        version_major = u.DecodeUint();
	        version_minor = u.DecodeUint();
	        profile = u.DecodeUint();
	        level = u.DecodeUint();

	        SetDefaultVideoFormat(u.DecodeUint());
	        if(u.DecodeBool()) { /* frame dimensions */
	            this.width = u.DecodeUint();
	            this.height = u.DecodeUint();
	        }
	        if(u.DecodeBool()) { /* chroma format */
	            chroma_format = u.DecodeUint();
	        }

	        if(u.DecodeBool()) { /* scan format */
	            this.interlaced = u.DecodeBool();
	            if(this.interlaced) {
		        this.top_field_first = u.DecodeBool();
	            }
	        }

	        if(u.DecodeBool()) { /* frame rate */
	            int i = u.DecodeUint();
	            if(i == 0) {
		        this.frame_rate_numerator = u.DecodeUint();
		        this.frame_rate_denominator = u.DecodeUint();
	            } else {
		        SetDefaultFrameRate(i);
	            }
	        }

	        if(u.DecodeBool()) { /* aspect ratio */
	            int i = u.DecodeUint();
	            if(i == 0) {
		        this.aspect_ratio_numerator = u.DecodeUint();
		        this.aspect_ratio_denominator = u.DecodeUint();
	            } else {
		        SetDefaultAspectRatio(i);
	            }
	        }
        	
	        if(u.DecodeBool()) { /* clean area */
	            this.clean_width = u.DecodeUint();
	            this.clean_height = u.DecodeUint();
	            this.left_offset = u.DecodeUint();
	            this.top_offset = u.DecodeUint();
	        }
        	
	        if(u.DecodeBool()) { /* signal range */
	            int i = u.DecodeUint();
	            if(i == 0) {
		        this.luma_offset = u.DecodeUint();
		        this.luma_excursion = u.DecodeUint();
		        this.chroma_offset = u.DecodeUint();
		        this.chroma_excursion = u.DecodeUint();
	            } else {
		        SetDefaultSignalRange(i);
	            }
	        }

	        if(u.DecodeBool()) { /* colour spec */
	            int i = u.DecodeUint();
	            SetDefaultColourSpec(i);
	            if(i == 0) {
		        if(u.DecodeBool()) {
		            colour_primaries = u.DecodeUint();
		        }
		        if(u.DecodeBool()) {
		            colour_matrix = u.DecodeUint();
		        }
		        if(u.DecodeBool()) {
		            transfer_function = u.DecodeUint();
		        }
	            }
	        } else {
	            colour = new ColourSpace(0,this);
	        }
	        interlaced_coding = u.DecodeUint();
	        Validate();
        }

        private void Validate() {
	        if(0 == this.aspect_ratio_numerator) {
	            this.aspect_ratio_numerator = 1;
	        }
	        if(0 == this.aspect_ratio_denominator) {
	            this.aspect_ratio_denominator = 1;
	        }
	        int max = System.Math.Max(this.chroma_excursion,
				           this.luma_excursion);
	        if(max > 255 || max < 128) {
	            /* well, now what? */
	        }
    	
        }

        public bool Equals(VideoFormat o) {
	        bool diff = false;
	        diff = diff || (o.index != this.index);
	        diff = diff || (o.width != this.width);
	        diff = diff || (o.height != this.height);
	        diff = diff || (o.chroma_format != this.chroma_format);
	        diff = diff || (o.interlaced != this.interlaced);
	        diff = diff || (o.top_field_first != this.top_field_first);
	        diff = diff || (o.frame_rate_numerator != this.frame_rate_numerator);
	        diff = diff || (o.frame_rate_denominator != this.frame_rate_denominator);
	        diff = diff || (o.aspect_ratio_numerator != this.aspect_ratio_numerator);
	        diff = diff || (o.clean_width != this.clean_width);
	        diff = diff || (o.clean_height != this.clean_height);
	        diff = diff || (o.luma_offset != this.luma_offset);
	        diff = diff || (o.luma_excursion != this.luma_excursion);
	        diff = diff || (o.chroma_offset != this.chroma_offset);
	        diff = diff || (o.chroma_excursion != this.chroma_excursion);
	        diff = diff || (o.interlaced_coding != this.interlaced_coding);
	        return diff == false;
        }
        
        public void GetPictureLumaSize( ref int[]  res) {
	        res[0] = this.width;
	        res[1] = Util.RoundUpShift(this.height, this.interlaced_coding);
        }

        public void GetPictureChromaSize(ref int[] res) {
	        res[0] = Util.RoundUpShift(this.width, ChromaHShift());
            res[1] = Util.RoundUpShift(this.height, ChromaVShift()); 
        }

        public int ChromaHShift() {
	        return (chroma_format > 0 ? 1 : 0);
        }

        public int ChromaVShift() {
	        return (chroma_format > 1 ? 1 : 0) + interlaced_coding;
        }
    }
}