using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HotColour.Data.Game;
using HotColour.Services;
using HotColour.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace HotColour.Pages
{
    public partial class Session
    {
        private const float playerInstanceYOffset = 20.0f;
        
        [Inject] private IJSRuntime Js { get; set; }
        IJSObjectReference _module;
        
        [Parameter]
        public string SessionId { get; set; }

        [CascadingParameter] private SessionManager Manager { get; set; }
        [Inject] private SessionHub SessionHub { get; set; }

        private List<Player> _players = new()
        {
            new Player("1", "Bob", "media/avatars/avatar-1.png"),
            new Player("1", "Miller", "media/avatars/avatar-2.png"),
            new Player("1", "Jennifer", "media/avatars/avatar-3.png"),
            new Player("1", "Mitchell", "media/avatars/avatar-5.png"),
            new Player("1", "Sarah", "media/avatars/avatar-6.png"),
            new Player("1", "Benjamin", "media/avatars/avatar-2.png"),
            new Player("1", "Flipper", "media/avatars/avatar-4.png"),
            new Player("1", "Doofus", "media/avatars/avatar-9.png"),
            new Player("1", "Limber", "media/avatars/avatar-8.png"),
            new Player("1", "Snorkle", "media/avatars/avatar-7.png"),
        };

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                _module = await Js.InvokeAsync<IJSObjectReference>("import", "/js/ColourPicker.js");
                await _module.InvokeVoidAsync("InitializeColourPicker");
            }
            
            await base.OnAfterRenderAsync(firstRender);
        }


        protected override void OnInitialized()
        {
            // _players = SessionHub.GetPlayers(SessionId);
            Manager.OnJoined(RefreshPlayerList);
            Manager.OnLeft(RefreshPlayerList);
        }

        private void RefreshPlayerList()
        {
            // _players = SessionHub.GetPlayers(SessionId);
            StateHasChanged();
        }

        private string GetTransformStyle(int index)
        {
            var angleBetweenPoints = 360f / _players.Count;

            var angle = angleBetweenPoints * (index + 1);
            var angleRad = Math.PI * angle / 180f;

            var x = Math.Cos(angleRad) - Math.Sin(angleRad);
            var y = Math.Cos(angleRad) + Math.Sin(angleRad);

            x *= 270f;
            y *= 270f;

            y += playerInstanceYOffset;
            
            return $"transform: translate({x}px, {y}px)";
        }
    }
}