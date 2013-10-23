using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using DataModel;
using Newtonsoft.Json.Linq;
using System.Windows.Data;
using System.Windows;
using System.IO.IsolatedStorage;
using System.Windows.Media.Imaging;

using AppNs  =
#if OLD_READER_WP7
Old_Reader_WP7;
#else
Old_Reader;
#endif

namespace Old_Reader_Utils
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
			String srcId=(String)value;
			// check to see if the feed in in the saved
			if (DataStore.CachedFeed.isStarred(srcId))
			{
				return "/Toolkit.Content/favs.removefrom.png";
			}
			else
			{
				return "/Toolkit.Content/favs.addto.png";
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
		private static Dictionary<String, BitmapImage> cachedImages = new Dictionary<String,BitmapImage>();

		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
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
					retVal = null;
				}
			}
			else
			{
				try
				{
					retVal = new BitmapImage(new Uri(path));
				}
				catch
				{
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

	public class OldReaderConsts
	{
		public static string Auth = "Auth";
		public static string id = "id";
		public static string summary = "summary";
		public static String published = "published";
		public static String crawlTimeMsec = "crawlTimeMsec";
		public static string author = "author";
		public static string canonical = "canonical";
		public static string unreadcounts = "unreadcounts";
		public static string count = "count";
		public static string max = "max";
		public static string tags = "tags";
		public static string categories = "categories";
		public static string iconUrl = "iconUrl";
		public static string htmlUrl = "htmlUrl";
		public static string url = "url";
		public static string title = "title";
		public static string subscriptions = "subscriptions";
		public static string items = "items";
		public static string href = "href";
		public static string content = "content";
		public static string origin = "origin";
		public static string streamId = "streamId";
		public static string EMail = "EMail";
		public static string Password = "Password";
		public static string label = "label";
		public static string continuation = "continuation";
		public static string showReadItems = "showReadItems";
		public static string additionalDownloadCount = "additionalDownloadCount";
	}

	public class OldReaderTrackingConsts
	{
		public static String appBarMenuItem = "appBarMenuItem";
		public static String showReadItems = "showReadItems";
		public static String oldReaderEvent = "oldReaderEvent";
		public static String tagListComplete = "tagListComplete";
		public static String subscriptionComplete = "subscriptionComplete";
		public static String unreadCountComplete = "unreadCountComplete";
		public static String addFeed = "addFeed";
		public static String keepUnread = "keepUnread";
		public static String feedShare = "feedShare";
		public static String feedSave = "feedSave";
		public static String feedSwipeNext = "feedSwipeNext";
		public static String feedSwipePrev = "feedSwipePrev";
		public static String remotingErrorEvent = "remotingErrorEvent";
	}

	public class Utils
	{
		public static Dictionary<String, String> toDictionary(String szData, char itemSep, char valueSep)
		{
			Dictionary<String, String> outDictionary = new Dictionary<string, string>();
			foreach (String curNameValue in szData.Split(itemSep))
			{
				String[] nameValue = curNameValue.Split(valueSep);
				if (nameValue.Length == 2)
				{
					outDictionary[nameValue[0]] = nameValue[1];
				}
			}
			return outDictionary;
		}

		public static int handleUnreadCounts(String szResponse,ObservableCollection<Subscription> subs,ObservableCollection<Tag> tags)
		{
			int nMax = 0;
			JObject rootObj = JObject.Parse(szResponse);
			nMax = int.Parse((String)rootObj[OldReaderConsts.max]);

			foreach (JObject unreadCntObj in (JArray)rootObj[OldReaderConsts.unreadcounts])
			{
				String objId = (String)unreadCntObj[OldReaderConsts.id];
				int unreadCount = int.Parse((String)unreadCntObj[OldReaderConsts.count]);
				// check the tags and subs
				Tag targetTag = tags.SingleOrDefault(t => t.id == objId);
				if (targetTag != null)
				{
					targetTag.unreadCount = unreadCount;
				}
				else
				{
					Subscription targetSub = subs.SingleOrDefault(s => s.id == objId);
					if (targetSub != null)
					{
						targetSub.unreadCount = unreadCount;
					}
				}
			}

			return nMax;
		}

		public static String getInitializationStateStr(DataModel.OldReaderContents.TInitializationStates state)
		{
			switch (state)
			{
				case DataModel.OldReaderContents.TInitializationStates.kGettingTagList:
					return AppNs.Resources.AppResources.strGettingTagList;
				case DataModel.OldReaderContents.TInitializationStates.kGettingSubscription:
					return AppNs.Resources.AppResources.strGettingSubscriptionList;
				case DataModel.OldReaderContents.TInitializationStates.kGettingUnreadCount:
					return AppNs.Resources.AppResources.strGettingUnreadCount;
				case DataModel.OldReaderContents.TInitializationStates.kGettingUnreadItems:
					return AppNs.Resources.AppResources.strGettingUnreadItems;
			}
			return "";
		}

		public static bool isSwipeHorizontalEnough(Microsoft.Phone.Controls.FlickGestureEventArgs e)
		{
			if (
				(
					(e.Angle < 30 || e.Angle > 330)
					|| (e.Angle > 150 && e.Angle < 210)
				)
				&& Math.Abs(e.HorizontalVelocity) > 50)
			{
				return true;
			}
			return false;
		}
	}
}
