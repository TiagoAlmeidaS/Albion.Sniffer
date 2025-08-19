using System;
using System.Threading.Tasks;
using AlbionOnlineSniffer.Core.Contracts;
using AlbionOnlineSniffer.Core.Services;
using AlbionOnlineSniffer.Queue.Interfaces;
using Microsoft.Extensions.Logging;

namespace AlbionOnlineSniffer.Queue.Publishers;

public sealed class V1ContractPublisherBridge
{
	private readonly EventDispatcher _dispatcher;
	private readonly EventContractRouter _router;
	private readonly IQueuePublisher _publisher;
	private readonly ILogger<V1ContractPublisherBridge> _logger;
	
	public V1ContractPublisherBridge(
		EventDispatcher dispatcher,
		EventContractRouter router,
		IQueuePublisher publisher,
		ILogger<V1ContractPublisherBridge> logger)
	{
		_dispatcher = dispatcher;
		_router = router;
		_publisher = publisher;
		_logger = logger;
		
		_dispatcher.RegisterGlobalHandler(OnEventAsync);
	}
	
	private async Task OnEventAsync(object gameEvent)
	{
		var result = _router.TryRoute(gameEvent);
		if (!result.Success)
		{
			return;
		}
		
		try
		{
			await _publisher.PublishAsync(result.Topic, result.Contract);
			_logger.LogDebug("Published V1 contract {Type} -> {Topic}", result.Contract.GetType().Name, result.Topic);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Failed to publish V1 contract {Type}", result.Contract.GetType().Name);
		}
	}
}
