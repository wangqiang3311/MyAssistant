   M S B A M L   `   `   `  ����\  XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089  �� 
TargetType   ComboxStyleBtn �� Property �� Value ��Name J� RenderTransformOrigin �� Stretch �� Angle �� Triggers P� Property	 P� Value
 �� 
TargetName  ComboBoxStyle �� 
TargetType � ColumnDefinitions ��Name �� 
IsReadOnly �� 
IsReadOnly �� Mode �� UpdateSourceTrigger �� CornerRadius �� 	ClickMode d� 	IsChecked �� Path �� RelativeSource �Name �� IsDropDownOpen � AllowsTransparency �� MaxDropDownHeight � ActualWidth J� SnapsToDevicePixels J� Effect3   -System.Windows.Media.Effects.DropShadowEffect    Color    
BlurRadius     ShadowDepth!    Opacity  ScrollViewerStyle" K� IsItemsHost-   'System.Windows.Input.KeyboardNavigation#   DirectionalNavigation$ �Name% � RowDefinitions& ��Name' ��Name( �� CanContentScroll) ��Name4  .System.Windows.Automation.AutomationProperties*   AutomationId+ � Cursor, �� ScrollableHeight- � Maximum. � Minimum*/ �� #ComputedVerticalScrollBarVisibility0 �� ViewportHeight1 �� ViewportSize  ScrollBarStyle2 � Value3 �� ScrollableWidth,4 �� %ComputedHorizontalScrollBarVisibility5 �� ViewportWidth6 �� Orientation  	windowBtn�� 5      F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation      :x,http://schemas.microsoft.com/winfx/2006/xaml      5      %�  &	        &	 �    7   &	 R    &	 �    �� 5      "  d�6   .�5   
   ��  � �� 5      �� 5      .R�5	      �� $
 Back��6   ���#F7FDF7$��1q�6@   
������ ._�5
      F� $��PathFill��6   ���#3798f9$��8��6>   $��6��6H   $��0��6S   "����@KL     р�� ��� �    ��� 	$ 0.5,0.5&�6�   $
 Fill=�6�   z�5      V� 5   "   .~�5   &   �� �� 5   &   �� 5   &   $	 180Q�66   T� 5   &   5   #   5      5       5      P� 5       �@J�IsMouseOver$
	 True��69   . �5      �� $
 PathFill��6%    � ��
 �����6[   �� 5      $

 Back��6%    � �� �#3798f96_   �� 5      $

 Back��6%    � �� �#3798f96`   5      5      5      5      �� 5"      "  ��6   .�5$   
   ��  � �� 5%      �� 5'      "  ��6   .�5(      ��  � ��$ 22��62   6?   �� 5)       � ��$ 60��61   6>   �� 5+       � �� 5,      �� 5-      " ��6.   .R�5.   "   �� $
��Back��6)   
������ $��0,0,0,0q�6O   ���#81D779._�5/   &   �� $��Center=�6X   $
��Left=�6s   $��5,0,0,0q�6�   ��67   �� *Source+6�   50   #    51   "   P� 52   &    �@J�IsMouseOver$
	 True��6E   . �53   *   �� $

 Back��61    � �� �#d8e9fa6k   54   '   P� 55   &    �@��IsHighlighted$
	 True��6G   . �56   *   �� $

 Back��61    � �� �#d8e9fa6k   57   '   59      5;      5<      5>      �� 5?   
    � �� 5@      �� 5A      " ��6"   .R�5B      � ���#d6d6d6 5C      �� 5D      $
��0.7* �6/   �� 5E      $
��0.3* �6/   $��30��6<   .+�5G      �� $ cbxText��6V   $��0��6#   $��Center=�63   # z 6g   ��6�   �� *
Foreground+$ TwoWay=�$ PropertyChanged=���6�   �� *
BookName+$ TwoWay=�$ PropertyChanged=�63  �� 5H      $ dropText��62   $��0��6"   $��1,1,0,1q�6D   ���#d6d6d6$ 1,0,0,1��6t   5J      �� 5K      $ dropRightBtn��62   $��1��6!   $��0,1,1,1q�6J   ���#d6d6d6$ 0,1,1,0��6z   ._�5L      d� 8��  $ Press=�6�    6S   �� $ IsDropDownOpen�$ TwoWay=� �� *TemplatedParent+6�   5M      � 5N      $ Popup��6]   #��z 6    $��Bottom=�6J   $|�False��6l   $
 True��6~   $��Slide=�6�   .��5O      �� $ DropDown��6�   $ 1��6%   #��z 66   #��z 6f   $
 True��6�    5P   "      5Q   &   $ Black��67   $ 2Q�6E   $  0Q�6T   $	! 0.5Q�6d   ._�5S   "   �� $��4,6,4,6q�6/   #���  6@   #��z 6l   $
 True��6�   $
��Auto=�6�   $
��Auto=�6�   $
��True��6�   .��5U   &   �� $
" True��61   $# 	Contained=�6D   
�������6y   5V   #   5W      5X      5Y       5[      P� 5]       �@J�IsMouseOver$
	 True��69   . �5^      �� $
 dropRightBtn��6%    � �� �#3798f96h   �� 5_      $
 dropText��6%    � �� �#3798f96d   5`      5d      5f      5g      �� 5k      "  ��6   .�5l   
   ��  � �� 5m      �� 5n      " ��6"   .R�5o      � $
$ Grid��6   #��z��6)    5p      �� 5q      $��* �6/   �� 5r      $
��Auto �6/   % 5t      �� 5u      $��* �6,   �� 5v      $
��Auto �6,   .+�5y      �� $& Corner��6$   $��1��65   $��1��6B   #��� ��6R   �� 5{      $!' PART_ScrollContentPresenter��61   $��0��6V   $��0��6c   #��z��6s   #( z��6�   #��z��6�   #��z��6  �� 5}      $) PART_VerticalScrollBar��6$   $��0��6F   $��1��6S   $* VerticalScrollBar��6c   $+ Arrow��6�   #- z, 6�   $. 0Q�6�   #y�z/ 6�   #1 z0 6�  #���  6�  2 6(  �� *VerticalOffset+$ OneWay=� �� *TemplatedParent+6�  �� 5      $) PART_HorizontalScrollBar��6$   $��1��6F   $��0��6S   $* HorizontalScrollBar��6c   $+ Arrow��6�   #- z3 6�   $. 0Q�6�   #y�z4 6�   #1 z5 6�  #���  6�  $6 
Horizontal=�6�  2 6(  �� *HorizontalOffset+$ OneWay=� �� *TemplatedParent+6�  5�       5�      P� 5�       � ��$	 Disabled=�6K   5�      5�      5�      5�      �� 5�      "  ��6   .�5�   
   ��  � ��$ Right=�60   �� 5�   
    � ��$	 Top=�6.   �� 5�   
    � ��$ 22��6#   �� 5�   
    � ��$ 27��6"   5�      5�      