using System.Linq;
using System.Windows;

namespace HashPad.Models;

internal static class ClipboardHelper
{
	public static bool TryReadHexText(out string text)
	{
		text = null;

		if (!Clipboard.ContainsText(TextDataFormat.Text))
			return false;

		var buffer = Clipboard.GetText()?.Replace('-', (char)0).Trim();
		if (string.IsNullOrEmpty(buffer))
			return false;

		if (!buffer.All(x => IsHex(x)))
			return false;

		text = buffer;
		return true;

		static bool IsHex(char c) => c is (>= '0' and <= '9') or (>= 'A' and <= 'F') or (>= 'a' and <= 'f');
	}
}