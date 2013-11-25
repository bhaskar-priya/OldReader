using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.IsolatedStorage;

using AppNs =
#if OLD_READER_WP7
Old_Reader_WP7;
#else
 Old_Reader;
#endif

namespace Utilities
{
	public class AppSettings
	{
		public static IsolatedStorageSettings userSettings = IsolatedStorageSettings.ApplicationSettings;

		public static T Read<T>(String szSettingName, T DefValue)
		{
			try
			{
				return (T)userSettings[szSettingName];
			}
			catch
			{
				return DefValue;
			}
		}

		public static void Write(String szSettingName, Object objVal)
		{
			userSettings[szSettingName] = objVal;
			userSettings.Save();
		}
	}
}
