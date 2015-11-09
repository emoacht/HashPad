using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace FileHashChecker.Common
{
	public abstract class NotificationObject : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		protected void SetProperty<T>(ref T store, T value, [CallerMemberName]string propertyName = null)
		{
			if (EqualityComparer<T>.Default.Equals(store, value))
				return;

			store = value;
			RaisePropertyChanged(propertyName);
		}

		protected void RaisePropertyChanged([CallerMemberName]string propertyName = null)
			=> PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}