using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Effects;
using System.Windows.Media.Media3D;
using Microsoft.Win32;
using System.IO;

//using CommunityToolkit.Mvvm.ComponentModel;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.Painting.Effects;
using LiveChartsCore.SkiaSharpView.VisualElements;
using SkiaSharp;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using System.Globalization;
using System.Windows.Data;
using LiveChartsCore.Geo;

namespace WpfApp1
{
    public class ApplicationViewModel : INotifyPropertyChanged
    {
        public GAUS resGaus = new GAUS();
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

        public ObservableCollection<ISeries> Series { get; set; }
        public LabelVisual Title { get; set; } =
            new LabelVisual
            {
                Text = "My chart title",
                TextSize = 25,
                Padding = new LiveChartsCore.Drawing.Padding(15),
                Paint = new SolidColorPaint(SKColors.DarkSlateGray)
            };

        public ApplicationViewModel()
        {
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
