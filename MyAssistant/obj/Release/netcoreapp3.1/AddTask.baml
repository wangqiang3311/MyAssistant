   M S B A M L   `   `   `  ���� F  BMyAssistant, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null)clr-namespace:MyAssistantMyAssistant      MyAssistant.AddTask   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     @d2http://schemas.microsoft.com/expression/blend/2008     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     +localclr-namespace:MyAssistant       � Title$  添加任务��$	��300��$	��500�� � WindowStartupLocation$ CenterScreen=� � 	Resources    DisabledForegroundColor �� 
TargetType �� Property �� Value �� 
TargetType �� Triggers P� Property	 P� Value
 �� Color  BtnInfoStyle ��Name J� SnapsToDevicePixels �� 
TargetName-  'System.Windows.Input.KeyboardNavigation ��Name��%�  &	        '��      &	     '�� �    ��	#FF888888���� " ��.���  � ��$
 Left=���  � ��$ Center=���  � ��$ 18(���  � ��$
 20 0q���  � �� �� " ��.R��� ._��� #��z��#��z��$
��True�� P�  � {�$	 false��. ���  � �� �� #
 �   �� " ��.���  � ��$	 120����  � ��$ 40����  � ��
 �������  � ��$ 0q���  � �� �#43a9c7��  � �� �� " ��.R��� $ border��#��z��#��z��#��z��$
 True��._��� #��z��#��z��$��Center=�$��Center=� P�  �@J�IsMouseOver$
	 True��. ��� $ border�� � �� �#2f96b4P�  � ��$
	 True��. ��� $ border�� � �� �#2a89a4�� " ��.���  � �� Z� ��  � �� Z� ��  � ��
 �   ���  � ��$ 18(���  �@ TabNavigation$
 None=���  � ��$ 1q���  � ��$
 Left=���  � ��$ Center=���  � ��$ 0.5,0q���  �@�FocusVisualStyle Z� ��  �@J�	AllowDrop$
 true����  � �� �� " ��.R��� $ 
BackBorder��$
 true��#��z����Z� ��Z� ._��� $ PART_ContentHost��# z  P�  � {�$	 false��. ��� $ 
BackBorder�� Z�  � ����  � �� �	#CCFFFFFFP�  �@J�IsMouseOver$
	 True��. ��� $ 
BackBorder�� � �� �	#30FFFFFF�� $ 
BackBorder�� � �� �	#30FFFFFFP�  �@J�	IsFocused$
	 True��. ��� $ 
BackBorder�� � �� �#FAF0E6�� $ 
BackBorder�� � �� �	#50FFFFFF.���  � RowDefinitions �� �� $��2* ��� ��  � ColumnDefinitions �� �� $��2* �.+��� .��	名称：�� -    ��Name$ txtTaskName��$��1������ *Name+ �� Mode$ TwoWay=� �� UpdateSourceTrigger$ PropertyChanged=��� $��1��.��	描述：�� -   $ txtDescription��$��1��$��1�� �� TextWrapping$
 Wrap=� �� AcceptsReturn$
 True��" �� VerticalScrollBarVisibility$ Visible=����� *Description+$ TwoWay=�$ PropertyChanged=��� $��2��.��计划完成时间：(  "System.Windows.Controls.DatePicker  -     Name$ dpkPlanTime��$��2��$��1��   Text �� *PlanFinishDateStr+$ TwoWay=�$ PropertyChanged=��� -    ��Name$ btnOk��$��3��$��确定��#��[ 