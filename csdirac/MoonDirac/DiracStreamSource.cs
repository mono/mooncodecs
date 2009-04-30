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
        public DiracStreamSource(Stream videoStream)
        {
            this.videoStream = videoStream;
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
            throw new NotImplementedException();
        }
        private MediaStreamDescription streamDescription;
        private Stream videoStream;

        protected override void OpenMediaAsync()
        {
            // Initialize data structures to pass to the Media pipeline via the MediaStreamSource
            Dictionary<MediaSourceAttributesKeys, string> mediaSourceAttributes = new Dictionary<MediaSourceAttributesKeys, string>();
            Dictionary<MediaStreamAttributeKeys, string> mediaStreamAttributes = new Dictionary<MediaStreamAttributeKeys, string>();
            List<MediaStreamDescription> mediaStreamDescriptions = new List<MediaStreamDescription>();

            // Pull in the entire Audio stream.
            byte[] videoData = new byte[this.videoStream.Length];
            if (videoData.Length != this.videoStream.Read(videoData, 0, videoData.Length))
            {
                throw new IOException("Could not read in the VideoStream");
            }
            //TODO parse until first frame
            //todo find what is the offset of first frame and put it in push len param
            org.diracvideo.Jirac.Decoder dec = new org.diracvideo.Jirac.Decoder();
            dec.Push(videoData, 0, videoData.Length);
            dec.Decode();
            
            mediaStreamAttributes[MediaStreamAttributeKeys.CodecPrivateData] = dec.format.ToString();
            this.streamDescription = new MediaStreamDescription(MediaStreamType.Video, mediaStreamAttributes);

            mediaStreamDescriptions.Add(streamDescription);

            // Setting a 0 duration to avoid the math to calcualte the Mp3 file length in minutes and seconds.
            // This was done just to simplify this initial version of the code for other people reading it.
            mediaSourceAttributes[MediaSourceAttributesKeys.Duration] = TimeSpan.FromMinutes(5).Ticks.ToString(CultureInfo.InvariantCulture);
            mediaSourceAttributes[MediaSourceAttributesKeys.CanSeek] = "0";

            // Report that the DiracMediaStreamSource has finished initializing its internal state and can now
            // pass in Dirac Samples.
            this.ReportOpenMediaCompleted(mediaSourceAttributes, mediaStreamDescriptions);

            //this.currentFrameStartPosition = result;
            //this.currentFrameSize = mpegLayer3Frame.FrameSize;
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
