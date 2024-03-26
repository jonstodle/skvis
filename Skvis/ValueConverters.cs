using System;
using Avalonia.Data.Converters;
using Skvis.Views.Main;

namespace Skvis;

public static class ValueConverters
{
	public static FuncValueConverter<FileConversionStatus, string> ConversionStatusToString { get; } =
		new FuncValueConverter<FileConversionStatus, string>(status => status switch
		{
			FileConversionStatus.NotConverted => "",
			FileConversionStatus.Converting => "⏲",
			FileConversionStatus.Converted => "✅",
			_ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
		});
}
