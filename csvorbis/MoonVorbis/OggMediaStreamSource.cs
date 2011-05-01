
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows.Media;
using csogg;
using csvorbis;
using MediaParsers;

namespace MoonVorbis
{
    public class DebugWriter : TextWriter
    {
        public override Encoding Encoding
        {
            get { return Encoding.UTF8; }
        }

        public override void WriteLine ()
        {
            Debug.WriteLine (String.Empty);
        }

        public override void WriteLine (string s)
        {
            Debug.WriteLine (s);
        }
    }

    public class OggMediaStreamSource : MediaStreamSource
	{
		int convsize=4096*2;
		//byte[] convbuffer=new byte[convsize]; // take 8k out of the data segment, not the stack
		byte[] convbuffer;

        TextWriter s_err = new DebugWriter(); // Console.Error;
		Stream input;

		public OggMediaStreamSource (Stream input)
		{
			convbuffer=new byte[convsize];
			vb = new Block (vd);
			if (input == null)
				throw new ArgumentNullException ("input");
			this.input = input;
		}

		SyncState oy=new SyncState(); // sync and verify incoming physical bitstream
		StreamState os=new StreamState(); // take physical pages, weld into a logical stream of packets
		Page og=new Page(); // one Ogg bitstream page.  Vorbis packets are inside
		Packet op=new Packet(); // one raw packet of data for decode
  
		Info vi=new Info();  // struct that stores all the static vorbis bitstream settings
		Comment vc=new Comment(); // struct that stores all the bitstream user comments
		DspState vd=new DspState(); // central working state for the packet->PCM decoder
		//Block vb=new Block(vd); // local working space for packet->PCM decode
		Block vb;

		IEnumerable<SampleBuffer> DecodeSamples ()
		{
            // Decode setup

            oy.init(); // Now we can read pages
            while (true)
			{ // we repeat if the bitstream is chained
				byte[] buffer;
				int bytes=0;
	
				int eos=0;
	
				// grab some data at the head of the stream.  We want the first page
				// (which is guaranteed to be small and only contain the Vorbis
				// stream initial header) We need the first page to get the stream
				// serialno.
	
				// submit a 4k block to libvorbis' Ogg layer
				int index=oy.buffer(4096);
				buffer=oy.data;
				try
				{
					bytes = input.Read(buffer, index, 4096);
				}
				catch(Exception e)
				{
					s_err.WriteLine(e);
				}
				oy.wrote(bytes);
	
				// Get the first page.
				if(oy.pageout(og)!=1)
				{
					// have we simply run out of data?  If so, we're done.
					if(bytes<4096) yield break;
	  
					// error case.  Must not be Vorbis data
					s_err.WriteLine("Input does not appear to be an Ogg bitstream.");
				}
	
				// Get the serial number and set up the rest of decode.
				// serialno first; use it to set up a logical stream
				os.init(og.serialno());
	
				// extract the initial header from the first page and verify that the
				// Ogg bitstream is in fact Vorbis data
	
				// I handle the initial header first instead of just having the code
				// read all three Vorbis headers at once because reading the initial
				// header is an easy way to identify a Vorbis bitstream and it's
				// useful to see that functionality seperated out.
	
				vi.init();
				vc.init();
				if(os.pagein(og)<0)
				{ 
					// error; stream version mismatch perhaps
					s_err.WriteLine("Error reading first page of Ogg bitstream data.");
				}
	
				if(os.packetout(op)!=1)
				{ 
					// no page? must not be vorbis
					s_err.WriteLine("Error reading initial header packet.");
				}
	
				if(vi.synthesis_headerin(vc,op)<0)
				{ 
					// error case; not a vorbis header
					s_err.WriteLine("This Ogg bitstream does not contain Vorbis audio data.");
				}
	
				// At this point, we're sure we're Vorbis.  We've set up the logical
				// (Ogg) bitstream decoder.  Get the comment and codebook headers and
				// set up the Vorbis decoder
	
				// The next two packets in order are the comment and codebook headers.
				// They're likely large and may span multiple pages.  Thus we reead
				// and submit data until we get our two pacakets, watching that no
				// pages are missing.  If a page is missing, error out; losing a
				// header page is the only place where missing data is fatal. */
	
				int i=0;
				
				while(i<2)
				{
					while(i<2)
					{
	
						int result=oy.pageout(og);
						if(result==0) break; // Need more data
						// Don't complain about missing or corrupt data yet.  We'll
						// catch it at the packet output phase
	
						if(result==1)
						{
							os.pagein(og); // we can ignore any errors here
							// as they'll also become apparent
							// at packetout
							while(i<2)
							{
								result=os.packetout(op);
								if(result==0)break;
								if(result==-1)
								{
									// Uh oh; data at some point was corrupted or missing!
									// We can't tolerate that in a header.  Die.
									s_err.WriteLine("Corrupt secondary header.  Exiting.");
								}
								vi.synthesis_headerin(vc,op);
								i++;
							}
						}
					}
					// no harm in not checking before adding more
					index=oy.buffer(4096);
					buffer=oy.data; 
					try
					{
						bytes=input.Read(buffer, index, 4096);
					}
					catch(Exception e)
					{
						s_err.WriteLine(e);
					}
					if(bytes==0 && i<2)
					{
						s_err.WriteLine("End of file before finding all Vorbis headers!");
					}
					oy.wrote(bytes);
				}
	
				// Throw the comments plus a few lines about the bitstream we're
				// decoding
			{
				byte[][] ptr=vc.user_comments;
				for(int j=0; j<vc.user_comments.Length;j++)
				{
					if(ptr[j]==null) break;
					s_err.WriteLine(vc.getComment(j));
				} 
				s_err.WriteLine("\nBitstream is "+vi.channels+" channel, "+vi.rate+"Hz");
				s_err.WriteLine("Encoded by: "+vc.getVendor()+"\n");
			}
	
				convsize=4096/vi.channels;
	
				// OK, got and parsed all three headers. Initialize the Vorbis
				//  packet->PCM decoder.
				vd.synthesis_init(vi); // central decode state
				vb.init(vd);           // local state for most of the decode
	
				// so multiple block decodes can
				// proceed in parallel.  We could init
				// multiple vorbis_block structures
				// for vd here
				float[][][] _pcm=new float[1][][];
				int[] _index=new int[vi.channels];
				// The rest is just a straight decode loop until end of stream
				while(eos==0)
				{
					while(eos==0)
					{
	
						int result=oy.pageout(og);
						if(result==0)break; // need more data
						if(result==-1)
						{ // missing or corrupt data at this page position
							s_err.WriteLine("Corrupt or missing data in bitstream; continuing...");
						}
						else
						{
							os.pagein(og); // can safely ignore errors at
							// this point
							while(true)
							{
								result=os.packetout(op);
	
								if(result==0)break; // need more data
								if(result==-1)
								{ // missing or corrupt data at this page position
									// no reason to complain; already complained above
								}
								else
								{
									// we have a packet.  Decode it
									int samples;
									if(vb.synthesis(op)==0)
									{ // test for success!
										vd.synthesis_blockin(vb);
									}
	
									// **pcm is a multichannel float vector.  In stereo, for
									// example, pcm[0] is left, and pcm[1] is right.  samples is
									// the size of each channel.  Convert the float values
									// (-1.<=range<=1.) to whatever PCM format and write it out
	
									while((samples=vd.synthesis_pcmout(_pcm, _index))>0)
									{
										float[][] pcm=_pcm[0];
										bool clipflag=false;
										int bout=(samples<convsize?samples:convsize);
	
										// convert floats to 16 bit signed ints (host order) and
										// interleave
										for(i=0;i<vi.channels;i++)
										{
											int ptr=i*2;
											//int ptr=i;
											int mono=_index[i];
											for(int j=0;j<bout;j++)
											{
												int val=(int)(pcm[i][mono+j]*32767.0);
												//        short val=(short)(pcm[i][mono+j]*32767.);
												//        int val=(int)Math.round(pcm[i][mono+j]*32767.);
												// might as well guard against clipping
												if(val>32767)
												{
													val=32767;
													clipflag=true;
												}
												if(val<-32768)
												{
													val=-32768;
													clipflag=true;
												}
												if(val<0) val=val|0x8000;
												convbuffer[ptr]=(byte)(val);
												convbuffer[ptr+1]=(byte)((uint)val>>8);
												ptr+=2*(vi.channels);
											}
										}
	
										if(clipflag) 
										{
											//s_err.WriteLine("Clipping in frame "+vd.sequence);
										}
	
										yield return new SampleBuffer (convbuffer, 0, 2*vi.channels*bout);
	
										vd.synthesis_read(bout); // tell libvorbis how
										// many samples we
										// actually consumed
									}     
								}
							}
							if(og.eos()!=0)eos=1;
						}
					}
					if(eos==0)
					{
						index=oy.buffer(4096);
						buffer=oy.data;
						try
						{
							bytes=input.Read(buffer,index,4096);
						}
						catch(Exception e)
						{
							s_err.WriteLine(e);
						}
						oy.wrote(bytes);
						if(bytes==0)eos=1;
					}
				}
			}

			// clean up this logical bitstream; before exit we see if we're
			// followed by another [chained]

			os.clear();

			// ogg_page and ogg_packet structs always point to storage in
			// libvorbis.  They're never freed or manipulated directly

			vb.clear();
			vd.clear();
			vi.clear();  // must be called last
		}

		struct SampleBuffer
		{
			byte [] buf;
			int index, count;
			public SampleBuffer (byte [] buf, int index, int count)
			{
				this.buf = buf;
				this.index = index;
				this.count = count;
			}

			public byte [] Data { get { return buf; } }
			public int Index { get { return index; } }
			public int Count { get { return count; } }
		}

		protected override void CloseMedia ()
		{
            // OK, clean up the framer
            oy.clear();
            this.sample_enumerator = null;
            s_err.WriteLine("Done.");
        }

		// FIXME: should be implemented, but can be done later.
		protected override void SwitchMediaStreamAsync (MediaStreamDescription mediaStreamDescription)
		{
			throw new System.NotImplementedException();
			// ReportSwitchMediaStreamCompleted (mediaStreamDescription);
		}

		// FIXME: should be implemented, but looks like it can be left as is
		// just to run decoder so far.
		protected override void SeekAsync (long seekToTime)
		{
			ReportSeekCompleted (seekToTime);
		}

		MediaStreamDescription audioStreamDescription;

		protected override void OpenMediaAsync ()
		{
            // Initialize data structures to pass to the Media pipeline via the MediaStreamSource
            Dictionary<MediaSourceAttributesKeys, string> mediaSourceAttributes = new Dictionary<MediaSourceAttributesKeys, string>();
            Dictionary<MediaStreamAttributeKeys, string> mediaStreamAttributes = new Dictionary<MediaStreamAttributeKeys, string>();
            List<MediaStreamDescription> mediaStreamDescriptions = new List<MediaStreamDescription>();

            // Initialize the Mp3 data structures used by the Media pipeline with state from the first frame.
            WaveFormatExtensible wfx = new WaveFormatExtensible();
            wfx.FormatTag = 1; // PCM
            wfx.Channels = 2;
            wfx.SamplesPerSec = 44100;
            wfx.AverageBytesPerSecond = 44100 * 2 * 2;
            wfx.BlockAlign = 4;
            wfx.BitsPerSample = 16;
            wfx.Size = 0;

            mediaStreamAttributes[MediaStreamAttributeKeys.CodecPrivateData] = wfx.ToHexString();
            this.audioStreamDescription = new MediaStreamDescription(MediaStreamType.Audio, mediaStreamAttributes);

            mediaStreamDescriptions.Add(this.audioStreamDescription);

			// <note>This part is mere copy of Mp3MediaStreamSource:
            // Setting a 0 duration to avoid the math to calcualte the Mp3 file length in minutes and seconds.
            // This was done just to simplify this initial version of the code for other people reading it.
            mediaSourceAttributes[MediaSourceAttributesKeys.Duration] = this.trackDuration.Ticks.ToString (CultureInfo.InvariantCulture);
            mediaSourceAttributes[MediaSourceAttributesKeys.CanSeek] = true.ToString ();
			// </note>

			sample_enumerator = this.DecodeSamples ().GetEnumerator ();
			this.ReportOpenMediaCompleted(mediaSourceAttributes, mediaStreamDescriptions);
		}

		Dictionary<MediaSampleAttributeKeys, string> emptyDict = new Dictionary<MediaSampleAttributeKeys, string>();
		IEnumerator<SampleBuffer> sample_enumerator;

		protected override void GetSampleAsync (MediaStreamType mediaStreamType)
		{
            MediaStreamSample audioSample = null;
			
            if (!sample_enumerator.MoveNext ())
            {
                // If you are near the end of the file, return a null stream, which
                // tells the MediaStreamSource and MediaElement to close down.
                audioSample = new MediaStreamSample(
                    this.audioStreamDescription,
                    null,
                    0,
                    0,
                    0,
                    emptyDict);
                this.ReportGetSampleCompleted(audioSample);
            }
            else
            {
				// FIXME: Stream should not be created every time.
				SampleBuffer buf = (SampleBuffer) sample_enumerator.Current;
                audioSample = new MediaStreamSample(
                    this.audioStreamDescription,
                    new MemoryStream (buf.Data, buf.Index, buf.Count, false),
                    buf.Index,
                    buf.Count,
                    timePosition,
                    emptyDict);
                timePosition += (long) buf.Count * 10000000L / (44100L * 2L * 2L);
                this.ReportGetSampleCompleted(audioSample);
            }
		}
        long timePosition;

		// FIXME: should be implemented, but can be done later.
		protected override void GetDiagnosticAsync (MediaStreamSourceDiagnosticKind diagnosticKind)
		{
			throw new System.NotImplementedException();
			// ReportGetDiagnosticCompleted(diagnosticKind, diagnosticValue);
		}
	}
}
