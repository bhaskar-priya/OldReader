#if OLD_READER_WP7
using Old_Reader_WP7.Resources;
#else
using Old_Reader.Resources;
#endif

#if OLD_READER_WP7
namespace Old_Reader_WP7
#else
namespace Old_Reader
#endif
{
	/// <summary>
	/// Provides access to string resources.
	/// </summary>
	public class LocalizedStrings
	{
		private static AppResources _localizedResources = new AppResources();

		public AppResources LocalizedResources { get { return _localizedResources; } }
	}
}