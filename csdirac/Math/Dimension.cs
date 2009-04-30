using System;
using System.Collections.Generic;
using System.Text;

namespace org.diracvideo.Math
{
    public class Dimension
    {
        private int width;
        private int height;
        public Dimension(int width, int height)
        {
            this.width = width;
            this.height = height;
        }
        public int Width { get { return width; } set { width = value; } }
        public int Height { get { return height; } set { height = value; } }
    }
}
