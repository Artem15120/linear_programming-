using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace WpfApp1
{
    public class ArtificialBasis
    {
        private int cStepBack = 0;
        private int cStepBackFlagOn = 0;
        private int cStepForward = 99;
        private int rowDeleteSave = 0;
        private int blockBack = 0;
        private Fraction[,] ArrayGlobal;//сохранить исходное уравнение
        private Fraction[,] historyBasisArray = new Fraction[0, 0];//массив изменения
        private Fraction[,] toSimplex;//tmp массив
        private int countBasis = 0;
        private Fraction[] addedBasis = new Fraction[0];
        private bool flag = false;//переключение (true/false)T->решение осн. симплекс задачи. F->решение доп. задачи с иск. базисами
        public int countstep = 1;
        public ArtificialBasis(Fraction[,] tmp)
        {
            this.ArrayGlobal = tmp;
        }
        public ArtificialBasis(){}
        public void setArray(Fraction[,] tmp)
        {
            this.ArrayGlobal = tmp;
            historyBasisArray = new Fraction[0, 0];
            flag = false;
            cStepBack = 0;
            cStepForward = 99;
            createCopyHistory(tmp);
        }
        private Fraction[,] replaceStartCondition(Fraction[,] chekArray)
        {
            //первичная проверка входных данных для задачи
            //если функция max нужно ее -> min
            if (chekArray[0, chekArray.GetLength(1)-1].CharValue.ToLower() == "max")
            {
                chekArray[0, chekArray.GetLength(1) - 1].CharValue = "min";
                for (int i = 0; i < chekArray.GetLength(1)-1; ++i)
                {
                    chekArray[0, i] *= new Fraction(-1);
                }

            }
            //если значение f ограничений < 0
            for (int i = 1; i < chekArray.GetLength(0); ++i)
            {
                if (chekArray[i, chekArray.GetLength(1)-1] < new Fraction(0))
                {
                    for (int j = 0; j < chekArray.GetLength(1); ++j)
                    {
                        chekArray[i,j] *= new Fraction(-1);
                    }
                }
            }
            return chekArray;
        }
        public void addBasis()
        {
            //проверить начальные условия задачи. и изменить при надобности
            //первый вывод в таблице
            Fraction[,] newArrayGlobal = CopyArray(this.ArrayGlobal);
            newArrayGlobal = replaceStartCondition(newArrayGlobal);

            int BasisCount = newArrayGlobal.GetLength(0)-1;
            Fraction[,] newArray = new Fraction[newArrayGlobal.GetLength(0), newArrayGlobal.GetLength(1)+BasisCount];
                //нужно добавить иск. базисы для решения задачи
            //add old value
            for(int i = 1;  i < newArray.GetLength(0); i++)
            {
                for(int j=0; j < newArrayGlobal.GetLength(1); j++)
                {
                    if (j == newArrayGlobal.GetLength(1)-1)
                    {
                        newArray[i,j+BasisCount] = newArrayGlobal[i,j];
                    }
                    else
                    {
                        newArray[i,j] = newArrayGlobal[i,j];
                    }
                }
            }
                //изменение исходной функции на функцию с иск. базисами (зануление всех осн. базисов)
            //новая функция (...-> min | строка)
            for (int i = 0; i < newArray.GetLength(1); i++)
            {
                if (i < newArrayGlobal.GetLength(1)-1)
                {
                    newArray[0, i] = new Fraction(0);
                }
                else
                {
                    newArray[0, i] = new Fraction(1);
                }
            }
            newArray[0, newArray.GetLength(1)-1] = newArrayGlobal[0,newArrayGlobal.GetLength(1)-1];
            //add new basis in col
            for (int i = 1; i < newArray.GetLength(0); i++)
            {
                for (int j = 0; j < BasisCount; j++)
                {
                    if( (i-1) == j)
                    {
                        newArray[i, j + newArrayGlobal.GetLength(1)-1] = new Fraction(1);
                    }
                    else
                    {
                        newArray[i, j + newArrayGlobal.GetLength(1)-1] = new Fraction(0);
                    }
                }
            }
            //дабавленных базисов
            this.countBasis = BasisCount;
            //this.ArrayGlobal = newArray;
            //сохранить изменения
            createCopyHistory(newArray);
            //создать таблицу симплекс
            createBasisTable(newArray);
            blockBack = historyBasisArray.GetLength(0);
        }
        private void createBasisTable(Fraction[,] curArray)
        {
            Fraction[,] tmpTable = new Fraction[curArray.GetLength(0)+1, curArray.GetLength(1)- this.countBasis+1];
            //str name column//изначальные
            for (int i = 1; i < curArray.GetLength(1) - this.countBasis; ++i)
            {
                tmpTable[0, i] = new Fraction("X"+(i));
            }
            tmpTable[0, curArray.GetLength(1) - this.countBasis] = new Fraction("F99");
            //str name row//добавленные базисы
            addedBasis = new Fraction[countBasis];
            for (int i = 1; i < curArray.GetLength(0); ++i) 
            {
                tmpTable[i, 0] = new Fraction("X" + (i+(curArray.GetLength(1) - this.countBasis-1)));
                addedBasis[i-1] = new Fraction(tmpTable[i,0].CharValue);
            }
            //table value
            for (int i = 1; i < curArray.GetLength(0); ++i)
            {
                for (int j = 1; j < curArray.GetLength(1) - this.countBasis; ++j) {
                    tmpTable[i,j] = curArray[i,j-1];
                }
            }
            //table column F
            for (int i = 1; i < curArray.GetLength(0); ++i)
            {
                tmpTable[i, tmpTable.GetLength(1)-1] = curArray[i, curArray.GetLength(1)-1];
            }
            //last str
            for (int i = 1; i < tmpTable.GetLength(1); ++i)
            {
                tmpTable[tmpTable.GetLength(0) - 1, i] = new Fraction(0);
                for (int j = 1; j < tmpTable.GetLength(0)-1; ++j)
                {
                    tmpTable[tmpTable.GetLength(0)-1,i] += tmpTable[j,i];
                }
                tmpTable[tmpTable.GetLength(0) - 1, i] *= new Fraction(-1);
            }

            toSimplex = CopyArray(tmpTable);
            createCopyHistory(tmpTable);
        }
        /////////////////////////////////////////////////////////////////////////////////// сверху первый шаг/
        private bool ChekStabilitySimplex(Fraction[,] curArray)
        {//если ответ есть, то последня строка целиком не пуста
            if (curArray[curArray.GetLength(0) - 1, 0] != null) return true;//answer create
            //если можно решать дальше, то в последней строке должны быть эл. < 0
            Fraction zeroPoint = new Fraction(0, 0);
            for (int i = 1; i < curArray.GetLength(1) - 1; ++i)
            {
                if (curArray[curArray.GetLength(0) - 1, i] < zeroPoint)
                {
                    return false;
                }
            }
            //если вся строка 0 верни false
            if (!flag)
            {
                for (int i = 1; i < curArray.GetLength(1) - 1; ++i)
                {
                    if (curArray[curArray.GetLength(0) - 1, i].Decimal != 0)
                    {//есть не ноль.  решений нет.
                        return true;
                    }
                }
            }//вся строка 0 верни false
            return false;
        }
        private int ChekIdleSimplexStep(Fraction[,] curArray)
        {
            //-1-> холостой симплекс шаг сделать нельзя;index != -1 можно делать холостой шаг;
            int countBasis = 0, indexABassis = -1;
            for (int i = 1; i < curArray.GetLength(0)-1; ++i)
            {
                for (int j = 0; j < addedBasis.GetLength(0); ++j)
                {
                    if (curArray[i, 0].CharValue == addedBasis[j].CharValue)
                    {
                        if (curArray[i,curArray.GetLength(1)-1].Decimal == 0)
                        {
                            //countBasis++;
                            //indexABassis = i;
                            return i;
                        }
                    }
                }
            }
            return indexABassis;
        }
        private bool ChekAnswer(Fraction[,] curArray)
        {
            for (int i = 0; i < curArray.GetLength(1) - 1; ++i)
            {
                if (curArray[curArray.GetLength(0) - 1, i] == null)
                {
                    return false;
                }
            }
            return true;
        }
        //кнопка: след. шаг решения
        public void stepForward(string SelectBasis=null)
        {
            if (cStepForward == 0)
                return;

            if (toSimplex != null)
            {
                rowDeleteSave = toSimplex.GetLength(0);
            }
            if (flag)
            {//решение основной задачи

                //после создания новой симплекс таблицы . 
                if (ChekStabilitySimplex(toSimplex))
                {//решений не оказалось(
                    //MessageBox.Show("Дальше нет решений");
                    setBack(1);
                    cStepForward = 0;
                    return;
                }

                //решать можно
                Simplex res = new Simplex(toSimplex, addedBasis, 2, getIndexString(SelectBasis));
                toSimplex = res.getBasisArray();
                countstep += res.CountStep;
                if(toSimplex is null)
                {//решений нет
                    //MessageBox.Show("Дальше нет решений");
                    setBack(1);
                    cStepForward = 0;
                    return;
                }
                
                updateHistoryColor(toSimplex);
                createCopyHistory(toSimplex); 
                
                if (ChekStabilitySimplex(toSimplex))
                {//решение найдено или нет
                    //MessageBox.Show("Дальше нет решений");
                    if(ChekAnswer(toSimplex)) 
                        updatePreHistoryColor(toSimplex);
                    setBack(1);
                    cStepForward = 0;
                    return;
                }
                countstep++;
                cStepBackFlagOn++;
                setBack(1);
            }
            else
            {
                //блочить решения .
                if (toSimplex is null)
                    return;

                Fraction[,] tmp = removeRow(CopyArray(toSimplex));
                if(tmp.GetLength(0) < toSimplex.GetLength(0))
                {//была удалено zeroSTR
                    toSimplex = CopyArray(tmp);
                    createCopyHistory(toSimplex);
                    return;
                }
                int indexIdleRow = ChekIdleSimplexStep(toSimplex);
                bool flagIdle = false;
                if (ChekStabilitySimplex(toSimplex))
                {//решений не оказалось(
                    if(indexIdleRow == -1)
                    {//возможностей решения нет
                        MessageBox.Show("Дальше нет решений");
                        setBack(1);
                        cStepForward = 0;
                        return;
                    }
                    else
                    {//холостой шаг сделать возможно
                        flagIdle = true;
                    }
                }
                Simplex res = new Simplex();
                if(flagIdle)
                {//холостой шаг симплекса
                    res = new Simplex(toSimplex, addedBasis, ((indexIdleRow*100)+1), getIndexString(SelectBasis));
                }
                else
                {//решить симплекс методом созданую таблицу с базисом
                    res = new Simplex(toSimplex, addedBasis, 1, getIndexString(SelectBasis));
                }
                
                //если res is null значит решение найдено
                //вся ниж. строка = 0.
                if(!(res.getBasisArray() is null))
                {//иск. базис задача
                    updateHistoryColor(toSimplex);
                    toSimplex = res.getBasisArray();
                    if (toSimplex is null)
                        return;
                    //убрать строку из решения
                    toSimplex = removeColumn(toSimplex);
                    //toSimplex = removeRow(toSimplex);
                    //сохранить результат решения симплекс метода в 1 шаг.
                    createCopyHistory(toSimplex);
                }
                else
                {//переход к решению основной задачи
                 //
                    saveBassis(toSimplex);
                    continueTheBasis(toSimplex);
                    flag = true;
                }
                setBack(1);
            }
        }
        public List<string> saveBassisList = new List<string>();
        public int colCount = 0;
        private void saveBassis(Fraction[,] simplex)
        {
            List<string> bassis = new List<string>();
            for (int i =0; i< simplex.GetLength(0); ++i)
            {
                if (simplex[i,0] != null)
                {
                    bassis.Add(simplex[i,0].CharValue);
                }
            }
            colCount = 0;
            for (int i = 1; i < simplex.GetLength(1)-1; ++i)
            {
                colCount++;
            }
            saveBassisList = bassis;
        }
        private void continueTheBasis(Fraction[,] lastArray)
        {
            //получить index базисов
            int[] basicVars = createBasisVars(lastArray);
            string lastChar; int number=0;
            //выраженые базисы 
            int col = lastArray.GetLength(1)-1+lastArray.GetLength(0)-2;
            Fraction[,] stringVar = new Fraction[basicVars.GetLength(0), col];
            
            for (int i = 0; i < basicVars.GetLength(0); ++i)
            {
                //добавить уравнение F.
                stringVar[i, stringVar.GetLength(1) - 1] = lastArray[i + 1, lastArray.GetLength(1) - 1];
                //stringVar[i, stringVar.GetLength(1) - 1] = lastArray[i + 1, lastArray.GetLength(0) - 1];

                //добавить уравнения свободных переменных
                for(int j = 1; j < lastArray.GetLength(1)-1; ++j)
                {
                    lastChar = lastArray[0, j].CharValue.Substring(1);
                    number = Int32.Parse(lastChar);
                    stringVar[i, number - 1] = lastArray[i+1,j];
                }
                //добавить уравнение базисного переменного
                lastChar = lastArray[i + 1, 0].CharValue.Substring(1);
                number = Int32.Parse(lastChar);
                stringVar[i, number - 1] = new Fraction(1);

                //занулить не найденые переменные
                for (int a = 0; a < stringVar.GetLength(1)-1; ++a)
                {
                    if (stringVar[i, a] is null)
                    {
                        stringVar[i, a] = new Fraction(0);
                    }
                }
                //сделать равенство
                for (int j = 0; j < stringVar.GetLength(1)-1; ++j)
                {
                    if ( (number-1) != j)
                    {
                        stringVar[i,j] *= new Fraction(-1);
                    }
                }
            }
            //куда подставлять переменный. исходное уравнение -> min
            Fraction[] tmpSubstitute = getIndexLine(1, this.historyBasisArray);
            tmpSubstitute =notNullArray(tmpSubstitute);
            //подставить в исходное уравнение выраженные базиси
            for (int i= 0; i < basicVars.GetLength(0); ++i)
            {
                tmpSubstitute = SubstituteVariable(tmpSubstitute, getIndexLine(i, stringVar), basicVars[i]);
            }
            //сделать min
            if (ArrayGlobal[0, ArrayGlobal.GetLength(1)-1].CharValue.ToLower() == "max")
            {
                for(int i = 0; i < tmpSubstitute.GetLength(0); ++i)
                {
                    tmpSubstitute[i] *= new Fraction(-1);
                }
            }
            //до множить на -1
            tmpSubstitute[tmpSubstitute.GetLength(0)-1] *= new Fraction(-1);

            createSimplexTable(tmpSubstitute, lastArray);
        }
        private void createSimplexTable(Fraction[] substitute, Fraction[,] curArray )
        {
            Fraction[,] tmpTable = new Fraction[curArray.GetLength(0), curArray.GetLength(1)];
            //str name column
            for (int i = 0; i < curArray.GetLength(0)-1; ++i)
            {
                for (int j = 0; j < curArray.GetLength(1); ++j)
                {
                    tmpTable[i, j] = curArray[i, j];
                }
            }
            for(int i=1; i< curArray.GetLength(1); ++i)
            {
                string lastChar = curArray[0, i].CharValue.Substring(1);
                int number = Int32.Parse(lastChar);
                if(number == 99)
                {
                    tmpTable[curArray.GetLength(0) - 1, i] = substitute[substitute.GetLength(0)-1];
                }
                else
                {
                    tmpTable[curArray.GetLength(0)-1,i] = substitute[number-1];
                }
            }

            toSimplex = CopyArray(tmpTable);
            createCopyHistory(tmpTable);
        }
        private void updateHistoryColor(Fraction[,] colorArray)
        {
            for (int i = 1; i < colorArray.GetLength(0)-1; ++i)
            {
                for (int j = 1; j < colorArray.GetLength(1)-1; ++j) {
                    if (colorArray[i, j] != null && historyBasisArray[historyBasisArray.GetLength(0) - colorArray.GetLength(0) + i, j]!= null)
                    {
                        int row = historyBasisArray.GetLength(0) - colorArray.GetLength(0) + i;
                        if (colorArray[i, j].Type == true)
                        {
                            historyBasisArray[historyBasisArray.GetLength(0) - colorArray.GetLength(0) + i, j].Type = true;
                        }
                    }
                }
            }
        }
        private void updatePreHistoryColor(Fraction[,] colorArray)
        {
            //поменять местами цвет у последней симплекс таблицы с пред последней
            int startRow = historyBasisArray.GetLength(0)-colorArray.GetLength(0);
            int maxRow = 1;
            if (startRow + maxRow >= historyBasisArray.GetLength(0))
            {
                MessageBox.Show("updatePreHistoryColor");
                return;
            }
            while (historyBasisArray[startRow+maxRow, 0] != null)
            {
                if (startRow + maxRow+1 >= historyBasisArray.GetLength(0))
                {
                    MessageBox.Show("updatePreHistoryColor");
                    return;
                }
                maxRow++;
            }
            if (startRow + maxRow + 2 >= historyBasisArray.GetLength(0))
            {
                MessageBox.Show("updatePreHistoryColor");
                return;
            }
            maxRow += 2;

            int newColorCol = 0, newColorRow = 0, startAndMaxRow = startRow + maxRow;
            if (historyBasisArray.GetLength(0) <= startAndMaxRow)
            {
                MessageBox.Show("Ошибка размеренности. uPHC");
                startAndMaxRow = historyBasisArray.GetLength(0) - 1;
            }
            for(int i = startRow+1; i < startAndMaxRow; ++i)
            {
                for(int j = 1; j < colorArray.GetLength(1)-1; j++)
                {
                    if (historyBasisArray[i,j] != null)
                    {
                        if (historyBasisArray[i,j].Type == true)
                        {
                            newColorCol = j;
                            newColorRow = i-maxRow;
                            historyBasisArray[newColorRow, newColorCol].Type = true;
                            historyBasisArray[i, j].Type = false;
                        }
                    }
                }
            }
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
        private Fraction[] notNullArray(Fraction[] newArray)
        {
            int count = 0;
            for(int i=0; i < newArray.GetLength(0); ++i)
            {
                if (newArray[i] is null)
                {
                    count = i; break;
                }
            }
            Fraction[] tmp = new Fraction[count];
            for(int i=0; i< tmp.GetLength(0); ++i)
            {
                tmp[i] = newArray[i];
            }

            return tmp;
        }
        //получить номера выраженных базисных переменных.
        private int[] createBasisVars(Fraction[,] curArray)
        {
            //int 
            int[] tmp = new int[curArray.GetLength(0)-2];
            for(int i=1; i < curArray.GetLength(0)-1; ++i)
            {
                string lastChar = curArray[i, 0].CharValue.Substring(curArray[i, 0].CharValue.Length - 1);
                int number = Int32.Parse(lastChar);
                tmp[i-1] = number-1;
            }
            return tmp;
        }
        private Fraction[,] removeColumn(Fraction[,] simplexArray)
        {
            int columnRemove = SearchColumnRemove(this.historyBasisArray, simplexArray), flagDropCol = 0;
            if (columnRemove == -1)
                return simplexArray;
            Fraction[,] returnArray = new Fraction[simplexArray.GetLength(0), simplexArray.GetLength(1)-1];
            for(int i= 0; i< returnArray.GetLength(0); ++i)
            {
                for(int j=0; j< returnArray.GetLength(1); ++j)
                {
                    if(j == columnRemove)
                    {
                        flagDropCol = 1;
                    }
                    returnArray[i, j] = simplexArray[i, j+flagDropCol];
                }
                flagDropCol = 0;
            }
            return returnArray;
        }
        private Fraction[,] removeRow(Fraction[,] simplexArray)
        {
            int[] chekRow = new int[simplexArray.GetLength(0)-2];
            int countZero = 0;
            for (int i = 0; i < chekRow.GetLength(0); ++i) {
                chekRow[i] = 0;//пустой массив
            }
            //находим нулевые строки (нужно ли делать различия между иск. базисами и не иск. базисами?)
            for (int i = 1; i < simplexArray.GetLength(0) - 1; ++i) {
                for (int j = 1; j < simplexArray.GetLength(1); ++j) {
                    if (simplexArray[i,j].Decimal != 0)
                    {
                        chekRow[i-1] = 99;
                    }
                }
            }
            for(int i=0; i< chekRow.GetLength(0); ++i)
            {
                if (chekRow[i] == 0)
                {
                    countZero++;
                }
            }
            //таких строк нет
            if (countZero == 0)
                return simplexArray;

            Fraction[,] returnArray = new Fraction[simplexArray.GetLength(0)-countZero, simplexArray.GetLength(1)];
            int flag = 1;
            //копировать все строки кроме первой и последней
            for (int i = 1; i < returnArray.GetLength(0)-1; ++i)
            {
                if (chekRow[i-1] == 0)
                {
                    ++flag;
                }
                for (int j = 0; j < returnArray.GetLength(1); ++j)
                {
                    returnArray[i,j] = simplexArray[flag,j];
                }
                ++flag;
            }
            //копировать первую и посл. строку
            for (int i = 0; i < returnArray.GetLength(1); ++i)
            {
                returnArray[0,i]  = simplexArray[0,i];
            }
            for (int i = 0; i < returnArray.GetLength(1); ++i)
            {
                returnArray[returnArray.GetLength(0)-1, i] = simplexArray[simplexArray.GetLength(0)-1, i];
            }
            return returnArray;
        }
        //удалить столбец после симплекс шага
        private int SearchColumnRemove(Fraction[,] historyArray, Fraction[,] newArray)
        {
            if(historyArray.GetLength(1) < newArray.GetLength(1))
            {
                throw new ArgumentException("Количество строк в матрицах не совпадает.");
            }

            int rowHistory = historyArray.GetLength(0) - newArray.GetLength(0);
            for(int i=1; i< newArray.GetLength(1); ++i)
            {
                //найти разные столбцы по названию
                if (historyArray[rowHistory, i].CharValue != newArray[0,i].CharValue)
                {//проверить. явл. ли этой иск. базисом
                    for(int j=0; j < addedBasis.GetLength(0); ++j)
                    {
                        if (newArray[0,i].CharValue == addedBasis[j].CharValue)
                        {//вернуть столбец для удаления
                            return i;
                        }
                    }
                    return -1;
                }
            }
            //вся строка одинакова
            return -1;
        } 
        public void stepBack()
        {
            if (cStepBack == 0)
                return;
            setBack(-1);
            if(cStepBackFlagOn > 0 || cStepBackFlagOn < 0)
            {
                cStepBackFlagOn--;
            }else if (cStepBackFlagOn == 0)
            {
                flag = false;
            }

            int rowDelete = 0;
            if (toSimplex != null)
            {
                rowDelete = toSimplex.GetLength(0)+1;
            }
            else
            {
                rowDelete = rowDeleteSave+1;
            }

            if (blockBack >= historyBasisArray.GetLength(0))
            {
                cStepBack = 0;
                return;
            }

            //NEW history mas
            Fraction[,] newHistory = new Fraction[historyBasisArray.GetLength(0)-rowDelete,historyBasisArray.GetLength(1)];
            for (int i = 0; i< newHistory.GetLength(0);++i)
            {
                for (int j = 0; j < newHistory.GetLength(1); ++j)
                {
                    newHistory[i,j] = historyBasisArray[i,j];
                }
            }
            //new toSimplex
            int startCol = 1;
            int newRow = 2, startRow = newHistory.GetLength(0) - 2;
            //узнать высоту строки
            while (newHistory[startRow, 0] != null)
            {
                --startRow;
                ++newRow;
            }
            //узнать длину строки
            while (newHistory[startRow,startCol] != null )
            {
                ++startCol;
                if (startCol == newHistory.GetLength(1))
                {
                    startCol = newHistory.GetLength(1) - 1;
                    break;
                }
            }
            Fraction[,] newToSimplex = new Fraction[newRow, startCol];
            for (int i = 0; i < newToSimplex.GetLength(0); ++i)
            {
                for (int j = 0; j < newToSimplex.GetLength(1); ++j) {
                    newToSimplex[i,j] = newHistory[i+startRow, j];
                }
            }

            historyBasisArray = newHistory;
            toSimplex = newToSimplex;
        }
        
        public bool getBack()
        {
            if (cStepBack == 0)
            {
                return false;
            }
            return true;
        }
        public bool getForward()
        {
            if (cStepForward == 0)
            {
                return false;
            }
            return true;
        }
        private void setBack(int i)
        {
            cStepBack += i;
            cStepForward = 1;
        }

        //выпрать опорный элемент
        public string[] takeElementsBasis()
        {
            if (toSimplex is null)
                return null;
            Simplex res = new Simplex(toSimplex, addedBasis, 3);
            if (toSimplex is null)
                return null;
            string[] tmp = res.getElementBasisStep();
            return tmp;
        }
        public void paintColorLastTable(string newColorIndex)
        {
            if (newColorIndex is null)
                return;
            int[] RowCol = getIndexString(newColorIndex);
            int row = RowCol[0];
            int col = RowCol[1];
            row = historyBasisArray.GetLength(0) - toSimplex.GetLength(0) + row;
            historyBasisArray[row, col].Type = true;
        }

        public void paintColorAllTable(string[] basisString)
        {//закрасить все опрные элементы и
            if (basisString is null || basisString.Length == 0)
                return;
            for (int i=0; i< basisString.GetLength(0); ++i)
            {
                paintColorLastTable(basisString[i]);
            }
        }
        private void clearColorLastTable(string newColorIndex)
        {
            if (newColorIndex is null)
                return;
            int[] RowCol = getIndexString(newColorIndex);
            int row = RowCol[0];
            int col = RowCol[1];
            row = historyBasisArray.GetLength(0) - toSimplex.GetLength(0) + row;
            historyBasisArray[row, col].Type = false;
        }
        public void clearColorAllTable(string[] newColorIndex)
        {
            if (newColorIndex is null || newColorIndex.Length == 0)
                return;
            for(int i=0; i< newColorIndex.GetLength(0);++i)
            {
                clearColorLastTable(newColorIndex[i]);
            }
        }
        private int[] getIndexString(string newColorIndex)
        {
            if (newColorIndex is null)
                return null;
            Match matchRow = Regex.Match(newColorIndex, @"row:(\d+)");
            int row = int.Parse(matchRow.Groups[1].Value);

            Match matchCol = Regex.Match(newColorIndex, @"col:(\d+)");
            int col = int.Parse(matchCol.Groups[1].Value);
            return new int[] { row, col };
        }
        public string getIndexStringFormat(int newColumn, string[] basisString)
        {
            if(basisString is null || basisString.Length == 0) return null;
            for (int i = 0; i < basisString.Length; i++)
            {
                int[] RowCol = getIndexString(basisString[i]);
                int col = RowCol[1];
                if (newColumn == col)
                {
                    return "row:" + RowCol[0] + " col:" + col; ;
                }
            }
            return null;
        }
        private void createCopyHistory(Fraction[,] addArrayInHistory)
        {
            int row = historyBasisArray.GetLength(0) + addArrayInHistory.GetLength(0) + 1;
            int col = Math.Max(addArrayInHistory.GetLength(1), historyBasisArray.GetLength(1));
            Fraction[,] newArrayHistory = new Fraction[row, col];
            //history add
            for (int i = 0; i < this.historyBasisArray.GetLength(0); ++i)
            {
                for (int j = 0; j < this.historyBasisArray.GetLength(1); ++j)
                {
                    newArrayHistory[i, j] = historyBasisArray[i, j];
                }
            }
            //new array add
            for (int i = 0; i < addArrayInHistory.GetLength(0); ++i)
            {
                for (int j = 0; j < addArrayInHistory.GetLength(1); ++j)
                {
                    newArrayHistory[i + this.historyBasisArray.GetLength(0)+1, j] = addArrayInHistory[i, j];
                }
            }
            historyBasisArray = newArrayHistory;
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
        public Fraction[,] getArray()
        {
            //return Array;
            return historyBasisArray;
        }

        private List<string> saveAnsver = new List<string>();
        public string getAnswer()
        {
            if (toSimplex is null)
                return null;
            List<string> newLine = new List<string>();
            int flag = 0;
            for(int i  = historyBasisArray.GetLength(0)-1; i < historyBasisArray.GetLength(0); ++i)
            {
                if (historyBasisArray[i, 0] == null)
                {
                    return null;
                }
                for (int j = 0; j < historyBasisArray.GetLength(1); ++j)
                {
                    if (historyBasisArray[i, j] != null)
                    {
                        ++flag;
                        newLine.Add(historyBasisArray[i, j].ToString());
                    }

                }
            }
            string tmp = "f(";
            for (int i = 0; i < flag-1; ++i)
            {
                if (newLine[i] != null)
                {
                    tmp += newLine[i].ToString()+"; ";
                }
            }
            tmp +=") ="+ newLine[flag-1].ToString();
            saveAnsver = newLine;
            return tmp;
        }
        public List<string> getListAnswer()
        {
            return saveAnsver;
        }

        public int getBassis()
        {
            return 0;
        }

        public void stepFullForward()
        {
            if (toSimplex is null)
                return;
            while (getForward())
            {
                stepForward();
            }
        }
    }
}
