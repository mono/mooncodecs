namespace org.diracvideo.Jirac
{

    /** Global
     *
     * The class for global motion estimation */
    internal class Global
    {
        public int b0, b1;
        public int a_exp, a00, a01, a10, a11;
        public int c_exp, c0, c1;

        public void GetVector(Vector mv, int x, int y, int n)
        {
            int scale = (1 << c_exp) - (c0 * x + c1 * y);
            int dx = scale * (a00 * x + a01 * y + (1 << a_exp) * b0);
            int dy = scale * (a10 * x + a11 * y + (1 << a_exp) * b1);
            mv.dx[n] = dx >> (a_exp + c_exp);
            mv.dy[n] = dy >> (a_exp + c_exp);
        }
    }
}