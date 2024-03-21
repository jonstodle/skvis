using Avalonia.Controls;

namespace Skvis.Views;

public partial class MainWindow : Window
{
	public MainWindow()
	{
		InitializeComponent();

		QualitySliderLabel.Content = Strings.MainWindow.QualitySliderLabel;
		PercentSliderLabel.Content = Strings.MainWindow.PercentSliderLabel;
		SqueezeButton.Content = Strings.MainWindow.SqueezeButton;
	}
}
