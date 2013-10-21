using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DataModel
{
	public class NotifyPropertyImpl : INotifyPropertyChanged, INotifyPropertyChanging
	{
		#region INotifyPropertyChange Members

		public event PropertyChangedEventHandler PropertyChanged;

		// Used to notify Silverlight that a property has changed.
#if OLD_READER_WP7
		protected void NotifyPropertyChanged(String propertyName)
#else
		protected void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
#endif
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		public event PropertyChangingEventHandler PropertyChanging;

		// Used to notify Silverlight that a property has changed.
#if OLD_READER_WP7
		protected void NotifyPropertyChanging(String propertyName)
#else
		protected void NotifyPropertyChanging([CallerMemberName] String propertyName = "")
#endif
		{
			if (PropertyChanging != null)
			{
				PropertyChanging(this, new PropertyChangingEventArgs(propertyName));
			}
		}
		#endregion
	}
}
