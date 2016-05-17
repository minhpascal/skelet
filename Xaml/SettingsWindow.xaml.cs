namespace Robot
{
	using System.Windows;

	public partial class SettingsWindow
	{
		public SettingsWindow()
		{
			InitializeComponent();

			SettingsGrid.SelectedObject = SettingsEngine.Instance.Properties;
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			DialogResult = true;

			SettingsEngine.Instance.SaveSettings();
		}
	}
}
