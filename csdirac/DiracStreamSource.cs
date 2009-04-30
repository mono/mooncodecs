using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media;
using System.IO;
using System.Globalization;


namespace csdirac
{
	public class DiracStreamSource : MediaStreamSource
	{
		private long timestamp;
		private MediaStreamDescription streamDescription;
		private org.diracvideo.Jirac.Decoder dec;
		private Stream videoStream;
		private Dictionary<MediaSampleAttributeKeys, string> empty_dict;


		public DiracStreamSource(Stream videoStream)
		{
			this.videoStream = videoStream;
			this.timestamp = 0;
 			this.empty_dict = new Dictionary<MediaSampleAttributeKeys, string>();
		}

		protected override void CloseMedia()
		{
			throw new NotImplementedException();
		}

		protected override void GetDiagnosticAsync(MediaStreamSourceDiagnosticKind diagnosticKind)
		{
			throw new NotImplementedException();
		}

		protected override void GetSampleAsync(MediaStreamType mediaStreamType)
		{
			MemoryStream frame = new MemoryStream ();
			org.diracvideo.Jirac.Picture p = dec.Pull ();
			MediaStreamSample sample;
			int [] pixels;

			p.Decode ();
			pixels = p.GetImage ();

			foreach (int i in pixels)
				frame.Write (BitConverter.GetBytes (i), 0, 4);

			sample = new MediaStreamSample (streamDescription, frame, 0, frame.Length, timestamp, empty_dict);
 
			timestamp += 50;

			ReportGetSampleCompleted(sample);
		}

		protected override void OpenMediaAsync()
		{
			Dictionary<MediaSourceAttributesKeys, string> mediaSourceAttributes = new Dictionary<MediaSourceAttributesKeys, string>();
			Dictionary<MediaStreamAttributeKeys, string> mediaStreamAttributes = new Dictionary<MediaStreamAttributeKeys, string>();
			List<MediaStreamDescription> mediaStreamDescriptions = new List<MediaStreamDescription>();

			byte[] videoData = new byte[this.videoStream.Length];
			if (videoData.Length != this.videoStream.Read(videoData, 0, videoData.Length))
			{
				throw new IOException("Could not read in the VideoStream");
			}

			dec = new org.diracvideo.Jirac.Decoder();
			dec.Push(videoData, 0, videoData.Length);
			dec.Decode();
			
			mediaStreamAttributes[MediaStreamAttributeKeys.VideoFourCC] = "RGBA";
			mediaStreamAttributes[MediaStreamAttributeKeys.Height] = dec.format.width.ToString ();
			mediaStreamAttributes[MediaStreamAttributeKeys.Width] = dec.format.height.ToString ();

			this.streamDescription = new MediaStreamDescription(MediaStreamType.Video, mediaStreamAttributes);

			mediaStreamDescriptions.Add(streamDescription);

			mediaSourceAttributes[MediaSourceAttributesKeys.Duration] = TimeSpan.FromMinutes(5).Ticks.ToString(CultureInfo.InvariantCulture);
			mediaSourceAttributes[MediaSourceAttributesKeys.CanSeek] = false.ToString ();

			this.ReportOpenMediaCompleted(mediaSourceAttributes, mediaStreamDescriptions);
		}

		protected override void SeekAsync(long seekToTime)
		{
			throw new NotImplementedException();
		}

		protected override void SwitchMediaStreamAsync(MediaStreamDescription mediaStreamDescription)
		{
			throw new NotImplementedException();
		}
	}
}
