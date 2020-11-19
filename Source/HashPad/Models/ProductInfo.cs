using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace HashPad.Models
{
	public static class ProductInfo
	{
		static ProductInfo()
		{
			var assembly = Assembly.GetExecutingAssembly();

			Version = assembly.GetName().Version;
			Product = assembly.GetAttribute<AssemblyProductAttribute>().Product;
			Title = assembly.GetAttribute<AssemblyTitleAttribute>().Title;
			Location = assembly.Location;
		}

		public static Version Version { get; }
		public static string Product { get; }
		public static string Title { get; }

		internal static string Location { get; }

		private static TAttribute GetAttribute<TAttribute>(this Assembly assembly) where TAttribute : Attribute =>
			(TAttribute)Attribute.GetCustomAttribute(assembly, typeof(TAttribute));
	}
}