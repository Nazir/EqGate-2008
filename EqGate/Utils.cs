using System;
using System.IO;
using System.Text;

namespace Utils
{
    public partial class Util
    {
        public static bool SaveTextToFile(string Text, string ToFile, bool Append)
        {

            if ((!Append) && (File.Exists(ToFile)))
            {
                File.Delete(ToFile);
            }

            using (StreamWriter sw = new StreamWriter(ToFile, Append, Encoding.GetEncoding(1251)))
            {
                sw.Write(Text);
            }
            return true;
        }

        public static string ConvertDateToFormat(string Date, string Format)
        {
            if (Format == String.Empty)
                Format = "dd.MM.yyyy";

            DateTime dt = new DateTime();
            if ((Date == "") || (Date == "00000000") || (Date == "        "))
                dt = new DateTime(1900, 01, 01);
            else
                dt = new DateTime(Convert.ToInt32(Date.Substring(0, 4)),
                    Convert.ToInt32(Date.Substring(4, 2)),
                    Convert.ToInt32(Date.Substring(6, 2)));
            return dt.ToString(Format);
        }

        public static string ConvertToCurrency(string Value, string type, string Size)
        {
            int Numerator = 0; // числитель
            int Denominator = 0; // знаменатель
            int MinusSign = 1; // знак минус
            int DenominatorSize = 0; // количество десятичных знаков

            string CurrencySeparator = "."; // разделитель

            if (type == "U")
                Numerator = Convert.ToInt32(Value.Substring(0, Value.Length - DenominatorSize));
            if (type == "S")
            {
                Numerator = Convert.ToInt32(Value.Substring(1, Value.Length - DenominatorSize));
                if (Value.Substring(0, 1) == "-")
                    MinusSign = -1;
            }
            Numerator = MinusSign * Numerator;
            if ((type == "U") || (type == "S"))
                DenominatorSize = Convert.ToInt32(Size.Substring(Size.IndexOf(",") + 1));
            if (DenominatorSize != 0)
                if ((type == "U") || (type == "S"))
                    Denominator = Convert.ToInt32(Value.Substring(Value.Length - DenominatorSize, DenominatorSize));

            return Numerator.ToString() + CurrencySeparator + Denominator.ToString();
        }

        public static string InsertSpaces(string Value, int Size)
        {
            // Заполение пробелами
            string Result = Value;
            while (Result.Length < Size)
            {
                Result += " ";
            }
//            if (Result.Length != Size)
  //              MessageBox.Show("Error: InsertSpaces");

            return Result;
        }

        public static string InsertZero(string Value, int Size)
        {
            // Заполение пробелами
            string Result = Value;
            while (Result.Length < Size)
            {
                Result = "0" + Result;
            }
       //     if (Result.Length != Size)
         //       MessageBox.Show("Error: InsertSpaces");

            return Result;
        }

    }
}
