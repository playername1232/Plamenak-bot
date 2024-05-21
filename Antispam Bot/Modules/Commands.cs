using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Globalization;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Antispam_Bot.Modules;
using System.Runtime.CompilerServices;
using System.Xml.Schema;
using System.Security.Cryptography;
using System.Data;
using System.Threading.Channels;

namespace Plamenak_Bot.Modules
{
    [Patch("2.2.1")]
    public class Commands : ModuleBase<SocketCommandContext>
    {
        public static DiscordSocketClient _client;

        [Command("test")]
        public async Task Test() => await ReplyAsync("Test 1 . 2 . 3");

        [Command("patch")]
        public async Task GetCurrentPatch()
        {
            await ReplyAsync($"Aktuální Patch je: {GetAttribute(typeof(Commands))}");
        }

        private static string GetAttribute(Type t)
        {
            PatchAttribute patch = (PatchAttribute)Attribute.GetCustomAttribute(t, typeof(PatchAttribute));
             if (patch != null)
                 return patch.Patch;

             Console.WriteLine("Invalid attribute");
             return default;
        }

        [RequireUserPermission(GuildPermission.Administrator)]
        [Command("killswitch")]
        public async Task KillSwitch()
        {
            await Context.Message.ReplyAsync("Vypínám se o7");
            Environment.Exit(0);
        }

        [Command("serializeInt64")]
        public async Task SerializeInt64()
        {
            ulong[] numbers = { 78484, 61511, 1, 5445, 15, 1, 51, 515, 5456 };

            string serialized = JsonSerializer.Serialize(numbers);
            File.WriteAllText($@"{Environment.CurrentDirectory}\SerializationTest.json", serialized);
        }

        [Command("currentguild")]
        public async Task CurrentGuild() => await Context.Message.ReplyAsync($"{Context.Guild.ToString()}");

        [Command("whereami")]
        public async Task WhereAmI() => System.Console.WriteLine($"{Environment.CurrentDirectory}");

        [Command("myid")]
        public async Task MyID()
        {
            await ReplyAsync($"{Context.Message.Author.Username}#{Context.Message.Author.Discriminator}");
        }

        [Command("getroot")]
        public async Task GetFileRootDirectory(params string[] _) => await Context.Message.ReplyAsync($"Root file directory = \"{Program.MainDirectory}\"");

        //mm/dd/yyyy
        [Command("lastplamenak")]
        public async Task LastPlamenak(string date, string time)
        {
            Console.WriteLine("Temporary disabled");
            return;

            if (Context.Message.Author.Id != Program.PLAJA_ID)
                return;

            try
            {
                if (!Directory.Exists(Program.MainDirectory))
                    Directory.CreateDirectory(Program.MainDirectory);

                int month = int.Parse(date.Split('/')[0]);
                int day = int.Parse(date.Split('/')[1]);
                int year = int.Parse(date.Split('/')[2]);

                int hours = int.Parse(time.Split(':')[0]);
                int minutes = int.Parse(time.Split(':')[1]);

                date = $"{month}/{day}/{year} {hours}:{minutes}:00";

                StreamWriter sw = new StreamWriter($@"{Program.MainDirectory}\lastplamenak.txt", encoding: Encoding.UTF8, append: false);
                sw.Write(date);
                sw.Close();

                IUserMessage message = await ReplyAsync("Done");

            }
            catch (Exception e)
            {
                await ReplyAsync($"Exception UwU čekni ConsolUwU");

                Console.WriteLine($"\nEXCEPTION: {e.Message}\nSTACK: {e.StackTrace}");
            }
        }

        [Command("getlastplamenak")]
        public async Task GetLastPlamenak()
        {
            Console.WriteLine("Temporary disabled");
            return;

            if (!Directory.Exists(Program.MainDirectory))
                Directory.CreateDirectory(Program.MainDirectory);
            DateTime LastPlamenak = DateTime.Now;

            try
            { LastPlamenak = Convert.ToDateTime(File.ReadAllLines($@"{Program.MainDirectory}\lastplamenak.txt")[0]); }

            catch (Exception e)
            { Console.WriteLine($"\nEXCEPTION: {e.Message}\nSTACK: {e.StackTrace}"); }

            int hours = 0, minutes = 0;

            TimePlamenakMethods.GetPlamenakTime(ref hours, ref minutes, LastPlamenak);

            await ReplyAsync($"Plameňák naposledy napsal: {LastPlamenak.ToString("G")}\nDoba od poslední zprávy {hours} hodin a {minutes} minut");
        }

        [Command("copy")]
        public async Task CopyMessage(string message) => Console.WriteLine($"Copied message: {message}");

        [Command("choose")]
        public async Task ChooseOption(params string[] options)
        {
            Random ran = new Random();

            await Context.Message.ReplyAsync($"{options[ran.Next(0, options.Length)]}");
        }

        [RequireUserPermission(GuildPermission.Administrator, ErrorMessage = "User is not Administrator!")]
        [Command("SendMessagesToChannelsInCategory")]
        public async Task SendMessagesToChannelsInCategory(ulong categoryID, string message)
        {
            SocketCategoryChannel category = Context.Guild.GetCategoryChannel(categoryID);
            if (category == null)
            {
                await ReplyAsync($"Kategorie s ID {categoryID} Neexistuje!");
                return;
            }

            category.Channels.ToList().ForEach(x => (x as IMessageChannel).SendMessageAsync(message));
        }
        [RequireUserPermission(GuildPermission.Administrator, ErrorMessage = "User is not administrator!")]
        [Command("createchannels")]
        public async Task CreateChannels(ulong categoryID, string channelPrefix, int start, int end)
        {
            if (start > end)
                (start, end) = (end, start);

            SocketCategoryChannel category = Context.Guild.GetCategoryChannel(categoryID);
            if(category == null)
            {
                await ReplyAsync($"Kategorie s ID {categoryID} Neexistuje!");
                return;
            }

            for(int i = start; i <= end; i++)
            {
                Debug.WriteLine($"Creating channel {channelPrefix}{i} in category = {categoryID}");
                await Context.Guild.CreateTextChannelAsync($"{channelPrefix}{i}", x => x.CategoryId = categoryID);
            }
        }
        [RequireUserPermission(GuildPermission.Administrator, ErrorMessage = "User is not administrator!")]
        [Command("deletechannels")]
        public async Task DeleteChannels(ulong categoryID)
        {
            SocketCategoryChannel category = Context.Guild.GetCategoryChannel(categoryID);
            if (category == null)
            {
                await ReplyAsync($"Kategorie s ID {categoryID} Neexistuje!");
                return;
            }

            List<SocketGuildChannel> channels = category.Channels.ToList();

            channels.ForEach(async channel => await channel.DeleteAsync());
        }

        [Command("addrole")]
        public async Task AddRole(SocketRole roleSocket, params SocketUser[] users)
        {
            //var _role = Context.Guild.GetRole(roleID);

            foreach (SocketUser user in users)
            {
                try
                {
                    await (user as IGuildUser).AddRoleAsync(roleSocket);
                    Console.WriteLine($"Adding {user.Username}#{user.DiscriminatorValue} to {roleSocket.Name}");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            Console.WriteLine("Finished!");
            await Context.Message.Channel.SendMessageAsync("Done'n done!");
        }

        [RequireUserPermission(GuildPermission.Administrator, ErrorMessage = "User is not administrator!")]
        [Command("addrole")]
        public async Task AddRole(SocketRole roleSocket, params string[] entry)
        {
            List<ulong> users = new List<ulong>();

            foreach(string pom in entry)
            {
                string _innerEntry = pom;

                if (pom.Contains("><"))
                    _innerEntry = pom.Replace("><", "> <");
            }

            for(int i = 0; i < entry.Length; i++)
            {
                try
                {
                    if (entry[i].Contains("><"))
                    {
                        entry[i].Replace("><", "> <");
                        string[] xd = entry[i].Split(" ");

                        for (int j = 0; j < xd.Length; j++)
                        {
                            if (DiscordMethods.IsDiscordID(xd[j]))
                                users.Add(DiscordMethods.GetDiscordID(xd[j]));
                        }
                    }
                    else
                    {
                        if (DiscordMethods.IsDiscordID(entry[i]))
                            users.Add(DiscordMethods.GetDiscordID(entry[i]));
                    }
                }
                catch(Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }

            foreach(ulong uID in users)
            {
                try
                {
                    SocketUser user = Context.Guild.GetUser(uID);

                    await (user as IGuildUser).AddRoleAsync(roleSocket);
                }
                catch(Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }

        [RequireUserPermission(GuildPermission.Administrator, ErrorMessage = "User is not administrator!")]
        [Command("addrole")]
        public async Task AddRole(SocketRole roleSocket, string filePath)
        {
            //var _role = Context.Guild.GetRole(roleID);

            if(filePath.ToLower().StartsWith("root:"))
            {
                filePath = filePath.TrimStart("root:".ToCharArray());
                filePath = $@"{Program.MainDirectory}\{filePath}";
            }

            string[] fileLines = File.ReadAllLines(filePath);

            //SocketUser[] users = new SocketUser[fileLines.Length];

            for (int i = 0; i < fileLines.Length; i++)
            {
                string line = "";
                try
                {
                    line = fileLines[i];

                    line = line.Replace("# ", "#");
                    line = line.Replace(" #", "#");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }

                var users = Context.Guild.Users;

                users = users.OrderBy(x => x.Username[0]).ThenBy(y => y.Username[1]).ToList();

                foreach (SocketUser user in users)
                {
                    try
                    {
                        string strDiscriminatorValue = user.DiscriminatorValue.ToString().Length == 3 ? $"0{user.DiscriminatorValue.ToString()}" :
                            user.DiscriminatorValue.ToString().Length == 2 ? $"00{user.DiscriminatorValue.ToString()}" :
                            user.DiscriminatorValue.ToString().Length == 1 ? $"00{user.DiscriminatorValue.ToString()}" :
                            user.DiscriminatorValue.ToString();

                        Console.WriteLine($"Comparing {user.Username}#{strDiscriminatorValue} == {fileLines[i]} = {(user.Username + "#" + strDiscriminatorValue) == fileLines[i]}");
                        if ($"{user.Username}#{strDiscriminatorValue}".ToLower() == line.ToLower())
                        {
                            await (user as IGuildUser).AddRoleAsync(roleSocket);
                            Console.WriteLine($"Adding {user.Username}#{strDiscriminatorValue} to {roleSocket.Name}");
                            break;
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }
            }
            Console.WriteLine("Finished!");
            await Context.Message.Channel.SendMessageAsync("Done'n done!");
        }

        [RequireUserPermission(GuildPermission.Administrator, ErrorMessage = "User is not administrator!")]
        [Command("removerole")]
        public async Task RemoveRole(SocketRole roleSocket)
        {
            try
            {
                var users = roleSocket.Members;

                foreach (SocketUser user in users)
                {
                    await (user as IGuildUser).RemoveRoleAsync(roleSocket);
                    Console.WriteLine($"Removing {user.Username}#{user.DiscriminatorValue} from {roleSocket.Name}");
                }
                Console.WriteLine("Finished!");
                await Context.Message.Channel.SendMessageAsync("Done'n done!");
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        [RequireUserPermission(GuildPermission.Administrator, ErrorMessage = "User is not administrator!")]
        [Command("removerole")]
        public async Task RemoveRole(SocketRole roleSocket, params SocketUser[] users)
        {
            try
            {
                foreach (SocketUser user in users)
                {
                    await (user as IGuildUser).RemoveRoleAsync(roleSocket);
                    Console.WriteLine($"Removing {user.Username}#{user.DiscriminatorValue} from {roleSocket.Name}");
                }
                Console.WriteLine("Finished!");
                await Context.Message.Channel.SendMessageAsync("Done'n done!");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        [RequireUserPermission(GuildPermission.Administrator, ErrorMessage = "User is not administrator!")]
        [Command("removeroles")]
        public async Task RemoveRoles(params SocketRole[] roles)
        {
            foreach (SocketRole role in roles)
            {
                try
                {
                    foreach(SocketUser user in role.Members)
                    {
                        try
                        {
                            await (user as IGuildUser).RemoveRoleAsync(role);
                            Console.WriteLine($"Removing {user.Username}#{user.DiscriminatorValue} from {role.Name}");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                    }
                }
                catch(Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            Console.WriteLine("Finished!");
            await Context.Message.Channel.SendMessageAsync("Done'n done!");
        }

        [RequireUserPermission(GuildPermission.Administrator, ErrorMessage = "User is not administrator!")]
        [Command("assignroom")]
        public async Task AssignRoom(SocketTextChannel textSocketChannel, params SocketUser[] users)
        {
            foreach(SocketUser user in users)
            {
                try
                {
                    OverwritePermissions perms = new OverwritePermissions(
                        PermValue.Deny, PermValue.Deny, PermValue.Allow, PermValue.Allow, PermValue.Allow,
                        PermValue.Deny, PermValue.Deny, PermValue.Allow, PermValue.Allow, PermValue.Allow,
                        PermValue.Allow, PermValue.Allow, PermValue.Allow, PermValue.Allow, PermValue.Deny, PermValue.Deny, 
                        PermValue.Deny, PermValue.Deny, PermValue.Deny,
                        PermValue.Deny, PermValue.Deny, PermValue.Allow);

                    await textSocketChannel.AddPermissionOverwriteAsync(user, perms);

                    string strDiscriminatorValue = user.DiscriminatorValue.ToString().Length == 3 ? $"0{user.DiscriminatorValue.ToString()}" :
                         user.DiscriminatorValue.ToString().Length == 2 ? $"00{user.DiscriminatorValue.ToString()}" :
                         user.DiscriminatorValue.ToString().Length == 1 ? $"00{user.DiscriminatorValue.ToString()}" :
                         user.DiscriminatorValue.ToString();

                    Console.WriteLine($"Adding {user.Username}#{strDiscriminatorValue} to {textSocketChannel.Name} channel");
                }
                catch(Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            Console.WriteLine("Finished!");
            await Context.Message.Channel.SendMessageAsync("Done'n done!");
        }

        [RequireUserPermission(GuildPermission.Administrator, ErrorMessage = "User is not administrator!")]
        [Command("assignroom")]
        public async Task AssignRoom (SocketTextChannel textSocketChannel, params string[] entry)
        {
            List<ulong> users = new List<ulong>();

            foreach (string pom in entry)
            {
                string _innerEntry = pom;

                if (pom.Contains("><"))
                    _innerEntry = pom.Replace("><", "> <");
            }

            for (int i = 0; i < entry.Length; i++)
            {
                try
                {
                    if (entry[i].Contains("><"))
                    {
                        entry[i].Replace("><", "> <");
                        string[] xd = entry[i].Split(" ");

                        for (int j = 0; j < xd.Length; j++)
                        {
                            if (DiscordMethods.IsDiscordID(xd[j]))
                            {
                                users.Add(DiscordMethods.GetDiscordID(xd[j]));

                                SocketUser user = Context.Guild.GetUser(ulong.Parse(xd[j]));

                                string strDiscriminatorValue = user.DiscriminatorValue.ToString().Length == 3 ? $"0{user.DiscriminatorValue.ToString()}" :
                                    user.DiscriminatorValue.ToString().Length == 2 ? $"00{user.DiscriminatorValue.ToString()}" :
                                    user.DiscriminatorValue.ToString().Length == 1 ? $"00{user.DiscriminatorValue.ToString()}" :
                                    user.DiscriminatorValue.ToString();

                                Console.WriteLine($"Adding {user.Username}#{strDiscriminatorValue} to {textSocketChannel.Name} channel");
                            }
                        }
                    }
                    else
                    {
                        if (DiscordMethods.IsDiscordID(entry[i]))
                            users.Add(DiscordMethods.GetDiscordID(entry[i]));
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }

            foreach (ulong uID in users)
            {
                try
                {
                    SocketUser user = Context.Guild.GetUser(uID);

                    OverwritePermissions perms = new OverwritePermissions(
                        PermValue.Deny, PermValue.Deny, PermValue.Allow, PermValue.Allow, PermValue.Allow,
                        PermValue.Deny, PermValue.Deny, PermValue.Allow, PermValue.Allow, PermValue.Allow,
                        PermValue.Allow, PermValue.Allow, PermValue.Allow, PermValue.Allow, PermValue.Deny, PermValue.Deny,
                        PermValue.Deny, PermValue.Deny, PermValue.Deny,
                        PermValue.Deny, PermValue.Deny, PermValue.Allow);

                    await textSocketChannel.AddPermissionOverwriteAsync(user, perms);

                    string strDiscriminatorValue = user.DiscriminatorValue.ToString().Length == 3 ? $"0{user.DiscriminatorValue.ToString()}" :
                        user.DiscriminatorValue.ToString().Length == 2 ? $"00{user.DiscriminatorValue.ToString()}" :
                        user.DiscriminatorValue.ToString().Length == 1 ? $"00{user.DiscriminatorValue.ToString()}" :
                        user.DiscriminatorValue.ToString();

                    Console.WriteLine($"Adding {user.Username}#{strDiscriminatorValue} to {textSocketChannel.Name} channel");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }

        [RequireUserPermission(GuildPermission.Administrator, ErrorMessage = "User is not administrator!")]
        [Command("assignroom")]
        public async Task AssignRoom(SocketTextChannel textSocketChannel, string filePath)
        {
            //var _role = Context.Guild.GetRole(roleID);

            if (filePath.ToLower().StartsWith("root:"))
            {
                filePath = filePath.TrimStart("root:".ToCharArray());
                filePath = $@"{Program.MainDirectory}\{filePath}";
            }

            string[] fileLines = File.ReadAllLines(filePath);

            //SocketUser[] users = new SocketUser[fileLines.Length];

            for (int i = 0; i < fileLines.Length; i++)
            {
                string line = "";
                try
                {
                    line = fileLines[i];

                    line = line.Replace("# ", "#");
                    line = line.Replace(" #", "#");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }

                var users = Context.Guild.Users;

                users = users.OrderBy(x => x.Username[0]).ThenBy(y => y.Username[1]).ToList();

                foreach (SocketUser user in users)
                {
                    try
                    {
                        string strDiscriminatorValue = user.DiscriminatorValue.ToString().Length == 3 ? $"0{user.DiscriminatorValue.ToString()}" :
                            user.DiscriminatorValue.ToString().Length == 2 ? $"00{user.DiscriminatorValue.ToString()}" :
                            user.DiscriminatorValue.ToString().Length == 1 ? $"00{user.DiscriminatorValue.ToString()}" :
                            user.DiscriminatorValue.ToString();

                        Console.WriteLine($"Comparing {user.Username}#{strDiscriminatorValue} == {fileLines[i]} = {(user.Username + "#" + strDiscriminatorValue) == fileLines[i]}");
                        if ($"{user.Username}#{strDiscriminatorValue}".ToLower() == line.ToLower())
                        {
                            OverwritePermissions perms = new OverwritePermissions(
                                PermValue.Deny, PermValue.Deny, PermValue.Allow, PermValue.Allow, PermValue.Allow,
                                PermValue.Deny, PermValue.Deny, PermValue.Allow, PermValue.Allow, PermValue.Allow,
                                PermValue.Allow, PermValue.Allow, PermValue.Allow, PermValue.Allow, PermValue.Deny, PermValue.Deny,
                                PermValue.Deny, PermValue.Deny, PermValue.Deny,
                                PermValue.Deny, PermValue.Deny, PermValue.Allow);

                            await textSocketChannel.AddPermissionOverwriteAsync(user, perms);
                            Console.WriteLine($"Adding {user.Username}#{strDiscriminatorValue} to {textSocketChannel.Name} channel");
                            break;
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }
            }
            Console.WriteLine("Finished!");
            await Context.Message.Channel.SendMessageAsync("Done'n done!");
        }

        [RequireUserPermission(GuildPermission.Administrator, ErrorMessage = "User is not administrator!")]
        [Command("unassignroom")]
        public async Task UnassignRoom(SocketTextChannel textSocketChannel)
        {
            foreach (SocketUser user in textSocketChannel.Users)
            {
                if(!(user as IGuildUser).GuildPermissions.Administrator)
                {

                    await textSocketChannel.RemovePermissionOverwriteAsync(user);

                    string strDiscriminatorValue = user.DiscriminatorValue.ToString().Length == 3 ? $"0{user.DiscriminatorValue.ToString()}" :
                        user.DiscriminatorValue.ToString().Length == 2 ? $"00{user.DiscriminatorValue.ToString()}" :
                        user.DiscriminatorValue.ToString().Length == 1 ? $"00{user.DiscriminatorValue.ToString()}" :
                        user.DiscriminatorValue.ToString();

                    Console.WriteLine($"Removing {user.Username}#{strDiscriminatorValue} from {textSocketChannel.Name} channel");
                }
            }


        }

        [RequireUserPermission(GuildPermission.Administrator, ErrorMessage = "User is not administrator!")]
        [Command("unassignroom")]
        public async Task UnassignRoom(SocketTextChannel textSocketChannel, params SocketUser[] users)
        {
            foreach(SocketUser user in users)
            {
                await textSocketChannel.RemovePermissionOverwriteAsync(user);

                string strDiscriminatorValue = user.DiscriminatorValue.ToString().Length == 3 ? $"0{user.DiscriminatorValue.ToString()}" :
                    user.DiscriminatorValue.ToString().Length == 2 ? $"00{user.DiscriminatorValue.ToString()}" :
                    user.DiscriminatorValue.ToString().Length == 1 ? $"00{user.DiscriminatorValue.ToString()}" :
                    user.DiscriminatorValue.ToString();

                Console.WriteLine($"Removing {user.Username}#{strDiscriminatorValue} from {textSocketChannel.Name} channel");
            }

            Console.WriteLine("Finished!");
            await Context.Message.Channel.SendMessageAsync("Done'n done!");
        }

        [RequireUserPermission(GuildPermission.Administrator, ErrorMessage = "User is not administrator!")]
        [Command("unassignroom")]
        public async Task UnassignRoom(SocketTextChannel textSocketChannel, string filePath)
        {
            //var _role = Context.Guild.GetRole(roleID);

            if (filePath.ToLower().StartsWith("root:"))
            {
                filePath = filePath.TrimStart("root:".ToCharArray());
                filePath = $@"{Program.MainDirectory}\{filePath}";
            }

            string[] fileLines = File.ReadAllLines(filePath);

            //SocketUser[] users = new SocketUser[fileLines.Length];

            for (int i = 0; i < fileLines.Length; i++)
            {
                string line = "";
                try
                {
                    line = fileLines[i];

                    line = line.Replace("# ", "#");
                    line = line.Replace(" #", "#");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }

                var users = Context.Guild.Users;

                users = users.OrderBy(x => x.Username[0]).ThenBy(y => y.Username[1]).ToList();

                foreach (SocketUser user in users)
                {
                    try
                    {
                        string strDiscriminatorValue = user.DiscriminatorValue.ToString().Length == 3 ? $"0{user.DiscriminatorValue.ToString()}" :
                            user.DiscriminatorValue.ToString().Length == 2 ? $"00{user.DiscriminatorValue.ToString()}" :
                            user.DiscriminatorValue.ToString().Length == 1 ? $"00{user.DiscriminatorValue.ToString()}" :
                            user.DiscriminatorValue.ToString();

                        Console.WriteLine($"Comparing {user.Username}#{strDiscriminatorValue} == {fileLines[i]} = {(user.Username + "#" + strDiscriminatorValue) == fileLines[i]}");
                        if ($"{user.Username}#{strDiscriminatorValue}".ToLower() == line.ToLower())
                        {

                            await textSocketChannel.RemovePermissionOverwriteAsync(user);

                            Console.WriteLine($"Removing {user.Username}#{strDiscriminatorValue} from {textSocketChannel.Name} channel");

                            break;
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }
            }
            Console.WriteLine("Finished!");
            await Context.Message.Channel.SendMessageAsync("Done'n done!");
        }

        [RequireUserPermission(GuildPermission.Administrator, ErrorMessage = "User is not administrator!")]
        [Command("unassignroom")]
        public async Task UnassignRoom(params SocketTextChannel[] textSocketChannel)
        {
            foreach(SocketTextChannel _innerTextSocketChannel in textSocketChannel)
            {
                foreach (SocketUser user in _innerTextSocketChannel.Users)
                {
                    if (!(user as IGuildUser).GuildPermissions.Administrator)
                    {

                        await _innerTextSocketChannel.RemovePermissionOverwriteAsync(user);

                        string strDiscriminatorValue = user.DiscriminatorValue.ToString().Length == 3 ? $"0{user.DiscriminatorValue.ToString()}" :
                            user.DiscriminatorValue.ToString().Length == 2 ? $"00{user.DiscriminatorValue.ToString()}" :
                            user.DiscriminatorValue.ToString().Length == 1 ? $"00{user.DiscriminatorValue.ToString()}" :
                            user.DiscriminatorValue.ToString();

                        Console.WriteLine($"Removing {user.Username}#{strDiscriminatorValue} from {_innerTextSocketChannel.Name} channel");
                    }
                }
            }
        }

        [Command("kostka")]
        public async Task GiveRandomNumber(int min, int max)
        {
            if(min > max)
            {
                int pom = min;
                min = max;
                max = pom;
            }

            Random ran = new Random();

            await Context.Message.ReplyAsync($"{ran.Next(min, max+1)}");

        }

        [Command("status")]
        public async Task PrintStatusAsync()
        {
            //Moje ID: 423838753933623296

            if (Context.User.Id != 423838753933623296)
            {
                return;
            }

            int ping = Context.Client.Latency;

            if (ping <= 250)
            {
                await ReplyAsync($"```yaml\n+ ✔️ Ping latency {ping}ms ✔️```");
            }
            else if (ping > 250 && ping <= 500)
            {
                await ReplyAsync($"```fix\n- ⚠️ Ping latency {ping}ms ⚠️```");
            }
            else
            {
                await ReplyAsync($"```diff\n- ❌ Ping latency {ping}ms ❌```");
            }
        }

        [Command("pin")]
        public async Task PinMessage(params string[] _)
        {
            if (Context.Message.ReferencedMessage != null)
            {
                if (!Context.Message.ReferencedMessage.IsPinned)
                {
                    await Context.Message.ReferencedMessage.PinAsync();

                    InnerMethods.DirectoryChecker(new string[] { Program.PinsDirectory }, EnumDirectoryChecker.CheckOrCreate);

                    StreamWriter sw = new StreamWriter($@"{Program.PinsDirectory}\{DateTime.Now.Month}{DateTime.Now.Year}.txt", encoding: Encoding.UTF8, append: true);
                    sw.WriteLine($"{DateTime.Now.ToString("G")}\n{Context.Message.Author.ToString()} has pinned following message" +
                        $"\nAuthor -  {Context.Message.ReferencedMessage.Author}" +
                        $"\nContent - {Context.Message.ReferencedMessage.Content}\nin {Context.Message.Channel}" +
                        $"\n---------------------------------------------------------------------");
                    sw.Close();
                }
            }
        }

        [Command("unpin")]
        public async Task UnPinMessage(params string[] _)
        {
            if (Context.Message.ReferencedMessage != null)
            {
                if (Context.Message.ReferencedMessage.IsPinned)
                {
                    await Context.Message.ReferencedMessage.UnpinAsync();

                    InnerMethods.DirectoryChecker(new string[] { Program.UnPinsDirectory }, EnumDirectoryChecker.CheckOrCreate);

                    StreamWriter sw = new StreamWriter($@"{Program.UnPinsDirectory}\{DateTime.Now.Month}{DateTime.Now.Year}.txt", encoding: Encoding.UTF8, append: true);
                    sw.WriteLine($"{DateTime.Now.ToString("G")}\n{Context.Message.Author.ToString()} has unpinned following message" +
                        $"\nAuthor -  {Context.Message.ReferencedMessage.Author}" +
                        $"\nContent - {Context.Message.ReferencedMessage.Content}\nin {Context.Message.Channel}" +
                        $"\n---------------------------------------------------------------------");
                    sw.Close();
                }
            }
        }

        [Command("del")]
        [RequireUserPermission(ChannelPermission.ManageMessages)]
        [RequireBotPermission(ChannelPermission.ManageMessages)]
        public async Task DeleteRepliedMessageAsync()
        {
            if (Context.Message.ReferencedMessage != null)
                await Context.Message.ReferencedMessage.DeleteAsync();

            await Context.Message.DeleteAsync();
        }

        [Command("delete")]
        [Alias("clean")]
        [Summary("Vymaže určitý počet zpráv v daném kanále.")]
        [RequireUserPermission(ChannelPermission.ManageMessages)]
        [RequireBotPermission(ChannelPermission.ManageMessages)]
        public async Task MessagesDeleteAsync(SocketUser auth = null, int _count = -5) => await MessagesDeleteAsync(_count, auth);

        [Command("delete")]
        [Alias("clean")]
        [Summary("Vymaže určitý počet zpráv v daném kanále.")]
        [RequireUserPermission(ChannelPermission.ManageMessages)]
        [RequireBotPermission(ChannelPermission.ManageMessages)]
        public async Task MessagesDeleteAsync(int _count = -1, SocketUser auth = null)
        {
            bool authIsNull = auth == null;

            if (_count <= 0)
            {
                await ReplyAsync("Počet zpráv musí být větší nule!");
                return;
            }
            if (_count > 100)
                _count = 100;

            int max = _count;
            _count = (!authIsNull) ? 50 : _count;

            IEnumerable<IMessage> messages = await Context.Channel.GetMessagesAsync(Context.Message, Direction.Before, _count).FlattenAsync();


            IEnumerable<IMessage> filteredMessages = (authIsNull) ? messages.Where(x => (DateTimeOffset.UtcNow - x.Timestamp).TotalDays <= 14)
                : messages.Where(x => (DateTimeOffset.UtcNow - x.Timestamp).TotalDays <= 14 && x.Author == auth);

            if (filteredMessages.Count() == 0)
            {
                await ReplyAsync("Není nic k vymazání!");
                return;
            }

            if (authIsNull)
                await (Context.Channel as ITextChannel).DeleteMessagesAsync(filteredMessages);
            else
            {
                int forCount = 0;
                foreach (IMessage item in filteredMessages)
                {
                    if (item.Author == auth)
                    {
                        await (Context.Channel as ITextChannel).DeleteMessageAsync(item);
                        forCount += 1;
                        if (forCount == max)
                            break;
                    }
                }
            }
        }

        [Command("kick")]
        [RequireBotPermission(permission: GuildPermission.KickMembers)]
        [RequireUserPermission(permission: GuildPermission.KickMembers)]
        public async Task KickUser(SocketUser user, string reason = null)
        {
            if(user is null)
            {
                await ReplyAsync("Vyskytla se chyba při vyhledávání uživatele");
                return;
            }

            string reasonOut = reason is null ? "Důvod nebyl zadán" : reason;

            char colmn = '"';

            await (user as IGuildUser).KickAsync(reason);

            reasonOut = $"Uživatel {user.ToString()} byl kicknut uživatelem {Context.Message.Author.ToString()} z důvodu \"{reasonOut}\"";

            Console.WriteLine($"{Context.Message.Author.ToString()} has kicked {user.ToString()}");

            await ReplyAsync(reasonOut);

            InnerMethods.DirectoryChecker(new string[] { Program.KicksDirectory }, EnumDirectoryChecker.CheckOrCreate);

            StreamWriter sw = new StreamWriter($@"{Program.KicksDirectory}\{DateTime.Now.Month}{DateTime.Now.Year}.txt", encoding: Encoding.UTF8, append: true);
            sw.WriteLine($"{DateTime.Now.ToString("G")}\n{Context.Message.Author.ToString()} has kicked {user.ToString()}\n---------------------------------------------------------------------");
            sw.Close();
        }

        [Command("ban")]
        [RequireBotPermission(permission: GuildPermission.BanMembers)]
        [RequireUserPermission(permission: GuildPermission.BanMembers)]
        public async Task BanUser(SocketUser user, int length = 1, string reason = null)
        {
            if (user is null)
            {
                await ReplyAsync("Vyskytla se chyba při předávání uživatele");
                return;
            }

            if (length > 7)
                length = 7;
            else if (length < 0)
                length = 1;

            string reasonOutput = reason is null ? "Důvod nebyl zadán" : reason,
                   days = length == 1 ? "den" : (length == 2 || length == 3 || length == 4) ? "dny" : "dní",
                   banLen = (length == 0) ? "permanentně zabanován" : $"zabanován na {length} {days}";

            reasonOutput = $"Uživatel {user.ToString()} byl {banLen} uživatelem {Context.Message.Author.ToString()} z důvodu \"{reasonOutput}\"";

            await (user as IGuildUser).BanAsync(length, reason);

            Console.WriteLine($"{Context.Message.Author.ToString()} has banned {user.ToString()}");

            await ReplyAsync($"{reasonOutput}");

            InnerMethods.DirectoryChecker(new string[] { Program.BansDirectory }, EnumDirectoryChecker.CheckOrCreate);

            StreamWriter sw = new StreamWriter($@"{Program.BansDirectory}\{DateTime.Now.Month}{DateTime.Now.Year}.txt", encoding: Encoding.UTF8, append: true);
            sw.WriteLine($"{DateTime.Now.ToString("G")}\n{Context.Message.Author.ToString()} has banned {user.ToString()}\n" +
                $"Length: {(length == 0 ? "Permanent ban" : $"{length} day ban")}\n---------------------------------------------------------------------");
            sw.Close();

        }

        [Command("ban")]
        [RequireBotPermission(permission: GuildPermission.BanMembers)]
        [RequireUserPermission(permission: GuildPermission.BanMembers)]
        public async Task BanUser(SocketUser user, string reason = null, int length = 1) => await BanUser(user, length, reason);

        [Command("filter")]
        [Alias("Cenzura")]
        [Summary("Projde všechny kanály na DC serveru a smaže z nich zprávy od označeného uživatele")]
        [RequireUserPermission(ChannelPermission.ManageMessages)]
        [RequireBotPermission(ChannelPermission.ManageMessages)]
        public async Task FilterUser(SocketUser user, int ban = 0, int len = 1)
        {
            Debug.WriteLine($"user == null ? {user == null}");
            if (user == null)
            {
                await ReplyAsync("Neplatný uživatel");
                return;
            }

            if (len < 0 || len > 7)
                len = 1;

            bool banned = false;
            var frstMsg = await ReplyAsync($"{Context.Message.Author.Mention} Filter in progress..");

            Context.Guild.TextChannels.ToList().ForEach(async x => 
            {
                Debug.WriteLine($"Channel: {x.Name}");

                IEnumerable<IMessage> messages = await x.GetMessagesAsync(100).FlattenAsync();

                IEnumerable<IMessage> filteredMessages = messages.Where(y => (DateTimeOffset.UtcNow - y.Timestamp).TotalDays <= 14 && y.Author == user);

                int filterCount = filteredMessages.Count();

                Debug.WriteLine($"Amount of messages: {filterCount}");

                if(filterCount != 0)
                {
                    if (ban == 1 && banned == false && (Context.Message.Author as IGuildUser).GuildPermissions.BanMembers)
                    {
                        await (user as IGuildUser).BanAsync(len, "Filter ban", RequestOptions.Default);
                        banned = true;
                    }

                    await (Context.Channel as ITextChannel).DeleteMessagesAsync(filteredMessages);
                }
            });

            /*IEnumerable<ISocketMessageChannel> channels = Context.Guild.TextChannels;

            bool banned = false;

            var frstMsg = await ReplyAsync($"{Context.Message.Author.Mention} Filter in progress..");

            foreach(ISocketMessageChannel channel in channels)
            {
                Debug.WriteLine($"Channel: {channel.Name}");
                IEnumerable<IMessage> messages = await channel.GetMessagesAsync(50).FlattenAsync();

                IEnumerable<IMessage> filteredMessages = messages.Where(x => (DateTimeOffset.UtcNow - x.Timestamp).TotalDays <= 14 && x.Author == user);

                Debug.WriteLine($"Amount of messages: {filteredMessages.Count()}");

                if (filteredMessages.Count() != 0)
                {
                    if(ban == 1 && banned == false && (Context.Message.Author as IGuildUser).GuildPermissions.BanMembers)
                    {
                        await (user as IGuildUser).BanAsync(len, "Filter ban", RequestOptions.Default);
                        banned = true;
                    }

                    await (Context.Channel as ITextChannel).DeleteMessagesAsync(filteredMessages);
                }
            }*/

            var scndMsg = await ReplyAsync("Filter has finished!");

            await frstMsg.DeleteAsync();

            await Task.Delay(2000);
            await scndMsg.DeleteAsync();
        }

        [Command("filtermessage")]
        [Alias("Cenzura")]
        [Summary("Projde všechny kanály na DC serveru a smaže z nich zprávy, které jsou stejné")]
        [RequireUserPermission(ChannelPermission.ManageMessages)]
        [RequireBotPermission(ChannelPermission.ManageMessages)]
        public async Task FilterMessage(string message, int ban = 0, int len = 1)
        {
            if (len < 0 || len > 7)
                len = 7;

            if (ban != 0 || ban != 1)
                ban = 0;

            if (message.Length == 0)
            {
                await ReplyAsync("Neplatná zpráva!");
                return;
            }

            List<IUser> bannedUsers = new List<IUser>();

            Context.Guild.TextChannels.ToList().ForEach(async x => 
            {
                Debug.WriteLine($"Channel: {x.Name}");

                IEnumerable<IMessage> messages = await x.GetMessagesAsync(100).FlattenAsync();
                IEnumerable<IMessage> filteredMessages = messages.Where(y => (DateTimeOffset.Now - y.Timestamp).TotalDays <= 14 
                    && y.Content == message 
                    && !(y.Author as SocketGuildUser).GuildPermissions.Administrator);

                if (ban == 1 && (Context.Message.Author as SocketGuildUser).GuildPermissions.BanMembers)
                {
                    foreach (IMessage item in filteredMessages)
                    {
                        if (!bannedUsers.Any(x => x == item))
                        {
                            await (item.Author as SocketGuildUser).BanAsync(len, $"Message filter ban by {Context.Message.Author.ToString()}", RequestOptions.Default);
                            bannedUsers.Add(item.Author);
                        }

                        await item.DeleteAsync();
                    }
                }
                else
                    await (Context.Channel as ITextChannel).DeleteMessagesAsync(filteredMessages);
            });

            /*IEnumerable<ISocketMessageChannel> channels = Context.Guild.TextChannels;

            List<IUser> bannedUsers = new List<IUser>();

            foreach (ISocketMessageChannel channel in channels)
            {
                IEnumerable<IMessage> messages = await channel.GetMessagesAsync(100).FlattenAsync();

                IEnumerable<IMessage> filteredMessages = messages.Where(x => (DateTimeOffset.UtcNow - x.Timestamp).TotalDays <= 14 && x.Content.ToLower() == message.ToLower());

                if (ban == 1 && (Context.Message.Author as SocketGuildUser).GuildPermissions.BanMembers)
                {
                    foreach (IMessage item in filteredMessages)
                    {
                        if (bannedUsers.Where(x => x == item.Author).Count() == 0 && item.Author != Context.Message.Author && !(Context.Message.Author as SocketGuildUser).GuildPermissions.Administrator)
                        {
                            await (item.Author as SocketGuildUser).BanAsync(len, $"Message filter ban by {Context.Message.Author.ToString()}", RequestOptions.Default);
                            bannedUsers.Add(item.Author);
                        }

                        await item.DeleteAsync();
                    }
                }
                else
                    await (Context.Channel as ITextChannel).DeleteMessagesAsync(filteredMessages);
            }*/
        }

        #region TFTCommands

        [Command("tftassignroles")]
        [RequireBotPermission(GuildPermission.Administrator)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task TFTAssignRoles()
        {
            string scriptLocation = $@"{Environment.CurrentDirectory}\TFTDataLoadThingie\LoadData.py",
                   playerLocation = $@"{Environment.CurrentDirectory}\TFTDataLoadThingie\tftlobbies.txt",
                   lobbyRolesIDSLocation = $@"{Environment.CurrentDirectory}\TFTDataLoadThingie\lobbyRoles.json";


            Process proc = new Process();
            proc.StartInfo = new ProcessStartInfo($"{scriptLocation}")
            {
                UseShellExecute = true
            };

            proc.Start();
            proc.WaitForExit();

            string[] data = File.ReadAllLines(playerLocation);
            ulong[] tftLobbies = JsonSerializer.Deserialize<ulong[]>(File.ReadAllText(lobbyRolesIDSLocation));
            Dictionary<string, ulong> channels = new Dictionary<string, ulong>();

            if (data[0].ToLower().Contains("error"))
            {
                await ReplyAsync("An error occured in Python script!");
                return;
            }

            Dictionary<SocketRole, string> roles = new Dictionary<SocketRole, string>();


            foreach(string lines in data)
            {
                string[] splitted = lines.Split(';');
                if(splitted.Length != 3)
                {
                    await ReplyAsync($"Following object is missing required assets: {lines}. Length is {splitted.Length}");
                    return;
                }
                ulong playerID = 0;

                if (ulong.TryParse(splitted[2], out playerID))
                {
                    int roleIndex = int.Parse(splitted[0].ToLower().Split("lobby")[1]) - 1;
                    ulong roleId = tftLobbies[roleIndex];
                    SocketRole role = Context.Guild.GetRole(roleId);

                    if (!roles.Any(x => x.Key == role))
                    {
                        roles.Add(role, role.Name);
                        Console.WriteLine($"\nPlayer Role = {splitted[0]} with id: {roleId}\nAdding role: {role.Name} with Id: {role.Id}");
                    }

                    if (role == null)
                    {
                        await ReplyAsync($"No role with ID {roleId} exist");
                        return;
                    }

                    SocketGuildUser user = Context.Guild.GetUser(playerID);

                    string strDiscriminatorValue = user.Discriminator.ToString().Length == 3 ? $"0{user.Discriminator.ToString()}" :
                        user.Discriminator.ToString().Length == 2 ? $"00{user.Discriminator.ToString()}" :
                        user.Discriminator.ToString().Length == 1 ? $"00{user.Discriminator.ToString()}" :
                        user.Discriminator.ToString();

                    await (user as IGuildUser).AddRoleAsync(roleId);
                    Console.WriteLine($"Adding {user.Username}#{strDiscriminatorValue} to {Context.Guild.GetRole(roleId).Name}");
                }
                else
                {
                    if (!splitted[2].Contains('#'))
                        splitted[2] += "#0000";

                    List<SocketGuildUser> users = Context.Guild.Users.Where(x => x.Username[0] == splitted[2][0]).Where(y => y.Username[1] == splitted[2][1]).ToList();

                    foreach (SocketUser user in users)
                    {
                        try
                        {
                            string strDiscriminatorValue = user.Discriminator.ToString().Length == 3 ? $"0{user.Discriminator.ToString()}" :
                                user.Discriminator.ToString().Length == 2 ? $"00{user.Discriminator.ToString()}" :
                                user.Discriminator.ToString().Length == 1 ? $"00{user.Discriminator.ToString()}" :
                                user.Discriminator.ToString();

                            string userComp = $"{user.Username}#{strDiscriminatorValue}".ToLower();

                            if ($"{user.Username}#{strDiscriminatorValue}".ToLower() == splitted[2])
                            {
                                int roleIndex = int.Parse(splitted[0].ToLower().Split("lobby")[1]) - 1;
                                ulong roleId = tftLobbies[roleIndex];
                                SocketRole role = Context.Guild.GetRole(roleId);

                                if (!roles.Any(x => x.Key == role))
                                {
                                    roles.Add(role, role.Name);
                                    Console.WriteLine($"\nPlayer Role = {splitted[0]} with id: {roleId}\nAdding role: {role.Name} with Id: {role.Id}");
                                }

                                if (role == null)
                                {
                                    await ReplyAsync($"No role with ID {roleId} exist");
                                    return;
                                }

                                await (user as IGuildUser).AddRoleAsync(roleId);
                                Console.WriteLine($"Adding {user.Username}#{strDiscriminatorValue} to {Context.Guild.GetRole(roleId).Name}");
                                break;
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.Message);
                        }
                    }
                }
                Console.WriteLine($"Searching for {splitted[1]}{splitted[2]}");

                Context.Guild.Channels.ToList().ForEach(x =>
                {
                    if(x.Name.ToLower().StartsWith("lobby") && roles.Any(y => y.Key.Name.ToLower() == x.Name.ToLower()) && !channels.Any(y => y.Key.ToLower() == x.Name.ToLower()))
                    {
                        Console.WriteLine($"Adding lobby: {x.Name} with id: {x.Id}");
                        channels.Add(x.Name, x.Id);
                    }
                });
            }

            Console.WriteLine(roles.Count);
            foreach (SocketRole role in roles.Keys)
            {
                SocketChannel channel = Context.Guild.GetChannel(channels[role.Name]);
                await (channel as ITextChannel).SendMessageAsync("Zde máte návod jak se pozvat do lobby přímo z roomky na dc: https://www.youtube.com/watch?v=TXzximlQp1c");
            }

            await ReplyAsync("Done!");
        }

        [Command("tftunassignroles")]
        [RequireBotPermission(GuildPermission.Administrator)]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task TFTUnAssignRoles()
        {
            string lobbyRolesIDSLocation = $@"{Environment.CurrentDirectory}\TFTDataLoadThingie\lobbyRoles.json";

            ulong[] tftLobbies = JsonSerializer.Deserialize<ulong[]>(File.ReadAllText(lobbyRolesIDSLocation));

            foreach (ulong roleID in tftLobbies)
            {
                var role = Context.Guild.GetRole(roleID);

                if(role == null)
                {
                    await ReplyAsync($"No role with ID {roleID} exists");
                }

                foreach(SocketUser user in role.Members)
                {
                    await Console.Out.WriteLineAsync($"Removing {user.Username} from {role.Name}");
                    await (user as IGuildUser).RemoveRoleAsync(roleID);
                    await Console.Out.WriteLineAsync("Removed!");
                }
            }
            await ReplyAsync("Done!");
        }

        [Command("sendmessage")]
        [RequireBotPermission(GuildPermission.Administrator)]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task SendMessage(string message, int first, int last = 0)
        {
            if (last == 0)
                last = first;

            if(first > last)
            {
                int pom = last;
                last = first;
                first = pom;
            }

            ulong[] lobbies = JsonSerializer.Deserialize<ulong[]>(File.ReadAllText($@"{Environment.CurrentDirectory}\TFTDataLoadThingie\lobbyRoles.json"));

            Dictionary<SocketRole, string> roles = new Dictionary<SocketRole, string>();
            lobbies.ToList().ForEach(x =>
            {
                SocketRole role = Context.Guild.GetRole(x);
                roles.Add(role, role.Name);
            });

            Dictionary<string, ulong> channels = new Dictionary<string, ulong>();

            Context.Guild.Channels.ToList().ForEach(x =>
            {
                if (x.Name.ToLower().StartsWith("lobby") && roles.Any(y => y.Key.Name.ToLower() == x.Name.ToLower()) && !channels.Any(y => y.Key.ToLower() == x.Name.ToLower()))
                {
                    //Console.WriteLine($"Adding lobby: {x.Name} with id: {x.Id}");
                    channels.Add(x.Name, x.Id);
                }
            });

            for (int i = first -1; i < last; i++)
            {
                SocketGuildChannel channel = Context.Guild.GetChannel(channels.First(x => x.Key == roles.First(y => y.Key.Id == lobbies[i]).Key.Name).Value);
                await (channel as ITextChannel).SendMessageAsync(message);
            }

            await ReplyAsync("Sending done!");
        }

        [Command("cleanlobbies")]
        [RequireBotPermission(GuildPermission.Administrator)]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task CleanLobbies(int first, int last, int count = 100)
        {
            if (last == 0)
                last = first;

            if (first > last)
            {
                int pom = last;
                last = first;
                first = pom;
            }

            ulong[] lobbies = JsonSerializer.Deserialize<ulong[]>(File.ReadAllText($@"{Environment.CurrentDirectory}\TFTDataLoadThingie\lobbyRoles.json"));

            Dictionary<SocketRole, string> roles = new Dictionary<SocketRole, string>();
            lobbies.ToList().ForEach(x =>
            {
                SocketRole role = Context.Guild.GetRole(x);
                roles.Add(role, role.Name);
            });

            Dictionary<string, ulong> channels = new Dictionary<string, ulong>();

            Context.Guild.Channels.ToList().ForEach(x =>
            {
                if (x.Name.ToLower().StartsWith("lobby") && roles.Any(y => y.Key.Name.ToLower() == x.Name.ToLower()) && !channels.Any(y => y.Key.ToLower() == x.Name.ToLower()))
                {
                    //Console.WriteLine($"Adding lobby: {x.Name} with id: {x.Id}");
                    channels.Add(x.Name, x.Id);
                }
            });

            for (int i = first - 1; i < last; i++)
            {

                SocketGuildChannel channel = Context.Guild.GetChannel(channels.First(x => x.Key == roles.First(y => y.Key.Id == lobbies[i]).Key.Name).Value);

                await Console.Out.WriteLineAsync($"Cleaning in: {channel.Name}");

                IEnumerable<IMessage> messages = await (channel as ISocketMessageChannel).GetMessagesAsync(count).FlattenAsync();

                IEnumerable<IMessage> filteredMessages = messages.Where(x => (DateTimeOffset.UtcNow - x.Timestamp).TotalDays <= 14);

                await (channel as ITextChannel).DeleteMessagesAsync(filteredMessages);
                await Console.Out.WriteLineAsync($"Cleaned channel: {channel.Name}");
            }

            await ReplyAsync("Cleaning done!");
        }
        #endregion
    }
}