using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;

namespace GridWorld
{
    class Tuple<T1, T2> //: IEquatable<Tuple<T1, T2>>
    {
        public T1 Item1;
        public T2 Item2;

        public Tuple(T1 i1, T2 i2) {
            this.Item1 = i1;
            this.Item2 = i2;
        }
   
        public override bool Equals(Object obj) {
            if (obj == null)
            {
                return false;
            }

            Tuple<T1, T2> t2 = obj as Tuple<T1, T2>;
            if ((System.Object)t2 == null)
            {
                return false;
            }
            
            return (this.Item1.Equals(t2.Item1) && this.Item2.Equals(t2.Item2));
        }

        public bool Equals(Tuple<T1, T2> t2)
        {
            return (this.Item1.Equals(t2.Item1) && this.Item2.Equals(t2.Item2));
        }
    }
}
