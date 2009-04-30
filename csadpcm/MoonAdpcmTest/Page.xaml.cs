﻿using System;
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
using MoonAdpcm;

namespace MoonAdpcmTest
{
    public partial class Page : UserControl
    {
        public Page()
        {
            InitializeComponent();
        }

        private void OpenMedia(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.ShowDialog();
            if (ofd.File == null)
                return;
            AdpcmMediaStreamSource mediaSource = new AdpcmMediaStreamSource(ofd.File.OpenRead());
            me.SetSource(mediaSource);
        }
    }
}
