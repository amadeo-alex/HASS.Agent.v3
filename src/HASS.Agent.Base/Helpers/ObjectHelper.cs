using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HASS.Agent.Helpers;

public static class ObjectHelper
{
	public static T JsonClone<T>(this object obj) //TODO(Amadeo): anti-pattern?
	{
		var objString = JsonConvert.SerializeObject(obj);
		return JsonConvert.DeserializeObject<T>(objString) ?? throw new ArgumentException("object cannot be json cloned");
	}
}

