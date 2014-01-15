using System.Windows;
using Microsoft.Phone.Scheduler;
using Microsoft.Phone.Shell;
using System;
using Newtonsoft.Json.Linq;
using Old_Reader_Utils;

namespace OldReaderBackgroundAgent_WP7
{
	public class ScheduledAgent : ScheduledTaskAgent
	{
		private static volatile bool _classInitialized;

		/// <remarks>
		/// ScheduledAgent constructor, initializes the UnhandledException handler
		/// </remarks>
		public ScheduledAgent()
		{
			if (!_classInitialized)
			{
				_classInitialized = true;
				// Subscribe to the managed exception handler
				Deployment.Current.Dispatcher.BeginInvoke(delegate
				{
					Application.Current.UnhandledException += ScheduledAgent_UnhandledException;
				});
			}
		}

		/// Code to execute on Unhandled Exceptions
		private void ScheduledAgent_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
		{
			if (System.Diagnostics.Debugger.IsAttached)
			{
				// An unhandled exception has occurred; break into the debugger
				System.Diagnostics.Debugger.Break();
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
			WS.Remoting.token = Utilities.AppSettings.Read<String>(OldReaderConsts.AuthToken, "");
			WS.Remoting rm = new WS.Remoting(unreadCountComplete, remotingError);
			rm.getUnreadCount();
		}

		private void unreadCountComplete(String szResponse)
		{
			int nUnreadCount = 0;
			try
			{
				JObject rootObj = JObject.Parse(szResponse);
				nUnreadCount = int.Parse((String)rootObj[OldReaderConsts.max]);
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

		private static Version TargetedVersion = new Version(7, 10, 8858);
		public static bool IsTargetedVersion
		{
			get
			{
				return Environment.OSVersion.Version >= TargetedVersion;
			}
		}

		private static void SetProperty(object instance, string name, object value)
		{
			var setMethod = instance.GetType().GetProperty(name).GetSetMethod();
			setMethod.Invoke(instance, new object[] { value });
		}

		private void UpdateUnreadCount(int nUnreadCount = 0)
		{
			if (IsTargetedVersion)
			{
				Type iconicTileDataType = Type.GetType("Microsoft.Phone.Shell.IconicTileData, Microsoft.Phone");

				// Get the ShellTile type so we can call the new version of "Update" that takes the new Tile templates.
				Type shellTileType = Type.GetType("Microsoft.Phone.Shell.ShellTile, Microsoft.Phone");

				foreach (var curTile in ShellTile.ActiveTiles)
				{
					try
					{
						var UpdateTileData = iconicTileDataType.GetConstructor(new Type[] { }).Invoke(null);
						SetProperty(UpdateTileData, "Count", Math.Min(nUnreadCount, 99));

						shellTileType.GetMethod("Update").Invoke(curTile, new Object[] { UpdateTileData });
					}
					catch
					{
					}
				}
			}
			
		}
	}
}