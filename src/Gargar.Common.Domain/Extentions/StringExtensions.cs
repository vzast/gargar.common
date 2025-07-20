using Gargar.Common.Domain.Helpers;
using System.Globalization;
using System.Text;

namespace Gargar.Common.Domain.Extentions;

public static class StringExtensions
{
    private static readonly char[] s_georgianChars = new char[33]
    {
        'ა', 'ბ', 'გ', 'დ', 'ე', 'ვ', 'ზ', 'თ', 'ი', 'კ',
        'ლ', 'მ', 'ნ', 'ო', 'პ', 'ჟ', 'რ', 'ს', 'ტ', 'უ',
        'ფ', 'ქ', 'ღ', 'ყ', 'შ', 'ჩ', 'ც', 'ძ', 'წ', 'ჭ',
        'ხ', 'ჯ', 'ჰ'
    };

    private static readonly byte[] s_asciiOtherCodes = new byte[38]
    {
        9, 10, 11, 12, 13, 32, 33, 34, 35, 36,
        37, 38, 39, 40, 41, 42, 43, 44, 45, 46,
        47, 58, 59, 60, 61, 62, 63, 64, 91, 92,
        93, 94, 95, 96, 123, 124, 125, 126
    };

    public static string AddGuid(this string str) => $"{str}_{Guid.NewGuid():N}";

    public static T ToEnum<T>(this string value, T defaultValue)
    {
        if (value.IsEmpty())
        {
            return defaultValue;
        }

        try
        {
            return (T)Enum.Parse(typeof(T), value, ignoreCase: true);
        }
        catch (ArgumentException)
        {
            return defaultValue;
        }
    }

    public static string EmptyNull(this string value)
    {
        return (value ?? string.Empty).Trim();
    }

    public static string? NullEmpty(this string value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            return value;
        }

        return null;
    }

    public static bool IsCaseSensitiveEqual(this string value, string comparing)
    {
        return string.CompareOrdinal(value, comparing) == 0;
    }

    public static bool IsCaseInsensitiveEqual(this string value, string comparing)
    {
        return string.Equals(value, comparing, StringComparison.OrdinalIgnoreCase);
    }

    public static bool IsEmpty(this string value)
    {
        return string.IsNullOrWhiteSpace(value);
    }

    public static bool IsWhiteSpace(this string value)
    {
        Guard.NotNull(value, "value");
        if (value.Length == 0)
        {
            return false;
        }

        for (int i = 0; i < value.Length; i++)
        {
            if (!char.IsWhiteSpace(value[i]))
            {
                return false;
            }
        }

        return true;
    }

    //
    // Summary:
    //     მეთოდი განკუთვნილია ქართული ტექსტების ლათინურში გადასაყვანად (მაგალითად: SMS-ისთვის)
    //
    //
    // Parameters:
    //   georgianUnicodeText:
    //     შემავალი ქართული ტექსტი უნიკოდში
    //
    // Returns:
    //     ლათინური ტექსტი
    public static string GeoUnicodeToLatin(this string georgianUnicodeText)
    {
        StringBuilder stringBuilder = new StringBuilder();
        foreach (char c in georgianUnicodeText)
        {
            switch (c)
            {
                case 'ა':
                    stringBuilder.Append('a');
                    break;

                case 'ბ':
                    stringBuilder.Append('b');
                    break;

                case 'გ':
                    stringBuilder.Append('g');
                    break;

                case 'დ':
                    stringBuilder.Append('d');
                    break;

                case 'ე':
                    stringBuilder.Append('e');
                    break;

                case 'ვ':
                    stringBuilder.Append('v');
                    break;

                case 'ზ':
                    stringBuilder.Append('z');
                    break;

                case 'თ':
                    stringBuilder.Append('t');
                    break;

                case 'ი':
                    stringBuilder.Append('i');
                    break;

                case 'კ':
                    stringBuilder.Append('k');
                    break;

                case 'ლ':
                    stringBuilder.Append('l');
                    break;

                case 'მ':
                    stringBuilder.Append('m');
                    break;

                case 'ნ':
                    stringBuilder.Append('n');
                    break;

                case 'ო':
                    stringBuilder.Append('o');
                    break;

                case 'პ':
                    stringBuilder.Append('p');
                    break;

                case 'ჟ':
                    stringBuilder.Append('j');
                    break;

                case 'რ':
                    stringBuilder.Append('r');
                    break;

                case 'ს':
                    stringBuilder.Append('s');
                    break;

                case 'ტ':
                    stringBuilder.Append('t');
                    break;

                case 'უ':
                    stringBuilder.Append('u');
                    break;

                case 'ფ':
                    stringBuilder.Append('p');
                    break;

                case 'ქ':
                    stringBuilder.Append('k');
                    break;

                case 'ღ':
                    stringBuilder.Append('g');
                    break;

                case 'ყ':
                    stringBuilder.Append('k');
                    break;

                case 'შ':
                    stringBuilder.Append("sh");
                    break;

                case 'ჩ':
                    stringBuilder.Append("ch");
                    break;

                case 'ც':
                    stringBuilder.Append("ts");
                    break;

                case 'ძ':
                    stringBuilder.Append("dz");
                    break;

                case 'წ':
                    stringBuilder.Append("ts");
                    break;

                case 'ჭ':
                    stringBuilder.Append("tch");
                    break;

                case 'ხ':
                    stringBuilder.Append("kh");
                    break;

                case 'ჯ':
                    stringBuilder.Append("dj");
                    break;

                case 'ჰ':
                    stringBuilder.Append('h');
                    break;

                default:
                    stringBuilder.Append(c);
                    break;
            }
        }

        return stringBuilder.ToString();
    }

    //
    // Summary:
    //     შემთხვევითი ტექსტის დასაგენერირებელი მეთოდი. (გამოყენება პაროლისთვის ან სატესტო
    //     შემთხვევბისთვის როდესაც გინდა შემთხვევითი ტექსტი განსაზღვრული სიგრძით)
    //
    // Parameters:
    //   length:
    //     ტექსტის სიგრძის დაშვება
    //
    //   hasLowCases:
    //     პატარა ლათინური ასოების დაშვება
    //
    //   hasUpCases:
    //     დიდი ლათინური ასოების დაშვება
    //
    //   hasNumbers:
    //     ციფრების დაშვება
    //
    //   hasGeorgianUnicodeChars:
    //     ქართული უნიკოდ ასოების დაშვება
    //
    //   hasOtherAsciiChars:
    //     სხვა დანარჩენი სიმბოლოების დაშვება
    //
    // Returns:
    //     დაგენერირებული ტექსტი
    public static string GenerateRandomString(int length, bool hasLowASCIIChars = true, bool hasUpASCIIChars = true, bool hasNumbers = true, bool hasGeorgianUnicodeChars = false, bool hasOtherAsciiChars = false)
    {
        byte b = 0;
        int num = 0;
        b = (byte)((uint)b | (hasLowASCIIChars ? 1u : 0u));
        b |= (byte)(hasUpASCIIChars ? 2u : 0u);
        b |= (byte)(hasNumbers ? 4u : 0u);
        b |= (byte)(hasGeorgianUnicodeChars ? 8u : 0u);
        b |= (byte)(hasOtherAsciiChars ? 16u : 0u);
        for (byte b2 = b; b2 != 0; b2 >>= 1)
        {
            num += b2 & 1;
        }

        if (num > length)
        {
            throw new ArgumentOutOfRangeException("length", "Length for text generation is less than symbol variety");
        }

        if (num == 0)
        {
            throw new ArgumentOutOfRangeException("varietyCount", "Please select one of the char symbol diapason selector, such as (hasLowCases, hasUpCases, hasNumbers, hasGeorgianUnicodeChars, hasOtherAsciiChars)");
        }

        Random random = new Random(new Random().Next());
        char[] array = new char[length];
        int num2 = length;
        byte b3 = b;
        while (num2-- != 0)
        {
            if (b3 == 0)
            {
                b3 = b;
            }

            if ((b3 & 1) == 1)
            {
                array[num2] = (char)random.Next(97, 122);
                b3 = (byte)(b3 ^ 1u);
            }
            else if ((b3 & 2) == 2)
            {
                array[num2] = (char)random.Next(65, 90);
                b3 = (byte)(b3 ^ 2u);
            }
            else if ((b3 & 4) == 4)
            {
                array[num2] = (char)random.Next(48, 57);
                b3 = (byte)(b3 ^ 4u);
            }
            else if ((b3 & 8) == 8)
            {
                array[num2] = s_georgianChars[random.Next(s_georgianChars.Length - 1)];
                b3 = (byte)(b3 ^ 8u);
            }
            else if ((b3 & 0x10) == 16)
            {
                array[num2] = (char)s_asciiOtherCodes[random.Next(s_asciiOtherCodes.Length - 1)];
                b3 = (byte)(b3 ^ 0x10u);
            }
        }

        return new string(array);
    }

    //
    // Summary:
    //     კონვერტაცია Byte => HexString
    //
    // Parameters:
    //   bytes:
    //     დასაკონვერტირებელი ბაიტები
    //
    // Returns:
    //     აბრუნებს HEX ტექსტს
    public static string? ToHexString(this byte[]? bytes)
    {
        if (bytes != null)
        {
            return Convert.ToHexString(bytes);
        }

        return null;
    }

    //
    // Summary:
    //     კონვერტაცია Byte => HexString
    //
    // Parameters:
    //   bytes:
    //     დასაკონვერტირებელი ბაიტები
    //
    // Returns:
    //     აბრუნებს HEX ტექსტს
    public static string ToHexString(this ReadOnlySpan<byte> bytes)
    {
        if (bytes.IsEmpty)
        {
            return string.Empty;
        }

        if (bytes.Length > 1073741823)
        {
            throw new ArgumentOutOfRangeException("bytes", "Input is too large to be processed.");
        }

        return Convert.ToHexString(bytes);
    }

    //
    // Summary:
    //     კონვერტაცია HexString-დან Byte-ში
    //
    // Parameters:
    //   hexString:
    //     ტექსტი Hex ფორმატში
    //
    // Returns:
    //     აბრუნებს ბაიტებს
    public static byte[]? ToByteArray(this string? hexString)
    {
        if (hexString != null)
        {
            return Convert.FromHexString(hexString);
        }

        return null;
    }

    //
    // Summary:
    //     რიცხვის სახელის დასაგენერირებელი ფუნქცია
    //
    // Parameters:
    //   number:
    //     რიცხვი რისი სახელიც გვაინტერესებს
    //
    //   separator:
    //     სახელში გამყოფი ობიექტი, (მაგ: თუ separator = "_" მაშინ რიცხვის 2021 სახელი იქნება
    //     "ორი_ათას_ოცდა_ორი" )
    //
    // Returns:
    //     მიწოდებული რიცხვის სახელი
    public static string GetNumberName(long number, string separator = "")
    {
        if (number != 0L)
        {
            return GetNineteenSignDigitName(number.ToString(CultureInfo.InvariantCulture), separator);
        }

        return "ნული";
    }

    private static string GetOneSignDigitName(string digit)
    {
        if (digit.Length == 1)
        {
            switch (digit[0])
            {
                case '0':
                    return "ი";

                case '1':
                    return "ერთი";

                case '2':
                    return "ორი";

                case '3':
                    return "სამი";

                case '4':
                    return "ოთხი";

                case '5':
                    return "ხუთი";

                case '6':
                    return "ექვსი";

                case '7':
                    return "შვიდი";

                case '8':
                    return "რვა";

                case '9':
                    return "ცხრა";
            }
        }

        return string.Empty;
    }

    private static string GetToNineteenName(string number)
    {
        if (number.Length == 2)
        {
            switch (number)
            {
                case "10":
                    return "ათი";

                case "11":
                    return "თერთმეტი";

                case "12":
                    return "თორმეტი";

                case "13":
                    return "ცამეტი";

                case "14":
                    return "თოთხმეტი";

                case "15":
                    return "თხუთმეტი";

                case "16":
                    return "თექვსმეტი";

                case "17":
                    return "ჩვიდმეტი";

                case "18":
                    return "თვრამეტი";

                case "19":
                    return "ცხრამეტი";
            }
        }
        else if (number.Length < 2)
        {
            return GetOneSignDigitName(number);
        }

        return string.Empty;
    }

    private static string GetTwoSignDigitName(string number)
    {
        if (number.Length == 2)
        {
            long num = Convert.ToInt64(number, CultureInfo.InvariantCulture);
            switch (number[0])
            {
                case '0':
                    return GetOneSignDigitName(number[1].ToString());

                case '1':
                    return GetToNineteenName(number);

                case '2':
                case '3':
                    if (!(number == "20"))
                    {
                        return "ოცდა" + GetToNineteenName((num % 20).ToString(CultureInfo.InvariantCulture));
                    }

                    return "ოცი";

                case '4':
                case '5':
                    if (!(number == "40"))
                    {
                        return "ორმოცდა" + GetToNineteenName((num % 20).ToString(CultureInfo.InvariantCulture));
                    }

                    return "ორმოცი";

                case '6':
                case '7':
                    if (!(number == "60"))
                    {
                        return "სამოცდა" + GetToNineteenName((num % 20).ToString(CultureInfo.InvariantCulture));
                    }

                    return "სამოცი";

                case '8':
                case '9':
                    if (!(number == "80"))
                    {
                        return "ოთხმოცდა" + GetToNineteenName((num % 20).ToString(CultureInfo.InvariantCulture));
                    }

                    return "ოთხმოცი";
            }
        }
        else if (number.Length < 2)
        {
            return GetOneSignDigitName(number);
        }

        return string.Empty;
    }

    private static string GetThreeSignDigitName(string number, string separator)
    {
        if (number.Length == 3)
        {
            string twoSignDigitName = GetTwoSignDigitName(Convert.ToString(Convert.ToInt64(number, CultureInfo.InvariantCulture) % 100, CultureInfo.InvariantCulture));
            switch (number[0])
            {
                case '0':
                    return GetTwoSignDigitName($"{number[1]}{number[2]}");

                case '1':
                    return "ას" + ((twoSignDigitName == "ი") ? twoSignDigitName : (separator + twoSignDigitName));

                case '2':
                    return "ორას" + ((twoSignDigitName == "ი") ? twoSignDigitName : (separator + twoSignDigitName));

                case '3':
                    return "სამას" + ((twoSignDigitName == "ი") ? twoSignDigitName : (separator + twoSignDigitName));

                case '4':
                    return "ოთხას" + ((twoSignDigitName == "ი") ? twoSignDigitName : (separator + twoSignDigitName));

                case '5':
                    return "ხუთას" + ((twoSignDigitName == "ი") ? twoSignDigitName : (separator + twoSignDigitName));

                case '6':
                    return "ექვსას" + ((twoSignDigitName == "ი") ? twoSignDigitName : (separator + twoSignDigitName));

                case '7':
                    return "შვიდას" + ((twoSignDigitName == "ი") ? twoSignDigitName : (separator + twoSignDigitName));

                case '8':
                    return "რვაას" + ((twoSignDigitName == "ი") ? twoSignDigitName : (separator + twoSignDigitName));

                case '9':
                    return "ცხრაას" + ((twoSignDigitName == "ი") ? twoSignDigitName : (separator + twoSignDigitName));
            }
        }
        else if (number.Length < 3)
        {
            return GetTwoSignDigitName(number);
        }

        return string.Empty;
    }

    private static string GetFourSignDigitName(string number, string separator)
    {
        if (number.Length == 4)
        {
            string number2 = number.Substring(1);
            if (number[0] == '0')
            {
                return GetThreeSignDigitName(number2, separator);
            }

            return $"{GetThreeSignDigitName(number[0].ToString(), separator)}{separator}ათას{separator}{GetThreeSignDigitName(number2, separator)}";
        }

        if (number.Length < 4)
        {
            return GetThreeSignDigitName(number, separator);
        }

        return string.Empty;
    }

    private static string GetFiveSignDigitName(string number, string separator)
    {
        if (number.Length == 5)
        {
            if (number[0] == '0')
            {
                return GetFourSignDigitName(number.Substring(1), separator);
            }

            return $"{GetThreeSignDigitName(number.Substring(0, 2), separator)}{separator}ათას{separator}{GetThreeSignDigitName(number.Substring(2), separator)}";
        }

        if (number.Length < 5)
        {
            return GetFourSignDigitName(number, separator);
        }

        return string.Empty;
    }

    private static string GetSixSignDigitName(string number, string separator)
    {
        if (number.Length == 6)
        {
            if (number[0] == '0')
            {
                return GetFiveSignDigitName(number.Substring(1), separator);
            }

            return $"{GetThreeSignDigitName(number.Substring(0, 3), separator)}{separator}ათას{separator}{GetThreeSignDigitName(number.Substring(3), separator)}";
        }

        if (number.Length < 6)
        {
            return GetFiveSignDigitName(number, separator);
        }

        return string.Empty;
    }

    private static string GetSevenSignDigitName(string number, string separator)
    {
        if (number.Length == 7)
        {
            string number2 = number.Substring(1);
            if (number[0] == '0')
            {
                return GetSixSignDigitName(number2, separator);
            }

            return $"{GetSixSignDigitName(number.Substring(0, 1), separator)}{separator}მილიონ{separator}{GetSixSignDigitName(number2, separator)}";
        }

        if (number.Length < 7)
        {
            return GetSixSignDigitName(number, separator);
        }

        return string.Empty;
    }

    private static string GetEightSignDigitName(string number, string separator)
    {
        if (number.Length == 8)
        {
            if (number[0] == '0')
            {
                return GetSevenSignDigitName(number.Substring(1), separator);
            }

            return $"{GetSixSignDigitName(number.Substring(0, 2), separator)}{separator}მილიონ{separator}{GetSixSignDigitName(number.Substring(2), separator)}";
        }

        if (number.Length < 8)
        {
            return GetSevenSignDigitName(number, separator);
        }

        return string.Empty;
    }

    private static string GetNineSignDigitName(string number, string separator)
    {
        if (number.Length == 9)
        {
            if (number[0] == '0')
            {
                return GetEightSignDigitName(number.Substring(1), separator);
            }

            return $"{GetSixSignDigitName(number.Substring(0, 3), separator)}{separator}მილიონ{separator}{GetSixSignDigitName(number.Substring(3), separator)}";
        }

        if (number.Length < 9)
        {
            return GetEightSignDigitName(number, separator);
        }

        return string.Empty;
    }

    private static string GetTenSignDigitName(string number, string separator)
    {
        if (number.Length == 10)
        {
            string number2 = number.Substring(1);
            if (number[0] == '0')
            {
                return GetNineSignDigitName(number2, separator);
            }

            return $"{GetNineSignDigitName(number.Substring(0, 1), separator)}{separator}მილიარდ{separator}{GetNineSignDigitName(number2, separator)}";
        }

        if (number.Length < 10)
        {
            return GetNineSignDigitName(number, separator);
        }

        return string.Empty;
    }

    private static string GetElevenSignDigitName(string number, string separator)
    {
        if (number.Length == 11)
        {
            if (number[0] == '0')
            {
                return GetTenSignDigitName(number.Substring(1), separator);
            }

            return $"{GetNineSignDigitName(number.Substring(0, 2), separator)}{separator}მილიარდ{separator}{GetNineSignDigitName(number.Substring(2), separator)}";
        }

        if (number.Length < 11)
        {
            return GetTenSignDigitName(number, separator);
        }

        return string.Empty;
    }

    private static string GetTwelveSignDigitName(string number, string separator)
    {
        if (number.Length == 12)
        {
            if (number[0] == '0')
            {
                return GetElevenSignDigitName(number.Substring(1), separator);
            }

            return $"{GetNineSignDigitName(number.Substring(0, 3), separator)}{separator}მილიარდ{separator}{GetNineSignDigitName(number.Substring(3), separator)}";
        }

        if (number.Length < 12)
        {
            return GetElevenSignDigitName(number, separator);
        }

        return string.Empty;
    }

    private static string GetThirteenSignDigitName(string number, string separator)
    {
        if (number.Length == 13)
        {
            string number2 = number.Substring(1);
            if (number[0] == '0')
            {
                return GetTwelveSignDigitName(number2, separator);
            }

            return $"{GetTwelveSignDigitName(number.Substring(0, 1), separator)}{separator}ტრილიონ{separator}{GetTwelveSignDigitName(number2, separator)}";
        }

        if (number.Length < 13)
        {
            return GetTwelveSignDigitName(number, separator);
        }

        return string.Empty;
    }

    private static string GetFourteenSignDigitName(string number, string separator)
    {
        if (number.Length == 14)
        {
            if (number[0] == '0')
            {
                return GetThirteenSignDigitName(number.Substring(1), separator);
            }

            return $"{GetTwelveSignDigitName(number.Substring(0, 2), separator)}{separator}ტრილიონ{separator}{GetTwelveSignDigitName(number.Substring(2), separator)}";
        }

        if (number.Length < 14)
        {
            return GetThirteenSignDigitName(number, separator);
        }

        return string.Empty;
    }

    private static string GetFifteenSignDigitName(string number, string separator)
    {
        if (number.Length == 15)
        {
            if (number[0] == '0')
            {
                return GetFourteenSignDigitName(number.Substring(1), separator);
            }

            return $"{GetTwelveSignDigitName(number.Substring(0, 3), separator)}{separator}ტრილიონ{separator}{GetTwelveSignDigitName(number.Substring(3), separator)}";
        }

        if (number.Length < 15)
        {
            return GetFourteenSignDigitName(number, separator);
        }

        return string.Empty;
    }

    private static string GetSixteenSignDigitName(string number, string separator)
    {
        if (number.Length == 16)
        {
            string number2 = number.Substring(1);
            if (number[0] == '0')
            {
                return GetFifteenSignDigitName(number2, separator);
            }

            return $"{GetFifteenSignDigitName(number.Substring(0, 1), separator)}{separator}კვადრილიონ{separator}{GetFifteenSignDigitName(number2, separator)}";
        }

        if (number.Length < 16)
        {
            return GetFifteenSignDigitName(number, separator);
        }

        return string.Empty;
    }

    private static string GetSeventeenSignDigitName(string number, string separator)
    {
        if (number.Length == 17)
        {
            if (number[0] == '0')
            {
                return GetSixteenSignDigitName(number.Substring(1), separator);
            }

            return $"{GetFifteenSignDigitName(number.Substring(0, 2), separator)}{separator}კვადრილიონ{separator}{GetFifteenSignDigitName(number.Substring(2), separator)}";
        }

        if (number.Length < 17)
        {
            return GetSixteenSignDigitName(number, separator);
        }

        return string.Empty;
    }

    private static string GetEighteenSignDigitName(string number, string separator)
    {
        if (number.Length == 18)
        {
            if (number[0] == '0')
            {
                return GetSeventeenSignDigitName(number.Substring(1), separator);
            }

            return $"{GetFifteenSignDigitName(number.Substring(0, 3), separator)}{separator}კვადრილიონ{separator}{GetFifteenSignDigitName(number.Substring(3), separator)}";
        }

        if (number.Length < 18)
        {
            return GetSeventeenSignDigitName(number, separator);
        }

        return string.Empty;
    }

    private static string GetNineteenSignDigitName(string number, string separator)
    {
        if (number.Length == 19)
        {
            string number2 = number.Substring(1);
            if (number[0] == '0')
            {
                return GetEighteenSignDigitName(number2, separator);
            }

            return $"{GetEighteenSignDigitName(number.Substring(0, 1), separator)}{separator}კვინტილიონ{separator}{GetEighteenSignDigitName(number2, separator)}";
        }

        if (number.Length < 19)
        {
            return GetEighteenSignDigitName(number, separator);
        }

        return string.Empty;
    }
}