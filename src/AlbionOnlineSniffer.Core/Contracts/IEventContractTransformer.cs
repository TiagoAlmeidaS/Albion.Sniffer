using System;

namespace AlbionOnlineSniffer.Core.Contracts;

public interface IEventContractTransformer
{
	bool CanTransform(object gameEvent);
	(bool Success, string Topic, object Contract) TryTransform(object gameEvent);
}
