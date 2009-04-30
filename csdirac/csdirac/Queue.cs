namespace org.diracvideo.Jirac
{

    /** Synchronized Picture Queue. */

    public class Node {
        public Picture load;
        public Node next;
    }

    public class Queue {
        protected Node free, head, tail;
        protected object obj = new object();

        public virtual void Push(Picture p)
        {
            lock (obj)
            {
	            Node nd;
	            if(free == null) 
	                free = new Node();
	            nd = free;
	            nd.load = p;
	            free = free.next;
	            if(head == null)
	                head = tail = nd;
	            else {
	                tail.next = nd;
	                tail = nd;
	            }
            }
        }

        public virtual Picture Pull()
        {
            lock (obj)
            {
	            if(Empty()) 
                    return null;
	            Node nd = head;
	            Picture p = nd.load;
	            head = head.next;
	            nd.load = null;
	            nd.next = free;
	            free = nd;
	            return p;
            }
        }

        public virtual bool Empty() {
	        return head == null;
        }
        
        public bool Cyclic() {
            lock (obj)
            {
	            if(head == null)
	                return false;
	            Node rabbit = head.next, turtle = head;
	            while(rabbit != null &&
	                  turtle != null) {
	                if(rabbit == turtle ||
	                   turtle.next == rabbit.next) 
		            return true;
	                turtle = turtle.next;
	                rabbit = rabbit.next;
	                if(rabbit != null)
		            rabbit = rabbit.next;
	            }
	            return false; 
            }
        }

    }

    internal class InputQueue : Queue {
        public override bool Empty() {
            lock (obj)
            {
	            for(Node nd = head; nd != null; nd = nd.next)
	                if(nd.load.Decodable()) return false;
	            return true; 
            }
        }

        public override Picture Pull()
        {
            lock(obj)
            {
	            Node pr = null;
	            for(Node nd = head; nd != null; nd = nd.next) 
	                if(nd.load.Decodable()) {
		            Picture pic = nd.load;
		            if(null == pr) head = nd.next;
		            else pr.next = nd.next;
		            nd.load = null;
		            nd.next = free;
		            free = nd;
		            return pic;
	                } else pr = nd;
	            return null;
            }
        }
    }

    class OutputQueue : Queue {
        public override void Push(Picture pic)
        {
            lock (obj)
            {
                if (free == null) free = new Node();
                Node nd = free; free = free.next;
                nd.load = pic;
                if (head == null)
                    head = tail = nd;
                else if (head.load.num > pic.num)
                {
                    nd.next = head; head = nd;
                }
                else if (tail.load.num < pic.num)
                {
                    tail.next = nd; tail = nd;
                }
                else 
                    for (Node pr = head, cmp = head.next;
                     cmp != null; cmp = cmp.next)
                        if (cmp.load.num > pic.num)
                        {
                            pr.next = nd;
                            nd.next = cmp;
                            break;
                        }
                        else pr = cmp;
            }
        }
    }
}