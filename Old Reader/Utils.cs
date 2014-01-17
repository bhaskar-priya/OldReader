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
using Microsoft.Phone.Shell;

using AppNs  =
#if OLD_READER_WP7
Old_Reader_WP7;
#else
Old_Reader;
#endif

namespace Old_Reader_Utils
{
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

		private static void SetProperty(object instance, string name, object value)
		{
			var setMethod = instance.GetType().GetProperty(name).GetSetMethod();
			setMethod.Invoke(instance, new object[] { value });
		}

#if OLD_READER_WP7
		private static Version TargetedVersion = new Version(7, 10, 8858);
		public static bool IsTargetedVersion
		{
			get
			{
				return Environment.OSVersion.Version >= TargetedVersion;
			}
		}
#endif

		public static void ClearTileCount()
		{
			foreach (var curTile in ShellTile.ActiveTiles)
			{
				try
				{
#if OLD_READER_WP7
					if (IsTargetedVersion)
					{
						// Get the new FlipTileData type.
						Type flipTileDataType = Type.GetType("Microsoft.Phone.Shell.FlipTileData, Microsoft.Phone");

						// Get the ShellTile type so we can call the new version of "Update" that takes the new Tile templates.
						Type shellTileType = Type.GetType("Microsoft.Phone.Shell.ShellTile, Microsoft.Phone");

						var UpdateTileData = flipTileDataType.GetConstructor(new Type[] { }).Invoke(null);

						Uri NormalIcon = new Uri("/Resources/oldreader-icon.png", UriKind.Relative);
						// Set the properties. 
						SetProperty(UpdateTileData, "Title", "Old Reader");
						SetProperty(UpdateTileData, "Count", 0);
						SetProperty(UpdateTileData, "BackTitle", "Old Reader");
						SetProperty(UpdateTileData, "SmallBackgroundImage", NormalIcon);
						SetProperty(UpdateTileData, "BackgroundImage", NormalIcon);
						SetProperty(UpdateTileData, "BackBackgroundImage", NormalIcon);
						SetProperty(UpdateTileData, "WideBackgroundImage", NormalIcon);
						SetProperty(UpdateTileData, "WideBackBackgroundImage", NormalIcon);
						SetProperty(UpdateTileData, "WideBackContent", "Old Reader");

						// Invoke the new version of ShellTile.Update.
						shellTileType.GetMethod("Update").Invoke(curTile, new Object[] { UpdateTileData });
					}
#else
					IconicTileData iconicTileData = new IconicTileData();
					iconicTileData.Count = 0;
					iconicTileData.IconImage = new Uri("/Resources/oldreader-icon.png", UriKind.Relative);
					iconicTileData.SmallIconImage = iconicTileData.IconImage;
					curTile.Update(iconicTileData);
#endif
				}
				catch
				{
				}
			}
		}

		public static void SyncStarredItems()
		{
			// all said and done
			// migrate the starred items once
			if (!AppNs.App.StarredMigrationDone)
			{
				AppNs.App.StarredMigrationDone = true;
				var localFeedItems = from DataStore.CachedFeed curItem in AppNs.App.ReaderDB.CachedFeeds where curItem.Starred == true select curItem;
				List<string> strStarredItemIds = new List<string>();
				foreach (var curCachedFeedItem in localFeedItems)
				{
					strStarredItemIds.Add(curCachedFeedItem.ID);
				}
				(new WS.Remoting()).starItems(strStarredItemIds, true);
			}
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
				case DataModel.OldReaderContents.TInitializationStates.kGettingStarredItems:
					return AppNs.Resources.AppResources.strGettingStarredItems;
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

		private static Dictionary<String, int> daysToString = null;

		public static Dictionary<String, int> DaysToString
		{
			get
			{
				if (daysToString == null)
				{
					daysToString = new Dictionary<string, int>();
					daysToString[AppNs.Resources.AppResources.strDuration1Day] = 1;
					daysToString[AppNs.Resources.AppResources.strDuration1Week] = 7;
					daysToString[AppNs.Resources.AppResources.strDuration15Days] = 15;
					daysToString[AppNs.Resources.AppResources.strDuration1Month] = 30;
					daysToString[AppNs.Resources.AppResources.strDuration2Months] = 60;
					daysToString[AppNs.Resources.AppResources.strDuration3Months] = 90;
					daysToString[AppNs.Resources.AppResources.strDuration6Months] = 180;
					daysToString[AppNs.Resources.AppResources.strDuration1Year] = 365;
				}
				return daysToString;
			}
		}
	}
}
