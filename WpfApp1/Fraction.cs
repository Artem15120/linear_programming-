using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace WpfApp1
{
    public class Fraction
    {
        public string CharValue { get; set; } // пустая строка
        public int Numerator { get; private set; } // Числитель
        public int Denominator { get; private set; } // Знаменатель
        public double Decimal { get; private set; } // Десятичное
        public static readonly Fraction Zero = new Fraction(0, 0);
        public bool Type { get; set; }
        public static string _outputFormat = "Дробь"; // по умолчанию вывод в виде дроби
                                                         // Добавляем конструктор по умолчанию
        [JsonConstructor]
        public Fraction(string charValue, int numerator, int denominator)
        {
            if (denominator == 0)
            {
                if (numerator != 0) { }
                    //throw new ArgumentException("Знаменатель не может быть равен нулю.");
                else
                {
                    Numerator = 0;
                    Denominator = 0;
                    Decimal = 0;
                }
            }
            else if (numerator == 0)
            {
                Numerator = 0;
                Denominator = 0;
                Decimal = 0;
            }
            else
            {
                int gcd = GCD(numerator, denominator);
                Numerator = numerator / gcd;
                Denominator = denominator / gcd;
                Decimal = (double)numerator / denominator;

                if (Decimal < 0)
                {
                    Numerator = Math.Abs(Numerator) * -1;
                    Denominator = Math.Abs(Denominator);
                }
                else if (Decimal > 0)
                {
                    Numerator = Math.Abs(Numerator);
                    Denominator = Math.Abs(Denominator);
                }
            }
            this.Type = false;
            CharValue = charValue;
        }
        public Fraction(double value)
        {
            // Преобразуем double в дробь с заданной точностью 
            const long precision = 10000L;
            int numerator = (int)(value * precision);
            int denominator = (int)precision;

            // Сокращаем дробь до наименьшего вида.
            int greatestCommonDivisor = GCD((int)numerator, (int)denominator);
            int reducedNumerator = numerator / greatestCommonDivisor;
            int reducedDenominator = denominator / greatestCommonDivisor;
            Numerator = reducedNumerator;
            Denominator = reducedDenominator;
            Decimal = value;
            Fraction tmp = new Fraction(Numerator, Denominator);
            Numerator = tmp.Numerator; Denominator = tmp.Denominator; Decimal = tmp.Decimal;

            this.Type = false;
            CharValue = null;
        }
        public Fraction(int numerator, int denominator = 1)
        {
            if (denominator == 0)
            {
                if (numerator != 0)
                    throw new ArgumentException("Знаменатель не может быть равен нулю.");
                else{Numerator = 0; Denominator = 0; Decimal = 0;}
            }
            else if (numerator == 0){Numerator = 0; Denominator = 0; Decimal = 0;}
            else
            {
                int gcd = GCD(numerator, denominator);
                Numerator = numerator / gcd;
                Denominator = denominator / gcd;
                Decimal = (double)numerator / denominator;

                if (Decimal < 0)
                {
                    Numerator = Math.Abs(Numerator) * -1;
                    Denominator = Math.Abs(Denominator);
                }
                else if (Decimal > 0)
                {
                    Numerator = Math.Abs(Numerator);
                    Denominator = Math.Abs(Denominator);
                }
            }
            this.Type = false;
            CharValue = null;
        }
        public Fraction(string charValue = null)
        {
            this.Type = false;
            CharValue = charValue;
        }
        public Fraction(Fraction tmp)
        {
            if (tmp is null)
                return;
            if(tmp.CharValue == null)
            {
                this.CharValue = null;
            }
            else
            {
                this.CharValue = tmp.CharValue;

            }
            if(tmp.Numerator != null)
            {
                this.Numerator = tmp.Numerator;
                this.Denominator = tmp.Denominator;
                this.Decimal = tmp.Decimal;
            }

            this.Type = tmp.Type;
        }
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;
            Fraction other = (Fraction)obj;
            return Numerator == other.Numerator && Denominator == other.Denominator;
        }

        public static Fraction operator +(Fraction f1, Fraction f2)
        {
            if (f1.Denominator == 0) return f2;
            if (f2.Denominator == 0) return f1;
            return new Fraction(f1.Numerator * f2.Denominator + f2.Numerator * f1.Denominator, f1.Denominator * f2.Denominator);
        }

        public static Fraction operator -(Fraction f1, Fraction f2)
        {
            if (f1.Denominator == 0)
            {
                f2.Numerator *= -1;
                return f2; 
            }
            if (f2.Denominator == 0) return f1;
            return new Fraction(f1.Numerator * f2.Denominator - f2.Numerator * f1.Denominator, f1.Denominator * f2.Denominator);
        }

        public static Fraction operator *(Fraction f1, Fraction f2)
        {
            if (f1.Denominator == 0 || f2.Denominator == 0) return new Fraction(0, 0);
            return new Fraction(f1.Numerator * f2.Numerator, f1.Denominator * f2.Denominator);
        }

        public static Fraction operator /(Fraction f1, Fraction f2)
        {
            if (f1.Denominator == 0 || f2.Numerator == 0) return new Fraction(0, 0);
            if (f2.Denominator == 0) return new Fraction(0, 0);
            return new Fraction(f1.Numerator * f2.Denominator, f1.Denominator * f2.Numerator);
        }
        // Оператор >
        public static bool operator >(Fraction tmpF1, Fraction tmpF2)
        {
            Fraction f1 = new Fraction(tmpF1.Numerator, tmpF1.Denominator);
            Fraction f2 = new Fraction(tmpF2.Numerator, tmpF2.Denominator);
            if (f1.Numerator == 0)
            {
                f1.Denominator = 1;
            }
            if (f2.Numerator == 0)
            {
                f2.Denominator = 1;
            }

            return f1.Numerator * f2.Denominator > f2.Numerator * f1.Denominator;
        }
        // Оператор <
        public static bool operator <(Fraction tmpF1, Fraction tmpF2)
        {
            Fraction f1 = new Fraction(tmpF1.Numerator, tmpF1.Denominator);
            Fraction f2 = new Fraction(tmpF2.Numerator, tmpF2.Denominator);
            if (f1.Numerator == 0)
            {
                f1.Denominator = 1;
            }
            if (f2.Numerator == 0)
            {
                f2.Denominator = 1;
            }
            return f1.Numerator * f2.Denominator < f2.Numerator * f1.Denominator;
        }

        public static void ChangeOutputFormat()
        {
            if (_outputFormat == "Дробь")
            {
                _outputFormat = "Десятичное";
            }
            else
            {
                _outputFormat = "Дробь";
            }
        }
        public override string ToString()
        {
            if (!(CharValue is null))
                return $"{CharValue}";
            if (_outputFormat == "Десятичное")
            {
                return $"{Decimal}";
            }
            else if (Denominator == 1 || Denominator == 0)
                return $"{Numerator}";
            else
                return $"{Numerator}/{Denominator}";
        }
        public static bool TryParse(string fractionString, out Fraction result)
        {
            result = null;
            if (string.IsNullOrWhiteSpace(fractionString))
                return false;

            string[] parts = fractionString.Split('/');
            if (parts.Length != 2)
            {
                if (parts.Length == 1)
                {
                    if(fractionString.ToLower() == "min" || fractionString.ToLower() == "max" )
                    {
                        result = new Fraction(fractionString);
                        return true;
                    }else if (int.TryParse(parts[0], out int Num))
                    {
                        try
                        {
                            result = new Fraction(Num);
                            return true;
                        }
                        catch (ArgumentException)
                        {
                            return false;
                        }
                    }

                    string[] parts2 = parts[0].Split('.');

                    if (parts2.Length == 2)
                    {
                        int integerPart;
                        bool integerPartParsed = int.TryParse(parts2[0], out integerPart);

                        int fractionalPart;
                        bool fractionalPartParsed = int.TryParse(parts2[1], out fractionalPart);

                        if (integerPartParsed && fractionalPartParsed)
                        {
                            double NumDouble = integerPart + (double)fractionalPart / Math.Pow(10, parts2[1].Length);
                            result = new Fraction(NumDouble);
                            return true;
                        }
                        return false;
                    }
                    else 
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }

            if (!int.TryParse(parts[0], out int Numerator) || !int.TryParse(parts[1], out int Denominator))
                return false;
            try{
                result = new Fraction(Numerator, Denominator);
                return true;
            }catch (ArgumentException) {
                return false;
            }
        }
        private int GCD(int a, int b) {
            while (b != 0){
                int temp = b;
                b = a % b;
                a = temp;
            }
            return Math.Abs(a);
        }
    }
}
