namespace Robot
{
	using System.ComponentModel;

	using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

	[DisplayName(@"Настройки робота")]
	public class SettingsProperties : INotifyPropertyChanged
	{
		public enum Type
		{
			Quik,
			Alfa
		}

		private Type _connectionType = Type.Quik;
		private int _marketTimeOffset;
		private bool _emulation;
		private string _quikPath = @"c:\bin\QUIK-Junior\info.exe";
		private string _quikLogin;
		private string _quikPassword;

		[Category(@"Основные")]
		//[CategoryOrder(0)]
		[DisplayName(@"Подключение")]
		[Description(@"Тип подключения.")]
		[PropertyOrder(0)]
		public Type ConnectionType
		{
			get { return _connectionType; }
			set
			{
				_connectionType = value;
				OnPropertyChanged("ConnectionType");
			}
		}

		[Category(@"Основные")]
		//[CategoryOrder(0)]
		[DisplayName(@"Эмуляция")]
		[Description(@"Виртуальное исполнение сделок.")]
		[PropertyOrder(1)]
		public bool Emulation
		{
			get { return _emulation; }
			set
			{
				_emulation = value;
				OnPropertyChanged("Emulation");
			}
		}

		[Category(@"Основные")]
		//[CategoryOrder(0)]
		[DisplayName(@"Часовой сдвиг")]
		[Description(@"Смещение времени с серверами биржи")]
		[PropertyOrder(2)]
		public int MarketTimeOffset
		{
			get { return _marketTimeOffset; }
			set
			{
				_marketTimeOffset = value;
				OnPropertyChanged("MarketTimeOffset");
			}
		}

		[DisplayName(@"Путь Quik")]
		[Category(@"Подключение (Quik)")]
		//[CategoryOrder(3)]
		[Description(@"Полный путь к info.exe")]
		[PropertyOrder(0)]
		public string QuikPath
		{
			get { return _quikPath; }
			set
			{
				_quikPath = value;
				OnPropertyChanged("QuikPath");
			}
		}

		[DisplayName(@"Логин")]
		[Category(@"Подключение (Quik)")]
		//[CategoryOrder(3)]
		[Description(@"Логин для авторизации в Quik. Используется для авто-запуска терминала.")]
		[PropertyOrder(1)]
		public string QuikLogin
		{
			get { return _quikLogin; }
			set
			{
				_quikLogin = value;
				OnPropertyChanged("QuikLogin");
			}
		}

		[DisplayName(@"Пароль")]
		[Category(@"Подключение (Quik)")]
		//[CategoryOrder(3)]
		[Description(@"Пароль для авторизации в Quik. Используется для авто-запуска терминала.")]
		[PropertyOrder(2)]
		public string QuikPassword
		{
			get { return _quikPassword; }
			set
			{
				_quikPassword = value;
				OnPropertyChanged("QuikPassword");
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		protected void OnPropertyChanged(string name)
		{
			var handler = PropertyChanged;
			if (handler != null)
			{
				handler(this, new PropertyChangedEventArgs(name));
			}
		}

	}
}
