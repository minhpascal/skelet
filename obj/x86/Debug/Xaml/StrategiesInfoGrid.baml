   M S B A M L   `   `   `  ����@  <Shell, Version=4.0.0.0, Culture=neutral, PublicKeyToken=nullclr-namespace:RobotRobot  5          Robot.StrategiesInfoGrid   5      W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      ,OpenSpreaderclr-namespace:Robot     5        � DataContext  5   7   ��  �� RelativeSource �� *Self+ � 	Resources 5	      (  "Robot.ProcessStateToColorConverter   stateToColorConverter��%   &	          5
      6.   5      .��5      � .+�5      �� -   5         ��Name$ ListViewStrategies��5      $��0��6    �� SelectionMode$ Single=�6O    �� SelectedIndex$ 0��6f    �� View 5      �� 5      .P�5      ��  �� Width$	 140��6   $
��Name��6"    �� CellTemplate 5      �� 5   	   .R�5   
   �� 	 �� TextAlignment$	 Right=�66   ��6   �� 
 �� Path$
 Params.Name�66   5   
   5      �� 5      $ 70��6   $��State��6!    5      �� 5   	   .R�5   
   �� $	 Right=�67   ��6   �� $
 ProcessState���6M   �� $
 ProcessState� �� 	Converter# [  5   
   5      �� 5      $ 50��6   $��Trades��6!    5       �� 5!   	   .R�5"   
   �� $	 Right=�69   ��6   �� $
 MyTrades.Count�69   5#   
   5%      �� 5&      $ 50��6   $��Orders��6!    5'      �� 5(   	   .R�5)   
   �� $	 Right=�67   ��6   �� $
 Orders.Count�67   5*   
   5,      �� 5-      $ 45��6   $��Position��6!    5.      �� 5/   	   .R�50   
   �� $	 Right=�63   ��6   �� $
 Position�63   51   
   53      �� 54      $ 60��6   $	��P&L��6!    55      �� 56   	   .R�57   
   �� $	 Right=�6?   ��6   �� $	
 PnL� �� StringFormat$ F2��6?   58   
   5:      �� 5;      $ 60��6   $��Slippage��6!    5<      �� 5=   	   .R�5>   
   �� $	 Right=�6D   ��6   �� $
 Slippage�$ F2��6D   5?   
   5A      �� 5B      $ 70��6   $
��Time��6!    �� DisplayMemberBinding 6/   �� $
 WorkingTime�$ 
hh\:mm\:ss���� 5D      $ 70��6   $��Price��6!    60   �� $
 LastTradePrice�$ F2��5E      5G      5H      5I      