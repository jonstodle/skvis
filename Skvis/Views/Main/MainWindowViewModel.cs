using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using DynamicData;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Skvis.ViewModels;

namespace Skvis.Views.Main;

public class MainWindowViewModel : ViewModelBase
{
	public MainWindowViewModel()
	{
		this.WhenActivated(d =>
		{
			_storageFiles
				.Connect()
				.Transform(isf => new File(
					isf.Name,
					new FileInfo(isf.Path.AbsolutePath).DirectoryName,
					isf))
				.ObserveOn(RxApp.MainThreadScheduler)
				.Bind(out _files)
				.Subscribe(Observer.Create<IChangeSet<File>>(_ => { }))
				.DisposeWith(d);
		});
		AddFileCommand = ReactiveCommand.CreateFromTask<IStorageItem>(AddFile);
		RemoveFileCommand = ReactiveCommand.Create(RemoveFile);
		SqueezeCommand = ReactiveCommand.CreateFromTask(Squeeze);
	}

	private ReadOnlyObservableCollection<File> _files = default!;
	public ReadOnlyObservableCollection<File> Files => _files;

	[Reactive]
	public File? SelectedFile { get; set; }

	[Reactive]
	public int Quality { get; set; }

	[Reactive]
	public int Percent { get; set; }

	public ReactiveCommand<IStorageItem, Unit> AddFileCommand { get; }

	public ReactiveCommand<Unit, Unit> RemoveFileCommand { get; }

	public ReactiveCommand<Unit, Unit> SqueezeCommand { get; }

	private SourceList<IStorageFile> _storageFiles = new();

	private async Task AddFile(IStorageItem storageItem)
	{
		async Task<List<IStorageFile>> GetFiles(
			List<IStorageFile> files,
			IStorageFolder folder)
		{
			await foreach (var item in folder.GetItemsAsync())
			{
				switch (item)
				{
					case IStorageFile file:
						files.Add(file);
						break;
					case IStorageFolder subFolder:
						await GetFiles(files, subFolder);
						break;
				}
			}

			return files;
		}

		switch (storageItem)
		{
			case IStorageFile file:
				_storageFiles.Add(file);
				break;
			case IStorageFolder folder:
				_storageFiles.AddRange(await GetFiles(new(), folder));
				break;
		}
	}

	private void RemoveFile()
	{
		if (SelectedFile is not { } file)
		{
			return;
		}

		_storageFiles.Remove(file.StorageFile);
	}

	private Task Squeeze() => Task.Delay(3000);
}

public record File(string Name, string? DirectoryPath, IStorageFile StorageFile);
