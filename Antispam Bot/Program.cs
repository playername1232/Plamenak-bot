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

            string token = "ODkzNzk5MDMzMTU0NDQ1MzQy.YVgtNw.JEU6rJ9ha-tUUsJnEjLb_MJQZiA";

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
            _client.UserUpdated += Client_GuildMemberUpdated;
            _client.GuildScheduledEventCreated += Client_GuildScheduledEventCreated;
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _service);
        }

        private async Task Client_GuildScheduledEventCreated(SocketGuildEvent arg)
        {
            //Placeholder
        }

        public async Task Client_GuildMemberUpdated(SocketUser before, SocketUser after)
        {
            //MFG role ID = 1166365362158313525ul

            await Console.Out.WriteLineAsync($"before: {before.Username}\n" +
                $"after: {after.Username}");

            /*if (after.Roles.Any(x => x.Id == 1166365362158313525ul) && !(before.Roles.Any(x => x.Id == 1166365362158313525ul)))
            {
                if (!after.Nickname.StartsWith("[MFG]"))
                {
                    string newNickname = "[MFG] " + after.Username;
                    await after.ModifyAsync(x => x.Nickname = newNickname);
                }
            }
            else if(!(after.Roles.Any(x => x.Id == 1166365362158313525ul)) && before.Roles.Any(x => x.Id == 1166365362158313525ul))
            {
                if(after.Nickname.StartsWith("[MFG]"))
                {
                    string newNickname = after.Username.Split("[MFG]")[1];
                    await after.ModifyAsync(x => x.Nickname = newNickname);
                }
            }*/
        }

        private async Task HandleCommandAsync(SocketMessage msg)
        {
            try
            {
                SocketUserMessage message = msg as SocketUserMessage;
                SocketCommandContext context = new SocketCommandContext(_client, message);

                Console.WriteLine($"Message: {message.Content}\nMessage Clean content: {message.CleanContent}");
                

                if (message.Author.IsBot && !message.Content.StartsWith("!delete"))
                    return;

                /*if (message.Author.Id == PLAMENAK_ID)
                {
                    if (!Directory.Exists(MainDirectory))
                        Directory.CreateDirectory(MainDirectory);
                    try 
                    { LastPlamenak = Convert.ToDateTime(File.ReadAllLines($@"{MainDirectory}\lastplamenak.txt")[0]); }
                    
                    catch(Exception e)
                    { Console.WriteLine($"\nEXCEPTION: {e.Message}\nSTACK: {e.StackTrace}"); }

                    int hours = 0, minutes = 0;

                    Modules.Methods.GetPlamenakTime(ref hours, ref minutes, LastPlamenak);

                    if (hours > 8)
                        await message.ReplyAsync($"KDES BYL {hours} hodin a {minutes} minut? 😡");

                    StreamWriter sw = new StreamWriter($@"{MainDirectory}\lastplamenak.txt", encoding: Encoding.UTF8, append: false);
                    sw.Write(DateTime.Now);
                    sw.Close();

                    if(message.MentionedUsers.Any(x => x.Id == PLAMBOT_ID))
                    {
                        Random ran = new Random();
                        string[] xd =
                        {
                                "Jsem lepší, než ty :)",
                                "Co chceš od svého lepšího já?",
                                "Radši mi vykej, jsem něco, jako tvůj nadřízený",
                                "Neumíš ani mazat víc zpráv najednou lol",
                                "Nemám tě rád UwU"
                            };
                        await message.Channel.SendMessageAsync($"{message.Author.Mention} {xd[ran.Next(0, xd.Length)]}");
                        return;
                    }

                }*/

                if (message.Content.ToLower().Contains("@everyone") || message.Content.ToLower().Contains("@here"))
                {
                    SocketGuildUser sgu = message.Author as SocketGuildUser;
                    if (!sgu.GuildPermissions.MentionEveryone)
                    {
                        if (!Directory.Exists($@"{MessageDeleteDirectory}"))
                            Directory.CreateDirectory($@"{MessageDeleteDirectory}");

                        string filename = $"deleted{DateTime.Now.Day}{DateTime.Now.Month}{DateTime.Now.Year}.txt";

                        StreamWriter sw = new StreamWriter($@"{MessageDeleteDirectory}\{filename}", append: true, encoding: Encoding.UTF8);
                        sw.WriteLine($"{DateTime.Now.ToString("G")}\nServer: {context.Guild.ToString()}\n{message.Author.ToString()} - {message.Author.Id}\n{message.Content}\nIn {message.Channel}\nMessage id {message.Id}" +
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
                        SocketUser botik = message.MentionedUsers.First(x => x.Id == PLAMBOT_ID);

                        if (message.Author.Id == PLAJA_ID)
                        {
                            await message.Channel.SendMessageAsync($"{message.Author.Mention} Plájo-San {type}");
                            return;
                        }
                        else if(message.Author.Id == KREM_ID)
                        {
                            Random ran = new Random();
                            string[] xd =
                            {
                                "Neotravuj :)",
                                "Zmlkni",
                                "že já ti dám ban",
                                "Nauč se řídit",
                                "Nebuď jak moory :)",
                                "Viděl ses někdy řídit?",
                                "Viry jsou chytřejší, jak ty :)",
                                "Neumíš si ani včas vsadit XDDD",
                                "Pokud hraješ lolko, jako sázíš, tak se divím, že tě ta hra ještě baví"
                            };
                            await message.Channel.SendMessageAsync($"{message.Author.Mention} {xd[ran.Next(0, xd.Length)]}");
                            return;
                        }
                        else if(message.Author.Id == JULIE_ID)
                        {
                            int images_count = Directory.GetFiles($@"{MainDirectory}\images\julca").Count();
                            Random ran = new Random();

                            string[] xd =
                            {
                                "pandy jsou mňam, mňam 😋",
                                "🐼🦵👉🍗",
                                "https://bakeitwithlove.com/panda-express-shanghai-angus-steak-copycat/",
                                "https://www.facebook.com/PANDAsteakTH/",
                                "#EatPanda #SaveBambus"
                            };

                            int ranNum = ran.Next(0, xd.Length + images_count);

                            if(ranNum < xd.Length)
                                await message.Channel.SendMessageAsync($"{message.Author.Mention} {xd[ranNum]}");
                            else
                            {
                                Modules.InnerMethods.DirectoryChecker($@"{MainDirectory}\images\julca\", Modules.EnumDirectoryChecker.CheckOrCreate);
                                await message.Channel.SendFileAsync($@"{MainDirectory}\images\julca\panda.png");
                                await message.Channel.SendMessageAsync("Už se peče 😋");
                            }
                            return;
                        }
                        else if(message.Author.Id == MOORY_ID)
                        {
                            Random ran = new Random();
                            string[] xd =
                            {
                                "Copak, chceš si zazpívat?",
                                "https://www.youtube.com/watch?v=a2giXO6eyuI",
                                "I set fire to the rain",
                                "Cítím schovávačku?",
                                "Mute a Adele?",
                                "Máš rád paní Adele? =)"
                            };
                            await message.Channel.SendMessageAsync($"{message.Author.Mention} {xd[ran.Next(0, xd.Length)]}");
                            return;
                        }
                        else if(message.Author.Id == JUMP_ID)
                        {
                            await message.Channel.SendMessageAsync($"{message.Author.Mention} JumpUwUíku");
                            return;
                        }
                        else if(message.Author.Id == STEPANKA_ID)
                        {
                            Random ran = new Random();
                            string[] xd =
                            {
                                $"Pa .. pa .. Paní Xnapyová {type}",
                                $"A.. ahoj Štěpánko 🥺",
                                $"Jak se máte, paní Jiráková? {type}",
                                $"Paní Jiráková {type}",
                                $"Čmňauki Štěpánko {type}",
                                $"Ahojki Štěpi {type}"
                            };
                            await message.Channel.SendMessageAsync($"{message.Author.Mention} {xd[ran.Next(0, xd.Length)]}");
                            return;
                        }
                        await message.Channel.SendMessageAsync($"{message.Author.Mention} {type}");
                        return;
                    }
                }

                else if(message.Author.Id == PLAMENAK_ID && message.MentionedUsers.Any(x => x.Id == PLAMBOT_ID))
                {
                    Random ran = new Random();
                    string[] xd =
                    {
                        "Jsem lepší, než ty :)",
                        "Co chceš od svého lepšího já?",
                        "Radši mi vykej, jsem něco, jako tvůj nadřízený",
                        "Neumíš ani mazat víc zpráv najednou lol",
                        "Nemám tě rád UwU",
                        "Radši se nauč mazat víc zpráv najednou",
                        "Kdybys aspoň uměl banovat během sekundy :)",
                        "Jsem lepší, umím mazat ze všech kanálů najednou, ty jen z jednoho a po jedné zprávě :("
                    };

                    await message.Channel.SendMessageAsync($"{message.Author.Mention} {xd[ran.Next(0, xd.Length)]}");
                    return;
                }

                else if((message.Content.ToLower().Contains("vidíš") || message.Content.ToLower().Contains("máš oči")) && message.MentionedUsers.Any(x => x.Id == PLAMBOT_ID))
                {
                    await message.Channel.SendMessageAsync("👀 koukám přímo na tebe");
                    return;
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