   M S B A M L   `   `   `  ����F  BMyAssistant, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null)clr-namespace:MyAssistantMyAssistant  5   	       MyAssistant.AddTask   5   	   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   +localclr-namespace:MyAssistant     5   	     � Title$  添加任务��5   	   $	��300��6   $	��500��6#    � WindowStartupLocation$ CenterScreen=�6/    � 	Resources 5	         DisabledForegroundColor �� 
TargetType �� Property �� Value �� 
TargetType �� Triggers P� Property	 P� Value
 �� Color  BtnInfoStyle ��Name J� SnapsToDevicePixels �� 
TargetName-  'System.Windows.Input.KeyboardNavigation ��Name��%
  &	        '�� !     &	     '�� �    ��5      	#FF888888��6=   �� 5      " ��5      .�5      ��  � ��$
 Left=�6>   �� 5       � ��$ Center=�6<   �� 5       � ��$ 18(�6,   �� 5       � ��$
 20 0q�6*   �� 5       � �� 5      �� 5      " ��6+   .R�5       �� ._�5   %   �� #��z��66   #��z��5      $
��True��5      5   !    5       P� 5   %    � {�$	 false��5      . �5   *   ��  � �� 5    /   �� 5!   4   #
 �   6D   5#   +   5$   &   5&      5(      5)      �� 5+      " ��6'   .�5,      ��  � ��$	 120��6)   �� 5-       � ��$ 40��6*   �� 5.       � ��
 �����6.   �� 5/       � ��$ 0q�63   �� 50       � �� �#43a9c76.   �� 51       � �� 52      �� 53      " ��6+   .R�54       �� $ border��6'   #��z��67   #��z��6a   #��z��6�   $
 True��6�   ._�55   %   �� #��z��6/   #��z��6P   $��Center=�6z   $��Center=�6�   56   !    57       P� 58   %    �@J�IsMouseOver$
	 True��6D   . �59   *   �� $ border��61    � �� �#2f96b46[   5:   &   P� 5;   %    � ��$
	 True��6B   . �5<   *   �� $ border��61    � �� �#2a89a46[   5=   &   5?      5A      5B      �� 5D      " ��6+   .�5E      ��  � �� 6.   Z� �� 5F       � �� 6/   Z� �� 5G       � ��
 �   �6.   �� 5H       � ��$ 18(�6,   �� 5I       �@ TabNavigation$
 None=�6D   �� 5J       � ��$ 1q�63   �� 5K       � ��$
 Left=�6>   �� 5L       � ��$ Center=�6<   �� 5M       � ��$ 0.5,0q�6+   �� 5N       �@�FocusVisualStyle 64   Z� �� 5O       �@J�	AllowDrop$
 true��6-   �� 5P       � �� 5Q      �� 5R      " ��6+   .R�5S       �� $ 
BackBorder��6'   $
 true��6;   #��z��6V   ��6�   Z� ��6�   Z� ._�5T   %   �� $ PART_ContentHost��62   # z 6L   5U   !    5V       P� 5W   %    � {�$	 false��6B   . �5X   *   �� $ 
BackBorder��6G    6_   Z�  � ���� 5Y   *    � �� �	#CCFFFFFF6G   5Z   &   P� 5[   %    �@J�IsMouseOver$
	 True��6D   . �5\   *   �� $ 
BackBorder��6G    � �� �	#30FFFFFF6_   �� 5]   *   $ 
BackBorder��6H    � �� �	#30FFFFFF6`   5^   &   P� 5_   %    �@J�	IsFocused$
	 True��6B   . �5`   *   �� $ 
BackBorder��6G    � �� �#FAF0E66_   �� 5a   *   $ 
BackBorder��6H    � �� �	#50FFFFFF6`   5b   &   5d      5f      5g      5i      .��5k      �  � RowDefinitions 5l      �� 5m      6!   �� 5n      $��2* �6   6-   �� 5o      6!   �� 5p      6!    � ColumnDefinitions 5t      �� 5u      �� 5v      $��2* �6"   .+�5z      �� .��6   	名称：6   �� 5{      -   5         ��Name$ txtTaskName��5{   $   $��1��6   ��6:   �� *Name+ �� Mode$ TwoWay=� �� UpdateSourceTrigger$ PropertyChanged=�6�   �� 5}      $��1��6   .��6   	描述：6$   �� 5~      -   5        $ txtDescription��5~   2   $��1��6   $��1��6"    �� TextWrapping$
 Wrap=�6J    �� AcceptsReturn$
 True��6^   " �� VerticalScrollBarVisibility$ Visible=�5      ��6;   �� *Description+$ TwoWay=�$ PropertyChanged=��� 5�      $��2��6   .��6   计划完成时间：6(   (  "System.Windows.Controls.DatePicker  5�      -   5          Name$ dpkPlanTime��5�      $��2��6,   $��1��69      Text 6J   �� *PlanFinishDateStr+$ TwoWay=�$ PropertyChanged=��� 5�      -   5         ��Name$ btnOk��5�   -   $��3��6   $��确定��6    #��[ 6=   6c   5�      5�      