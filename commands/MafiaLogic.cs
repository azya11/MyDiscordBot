using DSharpPlus.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;
using MyTaskManagerBot.commands;
using System.Linq;
using System;
using MyTaskManagerBot;
using DSharpPlus.Interactivity.Extensions;
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
    private DSharpPlus.Interactivity.InteractivityExtension Interactivity
    => Program.Client.GetInteractivity();

    private List<Game> _gameList;
    // Constructor to initialize a new game with the channel ID
    public Game(ulong channelId, List<Game> gameList)
    {
        ChannelId = channelId;
        Players = new List<Player>();
        IsReady = false;
        _gameList = gameList;
    }
    private static readonly TimeSpan DefaultVoteTimeout = TimeSpan.FromSeconds(120);
    private static readonly Random Rng = new Random();
    //public async Task addMafia()
    //{
    //    this.mafia++;
    //}
    //public async Task addDoc()
    //{
    //    this.doctor++;
    //}
    //public async Task addCop()
    //{
    //    this.cop++;
    //}

    public async Task AddPlayer(DiscordMember playerId)
    {
        Player player = new Player(playerId);
        if (!Players.Contains(player)) //Check if the player is already in the game
        {
            Players.Add(player);
        }

    }

    public async Task StartGame()
    {
        AssignRoles();
        await SendRolesToPlayers();

        while (!IsGameOver())
        {
            await NightPhase();
            await DayPhase();
        }

        await AnnounceWinner();
        _gameList.Remove(this); // Удаление игры из списка
    }
    private async Task NightPhase()
    {
        // только живые игроки
        var alive = Players.Where(p => p.isLive).ToList();

        // mafia vote
        var mafia = alive.Where(p => p.role == 1).ToList();
        var target = await CollectVotes("Мафия, выберите цель", mafia, alive);

        // doctor heal
        var doctor = alive.FirstOrDefault(p => p.role == 2);
        var healed = doctor != null ? await CollectVote("Доктор, кого лечить?", doctor, alive) : null;

        // cop inspect
        var cop = alive.FirstOrDefault(p => p.role == 3);
        if (cop != null)
        {
            var suspect = await CollectVote("Комиссар, кого проверить?", cop, alive);
            if (suspect != null)
                await cop.Member.SendMessageAsync(suspect.role == 1 ? "Это мафия!" : "Это не мафия.");
        }

        // результат ночи
        if (target != null && target != healed)
        {
            target.isLive = false;
            var channel1 = await GetChannel();
            await channel1.SendMessageAsync($"💀 Игрок {target.Member.DisplayName} был убит этой ночью.");
        }
        else
        {
            var channel1 = await GetChannel();
            await channel1.SendMessageAsync("🌙 Никто не погиб этой ночью.");
        }
    }
    private async Task DayPhase()
    {
        var alive = Players.Where(p => p.isLive).ToList();
        var voteTarget = await CollectVotes("☀️ День наступил. Голосуйте, кого казнить.", alive, alive);

        if (voteTarget != null)
        {
            voteTarget.isLive = false;
            var channel1 = await GetChannel();
            await channel1.SendMessageAsync($"⚰️ Игрок {voteTarget.Member.DisplayName} был казнен по решению большинства.");
        }
    }
    private bool IsGameOver()
    {
        int mafia = Players.Count(p => p.isLive && p.role == 1);
        int civs = Players.Count(p => p.isLive && p.role != 1);
        return mafia == 0 || mafia >= civs;
    }
    private async Task AnnounceWinner()
    {
        int mafia = Players.Count(p => p.isLive && p.role == 1);
        string winner = mafia == 0 ? "Город победил! 🎉" : "Мафия победила! 💀";
        var channel = await GetChannel();
        await channel.SendMessageAsync($"🏆 {winner}");
    }

    public async Task AssignRoles()
    {
        int totalPlayers = Players.Count;
        int mafiaCount = Math.Max(1, totalPlayers / 4); // Примерное соотношение: 1 мафия на 4 игроков
        int doctorCount = 1;
        int copCount = 1;

        // Shuffle players
        Random rng = new Random();
        Players = Players.OrderBy(p => rng.Next()).ToList();

        for (int i = 0; i < totalPlayers; i++)
        {
            if (i < mafiaCount)
                Players[i].role = 1; // Mafia
            else if (i < mafiaCount + doctorCount)
                Players[i].role = 2; // Doctor
            else if (i < mafiaCount + doctorCount + copCount)
                Players[i].role = 3; // Cop
            else
                Players[i].role = 0; // Civillian
        }
    }
    private async Task SendRolesToPlayers()
    {
        foreach (var player in Players)
        {
            string roleName = "";

            switch (player.role)
            {
                case 1:
                    roleName = "You are MAFIA!";
                    break;
                case 2:
                    roleName = "You are DOCTOR!";
                    break;
                case 3:
                    roleName = "You are SHERIFF!";
                    break;
                default:
                    roleName = "You are CIVILLIAN!";
                    break;
            }

            await player.Member.SendMessageAsync($"📢 {roleName}");
        }
    }

    private async Task<Player> CollectVote(string prompt, Player voter, List<Player> options, TimeSpan? timeout = null)
    {
        // DM the voter with numbered options
        var dm = await voter.Member.CreateDmChannelAsync();
        var lines = string.Join("\n", options.Select((p, i) => $"{i + 1}. {p.Member.DisplayName}"));
        await dm.SendMessageAsync($"{prompt}\n\nОтветьте **числом** (1–{options.Count}) или упомяните игрока.\n\n{lines}");

        var limit = timeout ?? DefaultVoteTimeout;

        // Wait for a message from the voter in THIS DM
        var result = await Interactivity.WaitForMessageAsync(
            m => m.ChannelId == dm.Id && m.Author.Id == voter.Member.Id,
            limit
        );

        if (result.TimedOut || result.Result is null)
        {
            await dm.SendMessageAsync("⏰ Время вышло. Голос не засчитан.");
            return null;
        }

        var content = result.Result.Content.Trim();

        // 1) Try number
        if (int.TryParse(content, out var idx))
        {
            if (idx >= 1 && idx <= options.Count)
            {
                var pick = options[idx - 1];
                await dm.SendMessageAsync($"✅ Принято: **{pick.Member.DisplayName}**");
                return pick;
            }
            await dm.SendMessageAsync("⚠️ Неверный номер. Голос не засчитан.");
            return null;
        }

        // 2) Try mention
        if (result.Result.MentionedUsers.Any())
        {
            var id = result.Result.MentionedUsers.First().Id;
            var pick = options.FirstOrDefault(p => p.Member.Id == id);
            if (pick != null)
            {
                await dm.SendMessageAsync($"✅ Принято: **{pick.Member.DisplayName}**");
                return pick;
            }
            await dm.SendMessageAsync("⚠️ Такого игрока нет среди доступных.");
            return null;
        }

        // 3) Try exact name match
        var byName = options.FirstOrDefault(p =>
            string.Equals(p.Member.DisplayName, content, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(p.Member.Username, content, StringComparison.OrdinalIgnoreCase));

        if (byName != null)
        {
            await dm.SendMessageAsync($"✅ Принято: **{byName.Member.DisplayName}**");
            return byName;
        }

        await dm.SendMessageAsync("⚠️ Не распознал выбор. Голос не засчитан.");
        return null;
    }


    private async Task<Player> CollectVotes(string prompt, List<Player> voters, List<Player> options, TimeSpan? timeout = null)
    {
        // Run all voters concurrently (each via DM)
        var tasks = voters.Select(v => CollectVote(prompt, v, options, timeout)).ToList();
        var picks = await Task.WhenAll(tasks);

        // Tally valid votes
        var valid = picks.Where(p => p != null).ToList();
        if (valid.Count == 0) return null;

        // Plurality winner; break ties randomly;
        var groups = valid.GroupBy(p => p).OrderByDescending(g => g.Count()).ToList();
        var topCount = groups.First().Count();
        var topGroup = groups.Where(g => g.Count() == topCount).Select(g => g.Key).ToList();

        return topGroup.Count == 1
            ? topGroup[0]
            : topGroup[Rng.Next(topGroup.Count)];
    }

    private async Task<DiscordChannel> GetChannel()
    {
        return await Program.Client.GetChannelAsync(ChannelId);
    }



}
public class Player
{
    // 0 - Civillian
    // 1 - Mafia
    // 2 - Doctor
    // 3 - Cop
    public int role { get; set; }
    public DiscordMember Member { get; set; }
    public bool isLive { get; set; }

    public Player(DiscordMember Member)
    {
        this.Member = Member;
        this.role = 0;
        this.isLive = true;
    }
}