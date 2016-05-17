namespace Robot
{
	using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

	public enum OrderType
	{
		Market,
		Quoting
	}

	internal class OrderTypeItemsSource : IItemsSource
	{
		public ItemCollection GetValues()
		{
			var types = new ItemCollection
			{
				{OrderType.Market, "Маркет"},
				{OrderType.Quoting, "Котирование"}
			};

			return types;
		}
	}
}
