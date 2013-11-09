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
		public static String feedUnsubscribe = "feedUnsubscribe";
		public static String feedMoved = "feedMoved";
		public static String feedOpenedInIE = "feedOpenedInIE";
		public static String feedEMailed = "feedEMailed";
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
