using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace WpfApp1
{
    internal class GausSimplex
    {
        Fraction[,] CurArray;
        public GausSimplex(Fraction[,] ArrayForm, int[] basicVars)
        {
            // Создаем новый массив без 0-й строки
            int rows = ArrayForm.GetLength(0);
            int cols = ArrayForm.GetLength(1);
            Fraction[,] newMatrix = new Fraction[rows - 1, cols];

            if (ArrayForm[0,cols-1].CharValue == "max")
            {
                for(int i = 0; i < rows-1; i++)
                {
                    ArrayForm[0, i] = ArrayForm[0, i] * new Fraction(-1);
                }
            }

            // Копируем все строки, кроме 0-й, в новый массив
            for (int i = 0; i < rows - 1; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    newMatrix[i, j] = ArrayForm[i + 1, j];
                }
            }
            Fraction[,] tmp = new Fraction[1, 1];

            tmp = GaussMethod(newMatrix, basicVars);
            // Копируем все строки, кроме 0-й, в новый массив
            for (int i = 1; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    ArrayForm[i, j] = tmp[i - 1, j];
                }
            }
            CurArray = ArrayForm;
        }
        public GausSimplex(Fraction[,] ArrayForm)
        {
            // Создаем новый массив без 0-й строки
            int rows = ArrayForm.GetLength(0);
            int cols = ArrayForm.GetLength(1);
            Fraction[,] newMatrix = new Fraction[rows - 1, cols];

            // Копируем все строки, кроме 0-й, в новый массив
            for (int i = 0; i < rows - 1; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    newMatrix[i, j] = ArrayForm[i + 1, j];
                }
            }

            Fraction[,] tmp = new Fraction[1, 1];
            tmp = GaussMethodWithoutPivot(newMatrix);

            CurArray = tmp;
        }
        Fraction[,] GaussMethod(Fraction[,] matrix, int[] basicVars)
        {
            //if (basicVars.Length != matrix.GetLength(0))
            //{
            //    MessageBox.Show("Количество базисных переменных должно быть равно количеству строк в матрице.");
            //    return null;
            //}

            // Приводим столбцы с базисными переменными к единичной матрице
            for (int i = 0; i < basicVars.Length; i++)
                //for (int i = 0; i < matrix.GetLength(0); i++)
            {
                int basicVarIndex = basicVars[i];
                // Индекс базисной переменной в текущей строке
                Fraction diagonalElement = matrix[i, basicVarIndex];
                if (diagonalElement.Numerator != 1)
                {
                    for (int j = 0; j < matrix.GetLength(1); j++)
                    {
                        matrix[i, j] /= diagonalElement;
                    }
                }
                Debug.WriteLine("2 " + i);
                mesCons(matrix);
                // Обнуляем остальные элементы в столбце
                for (int j = 0; j < matrix.GetLength(0); j++)
                {
                    if (j == i) continue;

                    Fraction coefficient = matrix[j, basicVarIndex];

                    if (coefficient.Numerator != 0)
                    {
                        mesCons(matrix);
                        for (int k = 0; k < matrix.GetLength(1); k++)
                        {
                            matrix[j, k] -= coefficient * matrix[i, k];
                            mesCons(matrix);

                        }
                    }
                }
                Debug.WriteLine("3 "+i);
                mesCons(matrix);
            }

            return matrix;
        }
        Fraction[,] GaussMethodWithoutPivot(Fraction[,] matrix)
        {
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);

            for (int i = 0; i < rows; i++)
            {
                Fraction diagonalElement = matrix[i, i];
                if (diagonalElement.Numerator != 1)
                {
                    for (int j = 0; j < cols; j++)
                    {
                        matrix[i, j] /= diagonalElement;
                    }
                }

                for (int j = 0; j < rows; j++)
                {
                    if (j == i) continue;

                    Fraction coefficient = matrix[j, i];

                    if (coefficient.Numerator != 0)
                    {
                        for (int k = 0; k < cols; k++)
                        {
                            matrix[j, k] -= coefficient * matrix[i, k];
                        }
                    }
                }
            }

            return matrix;
        }
        internal Fraction[,] getArray()
        {
            return CurArray;
        }

        public Fraction[,] getArrayWithoutPivot()
        {
            Fraction[,] tmp = new Fraction[CurArray.GetLength(0)-1, CurArray.GetLength(1)];
            for (int i = 0; i < CurArray.GetLength(0)-1; i++)
            {
                for (int j = 0; j < CurArray.GetLength(1); j++)
                {
                    tmp[i, j] = CurArray[i + 1, j];
                }
            }
            return tmp;
        }

        private void mesCons(Fraction[,] curArray)
        {
            Debug.WriteLine("\nNEW STEP...");
            for (int i = 0; i < curArray.GetLength(0); i++)
            {
                for (int j = 0; j < curArray.GetLength(1); ++j)
                {
                    Debug.Write(curArray[i, j] + " ");
                }
                Debug.WriteLine("");
            }
        }
    }
}
