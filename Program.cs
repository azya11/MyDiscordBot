using System;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using MyTaskManagerBot.commands; //Make sure to add your commands here

namespace MyTaskManagerBot
{
    internal class Program
    {
        public static DiscordClient Client { get; set; }
        private static CommandsNextExtension Commands { get; set; }
        static async Task Main(string[] args)
        {
            var JsonReader = new config.JSONReader();
            await JsonReader.ReadJSON();

            var DiscordConfig = new DiscordConfiguration() //Discord config 
            {
                Intents = DiscordIntents.All,
                Token = JsonReader.Token, //Pass token from JSON
                TokenType = TokenType.Bot, //Specify that this is a bot token
                AutoReconnect = true //Enable auto-reconnect
            };

            Client = new DiscordClient(DiscordConfig); //Apply this config to our client
            Client.UseInteractivity(new InteractivityConfiguration(){
                Timeout = TimeSpan.FromMinutes(5)
            });
            Client.Ready += ClientReady; //Subscribe to the Ready event

            var commandConfig = new CommandsNextConfiguration() //CommandsNext config
            {
                StringPrefixes = new string[] { JsonReader.Prefix },
                EnableMentionPrefix = true, //Enable mention prefix
                EnableDms = true, //Enable DMs
                EnableDefaultHelp = false, //I am gonna make my own help command
            };

            Commands = Client.UseCommandsNext(commandConfig); //Apply this config to our commands

            Commands.RegisterCommands<commands.Command>(); //Register commands from the Commands class

            await Client.ConnectAsync(); //Connect to Discord
            await Task.Delay(-1); //Wait indefinitely to make sure the bot stays online for as long as possible
        }

        public static Task ClientReady(DiscordClient client, DSharpPlus.EventArgs.ReadyEventArgs e)
        {
            return Task.CompletedTask; //Return a completed task
        }
    }
}
