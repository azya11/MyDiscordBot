using DSharpPlus.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;
using MyTaskManagerBot.commands;
using System.Linq;
using System;
using MyTaskManagerBot;

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
        Program.currentGames.Remove(this); // Удаление игры из списка
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
            await GetChannel().SendMessageAsync($"💀 Игрок {target.Member.DisplayName} был убит этой ночью.");
        }
        else
        {
            await GetChannel().SendMessageAsync("🌙 Никто не погиб этой ночью.");
        }
    }
    private async Task DayPhase()
    {
        var alive = Players.Where(p => p.isLive).ToList();
        var voteTarget = await CollectVotes("☀️ День наступил. Голосуйте, кого казнить.", alive, alive);

        if (voteTarget != null)
        {
            voteTarget.isLive = false;
            await GetChannel().SendMessageAsync($"⚰️ Игрок {voteTarget.Member.DisplayName} был казнен по решению большинства.");
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
        await GetChannel().SendMessageAsync($"🏆 {winner}");
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
                    roleName = "WASUP DOCTOR!";
                    break;
                case 3:
                    roleName = "HEY SHERIFF SHOULD WE SHOOT SOME NIGGERS?";
                    break;
                default:
                    roleName = "Your are just basic person, nothing crazy.";
                    break;
            }

            await player.Member.SendMessageAsync($"📢 {roleName}");
        }
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