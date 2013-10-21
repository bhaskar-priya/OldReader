using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Old_Reader_Utils;

namespace Analytics
{
	public class GAnalytics
	{
		public static void trackPageView(String pageName)
		{
			GoogleAnalytics.EasyTracker.GetTracker().SendView(pageName);
		}

		public static void trackAppBarMenuItems(String eventName, int value)
		{
			GoogleAnalytics.Tracker tracker = GoogleAnalytics.EasyTracker.GetTracker();
			tracker.SendEvent(OldReaderTrackingConsts.appBarMenuItem, eventName, eventName, value);
		}

		public static void trackOldReaderEvent(String eventName, int value)
		{
			GoogleAnalytics.Tracker tracker = GoogleAnalytics.EasyTracker.GetTracker();
			tracker.SendEvent(OldReaderTrackingConsts.oldReaderEvent, eventName, eventName, value);
		}

		public static void trackKeepUnread()
		{
			GoogleAnalytics.Tracker tracker = GoogleAnalytics.EasyTracker.GetTracker();
			tracker.SendEvent(OldReaderTrackingConsts.oldReaderEvent, OldReaderTrackingConsts.keepUnread, OldReaderTrackingConsts.keepUnread, 0);
		}

		public static void trackFeedShare()
		{
			GoogleAnalytics.Tracker tracker = GoogleAnalytics.EasyTracker.GetTracker();
			tracker.SendEvent(OldReaderTrackingConsts.oldReaderEvent, OldReaderTrackingConsts.feedShare, OldReaderTrackingConsts.feedShare, 0);
		}

		public static void trackFeedSave(bool bFromList)
		{
			GoogleAnalytics.Tracker tracker = GoogleAnalytics.EasyTracker.GetTracker();
			tracker.SendEvent(OldReaderTrackingConsts.oldReaderEvent, OldReaderTrackingConsts.feedSave, OldReaderTrackingConsts.feedSave, bFromList ? 1 : 0);
		}
	}
}
