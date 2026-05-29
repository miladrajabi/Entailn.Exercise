using Entain.Application.Common.Exception;
using Entain.Application.Interface.Service;

namespace Entain.Application.Service;

public class CallGroup<TOperation> : ICallGroup<TOperation>
{
    private readonly int _initialParticipantCount;
    private readonly Func<IReadOnlyCollection<TOperation>, Task> _groupCallDelegate;
    private readonly TimeSpan _timeout;
    private readonly List<TOperation> _requests;
    private readonly Task _task;
    private readonly object _lock = new();
    private readonly TaskCompletionSource _barrier;
    private int _joinedParticipantCount;

    public CallGroup(int participantCount,
        Func<IReadOnlyCollection<TOperation>, Task> groupCallDelegate,
        TimeSpan timeout)
    {
        if (participantCount <= 0)
            throw new ArgumentException("Value should be greater than zero!");

        _joinedParticipantCount = 0;
        _initialParticipantCount = participantCount;
        this._groupCallDelegate = groupCallDelegate;
        this._timeout = timeout;
        _requests = new List<TOperation>();
        _barrier = new TaskCompletionSource();
        _task = Task.Run(WaitParticipants);
    }

    public async Task Join(TOperation value)
    {
        ProcessNewJoiner(() => _requests.Add(value));
        await _task;
    }

    private async Task WaitParticipants()
    {
        try
        {
            await _barrier.Task.WaitAsync(_timeout);
        }
        //catch (TimeoutException)
        //{
        //    await _groupCallDelegate(_requests);
        //}
        catch (Exception err)
        {
            throw new CallGroupException(err.Message, err);
        }

        await _groupCallDelegate(_requests);
    }

    public void Leave()
    {
        ProcessNewJoiner(() =>
        {
        });
    }

    private void ProcessNewJoiner(Action addState)
    {
        lock (_lock)
        {
            var leftToJoin = _initialParticipantCount - _joinedParticipantCount;
            if (leftToJoin <= 0)
                throw new Exception("Too many participants!");

            _joinedParticipantCount++;
            addState();

            if (leftToJoin == 1)
            {
                _barrier.SetResult();
            }
        }
    }
}