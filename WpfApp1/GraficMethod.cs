using Newtonsoft.Json.Linq;
using OxyPlot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace WpfApp1
{
    public class GraficMethod
    {
        private string NameResult = "max";
        private Fraction paintLong = new Fraction(10);//max значение на графике в 1 четверти
        private Fraction[,] curGausArray = new Fraction[1,1];
        private Fraction[,] firstArray = new Fraction[1,1];
        private List<int> curCol = new List<int>();
        private List<(int y, int x, double resultPoint)> pointFuncList = new List<(int y, int x, double resultPoint)>();
        private (int y, int x, double resultPoint) pointMaxFunc;
        
        private List<(double y, double x, bool LeftRight)> pointLineVertical = new List<(double y, double x, bool LeftRight)>();
        private List<(double y, double x, bool UpDown)> pointLineHorizintal = new List<(double y, double x, bool UpDown)>();
        private List<(double y, double x, bool UpDown, bool LeftRight)> pointLineStandart = new List<(double y, double x, bool UpDown, bool LeftRight)>();
        public GraficMethod()
        {
        }
        public GraficMethod(Fraction[,] array)
        {
            firstArray = array;
            GausSimplex res = new GausSimplex(array);
            this.curGausArray = res.getArray();
        }
        public void setArray(Fraction[,] array, int[] basicVars)
        {//новый расчет функциий
            firstArray = array;
            //задача условие
            NameResult = array[0, array.GetLength(1) - 1].CharValue;

            if (array.GetLength(1) - 1 < 3)
            {
                this.curGausArray = CopyArray(array);
                //получить единицы в гаусе
                create(FindUnitVariablesWithUniqueColumn());
            }
            else
            {
                if (true)
                {
                    GausSimplex res = new GausSimplex(array, basicVars);
                    this.curGausArray = res.getArrayWithoutPivot();
                    //see();
                    //получить единицы в гаусе
                    create(FindUnitVariablesWithUniqueColumn());
                }
            }
            CreatePoint();
        }


        internal List<(int row, int col)> FindUnitVariablesWithUniqueColumn()
        {
            int rows = curGausArray.GetLength(0);
            int cols = curGausArray.GetLength(1);
            List<(int row, int col)> unitVariables = new List<(int row, int col)>();

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols - 1; j++)
                {
                    if (curGausArray[i, j].Numerator == 1 && curGausArray[i, j].Denominator == 1)
                    {
                        bool isUnique = true;
                        for (int k = 0; k < rows; k++)
                        {
                            if (k != i && curGausArray[k, j].Numerator != 0)
                            {
                                isUnique = false;
                                break;
                            }
                        }

                        if (isUnique)
                        {
                            unitVariables.Add((i, j));
                        }
                    }
                }
            }
            return unitVariables;
        }
        private void see()
        {
            Console.WriteLine();
            for (int i = 0; i < curGausArray.GetLength(0); ++i)
            {
                for (int j = 0; j < curGausArray.GetLength(1); ++j)
                {
                    Console.Write(curGausArray[i, j] + " ");
                }
                Console.WriteLine();
            }
        }
        private void create(List<(int row, int col)> list)
        {
            //перенос значений за =
            for(int i = 0; i < curGausArray.GetLength(0); ++i)
            {
                for (int j = 0; j < curGausArray.GetLength(1)-1; ++j)
                {
                    if (!list.Contains((i,j)))
                    {
                        curGausArray[i, j] *= new Fraction(-1);
                    }
                }
            }
            //получить выраженные x -> 0 = x1[0]
            List<int> colIndices = list.Select(v => v.col).ToList();
            List<int> rowlIndices = list.Select(v => v.row).ToList();
            //основныее столбцы(x1, x2)// выбрать столбцы которые не входят в единичную матрицу после гауса
            if (curGausArray.GetLength(1) - 1 < 3)
            {//случай для x1,x2,F
                curCol.Add(0);
                curCol.Add(1);
                //заменить min на 0
                firstArray[0, firstArray.GetLength(1) - 1] = new Fraction(0, 0);
                //выражать в исходном уравнении не нужно.
                //step1() | пропуск шага
                //Занулять не нужно. Пропуск шага
                //ограничения подходят без изменения. второй шаг пропуск
                //step2();
            }
            else
            {//для x1,x2,...,F
                for(int i = 0; i < curGausArray.GetLength(1)-1; ++i)
                {
                    if (!colIndices.Contains(i))
                    {
                        curCol.Add(i);
                    }
                }
                //заменить min на 0
                firstArray[0, firstArray.GetLength(1) - 1] = new Fraction(0, 0);
                //работает только для уравнения с 3 переменными (x1,x2,x3,F)
                step1(0 ,colIndices, rowlIndices, CopyArray(curGausArray));
                //занулить x которые будут выражеными в основном уравнении
                // x1 + x2 + x3 = F || gaus -> x1 = x2 + x3 |-> в гаусе x1 зануляем и подставляем в исходное
                // (x2 + x3) + x2 + x3 = F.
                for (int i = 0; i < list.Count(); ++i)
                {
                    curGausArray[list[i].row, list[i].col] = new Fraction(0, 0);
                }
                step2();
            }
        }
        private void step1(int flagRow, List<int> colIndices, List<int> rowlIndices, Fraction[,] tmpArray)
        {
            //*
            //подготовка выраженных x из гауса для подставления в исходное уравнение
            for (int i = 0; i < firstArray.GetLength(1); ++i)
            {
                if (colIndices.Contains(i))
                {//находим индексы единичной матрицы
                    int index = colIndices.FindIndex(v => v == i);
                    int rowI = rowlIndices[index];
                    //в решенном гауса домножаем те столбцы в нужной строке которые будут подставляться вместо переменной
                    // x1 ->1, x2 -> 0, x3 -> 1, x4 -> 1, F -> 2. Выражать будем x1
                    // в исходном уравнении x1 -> 2x1// значит до множаем всю строку на 2
                    for (int j = 0; j < tmpArray.GetLength(1); ++j)
                    {
                        tmpArray[rowI, j] *= firstArray[flagRow, i];
                    }
                }

            }
            //выразить x в осн. уравнении 
            for (int i = 0; i < tmpArray.GetLength(0); ++i)
            {
                for (int el = 0; el < tmpArray.GetLength(1); ++el)
                {
                    firstArray[flagRow, el] += tmpArray[i, el];
                }
            }
            //занулить переменные, которые были выражены
            for (int i = 0; i < firstArray.GetLength(1) - 1; ++i)
            {
                if (colIndices.Contains(i))
                {
                    firstArray[flagRow, i] = new Fraction(0, 0);
                }
            }
        }
        private void step2()
        {
            //изменить исходные ограничения на новые, полученные 
            for (int i = 0; i < curGausArray.GetLength(0); ++i)
            {//пропустить первую строку в firstArray и получить новые значения для ограничений
                for (int el = 0; el < curGausArray.GetLength(1); ++el)
                {
                    firstArray[i+1, el] = curGausArray[i, el];
                }
            }
            //делаем перенос C за знак >= 0
            // -(x1 + x2 + x3) <= C
            for (int i = 1; i < firstArray.GetLength(0); ++i)
            {
                for (int el = 0; el < firstArray.GetLength(1)-1; ++el)
                {
                    firstArray[i, el] *= new Fraction(-1);
                }
            }
        }
        private void CreatePoint()
        {
            //рисуем график функций
            //curCol[0] - x//curCol[1] - y//curCol[..] - spam
            //0 -> left(y) //1 -> right(x)
            //List<(Fraction y, Fraction x, bool up)> unitVariables = new List<(Fraction y, Fraction x, bool up)>();
            //вектор нормали или анти-нормали n
            if (NameResult == "min")
            {
                pointLineStandart.Add((0, 0, false, false));
                pointLineStandart.Add((firstArray[0, curCol[0]].Decimal * (-1), firstArray[0, curCol[1]].Decimal * (-1), false, false));
                //pointLineStandart.Add((0, 0, false, false));
            }
            else if(NameResult == "max")
            {
                pointLineStandart.Add((0, 0, false, false));
                pointLineStandart.Add((firstArray[0, curCol[0]].Decimal, firstArray[0, curCol[1]].Decimal, false, false));
                //pointLineStandart.Add((0, 0, false,false));
            }

            //ограничения остальных уравнений
            for (int i = 1; i< firstArray.GetLength(0); ++i)
            {

                Fraction newVariable1 = firstArray[i, firstArray.GetLength(1)-1] / firstArray[i, curCol[0]];//x -> (x,0)
                Fraction newVariable2 = firstArray[i, firstArray.GetLength(1)-1] / firstArray[i, curCol[1]];//y -> (0,y)
                if (firstArray[i, firstArray.GetLength(1) - 1].Decimal == 0)
                {
                    newVariable1 = new Fraction(1);
                    newVariable2 = new Fraction(1);
                    isLine(newVariable1,newVariable2, i);
                }
                else
                {
                    //изменяем максимальное значение графика
                    if(this.paintLong < newVariable1)
                    {
                        this.paintLong = newVariable1;
                    }
                    if(this.paintLong < newVariable2)
                    {
                        this.paintLong = newVariable2;
                    }
                    //нарисовать прямую
                    if (isEqualZero(newVariable1))
                    {
                        isLineHorizintalResult(newVariable2, i);
                    }
                    else if (isEqualZero(newVariable2))
                    {
                        isLineVerticalResult(newVariable1, i);
                    }
                    else
                    {
                        isLineStandart(newVariable1, newVariable2, i);
                    }
                }

            }
            //найти max результат для функции
            searchMaxResult();
        }

        private bool isEqualZero(Fraction newVariable1)
        {
            if (equal(newVariable1, new Fraction(0, 0)))
            {
                return true;
            }
            return false;
        }
        private void isLineVerticalResult(Fraction newVariable1, int i)
        {
            bool areaRes = areaVertical(newVariable1, firstArray[i, firstArray.GetLength(1) - 1]);
            pointLineVertical.Add( (newVariable1.Decimal, paintLong.Decimal, areaRes) );
            pointLineVertical.Add( (newVariable1.Decimal, 0, areaRes) );
        }
        private void isLineHorizintalResult(Fraction newVariable2, int i)
        {
            bool areaRes = areaVertical(newVariable2, firstArray[i, firstArray.GetLength(1) - 1]);
            pointLineHorizintal.Add((0, newVariable2.Decimal, areaRes));
            pointLineHorizintal.Add((paintLong.Decimal, newVariable2.Decimal, areaRes));
        }
        private void isLineStandart(Fraction newVariable1, Fraction newVariable2, int i)
        {
            if (newVariable1 < new Fraction(0, 0))//если линии не проходят до 1 четверти, дорисовать
            {
                Fraction tmp0 = firstArray[i, curCol[0]] * new Fraction(-1) * paintLong;    //переносим Cx -> ..= .. - Cx// и считаем C
                Fraction tmp1 = tmp0 + firstArray[i, firstArray.GetLength(1) - 1];          //прибавляем значение за = 1 + Cx
                Fraction tmp2 = tmp1 / firstArray[i, curCol[1]];                            //если |-> Yx = 1 + Cx // x = (1 + Cx)/Y
                newVariable1 = tmp2;

                bool areaPoint = pointUpLine((0, newVariable2.Decimal, false), (newVariable1.Decimal, paintLong.Decimal, false), (0, 0));
                bool areaRes = area( i, areaPoint);
                
                bool areaLeftRight = pointLeftRightLine((0, newVariable2.Decimal, false), (newVariable1.Decimal, paintLong.Decimal, false), (0, 0));
                areaLeftRight = area(i, areaLeftRight);

                pointLineStandart.Add( (0, newVariable2.Decimal, areaRes, areaLeftRight) );
                //менял x и y местами снизу!!!
                pointLineStandart.Add((paintLong.Decimal, newVariable1.Decimal, areaRes, areaLeftRight));
            }
            else if (newVariable2 < new Fraction(0, 0))
            {
                Fraction tmp0 = firstArray[i, curCol[1]] * new Fraction(-1) * paintLong;    //переносим Cx -> ..= .. - Cx// и считаем C
                Fraction tmp1 = tmp0 + firstArray[i, firstArray.GetLength(1) - 1];          //прибавляем значение за = 1 + Cx
                Fraction tmp2 = tmp1 / firstArray[i, curCol[0]];                            //если |-> Yx = 1 + Cx // x = (1 + Cx)/Y
                newVariable2 = tmp2;

                //если точка выше линии верни true
                bool areaPoint = pointUpLine((newVariable1.Decimal, 0, false), (newVariable2.Decimal, paintLong.Decimal, false), (0, 0));
                bool areaRes = area(i, areaPoint);
                //True -> на линии или справа. //False -> слева от линии
                bool areaLeftRight = pointLeftRightLine((newVariable1.Decimal, 0, false), (newVariable2.Decimal, paintLong.Decimal, false), (0, 0));
                areaLeftRight = area(i, areaLeftRight);

                pointLineStandart.Add((newVariable1.Decimal, 0, areaRes, areaLeftRight));
                pointLineStandart.Add((newVariable2.Decimal, paintLong.Decimal,  areaRes, areaLeftRight));
            }
            else
            {
                //если точка выше линии верни true
                bool areaPoint = pointUpLine( (newVariable1.Decimal, 0, false), (0, newVariable2.Decimal, false),(0, 0));
                bool areaRes = area(i, areaPoint);

                //True -> на линии или справа. //False -> слева от линии
                bool areaLeftRight = pointLeftRightLine((newVariable1.Decimal, 0, false), (newVariable2.Decimal, paintLong.Decimal, false), (0, 0));
                areaLeftRight = area(i, areaLeftRight);

                pointLineStandart.Add((newVariable1.Decimal, 0, areaRes, areaLeftRight));
                pointLineStandart.Add((0, newVariable2.Decimal, areaRes, areaLeftRight));
            }
        }
        private void isLine(Fraction newVariable1, Fraction newVariable2, int i)
        {   //-> y = x; 
            //если точка выше линии верни true
            bool areaPoint = pointUpLine((newVariable1.Decimal, 0, false), (0, newVariable2.Decimal, false), (0, 0));
            bool areaRes = area(i, areaPoint);

            //True -> на линии или справа. //False -> слева от линии
            bool areaLeftRight = pointLeftRightLine((newVariable1.Decimal, 0, false), (newVariable2.Decimal, paintLong.Decimal, false), (0, 0));
            areaLeftRight = area(i, areaLeftRight);

            pointLineStandart.Add((0, 0, areaRes, areaLeftRight));
            pointLineStandart.Add((paintLong.Decimal, paintLong.Decimal, areaRes, areaLeftRight));
        }

        //false закрасить ниже // true закрасить выше!
        private bool area( int rowIndex, bool pointUpLineRes)
        {
            Fraction tmpSum = firstArray[rowIndex, curCol[0]]* new Fraction(0,0);
            tmpSum = tmpSum + (firstArray[rowIndex, curCol[1]]* new Fraction(0,0));
            bool chek = tmpSum.Decimal <= firstArray[rowIndex, firstArray.GetLength(1) - 1].Decimal;

            //точка входит в область
            if (chek)
            {
                ////если точка выше линии верни false
                if (pointUpLineRes)
                {//точка входит в область, и точка выше линии
                    return true;
                }
                else
                {//..., точка ниже линии
                    return false;
                }
            }
            else//точка не входит в область
            {
                if (pointUpLineRes)
                {
                    //точка не входит в область, и точка выше линии
                    return false;
                }
                else
                {
                    //точка не входит в область, и точка ниже линии
                    return true;
                }
            }
        }
        //false закрасить левее // true закрасить правее!
        private bool areaVertical(Fraction yPoint, Fraction eq)
        {
            Fraction tmp = yPoint + new Fraction(1);
            if (tmp.Decimal >= eq.Decimal)
            {
                return false;
            }
            return true;
        }

        //private void searchMaxResult(List<(Fraction y, Fraction x, bool up)> pointList)
        private void searchMaxResult()
        {   double maxValue = 0;
            int maxIndexI = 0, maxIndexJ= 0;
            if (NameResult == "min")
            {
                maxValue = 9999;
            }
            else
            {
                maxValue = -999;
            }

            List<(int y, int x, double resultPoint)> unitVariables = new List<(int y, int x, double resultPoint)>();
            //пройти по всем R точкам x,y -> 0,...,10;0,...,10;
            for (int i = 0; i < paintLong.Numerator; ++i) {
                for (int j = 0; j < paintLong.Numerator; ++j) { 
                    //проверить точку на ее нахождении в области
                    if(chekPointArea((j, i)))
                    {
                        //получить результат этой точки
                        double tmp = searchPointResult(j, i);
                        unitVariables.Add( (j, i, tmp) );
                        if(NameResult == "min")
                        {
                            if (tmp < maxValue)
                            {
                                maxValue = tmp;
                                maxIndexI = i; maxIndexJ = j;
                            }
                        }
                        else
                        {
                            if (tmp > maxValue)
                            {
                                maxValue = tmp;
                                maxIndexI = i; maxIndexJ = j;
                            }
                        }
                    }
                } 
            }

            pointMaxFunc.y = maxIndexJ; pointMaxFunc.x = maxIndexI; pointMaxFunc.resultPoint = maxValue;
            pointFuncList = unitVariables;//точки 
        }
        //высчитать результат функции в точке
        private double searchPointResult(int x, int y)
        {
            Fraction xF = new Fraction(x);  Fraction yF = new Fraction(y);  Fraction sum = new Fraction(0, 0);
            //sum + x*C
            sum += (firstArray[0, curCol[0]] * yF);
            //sum + y*C
            sum += (firstArray[0, curCol[1]] * xF);
            //sum + c(F)
            sum += firstArray[0, firstArray.GetLength(1)-1];
            return sum.Decimal;
        }

        //проверить находится ли точка в нужной области ограничений// 
        private bool chekPointArea((int x, int y) pointFunc)
        {
            bool resultAllArea = chekPointAreaForLineStandart(pointFunc);
            if (!resultAllArea) return false;

            resultAllArea = chekPointAreaForLineHorizintal(pointFunc);
            if (!resultAllArea) return false;

            resultAllArea = chekPointAreaForLineVertical(pointFunc);
            return resultAllArea;
        }
        private bool chekPointAreaForLineStandart((int x, int y) pointFunc)
        {
            //pointList[pointIndex].up -> //false закрасить ниже // true закрасить выше
            //для одной точки пройтись по всем линиям ограничений.
            bool isNeedPoint = true;
            //проверить стандартные линии
            for (int pointIndex = 2; pointIndex < pointLineStandart.Count; pointIndex += 2)
            {
                //если точка выше линии верни true//иначе//false если точка на линии или ниже ее
                bool pointUL = pointUpLine(
                    (pointLineStandart[pointIndex].y, pointLineStandart[pointIndex].x, false),
                    (pointLineStandart[pointIndex + 1].y, pointLineStandart[pointIndex + 1].x, false),
                    pointFunc);

                if (pointUL && pointLineStandart[pointIndex].UpDown)
                {
                    //точка выше линии и область выше линии//return true;
                }
                else if (!pointUL && !pointLineStandart[pointIndex].UpDown)
                {
                    //точка ниже линии или на ней И область линии ниже//return true;
                }
                else
                {
                    return false;
                }
            }
            return isNeedPoint;
        }
        private bool chekPointAreaForLineVertical((int x, int y) pointFunc)
        {
            //pointList[pointIndex].up -> //false закрасить левее // true закрасить правее
            for (int pointIndex = 0; pointIndex < pointLineVertical.Count; pointIndex += 2)
            {
                //false точка слева от вертикали|| true справа
                bool pointUL = pointLeftRightLine(pointLineVertical[pointIndex], pointLineVertical[pointIndex + 1], pointFunc);
                bool pointOL = pointOnLine(pointLineVertical[pointIndex], pointLineVertical[pointIndex + 1], pointFunc);
                if (pointUL && pointLineVertical[pointIndex].LeftRight)
                {
                    //точка правее линии и область правее линии//return true;
                }
                else if (!pointUL && !pointLineVertical[pointIndex].LeftRight)
                {
                    //точка левее линии И область линии левее//return true;
                }
                else if (pointOL)
                {
                    //точка на линии всегда подходит.
                }
                else
                {
                    return false;
                }
            }
            return true;
        }
        private bool chekPointAreaForLineHorizintal((int x, int y) pointFunc)
        {
            //pointList[pointIndex].up -> //false закрасить ниже // true закрасить выше
            for (int pointIndex = 0; pointIndex < pointLineHorizintal.Count; pointIndex += 2)
            {
                //если точка выше линии верни false//иначе//true если точка на линии или ниже ее
                bool pointUL = pointUpLine(pointLineHorizintal[pointIndex], pointLineHorizintal[pointIndex + 1], pointFunc);
                bool pointOL = pointOnLine(pointLineHorizintal[pointIndex], pointLineHorizintal[pointIndex + 1], pointFunc);
                if (!pointUL && pointLineHorizintal[pointIndex].UpDown)
                {
                    //точка выше линии и область выше линии//return true;
                }
                else if (pointUL && !pointLineHorizintal[pointIndex].UpDown)
                {
                    //точка ниже линии или на ней И область линии ниже//return true;
                }
                else if (pointOL)
                {
                    //точка на линии всегда подходит.
                }
                else
                {
                    return false;
                }
            }
            return true;
        }

        //если точка выше линии верни false
        //public bool pointUpLine( 
        //    (double y, double x, bool up) pointLine1,
        //    (double y, double x, bool up) pointLine2, 
        //    (int x, int y) pointFunc)
        //{
        //    //True -> на линии или ниже.//False -> над линией
        //    double x1 = pointLine1.x, x2 = pointLine2.x, y1 = pointLine1.y, y2 = pointLine2.y;
        //    double x = pointFunc.x, y = pointFunc.y;
        //    // Вектор AB
        //    double dxAb = x2 - x1;
        //    double dyAb = y2 - y1;

        //    // Вектор AP
        //    double dxAp = x - x1;
        //    double dyAp = y - y1;

        //    // Векторное произведение векторов AB и AP
        //    double vectorProduct = dxAb * dyAp - dyAb * dxAp;

        //    // Если векторное произведение не равно нулю, то точка не лежит на прямой, проходящей через отрезок
        //    if (vectorProduct != 0)
        //    {
        //        // Если векторное произведение положительно, то точка находится слева от отрезка
        //        // Если векторное произведение отрицательно, то точка находится справа от отрезка
        //        return (vectorProduct >= 0); // 
        //    }
        //    else
        //    {
        //        // Точка лежит на прямой, проходящей через отрезок
        //        // Проверим, находится ли точка на отрезке
        //        if ((x1 <= x && x <= x2) || (x1 >= x && x >= x2))
        //        {
        //            if ((y1 <= y && y <= y2) || (y1 >= y && y >= y2))
        //            {
        //                return true;
        //            }
        //        }

        //        // Точка не лежит на отрезке, а лежит на его продолжении
        //        return false;
        //    }
        //}

        private bool pointUpLine(        //если точка выше линии верни true
            (double x, double y, bool up) pointLine1,
            (double x, double y, bool up) pointLine2,
            (double x, double y) pointFunc)
        {
            // Координаты точек линии
            double x1 = pointLine1.x, x2 = pointLine2.x, y1 = pointLine1.y, y2 = pointLine2.y;
            // Координаты точки, которую нужно проверить
            double x = pointFunc.x, y = pointFunc.y;

            // Вектор AB
            double dxAb = x2 - x1;
            double dyAb = y2 - y1;

            // Вектор AP
            double dxAp = x - x1;
            double dyAp = y - y1;

            // Векторное произведение векторов AB и AP
            double vectorProduct = dxAb * dyAp - dyAb * dxAp;
            // Если векторное произведение положительно, то точка находится выше линии
            // Если векторное произведение отрицательно или равно нулю, то точка находится ниже линии или на ней
            return (vectorProduct > 0);
        }


        //True -> на линии или справа. //False -> слева от линии
        public bool pointLeftRightLine(
            (double y, double x, bool up) pointLine1,
            (double y, double x, bool up) pointLine2,
            (int x, int y) pointFunc)
        {
            
            double x1 = pointLine1.x, x2 = pointLine2.x, y1 = pointLine1.y, y2 = pointLine2.y;
            double x = pointFunc.x, y = pointFunc.y;
            // Вектор AB
            double dxAb = x2 - x1;
            double dyAb = y2 - y1;

            // Вектор AP
            double dxAp = x - x1;
            double dyAp = y - y1;

            // Векторное произведение векторов AB и AP
            double vectorProduct = dxAb * dyAp - dyAb * dxAp;

            // Если векторное произведение не равно нулю, то точка не лежит на прямой, проходящей через отрезок
            if (vectorProduct != 0)
            {
                // Если векторное произведение положительно, то точка находится слева от отрезка
                // Если векторное произведение отрицательно, то точка находится справа от отрезка
                return vectorProduct <= 0;
            }
            else
            {
                // Точка лежит на прямой, проходящей через отрезок
                // Проверим, находится ли точка на отрезке
                if ((x1 <= x && x <= x2) || (x1 >= x && x >= x2))
                {
                    if ((y1 <= y && y <= y2) || (y1 >= y && y >= y2))
                    {
                        return true;
                    }
                }

                // Точка не лежит на отрезке, а лежит на его продолжении
                return false;
            }
        }

        private bool pointOnLine(
            (double y, double x, bool up) linePoint1,
            (double y, double x, bool up) linePoint2,
            (int x, int y) point)
        {
            // Vector AB
            double dxAb = linePoint2.x - linePoint1.x;
            double dyAb = linePoint2.y - linePoint1.y;

            // Vector AP
            double dxAp = point.x - linePoint1.x;
            double dyAp = point.y - linePoint1.y;

            // Cross product of vectors AB and AP
            double crossProduct = dxAb * dyAp - dyAb * dxAp;

            // If the cross product is zero, the point lies on the line passing through the segment
            if (crossProduct == 0)
            {
                // Check if the point lies on the segment
                if ((linePoint1.x <= point.x && point.x <= linePoint2.x) || (linePoint1.x >= point.x && point.x >= linePoint2.x))
                {
                    if ((linePoint1.y <= point.y && point.y <= linePoint2.y) || (linePoint1.y >= point.y && point.y >= linePoint2.y))
                    {
                        return true;
                    }
                }
            }
           // The point does not lie on the segment or its extension
            return false;
        }

        public List<int> getCurColl()
        {
            return this.curCol;
        }

        public List<(double y, double x, bool up, bool left)> getArrayStandart()
        {
            return pointLineStandart;
        }
        public List<(double y, double x, bool LeftRight)> getArrayVertical()
        {
            return pointLineVertical;
        }
        public List<(double y, double x, bool up)> getArrayHorizintal()
        {
            return pointLineHorizintal;
        }

        public Fraction[,] getArrayFirst()
        {
            return this.firstArray;
        }

        public string getNameResult()
        {
            return NameResult;
        }
        private bool equal(Fraction tmpF1, Fraction tmpF2)
        {
            Fraction f1 = new Fraction(tmpF1.Numerator, tmpF1.Denominator);
            Fraction f2 = new Fraction(tmpF2.Numerator, tmpF2.Denominator);
            if (f1.Numerator == f2.Numerator && f1.Denominator == f2.Denominator)
                return true;
            return false;
        }

        public List<(int y, int x, double resultPoint)> getPointsArray()
        {
            return pointFuncList;
        }

        public double getMax()
        {
            return paintLong.Decimal;
        }

        public (int y, int x, double resultPoint) getMaxFuncInGrafic()
        {
            return pointMaxFunc;
        }
        private Fraction[,] CopyArray(Fraction[,] copy)
        {
            Fraction[,] tmpCopy = new Fraction[copy.GetLength(0), copy.GetLength(1)];
            for (int i = 0; i < tmpCopy.GetLength(0); ++i)
            {
                for (int j = 0; j < tmpCopy.GetLength(1); ++j)
                {
                    if (copy[i, j] != null)
                    {
                        tmpCopy[i, j] = new Fraction(copy[i, j]);
                        if (copy[i, j].Type)
                        {
                            tmpCopy[i, j].Type = true;
                        }
                    }
                    else
                    {
                        tmpCopy[i, j] = null;
                    }
                }
            }
            return tmpCopy;
        }

    }
}
