namespace UnoGame.Core.Helpers;

public static class ListHelpers
{
    private static Random Rng { get; } = new();

    public static void InsertRandomly<T>(this IList<T> list, T item)
    {
        list.Insert(Rng.Next(list.Count), item);
    }
}