   M S B A M L   `   `   `  ����N  JWindowInspector.App, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null9!clr-namespace:WindowInspector.AppWindowInspector.App  5   	   O,clr-namespace:WindowInspector.App.ConvertersWindowInspector.App.Converters  5   	   O,clr-namespace:WindowInspector.App.ViewModelsWindowInspector.App.ViewModels  5   	   $    WindowInspector.App.MainWindow   5   	   W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5   	   R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   3local!clr-namespace:WindowInspector.App     5   	   C
converters,clr-namespace:WindowInspector.App.Converters     5   	   C
viewModels,clr-namespace:WindowInspector.App.ViewModels     5   	     � Title$  Window Inspector��5
   	   $	��400��5   	   $	��500��5   	    � WindowStyle$ 
ToolWindow=�5   	    � Topmost$
 True��5   	    � 
ResizeMode$ 	CanResize=�5   	    J� InputBindings 5      �� 5   
   
 �� Key$ Space��6    �� Command 6!   �� *TogglePauseCommand+�� 5   
   $ C��6    �� 	Modifiers$ Controlf�6    61   �� *CopyToClipboardCommand+ � 	Resources 5         BoolToVisibilityConverter:   4WindowInspector.App.Converters.BoolToStringConverter  BoolToTooltipConverter	   	TrueValue
   
FalseValue  BoolToPauseTextConverter��%�   &	        &	      &	 �     �� 5   
   6'     5   
   $0	 *Resume tracking (currently paused) [Space]��5   )   $
 Pause tracking [Space]��5   )     5   
   $	 Resume��5    )   $
 Pause��5!   )   5"       � DataContext 5$      8   2WindowInspector.App.ViewModels.MainWindowViewModel  5%   
   .��5(      � $��10q�6    � RowDefinitions 5)   
   �� 5*      $��* �6   �� 5+      $
��Auto �6   .+�5/   
   �� $��0��6   ���#DDDDDD$��1q�51      $��0,0,0,10q�52      ._�53      � .+�55      �� ���	#80FFFFFFy�56      �� *
IsUpdating+ �� 	Converter# [  ._�57      �� $��Updating...��6    $��Center=�58      $��Center=�59      $��16(�5:      ���#6666665;      5<      �� 5?      -   5         ��Name$ HierarchyTextBox��5?       �� 
IsReadOnly$
 True��5A       �� TextWrapping$
 Wrap=�5B      " �� VerticalScrollBarVisibility$
 Auto=�5C      $ �� HorizontalScrollBarVisibility$
 Auto=�5D      $��Consolas)�5E      $��8q�5F      ���#FAFAFA$��0q�5H      ��5@      �� *HierarchyText+ �� Mode$ OneWay=�.��5H   .   ��5I      �� 5J       �� 
TargetType" ��6     �� Triggers 5K      P� 5L   "    P� Property �@��
IsReadOnly P� Value$
 True��6@   . �5M   &   ��  �� Property � �� �� Value �#3333336C   5N   #   �� 5O   "    �� Binding 6.   �� *
IsUpdating+ �� Value True6M   .a�5P   &   ��  �@J�Opacity$	 0.7Q�6@   5Q   #   5S      .��5T   %   5U      5V      5W      � 5Z   
   $��1��6    � ColumnDefinitions 5[      �� 5\      $��* �6#   �� 5]      $
��Auto �6#   �� 5^      $
��Auto �6#   .+�5b      �� $��0��6   ���#F5F5F5���#DDDDDD$��1q�5e       �� Padding$ 5q�5f      $��0,0,10,0q�5g      ._�5h      �� $
��Wrap=�5i      $��Center=�5j      ��5h      �� *
StatusText+��5k      �� 5l      " ��6     5m      �� 5n   "    6.   �� *
StatusText+ Copied to clipboard!6M   .a�5o   &   ��  � �� �#28a7456C   5p   #   �� 5q   "    6.   �� *
StatusText+  Resuming element tracking...6M   .a�5r   &   ��  � �� �#17a2b86C   5s   #   ^� 5t   "    ^� 
Conditions 5u   &   �� 5v   *    �� Binding 64   �� *
StatusText+  �� Value7  3Tracking paused. Click Resume to continue tracking.6S   �� 5w   *    64   �� *
IsPaused+  True6Q   .0�5y   &   ��  � �� �#ffc1076C   5z   #   �� 5{   "    6.   �� *
StatusText+	 Error6M   .a�5|   &   ��  � �� �#dc35456C   5}   #   5      5�      5�      �� 5�      $��1��6   $	��8,5q�5�      $��0,0,10,0q�5�      ��5�      �� *TogglePauseCommand+! � ToolTip! 5�      �� *
IsPaused+# [ .��5�      �� " �� Orientation$" 
Horizontal=�6   .+�5�      F� $��16��6   $��16��5�      # �� Stretch$# Uniform=�5�      $��0,0,5,0q�5�      ���#333333��5�      �� 5�      " F�6$    5�   "   �� 5�   &    62   �� *
IsPaused+ True6O   .a�5�   *   ��  � ��+ �� z  nN � z �$���!�=� � z  nN 	6A   5�   '   �� 5�   &    62   �� *
IsPaused+	 False6O   .a�5�   *   ��  � ��X �Ѐ�� ��!р���!р�@KL р�� @KL ���[ ��!р�� ��!р�� @KL р�[ @KL р�[ ��!	6A   5�   '   5�      5�      �� 5�      $��Center=�5�      ��5�       �� *
IsPaused+# [ 5�      5�      5�      �� 5�      $��2��6   $0! *Copy element details to clipboard [Ctrl+C]��5�      $	��8,5q�5�      ��5�      �� *CopyToClipboardCommand+.��5�      �� $" 
Horizontal=�6   .+�5�      F� �������!@o@� z @o@� z ��j ���!��j ���!@KL � z @KL ׀�[ ��j  �� �� р�[ @o@� z ��^ �� �� ���!��^�@o@@o@ �� �� �@o@��j ���!@KL  �� �� � $� @B � 	= @B ׀� ��-  �� �� р� @f� 	= @f� 	= ��- � $� ��- � $� @B 	���#333333$��16��5�      $��16��5�      $# Uniform=�5�      $��0,0,5,0q�5�      �� 5�      $
��Copy��6    $��Center=�5�      5�      5�      5�      5�      5�      