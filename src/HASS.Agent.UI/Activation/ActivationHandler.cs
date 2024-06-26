﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HASS.Agent.UI.Contracts;

namespace HASS.Agent.UI.Activation;
public abstract class ActivationHandler<T> : IActivationHandler
    where T : class
{
    // Override this method to add the logic for whether to handle the activation.
    protected virtual bool CanHandleInternal(T args) => true;

    // Override this method to add the logic for your activation handler.
    protected abstract Task HandleInternalAsync(T args);

    public bool CanHandle(object args) => args is T && CanHandleInternal((args as T)!);

    public async Task HandleAsync(object args) => await HandleInternalAsync((args as T)!);
}
