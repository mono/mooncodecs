using org.diracvideo.Jirac;
using NUnit.Framework;
using System.IO;
using System;
using System.Drawing;
using System.Windows.Forms;
using System.Threading;

namespace org.diracvideo.Jirac.Test
{
    [TestFixture]
    public class DecoderTest {
        [Test]
        public void main() {
            //TODO dialogue to ask string
            string[] a = new string[0];

	        Decoder dec = new Decoder();
	        int ev = 0;
	        FileStream input = null;
	        Form win;
	        try {
	            input = tryOpen(a);
	            byte[] packet;
	            while(dec.format == null) {
		            packet = readPacket(input);
		            dec.Push(packet, 0, packet.Length);
	            }
                ThreadPool.QueueUserWorkItem(new WaitCallback(delegate(object o) { dec.Decode(); }));

	            win = createWindow(dec);
	            while(input.Length > 0 && !dec.Done()) {
		            packet = readPacket(input);
		            dec.Push(packet, 0, packet.Length);
	            }
	            dec.status = Decoder.Status.DONE;
	            input.Close();
	            win.Visible = false;
	            win.Dispose();
	        } catch(Exception e) {
	            Console.WriteLine(e);
	            ev = 1;
	        } finally { 
	            Environment.Exit(ev);
	        }
        }

        private byte[] readPacket(FileStream input) {
	        if(true) {
                int read = (int)System.Math.Min(input.Length, 100);
                byte[] packet = new byte[read];
	            input.Read(packet, 0, read);
	            return packet;
	        } else {
	            byte[] header = new byte[13];
	            input.Read(header, 0, 13);
	            Unpack u = new Unpack(header);
	            if(u.DecodeLit32() != 0x42424344) {
		            throw new IOException("Cannot parse dirac stream");
	            } 
	            if(u.Bits(8) == 0x10) {
		            return header;
	            }
	            int size = u.DecodeLit32();
	            byte[] packet = new byte[size];
	            Array.Copy(header, 0, packet, 0, 13);
	            input.Read(packet, 13, size - 13);
	            return packet;
	        }
        }


        private FileStream tryOpen(string[] a) {
            foreach (string f in a)
            {
                if (File.Exists(f))
                {
                    try
                    {
                        return new FileStream(f, FileMode.Open);
                    }
                    catch (IOException e)
                    {
                        Console.WriteLine(e);
                    }
                }
            }

            foreach (string f in Directory.GetFiles("."))
            {
                if (f.Length == f.LastIndexOf(".drc") + 4 && File.Exists(f))
                {
                    try
                    {
                        return new FileStream(f, FileMode.Open);
                    }
                    catch (IOException e)
                    {
                        Console.WriteLine(e);
                    }
                }
	        }
	        Console.WriteLine("No dirac file was found");
	        Environment.Exit(0);
	        return null;
        }

        private Form createWindow(Decoder dec)
        {
            Form fr = new Form();
            fr.FormClosed += new FormClosedEventHandler(delegate(object sender, FormClosedEventArgs args) { Environment.Exit(0); });
            PictureBox b = new PictureBox();
            b.Size = new Size(dec.format.width, dec.format.height);
            b.Paint += new PaintEventHandler(delegate(object sender, PaintEventArgs args) {
                //Need b.Invoke or not?
                int wait = (1000 * dec.format.frame_rate_denominator) / dec.format.frame_rate_numerator;

                Picture pic = dec.Pull();
                while (pic.status != Decoder.Status.DONE)
                {
                    if (pic != null &&
                       pic.error == null)
                    {
                        b.Image = pic.GetImage();
                    }
                    Thread.Sleep(wait);
                }
            });
            fr.Controls.Add(b);
            fr.Show();
	        return fr;
        }
    }
}