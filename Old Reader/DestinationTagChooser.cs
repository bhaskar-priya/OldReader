using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.ComponentModel;

using AppNs =
#if OLD_READER_WP7
Old_Reader_WP7;
#else
 Old_Reader;
#endif

namespace Old_Reader_Utils
{
	class DestinationChooserDoneEvent : System.EventArgs
	{
		public DataModel.Tag selectedTag;
		public DestinationChooserDoneEvent(DataModel.Tag selTag)
		{
			selectedTag = selTag;
		}
	}

	class DestinationTagChooser
	{
		PhoneApplicationPage m_ownerPage;
		public DestinationTagChooser(PhoneApplicationPage ownerPage)
		{
			m_ownerPage = ownerPage;
		}

		public DataModel.Tag SelectedTag
		{
			get;
			set;
		}

		public DataModel.Subscription CurItem
		{
			get;
			set;
		}

		public ObservableCollection<DataModel.Tag> Tags
		{
			get;
			set;
		}

		public event EventHandler<DestinationChooserDoneEvent> Done;

		private Popup m_Popup = null;
		private bool trayVisibility = false;
		private bool appBarVisibility = false;

		public void show()
		{
			trayVisibility = SystemTray.IsVisible;
			SystemTray.IsVisible = false;
			appBarVisibility = m_ownerPage.ApplicationBar.IsVisible;
			m_ownerPage.ApplicationBar.IsVisible = false;

			m_Popup = new Popup();
			// create a border
			Border border = new Border();
			border.BorderBrush = (Brush)Application.Current.Resources["PhoneBackgroundBrush"];
			border.BorderThickness = new Thickness(1);
			border.Background = (Brush)Application.Current.Resources["PhoneBackgroundBrush"];
#if OLD_READER_WP7
			border.Width = (AppNs.App.Current as AppNs.App).RootFrame.ActualWidth;
			border.Height = (AppNs.App.Current as AppNs.App).RootFrame.ActualHeight;
#else
			border.Width = AppNs.App.RootFrame.ActualWidth;
			border.Height = AppNs.App.RootFrame.ActualHeight;
#endif

			// build the selector
			ScrollViewer scroller = new ScrollViewer();
			
			StackPanel skt_pnl_outter = new StackPanel();                             // stack panel 
			skt_pnl_outter.Background = (Brush)Application.Current.Resources["PhoneBackgroundBrush"];
			skt_pnl_outter.Orientation = System.Windows.Controls.Orientation.Vertical;

			Brush foregroundBrush = (Brush)Application.Current.Resources["PhoneForegroundBrush"];
			Brush accentBrush = (Brush)Application.Current.Resources["PhoneAccentBrush"];
			foreach (var curItem in Tags)
			{
				TextBlock txtBlock = new TextBlock();
				txtBlock.Text = curItem.ToString();
				txtBlock.Tag = curItem;
				txtBlock.TextAlignment = TextAlignment.Left;
				txtBlock.FontSize = 43;
				txtBlock.Margin = new Thickness(32, 21, 0, 20);
				txtBlock.Foreground = curItem == SelectedTag ? accentBrush : foregroundBrush;
				txtBlock.Tap += txtBlock_Tap;
				Microsoft.Phone.Controls.TiltEffect.SetIsTiltEnabled(txtBlock, true);
				skt_pnl_outter.Children.Add(txtBlock);
			}

			scroller.Content = skt_pnl_outter;
			border.Child = scroller;

			m_Popup.Child = border;
			m_Popup.IsOpen = true;
		}

		void txtBlock_Tap(object sender, System.Windows.Input.GestureEventArgs e)
		{
			if (sender is TextBlock)
			{
				SelectedTag = (DataModel.Tag)(sender as TextBlock).Tag;
				Close();
				Done(this, new DestinationChooserDoneEvent(SelectedTag));
			}
		}

		public void Close()
		{
			if (m_Popup != null)
			{
				SystemTray.IsVisible = trayVisibility;
				m_ownerPage.ApplicationBar.IsVisible = appBarVisibility;

				m_Popup.IsOpen = false;
				m_Popup.Child = null;
				m_Popup = null;
			}
		}

		public bool IsOpen
		{
			get
			{
				if (m_Popup != null)
				{
					return m_Popup.IsOpen;
				}
				return false;
			}
		}
	}
}
