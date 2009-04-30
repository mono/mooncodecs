namespace org.diracvideo.Jirac
{

    public class Cache {
        private int end;
        private int[] nums;
        private Block[][] blocks, scaled;

        public Cache(int n) {
	    end = 0;
	    nums = new int[n];
	    blocks = new Block[n][];
	    scaled = new Block[n][];
        }

        public void Add(int n, Block[] refs) {
	        if(end == nums.Length) 
	            ShiftFrom(0);
	        nums[end] = n;
	        blocks[end] = refs;
	        end++;
        }
        
        public Block[] Get(int n, bool upscaled) {
	        int i = GetIndex(n);
	        if(i < 0) return null;
	        if(upscaled) {
	            if(scaled[i] == null) {
		        scaled[i] = new Block[blocks[i].Length];
		        for(int j = 0; j < scaled[i].Length; j++)
		            scaled[i][j] = blocks[i][j].UpSample();
	            }
	            return scaled[i];
	        } 
	        return blocks[i];
        } 

        public void Remove(int n) {
	        int i = GetIndex(n);
	        if(i >= 0) ShiftFrom(i);
        }
            
        public bool Has(int n) {
            return GetIndex(n) >= 0;
        }
        
        private void ShiftFrom(int i) {
	        while(++i < end) {
	            nums[i-1] = nums[i];
	            blocks[i-1] = blocks[i];
	            scaled[i-1] = scaled[i];
	        }
	        end--;
	        nums[end] = -1;
	        blocks[end] = scaled[end] = null;
        }

        private int GetIndex(int n) {
	        for(int i = 0; i < end; i++)
	            if(nums[i] == n) return i;
	        return -1;
        }
    }
}