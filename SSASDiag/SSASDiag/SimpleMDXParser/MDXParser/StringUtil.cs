namespace SimpleMDXParser
{
    using System;

    internal static class StringUtil
    {
        internal static string CamelCase(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return string.Empty;
            }
            char[] chArray = name.ToCharArray();
            chArray[0] = char.ToUpper(chArray[0]);
            int length = chArray.Length;
            for (int i = 1; i < length; i++)
            {
                chArray[i] = char.ToLower(chArray[i]);
            }
            return new string(chArray);
        }
    }
}

