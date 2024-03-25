using ReactiveUI;

namespace Skvis.ViewModels;

public class ViewModelBase : ReactiveObject, IActivatableViewModel
{
	public ViewModelActivator Activator { get; } = new();
}
