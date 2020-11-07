using Discord;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Tsukihi.Objects
{
    internal class ReactionListener : IDisposable
    {
        public enum Action { Added, Removed }

        public delegate Task OnReactionChangedHandler(IEmote emote, Action action);

        public event OnReactionChangedHandler OnReactionChanged;

        private IUserMessage Message;
        private Dictionary<int, IEmote> Emotes;

        public ReactionListener(IUserMessage message, Dictionary<int, IEmote> emotes)
        {
            Message = message;
            Emotes = emotes;
        }

        public async void Initialize()
        {
            await SendReactions();
            Tsukihi.Client.ReactionAdded += OnReactionAdded;
            Tsukihi.Client.ReactionRemoved += OnReactionRemoved;
        }

        private async Task OnReactionRemoved(Cacheable<IUserMessage, ulong> arg1, Discord.WebSocket.ISocketMessageChannel arg2, Discord.WebSocket.SocketReaction arg3)
        {
            if (arg1.Id == Message.Id && arg3.UserId != Tsukihi.Client.CurrentUser.Id && Emotes.ContainsValue(arg3.Emote))
            {
                await OnReactionChanged(arg3.Emote, Action.Removed);
            }
        }

        private async Task OnReactionAdded(Cacheable<IUserMessage, ulong> arg1, Discord.WebSocket.ISocketMessageChannel arg2, Discord.WebSocket.SocketReaction arg3)
        {
            if (arg1.Id == Message.Id && arg3.UserId != Tsukihi.Client.CurrentUser.Id && Emotes.ContainsValue(arg3.Emote))
            {
                await OnReactionChanged(arg3.Emote, Action.Added);
            }
        }

        private async Task SendReactions()
        {
            for (int i = 0; i < Emotes.Count; i++)
            {
                IEmote emote;
                Emotes.TryGetValue(i + 1, out emote);
                await Task.Delay(250);
                await Message.AddReactionAsync(emote);
            }
        }

        public void Dispose()
        {
            Tsukihi.Client.ReactionAdded -= OnReactionAdded;
            Tsukihi.Client.ReactionRemoved -= OnReactionRemoved;
        }
    }
}