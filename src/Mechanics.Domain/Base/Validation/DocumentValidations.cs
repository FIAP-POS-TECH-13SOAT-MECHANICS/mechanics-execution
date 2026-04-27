namespace Mechanics.Domain.Base.Validation;

public static class DocumentValidations
{
    public static bool ValidateCpf(string number)
    {
        if (!RegexUtils.Cpf().IsMatch(number) || number.Distinct().Count() == 1)
            return false;

        var sum = 0;
        for (var i = 0; i < 9; i++)
            sum += (number[i] - '0') * (10 - i);

        var remainder = sum % 11;
        var firstDigit = remainder < 2 ? 0 : 11 - remainder;

        if (firstDigit != number[9] - '0')
            return false;

        sum = 0;
        for (var i = 0; i < 10; i++)
            sum += (number[i] - '0') * (11 - i);

        remainder = sum % 11;
        var secondDigit = remainder < 2 ? 0 : 11 - remainder;

        return secondDigit == number[10] - '0';
    }

    public static bool ValidateCnpj(string number)
    {
        if (!RegexUtils.Cnpj().IsMatch(number))
            return false;

        const int baseValue = 48;
        int[] weights = [6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];

        var sumDv1 = 0;
        var sumDv2 = 0;

        for (var i = 0; i < 12; i++)
        {
            var asciiDigit = number[i] - baseValue;
            sumDv1 += asciiDigit * weights[i + 1];
            sumDv2 += asciiDigit * weights[i];
        }

        var dv1 = sumDv1 % 11 < 2 ? 0 : 11 - (sumDv1 % 11);

        if (dv1 != number[12] - '0')
            return false;

        sumDv2 += dv1 * weights[12];
        var dv2 = sumDv2 % 11 < 2 ? 0 : 11 - (sumDv2 % 11);

        return dv2 == number[13] - '0';
    }
}
