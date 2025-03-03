using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HASS.Agent.Contracts.Models.MediaPlayer;
using MQTTnet;

namespace HASS.Agent.Contracts.Managers;
public interface IMediaManager
{
    void HandleReceivedCommand(MediaPlayerCommand command);
}
