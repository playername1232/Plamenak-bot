using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
namespace Plamenak_Bot
{
    class Program
    {
        DiscordSocketClient _client;
        CommandService _commands;
        IServiceProvider _service;
        public const ulong PLAJA_ID = 423838753933623296;
        const ulong STEPANKA_ID = 262282949653692417;
        const ulong PLAMBOT_ID = 893799033154445342;
        const ulong PLAMENAK_ID = 182122343064141824;
        const ulong KREM_ID = 771084524196724797;
        const ulong JULIE_ID = 763774835255541781;
        const ulong MOORY_ID = 463995369420881921;
        const ulong JUMP_ID = 645373416668528652;

        public static readonly string MainDirectory = $@"D:\PlamenakBot";
        public static readonly string MessageDeleteDirectory = $@"{MainDirectory}\delmessages";
        public static readonly string PinsDirectory = $@"{MainDirectory}\pins";
        public static readonly string UnPinsDirectory = $@"{MainDirectory}\unpins";
        public static readonly string KicksDirectory = $@"{MainDirectory}\kicks";
        public static readonly string BansDirectory = $@"{MainDirectory}\bans";
        public static readonly string FilterDirectory = $@"{MainDirectory}\filters";

        public static DateTime LastPlamenak = DateTime.Now;

        static void Main(string[] args) => new Program().RunBotAsync().GetAwaiter().GetResult();

        public async Task RunBotAsync()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Title = "Lepší plameňák";

            DiscordSocketConfig config = new DiscordSocketConfig()
            {
                GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent
            };

            _client = new DiscordSocketClient(config);
            _commands = new CommandService();
            _service = new ServiceCollection().AddSingleton(_client).AddSingleton(_commands).BuildServiceProvider();

            // API Key changed as it was part of previous Github commits. Moved to file
            string token = File.ReadAllText($@"{Environment.CurrentDirectory}\APIKeys\plamenak_apikey.txt");

            _client.Log += Client_Log;

            await RegisterCommandsAsync();
            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();

            await _client.SetGameAsync("Jsem lepší plameňák", @"https://www.youtube.com/watch?v=yXqL9TtpDY4", ActivityType.Listening); // Lepší Plameňák

            Modules.Commands._client = _client;

            await Task.Delay(-1);
        }

        public Task Client_Log(LogMessage msg)
        {
            Console.WriteLine(msg);
            return Task.CompletedTask;
        }

        public async Task RegisterCommandsAsync()
        {
            _client.MessageReceived += HandleCommandAsync;
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _service);
        }
        
        private async Task HandleCommandAsync(SocketMessage msg)
        {
            try
            {
                SocketUserMessage message = msg as SocketUserMessage;
                SocketCommandContext context = new SocketCommandContext(_client, message);

                if (message.Author.IsBot && !message.Content.StartsWith("!delete"))
                    return;

                if (message.Content.ToLower().Contains("@everyone") || message.Content.ToLower().Contains("@here"))
                {
                    SocketGuildUser sgu = message.Author as SocketGuildUser;
                    if (!sgu.GuildPermissions.MentionEveryone)
                    {
                        if (!Directory.Exists($@"{MessageDeleteDirectory}"))
                            Directory.CreateDirectory($@"{MessageDeleteDirectory}");

                        string filename = $"deleted{DateTime.Now.Day}{DateTime.Now.Month}{DateTime.Now.Year}.txt";

                        StreamWriter sw = new StreamWriter($@"{MessageDeleteDirectory}\{filename}", append: true, encoding: Encoding.UTF8);
                        await sw.WriteLineAsync($"{DateTime.Now:G}\nServer: {context.Guild.ToString()}\n{message.Author.ToString()} - {message.Author.Id}\n{message.Content}\nIn {message.Channel}\nMessage id {message.Id}" +
                                                $"\n---------------------------------------------------------------------");

                        sw.Close();

                        await message.DeleteAsync();
                        Console.WriteLine($"A Message was deleted!\nCheck {filename}");
                        return;
                    }
                }

                if (message.Content.ToLower().Contains("uwu") 
                    || message.Content.ToLower().Contains("owo") 
                    || message.Content.ToLower().Contains("qwq") 
                    || message.Content.ToLower().Contains("twt"))
                {
                    string type = message.Content.ToLower().Contains("uwu") ? "UwU" : message.Content.ToLower().Contains("owo") ? "OwO" : message.Content.ToLower().Contains("qwq") ? "QwQ" : "TwT";
                    if (message.MentionedUsers.Any(x => x.Id == PLAMBOT_ID))
                    {
                        await message.Channel.SendMessageAsync($"{message.Author.Mention} {type}");
                        return;
                    }
                }
                
                int _argIndex = 0;

                if (message.HasStringPrefix("!", ref _argIndex))
                {

                    var result = await _commands.ExecuteAsync(context, _argIndex, _service);

                    if (!result.IsSuccess)
                        Console.WriteLine($"Chyba!\nException: {result.ErrorReason}");


                    if (message.Content.ToLower().StartsWith("!delete") && result.IsSuccess)
                        await message.DeleteAsync();

                    return;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"\nEXCEPTION!: {e.Message}\n{e.StackTrace}"); ;
            }
        }
    }
}
