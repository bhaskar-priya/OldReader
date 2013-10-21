using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.ObjectModel;

using AppNs =
#if OLD_READER_WP7
 Old_Reader_WP7;
#else
 Old_Reader;
#endif

namespace Old_Reader
{
	public partial class TagView : PhoneApplicationPage, INotifyPropertyChanged, INotifyPropertyChanging
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

		private int m_JobsPending = 0;
		private void StartJob()
		{
			m_JobsPending++;
			progressBar.Visibility = System.Windows.Visibility.Visible;
		}

		private void JobComplete()
		{
			m_JobsPending--;
			m_JobsPending = m_JobsPending < 0 ? 0 : m_JobsPending;
			if (m_JobsPending == 0)
			{
				progressBar.Visibility = System.Windows.Visibility.Collapsed;
			}
		}
		
		public TagView()
		{
			InitializeComponent();

			this.DataContext = this;

			(ApplicationBar.Buttons[0] as ApplicationBarIconButton).Text = AppNs.Resources.AppResources.strAddFeed;
		}

		private DataModel.Tag m_CurTag;
		public DataModel.Tag CurTag
		{
			get
			{
				return m_CurTag;
			}
			set
			{
#if OLD_READER_WP7
				NotifyPropertyChanging("CurTag");
#else
				NotifyPropertyChanging();
#endif
				m_CurTag = value;
#if OLD_READER_WP7
				NotifyPropertyChanged("CurTag");
#else
				NotifyPropertyChanged();
#endif
			}
		}

//		private ObservableCollection<DataModel.FeedItem> m_feedItems;
//		public ObservableCollection<DataModel.FeedItem> FeedItems
//		{
//			get
//			{
//				return m_feedItems;
//			}
//			set
//			{
//#if OLD_READER_WP7
//				NotifyPropertyChanging("FeedItems");
//#else
//				NotifyPropertyChanging();
//#endif
//				m_feedItems = value;
//#if OLD_READER_WP7
//				NotifyPropertyChanged("FeedItems");
//#else
//				NotifyPropertyChanged();
//#endif
//			}
//		}

		public int itemCount
		{
			get
			{
				int nCount = 0;
				//if (FeedItems != null)
				//{
				//	nCount += FeedItems.Count;
				//}
				if (CurTag != null && CurTag.Subscriptions != null)
				{
					nCount += CurTag.Subscriptions.Count;
				}
				return nCount;
			}
		}

		protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
		{
			base.OnNavigatedTo(e);
			Analytics.GAnalytics.trackPageView("tagView");

			String tagId = "";
			NavigationContext.QueryString.TryGetValue("tagId", out tagId);

			if (CurTag != null)
			{
				CurTag = null;
			}

			if (!String.IsNullOrEmpty(tagId))
			{
				DataModel.Tag curTag = AppNs.App.Contents.Tags.FirstOrDefault(t => t.id == tagId);
				if (curTag != null)
				{
					CurTag = curTag;
				}
			}

			if (CurTag != null)
			{
				// see if the display objects have to be items or subscription list
				if (CurTag.Subscriptions == null)
				{
					// get the feeds for the current tag
					//ObservableCollection<DataModel.FeedItem> tmpItems = new ObservableCollection<DataModel.FeedItem>();
					//foreach (DataModel.FeedItem curFeedItem in AppNs.App.Contents.FeedItems.Where(f => f.tags.Contains(CurTag) && f.isUnread))
					//{
					//	tmpItems.Add(curFeedItem);
					//}
					//FeedItems = tmpItems;
					MessageBox.Show("Should never have happened");
				}

				NotifyPropertyChanged("itemCount");
			}
		}

		private void subscriptionList_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (subscriptionList.SelectedIndex >= 0)
			{
				NavigationService.Navigate(new Uri("/SubscriptionView.xaml?feedId=" + (subscriptionList.SelectedItem as DataModel.Subscription).id, UriKind.Relative));
			}
		}

		private void feedList_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			//if (feedList.SelectedIndex >= 0)
			//{
			//	foreach (var f in FeedItems)
			//	{
			//		AppNs.App.FeedItems.Add(f);
			//	}
			//	NavigationService.Navigate(new Uri("/FeedView.xaml?selIdx=" + feedList.SelectedIndex, UriKind.Relative));
			//}
		}

		private void ApplicationBarAddIconButton_Click(object sender, EventArgs e)
		{
			NavigationService.Navigate(new Uri("/AddSubscription.xaml?tagId=" + CurTag.id, UriKind.Relative));
		}
	}
}