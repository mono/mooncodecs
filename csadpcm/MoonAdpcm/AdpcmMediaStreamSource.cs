
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows.Media;
using MediaParsers;
using csadpcm;

namespace MoonAdpcm
{
    class DebugWriter : TextWriter
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

    public class AdpcmMediaStreamSource : MediaStreamSource
	{
		ImaAdpcm source;

//		int convsize=4096*2;
		//byte[] convbuffer=new byte[convsize]; // take 8k out of the data segment, not the stack
//		byte[] convbuffer;

        TextWriter s_err = new DebugWriter(); // Console.Error;
//		Stream input;

		public AdpcmMediaStreamSource (Stream input)
		{
			/*
			convbuffer=new byte[convsize];
			vb = new Block (vd);
			if (input == null)
				throw new ArgumentNullException ("input");
			this.input = input;
			*/
			source = new ImaAdpcm (input);
		}

		IEnumerable<SampleBuffer> DecodeSamples ()
		{
			foreach (byte [] buf in source.DecodeSamples ())
				yield return new SampleBuffer (buf, 0, buf.Length);
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
            wfx.Channels = (short) this.source.Channels;
            wfx.SamplesPerSec = this.source.SamplesPerSec;
            wfx.BlockAlign = (short) (this.source.Channels * 2);
            wfx.BitsPerSample = 16;
            wfx.AverageBytesPerSecond = wfx.SamplesPerSec * wfx.Channels * 2;
            wfx.Size = 0;

            mediaStreamAttributes[MediaStreamAttributeKeys.CodecPrivateData] = wfx.ToHexString();
            this.audioStreamDescription = new MediaStreamDescription(MediaStreamType.Audio, mediaStreamAttributes);

            mediaStreamDescriptions.Add(this.audioStreamDescription);

			// <note>This part is mere copy of Mp3MediaStreamSource:
            // Setting a 0 duration to avoid the math to calcualte the Mp3 file length in minutes and seconds.
            // This was done just to simplify this initial version of the code for other people reading it.
            mediaSourceAttributes[MediaSourceAttributesKeys.Duration] = TimeSpan.FromMinutes(0).Ticks.ToString (CultureInfo.InvariantCulture);
            mediaSourceAttributes[MediaSourceAttributesKeys.CanSeek] = false.ToString ();
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
                timePosition += buf.Count * 10000000 / (44100 * 2 * 2);
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
