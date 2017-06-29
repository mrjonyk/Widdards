using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.Shuttler.Widdards
{
    public class CyclicList<T> : List<T>
    {
        private int offset = 0;

        public int Offset
        {
            get { return offset; }
            set
            {
                offset = Modulo(value);
            }
        }

        public CyclicList() : base() { }
        public CyclicList(CyclicList<T> list) : base(list)
        {
            offset = list.offset;
        }

        public new T this[int i]
        {
            get {
                return base[Modulo(i + offset)]; }
            set
            {
                base[Modulo(i + offset)] = value;
            }
        }

        public int GetActualIndex(int index)
        {
            return Modulo(index + offset);
        }
        
        public int Modulo(int num)
        {
            while (num < 0)
            {
                num += Count;
            }
            while (num >= Count)
            {
                num -= Count;
            }
            return num;
        }
    }
}
