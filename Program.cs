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
        static void Main(string[] args)
        {
            MainAsync().GetAwaiter().GetResult();
        }

        static async Task MainAsync()
        {
            var JsonReader = new config.JSONReader();
            await JsonReader.ReadJSON();

            var DiscordConfig = new DiscordConfiguration()
            {
                Intents = DiscordIntents.All,
                Token = JsonReader.Token,
                TokenType = TokenType.Bot,
                AutoReconnect = true
            };

            Client = new DiscordClient(DiscordConfig);
            Client.UseInteractivity(new InteractivityConfiguration()
            {
                Timeout = TimeSpan.FromMinutes(5)
            });
            Client.Ready += ClientReady;

            var commandConfig = new CommandsNextConfiguration()
            {
                StringPrefixes = new string[] { JsonReader.Prefix },
                EnableMentionPrefix = true,
                EnableDms = true,
                EnableDefaultHelp = false,
            };

            Commands = Client.UseCommandsNext(commandConfig);
            Commands.RegisterCommands<commands.Command>();

            await Client.ConnectAsync();
            await Task.Delay(-1); // держим процесс бесконечно
        }


        public static Task ClientReady(DiscordClient client, DSharpPlus.EventArgs.ReadyEventArgs e)
        {
            return Task.CompletedTask; //Return a completed task
        }
    }
}
