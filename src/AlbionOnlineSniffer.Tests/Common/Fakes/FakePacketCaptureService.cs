using System.Threading.Channels;

namespace AlbionOnlineSniffer.Tests.Common.Fakes;

/// <summary>
/// Implementação fake do serviço de captura de pacotes para testes
/// </summary>
public class FakePacketCaptureService : IPacketCaptureService
{
    private readonly Queue<byte[]> _packets = new();
    private readonly List<byte[]> _capturedPackets = new();
    private bool _isCapturing = false;
    private CancellationTokenSource? _captureTokenSource;
    private Channel<byte[]>? _channel;

    public bool IsCapturing => _isCapturing;
    public IReadOnlyList<byte[]> CapturedPackets => _capturedPackets.AsReadOnly();

    public FakePacketCaptureService(params byte[][] initialPackets)
    {
        foreach (var packet in initialPackets)
        {
            _packets.Enqueue(packet);
        }
    }

    public void EnqueuePacket(byte[] packet)
    {
        _packets.Enqueue(packet);
        
        // Se já estiver capturando, envia o pacote imediatamente
        if (_isCapturing && _channel != null)
        {
            _ = _channel.Writer.TryWrite(packet);
        }
    }

    public void EnqueuePackets(params byte[][] packets)
    {
        foreach (var packet in packets)
        {
            EnqueuePacket(packet);
        }
    }

    public Task StartCaptureAsync(CancellationToken cancellationToken = default)
    {
        if (_isCapturing)
            throw new InvalidOperationException("Capture already started");

        _isCapturing = true;
        _captureTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _channel = Channel.CreateUnbounded<byte[]>();

        // Envia todos os pacotes enfileirados
        Task.Run(async () =>
        {
            while (!_captureTokenSource.Token.IsCancellationRequested)
            {
                if (_packets.TryDequeue(out var packet))
                {
                    _capturedPackets.Add(packet);
                    await _channel.Writer.WriteAsync(packet, _captureTokenSource.Token);
                }
                else
                {
                    // Aguarda um pouco antes de verificar novamente
                    await Task.Delay(10, _captureTokenSource.Token);
                }
            }
            
            _channel.Writer.TryComplete();
        }, _captureTokenSource.Token);

        return Task.CompletedTask;
    }

    public Task StopCaptureAsync()
    {
        if (!_isCapturing)
            return Task.CompletedTask;

        _isCapturing = false;
        _captureTokenSource?.Cancel();
        _channel?.Writer.TryComplete();
        
        return Task.CompletedTask;
    }

    public async IAsyncEnumerable<byte[]> ReadPacketsAsync(
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (_channel == null)
            throw new InvalidOperationException("Capture not started");

        await foreach (var packet in _channel.Reader.ReadAllAsync(cancellationToken))
        {
            yield return packet;
        }
    }

    public void Clear()
    {
        _packets.Clear();
        _capturedPackets.Clear();
    }

    public void SimulateError(Exception exception)
    {
        _channel?.Writer.TryComplete(exception);
    }
}

/// <summary>
/// Interface para o serviço de captura de pacotes
/// </summary>
public interface IPacketCaptureService
{
    bool IsCapturing { get; }
    Task StartCaptureAsync(CancellationToken cancellationToken = default);
    Task StopCaptureAsync();
    IAsyncEnumerable<byte[]> ReadPacketsAsync(CancellationToken cancellationToken = default);
}