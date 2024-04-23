using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1
{
    internal class CanonicalViewArray
    {
        Fraction[,] Array;
        public CanonicalViewArray(Fraction[,] tmp) {
            Array = tmp;
        }

        public void AddEqualities()
        {
            Fraction[,] newArray = new Fraction[Array.GetLength(0),Array.GetLength(1)+1];
            for (int i = 0; i < Array.GetLength(0); i++)
            {
                for (int j = 0; j < Array.GetLength(1); j++)
                {
                    newArray[i,j] = Array[i,j];
                }
            }
            for (int i = 0; i < newArray.GetLength(0); i++) {
                newArray[i, newArray.GetLength(1) - 1] = newArray[i, newArray.GetLength(1)-2];
                newArray[i, newArray.GetLength(1) - 2] = new Fraction("=");
            }
            Array = newArray;
        }

        public void RemoveEqualities()
        {

        }

        public Fraction[,] getArray()
        {
            return Array;
        }
    }
}
