using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HASS.Agent.Contracts.Helpers;
public static class StringHelper
{
    public static string RemoveNonAlphanumericCharacters(this string value) => new(value.Where(c => char.IsLetterOrDigit(c) || char.IsWhiteSpace(c)).ToArray());

}
