using System;
using System.Threading.Tasks;
using System.Timers;
using Discord;
using Discord.WebSocket;

class Program
{
    private static async Task Main(string[] args)
    {
        var bot = new PomodoroBot();
        await bot.RunAsync();
    }
}

public class PomodoroBot
{
    private const string Prefix = "!"; // Define the bot's prefix
    private string? Token = Environment.GetEnvironmentVariable("DISCORD_BOT_TOKEN"); // Get token from environment variables

    private DiscordSocketClient? _client;
    private System.Timers.Timer? _pomodoroTimer;
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

        if (command == "startpomodoro")
        {
            await StartPomodoroAsync(message.Channel);
        }
        else if (command == "stoppomodoro")
        {
            await StopPomodoroAsync(message.Channel);
        }
    }

    private async Task StartPomodoroAsync(ISocketMessageChannel channel)
    {
        if (_isWorking)
        {
            await channel.SendMessageAsync("You are already in a Pomodoro session! Please wait until the current session ends.");
            return;
        }

        _isWorking = true;
        _channel = channel;

        await channel.SendMessageAsync("Pomodoro started! Focus for 25 minutes.");

        // Start the work timer (25 minutes)
        _pomodoroTimer = new System.Timers.Timer(WorkDuration);
        _pomodoroTimer.Elapsed += async (sender, e) => await EndWorkSessionAsync();
        _pomodoroTimer.Start();
    }

    private async Task EndWorkSessionAsync()
{
    // Check if _pomodoroTimer is not null before stopping it
    if (_pomodoroTimer != null)
    {
        _pomodoroTimer.Stop();
    }
    if (_channel != null)
    {
        await _channel.SendMessageAsync("Work session complete! Take a 5-minute break.");
    }

    // Start the break timer (5 minutes)
    _pomodoroTimer = new System.Timers.Timer(BreakDuration);
    _pomodoroTimer.Elapsed += async (sender, e) => await EndBreakSessionAsync();
    _pomodoroTimer.Start();
}

private async Task EndBreakSessionAsync()
{
    // Check if _pomodoroTimer is not null before stopping it
    if (_pomodoroTimer != null)
    {
        _pomodoroTimer.Stop();
    }
    if (_channel != null)
    {
        await _channel.SendMessageAsync("Break time is over! Ready to start another Pomodoro?");
    }

    _isWorking = false;
}


    private async Task StopPomodoroAsync(ISocketMessageChannel channel)
    {
        if (_pomodoroTimer != null)
        {
            _pomodoroTimer.Stop();
            await channel.SendMessageAsync("Pomodoro stopped.");
        }
        else
        {
            await channel.SendMessageAsync("No Pomodoro session is currently running.");
        }

        _isWorking = false;
    }
}
