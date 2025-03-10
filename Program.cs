using System;
using System.Threading.Tasks;
using System.Timers;
using Discord;
using Discord.WebSocket;

class Program
{
    private static async Task Main(string[] args)
    {
        var bot = new BuddyBot();
        await bot.RunAsync();
    }
}

public class BuddyBot
{
    private const string Prefix = "!"; // Define the bot's prefix
    private string? Token = Environment.GetEnvironmentVariable("DISCORD_BOT_TOKEN"); // Get token from environment variables

    private DiscordSocketClient? _client;
    private System.Timers.Timer? _BuddyTimer;
    private bool _isWorking;
    private ISocketMessageChannel? _channel;

    private const int WorkDuration = 25 * 60 * 1000;  // 25 minutes in milliseconds
    private const int BreakDuration = 5 * 60 * 1000;  // 5 minutes in milliseconds

    public async Task RunAsync()
    {
        _client = new DiscordSocketClient(new DiscordSocketConfig
{
    GatewayIntents = GatewayIntents.Guilds | GatewayIntents.GuildMessages | GatewayIntents.MessageContent
});
        _client.Log += Log;
        _client.Ready += Ready;
        _client.MessageReceived += MessageReceived;

        await _client.LoginAsync(TokenType.Bot, Token);
        await _client.StartAsync();

        // Keep the bot running
        await Task.Delay(-1);
    }

    private Task Log(LogMessage log)
    {
        Console.WriteLine(log);
        return Task.CompletedTask;
    }

    private Task Ready()
    {
        Console.WriteLine("✅ Bot is connected and ready!");
        return Task.CompletedTask;
    }

    private async Task MessageReceived(SocketMessage message)
    {
         Console.WriteLine($"Received message: {message.Content} from {message.Author}");
        if (message.Author.IsBot || !message.Content.StartsWith(Prefix)) return;

        await Task.Delay(1000); // Add a delay to prevent spamming

        string command = message.Content.Substring(Prefix.Length).ToLower().Trim(); // Extract the command

        if (command == "startbuddy")
        {
            await StartBuddyAsync(message.Channel);
        }
        else if (command == "stopbuddy")
        {
            await StopBuddyAsync(message.Channel);
        }
    }

    private async Task StartBuddyAsync(ISocketMessageChannel channel)
    {
        if (_isWorking)
        {
            await channel.SendMessageAsync("You are already in a Timebud session! Please wait until the current session ends.");
            return;
        }

        _isWorking = true;
        _channel = channel;

        await channel.SendMessageAsync("Timebud started! Focus for 25 minutes.");

        // Start the work timer (25 minutes)
        _BuddyTimer = new System.Timers.Timer(WorkDuration);
        _BuddyTimer.Elapsed += async (sender, e) => await EndWorkSessionAsync();
        _BuddyTimer.Start();
    }

    private async Task EndWorkSessionAsync()
{
    // Check if _BuddyTimer is not null before stopping it
    if (_BuddyTimer != null)
    {
        _BuddyTimer.Stop();
    }
    if (_channel != null)
    {
        await _channel.SendMessageAsync("Work session complete! Take a 5-minute break.");
    }

    // Start the break timer (5 minutes)
    _BuddyTimer = new System.Timers.Timer(BreakDuration);
    _BuddyTimer.Elapsed += async (sender, e) => await EndBreakSessionAsync();
    _BuddyTimer.Start();
}

private async Task EndBreakSessionAsync()
{
    // Check if _BuddyTimer is not null before stopping it
    if (_BuddyTimer != null)
    {
        _BuddyTimer.Stop();
    }
    if (_channel != null)
    {
        await _channel.SendMessageAsync("Break time is over! Ready to start another session?");
    }

    _isWorking = false;
}


    private async Task StopBuddyAsync(ISocketMessageChannel channel)
    {
        if (_BuddyTimer != null)
        {
            _BuddyTimer.Stop();
            await channel.SendMessageAsync("Timebud stopped.");
        }
        else
        {
            await channel.SendMessageAsync("No session is currently running.");
        }

        _isWorking = false;
    }
}
