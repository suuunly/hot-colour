namespace HotColour.Data.Game;

public class RoundEndDelay : IDisposable
{
    private readonly TimeSpan _initialRoundLength;
    private readonly Timer _timer;

    public RoundEndDelay(TimeSpan initialRoundLength, Action onEndedCallback)
    {
        _initialRoundLength = initialRoundLength;

        _timer = new Timer(
            _ => onEndedCallback(),
            null,
            Timeout.Infinite,
            Timeout.Infinite
        );
        ResetTime();
    }

    public DateTime StartTime { get; private set; }
    public DateTime RoundEndTime { get; private set; }

    public void Dispose()
    {
        _timer?.Dispose();
    }

    private void ResetTime()
    {
        StartTime = DateTime.Now;
        RoundEndTime = DateTime.Now + _initialRoundLength;
    }

    public void Start()
    {
        ResetTime();

        var duration = (RoundEndTime - StartTime).TotalSeconds;
        _timer.Change(TimeSpan.FromSeconds(duration), Timeout.InfiniteTimeSpan);
    }

    public void AddTime(TimeSpan time)
    {
        var currentTime = DateTime.Now;

        var totalSeconds = (RoundEndTime + time - currentTime).TotalSeconds;

        var capSeconds = _initialRoundLength.Seconds;

        RoundEndTime += totalSeconds > capSeconds
            ? TimeSpan.FromSeconds(capSeconds - totalSeconds)
            : time;

        var extendedDuration = (RoundEndTime - currentTime).TotalSeconds;

        _timer.Change(TimeSpan.FromSeconds(extendedDuration), Timeout.InfiniteTimeSpan);
    }
}