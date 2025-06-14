using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading.Tasks;

namespace HASS.Agent.Contracts.Managers;

public interface IExceptionManager
{
	public void OnFirstChanceExceptionHandler(object? sender, FirstChanceExceptionEventArgs? eventArgs);
}
