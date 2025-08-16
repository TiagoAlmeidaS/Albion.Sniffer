using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using AlbionOnlineSniffer.Core;
using AlbionOnlineSniffer.Core.Services;
using AlbionOnlineSniffer.Capture;
using AlbionOnlineSniffer.Capture.Services;
using AlbionOnlineSniffer.Web.Hubs;
using AlbionOnlineSniffer.Web.Services;
using System.Numerics;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.Services.AddSignalR();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddRouting();
builder.Services.AddSingleton<EventStreamService>();

// Core services
Core.DependencyProvider.RegisterServices(builder.Services);

// Packet capture service with DI logger
builder.Services.AddSingleton<PacketCaptureService>(sp =>
{
	var logger = sp.GetRequiredService<ILogger<PacketCaptureService>>();
	return new PacketCaptureService(5050, logger);
});

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapHub<SnifferHub>("/hubs/sniffer");

app.MapGet("/api/metrics", (PacketCaptureService capture) => Results.Json(capture.Monitor.Metrics));
app.MapGet("/api/events/recent", (EventStreamService stream) => Results.Json(stream.GetRecentEvents()));
app.MapGet("/api/packets/recent", (EventStreamService stream) => Results.Json(stream.GetRecentPackets()));
app.MapPost("/api/capture/start", (PacketCaptureService capture) => { capture.Start(); return Results.Ok(new { started = true }); });
app.MapPost("/api/capture/stop", (PacketCaptureService capture) => { capture.Stop(); return Results.Ok(new { started = false }); });

// Wire up sniffer pipeline on startup
using (var scope = app.Services.CreateScope())
{
	var services = scope.ServiceProvider;
	var loggerFactory = services.GetRequiredService<ILoggerFactory>();
	var eventDispatcher = services.GetRequiredService<EventDispatcher>();
	var definitionLoader = services.GetRequiredService<PhotonDefinitionLoader>();
	var albionNetworkHandlerManager = services.GetRequiredService<AlbionNetworkHandlerManager>();
	var receiverBuilder = albionNetworkHandlerManager.ConfigureReceiverBuilder();
	var photonReceiver = receiverBuilder.Build();
	var protocol16Deserializer = new Protocol16Deserializer(
		photonReceiver,
		loggerFactory.CreateLogger<Protocol16Deserializer>()
	);

	var capture = services.GetRequiredService<PacketCaptureService>();
	var hubContext = services.GetRequiredService<IHubContext<SnifferHub>>();
	var stream = services.GetRequiredService<EventStreamService>();

	// Forward raw UDP payloads to UI and deserializer
	capture.OnUdpPayloadCaptured += payload =>
	{
		try
		{
			stream.AddRawPacket(payload);
			hubContext.Clients.All.SendAsync("udpPayload", new
			{
				Length = payload?.Length ?? 0,
				Hex = payload != null ? Convert.ToHexString(payload) : ""
			});
			protocol16Deserializer.ReceivePacket(payload);
		}
		catch { }
	};

	// Broadcast metrics periodically
	capture.Monitor.OnMetricsUpdated += metrics =>
	{
		stream.UpdateMetrics(metrics);
		hubContext.Clients.All.SendAsync("metrics", metrics);
	};

	// Broadcast parsed game events
	eventDispatcher.RegisterGlobalHandler(async gameEvent =>
	{
		object? location = null;
		try
		{
			if (gameEvent is AlbionOnlineSniffer.Core.Models.Events.IHasPosition hasPosition)
			{
				var pos = hasPosition.Position;
				location = new { X = pos.X, Y = pos.Y };
			}
			else
			{
				var posProp = gameEvent.GetType().GetProperty("Position");
				if (posProp != null && posProp.PropertyType == typeof(Vector2))
				{
					var pos = (Vector2)posProp.GetValue(gameEvent);
					location = new { X = pos.X, Y = pos.Y };
				}
			}
		}
		catch { }

		var message = new
		{
			EventType = gameEvent.GetType().Name,
			Timestamp = DateTime.UtcNow,
			Position = location,
			Data = gameEvent
		};

		stream.AddEvent(message);
		await hubContext.Clients.All.SendAsync("gameEvent", message);
	});

	// Optionally load bin-dumps definitions if directory exists (mirrors console app behavior)
	try
	{
		var binDumpsPath = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "ao-bin-dumps");
		if (System.IO.Directory.Exists(binDumpsPath))
		{
			definitionLoader.Load(binDumpsPath);
		}
	}
	catch { }
}

app.Lifetime.ApplicationStarted.Register(() =>
{
	try
	{
		var capture = app.Services.GetRequiredService<PacketCaptureService>();
		capture.Start();
	}
	catch (Exception ex)
	{
		var logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");
		logger.LogError(ex, "Falha ao iniciar captura automaticamente. Use /api/capture/start para tentar novamente.");
	}
});

app.Lifetime.ApplicationStopping.Register(() =>
{
	try
	{
		var capture = app.Services.GetRequiredService<PacketCaptureService>();
		capture.Stop();
	}
	catch { }
});

app.Run();