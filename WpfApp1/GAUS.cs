using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace WpfApp1
{
    public class GAUS
    {
        public GAUS() { }
        //public string[] GetInfo(double[][] matrix)
        //{
        //    int n = matrix.Length;
        //    for (int i = 0; i < n; i++)
        //    {
        //        for (int j = i + 1; j < n; j++)
        //        {
        //            double factor = matrix[j][i] / matrix[i][i];
        //            for (int k = i; k < n + 1; k++)
        //            {
        //                matrix[j][k] -= factor * matrix[i][k];
        //            }
        //        }
        //    }
        //    double[] result = new double[n];
        //    for (int i = n - 1; i >= 0; i--)
        //    {
        //        result[i] = matrix[i][n] / matrix[i][i];
        //        for (int j = i - 1; j >= 0; j--)
        //        {
        //            matrix[j][n] -= matrix[j][i] * result[i];
        //        }
        //    }

        //    string[] solutions = new string[n];
        //    for (int i = 0; i < n; i++)
        //    {
        //        solutions[i] = $"x{i + 1} = {result[i]:F2}";
        //    }

        //    return solutions;
        //}

        public string[] GetInfo1(double[][] matrix)
        {
            int n = matrix.Length;
            string[] result = new string[n];
            double[][] modifiedMatrix = new double[n][];
            for (int i = 0; i < n; i++)
            {
                modifiedMatrix[i] = new double[matrix[i].Length];
                matrix[i].CopyTo(modifiedMatrix[i], 0);
            }
            for (int i = 0; i < n; i++)
            {
                for (int j = i + 1; j < n; j++)
                {
                    double factor = modifiedMatrix[j][i] / modifiedMatrix[i][i];
                    for (int k = i; k < n + 1; k++)
                    {
                        modifiedMatrix[j][k] -= factor * modifiedMatrix[i][k];
                    }
                    result[j] = string.Join(", ", modifiedMatrix[j]);
                }
            }
            for (int i = n - 1; i >= 0; i--)
            {
                result[i] = (modifiedMatrix[i][n] / modifiedMatrix[i][i]).ToString("F2");
                for (int j = i - 1; j >= 0; j--)
                {
                    modifiedMatrix[j][n] -= modifiedMatrix[j][i] * double.Parse(result[i]);
                    result[j] = string.Join(", ", modifiedMatrix[j]);
                }
            }
            return result;
        }

        public string[][] GetInfo2(double[][] matrix)
        {
            int n = matrix.Length;
            string[][] results = new string[n][];

            double[][] historyMatrix = new double[n][];
            for (int i = 0; i < n; i++)
            {
                historyMatrix[i] = new double[matrix[i].Length];
                matrix[i].CopyTo(historyMatrix[i], 0);
            }

            for (int i = 0; i < n; i++)
            {
                for (int j = i + 1; j < n; j++)
                {
                    double factor = matrix[j][i] / matrix[i][i];
                    for (int k = i; k < n + 1; k++)
                    {
                        matrix[j][k] -= factor * matrix[i][k];
                    }
                    results[j] = new string[n];
                    for (int l = 0; l < n; l++)
                    {
                        results[j][l] = string.Join(", ", matrix[j]);
                    }
                }
            }

            for (int i = n - 1; i >= 0; i--)
            {
                results[i] = new string[n];
                results[i][n - 1] = (matrix[i][n] / matrix[i][i]).ToString("F2");
                for (int j = i - 1; j >= 0; j--)
                {
                    matrix[j][n] -= matrix[j][i] * double.Parse(results[i][n - 1]);
                    results[j] = new string[n];
                    for (int k = 0; k < n; k++)
                    {
                        results[j][k] = string.Join(", ", matrix[j]);
                    }
                }
                // Store the current state of the matrix in history
                historyMatrix[i] = new double[matrix[i].Length];
                matrix[i].CopyTo(historyMatrix[i], 0);
                results[i][n - 1] += ", Solution: " + results[i][n - 1];
            }

            string[][] finalResults = new string[n][];
            for (int i = 0; i < n; i++)
            {
                finalResults[i] = new string[n];
                finalResults[i] = results[i];
            }

            Console.WriteLine("History of matrix modifications:");
            foreach (var row in historyMatrix)
            {
                Console.WriteLine(string.Join(", ", row));
            }

            return finalResults;
        }


        public List<string> GetInfo(double[][] matrix)
        {
            int n = matrix.Length;
            List<string> history = new List<string> ();

            double[][] historyMatrix = new double[n][];
            for (int i = 0; i < n; i++)
            {
                historyMatrix[i] = new double[matrix[i].Length];
                matrix[i].CopyTo(historyMatrix[i], 0);
            }

            for (int i = 0; i < n; i++)
            {
                history.Add("Matrix at step " + i + ":");
                for (int j = 0; j < n; j++)
                {
                    history.Add($"Row {j}: {string.Join(", ", matrix[j])}");
                }
                for (int j = i + 1; j < n; j++)
                {
                    history.Add("Changing row " + j + ": " + string.Join(", ", matrix[j]));
                    double factor = matrix[j][i] / matrix[i][i];
                    for (int k = i; k < n + 1; k++)
                    {
                        matrix[j][k] -= factor * matrix[i][k];
                    }
                    history.Add("Modified row " + j + ": " + string.Join(", ", matrix[j]));
                }
            }

            for (int i = n - 1; i >= 0; i--)
            {
                for (int j = i - 1; j >= 0; j--)
                {
                    matrix[j][n] -= matrix[j][i] * (matrix[i][n] / matrix[i][i]);
                }
                matrix[i][n] /= matrix[i][i];
            }

            history.Add("Matrix at final step:");
            for (int j = 0; j < n; j++)
            {
                history.Add($"Row {j}: {string.Join(", ", matrix[j])}");
            }
            history.Add("final vector");
            int last = matrix[0].Length-1;
            for(int i = 0; i < n; ++i)
            {
                history.Add($"X {i}: {string.Join(", ", matrix[i][last])}");
            }
            return history;
        }
    }
}