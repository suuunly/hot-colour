using HotColour.Data;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;

namespace HotColour.Shared;

public partial class SessionManager
{
    private HubConnection _sessionConnection;
    [Inject] private NavigationManager NavigationManager { get; set; }

    [Parameter] public RenderFragment? ChildContent { get; set; }

    protected override Task OnInitializedAsync()
    {
        _sessionConnection = new HubConnectionBuilder()
            .WithUrl(NavigationManager.ToAbsoluteUri("/socket"))
            .Build();

        return _sessionConnection.StartAsync();
    }

    public void OnJoined(Action callback)
    {
        _sessionConnection.On(SessionCallbacks.JoinedGame, callback);
    }

    public void OnLeft(Action callback)
    {
        _sessionConnection.On(SessionCallbacks.LeftGame, callback);
    }

    public void OnStarted(Action callback)
    {
        _sessionConnection.On(SessionCallbacks.StartedGame, callback);
    }

    public void OnGuessedColour(Action callback)
    {
        _sessionConnection.On(SessionCallbacks.GuessedColour, callback);
    }

    public void OnRoundEnded(Action callback)
    {
        _sessionConnection.On(SessionCallbacks.RoundEnded, callback);
    }

    public void OnEnded(Action callback)
    {
        _sessionConnection.On(SessionCallbacks.GameEnded, callback);
    }
}