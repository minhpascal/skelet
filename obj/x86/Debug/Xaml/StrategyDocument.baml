   M S B A M L   `   `   `  ����@  <Shell, Version=4.0.0.0, Culture=neutral, PublicKeyToken=nullclr-namespace:RobotRobot  5          Robot.StrategyDocument   5      W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      T PStockSharp.Xaml.Charting, Version=4.2.68.0, Culture=neutral, PublicKeyToken=nullK GStockSharp.Xaml, Version=4.2.68.0, Culture=neutral, PublicKeyToken=null-sx"http://schemas.stocksharp.com/xaml   5      !Robotclr-namespace:Robot   5      h dXceed.Wpf.AvalonDock.Themes.Metro, Version=2.2.0.0, Culture=neutral, PublicKeyToken=3e4669d2f30244f4g cXceed.Wpf.AvalonDock.Themes.Aero, Version=2.2.0.0, Culture=neutral, PublicKeyToken=3e4669d2f30244f4i	 eXceed.Wpf.AvalonDock.Themes.VS2010, Version=2.2.0.0, Culture=neutral, PublicKeyToken=3e4669d2f30244f4[
 WXceed.Wpf.AvalonDock, Version=2.2.0.0, Culture=neutral, PublicKeyToken=3e4669d2f30244f4=xcad,http://schemas.xceed.com/wpf/xaml/avalondock   	 
 5      .��5      ) 
 #Xceed.Wpf.AvalonDock.DockingManager  -   5           Name$  dockManager��5         AllowMixedOrientation .    Theme 5      ,  &Xceed.Wpf.AvalonDock.Themes.MetroTheme  5         Layout. 5      , 
 &Xceed.Wpf.AvalonDock.Layout.LayoutRoot     	RootPanel. 5      - 
 'Xceed.Wpf.AvalonDock.Layout.LayoutPanel     Orientation$ 
Horizontal=�6   � 
 �Xceed.Wpf.AvalonDock.Layout.LayoutGroup`1[[Xceed.Wpf.AvalonDock.Layout.ILayoutPanelElement, Xceed.Wpf.AvalonDock, Version=2.2.0.0, Culture=neutral, PublicKeyToken=3e4669d2f30244f4]]   Children. 5        $ Vertical=�6   . 5      6 
 0Xceed.Wpf.AvalonDock.Layout.LayoutAnchorablePane  � 
 �Xceed.Wpf.AvalonDock.Layout.LayoutPositionableGroup`1[[Xceed.Wpf.AvalonDock.Layout.LayoutAnchorable, Xceed.Wpf.AvalonDock, Version=2.2.0.0, Culture=neutral, PublicKeyToken=3e4669d2f30244f4]]   
DockHeight$ 70 �6"   � 
 �Xceed.Wpf.AvalonDock.Layout.LayoutGroup`1[[Xceed.Wpf.AvalonDock.Layout.LayoutAnchorable, Xceed.Wpf.AvalonDock, Version=2.2.0.0, Culture=neutral, PublicKeyToken=3e4669d2f30244f4]]   Children. 5      2	 
 ,Xceed.Wpf.AvalonDock.Layout.LayoutAnchorable	  -   5        ToolWindowChart��/
 
 )Xceed.Wpf.AvalonDock.Layout.LayoutContent	 
  Title$	 Информация��5   8   
 
  ToolTip$
 My WinForms Tool��5   6    	  CanHide .   
  CanClose .   
  Content. 5         Robot.StrategiesInfoGrid  -   5          Name$ _strategyInfoGrid��5   7   5      5        5      . 5      	  -   5        ToolWindowOrders��$	 Заявки��5   9   $
 My WinForms Tool��5   6    .   .  . 5   	     StockSharp.Xaml.OrderGrid  -   5          Name$ 	OrderGrid��5      6)   5   	   5        5      . 5       	  -   5        ToolWindowTrades�� 
  	ContentId$ WinFormsWindow��5    9   $	 Сделки��6T   $
 My WinForms Tool��5!   6    .   .  . 5"        StockSharp.Xaml.TradeGrid  -   5          Name$ 
_tradeGrid��5"   +   5#      5$        5%      . 5&      	  -   5        ToolWindowLog��$ WinFormsWindow��5&   6   $	 Сообщения��6Q   $
 My WinForms Tool��5'   6    .   .  . 5(   	      StockSharp.Xaml.LogControl  -	   5          Name$ 
logControl��5(         MaxItemsCount$
 3000��6+      
TimeFormat$ yy/MM/dd HH:mm:ss.fff��5)   !      
AutoResize . 6D   5*      5+      5,        5-         	DockWidth$	 340 �6    . 5.      	  $	 Свойства��6   $ PropertiesContent��6.   . 5/      1  +StockSharp.Xaml.PropertyGrid.PropertyGridEx  -
   5          Name$ ProperyGridStrategy��5/   ,   X TXceed.Wpf.Toolkit, Version=2.3.0.0, Culture=neutral, PublicKeyToken=3e4669d2f30244f41  +Xceed.Wpf.Toolkit.PropertyGrid.PropertyGrid   	ShowTitle .     ShowSortOptions .     ShowSearchBox .     NameColumnWidth$	 150Q�50   T   51      52      53      54      55      56      