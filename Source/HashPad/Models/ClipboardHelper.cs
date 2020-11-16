using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace HashPad.Models
{
	internal class ClipboardHelper
	{
		public static bool TryReadHexText(out string text)
		{
			text = null;

			if (!Clipboard.ContainsText(TextDataFormat.Text))
				return false;

			var buffer = Clipboard.GetText()?.Replace('-', (char)0).Trim();
			if (string.IsNullOrEmpty(buffer))
				return false;

			if (!HashTypeHelper.TryGetHashType(buffer.Length, out _))
				return false;

			if (!buffer.All(x => IsHex(x)))
				return false;

			text = buffer;
			return true;

			static bool IsHex(char c) => char.IsDigit(c) || ('A' <= c && c <= 'F') || ('a' <= c && c <= 'f');
		}
	}
}