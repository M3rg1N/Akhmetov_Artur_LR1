using System.Text;

namespace BanID.Services;

public static class SecurityService
{
    private const int HashLength = 64;
    private const int HashRounds = 1000;
    private const string LoginSalt = "BanID_LOGIN_SALT";
    private const string EncryptionKey = "BanID_PERSONAL_DATA_KEY";

    public static char[] Symbols = CreateSymbols();

    public static string GenerateSalt()
    {
        Random random = new Random();
        int length = random.Next(5, 8);
        StringBuilder builder = new StringBuilder(length);

        for (int i = 0; i < length; i++)
        {
            builder.Append(Symbols[random.Next(Symbols.Length)]);
        }

        return builder.ToString();
    }

    public static string HashPassword(string password, string salt)
    {
        return CustomHash(password, salt);
    }

    public static string HashLogin(string login)
    {
        return CustomHash(NormalizeLogin(login), LoginSalt);
    }

    public static string NormalizePhone(string phone)
    {
        StringBuilder builder = new StringBuilder();

        foreach (char ch in phone)
        {
            if (char.IsDigit(ch))
            {
                builder.Append(ch);
            }
        }

        string digits = builder.ToString();

        if (digits.Length == 10 && digits.StartsWith("9"))
        {
            return "7" + digits;
        }

        if (digits.Length == 11 && digits.StartsWith("8"))
        {
            return "7" + digits.Substring(1);
        }

        return digits;
    }

    public static bool IsValidPassword(string password, out string error)
    {
        if (password.Length < 8)
        {
            error = "Пароль должен быть не короче 8 символов.";
            return false;
        }

        if (!password.Any(char.IsUpper))
        {
            error = "В пароле нужна хотя бы одна большая буква.";
            return false;
        }

        if (!password.Any(char.IsLower))
        {
            error = "В пароле нужна хотя бы одна маленькая буква.";
            return false;
        }

        if (!password.Any(char.IsDigit))
        {
            error = "В пароле нужна хотя бы одна цифра.";
            return false;
        }

        if (!password.Any(IsSpecialSymbol))
        {
            error = "В пароле нужен один спецсимвол: + - = , * / .";
            return false;
        }

        error = "";
        return true;
    }

    public static string EncryptPersonalData(string value, string salt)
    {
        if (string.IsNullOrEmpty(value))
        {
            return "";
        }

        List<string> numbers = new List<string>();

        for (int i = 0; i < value.Length; i++)
        {
            int keyCode = EncryptionKey[i % EncryptionKey.Length];
            int saltCode = salt[i % salt.Length];
            int encryptedCode = value[i] + keyCode + saltCode + (i * 3);
            numbers.Add(Convert.ToString(encryptedCode, 8));
        }

        string packed = string.Join(".", numbers);
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(packed));
    }

    public static string DecryptPersonalData(string encryptedValue, string salt)
    {
        if (string.IsNullOrEmpty(encryptedValue))
        {
            return "";
        }

        string packed = Encoding.UTF8.GetString(Convert.FromBase64String(encryptedValue));
        string[] parts = packed.Split('.');
        StringBuilder builder = new StringBuilder(parts.Length);

        for (int i = 0; i < parts.Length; i++)
        {
            int encryptedCode = Convert.ToInt32(parts[i], 8);
            int keyCode = EncryptionKey[i % EncryptionKey.Length];
            int saltCode = salt[i % salt.Length];
            int sourceCode = encryptedCode - keyCode - saltCode - (i * 3);
            builder.Append((char)sourceCode);
        }

        return builder.ToString();
    }

    public static string NormalizeLogin(string login)
    {
        StringBuilder builder = new StringBuilder();

        foreach (char ch in login.Trim())
        {
            if (char.IsLetterOrDigit(ch) || IsSpecialSymbol(ch))
            {
                builder.Append(char.ToLowerInvariant(ch));
            }
        }

        return builder.ToString();
    }

    private static string CustomHash(string value, string salt)
    {
        string current = value + salt;

        for (int i = 0; i < HashRounds; i++)
        {
            current = HashRound(current, i);
            current = NormalizeHashLength(current, HashLength, i);
        }

        return current;
    }

    private static string HashRound(string value, int round)
    {
        StringBuilder result = new StringBuilder();
        int accumulator = value.Length + round;

        for (int i = 0; i < value.Length; i++)
        {
            string octal = Convert.ToString(value[i] + round + i, 8);
            bool useTwoDigits = false;

            for (int j = 0; j < octal.Length;)
            {
                int size = useTwoDigits && j + 1 < octal.Length ? 2 : 1;
                int number = Convert.ToInt32(octal.Substring(j, size));
                number = ((number + accumulator + round + i + j) * 1015) / 1000;

                int index = number % Symbols.Length;
                result.Append(Symbols[index]);
                accumulator = (accumulator + number + index + value[i]) % 100000;

                j += size;
                useTwoDigits = !useTwoDigits;
            }
        }

        return result.ToString();
    }

    private static string NormalizeHashLength(string value, int targetLength, int round)
    {
        if (value.Length == targetLength)
        {
            return value;
        }

        if (value.Length < targetLength)
        {
            StringBuilder newText = new StringBuilder(value);
            int i = 0;

            while (newText.Length < targetLength)
            {
                int current = newText[i % newText.Length];
                int next = newText[(i + 1) % newText.Length];
                newText.Append(Symbols[(current + next + i + round) % Symbols.Length]);
                i++;
            }

            return newText.ToString();
        }

        StringBuilder shortText = new StringBuilder(targetLength);

        for (int i = 0; i < targetLength; i++)
        {
            int left = value[i];
            int right = value[value.Length - 1 - (i % value.Length)];
            shortText.Append(Symbols[(left + right + i + round) % Symbols.Length]);
        }

        return shortText.ToString();
    }

    private static char[] CreateSymbols()
    {
        List<char> symbols = new List<char>(69);

        for (char ch = 'A'; ch <= 'Z'; ch++)
        {
            symbols.Add(ch);
        }

        for (char ch = 'a'; ch <= 'z'; ch++)
        {
            symbols.Add(ch);
        }

        for (char ch = '0'; ch <= '9'; ch++)
        {
            symbols.Add(ch);
        }

        symbols.Add('+');
        symbols.Add('-');
        symbols.Add('=');
        symbols.Add(',');
        symbols.Add('*');
        symbols.Add('/');
        symbols.Add('.');

        return symbols.ToArray();
    }

    private static bool IsSpecialSymbol(char ch)
    {
        return ch is '+' or '-' or '=' or ',' or '*' or '/' or '.';
    }
}