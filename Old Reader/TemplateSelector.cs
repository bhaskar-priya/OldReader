using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Data;

namespace Old_Reader_Utils
{
	public class FeedTemplateSelector : IValueConverter
	{
		public Object ReadTemplate
		{
			get
			{
				return Application.Current.Resources["PhoneTextSubtleStyle"];
			}
		}

		public Object UnreadTemplate
		{
			get
			{
				return Application.Current.Resources["PhoneTextAccentStyle"];
			}
		}

		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if (value == null)
			{
				return UnreadTemplate;
			}
			bool isUnread = (bool)value;
			return isUnread ? UnreadTemplate : ReadTemplate;
		}

		// had to be implemented
		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
