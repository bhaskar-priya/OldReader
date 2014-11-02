using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Windows.Data;
using System.Windows;
using System.IO.IsolatedStorage;
using System.Windows.Media.Imaging;

using AppNs =
#if OLD_READER_WP7
Old_Reader_WP7;
#else
Old_Reader;
#endif

namespace Utilities
{
	public class VisibilityConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			// just in case
			if (value == null)
			{
				return Visibility.Collapsed;
			}
			else if (value is String && String.IsNullOrEmpty(value as String))
			{
				return Visibility.Collapsed;
			}

			return Visibility.Visible;
		}

		// had to be implemented
		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}

	public class DispObjCtxMenuConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			// just in case
			if (value == null)
			{
				return false;
			}

			if (value is DataModel.Subscription)
			{
				return true;
			}
			return false;
		}

		// had to be implemented
		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}

	public class InvertingVisibilityConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			// just in case
			if (value == null)
			{
				return Visibility.Visible;
			}
			else if (value is String && String.IsNullOrEmpty(value as String))
			{
				return Visibility.Visible;
			}

			return Visibility.Collapsed;
		}

		// had to be implemented
		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}

	public class EmptyItemVisibilityConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if (Old_Reader.App.HideEmptyItems)
			{
				// just in case
				if (value == null)
				{
					return Visibility.Visible;
				}

				return (System.Convert.ToInt32(value) > 0) ? Visibility.Visible : Visibility.Collapsed;
			}

			return Visibility.Visible;
		}

		// had to be implemented
		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}

	public class IntegerInvertingVisibilityConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			// just in case
			if (value == null)
			{
				return Visibility.Visible;
			}

			return (System.Convert.ToInt32(value) <= 0) ? Visibility.Visible : Visibility.Collapsed;
		}

		// had to be implemented
		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}

	public class EnabledConvertor : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			// just in case
			if (value == null)
			{
				return false;
			}

			return (System.Convert.ToInt32(value) <= 0) ? false : true;
		}

		// had to be implemented
		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}

	public class FeedListIconConvertor : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			String srcId = (String)value;
			return getIconString(srcId);
		}

		public static String getIconString(String srcId)
		{
			// check to see if the feed in in the saved
			if (!DataStore.CachedFeed.isStarred(srcId))
			{
				return "/Toolkit.Content/heart2.empty.png";
			}
			else
			{
				return "/Toolkit.Content/heart.png";
			}
		}

		// had to be implemented
		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}

	public class FilePathToImageConverter : IValueConverter
	{
		private static IsolatedStorageFile ISF = IsolatedStorageFile.GetUserStoreForApplication();
		private static Dictionary<String, BitmapImage> cachedImages = new Dictionary<String, BitmapImage>();

		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if (value == null)
			{
				return null;
			}
			if (value is DataModel.Subscription)
			{
				value = (value as DataModel.Subscription).iconUrl;
			}
			string path = value as string;

			if (cachedImages.Keys.Contains(path))
			{
				return cachedImages[path];
			}

			BitmapImage retVal = null;
			if (String.IsNullOrEmpty(path))
				return null;
			if ((path.Length > 9) && (path.ToLower().Substring(0, 9).Equals("isostore:")))
			{
				try
				{
					using (var sourceFile = ISF.OpenFile(path.Substring(9), System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.Read))
					{
						retVal = new BitmapImage();
						retVal.SetSource(sourceFile);
					}
				}
				catch (Exception exp)
				{
					Analytics.GAnalytics.sendException(exp.Message, false);
					retVal = null;
				}
			}
			else
			{
				try
				{
					retVal = new BitmapImage(new Uri(path));
				}
				catch (Exception exp)
				{
					Analytics.GAnalytics.sendException(exp.Message, false);
					retVal = new BitmapImage(new Uri(path, UriKind.Relative));
				}
			}

			cachedImages[path] = retVal;
			return retVal;
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}

	public class URLToImageConverter : IValueConverter
	{
		private static IsolatedStorageFile ISF = IsolatedStorageFile.GetUserStoreForApplication();

		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if (value is String)
			{
				Uri uri = new Uri(value as String);
				String path = String.Format(@"isostore:/{0}", DataStore.IconManager.getIconFileNameForHost(uri.Host).Replace(@"\", "/"));
				if (ISF.FileExists(path))
				{
					using (var sourceFile = ISF.OpenFile(path.Substring(9), System.IO.FileMode.Open, System.IO.FileAccess.Read))
					{
						BitmapImage bm = new BitmapImage();
						bm.SetSource(sourceFile);
						return bm;
					}
				}
			}

			return "";
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}

	public class TimeToHumanStrConverter : IValueConverter
	{
		static TimeSpan hourSpan = new TimeSpan(1, 0, 0);
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			String timeValue = AppNs.Resources.AppResources.strTimeStrUnknown;
			if (value is DateTime)
			{
				TimeSpan elapsedTime = DateTime.Now - ((DateTime)value);

				if (elapsedTime < hourSpan)
				{
					timeValue = AppNs.Resources.AppResources.strTimeStrNow;
				}
				else if (elapsedTime.Days < 1)
				{
					timeValue = String.Format(AppNs.Resources.AppResources.strTimeStrHours, elapsedTime.Hours);
				}
				else
				{
					timeValue = String.Format(AppNs.Resources.AppResources.strTimeStrDays, elapsedTime.Days);
				}
			}
			return timeValue;
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}

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