   M S B A M L   `   `   `  ���� N  JWindowInspector.App, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null9!clr-namespace:WindowInspector.AppWindowInspector.App  O,clr-namespace:WindowInspector.App.ConvertersWindowInspector.App.Converters  O,clr-namespace:WindowInspector.App.ViewModelsWindowInspector.App.ViewModels  $    WindowInspector.App.MainWindow   W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     @d2http://schemas.microsoft.com/expression/blend/2008     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     3local!clr-namespace:WindowInspector.App     C
converters,clr-namespace:WindowInspector.App.Converters     C
viewModels,clr-namespace:WindowInspector.App.ViewModels       � Title$  Window Inspector��$	��400��$	��500�� � WindowStyle$ 
ToolWindow=� � Topmost$
 True�� � 
ResizeMode$ 	CanResize=� J� InputBindings �� 
 �� Key$ Space�� �� Command �� *TogglePauseCommand+�� $ C�� �� 	Modifiers$ Controlf� �� *CopyToClipboardCommand+ � 	Resources    BoolToVisibilityConverter:   4WindowInspector.App.Converters.BoolToStringConverter  BoolToTooltipConverter	   	TrueValue
   
FalseValue  BoolToPauseTextConverter��%�   &	        &	      &	 X     ��   $0	 *Resume tracking (currently paused) [Space]��$
 Pause tracking [Space]��  $	 Resume��$
 Pause�� � DataContext 8   2WindowInspector.App.ViewModels.MainWindowViewModel  .��� $��10q� � RowDefinitions �� $��* ��� $
��Auto �.+��� $��0�����#DDDDDD$��1q�$��0,0,0,10q�._�� .+��� ���	#80FFFFFFy��� *
IsUpdating+ �� 	Converter# [  ._��� $��Updating...��$��Center=�$��Center=�$��16(����#666666�� -    ��Name$ HierarchyTextBox�� �� 
IsReadOnly$
 True�� �� TextWrapping$
 Wrap=�" �� VerticalScrollBarVisibility$
 Auto=�$ �� HorizontalScrollBarVisibility$
 Auto=�$��Consolas)�$��8q����#FAFAFA$��0q����� *HierarchyText+ �� Mode$ OneWay=�.������  �� 
TargetType" �� �� Triggers P�  P� Property �@��
IsReadOnly P� Value$
 True��. ���  �� Property � �� �� Value �#333333��  �� Binding �� *
IsUpdating+ �� Value True.a���  �@J�Opacity$	 0.7Q�.��� $��1�� � ColumnDefinitions �� $��* ��� $
��Auto ��� $
��Auto �.+��� $��0�����#F5F5F5���#DDDDDD$��1q� �� Padding$ 5q�$��0,0,10,0q�._��� $
��Wrap=�$��Center=����� *
StatusText+���� " �� ��  �� *
StatusText+ Copied to clipboard!.a���  � �� �#28a745��  �� *
StatusText+  Resuming element tracking....a���  � �� �#17a2b8^�  ^� 
Conditions ��  �� Binding �� *
StatusText+  �� Value7  3Tracking paused. Click Resume to continue tracking.��  �� *
IsPaused+  True.0���  � �� �#ffc107��  �� *
StatusText+	 Error.a���  � �� �#dc3545�� $��1��$	��8,5q�$��0,0,10,0q����� *TogglePauseCommand+! � ToolTip! �� *
IsPaused+# [ .���� " �� Orientation$" 
Horizontal=�.+�F� $��16��$��16��# �� Stretch$# Uniform=�$��0,0,5,0q����#333333���� " F� ��  �� *
IsPaused+ True.a���  � ��+ �� z  nN � z �$���!�=� � z  nN 	��  �� *
IsPaused+	 False.a���  � ��X �Ѐ�� ��!р���!р�@KL р�� @KL ���[ ��!р�� ��!р�� @KL р�[ @KL р�[ ��!	�� $��Center=����� *
IsPaused+# [ �� $��2��$0! *Copy element details to clipboard [Ctrl+C]��$	��8,5q����� *CopyToClipboardCommand+.���� $" 
Horizontal=�.+�F� �������!@o@� z @o@� z ��j ���!��j ���!@KL � z @KL ׀�[ ��j  �� �� р�[ @o@� z ��^ �� �� ���!��^�@o@@o@ �� �� �@o@��j ���!@KL  �� �� � $� @B � 	= @B ׀� ��-  �� �� р� @f� 	= @f� 	= ��- � $� ��- � $� @B 	���#333333$��16��$��16��$# Uniform=�$��0,0,5,0q��� $
��Copy��$��Center=�