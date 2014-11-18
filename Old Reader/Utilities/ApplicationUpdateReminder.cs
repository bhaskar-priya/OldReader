using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cimbalino.Phone.Toolkit.Services;
using System.Windows;
using Microsoft.Phone.Tasks;

using AppNs = Old_Reader;

namespace Utilities
{
	public class ApplicationUpdateReminder
	{
		private const string REJECT_UPDATE_COUNT = "REJECT_UPDATE_COUNT";

		public int UpdateRejectedCount
		{
			get
			{
				return AppSettings.Read<int>(REJECT_UPDATE_COUNT, 0);
			}
			set
			{
				AppSettings.Write(REJECT_UPDATE_COUNT, value);
			}
		}

		public ApplicationUpdateReminder()
		{
		}

		private async void CheckForUpdates()
		{
			try
			{
				var informationService = new MarketplaceInformationService();
				var applicationManifestService = new ApplicationManifestService();
				var result = await informationService.GetAppInformationAsync();
				var appInfo = applicationManifestService.GetApplicationManifest();
				Version myVersion;

				myVersion = new Version(appInfo.App.Version);

				var updatedVersion = new Version(result.Entry.Version);

				if (updatedVersion > myVersion)
				{
					if (MessageBox.Show(AppNs.Resources.AppResources.strUpdateAppMsgTitle,
						AppNs.Resources.AppResources.strUpdateAppMsgBody, MessageBoxButton.OKCancel) == MessageBoxResult.OK)
					{
						new MarketplaceDetailTask().Show();
					}
					else
					{
						UpdateRejectedCount++;
					}
				}
				else
				{
					UpdateRejectedCount = 0;
				}
			}
			catch (Exception ex)
			{
				//Log the error somehow

			}
		}

		public void ForceCheckForUpdates()
		{
			CheckForUpdates();
		}
	}
}
