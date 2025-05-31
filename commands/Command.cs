using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.EventArgs;

namespace MyTaskManagerBot.commands
{
    public class Command : BaseCommandModule
        {
            public List<Game> currentGames; //Create a list of games

        [Command("help")]
        public async Task HelpCommand(CommandContext ctx)
        {
            string username = ctx.User.Username;   //ctx. containts nearly everything that bot can do, just type ctx. and look what it can do
            var avatar = ctx.User.AvatarUrl;
            await ctx.Channel.SendMessageAsync($"Hello, {username}" + "\n"
                + "This is bot was created to play Mafia with your friends on discord!" + "\n"
                + "Here is the list of commands available:" + "\n"
                + "!create_game - creates a new Mafia Game in this channel" + "\n"
                + "!join - adds user to the existing game" + "\n"
                + "!ready - sets the game to ready, only creator of the game can call this command" + "\n"
                );

            }

            [Command("create_game")]

            public async Task CreateGameCommand(CommandContext ctx)
            {
                Game newGame = new Game(ctx.Channel.Id);
                if (this.currentGames == null) //Check if the list of games is null
            {
                this.currentGames = new List<Game>(); //If it is, create a new list
            }
            else
            {
                foreach (var game in this.currentGames) //Check if there is already a game in this channel
                {
                    if (game.ChannelId == ctx.Channel.Id)
                    {
                        await ctx.Channel.SendMessageAsync("There is already a game in this channel!"); //If there is, send a message
                        return; //Exit the method
                    }
                }
            }
                await newGame.AddPlayer(ctx.User.Username); //Add the player who created the game to the game
                this.currentGames.Add(newGame);
                await ctx.Channel.SendMessageAsync($"New game has been created!, join by typing !join");
        }

        [Command("join")]
            public async Task JoinGameCommand(CommandContext ctx)
            {
            if(this.currentGames == null || !this.currentGames.Any()) //Check if there are any games in the list
                {
                    await ctx.Channel.SendMessageAsync("There are no games to join!"); //If there are no games, send a message
                    return; //Exit the method
                }
            foreach (var game in this.currentGames)
                {
                    if (game.ChannelId == ctx.Channel.Id)
                    {
                        await game.AddPlayer(ctx.User.Username);
                        await ctx.Channel.SendMessageAsync($"{ctx.User.Username} have joined the game! Players in the game: {string.Join(", ", game.Players.Select(p => p.ToString()))}"); //Send a message with the players in the game
                    }
                    else {await ctx.Channel.SendMessageAsync("There is no game in this channel!"); }
                }
            }

            [Command("ready")]
            public async Task ReadyCommand(CommandContext ctx)
            {
                foreach (var game in this.currentGames)
                {
                    if(game.ChannelId == ctx.Channel.Id)
                    {
                        if(game.Players.First() == ctx.User.Username) //Check if the player who called !ready owner of the game
                        {   
                            if(game.Players.Count >= 4) //Check if there are 4 or more players in the game
                            {
                                game.IsReady = true; //Set the game to ready
                                await ctx.Channel.SendMessageAsync("WE ARE STARTING!!! \nSORRY FOR EVERYONE WHO WAS LATE!"); //Send a message that the game is ready
                            }
                            else
                            {
                                await ctx.Channel.SendMessageAsync("There are not enough players to start the game! \nYou need at least 4 players to start the game!"); //If there are not enough players, send a message
                            }

                    }
                    else
                        {
                            await ctx.Channel.SendMessageAsync("You are not the creator of the game! \nOnly creator can call !ready"); //If the player is not the first player, send a message
                        }
                    }
                }
            }

    }


    //CLASS GAME
    public class Game
    {
        //events

        // Parameters for the game
        public bool IsReady { get; set; }
        public ulong ChannelId { get; set; } //Channel ID where the game is played                                   
        public List<string> Players { get; set; } //List of players in the game



        // Constructor to initialize a new game with the channel ID
        public Game(ulong channelId)
        {
            ChannelId = channelId;
            Players = new List<string>();
            IsReady = false;
        }

        public async Task AddPlayer(string playerId)
        {
            if (!Players.Contains(playerId)) //Check if the player is already in the game
            {
                Players.Add(playerId);
            }

        }

    }
    public class Player
    {
        public int role { get; set; }
        public string Name { get; set; }

        public Player(string userName)
        {
            this.Name = userName;
        }
    }
}
