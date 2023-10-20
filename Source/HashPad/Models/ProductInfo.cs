using System;
using System.Reflection;
using System.Text.RegularExpressions;

namespace HashPad.Models;

public static class ProductInfo
{
	static ProductInfo()
	{
		var assembly = Assembly.GetExecutingAssembly();

		Version = assembly.GetName().Version;
		Product = assembly.GetAttribute<AssemblyProductAttribute>().Product;
		Title = assembly.GetAttribute<AssemblyTitleAttribute>().Title;
		Location = Regex.Replace(assembly.Location, @"\.dll$", ".exe");
	}

	public static Version Version { get; }
	public static string Product { get; }
	public static string Title { get; }

	internal static string Location { get; }

	private static TAttribute GetAttribute<TAttribute>(this Assembly assembly) where TAttribute : Attribute =>
		(TAttribute)Attribute.GetCustomAttribute(assembly, typeof(TAttribute));
}