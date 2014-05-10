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
#if DEBUG
			return;
#else
			GoogleAnalytics.EasyTracker.GetTracker().SendView(pageName);
#endif
		}

		public static void trackAppBarMenuItems(String eventName, int value)
		{
#if DEBUG
			return;
#else
			GoogleAnalytics.EasyTracker.GetTracker().SendEvent(OldReaderTrackingConsts.appBarMenuItem, eventName, eventName, value);
#endif
		}

		public static void trackOldReaderEvent(String eventName, int value)
		{
#if DEBUG
			return;
#else
			GoogleAnalytics.EasyTracker.GetTracker().SendEvent(OldReaderTrackingConsts.oldReaderEvent, eventName, eventName, value);
#endif
		}

		public static void trackKeepUnread()
		{
#if DEBUG
			return;
#else
			GoogleAnalytics.EasyTracker.GetTracker().SendEvent(OldReaderTrackingConsts.oldReaderEvent, OldReaderTrackingConsts.keepUnread, OldReaderTrackingConsts.keepUnread, 0);
#endif
		}

		public static void trackFeedShare()
		{
#if DEBUG
			return;
#else
			GoogleAnalytics.EasyTracker.GetTracker().SendEvent(OldReaderTrackingConsts.oldReaderEvent, OldReaderTrackingConsts.feedShare, OldReaderTrackingConsts.feedShare, 0);
#endif
		}

		public static void trackFeedSave(bool bFromList)
		{
#if DEBUG
			return;
#else
			GoogleAnalytics.EasyTracker.GetTracker().SendEvent(OldReaderTrackingConsts.oldReaderEvent, OldReaderTrackingConsts.feedSave, OldReaderTrackingConsts.feedSave, bFromList ? 1 : 0);
#endif
		}

		public static void trackFeedOpenInIE()
		{
#if DEBUG
			return;
#else
			GoogleAnalytics.EasyTracker.GetTracker().SendEvent(OldReaderTrackingConsts.oldReaderEvent, OldReaderTrackingConsts.feedOpenedInIE, OldReaderTrackingConsts.feedSave, 0);
#endif
		}

		public static void trackFeedEmailed()
		{
#if DEBUG
			return;
#else
			GoogleAnalytics.EasyTracker.GetTracker().SendEvent(OldReaderTrackingConsts.oldReaderEvent, OldReaderTrackingConsts.feedEMailed, OldReaderTrackingConsts.feedSave, 0);
#endif
		}

		public static void trackFeedSwipe(bool bNext)
		{
#if DEBUG
			return;
#else
			String swipeVal = bNext ? OldReaderTrackingConsts.feedSwipeNext : OldReaderTrackingConsts.feedSwipePrev;
			GoogleAnalytics.EasyTracker.GetTracker().SendEvent(OldReaderTrackingConsts.oldReaderEvent, swipeVal, swipeVal, 0);
#endif
		}

		public static void trackRemotingErrorEvent(String error)
		{
#if DEBUG
			return;
#else
			GoogleAnalytics.EasyTracker.GetTracker().SendEvent(OldReaderTrackingConsts.remotingErrorEvent, error, error, 0);
#endif
		}

		public static void trackUnsubscribe()
		{
#if DEBUG
			return;
#else
			GoogleAnalytics.EasyTracker.GetTracker().SendEvent(OldReaderTrackingConsts.oldReaderEvent, OldReaderTrackingConsts.feedUnsubscribe, OldReaderTrackingConsts.feedUnsubscribe, 0);
#endif
		}

		public static void trackSubMove()
		{
#if DEBUG
			return;
#else
			GoogleAnalytics.EasyTracker.GetTracker().SendEvent(OldReaderTrackingConsts.oldReaderEvent, OldReaderTrackingConsts.feedMoved, OldReaderTrackingConsts.feedMoved, 0);
#endif
		}

		public static void sendException(String szDescr, bool bIsFatal)
		{
#if DEBUG
			return;
#else
			GoogleAnalytics.EasyTracker.GetTracker().SendException(szDescr, bIsFatal);
#endif
		}
	}
}
