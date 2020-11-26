using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using HashPad.Common;

namespace HashPad.Models
{
	public class Settings : NotificationObject
	{
		public bool ComputesAutomatically
		{
			get => GetValue(ref _computesAutomatically);
			set => SetValue(ref _computesAutomatically, value);
		}
		private bool _computesAutomatically = true;

		public bool ReadsAutomatically
		{
			get => GetValue(ref _readsAutomatically);
			set => SetValue(ref _readsAutomatically, value);
		}
		private bool _readsAutomatically = true;

		internal string LastSourceFolderPath
		{
			get => GetValue(ref _lastSourceFolderPath);
			set => SetValue(ref _lastSourceFolderPath, value);
		}
		private string _lastSourceFolderPath;

		internal HashType LastTargetHashType
		{
			get => GetValue(ref _lastTargetHashType);
			set => SetValue(ref _lastTargetHashType, value);
		}
		private HashType _lastTargetHashType;

		#region Get/Set

		public bool IsPersistent => PlatformInfo.IsPackaged;

		private T GetValue<T>(ref T storage, [CallerMemberName] string propertyName = null)
		{
			if (IsPersistent && SettingsAccessor.TryGetValue(out T value, propertyName))
				storage = value;

			return storage;
		}

		private void SetValue<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
		{
			if (SetProperty(ref storage, value, propertyName) && IsPersistent)
				SettingsAccessor.SetValue(value, propertyName);
		}

		#endregion
	}
}