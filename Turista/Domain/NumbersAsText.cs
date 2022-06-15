using System;

namespace Domain
{
    public class NumbersAsText
    {
        public static string NumberToWords(int number)
        {
            if (number == 0)
                return "صفر";

            if (number < 0)
                return "- " + NumberToWords(Math.Abs(number));

            string words = "";

            if ((number / 1000000) > 0)
            {
                words += NumberToWords(number / 1000000) + " مليون ";
                number %= 1000000;
            }

            if ((number / 1000) > 0)
            {
                words += NumberToWords(number / 1000) + " ألف ";
                number %= 1000;
            }

            if ((number / 100) > 0)
            {
                words += NumberToWords(number / 100) + " مائة ";
                number %= 100;
            }

            if (number > 0)
            {
                if (words != "")
                    words += "و ";

                var unitsMap = new[] { "صفر" , "واحد" , "اثنان" , "ثلاثة" , "أربعة" , "خمسة" , "ستة" , "سبعة" , "ثمانية" , "تسعة" , "عشرة" , "أحد عشر" , " اثني عشر "," ثلاثة عشر "," أربعة عشر "," خمسة عشر "," ستة عشر "," سبعة عشر "," ثمانية عشر "," تسعة عشر " };
                var tensMap = new[] { "صفر" , "عشرة" , "عشرون" , "ثلاثون" , "أربعون" , "خمسون" , "ستون" , "سبعون" , "ثمانون" , "تسعون" };

                if (number < 20)
                    words += unitsMap[number];
                else
                {
                    words += tensMap[number / 10];
                    if ((number % 10) > 0)
                        words += "-" + unitsMap[number % 10];
                }
            }

            return words;
        }
        public static String ConvertToWords(String numb)
        {
            String val = "", wholeNo = numb, points = "", andStr = "", pointStr = "";
            String endStr = "فقط";
            try
            {
                int decimalPlace = numb.IndexOf(".");
                if (decimalPlace > 0)
                {
                    wholeNo = numb.Substring(0, decimalPlace);
                    points = numb.Substring(decimalPlace + 1);
                    if (Convert.ToInt32(points) > 0)
                    {
                        andStr = "و";// just to separate whole numbers from points/cents  
                        endStr = "Paisa " + endStr;//Cents  
                        pointStr = ConvertDecimals(points);
                    }
                }
                val = String.Format("{0} {1}{2} {3}", ConvertWholeNumber(wholeNo).Trim(), andStr, pointStr, endStr);
            }
            catch { }
            return val;
        }
        private static String ConvertWholeNumber(String Number)
        {
            string word = "";
            try
            {
                bool beginsZero = false;//tests for 0XX
                bool isDone = false;//test if already translated
                double dblAmt = (Convert.ToDouble(Number));
                //if ((dblAmt > 0) && number.StartsWith("0"))
                if (dblAmt > 0)
                {//test for zero or digit zero in a nuemric
                    beginsZero = Number.StartsWith("0");

                    int numDigits = Number.Length;
                    int pos = 0;//store digit grouping
                    String place = "";//digit grouping name:hundres,thousand,etc...
                    switch (numDigits)
                    {
                        case 1://ones' range

                            word = ones(Number);
                            isDone = true;
                            break;
                        case 2://tens' range
                            word = tens(Number);
                            isDone = true;
                            break;
                        case 3://hundreds' range
                            pos = (numDigits % 3) + 1;
                            place = " مائة ";
                            break;
                        case 4://thousands' range
                        case 5:
                        case 6:
                            pos = (numDigits % 4) + 1;
                            place = " ألف ";
                            break;
                        case 7://millions' range
                        case 8:
                        case 9:
                            pos = (numDigits % 7) + 1;
                            place = " مليون ";
                            break;
                        case 10://Billions's range
                        case 11:
                        case 12:

                            pos = (numDigits % 10) + 1;
                            place = " بليون ";
                            break;
                        //add extra case options for anything above Billion...
                        default:
                            isDone = true;
                            break;
                    }
                    if (!isDone)
                    {//if transalation is not done, continue...(Recursion comes in now!!)
                        if (Number.Substring(0, pos) != "0" && Number.Substring(pos) != "0")
                        {
                            try
                            {
                                word = ConvertWholeNumber(Number.Substring(0, pos)) + place + ConvertWholeNumber(Number.Substring(pos));
                            }
                            catch { }
                        }
                        else
                        {
                            word = ConvertWholeNumber(Number.Substring(0, pos)) + ConvertWholeNumber(Number.Substring(pos));
                        }

                        //check for trailing zeros
                        //if (beginsZero) word = " and " + word.Trim();
                    }
                    //ignore digit grouping names
                    if (word.Trim().Equals(place.Trim())) word = "";
                }
            }
            catch { }
            return word.Trim();
        }

        private static String tens(String Number)
        {
            int _Number = Convert.ToInt32(Number);
            String name = null;
            switch (_Number)
            {
                case 10:
                    name = "عشرة";
                    break;
                case 11:
                    name = "أحد عشر";
                    break;
                case 12:
                    name = "أثنا عشر";
                    break;
                case 13:
                    name = "ثلاث عشر";
                    break;
                case 14:
                    name = "أربع عشر";
                    break;
                case 15:
                    name = "خمس عشر";
                    break;
                case 16:
                    name = "ست عشر";
                    break;
                case 17:
                    name = "سبع عشر";
                    break;
                case 18:
                    name = "ثمان عشر";
                    break;
                case 19:
                    name = "تسع عشر";
                    break;
                case 20:
                    name = "عشرون";
                    break;
                case 30:
                    name = "ثلاثون";
                    break;
                case 40:
                    name = "أربعون";
                    break;
                case 50:
                    name = "خمسون";
                    break;
                case 60:
                    name = "ستون";
                    break;
                case 70:
                    name = "سبعون";
                    break;
                case 80:
                    name = "ثمانون";
                    break;
                case 90:
                    name = "تسعون";
                    break;
                default:
                    if (_Number > 0)
                    {
                        name = tens(Number.Substring(0, 1) + "0") + " " + ones(Number.Substring(1));
                    }
                    break;
            }
            return name;
        }
        private static String ones(String Number)
        {
            int _Number = Convert.ToInt32(Number);
            String name = "";
            switch (_Number)
            {

                case 1:
                    name = "واحد";
                    break;
                case 2:
                    name = "أثنان";
                    break;
                case 3:
                    name = "ثلاثة";
                    break;
                case 4:
                    name = "أربعة";
                    break;
                case 5:
                    name = "خمسة";
                    break;
                case 6:
                    name = "ستة";
                    break;
                case 7:
                    name = "سبعة";
                    break;
                case 8:
                    name = "ثمانية";
                    break;
                case 9:
                    name = "تسعة";
                    break;
            }
            return name;
        }

        private static String ConvertDecimals(String number)
        {
            String cd = "", digit = "", engOne = "";
            for (int i = 0; i < number.Length; i++)
            {
                digit = number[i].ToString();
                if (digit.Equals("0"))
                {
                    engOne = "صفر";
                }
                else
                {
                    engOne = ones(digit);
                }
                cd += " " + engOne;
            }
            return cd;
        }
    }
}