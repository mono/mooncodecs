using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;

namespace Wp7VorbisTest
{
    public partial class MainPage : PhoneApplicationPage
    {
        // Constructor
        public MainPage()
        {
            InitializeComponent();
        }

        private void PlayButton_TextInput(object sender, TextCompositionEventArgs e)
        {

        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            var wreq = WebRequest.Create(this.UrlInput.Text);
            wreq.BeginGetResponse (delegate (IAsyncResult result) {
                var wres = wreq.EndGetResponse(result);
                var stream = wres.GetResponseStream();
                var ogg = new MoonVorbis.OggMediaStreamSource(stream);
                Dispatcher.BeginInvoke (() => this.OggMediaElement.SetSource(ogg));
            }, null);
        }
    }
}