using LiveChartsCore.Geo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace WpfApp1
{
    public class Simplex
    {
        private int[] tmpBasisElement;
        private Fraction[,] curSimplexArray;//read only
        private Fraction[,] historySimplexArray = new Fraction[1, 1];
        private int[] basicVars;
        private Fraction[] addedBasis = new Fraction[0];
        private bool answerFlag = false;
        public int CountStep = 0;
        public Simplex(Fraction[,] tmp, int[] basicVars)
        {
            //подготовка матрицы для симплекс метода
            this.basicVars = basicVars;
            GausSimplex res = new GausSimplex(tmp, basicVars);
            this.curSimplexArray = res.getArray();
        }
        public Simplex(Fraction[,] BasisArray, Fraction[]addedBasis, int flag, int[] SelectedBasisRowCol=null)
        {
            //flag:
            //0->решение задачи только симплексом(или решение сразу)
            //1->решение с иск. базисами на один шаг.вернуть новую симплекс таблицу
            //2->решение иск. базиса найденно. вернуть решение + симплекс таблицу
            //3->получить возможные опорные элементы для их выбора в решении. вернуть опорные элементы
            //4->использовать выбранные опорные элементы для решения симплекс таблиц
            //5->сделать холостой симп. шаг
            if (haveAnswer(BasisArray))
            {
                return;
            }
            this.curSimplexArray = BasisArray;
            this.addedBasis = addedBasis;
            if(flag == 3)
            {//получить базис для след шага
                tmpBasisElement = searchAllBasis(BasisArray);
            }
            //else if (flag == 4)
            else if (SelectedBasisRowCol != null)
            {
                curSimplexArray = calculationSimplex(BasisArray, flag, SelectedBasisRowCol);
            }
            else
            {//высчитать след. шаг
                curSimplexArray = calculationSimplex(BasisArray, flag);
            }
            
        }
        public Simplex() { }
        private Fraction[,] calculationSimplex(Fraction[,] TableSimplex, int BasisFlag = 0, int[] SelectedBasisRowCol = null)
        {
            //выбрать какую Точку остановки для задачи проверять.
            ///////////////////////////////////////////////////////
            if (BasisFlag > 0)
            {
                if(SelectedBasisRowCol != null)
                {
                    return SimplexStepForSelectBasis(TableSimplex, SelectedBasisRowCol);
                }
                else if (ChekStabilitySimplex(TableSimplex, BasisFlag))
                {//иск. базисов не осталось
                    //MessageBox.Show("все элементы в последней строке >= 0, то оптимальное решение найдено");
                    if (BasisFlag == 1)
                    {
                        return null;
                    }
                    else if (BasisFlag == 2)
                    {//решение основной задачи
                        
                        if (ChekStabilitySimplex(TableSimplex, 0))
                        {//решение найдено
                            createAnswer(TableSimplex);
                            return historySimplexArray;
                        }
                        else
                        {//еще не найдено
                            this.CountStep++;
                            TableSimplex = SimplexStepForSelectBasis(TableSimplex);
                            if (TableSimplex == null)
                                return null;//решения нет
                            if (ChekStabilitySimplex(TableSimplex, 0))
                            {//решение найдено
                                createCopyHistory(TableSimplex);
                                createAnswer(TableSimplex);
                                return historySimplexArray;
                            }//нужен еще шаг
                            return TableSimplex;
                        }
                    }
                }
                else
                {//иск. базисы существуют.
                    return SimplexStepForSelectBasis(TableSimplex, null, BasisFlag);
                }
            }
            else
            {
                if (ChekStabilitySimplex(TableSimplex, BasisFlag))
                {
                    MessageBox.Show("все элементы в последней строке >= 0, то оптимальное решение найдено");
                    return historySimplexArray;
                }
            }
            ///////////////////////////////////////////////////////

            //поиск опрного объекта
            Fraction[,] newTableSimplex = new Fraction[TableSimplex.GetLength(0), TableSimplex.GetLength(1)];
            //сохранить стартовое значение полей и столбцов
            for (int i = 0; i < TableSimplex.GetLength(1); ++i)
            {
                newTableSimplex[0, i] = TableSimplex[0, i];
            }
            for (int i = 1; i < TableSimplex.GetLength(0); ++i)
            {
                newTableSimplex[i, 0] = TableSimplex[i, 0];
            }

            int minIndexCol = 1, minIndexRow = 1;
            Fraction minValueCol = TableSimplex[TableSimplex.GetLength(0) - 1, minIndexCol];
            //ищем опорный столбец по функции(последней строке)
            for (int i = 2; i < TableSimplex.GetLength(1)-1; ++i)
            {//правило Бланда +(улучшение Модифицированное Правило Бланда -- добавление малого числа к коэффициентам в строке целевой функции перед выбором опорного столбца.)
                if (minValueCol > TableSimplex[TableSimplex.GetLength(0) - 1, i])
                {
                    if ( (minValueCol < new Fraction(0,0)) && (TableSimplex[TableSimplex.GetLength(0) - 1, i] < new Fraction(0, 0)))
                    {
                        if (minIndexCol > i)
                        {
                            minIndexCol = i;
                            minValueCol = TableSimplex[TableSimplex.GetLength(0) - 1, i];
                        }
                    }
                    else
                    {
                        minValueCol = TableSimplex[TableSimplex.GetLength(0) - 1, i];
                        minIndexCol = i;
                    }
                }
            }
            //если в исмплекс таблице последняя строка >= 0.

            //выбрать какую Точку остановки для задачи проверять.
            ///////////////////////////////////////////////////////
            if (BasisFlag > 0)
            {
                if (ChekStabilitySimplex(TableSimplex, BasisFlag))
                {
                    MessageBox.Show("все элементы в последней строке >= 0, то оптимальное решение найдено");
                    if (BasisFlag == 1)
                    {
                        return null;
                    }
                    else if (BasisFlag == 2)
                    {
                        createAnswer(TableSimplex);
                        return historySimplexArray;
                    }
                }
            }
            else
            {
                if (ChekStabilitySimplex(TableSimplex, BasisFlag))
                {
                    MessageBox.Show("все элементы в последней строке >= 0, то оптимальное решение найдено");
                    return historySimplexArray;
                }
            }
            ///////////////////////////////////////////////////////
            minValueCol = new Fraction(0, 0);

            //ищем опорный объект(0) -???-
            Fraction minValueRow = TableSimplex[minIndexRow, TableSimplex.GetLength(1) - 1] / TableSimplex[minIndexRow, minIndexCol];
            for (int i = 2; i < TableSimplex.GetLength(0)-1; ++i)//исключить F строчку снизу
            {
                Fraction nextValue = TableSimplex[i, TableSimplex.GetLength(1) - 1] / TableSimplex[i, minIndexCol];
                if(minValueRow < new Fraction(0, 0) || minValueRow.Numerator == 0){//всегда ли будет значение >=0 ???
                    minValueRow = nextValue; minIndexRow = i;
                }else if ((minValueRow > nextValue) && (nextValue > new Fraction(0, 0))){
                    minValueRow = nextValue;
                    minIndexRow = i;
                }
            }
            if(!(minValueRow > new Fraction(0, 0)))
            {// Если нет положительных элементов в опорном столбце, то решение неограниченно
                //MessageBox.Show("ERROR SIMPLEX ALL COL IS {-alfa <= 0}");
                MessageBox.Show("Решение неограниченно");
                return historySimplexArray;
            }
            //добавить опорный элемент.(2)
            minValueRow = new Fraction(1 / 1) / TableSimplex[minIndexRow, minIndexCol];

            newTableSimplex[minIndexRow, minIndexCol] = minValueRow;
            TableSimplex[minIndexRow, minIndexCol].Type=true;

            //шаг обычного симплекса
            //поменять x Свободный <--> x Базис (1)
            newTableSimplex[minIndexRow, 0] = TableSimplex[0, minIndexCol];
            newTableSimplex[0, minIndexCol] = TableSimplex[minIndexRow, 0];

            //новая опорная строка(3)
            for (int i = 1; i < TableSimplex.GetLongLength(1); ++i)
            {
                if (i != minIndexCol)
                {
                    newTableSimplex[minIndexRow, i] = minValueRow * TableSimplex[minIndexRow, i];
                }
            }
            //новый опорный столбец(4)
            Fraction minValueMinus = minValueRow * new Fraction(-1, 1);
            for (int i = 1; i < TableSimplex.GetLongLength(0); ++i)
            {
                if (i != minIndexRow)
                {
                    newTableSimplex[i, minIndexCol] = minValueMinus * TableSimplex[i, minIndexCol];
                }
            }
            //досчитать оставшееся(5)
            for (int i = 1; i < TableSimplex.GetLength(0); ++i)
            {
                if (i != minIndexRow)
                {
                    for (int j = 1; j < TableSimplex.GetLength(1); ++j)
                    {
                        if (newTableSimplex[i, j] == null)
                        {
                            newTableSimplex[i, j] = TableSimplex[i, j] - (TableSimplex[i, minIndexCol] * newTableSimplex[minIndexRow, j]);
                        }

                    }
                }
            }

            if (BasisFlag > 0 )
            {
                return newTableSimplex;
            }
            else
            {
                //добавить последний результат вычислений в историю всех результатов решений
                createCopyHistory(newTableSimplex);
                //если все P > 0 найден ответ
                if (ChekStabilitySimplex(newTableSimplex,BasisFlag))
                {
                    MessageBox.Show("все элементы в последней строке >= 0, то оптимальное решение найдено");
                    createAnswer();
                    return historySimplexArray;
                }//иначе избавиться от отрицательных в P
                return calculationSimplex(newTableSimplex);
            }
        }
        private bool haveAnswer(Fraction[,] tmp)
        {
            if (tmp is null)
                return true;
            if (tmp[tmp.GetLength(0) - 1, 0] != null && tmp[tmp.GetLength(0) - 2, 0] != null)
            {
                return true;
            }
            return false;
        }
        public bool ChekStabilitySimplex(Fraction[,] curArray, int TypeChek)
        {
            if(TypeChek == 0)
            {//для обычного симплекса
                // Этот метод проверяет устойчивость симплекса, т.е. проверяет, что все элементы в строке целевой функции неотрицательны
                // Если все элементы неотрицательны, то симплекс устойчив и метод возвращает true
                // Если хотя бы один элемент отрицателен, то симплекс неустойчив и метод возвращает false
                Fraction zeroPoint = new Fraction(0, 0);
                for (int i = 1; i < curArray.GetLength(1) - 1; ++i)
                {
                    // Проверяем, что i-ый элемент в строке целевой функции неотрицателен
                    if (curArray[curArray.GetLength(0) - 1, i] < zeroPoint)
                    {
                        // Если i-ый элемент отрицателен, то симплекс неустойчив
                        return false;
                    }
                }
                // Если все элементы неотрицательны, то симплекс устойчив
                return true;
            }
            else//поиск иск.базисов//fasle - есть базис//true - нет базисов
            {
                for (int i = 1; i < curArray.GetLength(0) - 1; ++i)
                {
                    for (int j = 0; j < addedBasis.GetLength(0); ++j)
                    {
                        if(curArray[i, 0] != null)
                        {
                            // Проверяем, что i-ый элемент в столбце не иск.базис
                            if (curArray[i, 0].CharValue == addedBasis[j].CharValue)
                            {
                                // Если i-ый элемент иск. базис
                                return false;
                            }
                        }
                    }
                }
                //иск. базисов нету
                return true;
            }
        }
        //переменные для иск.базиса
        Fraction needValue = new Fraction(0);
        int numColumn = -1, numRow = -1;
        int[] banColumn = null; int banColumnFlag = 0;//запоминть col в которых нельзя получить опорный элемент.
        private int[] searchAllBasis(Fraction[,] curArray)
        {//возвоащает лучший опорный элемент в каждом столбце . приоритет отдается иск. базису если появляется выбор
            int[] mas = new int[curArray.GetLength(1)];
            //строка
            for (int i = 1; i < curArray.GetLength(1) - 1; ++i)
            {
                if (curArray[curArray.GetLength(0) - 1, i] < new Fraction(0, 0))
                {//подходящий столбец
                    mas[i] = 99;
                }
                else
                {//исключить столбцы из выборки
                    mas[i] = 0;
                }
            }
            //столбцы
            Fraction minRow = new Fraction(999);
            for (int i = 1; i < mas.GetLength(0)-1; ++i)
            {
                if (mas[i] == 99)
                {
                    bool flagBasisOnly = false;
                    //приоритет имеют иск. базисы
                    for (int irow = 1; irow < curArray.GetLength(0) - 1; ++irow)
                    {
                        if (isAddedBasis(curArray[irow, 0]))
                        {
                            Fraction tmp = curArray[irow, curArray.GetLength(1) - 1] / curArray[irow, i];
                            if (tmp < minRow && curArray[irow, i] > new Fraction(0, 0))
                            {
                                flagBasisOnly = true;
                                mas[i] = irow;
                                minRow = curArray[irow, curArray.GetLength(1) - 1] / curArray[irow, i];
                            }
                        }
                    }
                    if (!flagBasisOnly)
                    {
                        minRow = new Fraction(999);
                        for (int row = 1; row < curArray.GetLength(0) - 1; ++row)
                        {
                            Fraction tmp = curArray[row, curArray.GetLength(1) - 1] / curArray[row, i];
                            if (tmp < minRow && curArray[row, i] > new Fraction(0,0))
                            {
                                mas[i] = row;
                                minRow = curArray[row, curArray.GetLength(1) - 1] / curArray[row, i];
                            }
                        }
                    }

                    if (mas[i] == 99)
                        mas[i] = 0;
                    minRow = new Fraction(999);
                }
            }
            return mas;
        }
        //поиск первого попавшегося иск. базиса
        private bool searchBetterBasis(Fraction[,] curArray)
        {
            ///выход из рекурсии
            if(banColumnFlag == banColumn.GetLength(0))
            {
                return false;
            }
            Fraction minRow = new Fraction(99);
            numColumn = -1;
            //строка
            for (int i = 1; i < curArray.GetLength(1) - 1; ++i)
            {
                if (curArray[curArray.GetLength(0) - 1, i] < minRow && curArray[curArray.GetLength(0) - 1, i] < new Fraction(0,0))
                {
                    if (!(banColumn.Contains(i)))
                    {
                        numColumn = i;
                        minRow = curArray[curArray.GetLength(0) - 1, i];
                    }
                }
            }
            //столбец
            //нужна ли проверка что выбран базис когда нет иск. базисов?
            numRow = -1;
            minRow = new Fraction(99);
            if(numColumn != -1)
            {
                bool flagBasisOnly = false;
                //иск. базис элемент если он подходит addedBasis[]
                for (int i = 1; i < curArray.GetLength(0) - 1; ++i)
                {
                    if (isAddedBasis(curArray[i,0]))
                    {
                        Fraction tmp = curArray[i, curArray.GetLength(1) - 1] / curArray[i, numColumn];
                        if (tmp < minRow && curArray[i, numColumn] > new Fraction(0, 0))
                        {
                            flagBasisOnly = true;
                            numRow = i;
                            needValue = curArray[numRow, numColumn];
                            minRow = curArray[numRow, curArray.GetLength(1) - 1] / curArray[numRow, numColumn];
                        }
                    }
                }
                if (!flagBasisOnly)
                {
                    numRow = -1;
                    minRow = new Fraction(99);
                    //первый лучший опорный элемент
                    for (int i = 1; i < curArray.GetLength(0) - 1; ++i)
                    {
                        Fraction tmp = curArray[i, curArray.GetLength(1) - 1] / curArray[i, numColumn];
                        if (tmp < minRow && curArray[i, numColumn] > new Fraction(0, 0))
                        {
                            numRow = i;
                            needValue = curArray[numRow, numColumn];
                            minRow = curArray[numRow, curArray.GetLength(1)-1] / curArray[numRow, numColumn];
                        }
                    }
                }
            }
 
            if(numRow == -1)
            {//проверять пока не найдется нужный опорный эл.
                banColumn[banColumnFlag] = numColumn;
                ++banColumnFlag;
                //рекурсия вход
                return searchBetterBasis(curArray);
            }
            else if(numColumn == -1)
            {//опорных эл. нетт
                MessageBox.Show("Нет решения.");
                return false;
            }//опорный эл. найден
            return true;
        }

        private bool isAddedBasis(Fraction element)
        {
            for(int i = 0; i < this.addedBasis.GetLength(0); ++i)
            {
                if(element.CharValue == this.addedBasis[i].CharValue)
                {
                    return true;
                }
            }
            return false;
        }
        public Fraction[,] SimplexStepForSelectBasis(Fraction[,] curArray, int[] SelectedBasisRowCol = null, int indexIdle = -1)
        {
            Fraction[,] newArray = new Fraction[curArray.GetLength(0), curArray.GetLength(1)];
            for (int i = 0; i < curArray.GetLength(1); ++i)
            {
                newArray[0, i] = curArray[0, i];
            }
            for (int i = 1; i < curArray.GetLength(0); ++i)
            {
                newArray[i, 0] = curArray[i, 0];
            }
            needValue = new Fraction(0, 0);//??убрать??
            numColumn = -1; numRow = -1;//номер столбца для иск. базиса; номер строки для иск. базиса.
                                        //есть возможность сделать симплекс метод
            banColumn = new int[curArray.GetLength(1)];
            banColumnFlag = 0;
            //для холостого симплекса
            int newIndex = indexIdle / 100;
            //basis flag num
            indexIdle -= 100*newIndex; 
            if (newIndex < 0 && (SelectedBasisRowCol != null) || searchBetterBasis(curArray))
            {
                if(SelectedBasisRowCol != null)
                {
                    numColumn = SelectedBasisRowCol[1];
                    numRow = SelectedBasisRowCol[0];
                    needValue = new Fraction(1) / curArray[numRow, numColumn];
                }
                else
                {
                    //уже выбранно для какого базиса строить
                    needValue = new Fraction(1) / needValue;
                }

                newArray[numRow, numColumn] = needValue;
                curArray[numRow, numColumn].Type = true;
                newArray[numRow, numColumn].Type = true;
                //поменять x Свободный <--> x Базис (1)
                newArray[numRow, 0] = curArray[0, numColumn];
                newArray[0, numColumn] = curArray[numRow, 0];
                //новая опорная строка(3)
                for (int i = 1; i < curArray.GetLongLength(1); ++i)
                {
                    if (i != numColumn)
                    {
                        newArray[numRow, i] = needValue * curArray[numRow, i];
                    }
                }
                //новый опорный столбец(4)
                Fraction minValueMinus = needValue * new Fraction(-1);
                for (int i = 1; i < curArray.GetLongLength(0); ++i)
                {
                    if (i != numRow)
                    {
                        newArray[i, numColumn] = minValueMinus * curArray[i, numColumn];
                    }
                }
                //досчитать оставшееся(5)
                for (int i = 1; i < curArray.GetLength(0); ++i)
                {
                    if (i != numRow)
                    {
                        for (int j = 1; j < curArray.GetLength(1); ++j)
                        {
                            if (newArray[i, j] == null)
                            {
                                newArray[i, j] = curArray[i, j] - (curArray[i, numColumn] * newArray[numRow, j]);
                            }

                        }
                    }
                }
            }
            else if (newIndex > 0)
            {
                for (int i = 1; i < curArray.GetLength(1)-1; ++i)
                {
                    if (curArray[newIndex,i].Decimal != 0)
                    {
                        numColumn = i;
                        break;
                    }
                }
                
                numRow = newIndex;
                needValue = new Fraction(1) / curArray[numRow, numColumn];

                newArray[numRow, numColumn] = needValue;
                curArray[numRow, numColumn].Type = true;
                newArray[numRow, numColumn].Type = true;
                //поменять x Свободный <--> x Базис (1)
                newArray[numRow, 0] = curArray[0, numColumn];
                newArray[0, numColumn] = curArray[numRow, 0];
                //новая опорная строка(3)
                for (int i = 1; i < curArray.GetLongLength(1); ++i)
                {
                    if (i != numColumn)
                    {
                        newArray[numRow, i] = needValue * curArray[numRow, i];
                    }
                }
                //новый опорный столбец(4)
                Fraction minValueMinus = needValue * new Fraction(-1);
                for (int i = 1; i < curArray.GetLongLength(0); ++i)
                {
                    if (i != numRow)
                    {
                        newArray[i, numColumn] = minValueMinus * curArray[i, numColumn];
                    }
                }
                //досчитать оставшееся(5)
                for (int i = 1; i < curArray.GetLength(0); ++i)
                {
                    if (i != numRow)
                    {
                        for (int j = 1; j < curArray.GetLength(1); ++j)
                        {
                            if (newArray[i, j] == null)
                            {
                                newArray[i, j] = curArray[i, j] - (curArray[i, numColumn] * newArray[numRow, j]);
                            }

                        }
                    }
                }
            }
            else//
            {
                MessageBox.Show("ERROR 377 SIMPLEX  SimplexStepForSelectBasis");
                return null;
            }
            return CopyArray(newArray);
        }
        private Fraction[,] CreateSimplexTable()
        {
            //const 
            int col = curSimplexArray.GetLength(1) - basicVars.GetLength(0), row = basicVars.GetLength(0), flag = 1;
            Fraction[,] simplexTable = new Fraction[row + 2, col + 1];
            //print x1,x2,x3,...
            for (int i = 0; i < simplexTable.GetLength(0); ++i)
            {
                if (i == 0)
                {
                    string text = "name col";
                    //for (int j = 0; j < simplexTable.GetLength(1); ++j)
                    for (int j = 0; j < curSimplexArray.GetLength(1); ++j)
                    {
                        if (!basicVars.Contains(j))
                        {
                            text = "x" + (j + 1);
                            simplexTable[i, flag] = new Fraction(text);
                            ++flag;
                        }

                    }
                    text = "F99";
                    simplexTable[i, (simplexTable.GetLength(1) - 1)] = new Fraction(text);
                }
            }
            flag = 1;
            for (int j = 0; j < basicVars.GetLength(0); ++j)
            {
                string text = "x" + (basicVars[j] + 1);
                simplexTable[flag, 0] = new Fraction(text);
                ++flag;
            }
            //print value for x1,...
            Fraction[,] valueTable = getValueForTable();
            for (int i = 0; i < valueTable.GetLength(0); ++i)
            {
                for (int j = 0; j < valueTable.GetLength(1); ++j)
                {
                    simplexTable[i + 1, j + 1] = valueTable[i, j];
                }
            }
            return simplexTable;
        }
        private Fraction[,] getValueForTable()
        {
            int flag = 0;
            Fraction[,] tmp = new Fraction[basicVars.GetLength(0) + 1, curSimplexArray.GetLength(1) - basicVars.GetLength(0)];
            //данные из гауса. уравнения alfa1,...,beta1
            for (int i = 0; i < basicVars.GetLength(0); ++i)
            {
                for (int j = 0; j < curSimplexArray.GetLength(1); ++j)
                {
                    if (!basicVars.Contains(j))
                    {
                        tmp[i, flag] = curSimplexArray[i + 1, j];
                        ++flag;
                    }
                }
                flag = 0;
            }
            //последня строчка P1,...
            flag = 0;
            Fraction[] tmpArray = new Fraction[curSimplexArray.GetLength(1)];
            //выраженные переменные для базис
            Fraction[,] basicExpress = new Fraction[basicVars.GetLength(0), curSimplexArray.GetLength(1)];
            for (int i = 0; i < basicVars.GetLength(0); ++i)
            {
                //брать строку уравнения., чтобы выразить переменную базиса
                for (int colArray = 0; colArray < curSimplexArray.GetLength(1); ++colArray)
                {
                    tmpArray[colArray] = curSimplexArray[i + 1, colArray];
                }
                //получить выражение базисной переменной из уравнения
                Fraction[] tmpExpress = ExpressVariable(tmpArray, basicVars[i]);
                for (int j = 0; j < curSimplexArray.GetLength(1); ++j)
                {
                    basicExpress[i, j] = tmpExpress[j];
                }
            }

            Fraction[] tmpSubstitute = getIndexLine(0, curSimplexArray);
            //подставить в исходное уравнение выраженные базиси
            for (int i = 0; i < basicVars.GetLength(0); ++i)
            {
                tmpSubstitute = SubstituteVariable(tmpSubstitute, getIndexLine(i, basicExpress), basicVars[i]);
            }
            //перенос коэф. за знак "="
            tmpSubstitute[tmpSubstitute.GetLength(0) - 1] *= new Fraction(-1, 1);

            //записать строчку в таблицу
            for (int j = 0; j < curSimplexArray.GetLength(1); ++j)
            {
                if (!basicVars.Contains(j))
                {
                    tmp[basicVars.GetLength(0), flag] = tmpSubstitute[j];
                    flag++;
                }
            }

            return tmp;
        }
        //заменить 1 переменную из другого уравнения
        public Fraction[] SubstituteVariable(Fraction[] equation1, Fraction[] equation2, int variableToSubstitute)
        {
            //домножаем массив выраженного элемента на то число которое стоит в заменяемом уравнении
            for (int i = 0; i < equation2.GetLength(0); ++i)
            {
                equation2[i] *= equation1[variableToSubstitute];
            }
            //убираем заменяемую переменную в осн. уравнении
            equation1[variableToSubstitute] = new Fraction(0, 0);

            for (int i = 0; i < equation1.GetLength(0); ++i)
            {
                if (i != variableToSubstitute)
                {
                    equation1[i] += equation2[i];
                }
            }
            return equation1;
        }
        //выразить выбранную переменную из выбранного уравнения.
        public Fraction[] ExpressVariable(Fraction[] line, int xNumber)
        {
            if (line[xNumber].Numerator != 1)
            {
                for (int i = 0; i < line.Length; ++i)
                {
                    line[i] /= line[xNumber];
                }
            }

            for (int i = 0; i < line.Length - 1; ++i)
            {
                if (i != xNumber)
                {
                    line[i] *= new Fraction(-1, 1);
                }
            }
            return line;
        }
        public Fraction[,] getBasisArray()
        {
            return this.curSimplexArray;
        }
        public Fraction[,] getArray()
        {
            Fraction[,] tmpGaus = getArrayGaus();
            historySimplexArray = CreateSimplexTable();
            Fraction[,] tmpSimplex = calculationSimplex(historySimplexArray);

            int col = Math.Max(tmpGaus.GetLength(1), tmpSimplex.GetLength(1));
            int row = tmpGaus.GetLength(0) + tmpSimplex.GetLength(0) + 1;

            Fraction[,] newtmp = new Fraction[row, col];
            //add gaus table
            int curI = 0;
            for (int i = 0; i < tmpGaus.GetLength(0); ++i)
            {
                for (int j = 0; j < tmpGaus.GetLength(1); ++j)
                {
                    newtmp[i, j] = tmpGaus[i, j];
                }
            }
            //add simplex table
            curI = tmpGaus.GetLength(0) + 1;
            for (int i = 0; i < tmpSimplex.GetLength(0); ++i)
            {
                for (int j = 0; j < tmpSimplex.GetLength(1); ++j)
                {
                    newtmp[curI, j] = tmpSimplex[i, j];
                }
                curI++;
            }
            return newtmp;
        }
        public Fraction[,] getArrayGaus()
        {
            int zeroSTR = curSimplexArray.GetLength(0), step = 0;
            Fraction[,] newtmp = new Fraction[curSimplexArray.GetLength(0), curSimplexArray.GetLength(1)];
            for (int i = 0; i < newtmp.GetLength(0); ++i)
            {
                if (step == zeroSTR)
                {
                    for (int j = 0; j < newtmp.GetLength(1); ++j)
                    {
                        newtmp[i, j] = new Fraction("");
                    }
                    step = 0;
                }
                else
                {
                    for (int j = 0; j < newtmp.GetLength(1); ++j)
                    {
                        newtmp[i, j] = curSimplexArray[i, j];
                    }
                    ++step;
                }
            }
            return newtmp;
        }
        //copy mas[0,...] -> mas[]
        private Fraction[] getIndexLine(int Index, Fraction[,] curArray)
        {
            Fraction[] tmpArray = new Fraction[curArray.GetLength(1)];
            for (int colArray = 0; colArray < curArray.GetLength(1); ++colArray)
            {
                tmpArray[colArray] = curArray[Index, colArray];
            }
            return tmpArray;
        }
        //mega copy
        private void createCopyHistory(Fraction[,] addArrayInHistory)
        {
            int flag = 0;
            int row = historySimplexArray.GetLength(0) + addArrayInHistory.GetLength(0) + 1;
            int col = Math.Max(addArrayInHistory.GetLength(1), historySimplexArray.GetLength(1));
            if (answerFlag)
            {
                flag = historySimplexArray.GetLength(0) + 1;
                if (row < 3)
                {
                    row = 2;
                    col = addArrayInHistory.GetLength(1);
                    flag = 0;
                }
                
            }
            else if(historySimplexArray.GetLength(0) == 1 && historySimplexArray.GetLength(1)== 1)
            {
                row -= historySimplexArray.GetLength(0); 
                flag =  0;
            }
            else
            {
                flag = historySimplexArray.GetLength(0) + 1;
            }
            Fraction[,] newArrayHistory = new Fraction[row, col];
            //сохраняем тек. историю
            for (int i = 0; i < historySimplexArray.GetLength(0); ++i)
            {
                for (int j = 0; j < historySimplexArray.GetLength(1); ++j)
                {
                    newArrayHistory[i, j] = historySimplexArray[i, j];
                }
            }
            //добавляем новое решение
           
            for (int i = 0; i < addArrayInHistory.GetLength(0); ++i)
            {
                for (int j = 0; j < addArrayInHistory.GetLength(1); ++j)
                {
                    newArrayHistory[flag, j] = addArrayInHistory[i, j];
                }
                ++flag;
            }
            historySimplexArray = newArrayHistory;
        }
        //create answer x*(...)
        private void createAnswer()
        {
            Fraction[,] answer = new Fraction[2, curSimplexArray.GetLength(1)];
            int step = historySimplexArray.GetLength(0)-2, flag = 0;
            int tmp = historySimplexArray.GetLength(0) - 1;
            while (historySimplexArray[step,0] != null)
            {//x, value
                answer[0,flag] = historySimplexArray[step, 0];
                answer[1,flag] = historySimplexArray[step, historySimplexArray.GetLength(1)-1];
                --step; flag++;
            }
            for (int j = 1; j < historySimplexArray.GetLength(1)-1; ++j)
            {
                answer[0, flag] = historySimplexArray[step, j];
                answer[1, flag] = new Fraction(0,0);
                flag++;
            }
            answer[0, flag] = new Fraction("F18");
            answer[1, flag] = historySimplexArray[historySimplexArray.GetLength(0) - 1, historySimplexArray.GetLength(1) - 1];

            // Создание массива кортежей для сортировки
            (Fraction, Fraction)[] tempArray = new (Fraction, Fraction)[answer.GetLength(1)];
            for (int j = 0; j < answer.GetLength(1); j++)
            {
                tempArray[j] = (answer[0, j], answer[1, j]);
            }

            // Сортировка массива кортежей
            Array.Sort(tempArray, (a, b) =>
            {
                int numA = int.Parse(a.Item1.CharValue.Substring(1));
                int numB = int.Parse(b.Item1.CharValue.Substring(1));
                return numA.CompareTo(numB);
            });

            // Применение сортировки к массиву mas
            for (int j = 0; j < answer.GetLength(1); j++)
            {
                answer[0, j] = tempArray[j].Item1;
                answer[1, j] = tempArray[j].Item2;
            }

            //chek F(x)
            Fraction res = curSimplexArray[0, 0] * answer[1,0];
            for (int i= 1; i< answer.GetLength(1)-1;++i)
            {
                res += curSimplexArray[0, i] * answer[1, i];
            }
            answer[0, flag] = new Fraction("F");
            answer[1, flag] = res;
            createCopyHistory(answer);
        }
        private void createAnswer(Fraction[,] TableSimplex)
        {
            int col = (TableSimplex.GetLength(0) - 1) + TableSimplex.GetLength(1)-2;
            Fraction[,] answer = new Fraction[2, col];
            int step = TableSimplex.GetLength(0) - 2, flag = 0;
            int tmp = TableSimplex.GetLength(0) - 1;
            while (TableSimplex[step, 0] != null)
            {//x, value
                answer[0, flag] = TableSimplex[step, 0];
                answer[1, flag] = TableSimplex[step, TableSimplex.GetLength(1) - 1];
                --step; flag++;
            }
            for (int j = 1; j < TableSimplex.GetLength(1) - 1; ++j)
            {
                answer[0, flag] = TableSimplex[step, j];
                answer[1, flag] = new Fraction(0, 0);
                flag++;
            }
            answer[0, flag] = new Fraction("F18");
            answer[1, flag] = TableSimplex[TableSimplex.GetLength(0) - 1, TableSimplex.GetLength(1) - 1] * new Fraction(-1);

            // Создание массива кортежей для сортировки
            (Fraction, Fraction)[] tempArray = new (Fraction, Fraction)[answer.GetLength(1)];
            for (int j = 0; j < answer.GetLength(1); j++)
            {
                tempArray[j] = (answer[0, j], answer[1, j]);
            }

            // Сортировка массива кортежей
            Array.Sort(tempArray, (a, b) =>
            {
                int numA = int.Parse(a.Item1.CharValue.Substring(1));
                int numB = int.Parse(b.Item1.CharValue.Substring(1));
                return numA.CompareTo(numB);
            });

            // Применение сортировки к массиву mas
            for (int j = 0; j < answer.GetLength(1); j++)
            {
                answer[0, j] = tempArray[j].Item1;
                answer[1, j] = tempArray[j].Item2;
            }
            answerFlag = true;
            createCopyHistory(answer);
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
        public string[] getElementBasisStep()
        {
            if (tmpBasisElement is null)
                return null;
            int count = 0;
            string[] tmp = new string[tmpBasisElement.GetLength(0)];
            for(int i = 0; i < tmp.GetLength(0); ++i)
            {
                if (tmpBasisElement[i] != 0)
                {
                    ++count;
                    tmp[i] = "row:" + tmpBasisElement[i] + " col:" + i;
                }
            }

            string[] lastTmp = new string[count];
            count = 0;
            for (int i = 0; i < tmp.GetLength(0); ++i)
            {
                if (tmp[i] != null)
                {
                    lastTmp[count] = tmp[i];
                    ++count;
                }
            }
            return lastTmp;
        }
    }
    class SimplexTest
    {
        public int countOk = 0;
        public int countAll = 0;
        private Fraction[,] CurArray  = null;
        private Fraction[,] TestAnswer  = null;
        private int[] basicVars;
        public SimplexTest() {
            //x1 x2 x3 x4 x5 x6
            // 0  1  2  3  4  5

            if (false)
            {
                createTest();
            }
        }
        private void createTest()
        {
            //1 - primer para
            basicVars = new int[] { 2, 3 };
            CurArray = new Fraction[,]
            {
                { new Fraction(-2), new Fraction(-1), new Fraction(-3), new Fraction(-1), new Fraction("min") },
                { new Fraction(1), new Fraction(2), new Fraction(5), new Fraction(-1), new Fraction(4) },
                { new Fraction(1), new Fraction(-1), new Fraction(-1), new Fraction(2), new Fraction(1) }
            };
            TestAnswer = new Fraction[,]
            {
                { new Fraction("x1"), new Fraction("x2"), new Fraction("x3"), new Fraction("x4"), new Fraction("F") },
                { new Fraction(2), new Fraction(1), new Fraction(0), new Fraction(0), new Fraction(-5) }
            };
            createResult(CurArray, basicVars, TestAnswer);

            //2 - 3.1
            basicVars = new int[] { 0, 1 };
            CurArray = new Fraction[,]
            {
                { new Fraction(-1), new Fraction(2), new Fraction(-1), new Fraction("min") },
                { new Fraction(1), new Fraction(4), new Fraction(1), new Fraction(5) },
                { new Fraction(1), new Fraction(-2), new Fraction(-1), new Fraction(-1) }
            };
            TestAnswer = new Fraction[,]
            {
                { new Fraction("x1"), new Fraction("x2"), new Fraction("x3"), new Fraction("F") },
                { new Fraction(2), new Fraction(0), new Fraction(3), new Fraction(-5) }
            };
            createResult(CurArray, basicVars, TestAnswer);
            //3 - 3.2 
            basicVars = new int[] { 1, 2 };
            CurArray = new Fraction[,]
            {
                { new Fraction(-1), new Fraction(-1), new Fraction(-1), new Fraction("min") },
                { new Fraction(-1), new Fraction(1), new Fraction(1), new Fraction(2) },
                { new Fraction(3), new Fraction(-1), new Fraction(1), new Fraction(0) }
            };
            TestAnswer = new Fraction[,]
            {
                { new Fraction("x1"), new Fraction("x2"), new Fraction("x3"), new Fraction("F") },
                { new Fraction(1), new Fraction(3), new Fraction(0), new Fraction(-4) }
            };
            createResult(CurArray, basicVars, TestAnswer);

            //4 - 3.3   ???
            basicVars = new int[] { 2, 3 };
            CurArray = new Fraction[,]
            {
                { new Fraction(-2), new Fraction(-1), new Fraction(3), new Fraction(1),new Fraction("min") },
                { new Fraction(1), new Fraction(2), new Fraction(5),new Fraction(-1), new Fraction(4) },
                { new Fraction(1), new Fraction(-2), new Fraction(-3),new Fraction(2), new Fraction(1) }
            };
            TestAnswer = new Fraction[,]
            {
                { new Fraction("x1"), new Fraction("x2"), new Fraction("x3"), new Fraction("x4"), new Fraction("F") },
                { new Fraction(5,2), new Fraction(3,4), new Fraction(0), new Fraction(0), new Fraction(-23,4) }
            };
            createResult(CurArray, basicVars, TestAnswer);

            //0 - 3.4 //throw Exec
            //MessageBox.Show("This Test need ERROR");
            //basicVars = new int[] { 0, 3 };
            //CurArray = new Fraction[,]
            //{
            //    { new Fraction(-6), new Fraction(-1), new Fraction(-4), new Fraction(-5),new Fraction("min") },
            //    { new Fraction(3), new Fraction(1), new Fraction(-1),new Fraction(1), new Fraction(4) },
            //    { new Fraction(5), new Fraction(1), new Fraction(1),new Fraction(-1), new Fraction(4) }
            //};
            //TestAnswer = new Fraction[,]
            //{
            //    { new Fraction("x1"), new Fraction("x2"), new Fraction("x3"), new Fraction("x4"), new Fraction("F") },
            //    { new Fraction(0), new Fraction(0), new Fraction(0), new Fraction(0), new Fraction(0) }
            //};
            //createResult(CurArray, basicVars, TestAnswer);

            //5 - 3.5
            basicVars = new int[] { 1, 2 };
            CurArray = new Fraction[,]
            {
                { new Fraction(-1), new Fraction(-2), new Fraction(-3), new Fraction(1),new Fraction("min") },
                { new Fraction(1), new Fraction(-3), new Fraction(-1),new Fraction(-2), new Fraction(-4) },
                { new Fraction(1), new Fraction(-1), new Fraction(1),new Fraction(0), new Fraction(0) }
            };
            TestAnswer = new Fraction[,]
            {
                { new Fraction("x1"), new Fraction("x2"), new Fraction("x3"), new Fraction("x4"), new Fraction("F") },
                { new Fraction(2), new Fraction(2), new Fraction(0), new Fraction(0), new Fraction(-6) }
            };
            createResult(CurArray, basicVars, TestAnswer);

            //6 - 3.6
            basicVars = new int[] { 0, 2 };
            CurArray = new Fraction[,]
            {
                { new Fraction(-1), new Fraction(3), new Fraction(5), new Fraction(1),new Fraction("min") },
                { new Fraction(1), new Fraction(4), new Fraction(4),new Fraction(1), new Fraction(5) },
                { new Fraction(1), new Fraction(7), new Fraction(8),new Fraction(2), new Fraction(9) }
            };
            TestAnswer = new Fraction[,]
            {
                { new Fraction("x1"), new Fraction("x2"), new Fraction("x3"), new Fraction("x4"), new Fraction("F") },
                { new Fraction(1), new Fraction(0), new Fraction(0), new Fraction(4), new Fraction(3) }
            };
            createResult(CurArray, basicVars, TestAnswer);

            //7 - 3.7
            basicVars = new int[] { 1, 3 };
            CurArray = new Fraction[,]
            {
                { new Fraction(-1), new Fraction(-1), new Fraction(-1), new Fraction(-1),new Fraction("min") },
                { new Fraction(1), new Fraction(3), new Fraction(1),new Fraction(2), new Fraction(5) },
                { new Fraction(2), new Fraction(0), new Fraction(-1),new Fraction(1), new Fraction(1) }
            };
            TestAnswer = new Fraction[,]
            {
                { new Fraction("x1"), new Fraction("x2"), new Fraction("x3"), new Fraction("x4"), new Fraction("F") },
                { new Fraction(2), new Fraction(0), new Fraction(3), new Fraction(0), new Fraction(-5) }
            };
            createResult(CurArray, basicVars, TestAnswer);

            //8 - 3.8//throw Exec
            //MessageBox.Show("This Test need ERROR");
            //basicVars = new int[] { 2, 3 };
            //CurArray = new Fraction[,]
            //{
            //    { new Fraction(-1), new Fraction(-2), new Fraction(1), new Fraction(-1),new Fraction("min") },
            //    { new Fraction(1), new Fraction(1), new Fraction(2),new Fraction(3), new Fraction(1) },
            //    { new Fraction(2), new Fraction(-1), new Fraction(-1),new Fraction(3), new Fraction(2) }
            //};
            //TestAnswer = new Fraction[,]
            //{
            //    { new Fraction("x1"), new Fraction("x2"), new Fraction("x3"), new Fraction("x4"), new Fraction("F") },
            //    { new Fraction(2), new Fraction(0), new Fraction(3), new Fraction(0), new Fraction(-5) }
            //};
            //createResult(CurArray, basicVars, TestAnswer);

            //9 - 3.9//throw Exec
            //basicVars = new int[] { 0,2,3 };
            //CurArray = new Fraction[,]
            //{
            //    { new Fraction(-1), new Fraction(-1), new Fraction(-1), new Fraction(-1), new Fraction(-1), new Fraction("min") },
            //    { new Fraction(2), new Fraction(3), new Fraction(5), new Fraction(7), new Fraction(9), new Fraction(19)},
            //    { new Fraction(1), new Fraction(-1), new Fraction(0), new Fraction(1), new Fraction(2), new Fraction(2)}
            //};
            //TestAnswer = new Fraction[,]
            //{
            //    { new Fraction("x1"), new Fraction("x2"), new Fraction("x3"), new Fraction("x4"), new Fraction("F") },
            //    { new Fraction(2), new Fraction(0), new Fraction(3), new Fraction(0), new Fraction(-5) }
            //};
            //createResult(CurArray, basicVars, TestAnswer);
            //10 - 3.10
            basicVars = new int[] { 0, 1, 2 };
            CurArray = new Fraction[,]
            {
                { new Fraction(1), new Fraction(3), new Fraction(2), new Fraction(4), new Fraction(-2), new Fraction("min") },
                { new Fraction(-1), new Fraction(0), new Fraction(1), new Fraction(-2), new Fraction(-2), new Fraction(-2)},
                { new Fraction(0), new Fraction(1), new Fraction(-1), new Fraction(1), new Fraction(-2), new Fraction(0)},
                { new Fraction(2), new Fraction(1), new Fraction(0), new Fraction(5), new Fraction(1), new Fraction(7)}
            };
            TestAnswer = new Fraction[,]
            {
                { new Fraction("x1"), new Fraction("x2"), new Fraction("x3"), new Fraction("x4"),new Fraction("x5"), new Fraction("F") },
                { new Fraction(1), new Fraction(0), new Fraction(1), new Fraction(1), new Fraction(0),new Fraction(7) }
            };
            createResult(CurArray, basicVars, TestAnswer);
            //11 - 3.11 ///gaus error result
            //basicVars = new int[] { 0, 5 };
            //CurArray = new Fraction[,]
            //{
            //    { new Fraction(2), new Fraction(-1), new Fraction(-1), new Fraction(1), new Fraction(-4),new Fraction(-1), new Fraction("min") },
            //    { new Fraction(3), new Fraction(1), new Fraction(2), new Fraction(6), new Fraction(9),new Fraction(3), new Fraction(15) },
            //    { new Fraction(1), new Fraction(2), new Fraction(-1), new Fraction(2), new Fraction(3),new Fraction(1), new Fraction(5) },
            //};
            //TestAnswer = new Fraction[,]
            //{
            //    { new Fraction("x1"), new Fraction("x2"), new Fraction("x3"), new Fraction("x4"),new Fraction("x5"), new Fraction("F") },
            //    { new Fraction(1), new Fraction(0), new Fraction(1), new Fraction(1), new Fraction(0),new Fraction(7) }
            //};
            //createResult(CurArray, basicVars, TestAnswer);
            //12 - 3.12 // 1 bazis
            //basicVars = new int[] { 0 };
            //CurArray = new Fraction[,]
            //{
            //    { new Fraction(3), new Fraction(-2), new Fraction(1), new Fraction(3), new Fraction(3), new Fraction("max") },
            //    { new Fraction(2), new Fraction(-1), new Fraction(1), new Fraction(1), new Fraction(1), new Fraction(2) },
            //    { new Fraction(-4), new Fraction(3), new Fraction(-1), new Fraction(-1), new Fraction(-3), new Fraction(-4) },
            //    { new Fraction(3), new Fraction(2), new Fraction(3), new Fraction(5), new Fraction(0), new Fraction(3) },
            //};
            //TestAnswer = new Fraction[,]
            //{
            //    { new Fraction("x1"), new Fraction("x2"), new Fraction("x3"), new Fraction("x4"),new Fraction("x5"), new Fraction("F") },
            //    { new Fraction(1), new Fraction(0), new Fraction(1), new Fraction(1), new Fraction(0),new Fraction(7) }
            //};
            createResult(CurArray, basicVars, TestAnswer);

            messageTest();
        }

        public void createResult(Fraction[,] tmp, int[] basicVars, Fraction[,] testAnswer)
        {
            Simplex res = new Simplex(tmp, basicVars);
            tmp = res.getArray();
            Fraction[,] curAnswer = new Fraction[2, tmp.GetLength(1)];
            int flag = 0;
            for(int i=tmp.GetLength(0)-2; i< tmp.GetLength(0); i++)
            {
                for(int j=0; j< tmp.GetLength(1); ++j)
                {
                    curAnswer[flag, j] = tmp[i, j];
                }
                ++flag;
            }

            for(int i=0; i< testAnswer.GetLength(0);++i)
            {
                for(int j=0; j< testAnswer.GetLength(1); j++)
                {
                    if ( (testAnswer[i,j].CharValue != curAnswer[i,j].CharValue)||(testAnswer[i, j].Numerator != curAnswer[i, j].Numerator) ||(testAnswer[i, j].Denominator != curAnswer[i, j].Denominator) )
                    {
                        ++countAll;
                        return;
                    }
                }
            }

            countAll++;
            countOk++;
            return;
        }

        private void messageTest()
        {
            MessageBox.Show("All Test result:\n" + "countAll:" + countAll + "\n" + "countOk:" + countOk + "\n");
        }
    }
}
