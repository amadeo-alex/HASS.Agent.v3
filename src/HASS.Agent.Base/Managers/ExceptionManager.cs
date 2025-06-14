using System.Net.Sockets;
using System.Net;
using System.Runtime.ExceptionServices;
using HASS.Agent.Contracts.Managers;
using MQTTnet.Exceptions;
using Microsoft.Extensions.Logging;

namespace HASS.Agent.Base.Managers;
public class ExceptionManager : IExceptionManager
{
    private readonly ILogger<ExceptionManager> _logger;
    private readonly List<string> LoggedFirstChanceHttpRequestExceptions = new();

    private string _lastLog = string.Empty;

    public ExceptionManager(ILogger<ExceptionManager> logger)
    {
        _logger = logger;
    }

    public void OnFirstChanceExceptionHandler(object? sender, FirstChanceExceptionEventArgs? eventArgs)
    {
        try
        {
            if (eventArgs == null)
            {
                _logger.LogCritical("[PROGRAM] Error processing FirstChanceException, eventArgs are null");
                return;
            }

            if (!string.IsNullOrEmpty(_lastLog))
            {
                if (_lastLog == eventArgs.Exception.ToString()
                    || eventArgs.Exception.ToString().Contains(_lastLog))
                {
                    return;
                }
            }

            _lastLog = eventArgs.Exception.ToString();

            switch (eventArgs.Exception)
            {
                case FileNotFoundException fileNotFoundException:
                    HandleFirstChanceFileNotFoundException(fileNotFoundException);
                    return;

                case SocketException socketException:
                    HandleFirstChanceSocketException(socketException);
                    return;

                case WebException webException:
                    HandleFirstChanceWebException(webException);
                    return;

                case HttpRequestException httpRequestException:
                    HandleFirstChanceHttpRequestException(httpRequestException);
                    return;

                case MqttCommunicationException mqttCommunicationException:
                    HandleFirstChanceMqttCommunicationException(mqttCommunicationException);
                    return;

                default:
                    _logger.LogCritical(eventArgs.Exception, "[PROGRAM] FirstChanceException: {err}", eventArgs.Exception.Message);
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "[PROGRAM] Error processing FirstChanceException: {err}", ex.Message);
        }
    }

    private void HandleFirstChanceFileNotFoundException(FileNotFoundException fileNotFoundException)
    {
        if (fileNotFoundException.FileName != null && fileNotFoundException.FileName.Contains("resources"))
        {
            return;
        }

        _logger.LogError("[PROGRAM] FileNotFoundException: {err}", fileNotFoundException.Message);
    }

    private void HandleFirstChanceSocketException(SocketException socketException)
    {
        var socketErrorCode = socketException.SocketErrorCode;
        switch (socketErrorCode)
        {
            case SocketError.ConnectionRefused:
                _logger.LogError("[NET] [{type}] {err}", socketErrorCode.ToString(), socketException.Message);
                return;

            case SocketError.ConnectionReset:
                _logger.LogError("[NET] [{type}] {err}", socketErrorCode.ToString(), socketException.Message);
                return;

            default:
                _logger.LogError(socketException, "[NET] [{type}] {err}", socketErrorCode.ToString(), socketException.Message);
                break;
        }
    }

    private void HandleFirstChanceWebException(WebException webException)
    {
        var status = webException.Status;
        switch (status)
        {
            case WebExceptionStatus.ConnectFailure:
                _logger.LogError("[NET] [{type}] {err}", status.ToString(), webException.Message);
                return;

            case WebExceptionStatus.Timeout:
                _logger.LogError("[NET] [{type}] {err}", status.ToString(), webException.Message);
                return;

            default:
                _logger.LogError(webException, "[NET] [{type}] {err}", status.ToString(), webException.Message);
                return;
        }
    }

    private void HandleFirstChanceHttpRequestException(HttpRequestException httpRequestException)
    {
        if (httpRequestException.InnerException != null)
        {
            switch (httpRequestException.InnerException)
            {
                case SocketException sE:
                    HandleFirstChanceSocketException(sE);
                    break;
                case WebException wE:
                    HandleFirstChanceWebException(wE);
                    break;
            }
        }

        var excMsg = httpRequestException.ToString();
        if (LoggedFirstChanceHttpRequestExceptions.Contains(excMsg))
        {
            return;
        }

        LoggedFirstChanceHttpRequestExceptions.Add(excMsg);

        if (excMsg.Contains("SocketException"))
        {
            _logger.LogError("[NET] [{type}] {err}", "FirstChanceHttpRequestException.SocketException", httpRequestException.Message);
            return;
        }

        _logger.LogCritical(httpRequestException, "[NET] FirstChanceHttpRequestException: {err}", httpRequestException.Message);
    }

    private void HandleFirstChanceMqttCommunicationException(MqttCommunicationException mqttCommunicationException)
    {
        if (mqttCommunicationException.InnerException != null)
        {
            switch (mqttCommunicationException.InnerException)
            {
                case SocketException sE:
                    HandleFirstChanceSocketException(sE);
                    break;
                case WebException wE:
                    HandleFirstChanceWebException(wE);
                    break;
            }
        }

        var excMsg = mqttCommunicationException.ToString();
        if (excMsg.Contains("SocketException"))
        {
            _logger.LogError("[NET] [{type}] {err}", "MqttCommunicationException.SocketException", mqttCommunicationException.Message);
            return;
        }
        if (excMsg.Contains("OperationCanceledException"))
        {
            _logger.LogError("[NET] [{type}] {err}", "MqttCommunicationException.OperationCanceledException", mqttCommunicationException.Message);
            return;
        }

        _logger.LogCritical(mqttCommunicationException, "[NET] FirstChancemqttCommunicationException: {err}", mqttCommunicationException.Message);
    }
}
