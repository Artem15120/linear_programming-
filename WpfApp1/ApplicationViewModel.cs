using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using Microsoft.Win32;
using System.IO;

using Newtonsoft.Json;
//using CommunityToolkit.Mvvm.ComponentModel;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.Painting.Effects;
using LiveChartsCore.SkiaSharpView.VisualElements;
using SkiaSharp;
using LiveChartsCore.Defaults;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Media.Media3D;


namespace WpfApp1
{
    public class ApplicationViewModel : INotifyPropertyChanged
    {
        private int startingValue = 1;
        public GAUS resGaus = new GAUS();
        public GraficMethod graficMethod = new GraficMethod();

        public ArtificialBasis resBasis = new ArtificialBasis();

        public ObservableCollection<ObservablePoint> test1LineData = new ObservableCollection<ObservablePoint>();
        public ObservableCollection<ObservablePoint> test1PointData = new ObservableCollection<ObservablePoint>();
        private RelayCommand test1;
        private RelayCommand test2;
        public RelayCommand Test1
        {
            get{
                return test1 ??
                  (test1 = new RelayCommand(obj =>
                  {
                      // Создание экземпляра OpenFileDialog
                      OpenFileDialog openFileDialog = new OpenFileDialog();
                      // Установка фильтра для выбора только текстовых файлов
                      openFileDialog.Filter = "Text files (*.txt)|*.txt";
                      // Открытие диалогового окна выбора файла
                      bool? result = openFileDialog.ShowDialog();
                      // Проверка, был ли выбран файл
                      if (result == true)
                      {
                          // Получение пути к выбранному файлу
                          string filePath = openFileDialog.FileName;
                          GausLine = File.ReadAllLines(filePath);
                      }
                  }));
            }
        }
        public RelayCommand Test2
        {
            get
            {
                return test2 ??
                  (test2 = new RelayCommand(obj =>
                  {
                      string x = GausLine[0];
                      string[] parts = x.Split(' ');

                      if (!(int.TryParse(parts[0], out int firstNumber)) || !(int.TryParse(parts[1], out int secondNumber)))
                      {
                          MessageBox.Show("ERROR SYMBOL IN FIRST LINE");
                          return;
                      }

                      double[][] newMas = new double[firstNumber][];
                      for (int i = 0; i < firstNumber; i++)
                      {
                          string[] numbers = GausLine[i+1].Split(' ');
                          newMas[i] = new double[secondNumber];

                          for (int j = 0; j < secondNumber; j++)
                          {
                              double.TryParse(numbers[j], out newMas[i][j]);
                          }
                      }
                      List<string> tmp = resGaus.GetInfo(newMas);
                      GausLine = GausLine.Concat(tmp.Select(n => n.ToString())).ToArray();
                  }));
            }
        }

        private int sliderValueP=4;
        public int SliderValueP
        {
            get { return sliderValueP; }
            set
            {
                if (sliderValueP < value)
                {
                    Fraction[,] tmp = DeepCopy(CurArray);
                    CurArray = new Fraction[SliderValueO + 1, value];
                    SetCurArray(tmp);

                    for (int i = 0; i < CurArray.GetLength(0); ++i)
                    {
                        for (int j = SliderValueP; j < CurArray.GetLength(1); ++j)
                        {
                            CurArray[i, j] = new Fraction(startingValue);
                        }
                    }
                    CurArray[0, CurArray.GetLength(1) - 2] = new Fraction(1);
                    CurArray[0, CurArray.GetLength(1) - 1] = new Fraction("min");
                }
                else if(sliderValueP > value)
                {
                    Fraction[,] tmp = DeepCopy(CurArray);
                    CurArray = new Fraction[SliderValueO + 1, value];
                    SetCurArray(tmp);

                    for (int i = 0; i < CurArray.GetLength(0); ++i)
                    {
                        for (int j = SliderValueP + 1; j < CurArray.GetLength(1); ++j)
                        {
                            CurArray[i, j] = new Fraction(startingValue);
                        }
                    }
                    CurArray[0, CurArray.GetLength(1) - 1] = new Fraction("min");
                }
                sliderValueP = value;

                LoadData(Rows,Columns, CurArray);
                OnPropertyChanged("SliderValueP");
            }
        }
        private int sliderValueO=2;
        public int SliderValueO
        {
            get { return sliderValueO; }
            set
            {
                Fraction[,] tmp = DeepCopy(CurArray);
                CurArray = new Fraction[value + 1, SliderValueP];
                SetCurArray(tmp);
                //новые строки, новые единицы
                if (sliderValueO < value)
                {
                    for (int i = SliderValueO+1; i < CurArray.GetLength(0); ++i)
                    {
                        for (int j = 0; j < CurArray.GetLength(1); ++j)
                        {
                            CurArray[i, j] = new Fraction(startingValue);
                        }
                    }
                }
                sliderValueO = value;
                LoadData(Rows,Columns, CurArray);
                OnPropertyChanged("SliderValueO");
            }
        }

        private string[] gausLine;
        public string[] GausLine
        {
            get { return gausLine; }
            set
            {
                gausLine = value;
                OnPropertyChanged(nameof(GausLine));
            }
        }

        private Fraction[,] curArray;
        public Fraction[,] CurArray
        {
            get { return curArray; }
            set
            {
                curArray = value;
                OnPropertyChanged(nameof(CurArray));
            }
        }

        ////////////////////////////////////////////
        private ObservableCollection<DataRowValue> rows;
        public ObservableCollection<DataRowValue> Rows
        {
            get { return rows; }
            set
            {
                rows = value;
                OnPropertyChanged(nameof(Rows));
            }
        }
        private ObservableCollection<DataGridColumn> columns;
        public ObservableCollection<DataGridColumn> Columns
        {
            get { return columns; }
            set
            {
                columns = value;
                OnPropertyChanged(nameof(Columns));
            }
        }
        public class DataRowValue
        {
            //public bool ColorProperty { get; set; }
            public List<Fraction> Values { get; set; }

            public DataRowValue()
            {
                Values = new List<Fraction>();
            }
        }
        public void LoadData(ObservableCollection<DataRowValue> curRows, ObservableCollection<DataGridColumn> curColumns, Fraction[,] needArray)
        {
            curRows.Clear();
            for (int i = 0; i < needArray.GetLength(0); i++)
            {
                var dataRowValue = new DataRowValue();
                for (int j = 0; j < needArray.GetLength(1); j++)
                {
                    dataRowValue.Values.Add(needArray[i, j]);
                }
                curRows.Add(dataRowValue);

            }
            CreateColumnsForDataGrid(curColumns, needArray);
        }
        private void CreateColumnsForDataGrid(ObservableCollection<DataGridColumn> curColumns, Fraction[,] needArray)
        {
            string header = "name column";
            curColumns.Clear();
            if (needArray != null)
            {
                for (int i = 0; i < needArray.GetLength(1); i++)
                {
                    if(i + 1 == needArray.GetLength(1)) header = $"C";
                    else header = $"C {i + 1}";

                    var column = new DataGridTextColumn
                    {
                        Header = header,
                        Binding = new Binding($"Values[{i}]"),
                    };
                    column.ElementStyle = new Style(typeof(TextBlock))
                    {
                        Setters =
                        {
                            new Setter(TextBlock.BackgroundProperty, new SolidColorBrush(Colors.White))
                        },
                        Triggers =
                        {
                            new DataTrigger
                            {
                                Binding = new Binding($"Values[{i}].Type"),
                                Value = true,
                                Setters =
                                {
                                    new Setter(TextBlock.BackgroundProperty, new SolidColorBrush(Colors.LightBlue))
                                }
                            }
                        }
                    };
                    // выделение строки серым
                    var binding = new Binding("IsSelected");
                    binding.RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(DataGridRow), 1);

                    var multiDataTrigger = new MultiDataTrigger();
                    multiDataTrigger.Conditions.Add(new Condition { Binding = binding, Value = true });
                    multiDataTrigger.Conditions.Add(new Condition { Binding = new Binding($"Values[{i}].Type"), Value = false });
                    multiDataTrigger.Setters.Add(new Setter(TextBlock.BackgroundProperty, new SolidColorBrush(Colors.Gray)));

                    column.ElementStyle.Triggers.Add(multiDataTrigger);
                    //
                    curColumns.Add(column);
                }
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////
        ///
        //выбор как решать combobox
        private string selectedItem = "Искусственный базис";
        public string SelectedItem
        {
            get { return selectedItem; }
            set
            {
                selectedItem = value;
                OnPropertyChanged(nameof(SelectedItem));
            }
        }
        private string selectedItemNumberView = "Дробь";
        public string SelectedItemNumberView//измененик вида цифр
        {
            get { return selectedItemNumberView; }
            set
            {
                selectedItemNumberView = value;
                Fraction.ChangeOutputFormat();
                OnPropertyChanged(nameof(SelectedItemNumberView));

                if(ResArrayABasis is null)
                {
                    LoadData(Rows, Columns, CurArray);
                }
                else
                {
                    LoadData(Rows, Columns, CurArray);
                    ResArrayABasis = resBasis.getArray();
                    LoadData(RowsResABasis, ColumnsResABasis, ResArrayABasis);
                }
            }
        }
        //bazis vektor
        private string textBoxText;
        public string TextBoxText
        {
            get { return textBoxText; }
            set
            {
                textBoxText = value;
                OnPropertyChanged(nameof(TextBoxText));
            }
        }
        private Fraction[,] resArray;
        public Fraction[,] ResArray
        {
            get { return resArray; }
            set
            {
                resArray = value;
                OnPropertyChanged(nameof(ResArray));
            }
        }
        private RelayCommand buttonCommand;
        public RelayCommand ButtonCommand
        {
            get
            {
                return buttonCommand ??
                  (buttonCommand = new RelayCommand(obj =>
                  {
                      Fraction[,] tmp = DeepCopy(CurArray);
                      if(selectedItem == "Симплекс")
                      {
                          // Указываем разделители (запятая, точка, пробел)
                          char[] separators = new char[] { ',', '.', ' ' };
                          // Разбиваем строку на подстроки с помощью метода Split
                          string[] parts = TextBoxText.Split(separators, StringSplitOptions.RemoveEmptyEntries);
                          // Создаем массив целых чисел нужного размера
                          int[] basicVars = new int[parts.Length];
                          // Преобразуем подстроки в целые числа и заполняем массив
                          for (int i = 0; i < parts.Length; i++)
                          {
                              if (int.TryParse(parts[i], out int value))
                              {
                                  basicVars[i] = value;
                              }
                              else
                              {
                                  // Обработка ошибки преобразования, если подстрока не является целым числом
                                  MessageBox.Show(parts[i] + " Неожиданный символ");
                                  return;
                              }
                          }
                          Simplex res = new Simplex(tmp, basicVars);
                          ResArray =  res.getArray();
                          LoadData(RowsRes, ColumnsRes, ResArray);
                      }
                      else if (selectedItem == "Искусственный базис")
                      {
                          resBasis.setArray(tmp);
                          resBasis.addBasis();
                          ResArrayABasis = resBasis.getArray();
                          //заполнить опорные элементы
                          BasisString = resBasis.takeElementsBasis();
                          resBasis.paintColorAllTable(BasisString);
                          LoadData(RowsResABasis, ColumnsResABasis, ResArrayABasis);
                      }
                      else
                      {
                          MessageBox.Show("Try later");
                      }
                  }));
            }
        }

        private ObservableCollection<DataRowValue> rowsRes;
        public ObservableCollection<DataRowValue> RowsRes
        {
            get { return rowsRes; }
            set
            {
                rowsRes = value;
                OnPropertyChanged(nameof(RowsRes));
            }
        }
        private ObservableCollection<DataGridColumn> columnsRes;
        public ObservableCollection<DataGridColumn> ColumnsRes
        {
            get { return columnsRes; }
            set
            {
                columnsRes = value;
                OnPropertyChanged(nameof(ColumnsRes));
            }
        }
        ///////////////////////////////////////////////////////////////////////////////
        ///
        private Fraction[,] resArrayABasis;
        public Fraction[,] ResArrayABasis
        {
            get { return resArrayABasis; }
            set
            {
                resArrayABasis = value;
                OnPropertyChanged(nameof(ResArrayABasis));
            }
        }
        private ObservableCollection<DataRowValue> rowsResABasis;
        public ObservableCollection<DataRowValue> RowsResABasis
        {
            get { return rowsResABasis; }
            set
            {
                rowsResABasis = value;
                OnPropertyChanged(nameof(RowsResABasis));
            }
        }
        private ObservableCollection<DataGridColumn> columnsResABasis;
        public ObservableCollection<DataGridColumn> ColumnsResABasis
        {
            get { return columnsResABasis; }
            set
            {
                columnsResABasis = value;
                OnPropertyChanged(nameof(ColumnsResABasis));
            }
        }

        private RelayCommand buttonCommandForward;
        public RelayCommand ButtonCommandForward
        {
            get
            {
                return buttonCommandForward ??
                  (buttonCommandForward = new RelayCommand(obj =>
                  {
                      
                      //сделать шаг симплекса
                      resBasis.stepForward();
                      ResArrayABasis = resBasis.getArray();
                      //заполнить опорные элементы
                      BasisString = resBasis.takeElementsBasis();
                      resBasis.clearColorAllTable(BasisString);
                      resBasis.paintColorAllTable(BasisString);
                      //заполнить таблицу
                      LoadData(RowsResABasis, ColumnsResABasis, ResArrayABasis);
                  },
                  (obj) => resBasis.getForward()));
            }
        }
        private RelayCommand buttonCommandFullForward;
        public RelayCommand ButtonCommandFullForward
        {
            get
            {
                return buttonCommandFullForward ??
                  (buttonCommandFullForward = new RelayCommand(obj =>
                  {
                      resBasis.stepFullForward();
                      ResArrayABasis = resBasis.getArray();
                      //заполнить таблицу
                      LoadData(RowsResABasis, ColumnsResABasis, ResArrayABasis);
                  },
                  (obj) => resBasis.getForward()));
            }
        }
        private RelayCommand buttonCommandBack;
        public RelayCommand ButtonCommandBack
        {
            get
            {
                return buttonCommandBack ??
                  (buttonCommandBack = new RelayCommand(obj =>
                  {
                      resBasis.stepBack();
                      ResArrayABasis = resBasis.getArray();
                      //заполнить опорные элементы
                      BasisString = resBasis.takeElementsBasis();
                      resBasis.clearColorAllTable(BasisString);
                      resBasis.paintColorAllTable(BasisString);
                      LoadData(RowsResABasis, ColumnsResABasis, ResArrayABasis);
                  },
                  (obj)=>resBasis.getBack()));
            }
        }
        private RelayCommand buttonCommandSelectElement;
        public RelayCommand ButtonCommandSelectElement
        {
            get
            {
                return buttonCommandSelectElement ??
                  (buttonCommandSelectElement = new RelayCommand(obj =>
                  {
                      //сделать шаг симплекса по выб. опорному эл.
                      resBasis.stepForward(SelectedItemBasis);
                      ResArrayABasis = resBasis.getArray();
                      //заполнить опорные элементы
                      BasisString = resBasis.takeElementsBasis();
                      resBasis.clearColorAllTable(BasisString);
                      resBasis.paintColorAllTable(BasisString);
                      //заполнить таблицу
                      LoadData(RowsResABasis, ColumnsResABasis, ResArrayABasis);
                  },
                  (obj) => resBasis.getForward()));
            }
        }
        public void ChangeSelectElement(int columnIndex)
        {
            SelectedItemBasis = resBasis.getIndexStringFormat(columnIndex, BasisString);
        }
        private string[] basisString;
        public string[] BasisString {
            get { return basisString; }
            set
            {
                basisString = value;
                OnPropertyChanged(nameof(basisString));
            }
        }
        private string _selectedItemBasis;
        public string SelectedItemBasis
        {
            get { return _selectedItemBasis; }
            set
            {
                resBasis.clearColorAllTable(BasisString);
                _selectedItemBasis = value;
                //изменить цвет выбранной ячейки
                resBasis.paintColorLastTable(_selectedItemBasis);
                //обновить таблицу
                ResArrayABasis = resBasis.getArray();
                LoadData(RowsResABasis, ColumnsResABasis, ResArrayABasis);
                OnPropertyChanged("SelectedItem");
            }
        }

        //графический метод
        public string createAnswer()
        {
            //первый шаг
            Fraction[,] tmp = DeepCopy(CurArray);
            if (tmp.GetLength(0) > tmp.GetLength(1))
            {
                MessageBox.Show(tmp.GetLength(0)+" > " + tmp.GetLength(1));
                return null;
            }
            resBasis.setArray(tmp);
            resBasis.addBasis();
            ResArrayABasis = resBasis.getArray();
            //второгой шаг
            resBasis.stepFullForward();
            ResArrayABasis = resBasis.getArray();
            //заполнить таблицу
            LoadData(RowsResABasis, ColumnsResABasis, ResArrayABasis);
            return resBasis.getAnswer();
        }
        public int[] createAnswerSimplex()
        {
            //первый шаг
            Fraction[,] tmp = DeepCopy(CurArray);

            // Указываем разделители (запятая, точка, пробел)
            char[] separators = new char[] { ',', '.', ' ' };
            // Разбиваем строку на подстроки с помощью метода Split
            string[] parts = TextBoxText.Split(separators, StringSplitOptions.RemoveEmptyEntries);
            // Создаем массив целых чисел нужного размера
            int[] basicVars = new int[parts.Length];
            // Преобразуем подстроки в целые числа и заполняем массив
            for (int i = 0; i < parts.Length; i++)
            {
                if (int.TryParse(parts[i], out int value))
                {
                    basicVars[i] = value;
                }
                else
                {
                    // Обработка ошибки преобразования, если подстрока не является целым числом
                    MessageBox.Show(parts[i] + " Неожиданный символ");
                    return null;
                }
            }
            Simplex res = new Simplex(tmp, basicVars);
            ResArray = res.getArray();
            LoadData(RowsRes, ColumnsRes, ResArray);
            return basicVars;
        }
        //расчет для графика
        public List<(double y, double x, bool up, bool left)> getPoint(int[] basicVars)
        {
            graficMethod = new GraficMethod();
            graficMethod.setArray(DeepCopy(CurArray), basicVars);
            return graficMethod.getArrayStandart();
        }

        public List<(double y, double x, bool LeftRight)> getArrayVertical()
        {
            return graficMethod.getArrayVertical();
        }
        public List<(double y, double x, bool up)> getArrayHorizintal()
        {
            return graficMethod.getArrayHorizintal();
        }
        public List<(int y, int x, double resultPoint)> getPointsArray()
        {
            return graficMethod.getPointsArray();
        }

        public List<int> getCurCol()
        {
            return graficMethod.getCurColl();
        }
        public Fraction[,] getArrayGraficMethod()
        {
            return graficMethod.getArrayFirst();
        }
        public string getNameResultMaxOrMin()
        {
            return graficMethod.getNameResult();
        }
        public double getMaxGrafic()
        {
            return graficMethod.getMax();
        }
        public (int y, int x, double resultPoint) getMaxFuncInGrafic()
        {
            return graficMethod.getMaxFuncInGrafic();
        }

        public Fraction[,] DeepCopy(Fraction[,] source)
        {
            Fraction[,] result = (Fraction[,])source.Clone();
            for (int i = 0; i < source.GetLength(0); i++)
            {
                for (int j = 0; j < source.GetLength(1); j++)
                {
                    result[i, j] = source[i, j];
                }
            }
            return result;
        }
        public void SetCurArray(Fraction[,] source)
        {
            for(int i = 0; i < Math.Min(CurArray.GetLength(0), source.GetLength(0)); i++) { 
                for(int j = 0; j < Math.Min(CurArray.GetLength(1), source.GetLength(1)); j++)
                {
                    CurArray[i,j] = source[i,j];
                }
            }
        }
        public ObservableCollection<ISeries> Series { get; set; }
        public LabelVisual Title { get; set; } =
            new LabelVisual
            {
                Text = "My chart title",
                TextSize = 25,
                Padding = new LiveChartsCore.Drawing.Padding(15),
                Paint = new SolidColorPaint(SKColors.DarkSlateGray)
            };

        //Загрукза и сохранения
        private string _text6;
        public string Text6
        {
            get { return _text6; }
            set
            {
                if (_text6 != value)
                {
                    _text6 = value;
                    OnPropertyChanged(nameof(Text6));
                } 
            }
        }
        private string _textNameSave;
        public string TextNameSave
        {
            get { return _textNameSave; }
            set
            {
                if (_textNameSave != value)
                {
                    Text6 = value;
                    _textNameSave = value;
                    OnPropertyChanged(nameof(TextNameSave));

                    string filePath = "save/" + _textNameSave + ".json";
                    string json;
                    using (StreamReader file = File.OpenText(filePath))
                    {
                        json = file.ReadToEnd();
                    }
                    string[] jsonArr = JsonConvert.DeserializeObject<string[]>(json);
                    string jsonNet = jsonArr[0];

                    tmpSaveArray loadNetwork = JsonConvert.DeserializeObject<tmpSaveArray>(jsonNet);

                    SliderValueO = loadNetwork.getArray().GetLength(0);
                    SliderValueP = loadNetwork.getArray().GetLength(1);
                    CurArray = loadNetwork.getArray();
                    LoadData(Rows, Columns, CurArray);
                }
            }
        }
        public ObservableCollection<string> selectedSaveInFile { get; set; }
        private bool searchNameSave(string name)
        {
            foreach (string saveName in selectedSaveInFile)
            {
                if(saveName == name)
                {
                    return true;
                }
            }
            return false;
        }
        public void updateFileSave()
        {
            if (!Directory.Exists("save"))
            {
                //Создаем папку, если она не существует
                Directory.CreateDirectory("save");
            }
            string folderPath = "save";
            string[] fileNames = Directory.GetFiles(folderPath);
            selectedSaveInFile.Clear();

            foreach (string fileName in fileNames)
            {
                string documentName = System.IO.Path.GetFileNameWithoutExtension(fileName);
                selectedSaveInFile.Add(documentName);
            }
        }
        private RelayCommand saveCommand;
        private RelayCommand loadCommand;
        public RelayCommand SaveCommand
        {
            get
            {
                return saveCommand ??
                  (saveCommand = new RelayCommand(obj =>
                  {
                      tmpSaveArray array = new tmpSaveArray(DeepCopy(CurArray));
                      string jsonNet = JsonConvert.SerializeObject(array);
                      string filePath = "save/" + Text6 + ".json";

                      if (!Directory.Exists("save"))
                      {
                          //Создаем папку, если она не существует
                          Directory.CreateDirectory("save");
                      }

                      string jsonArray = JsonConvert.SerializeObject(new[] { jsonNet });

                      using (StreamWriter file = File.CreateText(filePath))
                      {
                          file.WriteLine(jsonArray);
                      }
                      ////VIEW LIST SAVE
                      updateFileSave();
                      MessageBox.Show("Save complited.");
                  }));
            }
        }
        public RelayCommand LoadCommand
        {
            get
            {
                return loadCommand ??
                  (loadCommand = new RelayCommand(obj =>
                  {
                      string filePath = "save/" + Text6 + ".json";
                      string json;
                      using (StreamReader file = File.OpenText(filePath))
                      {
                          json = file.ReadToEnd();
                      }
                      string[] jsonArr = JsonConvert.DeserializeObject<string[]>(json);
                      string jsonNet = jsonArr[0];

                      tmpSaveArray loadNetwork = JsonConvert.DeserializeObject<tmpSaveArray>(jsonNet);
                      
                      SliderValueO = loadNetwork.getArray().GetLength(0);
                      SliderValueP = loadNetwork.getArray().GetLength(1);
                      CurArray = loadNetwork.getArray();
                      LoadData(Rows, Columns, CurArray);
                  }));
            }
        }
        public ApplicationViewModel()
        {
            Rows = new ObservableCollection<DataRowValue>();
            RowsRes = new ObservableCollection<DataRowValue>();
            RowsResABasis = new ObservableCollection<DataRowValue>();
            Columns = new ObservableCollection<DataGridColumn>();
            ColumnsRes = new ObservableCollection<DataGridColumn>();
            ColumnsResABasis = new ObservableCollection<DataGridColumn>();
            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            TextBoxText = "2,3";
            //CurArray = new Fraction[,]
            //{
            //    { new Fraction(1), new Fraction(2),new Fraction(3),new Fraction(4),new Fraction(5),new Fraction("max") },
            //    { new Fraction(0), new Fraction(1),new Fraction(1),new Fraction(-2),new Fraction(7),new Fraction(2) },
            //    { new Fraction(1), new Fraction(0),new Fraction(1),new Fraction(-2),new Fraction(-6),new Fraction(2) },
            //    { new Fraction(1), new Fraction(1),new Fraction(0),new Fraction(-2),new Fraction(7),new Fraction(2) },
            //};


            //целочисленная задача графический метод+
            //16.238+
            //CurArray = new Fraction[,]
            //{
            //    { new Fraction(-1), new Fraction(-1),new Fraction(0),new Fraction(0), new Fraction("min") },
            //    { new Fraction(2), new Fraction(1),new Fraction(1),new Fraction(0), new Fraction(5) },
            //    { new Fraction(2), new Fraction(3),new Fraction(0),new Fraction(1), new Fraction(9) },
            //};
            //16.236 +
            //CurArray = new Fraction[,]
            //{
            //    { new Fraction(-2), new Fraction(2),new Fraction(-3),new Fraction(3),new Fraction(0), new Fraction("min") },
            //    { new Fraction(-1), new Fraction(-2),new Fraction(0),new Fraction(1),new Fraction(0), new Fraction(3) },
            //    { new Fraction(0), new Fraction(1),new Fraction(2),new Fraction(-2),new Fraction(0), new Fraction(5) },
            //    { new Fraction(0), new Fraction(3),new Fraction(0),new Fraction(1),new Fraction(1), new Fraction(4) },
            //};
            //16.229 +
            //            CurArray = new Fraction[,]
            //{
            //                                                                { new Fraction(-4), new Fraction(-3), new Fraction("min") },
            //                                                                { new Fraction(4), new Fraction(1), new Fraction(10) },
            //                                                                { new Fraction(2), new Fraction(3), new Fraction(8) },
            //};
            //16.228 ---------
            //            CurArray = new Fraction[,]
            //{
            //                                                { new Fraction(-9), new Fraction(-11), new Fraction("min") },
            //                                                { new Fraction(4), new Fraction(3), new Fraction(10) },
            //                                                { new Fraction(1), new Fraction(0), new Fraction(5) },
            //                                                { new Fraction(1), new Fraction(2), new Fraction(8) },
            //};
            //16.227 >=
            //CurArray = new Fraction[,]
            //{
            //    { new Fraction(-1), new Fraction(-1), new Fraction("min") },
            //    { new Fraction(2), new Fraction(3), new Fraction(36) },
            //    { new Fraction(1), new Fraction(0), new Fraction(13) },
            //    { new Fraction(3), new Fraction(1), new Fraction(6) }
            //};
            //+++
            //            CurArray = new Fraction[,]
            //{
            //                                                                { new Fraction(70), new Fraction(40), new Fraction("max") },
            //                                                                { new Fraction(6), new Fraction(3), new Fraction(37) },
            //                                                                { new Fraction(3), new Fraction(2), new Fraction(21) },
            //};
            //++
            //CurArray = new Fraction[,]
            //{
            //    { new Fraction(2), new Fraction(1), new Fraction(3), new Fraction(1),new Fraction("max") },
            //    { new Fraction(1), new Fraction(2), new Fraction(5),new Fraction(-1), new Fraction(4) },
            //    { new Fraction(1), new Fraction(-1), new Fraction(-1),new Fraction(2), new Fraction(1) }
            //};
            //искуственный базис задачи дз
            //4.10 + график чек
            //CurArray = new Fraction[,]
            //{
            //    { new Fraction(2), new Fraction(1), new Fraction(-1), new Fraction(3), new Fraction(-2), new Fraction("min") },
            //    { new Fraction(8), new Fraction(2), new Fraction(3), new Fraction(9),new Fraction(9), new Fraction(30) },
            //    { new Fraction(5), new Fraction(1), new Fraction(2),new Fraction(5), new Fraction(6),new Fraction(19) },
            //    { new Fraction(1), new Fraction(1), new Fraction(0), new Fraction(3), new Fraction(0), new Fraction(3) },
            //};
            //4.96ошибка должна быть плоъой график
            //CurArray = new Fraction[,]
            //{
            //    { new Fraction(-5), new Fraction(-4), new Fraction(-3), new Fraction(-2), new Fraction(3), new Fraction("min") },
            //    { new Fraction(2), new Fraction(1), new Fraction(1), new Fraction(1),new Fraction(-1), new Fraction(3) },
            //    { new Fraction(1), new Fraction(-1), new Fraction(0),new Fraction(1), new Fraction(1),new Fraction(1) },
            //    { new Fraction(-2), new Fraction(-1), new Fraction(-1), new Fraction(1), new Fraction(0), new Fraction(1) },
            //};
            //4.8+
            //            CurArray = new Fraction[,]
            //{
            //                                        { new Fraction(5), new Fraction(-2), new Fraction(2), new Fraction(-4), new Fraction(1),new Fraction(2), new Fraction("max") },
            //                                        { new Fraction(2), new Fraction(-1), new Fraction(1), new Fraction(-2), new Fraction(1),new Fraction(1), new Fraction(1) },
            //                                        { new Fraction(-3), new Fraction(1), new Fraction(0), new Fraction(1), new Fraction(-1), new Fraction(1),new Fraction(2) },
            //                                        { new Fraction(-5), new Fraction(1), new Fraction(-2), new Fraction(1), new Fraction(0),new Fraction(-1), new Fraction(3) },
            //};
            //4.7+
            //CurArray = new Fraction[,]
            //{
            //    { new Fraction(-2), new Fraction(2), new Fraction(1), new Fraction(2), new Fraction(-3), new Fraction("max") },
            //    { new Fraction(-2), new Fraction(1), new Fraction(-1), new Fraction(-1), new Fraction(0), new Fraction(1) },
            //    { new Fraction(1), new Fraction(-1), new Fraction(2), new Fraction(1), new Fraction(1), new Fraction(4) },
            //    { new Fraction(-1), new Fraction(1), new Fraction(0), new Fraction(0), new Fraction(-1), new Fraction(4) },
            //};
            //4.6ошибка должна быть плоъой график
            //CurArray = new Fraction[,]
            //{
            //    { new Fraction(-1), new Fraction(-10), new Fraction(1), new Fraction(-5), new Fraction("min") },
            //    { new Fraction(1), new Fraction(2), new Fraction(-1), new Fraction(-1), new Fraction(1) },
            //    { new Fraction(-1), new Fraction(2), new Fraction(3), new Fraction(1), new Fraction(2) },
            //    { new Fraction(1), new Fraction(5), new Fraction(1), new Fraction(-1), new Fraction(5) }
            //};
            //4.5+
            //CurArray = new Fraction[,]
            //{
            //    { new Fraction(-1), new Fraction(5), new Fraction(1), new Fraction(-1), new Fraction("min") },
            //    { new Fraction(1), new Fraction(3), new Fraction(3), new Fraction(1), new Fraction(3) },
            //    { new Fraction(2), new Fraction(0), new Fraction(3), new Fraction(-1), new Fraction(4) }
            //};
            //4,4 ошибка должна быть плоъой график
            //            CurArray = new Fraction[,]
            //{
            //            { new Fraction(-1), new Fraction(4), new Fraction(-3), new Fraction(-10), new Fraction("min") },
            //            { new Fraction(1), new Fraction(1), new Fraction(-1), new Fraction(1), new Fraction(0) },
            //            { new Fraction(1), new Fraction(14), new Fraction(10), new Fraction(-10), new Fraction(11) }
            //};
            //4,3+
            //CurArray = new Fraction[,]
            //{
            //    { new Fraction(-1), new Fraction(-2), new Fraction(-3), new Fraction(4), new Fraction("min") },
            //    { new Fraction(1), new Fraction(1), new Fraction(-1), new Fraction(1), new Fraction(2) },
            //    { new Fraction(1), new Fraction(14), new Fraction(10), new Fraction(-10), new Fraction(24) }
            //};
            //4.2-
            //CurArray = new Fraction[,]
            //{
            //    { new Fraction(-1), new Fraction(10), new Fraction(-1), new Fraction("min") },
            //    { new Fraction(-1), new Fraction(5), new Fraction(7), new Fraction(13) },
            //    { new Fraction(1), new Fraction(29,2), new Fraction(7), new Fraction(15) }
            //};
            //4,1+
            //CurArray = new Fraction[,]
            //{
            //    { new Fraction(-1), new Fraction(-4), new Fraction(-1),new Fraction("min") },
            //    { new Fraction(1), new Fraction(-1), new Fraction(1), new Fraction(3) },
            //    { new Fraction(2), new Fraction(-5), new Fraction(-1), new Fraction(0) }
            //};
            //TextBoxText = "0, 2";+
            CurArray = new Fraction[,]
            {
                { new Fraction(-1), new Fraction(3), new Fraction(5), new Fraction(1),new Fraction("min") },
                { new Fraction(1), new Fraction(4), new Fraction(4),new Fraction(1), new Fraction(5) },
                { new Fraction(1), new Fraction(7), new Fraction(8),new Fraction(2), new Fraction(9) }
            };
            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            LoadData(Rows, Columns, CurArray);

            selectedSaveInFile = new ObservableCollection<string>();
            updateFileSave();








            //gaus sett
            GausLine = new string[] { "3 4", "3 2 -5 -1", "2 -1 3 13", "1 2 -1 9" };

            test1PointData.Add(new ObservablePoint(1.5, 1.5));

            var strokeThickness = 10;
            var strokeDashArray = new float[] { 3 * strokeThickness, 2 * strokeThickness };
            var effect = new DashEffect(strokeDashArray);
            Series = new ObservableCollection<ISeries>
            {
                new LineSeries<ObservablePoint>
                {
                    Values = test1LineData,
                },
                new LineSeries<ObservablePoint>
                {
                    Values = test1PointData,
                    Stroke = new SolidColorPaint
                    {
                        Color = SKColors.Green,
                        StrokeCap = SKStrokeCap.Round,
                        StrokeThickness = strokeThickness,
                        PathEffect = effect
                    },
                    Fill = null
                }
            };
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
    } 
    public class IndexToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int index = (int)value;
            return index == 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class ArrayToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string[] array)
            {
                return string.Join("\n", array.Skip(1));
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
