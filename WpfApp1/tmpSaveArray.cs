using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1
{
    public class tmpSaveArray
    {
        public Fraction[,] Array;
        public tmpSaveArray(Fraction[,] newArray) {
            Array = newArray;
        }

        public Fraction[,] getArray()
        {
            return Array;
        }
        public void setArray(Fraction[,] array)
        {
            Array = array;
        }
    }
}
