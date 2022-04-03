using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HotColour.Data;
using HotColour.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;

namespace HotColour.Shared
{
    public partial class SessionManager
    {
        [Inject] private NavigationManager NavigationManager { get; set; }

        private HubConnection _sessionConnection;
        
        [Parameter]
        public RenderFragment? ChildContent { get; set; }
        
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
    }
}