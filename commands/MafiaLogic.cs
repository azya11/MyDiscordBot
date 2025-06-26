using DSharpPlus.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;
using MyTaskManagerBot.commands;

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