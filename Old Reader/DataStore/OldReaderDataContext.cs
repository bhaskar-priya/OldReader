﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Linq;
using Microsoft.Phone.Data.Linq;

using AppNs = Old_Reader;

namespace DataStore
{
	public class OldReaderDataContext : DataContext
	{
		// Specify the connection String as a static, used in main page and app.xaml.
		public static String DBConnectionString = "Data Source=isostore:/OldReader.sdf; max database size=512";
        
        // Pass the connection String to the base class.
		public OldReaderDataContext(String connectionString)
			: base(connectionString)
        {
			if (!DatabaseExists())
			{
				CreateDatabase();
				DatabaseSchemaUpdater dbVerInitializer = this.CreateDatabaseSchemaUpdater();
				dbVerInitializer.DatabaseSchemaVersion = 1;
				dbVerInitializer.Execute();
			}

			DatabaseSchemaUpdater dbUpdater = this.CreateDatabaseSchemaUpdater();
			int dbVersion = dbUpdater.DatabaseSchemaVersion;

			cleanDatabase();

			try
			{
				// Version history
				/*
				0
				 * Had saved Feeds
				1
				 * CachedFeed
						public String ID { get; set; }
						public String Content { get; set; }
						public String Author { get; set; }
						public String href { get; set; }
						public String Title { get; set; }
						public DateTime publishedTime { get; set; }
						public DateTime crawledtime { get; set; }
						public String Categories { get; set; }
						public String StreamId { get; set; }
						public bool Unread { get; set; }
						public bool dirty { get; set; }
						public bool Starred { get; set; }
				 * ContinuationId
						public String ID { get; set; }
						public String continuationID { get; set; }
				*/
				if (dbVersion < 1)
				{
					// this was default database
					dbUpdater.AddTable<CachedFeed>();
					dbUpdater.AddTable<ContinuationId>();
					dbUpdater.DatabaseSchemaVersion = 1;
					dbUpdater.Execute();
				}
			}
			catch (Exception exp)
			{
				Analytics.GAnalytics.sendException(exp.Message, false);
			}
        }

		public void cleanDatabase()
		{
			try
			{
				DateTime threshHoldTime = DateTime.Now.AddDays(0 - AppNs.App.RetentionDays);
				// delete all items which are more than two month old but save the starred items
				var oldItems = from c in CachedFeeds
							   where
								   c.publishedTime < threshHoldTime &&
								   c.Starred == false &&
								   c.Unread == false
							   select c;
				foreach (var oldItem in oldItems)
				{
					CachedFeeds.DeleteOnSubmit(oldItem);
				}
				SubmitChanges();
			}
			catch { }
		}

		public Table<CachedFeed> CachedFeeds;
		public Table<ContinuationId> ContinuationIds;
	}
}
