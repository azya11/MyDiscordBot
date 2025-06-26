using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity.Extensions;

namespace MyTaskManagerBot.commands
{
    public class Command : BaseCommandModule
    {
        public List<Game> currentGames; //Create a list of games

        [Command("help")]
        public async Task HelpCommand(CommandContext ctx)
        {
            var message = new DiscordEmbedBuilder
            {
                Title = "HELP",
                Color = DiscordColor.Blue,
                Description = "Here is the list of commands available:\n!create_game - creates a new Mafia Game in this channel\n!join - adds user to the existing game\n!ready - sets the game to ready, only creator of the game can call this comman\n!contacts - if you want to contact me"
            };
            await ctx.Channel.SendMessageAsync(embed: message);

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
                        var message = new DiscordEmbedBuilder
                        {
                            Title = "Game already exists",
                            Color = DiscordColor.Red,
                            Description = "There is already a game in this channel! \nYou can join it by typing !join"
                        };
                        await ctx.Channel.DeleteMessageAsync(ctx.Message);
                        var lastmessage = await ctx.Channel.SendMessageAsync(embed: message); //If there is, send a message
                        await Task.Delay(5000); //Wait for 5 seconds before deleting the message
                        await lastmessage.DeleteAsync(); //Delete the message after 5 seconds
                        return; //Exit the method
                    }
                }
            }
            await newGame.AddPlayer(ctx.User.Id); //Add the player who created the game to the game
            this.currentGames.Add(newGame);
            var created = new DiscordEmbedBuilder
            {
                Title = "New game has been created!",
                Description = "Type !join to enter the game",
                Color = DiscordColor.Green
            };
            await ctx.Channel.SendMessageAsync(embed: created);
        }

        [Command("join")]
        public async Task JoinGameCommand(CommandContext ctx)
        {
            if (this.currentGames == null || !this.currentGames.Any()) //Check if there are any games in the list
            {
                var message = new DiscordEmbedBuilder
                {
                    Title = "No games available",
                    Color = DiscordColor.Red,
                    Description = "There are no games to join! \nYou can create a new game by typing !create_game"
                };
                await ctx.Channel.DeleteMessageAsync(ctx.Message); //Delete the message that called the command
                var lastmessage = await ctx.Channel.SendMessageAsync(embed: message); //If there are no games, send a message
                await Task.Delay(5000);
                await lastmessage.DeleteAsync();

                return; //Exit the method
            }
            foreach (var game in this.currentGames)
            {
                if (game.ChannelId == ctx.Channel.Id)
                {
                    await game.AddPlayer(ctx.User.Id);
                    var okaymessage = new DiscordEmbedBuilder
                    {
                        Title = $"{ctx.User.Username} have joined the game!",
                        Description = $"Players in the game:\n{string.Join("\n", game.Players.Select(p => p.ToString()))}",
                        Color = DiscordColor.Green
                    };
                    await ctx.Channel.SendMessageAsync(embed: okaymessage);
                }
                else
                {
                    var message = new DiscordEmbedBuilder
                    {
                        Title = "No games available",
                        Color = DiscordColor.Red,
                        Description = "There are no games to join! \nYou can create a new game by typing !create_game"
                    };
                    await ctx.Channel.DeleteMessageAsync(ctx.Message); //Delete the message that called the command
                    var lastmessage = await ctx.Channel.SendMessageAsync(embed: message); //If there are no games, send a message
                    await Task.Delay(5000);
                    await lastmessage.DeleteAsync();
                }
            }
        }

        [Command("ready")]
        public async Task ReadyCommand(CommandContext ctx)
        {
            foreach (var game in this.currentGames)
            {
                if (game.ChannelId == ctx.Channel.Id)
                {
                    if (game.Players.First().ID == ctx.User.Id) //Check if the player who called !ready owner of the game
                    {
                        if (game.Players.Count >= 4) //Check if there are 4 or more players in the game
                        {
                            game.IsReady = true; //Set the game to ready
                            await ctx.Channel.SendMessageAsync("WE ARE STARTING!!! \nSORRY FOR EVERYONE WHO WAS LATE!"); //Send a message that the game is ready
                            game.StartGame(); //Start the game
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
        [Command("contacts")]
        public async Task ContactsCommand(CommandContext ctx)
        {
            var message = new DiscordEmbedBuilder
            {
                Title = "Hey, My name is Aziz Shamuratov. \nI've developed this bot for you to enjoy. My discord is .azya \n",
                Description = "If you have any questions or suggestions, feel free to contact me! \nI hope you enjoy playing Mafia with your friends!",
                Color = DiscordColor.White
            };
            await ctx.Channel.SendMessageAsync(message);
        }

        [Command("test")] //create game settings menu to allow changing the amount of roles played in the game.
        public async Task testcommand(CommandContext ctx)
        {
            var interactivity = Program.Client.GetInteractivity();
            //use interactivity
            var jointime = TimeSpan.FromMinutes(5);
            DiscordEmoji[] discordEmojis = {
                DiscordEmoji.FromName(Program.Client, ":one:"),
                DiscordEmoji.FromName(Program.Client, ":two:")
            };
            var joinMessage = new DiscordEmbedBuilder
            {
                Color = DiscordColor.Red,
                Title = "Join!"
            };
            var sentMessage = await ctx.Channel.SendMessageAsync(embed: joinMessage);
            foreach (var emoji in discordEmojis)
            {
                await sentMessage.CreateReactionAsync(emoji);
            }
            var totalReactions = await interactivity.CollectReactionsAsync(sentMessage, jointime);
        }
        [Command("dm")]
        public async Task dmcommand(CommandContext ctx) {
            var member = await ctx.Guild.GetMemberAsync(ctx.User.Id);
            await member.SendMessageAsync("Ответ");
        }
    }

        //CLASS GAME
        public class Game
    {
        //events

        // Parameters for the game
        public bool IsReady { get; set; }
        public ulong ChannelId { get; set; } //Channel ID where the game is played                                   
        public List<Player> Players { get; set; } //List of players in the game
        public int mafia { get; set; }
        public int doctor { get; set; }
        public int cop { get; set; }


        // Constructor to initialize a new game with the channel ID
        public Game(ulong channelId)
        {
            ChannelId = channelId;
            Players = new List<Player>();
            IsReady = false;
        }

        public async Task addMafia()
        {
            this.mafia++;
        }

        public async Task addDoc()
        {
            this.doctor++;
        }
        public async Task addCop()
        {
            this.cop++;
        }

        public async Task AddPlayer(ulong playerId)
        {
            Player player = new Player(playerId);
            if (!Players.Contains(player)) //Check if the player is already in the game
            {
                Players.Add(player);
            }

        }

        public async Task StartGame()
        {
            bool live = true;
            //Assign the roles to the players, without any rules yet.
            while (live == true)
            {
                foreach (Player player in Players)
                {
                    //while (this.Players.= true)
                    //{
                    //    async sendMessage(Player)
                    //}
                }
                
                //check the people that are alive
                // send them a turn notifications
                // wait for input
                // give the output
            }
            //Send each player their role assignment
            // This could include assigning roles, sending messages to players, etc.
        }

    }

    //public async Task sendMessage(Player targetPlayer)
    //{
    //    //logic to send message to the players if there are alive
    //}
    public class Player
    {
        // 0 - Civillian
        // 1 - Mafia
        // 2 - Doctor
        // 3 - Cop
        public int role { get; set; }
        public ulong ID { get; set; }
        public bool isLive { get; set; }

        public Player(ulong ID)
        {
            this.ID = ID;
            this.role = 0;
            this.isLive = true;
        }
    }
}