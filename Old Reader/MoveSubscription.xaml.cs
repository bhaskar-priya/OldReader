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
	public partial class MoveSubscription : PhoneApplicationPage, INotifyPropertyChanged, INotifyPropertyChanging
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

		public MoveSubscription()
		{
			InitializeComponent();

			this.DataContext = this;

			(ApplicationBar.Buttons[0] as ApplicationBarIconButton).Text = AppNs.Resources.AppResources.strDoneAppBarButton;
		}

		private DataModel.Subscription m_Sub;
		public DataModel.Subscription Sub
		{
			get
			{
				return m_Sub;
			}
			set
			{
#if OLD_READER_WP7
				NotifyPropertyChanging("Sub");
#else
				NotifyPropertyChanging();
#endif
				m_Sub = value;
#if OLD_READER_WP7
				NotifyPropertyChanged("Sub");
#else
				NotifyPropertyChanged();
#endif
			}
		}

		private String m_tagTitle;
		public String TagTitle
		{
			get
			{
				return m_tagTitle;
			}
			set
			{
#if OLD_READER_WP7
				NotifyPropertyChanging("TagTitle");
#else
				NotifyPropertyChanging();
#endif
				m_tagTitle = value;
#if OLD_READER_WP7
				NotifyPropertyChanged("TagTitle");
#else
				NotifyPropertyChanged();
#endif
			}
		}

		public ObservableCollection<DataModel.Tag> Tags
		{
			get
			{
				return AppNs.App.Contents.Tags;
			}
		}

		private String m_TagId = "";

		protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
		{
			if (Sub == null)
			{
				String szFeedId = "";
				NavigationContext.QueryString.TryGetValue("feedId", out szFeedId);

				Sub = AppNs.App.Contents.Subscriptions.FirstOrDefault(s => s.id == szFeedId);

				if (Sub.categories == null)
				{
					ctlTagList.SelectedItem = DataModel.Tag.AllItems;
				}
				else
				{
					ctlTagList.SelectedItem = Sub.categories[0];
				}

				m_TagId = (ctlTagList.SelectedItem as DataModel.Tag).id;
				TagTitle = (ctlTagList.SelectedItem as DataModel.Tag).title;

				manageUIState();
			}
		}

		private void ApplicationBarDoneIconButton_Click(object sender, EventArgs e)
		{
			StartJob();
			WS.Remoting rm = new WS.Remoting(MoveComplete);
			rm.moveSubscriptionToFolder(Sub.id, (ctlTagList.SelectedItem as DataModel.Tag).id);
		}

		private void MoveComplete(String szResponse)
		{
			Dispatcher.BeginInvoke(() =>
				{
					JobComplete();
					if (szResponse.ToUpper() != "OK")
					{
						MessageBox.Show(AppNs.Resources.AppResources.strFeedMoveError);
					}
					else
					{
						Analytics.GAnalytics.trackSubMove();
						AppNs.App.RefreshContents = true;
						NavigationService.GoBack();
					}
				}
			);
		}

		private int m_JobsPending = 0;
		private void StartJob()
		{
			m_JobsPending++;
			trayProgress.IsVisible = m_JobsPending != 0;
			(ApplicationBar.Buttons[0] as ApplicationBarIconButton).IsEnabled = false;
		}

		private void JobComplete()
		{
			m_JobsPending--;
			m_JobsPending = m_JobsPending < 0 ? 0 : m_JobsPending;
			if (m_JobsPending == 0)
			{
				trayProgress.IsVisible = m_JobsPending != 0;
				(ApplicationBar.Buttons[0] as ApplicationBarIconButton).IsEnabled = true;
			}
		}

		private void ctlTagList_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			manageUIState();
		}

		private void manageUIState()
		{
			if (ctlTagList.SelectedItem != null)
			{
				DataModel.Tag selTag = ctlTagList.SelectedItem as DataModel.Tag;
				(ApplicationBar.Buttons[0] as ApplicationBarIconButton).IsEnabled = selTag.id != m_TagId;
			}
		}
	}
}