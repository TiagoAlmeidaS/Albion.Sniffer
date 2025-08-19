using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace AlbionOnlineSniffer.Core.Contracts;

public class EventContractRouter
{
	private readonly IEnumerable<IEventContractTransformer> _transformers;
	private readonly ILogger<EventContractRouter> _logger;
	
	public EventContractRouter(IEnumerable<IEventContractTransformer> transformers, ILogger<EventContractRouter> logger)
	{
		_transformers = transformers;
		_logger = logger;
	}
	
	public (bool Success, string Topic, object Contract) TryRoute(object gameEvent)
	{
		foreach (var t in _transformers)
		{
			if (!t.CanTransform(gameEvent)) continue;
			var result = t.TryTransform(gameEvent);
			if (result.Success)
			{
				_logger.LogDebug("Transformed {EventType} -> {Topic}", gameEvent.GetType().Name, result.Topic);
				return result;
			}
		}
		return (false, string.Empty, null!);
	}
}
