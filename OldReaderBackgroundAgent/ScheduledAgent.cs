using System.Diagnostics;
using System.Windows;
using Microsoft.Phone.Scheduler;
using Microsoft.Phone.Shell;
using System;
using Newtonsoft.Json.Linq;
using Old_Reader_Utils;

namespace OldReaderBackgroundAgent
{
	public class ScheduledAgent : ScheduledTaskAgent
	{
		/// <remarks>
		/// ScheduledAgent constructor, initializes the UnhandledException handler
		/// </remarks>
		static ScheduledAgent()
		{
			// Subscribe to the managed exception handler
			Deployment.Current.Dispatcher.BeginInvoke(delegate
			{
				Application.Current.UnhandledException += UnhandledException;
			});
		}

		/// Code to execute on Unhandled Exceptions
		private static void UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
		{
			if (Debugger.IsAttached)
			{
				// An unhandled exception has occurred; break into the debugger
				Debugger.Break();
			}
		}

		/// <summary>
		/// Agent that runs a scheduled task
		/// </summary>
		/// <param name="task">
		/// The invoked task
		/// </param>
		/// <remarks>
		/// This method is called when a periodic or resource intensive task is invoked
		/// </remarks>
		protected override void OnInvoke(ScheduledTask task)
		{
			WS.Remoting.TService currentService = Utilities.AppSettings.Read<WS.Remoting.TService>(Old_Reader_Utils.OldReaderConsts.ServiceId, WS.Remoting.TService.kTheOldReader);
			WS.Remoting.token = Utilities.AppSettings.Read<String>(OldReaderConsts.AuthToken, "");
			WS.Remoting rm = new WS.Remoting(currentService, unreadCountComplete, remotingError);
			rm.getUnreadCount();
		}

		private void unreadCountComplete(String szResponse)
		{
			int nUnreadCount = 0;
			try
			{
				JObject rootObj = JObject.Parse(szResponse);
				if (rootObj[OldReaderConsts.bazTotalUnreadCount] != null)
				{
					nUnreadCount = int.Parse((String)rootObj[OldReaderConsts.bazTotalUnreadCount]);
				}
				else
				{
					nUnreadCount = int.Parse((String)rootObj[OldReaderConsts.max]);
				}
			}
			catch
			{
				nUnreadCount = 0;
			}

			UpdateUnreadCount(nUnreadCount);

			NotifyComplete();
		}

		private void remotingError(String szErrorMessage)
		{
			UpdateUnreadCount();
			NotifyComplete();
		}

		private void UpdateUnreadCount(int nUnreadCount = 0)
		{
			foreach (var curTile in ShellTile.ActiveTiles)
			{
				try
				{
					IconicTileData iconicTileData = new IconicTileData();
					iconicTileData.Count = Math.Min(nUnreadCount, 99);
					iconicTileData.IconImage = new Uri("/Resources/oldreader-icon.png", UriKind.Relative);
					if (nUnreadCount > 0)
					{
						iconicTileData.SmallIconImage = new Uri("/Resources/oldreader-icon-small.png", UriKind.Relative);
					}
					else
					{
						iconicTileData.SmallIconImage = iconicTileData.IconImage;
					}

					curTile.Update(iconicTileData);
				}
				catch
				{
				}
			}
		}
	}
}