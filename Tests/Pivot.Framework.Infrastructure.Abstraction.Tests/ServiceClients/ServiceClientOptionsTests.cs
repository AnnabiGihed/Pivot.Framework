using FluentAssertions;
using Pivot.Framework.Infrastructure.Abstraction.ServiceClients;

namespace Pivot.Framework.Infrastructure.Abstraction.Tests.ServiceClients;

/// <summary>
/// Author      : Gihed Annabi
/// Date        : 03-2026
/// Purpose     : Unit tests for <see cref="ServiceClientOptions"/>.
///              Verifies default resilience settings and property assignment.
/// </summary>
public class ServiceClientOptionsTests
{
	[Fact]
	public void ServiceClientOptions_ShouldHaveDefaults()
	{
		var options = new ServiceClientOptions { BaseUrl = "http://localhost:5000" };

		options.BaseUrl.Should().Be("http://localhost:5000");
		options.RetryCount.Should().Be(3);
		options.RetryBaseDelaySeconds.Should().Be(1);
		options.CircuitBreakerThreshold.Should().Be(5);
		options.CircuitBreakerDurationSeconds.Should().Be(30);
		options.TimeoutSeconds.Should().Be(30);
	}

	[Fact]
	public void ServiceClientOptions_ShouldSetAllProperties()
	{
		var options = new ServiceClientOptions
		{
			BaseUrl = "http://api.example.com",
			RetryCount = 5,
			RetryBaseDelaySeconds = 2,
			CircuitBreakerThreshold = 10,
			CircuitBreakerDurationSeconds = 60,
			TimeoutSeconds = 15
		};

		options.RetryCount.Should().Be(5);
		options.RetryBaseDelaySeconds.Should().Be(2);
		options.CircuitBreakerThreshold.Should().Be(10);
		options.CircuitBreakerDurationSeconds.Should().Be(60);
		options.TimeoutSeconds.Should().Be(15);
	}
}
