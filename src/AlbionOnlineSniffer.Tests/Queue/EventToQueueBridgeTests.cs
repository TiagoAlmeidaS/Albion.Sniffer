using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using AlbionOnlineSniffer.Core.Services;
using AlbionOnlineSniffer.Core.Interfaces;
using AlbionOnlineSniffer.Queue.Publishers;

namespace AlbionOnlineSniffer.Tests.Queue
{
    public class EventToQueueBridgeTests
    {
        private sealed class DummyEvent {}
        private sealed class DummyEventCustom {}
        private sealed class NewCharacterEvent {}

        [Fact]
        public async Task Should_Remove_Event_Suffix_From_Topic()
        {
            var services = new ServiceCollection();
            services.AddSingleton<ILoggerFactory>(_ => LoggerFactory.Create(b => { }));
            services.AddSingleton(typeof(ILogger<EventToQueueBridge>), sp => sp.GetRequiredService<ILoggerFactory>().CreateLogger<EventToQueueBridge>());
            services.AddSingleton<EventDispatcher>(sp => new EventDispatcher(sp.GetRequiredService<ILoggerFactory>().CreateLogger<EventDispatcher>()));

            var publisherMock = new Mock<IQueuePublisher>();
            services.AddSingleton(publisherMock.Object);

            services.AddSingleton<EventToQueueBridge>();
            var sp = services.BuildServiceProvider();

            // resolve para registrar handler global
            sp.GetRequiredService<EventToQueueBridge>();
            var dispatcher = sp.GetRequiredService<EventDispatcher>();

            // Dispara um tipo com sufixo Event
            await dispatcher.DispatchEvent(new NewCharacterEvent());

            // Verifica que publicou com tópico sem sufixo 'event'
            publisherMock.Verify(p => p.PublishAsync("albion.event.newcharacter", It.IsAny<object>()), Times.AtLeastOnce());
        }

        [Fact]
        public async Task Should_Not_Alter_NonSuffix_Event_In_Middle_Of_Name()
        {
            var services = new ServiceCollection();
            services.AddSingleton<ILoggerFactory>(_ => LoggerFactory.Create(b => { }));
            services.AddSingleton(typeof(ILogger<EventToQueueBridge>), sp => sp.GetRequiredService<ILoggerFactory>().CreateLogger<EventToQueueBridge>());
            services.AddSingleton<EventDispatcher>(sp => new EventDispatcher(sp.GetRequiredService<ILoggerFactory>().CreateLogger<EventDispatcher>()));

            var publisherMock = new Mock<IQueuePublisher>();
            services.AddSingleton(publisherMock.Object);

            services.AddSingleton<EventToQueueBridge>();
            var sp = services.BuildServiceProvider();

            sp.GetRequiredService<EventToQueueBridge>();
            var dispatcher = sp.GetRequiredService<EventDispatcher>();

            // Nome contém "Event" no meio, mas não termina com "Event"
            await dispatcher.DispatchEvent(new DummyEventCustom());

            publisherMock.Verify(p => p.PublishAsync("albion.event.dummyeventcustom", It.IsAny<object>()), Times.AtLeastOnce());
        }
    }
}


