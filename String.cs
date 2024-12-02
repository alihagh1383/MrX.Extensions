namespace MrX.Extensions;

public static class String
{
    public static IEnumerable<int> AllIndicesOf(this string str, string searchStr)
    {
        var minIndex = str.IndexOf(searchStr, StringComparison.Ordinal);
        while (minIndex != -1)
        {
            yield return minIndex;
            minIndex = str
                .IndexOf(searchStr, minIndex + searchStr.Length, StringComparison.Ordinal);
        }
    }

    public static string ReplaceAt(this string input, int index, char newChar)
    {
        var chars = input.ToCharArray();
        chars[index] = newChar;
        return new string(chars);
    }
}