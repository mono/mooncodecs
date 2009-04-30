using System;
namespace org.diracvideo.Jirac
{

    internal class Stream {
        private int prev;
        private Buffer next;
        private object obj = new object();

        public void Add(Buffer buf) {
            lock(obj)
            {
	            next = (next == null ? buf : next.Cat(buf));
            }
        }

        public Buffer Next() {
            lock (obj)
            {
                if (next == null) return null;
                int size = next.Size();
                if (size < 13) return null;
                if (next.GetInt(0) != 0x42424344)
                {
                    System.Console.WriteLine("Not reading a dirac stream");
                    next = null;
                    return null;
                }
                int offset = next.GetInt(5);
                if (offset == 0)
                    offset = 13;
                if (offset > size) return null;
                if (prev != next.GetInt(9))
                    throw new Exception();
                prev = offset;
                if (size == offset)
                {
                    Buffer tmp = next;
                    next = null;
                    return tmp;
                }
                else
                {
                    Buffer tmp = next.Sub(0, offset);
                    next = next.Sub(offset);
                    return tmp;
                }
            }
        }
    }
}