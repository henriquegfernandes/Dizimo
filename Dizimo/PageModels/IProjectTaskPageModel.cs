using CommunityToolkit.Mvvm.Input;
using Dizimo.Models;

namespace Dizimo.PageModels;

public interface IProjectTaskPageModel
{
    IAsyncRelayCommand<ProjectTask> NavigateToTaskCommand { get; }
    bool IsBusy { get; }
}