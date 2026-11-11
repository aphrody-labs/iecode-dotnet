namespace IESEViewModel.Tools;

// Extended Class for System.Collections.Generic

public static class Collections
{
    public static double SumIf<T>(this IEnumerable<T> source, Func<T, bool> predicate, Func<T, double> valueSelector)
    {
        return source.Where(predicate)
                     .Sum(valueSelector);
    }
}
