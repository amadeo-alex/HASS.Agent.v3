namespace HASS.Agent.Client.ViewModels;

public interface INavigationAware
{
    void OnNavigatedTo();

    void OnNavigatedFrom();
}