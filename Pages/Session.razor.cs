using HotColour.Data.Game;
using HotColour.Data.Response;
using HotColour.Services;
using HotColour.Shared;
using HotColour.Shared.Atoms.Refresher;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace HotColour.Pages;

public partial class Session : IDisposable
{
    private const float PlayerInstanceYOffset = 20.0f;
    private DotNetObjectReference<Session>? _dotnetHelper;

    private DataResponse<GameSessionData> _game;
    private IJSObjectReference _module;

    private Refresher _refresher;

    [Inject] private IJSRuntime Js { get; set; }


    [Inject] private SessionHub SessionHub { get; set; }

    [Inject] private NavigationManager NavigationManager { get; set; }

    [CascadingParameter] private SessionManager Manager { get; set; }

    [Parameter] public string SessionId { get; set; } = string.Empty;


    private string TargetColour
    {
        get
        {
            var (h, s, l) = _game.Data.TargetColour;
            return $"hsl({h},{s}%,{l}%)";
        }
    }

    public void Dispose()
    {
        _dotnetHelper?.Dispose();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _module = await Js.InvokeAsync<IJSObjectReference>("import", "/js/ColourPicker.js");
            _dotnetHelper = DotNetObjectReference.Create(this);
            await _module.InvokeVoidAsync("InitializeColourPicker", _dotnetHelper);
        }

        await base.OnAfterRenderAsync(firstRender);
    }


    protected override void OnInitialized()
    {
        _game = SessionHub.GameSessionData(SessionId);
        if (!_game.Successful)
        {
            NavigationManager.NavigateTo("/");
        }

        Manager.OnJoined(RefreshPlayerList);
        Manager.OnLeft(RefreshPlayerList);
        Manager.OnStarted(OnStarted);
        Manager.OnGuessedColour(RefreshPlayerList);
        Manager.OnRoundEnded(RefreshPlayerList);
        Manager.OnEnded(OnGameEnded);
    }

    private float CurrentPercentageLeft()
    {
        var startTime = _game.Data.TimeWhenRoundStarted;
        var endTime = _game.Data.TimeWhenRoundEnds;
        var currentTime = DateTime.Now;

        var totalTime = (float) (endTime - startTime).TotalSeconds;
        var elapsedTime = (float) (currentTime - startTime).TotalSeconds;

        return elapsedTime * 100f / totalTime;
    }

    private void OnGameEnded()
    {
        RefreshPlayerList();
        _refresher.Stop();
    }

    private void OnRoundEnded()
    {
        RefreshPlayerList();
        _refresher.Stop();
        _refresher.Start();
    }

    private void OnStarted()
    {
        RefreshPlayerList();
        _refresher.Start();
    }

    private void RefreshPlayerList()
    {
        _game = SessionHub.GameSessionData(SessionId);
        if (!_game.Successful)
        {
            NavigationManager.NavigateTo("/");
        }

        StateHasChanged();
    }

    private string GetTransformStyle(int index)
    {
        var angleBetweenPoints = 360f / _game.Data.Players.Count;

        var angle = angleBetweenPoints * (index + 1);
        var angleRad = Math.PI * angle / 180f;

        var x = Math.Cos(angleRad) - Math.Sin(angleRad);
        var y = Math.Cos(angleRad) + Math.Sin(angleRad);

        x *= 270f;
        y *= 270f;

        y += PlayerInstanceYOffset;

        return $"transform: translate({x}px, {y}px)";
    }

    private Task StartGame()
    {
        return SessionHub.StartGame(SessionId);
    }

    private bool IsPlayersTurn(string playerId)
    {
        return _game.Data.PlayerIdsTurn == playerId;
    }


    [JSInvokable]
    public async Task OnSelectedColour(int h, int s, int l)
    {
        var result = await SessionHub.GuessColour(SessionId, new HueColour(h, s, l));
        if (result.Error == TypeOfFailure.CouldNotFindGame)
        {
            NavigationManager.NavigateTo("/");
        }
    }
}