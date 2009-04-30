using System.Text;
using System;
namespace org.diracvideo.Jirac
{
    /** Vector
     *
     * The class representing a single motion vector
     * element.  */

    public class Vector
    {
        public int split, pred_mode;
        public int[] dx = new int[2];
        public int[] dy = new int[2];
        public int[] dc = new int[3];
        public bool namespace_global;

        public Vector Scale(int h_shift, int v_shift)
        {
            Vector o = new Vector();
            o.namespace_global = namespace_global;
            o.pred_mode = pred_mode;
            o.dx[0] = dx[0] >> h_shift;
            o.dx[1] = dx[1] >> h_shift;
            o.dy[0] = dy[0] >> v_shift;
            o.dy[1] = dy[1] >> v_shift;
            return o;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            if (!namespace_global)
            {
                switch (pred_mode)
                {
                    case 0:
                        sb.Append("Intra Vector\n");
                        for (int i = 0; i < 3; i++)
                            sb.Append(String.Format("DC[%d] = %d\n", i, dc[i]));
                        break;
                    case 1:
                        sb.Append("Ref1 Vector\n");
                        sb.Append(String.Format("dx[0] = %d\n", dx[0]));
                        sb.Append(String.Format("dy[0] = %d\n", dy[0]));
                        break;
                    case 2:
                        sb.Append("Ref2 Vector\n");
                        sb.Append(String.Format("dx[1] = %d\n", dx[1]));
                        sb.Append(String.Format("dy[1] = %d\n", dy[1]));
                        break;
                    case 3:
                        sb.Append("Ref1And2Vector\n");
                        for (int i = 0; i < 2; i++)
                        {
                            sb.Append(String.Format("dx[%d] = %d\n", i, dx[i]));
                            sb.Append(String.Format("dy[%d] = %d\n", i, dy[i]));
                        }
                        break;
                }

            }
            else
            {
                sb.Append("Global Vector\n");
            }
            return sb.ToString();
        }
    }
}