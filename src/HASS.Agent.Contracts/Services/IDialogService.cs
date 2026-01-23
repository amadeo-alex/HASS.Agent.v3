namespace HASS.Agent.Contracts.Services;

public interface IDialogService
{
	Task<T> ShowDialogAsync<T>(T viewModel);
}