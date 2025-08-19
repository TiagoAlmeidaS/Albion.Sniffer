using System.Collections.Generic;
using System.Threading.Tasks;

namespace Albion.Network
{
    // Minimal shims to satisfy compile-time references

    public interface IPhotonReceiver
    {
        void ReceivePacket(byte[] payload);
    }

    public class ReceiverBuilder
    {
        public IPhotonReceiver Build() => new NoopReceiver();

        // Add no-op registration methods used by manager
        public ReceiverBuilder AddEventHandler<T>(EventPacketHandler<T> handler) => this;
        public ReceiverBuilder AddResponseHandler<T>(ResponsePacketHandler<T> handler) => this;
        public ReceiverBuilder AddRequestHandler<T>(RequestPacketHandler<T> handler) => this;

        private sealed class NoopReceiver : IPhotonReceiver
        {
            public void ReceivePacket(byte[] payload) { }
        }
    }

    public abstract class BaseEvent
    {
        protected BaseEvent(Dictionary<byte, object> parameters) { }
    }

    public abstract class BaseOperation
    {
        protected BaseOperation(Dictionary<byte, object> parameters) { }
    }

    public abstract class PacketHandler<T>
    {
        protected PacketHandler() { }

        protected virtual Task OnHandleAsync(object packet) => Task.CompletedTask;

        // Some existing code overrides OnHandle; keep both for compatibility
        protected virtual void OnHandle(object packet) { }
    }

    public abstract class EventPacketHandler<T> : PacketHandler<T>
    {
        protected EventPacketHandler(int index) { }
        protected virtual Task OnActionAsync(T value) => Task.CompletedTask;
    }

    public abstract class ResponsePacketHandler<T> : PacketHandler<T>
    {
        protected ResponsePacketHandler(int index) { }
        protected virtual Task OnActionAsync(T value) => Task.CompletedTask;
    }

    public abstract class RequestPacketHandler<T> : PacketHandler<T>
    {
        protected RequestPacketHandler(int index) { }
        protected virtual Task OnActionAsync(T value) => Task.CompletedTask;
    }

    public abstract class Packet
    {
        public Dictionary<int, object> Parameters { get; } = new();
    }

    public class ResponsePacket : Packet { }
    public class RequestPacket : Packet { }
    public class EventPacket : Packet { }
}


