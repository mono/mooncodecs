using System;
using System.Threading;
namespace org.diracvideo.Jirac
{

    /** 
     * Decoder
     *
     * An interface to decoding a dirac stream. 
     * Most (all) of the actual work is done by the
     * Picture class, however Decoder can do general
     * dispatching, scheduling and bookkeeping.
     * That is the reason we keep it arround */

    public class Decoder {
        private Stream stream;
        public VideoFormat format;
        public Status status = Status.NULL;
        public Exception e;
        public Queue inQueue, outQueue;
        public Cache refs;
        public enum Status {NULL, OK, WAIT, DONE, ERROR}
        public static AutoResetEvent ev = new AutoResetEvent(false);

        public Decoder() {
	        stream = new Stream();
	        refs = new Cache(4);
	        inQueue = new InputQueue();
	        outQueue = new OutputQueue();
        }
        
        /** Push:
         * @param d byte array containing stream data
         * @param o offset in the byte array
         * 
         * The stated goal of this function is that
         * it should work even in the case of a so-called
         * DumbMuxingFormat which splits the dirac stream
    k     * into x-byte segments, and that the driver program
         * should only ever have to push such segments to 
         * the decoder. */

        public void Push(byte[] d, int s, int e)  {
	        Push(new Buffer(d,s,e));
        }


        private void Push(Buffer buf)  {
	        stream.Add(buf);
	        for(Buffer packet = stream.Next(); 
	            packet != null;
	            packet = stream.Next())
	            Dispatch(packet);
	        
            if(!inQueue.Empty() && status  == Status.WAIT)
                lock (this) { ev.Set(); }
        }


        /* at this point, the buffer must be a complete dirac packet */
        private void Dispatch(Buffer b)  {
	        if (b.GetInt(5) != b.Size()) 
                throw new Exception("Incorrect buffer sizes");
	        byte c = b.GetByte(4);
	        switch(c) {
	        case 0x00:
	            VideoFormat tmp = new VideoFormat(b);
	            if(format == null) {
		        format = tmp;
		        status = Status.OK;
	            } else if(!tmp.Equals(format)) {
		        throw new Exception("Stream Error: Inequal Video Formats");
	            }
	            break;
	        case 0x10:
	            status = Status.DONE;
	            break;
	        case 0x20:
	        case 0x30:
	            break;
	        default:
	            if(format == null) 
		            throw new Exception("Stream Error: Picture Before Header");
	            Picture pic = new Picture(b, this);
	            pic.Parse();
	            inQueue.Push(pic);
	            break;
	        }
        }

        public Picture Pull() {
	        return outQueue.Pull();
        }

        public void Decode() {
            lock (inQueue)
            {
	            while(!inQueue.Empty()) {
	                Picture pic = inQueue.Pull();
	                pic.Decode();
	                if(pic.error != null) 
                        Console.WriteLine(pic.error.ToString());
	                else 
                        outQueue.Push(pic);
	            }
            }
        }

        public bool Done() {
	        return status == Status.DONE && inQueue.Empty();
        }
    }

}