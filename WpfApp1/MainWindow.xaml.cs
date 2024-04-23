using OxyPlot.Series;
using OxyPlot;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static WpfApp1.ApplicationViewModel;
using OxyPlot.Axes;
using System.Linq.Expressions;
using Expression = System.Linq.Expressions.Expression;
using LiveChartsCore.SkiaSharpView;
using OxyPlot;
using OxyPlot.Wpf;
using OxyPlot.Legends;
using System.Collections;
using System.Text.RegularExpressions;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ApplicationViewModel mvvm = new ApplicationViewModel();
        public MainWindow()
        {
            InitializeComponent();
            DataContext = mvvm;
            Loaded += MainWindow_Loaded;
            //this.Width = 525;
            //this.Height = 350;
            this.SizeToContent = SizeToContent.WidthAndHeight;


            MethodBox.ItemsSource = new List<string> {"Симплекс", "Искусственный базис" };
            MethodBox.SelectedIndex = 0;
            ViewNumBox.ItemsSource = new List<string> { "Дробь", "Десятичное" };
            ViewNumBox.SelectedIndex = 0;
        }

        private void DataGrid_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Получить текущую ячейку
            DataGridCell cell = GetCell(myDataGrid, e.OriginalSource as FrameworkElement);
            if (cell != null)
            {
                int columnIndex = cell.Column.DisplayIndex;
                // Проверить, что индексы строки и столбца допустимы
                if (columnIndex >= 0 && columnIndex < myDataGrid.Columns.Count)
                {
                    // Получить экземпляр команды
                    RelayCommand buttonCommandSelectElementInstance = mvvm.ButtonCommandSelectElement;
                    // Вызвать команду
                    if (buttonCommandSelectElementInstance != null && buttonCommandSelectElementInstance.CanExecute(null))
                    {
                        //изменить эл. на выбранный элемент
                        mvvm.ChangeSelectElement(columnIndex);
                        //нет такого столбца. ничего не делай
                        if (mvvm.SelectedItemBasis is null)
                            return;
                        //сделать шаг по выбарнному опрному элементу
                        buttonCommandSelectElementInstance.Execute(null);
                    }
                }
            }
        }

        private DataGridCell GetCell(DataGrid dataGrid, FrameworkElement element)
        {
            // Проверить, является ли элемент DataGridCell
            if (element != null)
            {
                DependencyObject parent = VisualTreeHelper.GetParent(element);
                while (parent != null && parent.GetType() != typeof(DataGridCell))
                {
                    parent = VisualTreeHelper.GetParent(parent);
                }
                return parent as DataGridCell;
            }
            return null;
        }

        //изменение таблиц вручную
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (myDataGrid is null)
                return;
            myDataGrid.Columns.Clear();
            foreach (var column in mvvm.Columns)
            {
                myDataGrid.Columns.Add(column);
            }
            myDataGrid.Items.Refresh();
        }
        private void MainWindow_LoadedRes(object sender, RoutedEventArgs e)
        {
            if (simplDataGrid is null)
                return;
            simplDataGrid.Columns.Clear();
            foreach (var column in mvvm.ColumnsRes)
            {
                simplDataGrid.Columns.Add(column);
            }
            simplDataGrid.Items.Refresh();
        }
        private void MainWindow_LoadedResABasis(object sender, RoutedEventArgs e)
        {
            if (abasisDataGrid is null)
                return;
            abasisDataGrid.Columns.Clear();
            foreach (var column in mvvm.ColumnsResABasis)
            {
                abasisDataGrid.Columns.Add(column);
            }
            abasisDataGrid.Items.Refresh();
        }
        //когда запускать изменения таблиц
        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lowTabControl.SelectedItem == tabItemSimpl)
            {
                MainWindow_LoadedRes(sender, e);
            }
            else if (lowTabControl.SelectedItem == tabItemABasis)
            {
                MainWindow_LoadedResABasis(sender, e);
            }
        }
        
        int stop1=0, stop2=0, stop3 = 0;
        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            
            if(((Slider)sender).Name == "countP")
            {
                ((Slider)sender).SelectionEnd = e.NewValue;
                if (stop1 > 1) MainWindow_Loaded(sender, e);
                stop1++;
            }
            else if(((Slider)sender).Name == "countO")
            {
                ((Slider)sender).SelectionEnd = e.NewValue;
                if (stop2 > 1) MainWindow_Loaded(sender, e);
                stop2++;
            }            
        }
        //сохранение изменения ячейки
        private void DataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Commit)
            {
                DataGridBoundColumn column = e.Column as DataGridBoundColumn;
                if (column != null)
                {
                    TextBox textBox = e.EditingElement as TextBox;
                    if (textBox != null)
                    {
                        string newText = textBox.Text;
                        if (Fraction.TryParse(newText, out Fraction newValue))
                        {
                            // Получите текущий номер строки и столбца
                            int rowIndex = e.Row.GetIndex();
                            int columnIndex = column.DisplayIndex;

                            // Обновите значение ячейки в источнике данных
                            DataRowValue rowValue = myDataGrid.Items[rowIndex] as DataRowValue;
                            rowValue.Values[columnIndex] = newValue;

                             // сохранить изменения в model
                            Fraction[,] tmp = mvvm.CurArray;
                            tmp[rowIndex, columnIndex] = newValue;
                            myDataGrid.CancelEdit();
                        }
                        else
                        {
                            MessageBox.Show("Это не Fraction");
                            // Значение не является целым числом,
                        }
                    }
                }
            }
        }
        //Изменение цвета кнопки
        private void myButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            button.Background = new SolidColorBrush(GetRandomColor());
        }
        private Color GetRandomColor()
        {
            Random random = new Random();
            byte r = (byte)random.Next(256);
            byte g = (byte)random.Next(256);
            byte b = (byte)random.Next(256);
            return Color.FromRgb(r, g, b);
        }

        //графический метод решения задачи
        // Создаем массив цветов
        private int countGrafic = 0;
        OxyColor[] colors = new OxyColor[]
        {
                OxyColors.Blue,
                OxyColors.Red,
                OxyColors.Purple,
                OxyColors.Black,
                OxyColors.Orange,
                OxyColors.Yellow,
                OxyColors.Brown,
                OxyColors.Pink,
                OxyColors.Teal,
                OxyColors.Gray,
                OxyColors.Indigo,
                OxyColors.Lime,
                OxyColors.Maroon,
                OxyColors.Navy,
                OxyColors.Olive,
                OxyColors.Silver
        };
        static int CountNonZeroValues(string input)
        {
            if (input == null)
                return 99;
            string pattern = @"f\(([^)]+)\)";
            Match match = Regex.Match(input, pattern);

            if (match.Success)
            {
                string valuesString = match.Groups[1].Value;
                string valuesPattern = @"-?\d+(/-?\d+)?";
                MatchCollection matches = Regex.Matches(valuesString, valuesPattern);

                int count = matches
                    .Cast<Match>()
                    .Select(m =>
                    {
                        string value = m.Value;
                        if (value.Contains("/"))
                        {
                            int numerator = int.Parse(value.Split('/')[0]);
                            int denominator = int.Parse(value.Split('/')[1]);
                            return (double)numerator / denominator;
                        }
                        else
                        {
                            return int.Parse(value);
                        }
                    })
                    .Count(n => n != 0);

                return count;
            }

            return 0;
        }
        public void paintResult(object sender, RoutedEventArgs e)
        {
            List<(double y, double x, bool up, bool left)> listPoint = new List<(double y, double x, bool up, bool left)>();
            List<int> tmpCol = new List<int>();
            (double, double) maxYX = (10,10);
            countGrafic = 0;
            var plotModelnew = new PlotModel();
            plotView.Model = plotModelnew;
            //магческая функция возвращающая ответ
            string answer;
            try{
                answer = mvvm.createAnswer();
            }catch (Exception)
            {
                MessageBox.Show("Ошибка");
                return;
            }

            if (answer is null)//графический метод
            {
                //    int[] Bassis = mvvm.createAnswerSimplex();
                //    listPoint = mvvm.getPoint(Bassis);
                //MessageBox.Show(".");
                return;
            }
            else//использовать иск. базис для поиска точки
            {
                int countChek = CountNonZeroValues(answer);
                if (countChek > 2)
                    return;

                int[] Bassis;int count = 0, countnotzero = 0;
                mvvm.resBasis.countstep = 1;
                if (true)
                {
                    List<string> ansList = mvvm.resBasis.getListAnswer();
                    //создать массив базисов по иск. базису решенному
                    Fraction x;
                    for (int i = 0; i < ansList.Count() - 1; ++i)
                    {
                        if (Fraction.TryParse(ansList[i], out x))
                        {
                            if (x.Decimal == 0)
                            {
                                ++count;
                            }
                            else if (x.Decimal != 0)
                            {
                                ++countnotzero;
                            }
                        }
                    }
                    if (countnotzero > 2)
                        return;
                    Bassis = new int[count];
                    count = 0;
                    for (int i = 0; i < ansList.Count() - 1; ++i)
                    {
                        if (Fraction.TryParse(ansList[i], out x))
                        {
                            if (x.Decimal == 0)
                            {
                                Bassis[count] = i;
                                ++count;
                            }
                        }
                    }
                }
                listPoint = mvvm.getPoint(Bassis); 
                tmpCol = mvvm.getCurCol();
                maxYX = SearchLineMaxYandX(listPoint);
            }

            if(listPoint == null)
            {
                return;
            }

            //нарисовать координатную сетку
            //double max = mvvm.getMaxGrafic();
            // Создадим PlotModel//нарисовать ограничения
            var plotModel = CreatePlotModel(answer, tmpCol);
            plotModel = CreateStandert(plotModel, listPoint, maxYX);
            plotModel = CreateHorizintal(plotModel, maxYX.Item2);
            plotModel = CreateVertical(plotModel, maxYX.Item1);
            
            //получить точки для графика
            List<(int y, int x, double resultPoint)> listPointFunc = mvvm.getPointsArray();
            plotModel = CreatePoints(plotModel, listPointFunc);
            // Отобразим график
            //сетка координат
            var lineSeries = new LineSeries
            {
                ItemsSource = new[] { new DataPoint(0, 0), new DataPoint(0, maxYX.Item1) },
                Color = OxyColors.Black,
                MarkerType = MarkerType.None,
            };
            plotModel.Series.Add(lineSeries);
            lineSeries = new LineSeries
            {
                ItemsSource = new[] { new DataPoint(0, 0), new DataPoint(maxYX.Item2, 0) },
                Color = OxyColors.Black,
                MarkerType = MarkerType.None,
            };
            plotModel.Series.Add(lineSeries);
            plotView.Model = plotModel;
        }

        private string GetSubscriptString(int exponent)
        {
            if (exponent < 0 || exponent > 16)
            {
                return "?";
            }
            string[] subscripts = { "₀", "₁", "₂", "₃", "₄", "₅", "₆", "₇", "₈", "₉", "₁₀", "₁₁", "₁₂", "₁₃", "₁₄", "₁₅", "₁₆" };
            return $"x{subscripts[exponent]}";
        }
        private PlotModel CreatePlotModel(string answer, List<int> tmpCol)
        {
            var plotModel = new PlotModel { Title = " " };
            plotModel.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Bottom,
                Title = "X" + (tmpCol[0] + 1),
                MajorGridlineStyle = LineStyle.Solid, // включает основную сетку
                MinorGridlineStyle = LineStyle.Dot, // включает вспомогательную сетку
                MajorStep = 1, // шаг основной сетки
                MinorStep = 0.1 // шаг вспомогательной сетки
            });
            plotModel.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Left,
                Title = "X" + (tmpCol[1] + 1),
                MajorGridlineStyle = LineStyle.Solid, // включает основную сетку
                MinorGridlineStyle = LineStyle.Dot, // включает вспомогательную сетку
                MajorStep = 1, // шаг основной сетки
                MinorStep = 0.1 // шаг вспомогательной сетки
            });

            // Создаем новый экземпляр Legend
            (int y, int x, double resultPoint) maxPoint = mvvm.getMaxFuncInGrafic();
            Fraction[,] tmpArray = mvvm.getArrayGraficMethod();

            string textLegend = answer + "\n";

            for (int j = 0; j < tmpArray.GetLength(1) - 1; ++j)
            {
                if (tmpArray[0, j].Decimal != 0)
                {
                    if (tmpArray[0, j].Decimal > 0 && textLegend.Count() > 0)
                    {
                        textLegend += "+";
                    }
                    textLegend += tmpArray[0, j].ToString() + " * " + GetSubscriptString((j + 1)) + " ";
                }
            }
            textLegend += "--> "+mvvm.getNameResultMaxOrMin()+"\n";

            for (int i = 1; i < tmpArray.GetLength(0); ++i)
            {
                for (int j = 0; j < tmpArray.GetLength(1)-1; ++j)
                {
                    if (tmpArray[i,j].Decimal != 0)
                    {
                        if (tmpArray[i,j].Decimal > 0 )
                        {
                            textLegend +="+";
                        }
                        textLegend += tmpArray[i, j].ToString()+ " * " + GetSubscriptString((j + 1)) + " ";
                    }
                }
                textLegend += "<= "+ tmpArray[i, tmpArray.GetLength(1)-1]+"\n";
            }
            //legend end
            // Устанавливаем позицию легенды
            plotModel.Legends.Add(new Legend()
            {
                LegendTitle = textLegend,
                LegendPosition = LegendPosition.RightBottom,
            });
            return plotModel;
        }
        private PlotModel CreateStandert(PlotModel plotModel, List<(double y, double x, bool up, bool left)> listPoint, (double,double) max)
        {
            // Ограничения
            var points = new DataPoint[2, listPoint.Count()];
            bool[] areaUp = new bool[listPoint.Count()];
            for (int i = 0; i < listPoint.Count(); i += 2)
            {
                points[0, i] = new DataPoint(listPoint[i].y, listPoint[i].x);
                points[1, i] = new DataPoint(listPoint[i + 1].y, listPoint[i + 1].x);
                areaUp[i] = listPoint[i].up;
            }
            //нарисовать линии ограничений и 0-1 это вектор нормали
            for (int i = 0; i < points.GetLength(1); i += 2)
            {
                if (points[0, i].X == points[0, i].Y && points[1, i].X == points[1, i].Y && points[0, i].X == points[1, i].Y)
                {//это точка в 4 координатах, больше нет точек для рисования
                    return plotModel;
                }
                if (i == 0)
                {
                    var lineSeries = new LineSeries
                    {
                        ItemsSource = new[] { points[0, i], points[1, i] },
                        Color = OxyColors.Green,
                        MarkerType = MarkerType.None,
                        Title = "Вектор нормали"
                    };
                    plotModel.Series.Add(lineSeries);
                }
                else
                {
                    var lineSeries = new LineSeries
                    {
                        ItemsSource = new[] { points[0, i], points[1, i] },
                        Color = colors[countGrafic],
                        MarkerType = MarkerType.None,
                        Title = "Линия "+ countGrafic
                    };
                    plotModel.Series.Add(lineSeries);
                    //True -> на линии или справа. //False -> слева от линии
                    bool leftR = listPoint[i].left;
                    //areaUp - true закрасить выше
                    if (areaUp[i])
                    {
                        // Создаем AreaSeries для закрашивания области выше линии и левее
                        if (leftR)
                        {
                            double x1 = points[0, i].X;
                            double y1 = points[0, i].Y;
                            double x2 = points[1, i].X;
                            double y2 = points[1, i].Y;
                            var areaSeries = new AreaSeries
                            {
                                ItemsSource = new List<DataPoint>
                                {
                                    new DataPoint(x1, y1),
                                    new DataPoint(0, 0),
                                    new DataPoint(0, y2),
                                    new DataPoint(x2, y2),
                                    new DataPoint(x1, y1),
                                },
                                Color = colors[countGrafic],
                                Title = "Область " + (countGrafic),
                                MarkerType = MarkerType.None
                            };                            
                            plotModel.Series.Add(areaSeries);
                        }
                        else//правее
                        {
                            double x1 = points[0, i].X;
                            double y1 = points[0, i].Y;
                            double x2 = points[1, i].X;
                            double y2 = points[1, i].Y;
                            var areaSeries = new AreaSeries
                            {
                                ItemsSource = new List<DataPoint>
                                {
                                    new DataPoint(x2, y2),
                                    new DataPoint(max.Item2, max.Item1),
                                    new DataPoint(max.Item2, 0),
                                    new DataPoint(x1, y1),
                                    new DataPoint(x2, y2),
                                },
                                Color = colors[countGrafic],
                                Title = "Область " + (countGrafic),
                                MarkerType = MarkerType.None
                            };
                            plotModel.Series.Add(areaSeries);
                        }
                    }
                    else
                    {// Создаем AreaSeries для закрашивания области ниже линии
                        var areaSeries = new AreaSeries
                        {
                            ItemsSource = new[] { points[0, i], points[1, i] },
                            Color = colors[countGrafic], //  цвет с прозрачностью
                            Title = "Область " + (countGrafic),
                            MarkerType = MarkerType.None
                        };
                        plotModel.Series.Add(areaSeries);
                    }
                    ++countGrafic;
                }
            }

            return plotModel;
        }
        private PlotModel CreateHorizintal(PlotModel plotModel, double max)
        {
            List<(double y, double x, bool up)> listPoint = mvvm.getArrayHorizintal();
            var points = new DataPoint[2, listPoint.Count()];
            for (int i = 0; i < listPoint.Count(); i += 2)
            {
                points[0, i] = new DataPoint(listPoint[i].y, listPoint[i].x);
                points[1, i] = new DataPoint(listPoint[i + 1].y, listPoint[i + 1].x);
            }
            
            for (int i = 0; i < listPoint.Count; ++i)
            {
                if (points[0, i].X == points[0, i].Y && points[1, i].X == points[1, i].Y && points[0, i].X == points[1, i].Y)
                {//это точка в 4 координатах, больше нет точек для рисования
                    return plotModel;
                }
                var lineSeries = new LineSeries
                {
                    ItemsSource = new[] { points[0, i], points[1, i] },
                    Color = colors[countGrafic],
                    MarkerType = MarkerType.None,
                    Title = "Линия " + countGrafic
                };
                plotModel.Series.Add(lineSeries);
                double x1 = points[0, i].X;
                double y1 = points[0, i].Y;
                double x2 = points[1, i].X;
                double y2 = points[1, i].Y;
                x2 = max;
                if (listPoint[i].up)
                {
                    // Создаем AreaSeries для закрашивания области выше линии
                    var areaSeries = new AreaSeries
                    {
                        ItemsSource = new List<DataPoint>
                        {
                            new DataPoint(x1, y1),
                            new DataPoint(x1, max),
                            new DataPoint(max, max),
                            new DataPoint(x2, y2),
                            new DataPoint(x1, y1),
                        },
                        Color = colors[countGrafic],
                        Title = "Область " + countGrafic,
                        MarkerType = MarkerType.None
                    };
                    plotModel.Series.Add(areaSeries);
                }
                else
                {// Создаем AreaSeries для закрашивания области ниже линии
                    //double x1 = points[0, i].X;
                    //double y1 = points[0, i].Y;
                    //double x2 = points[1, i].X;
                    //double y2 = points[1, i].Y;
                    var areaSeries = new AreaSeries
                    {
                        ItemsSource = new List<DataPoint>
                        {
                            new DataPoint(x1, y1),
                            new DataPoint(x2, y2),
                            new DataPoint(x2, -1),
                            new DataPoint(0, -1),
                            new DataPoint(x1, y1),
                        },
                        Title = "Область " + countGrafic,
                        Color = colors[countGrafic],
                        MarkerType = MarkerType.None
                    };
                    plotModel.Series.Add(areaSeries);
                }
                ++countGrafic;
            }
            return plotModel;
        }
        private PlotModel CreateVertical(PlotModel plotModel, double max)
        {
            List<(double y, double x, bool LeftRight)> listPoint = mvvm.getArrayVertical();
            var points = new DataPoint[2, listPoint.Count()];
            for (int i = 0; i < listPoint.Count(); i += 2)
            {
                points[0, i] = new DataPoint(listPoint[i].y, listPoint[i].x);
                points[1, i] = new DataPoint(listPoint[i + 1].y, listPoint[i + 1].x);
            }

            for (int i = 0; i < listPoint.Count; ++i)
            {
                if (points[0, i].X == points[0,i].Y && points[1, i].X == points[1, i].Y && points[0, i].X == points[1, i].Y)
                {//это точка в 4 координатах, больше нет точек для рисования
                    return plotModel;
                }
                var lineSeries = new LineSeries
                {
                    ItemsSource = new[] { points[0, i], points[1, i] },
                    Color = colors[countGrafic],
                    MarkerType = MarkerType.None,
                    Title = "Линия " + countGrafic
                };
                plotModel.Series.Add(lineSeries);

                double x1 = points[0, i].X;
                double y1 = points[0, i].Y;
                double x2 = points[1, i].X;
                double y2 = points[1, i].Y;
                y1 = max;
                if (listPoint[i].LeftRight)
                {//right area
                    // Создаем AreaSeries для закрашивания области выше линии
                    //double x1 = points[0, i].X;
                    //double y1 = points[0, i].Y;
                    //double x2 = points[1, i].X;
                    //double y2 = points[1, i].Y;
                    var areaSeries = new AreaSeries
                    {
                        ItemsSource = new List<DataPoint>
                        {
                            new DataPoint(x2, y2),
                            new DataPoint(max, y2),
                            new DataPoint(max, max),
                            new DataPoint(x1, y1),
                            new DataPoint(x2, y2),
                        },
                        Title = "Область " + countGrafic,
                        Color = colors[countGrafic],
                        MarkerType = MarkerType.None
                    };
                    plotModel.Series.Add(areaSeries);
                }
                else
                {// Создаем left area
                    //double x1 = points[0, i].X;
                    //double y1 = points[0, i].Y;
                    //double x2 = points[1, i].X;
                    //double y2 = points[1, i].Y;
                    var areaSeries = new AreaSeries
                    {
                        ItemsSource = new List<DataPoint>
                        {
                            new DataPoint(x2, y2),
                            new DataPoint(-1, 0),
                            new DataPoint(-1,max),
                            new DataPoint(x1, y1),
                            new DataPoint(x2, y2),
                        },
                        Title = "Область " + countGrafic,
                        Color = colors[countGrafic],
                        MarkerType = MarkerType.None
                    };
                    plotModel.Series.Add(areaSeries);
                }
                ++countGrafic;
            }
            return plotModel;
        }
        private PlotModel CreatePoints(PlotModel plotModel, List<(int y, int x, double resultPoint)> listPointFunc)
        {
            List<int> tmpCol = mvvm.getCurCol();
            List<string> ansList = mvvm.resBasis.getListAnswer();
            bool flagPoint = false;

            if(tmpCol is null)
            {
                flagPoint = true;
            }
            else
            {
                Fraction x,y;

                if (Fraction.TryParse(ansList[tmpCol[0]], out x))
                {
                }
                if (Fraction.TryParse(ansList[tmpCol[1]], out y))
                {
                }

                // Создаем ScatterSeries для рисования точки
                var scatterSeries1 = new ScatterSeries
                {
                    ItemsSource = new[] { new ScatterPoint(x.Decimal, y.Decimal) }, // Устанавливаем координаты точки
                    MarkerType = MarkerType.Circle, // Устанавливаем тип маркера (точки)
                    MarkerSize = 5, // Устанавливаем размер маркера
                    MarkerFill = OxyColors.Red, // Устанавливаем цвет заливки маркера
                    MarkerStroke = OxyColors.Red, // Устанавливаем цвет контура маркера
                    MarkerStrokeThickness = 1, // Устанавливаем толщину контура маркера
                    TrackerFormatString = "{Tag}", // Устанавливаем формат отображения трекера
                    Title = "Точка max F(x)= " + ansList[ansList.Count - 1]
                };
                // Добавляем данные в контекст точки
                scatterSeries1.ItemsSource.Cast<ScatterPoint>().ToList()[0].Tag = "F:" + ansList[ansList.Count - 1];
                // Добавляем ScatterSeries на график
                plotModel.Series.Add(scatterSeries1);
                flagPoint = false;
            }


            (int y, int x, double resultPoint) maxPoint = mvvm.getMaxFuncInGrafic();
            for (int i = 0; i< listPointFunc.Count; ++i)
            {
                if (maxPoint.y == listPointFunc[i].y && maxPoint.x == listPointFunc[i].x && flagPoint)
                {
                    // Создаем ScatterSeries для рисования точки
                    var scatterSeries = new ScatterSeries
                    {
                        ItemsSource = new[] { new ScatterPoint(listPointFunc[i].x, listPointFunc[i].y) }, // Устанавливаем координаты точки
                        MarkerType = MarkerType.Circle, // Устанавливаем тип маркера (точки)
                        MarkerSize = 5, // Устанавливаем размер маркера
                        MarkerFill = OxyColors.Red, // Устанавливаем цвет заливки маркера
                        MarkerStroke = OxyColors.Red, // Устанавливаем цвет контура маркера
                        MarkerStrokeThickness = 1, // Устанавливаем толщину контура маркера
                        TrackerFormatString = "{Tag}", // Устанавливаем формат отображения трекера
                        Title = "Точка max F(x)= " + maxPoint.resultPoint
                    };
                    // Добавляем данные в контекст точки
                    scatterSeries.ItemsSource.Cast<ScatterPoint>().ToList()[0].Tag = "F:" + listPointFunc[i].resultPoint;
                    // Добавляем ScatterSeries на график
                    plotModel.Series.Add(scatterSeries);
                }
                else
                {
                    //// Создаем ScatterSeries для рисования точки
                    //var scatterSeries = new ScatterSeries
                    //{
                    //    ItemsSource = new[] { new ScatterPoint(listPointFunc[i].x, listPointFunc[i].y) }, // Устанавливаем координаты точки
                    //    MarkerType = MarkerType.Circle, // Устанавливаем тип маркера (точки)
                    //    MarkerSize = 5, // Устанавливаем размер маркера
                    //    MarkerFill = OxyColors.Blue, // Устанавливаем цвет заливки маркера
                    //    MarkerStroke = OxyColors.Blue, // Устанавливаем цвет контура маркера
                    //    MarkerStrokeThickness = 1, // Устанавливаем толщину контура маркера
                    //    TrackerFormatString = "{Tag}", // Устанавливаем формат отображения трекера
                    //};
                    //// Добавляем данные в контекст точки
                    //scatterSeries.ItemsSource.Cast<ScatterPoint>().ToList()[0].Tag = "F:" + listPointFunc[i].resultPoint;
                    //// Добавляем ScatterSeries на график
                    //plotModel.Series.Add(scatterSeries);
                }
            }
            return plotModel;
        }

        private (double,double) SearchLineMaxYandX(List<(double y, double x, bool up, bool left)> listPointL)
        { 
            List<(double y, double x, bool up)> listPointH = mvvm.getArrayHorizintal();
            List<(double y, double x, bool LeftRight)> listPointV = mvvm.getArrayVertical();

            (double, double) maxNew1 = getXY(listPointL);
            (double, double) maxNew2 = getXY(listPointH);
            (double, double) maxYX = getMax(maxNew1, maxNew2);

            maxNew1 = getXY(listPointV);
            maxYX = getMax(maxYX, maxNew1);

            return maxYX;
        }
        private (double,double) getXY(List<(double y, double x, bool up, bool left)> listPointL)
        {
            double maxX = 0,maxY = 0;
            for (int i = 0; i < listPointL.Count(); ++i)
            {
                if (listPointL[i].x > maxX)
                {
                    maxX = listPointL[i].x;
                }
                if (listPointL[i].y > maxY)
                {
                    maxY = listPointL[i].y;
                }
            }
            return (maxX,maxY);
        }
        private (double, double) getXY(List<(double y, double x, bool up)> listPointL)
        {
            double maxX = 0, maxY = 0;
            for (int i = 0; i < listPointL.Count(); ++i)
            {
                if (listPointL[i].x > maxX)
                {
                    maxX = listPointL[i].x;
                }
                if (listPointL[i].y > maxY)
                {
                    maxY = listPointL[i].y;
                }
            }
            return (maxX, maxY);
        }
        private (double, double) getMax((double,double) first, (double,double) second)
        {
            (double, double) max = second;
            if (first.Item1 > second.Item1)
            {
                max.Item1 = first.Item1;
            }
            if (first.Item2 > second.Item2)
            {
                max.Item2 = first.Item2;
            }
            return max;
        }
    }

    public static class VisualTreeHelperExtensions
    {
        public static T ParentOfType<T>(this DependencyObject element) where T : DependencyObject
        {
            Type targetType = typeof(T);
            DependencyObject parent = VisualTreeHelper.GetParent(element);

            while (parent != null && parent.GetType() != targetType)
            {
                parent = VisualTreeHelper.GetParent(parent);
            }

            return parent as T;
        }
    }
}
