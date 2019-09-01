using System;
using System.ComponentModel;
using System.Linq;
using Microsoft.EntityFrameworkCore;

public static class Extensions
{
    public static string GetEnumDescription<TEnum>(this TEnum item)
    {
        return item.GetType()
                .GetField(item.ToString())
                .GetCustomAttributes(typeof(DescriptionAttribute), false)
                .Cast<DescriptionAttribute>()
                .FirstOrDefault()?.Description ?? string.Empty;
    }

    public static void SeedEnumValues<T, TEnum>(this DbSet<T> dbSet, Func<TEnum, T> converter)
        where T : class
    {
        Enum.GetValues(typeof(TEnum))
            .Cast<object>()
            .Select(value => converter((TEnum)value))
            .ToList()
            .ForEach(instance => dbSet.Add(instance));
    }
}