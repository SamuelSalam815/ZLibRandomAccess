using System.Windows;
using System.Windows.Controls;

namespace ArchiveAccessPointVisualizer;

public partial class FilePathFieldUserControl : UserControl
{
    public static readonly DependencyProperty LabelProperty = DependencyProperty.Register(
        nameof(Label),
        typeof(string),
        typeof(FilePathFieldUserControl),
        new PropertyMetadata("Label"));

    public string Label
    {
        get => (string)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
        nameof(Value),
        typeof(string),
        typeof(FilePathFieldUserControl),
        new PropertyMetadata(default(string)));

    public string Value
    {
        get => (string)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public static readonly DependencyProperty ButtonLabelProperty = DependencyProperty.Register(
        nameof(ButtonLabel),
        typeof(string),
        typeof(FilePathFieldUserControl),
        new PropertyMetadata("📂"));

    public string ButtonLabel
    {
        get => (string)GetValue(ButtonLabelProperty);
        set => SetValue(ButtonLabelProperty, value);
    }

    public static readonly DependencyProperty TextBoxToLabelWidthRatioProperty = DependencyProperty.Register(
        nameof(TextBoxToLabelWidthRatio),
        typeof(string),
        typeof(FilePathFieldUserControl),
        new PropertyMetadata("6*"));

    public string TextBoxToLabelWidthRatio
    {
        get => (string)GetValue(TextBoxToLabelWidthRatioProperty);
        set => SetValue(TextBoxToLabelWidthRatioProperty, value);
    }

    public static readonly DependencyProperty OnButtonClickProperty = DependencyProperty.Register(
        nameof(OnButtonClick),
        typeof(EventHandler<RoutedEventArgs>),
        typeof(FilePathFieldUserControl),
        new PropertyMetadata(default(EventHandler<RoutedEventArgs>)));

    public EventHandler<RoutedEventArgs> OnButtonClick
    {
        get => (EventHandler<RoutedEventArgs>)GetValue(OnButtonClickProperty);
        set => SetValue(OnButtonClickProperty, value);
    }

    public FilePathFieldUserControl()
    {
        InitializeComponent();
    }
}
