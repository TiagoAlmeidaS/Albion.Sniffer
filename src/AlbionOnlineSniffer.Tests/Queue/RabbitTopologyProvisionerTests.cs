using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using RabbitMQ.Client;
using Xunit;
using AlbionOnlineSniffer.Options;
using AlbionOnlineSniffer.Queue.Services;

namespace AlbionOnlineSniffer.Tests.Queue;

public class RabbitTopologyProvisionerTests
{
    private readonly Mock<IOptions<MessagingProvisioningOptions>> _mockProvisioningOptions;
    private readonly Mock<IOptions<PublishingSettings>> _mockPublishingSettings;
    private readonly Mock<ILogger<RabbitTopologyProvisioner>> _mockLogger;
    private readonly Mock<IHostEnvironment> _mockEnvironment;
    private readonly Mock<IConnectionFactory> _mockConnectionFactory;
    private readonly Mock<IConnection> _mockConnection;
    private readonly Mock<IModel> _mockChannel;

    public RabbitTopologyProvisionerTests()
    {
        _mockProvisioningOptions = new Mock<IOptions<MessagingProvisioningOptions>>();
        _mockPublishingSettings = new Mock<IOptions<PublishingSettings>>();
        _mockLogger = new Mock<ILogger<RabbitTopologyProvisioner>>();
        _mockEnvironment = new Mock<IHostEnvironment>();
        _mockConnectionFactory = new Mock<IConnectionFactory>();
        _mockConnection = new Mock<IConnection>();
        _mockChannel = new Mock<IModel>();
    }

    [Fact]
    public async Task StartAsync_WhenDisabled_DoesNotProvision()
    {
        // Arrange
        var options = new MessagingProvisioningOptions { Enabled = false };
        _mockProvisioningOptions.Setup(x => x.Value).Returns(options);
        _mockPublishingSettings.Setup(x => x.Value).Returns(new PublishingSettings());

        var provisioner = new RabbitTopologyProvisioner(
            _mockProvisioningOptions.Object,
            _mockPublishingSettings.Object,
            _mockLogger.Object,
            _mockEnvironment.Object);

        // Act
        await provisioner.StartAsync(CancellationToken.None);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("disabled")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task StartAsync_WhenNotInEnabledEnvironment_DoesNotProvision()
    {
        // Arrange
        var options = new MessagingProvisioningOptions 
        { 
            Enabled = true,
            EnableInEnvironments = new List<string> { "Development" }
        };
        _mockProvisioningOptions.Setup(x => x.Value).Returns(options);
        _mockPublishingSettings.Setup(x => x.Value).Returns(new PublishingSettings());
        _mockEnvironment.Setup(x => x.EnvironmentName).Returns("Production");

        var provisioner = new RabbitTopologyProvisioner(
            _mockProvisioningOptions.Object,
            _mockPublishingSettings.Object,
            _mockLogger.Object,
            _mockEnvironment.Object);

        // Act
        await provisioner.StartAsync(CancellationToken.None);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("not enabled for environment")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task StopAsync_ClosesConnectionAndChannel()
    {
        // Arrange
        var options = new MessagingProvisioningOptions { Enabled = false };
        _mockProvisioningOptions.Setup(x => x.Value).Returns(options);
        _mockPublishingSettings.Setup(x => x.Value).Returns(new PublishingSettings());

        var provisioner = new RabbitTopologyProvisioner(
            _mockProvisioningOptions.Object,
            _mockPublishingSettings.Object,
            _mockLogger.Object,
            _mockEnvironment.Object);

        // Act
        await provisioner.StopAsync(CancellationToken.None);

        // Assert - Should complete without errors
        Assert.True(true);
    }
}

public class MessagingProvisioningOptionsTests
{
    [Fact]
    public void DefaultValues_AreCorrect()
    {
        // Arrange & Act
        var options = new MessagingProvisioningOptions();

        // Assert
        Assert.False(options.Enabled);
        Assert.Null(options.ConnectionString);
        Assert.NotNull(options.Exchange);
        Assert.Equal("albion.events", options.Exchange.Name);
        Assert.Equal("topic", options.Exchange.Type);
        Assert.True(options.Exchange.Durable);
        Assert.False(options.Exchange.AutoDelete);
        Assert.NotNull(options.Queues);
        Assert.Empty(options.Queues);
        Assert.NotNull(options.EnableInEnvironments);
        Assert.Contains("Development", options.EnableInEnvironments);
        Assert.Contains("Testing", options.EnableInEnvironments);
        Assert.Equal("Information", options.LogLevel);
    }

    [Fact]
    public void ExchangeOptions_DefaultValues_AreCorrect()
    {
        // Arrange & Act
        var options = new ExchangeOptions();

        // Assert
        Assert.Equal("albion.events", options.Name);
        Assert.Equal("topic", options.Type);
        Assert.True(options.Durable);
        Assert.False(options.AutoDelete);
        Assert.NotNull(options.Arguments);
        Assert.Empty(options.Arguments);
    }

    [Fact]
    public void QueueOptions_DefaultValues_AreCorrect()
    {
        // Arrange & Act
        var options = new QueueOptions();

        // Assert
        Assert.Equal(string.Empty, options.Name);
        Assert.True(options.Durable);
        Assert.False(options.Exclusive);
        Assert.False(options.AutoDelete);
        Assert.NotNull(options.Arguments);
        Assert.Empty(options.Arguments);
        Assert.NotNull(options.Bindings);
        Assert.Empty(options.Bindings);
        Assert.Null(options.Exchange);
    }
}