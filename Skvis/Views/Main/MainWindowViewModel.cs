using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Web;
using Avalonia.Platform.Storage;
using DynamicData;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
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
				.Sort(FileComparer.Instance)
				.ObserveOn(RxApp.MainThreadScheduler)
				.Bind(out _files)
				.Subscribe(Observer.Create<IChangeSet<File>>(_ => { }))
				.DisposeWith(d);
		});
		Quality = 75;
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

	public ReactiveCommand<IStorageItem, Unit> AddFileCommand { get; }

	public ReactiveCommand<Unit, Unit> RemoveFileCommand { get; }

	public ReactiveCommand<Unit, Unit> SqueezeCommand { get; }

	private SourceList<File> _storageFiles = new();

	private static readonly string[] ImageExtensions = [".jpg", ".jpeg", ".png", ".gif", ".bmp", ".tiff"];

	private async Task AddFile(IStorageItem storageItem)
	{
		bool IsImageFile(string fileName) =>
			ImageExtensions.Contains(Path.GetExtension(fileName).ToLower());

		async Task<List<File>> GetFiles(
			List<File> files,
			IStorageFolder folder)
		{
			await foreach (var item in folder.GetItemsAsync())
			{
				switch (item)
				{
					case IStorageFile file:
						if (IsImageFile(file.Name))
						{
							files.Add(File.From(file));
						}

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
				if (IsImageFile(file.Name))
				{
					_storageFiles.Add(File.From(file));
				}

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

		_storageFiles.Remove(file);
	}

	private async Task Squeeze()
	{
		foreach (var file in _storageFiles.Items)
		{
			file.Status = FileConversionStatus.Converting;
			try
			{
				var originalPath = HttpUtility.UrlDecode(file.StorageFile.Path.AbsolutePath);
				using var image = await Image.LoadAsync(originalPath);

				var newPath = Path.ChangeExtension(originalPath, "webp");
				await image.SaveAsWebpAsync(newPath, new WebpEncoder { Quality = Quality, });

				file.Status = FileConversionStatus.Converted;
			}
			catch (Exception)
			{
				file.Status = FileConversionStatus.NotConverted;
				throw;
			}
		}
	}
}

public enum FileConversionStatus
{
	NotConverted,
	Converting,
	Converted,
}

public class File : ReactiveObject
{
	public required string Name { get; init; }
	public string? DirectoryPath { get; init; }

	[Reactive]
	public FileConversionStatus Status { get; set; }

	public required IStorageFile StorageFile { get; init; }

	public static File From(IStorageFile isf) => new()
	{
		Name = isf.Name,
		DirectoryPath = new FileInfo(HttpUtility.UrlDecode(isf.Path.AbsolutePath)).DirectoryName,
		StorageFile = isf
	};
};

public class FileComparer : IComparer<File>
{
	public static FileComparer Instance { get; } = new();

	public int Compare(File? x, File? y)
	{
		if (ReferenceEquals(x, y)) return 0;
		if (ReferenceEquals(null, y)) return 1;
		if (ReferenceEquals(null, x)) return -1;
		var directoryComparison = string.Compare(x.DirectoryPath, y.DirectoryPath, StringComparison.Ordinal);
		if (directoryComparison != 0) return directoryComparison;
		return string.Compare(x.Name, y.Name, StringComparison.Ordinal);
	}
}
