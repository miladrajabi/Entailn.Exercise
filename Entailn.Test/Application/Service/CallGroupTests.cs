using Entain.Application.Service;
using FluentAssertions;
using Moq;
using Xunit;

namespace Entailn.Test.Application.Service;

public class CallGroupTests1
{
    [Fact]
    public void Constructor_ShouldThrowArgumentException_WhenParticipantCountIsZeroOrNegative()
    {
        // Arrange
        Func<IReadOnlyCollection<string>, Task> dummyDelegate = _ => Task.CompletedTask;

        // Act
        Action act = () => new CallGroup<string>(0, dummyDelegate, TimeSpan.FromSeconds(1));

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("Value should be greater than zero!*");
    }

    [Fact]
    public async Task Join_ShouldAddParticipantAndTriggerDelegate_WhenAllParticipantsJoined()
    {
        // Arrange
        var mockDelegate = new Mock<Func<IReadOnlyCollection<string>, Task>>();
        var callGroup = new CallGroup<string>(1, mockDelegate.Object, TimeSpan.FromSeconds(1));

        // Act
        await callGroup.Join("participant1");

        // Assert
        mockDelegate.Verify(d => d(It.IsAny<IReadOnlyCollection<string>>()), Times.Once);
    }

    [Fact]
    public void Leave_ShouldNotThrowException_WhenCalled()
    {
        // Arrange
        var mockDelegate = new Mock<Func<IReadOnlyCollection<string>, Task>>();
        var callGroup = new CallGroup<string>(1, mockDelegate.Object, TimeSpan.FromSeconds(1));

        // Act
        Action act = () => callGroup.Leave();

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public async Task Join_ShouldThrowException_WhenTooManyParticipantsJoin()
    {
        // Arrange
        var mockDelegate = new Mock<Func<IReadOnlyCollection<string>, Task>>();
        var callGroup = new CallGroup<string>(1, mockDelegate.Object, TimeSpan.FromSeconds(1));

        // Act
        await callGroup.Join("participant1");
        Func<Task> act = async () => await callGroup.Join("participant2");

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("Too many participants!");
    }
}