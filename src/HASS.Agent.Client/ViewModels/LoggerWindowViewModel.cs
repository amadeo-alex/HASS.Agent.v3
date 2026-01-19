using LogViewer.Core.ViewModels;

namespace HASS.Agent.Client.ViewModels;

public class LoggerWindowViewModel
{
    public LogViewerControlViewModel LogViewer { get; }
    
    public LoggerWindowViewModel(LogViewerControlViewModel logViewer)
    {
        LogViewer = logViewer;
    }
}