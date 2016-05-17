using Robot.Strategies;

namespace Robot
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Xml.Serialization;

	using Ecng.Common;

	using StockSharp.Logging;

	class SettingsEngine : ILogReceiver
	{
		private static SettingsEngine _instance;

		private const string _strategiesXml = @"DbStrategies.xml";
        private const string _testingStrategiesXml = @"DbTesting.xml";
		private const string _settingsXml = @"DbSettings.xml";

	    private const string _hustleEveryDayStrategies = @"HustleEveryDayStrategies.xml";
        private const string _testingHustleEveryDayStrategies = @"TestingHustleEveryDayStrategies.xml";

		private SettingsEngine() { }

		public SettingsProperties Properties { set; get; }

		public static SettingsEngine Instance
		{
			get
			{
				if (_instance == null)
				{
					_instance = new SettingsEngine();
					_instance.LoadSettings();
				}
				return _instance;
			}
		}

        public void SaveHustleEveryDayStrategiesProperties(List<HustleEveryDayStrategyProperties> properties)
        {
            this.AddInfoLog("Сохранение стратегий в {0}", _hustleEveryDayStrategies);
            using (var writer = new StreamWriter(_hustleEveryDayStrategies, false))
            {
                var mySerializer = new XmlSerializer(typeof(List<HustleEveryDayStrategyProperties>));
                mySerializer.Serialize(writer, properties);
            }
        }


        public List<HustleEveryDayStrategyProperties> LoadHustleEveryDayStrategiesProperties()
        {
            this.AddInfoLog("Загрузка стратегий из {0}", _hustleEveryDayStrategies);

            var result = new List<HustleEveryDayStrategyProperties>();

            if (File.Exists(_hustleEveryDayStrategies))
            {
                try
                {
                    using (var reader = new StreamReader(_hustleEveryDayStrategies))
                    {
                        var x = new XmlSerializer(typeof(List<HustleEveryDayStrategyProperties>));
                        result = (List<HustleEveryDayStrategyProperties>)x.Deserialize(reader);
                    }
                }
                catch (Exception ex)
                {
                    this.AddErrorLog("Ошибка загрузки {0}: {1}", _hustleEveryDayStrategies, ex.Message);
                }
            }

            return result;
        }

        public void SaveTestingHustleEveryDayStrategyProperties(List<HustleEveryDayStrategyTestingProperties> properties)
        {
            this.AddInfoLog("Сохранение тестовых стратегий в {0}", _testingHustleEveryDayStrategies);
            using (var writer = new StreamWriter(_testingHustleEveryDayStrategies, false))
            {
				var mySerializer = new XmlSerializer(typeof(List<HustleEveryDayStrategyTestingProperties>));
                mySerializer.Serialize(writer, properties);
            }
        }

        public List<HustleEveryDayStrategyTestingProperties> LoadTestingHustleEveryDayStrategyProperties()
        {
            this.AddInfoLog("Загрузка стратегий тестирования из {0}", _testingHustleEveryDayStrategies);

            var result = new List<HustleEveryDayStrategyTestingProperties>();

            if (File.Exists(_testingStrategiesXml))
            {
                using (var reader = new StreamReader(_testingHustleEveryDayStrategies))
                {
                    var x = new XmlSerializer(typeof(List<BaseShellTestingProperties>));
                    result = (List<HustleEveryDayStrategyTestingProperties>)x.Deserialize(reader);
                }
            }

            return result;
        }


        #region Base Methods


        public void SaveStrategies(List<BaseShellStrategyProperties> strategies)
		{
			this.AddInfoLog("Сохранение стратегий в {0}", _strategiesXml);
			using (var writer = new StreamWriter(_strategiesXml, false))
			{
				var mySerializer = new XmlSerializer(typeof(List<BaseShellStrategyProperties>));
				mySerializer.Serialize(writer, strategies);
			}
		}


		public List<BaseShellStrategyProperties> LoadStrategies()
		{
			this.AddInfoLog("Загрузка стратегий из {0}", _strategiesXml);

			var result = new List<BaseShellStrategyProperties>();

			if (File.Exists(_strategiesXml))
			{
			    try
			    {
                    using (var reader = new StreamReader(_strategiesXml))
                    {
                        var x = new XmlSerializer(typeof(List<BaseShellStrategyProperties>));
                        result = (List<BaseShellStrategyProperties>)x.Deserialize(reader);
                    }
			    }
			    catch (Exception ex)
			    {
			        this.AddErrorLog("Ошибка загрузки {0}: {1}", _strategiesXml, ex.Message);
			    }
			}

			return result;
		}

        public void SaveTestingStrategies(List<BaseShellTestingProperties> strategies)
        {
            this.AddInfoLog("Сохранение тестовых стратегий в {0}", _testingStrategiesXml);
            using (var writer = new StreamWriter(_testingStrategiesXml, false))
            {
                var mySerializer = new XmlSerializer(typeof(List<BaseShellTestingProperties>));
                mySerializer.Serialize(writer, strategies);
            }
        }

        public List<BaseShellTestingProperties> LoadTestingStrategies()
        {
            this.AddInfoLog("Загрузка стратегий тестирования из {0}", _testingStrategiesXml);

            var result = new List<BaseShellTestingProperties>();

            if (File.Exists(_testingStrategiesXml))
            {
                using (var reader = new StreamReader(_testingStrategiesXml))
                {
                    var x = new XmlSerializer(typeof(List<BaseShellTestingProperties>));
                    result = (List<BaseShellTestingProperties>)x.Deserialize(reader);
                }
            }

            return result;
        }


        #endregion
        public void SaveSettings()
		{
			this.AddInfoLog("Сохранение настроек в {0}", _settingsXml);

			using (var writer = new StreamWriter(_settingsXml, false))
			{
				var mySerializer = new XmlSerializer(typeof(SettingsProperties));
				mySerializer.Serialize(writer, Properties);
			}
		}

		public void LoadSettings()
		{
			this.AddInfoLog("Загрузка настроек из {0}", _settingsXml);

			if (File.Exists(_settingsXml))
			{
				using (var reader = new StreamReader(_settingsXml))
				{
					try
					{
						var x = new XmlSerializer(typeof(SettingsProperties));
						Properties = (SettingsProperties)x.Deserialize(reader);
					}
					catch (Exception ex)
					{
						this.AddWarningLog("Ошибка загрузки настроек из {0}. {1}", _settingsXml, ex);
						Properties = new SettingsProperties();
					}
				}
			}
			else
			{
				Properties = new SettingsProperties();
			}
		}

		#region Logging
		private readonly Guid _id = Guid.NewGuid();

		public Guid Id
		{
			get { return _id; }
		}

		string ILogSource.Name
		{
			get { return "Настройки"; }
		}

		ILogSource ILogSource.Parent
		{
			get { return null; }
			set { }
		}

		public LogLevels LogLevel
		{
			get
			{
				return LogLevels.Debug;
			}
			set
			{
			}
		}

        public DateTimeOffset CurrentTime
        {
            get
            {
                // TODO: use MarketTimeOffset after settings are loaded  
				return DateTimeOffset.Now;
                //return DateTime.Now.AddHours(SettingsEngine.Instance.Properties.MarketTimeOffset);
            }
        }

		public event Action<LogMessage> Log;

		public void AddLog(LogMessage message)
		{
			Log.SafeInvoke(message);
		}

		#endregion

		public void Dispose()
		{
			//throw new NotImplementedException();
		}
	}
}
