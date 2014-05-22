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

using AppNs = Old_Reader;

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
			Grid border = new Grid();
			border.Background = (Brush)Application.Current.Resources["PhoneBackgroundBrush"];
			border.Width = AppNs.App.RootFrame.ActualWidth;
			border.Height = AppNs.App.RootFrame.ActualHeight;

			// build the selector
			ScrollViewer scroller = new ScrollViewer();
			
			StackPanel stackPanel = new StackPanel();                             // stack panel 
			stackPanel.Background = (Brush)Application.Current.Resources["PhoneBackgroundBrush"];
			stackPanel.Orientation = System.Windows.Controls.Orientation.Vertical;

			Brush foregroundBrush = (Brush)Application.Current.Resources["PhoneForegroundBrush"];
			Brush accentBrush = (Brush)Application.Current.Resources["PhoneAccentBrush"];
			foreach (var curItem in Tags)
			{
				Button tagButton = new Button();
				tagButton.Content = curItem.ToString();
				tagButton.Tag = curItem;
				tagButton.HorizontalContentAlignment = HorizontalAlignment.Left;
				tagButton.FontSize = 43;
				tagButton.Foreground = curItem == SelectedTag ? accentBrush : foregroundBrush;
				tagButton.Tap += txtBlock_Tap;
				tagButton.Width = border.Width;
				tagButton.BorderThickness = new Thickness(0);

				stackPanel.Children.Add(tagButton);
			}
			
			scroller.Content = stackPanel;
			border.Children.Add(scroller);

			m_Popup.Child = border;
			m_Popup.IsOpen = true;
		}

		void txtBlock_Tap(object sender, System.Windows.Input.GestureEventArgs e)
		{
			if (sender is Button)
			{
				SelectedTag = (DataModel.Tag)(sender as Button).Tag;
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
