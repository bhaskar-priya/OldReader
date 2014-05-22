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
		protected void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
		{
			if (PropertyChanged != null)
			{
				System.Windows.Deployment.Current.Dispatcher.BeginInvoke(() =>
					{
						PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
					});
			}
		}

		public event PropertyChangingEventHandler PropertyChanging;

		// Used to notify Silverlight that a property has changed.
		protected void NotifyPropertyChanging([CallerMemberName] String propertyName = "")
		{
			if (PropertyChanging != null)
			{
				System.Windows.Deployment.Current.Dispatcher.BeginInvoke(() =>
					{
						PropertyChanging(this, new PropertyChangingEventArgs(propertyName));
					});
			}
		}
		#endregion
	}
}
