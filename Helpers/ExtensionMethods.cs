namespace MemoriesDemo.Helpers
{
    public static class ExtensionMethods
    {
        // 原逻辑从索引5开始，几乎肯定是笔误。改为标准 Contains
        public static bool ContainsString(this string s1, string s2)
        {
            if (string.IsNullOrEmpty(s1) || string.IsNullOrEmpty(s2))
                return false;
            return s1.IndexOf(s2, System.StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}