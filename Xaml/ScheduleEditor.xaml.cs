namespace Robot
{
	using System.Windows;
	using System.Windows.Data;

	using Ecng.Xaml;

	using Xceed.Wpf.Toolkit;
	using Xceed.Wpf.Toolkit.PropertyGrid;
	using Xceed.Wpf.Toolkit.PropertyGrid.Editors;

	//internal class ScheduleTypesItemsSource : IItemsSource
	//{
	//	public ItemCollection GetValues()
	//	{
	//		var sizes = new ItemCollection
	//		{
	//			{typeof(TimeRangeProperties),  TimeRangeProperties.DisplayName}
	//		};

	//		return sizes;
	//	}
	//}

	public partial class ScheduleEditor : ITypeEditor
	{
		PropertyItem _item;

		public ScheduleEditor()
		{
			InitializeComponent();
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			var editor = new CollectionControlDialog(_item.PropertyType)
			{
				//NewItemTypes = new[] { typeof(TimeRangeProperties) },
				Height = 400
			};

			var binding = new Binding("Value")
			{
				Source = _item,
				Mode = _item.IsReadOnly ? BindingMode.OneWay : BindingMode.TwoWay
			};

			BindingOperations.SetBinding(editor, CollectionControlDialog.ItemsSourceProperty, binding);
			editor.ShowModal(this);

			var obj = (BaseShellStrategyProperties)_item.Instance;

			obj.OnPropertyChanged("Schedule");
		}

		public FrameworkElement ResolveEditor(PropertyItem propertyItem)
		{
			_item = propertyItem;

			return this;
		}
	}
}
