using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.ReactiveUI;
using ReactiveUI;

namespace Skvis.Views.Main;

public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
{
	public MainWindow()
	{
		InitializeComponent();

		QualitySliderLabel.Content = Strings.MainWindow.QualitySliderLabel;
		SqueezeButton.Content = Strings.MainWindow.SqueezeButton;

		this.WhenActivated(d =>
		{
			this.OneWayBind(ViewModel,
					vm => vm.Files,
					v => v.FilesListBox.ItemsSource)
				.DisposeWith(d);
			this.Bind(ViewModel,
					vm => vm.SelectedFile,
					v => v.FilesListBox.SelectedItem)
				.DisposeWith(d);
			this.Bind(ViewModel,
					vm => vm.Quality,
					v => v.QualitySlider.Value)
				.DisposeWith(d);
			this.OneWayBind(ViewModel,
					vm => vm.Quality,
					v => v.QualitySliderValueLabel.Content)
				.DisposeWith(d);
			this.BindCommand(ViewModel,
				vm => vm.SqueezeCommand,
				v => v.SqueezeButton);
			Observable.CombineLatest(
					ViewModel.AddFileCommand.IsExecuting,
					ViewModel.SqueezeCommand.IsExecuting,
					(afc, sc) => !(afc && sc))
				.BindTo(this, v => v.SqueezeButton.IsEnabled)
				.DisposeWith(d);
		});

		AddHandler(DragDrop.DropEvent, Drop);
	}

	private void Drop(object? sender, DragEventArgs e)
	{
		if (!e.Data.Contains(DataFormats.Files) || e.Data.GetFiles() is not { } files)
		{
			return;
		}

		files.ToObservable()
			.InvokeCommand(ViewModel!.AddFileCommand);
	}
}
