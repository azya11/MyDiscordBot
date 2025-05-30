using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace MyTaskManagerBot.commands
{
    public class Command : BaseCommandModule
    {
        [Command("help")]
        public async Task HelpCommand(CommandContext ctx)
        {
            string username = ctx.User.Username;   //ctx. containts nearly everything that bot can do, just type ctx. and look what it can do
            var avatar = ctx.User.AvatarUrl;
            await ctx.Channel.SendMessageAsync($"Hello, {username}");
            
        }

        [Command("create_task")]
        [Description("Creates Task with two intakes name as string and due date as int")]

        public async  Task CreateTaskCommand(CommandContext ctx, string taskName, string deadline)
        {
            var avatar = ctx.User.AvatarUrl;
            // Here you would implement the logic to create a task
            // For now, we will just acknowledge the command
            await ctx.Channel.SendMessageAsync($"Task '{taskName}'{avatar} has been created!");
        }
    }
}
