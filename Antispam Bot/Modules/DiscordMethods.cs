using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Antispam_Bot.Modules
{
    internal class DiscordMethods
    {
        public static ulong GetDiscordID(string entry)
        {
            if (IsDiscordID(entry))
            {
                try
                {
                    return ulong.Parse(entry.TrimStart("<@".ToCharArray()).TrimEnd('>'));
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            return 0ul;
        }

        public static bool IsNumeric(string entry)
        {
            for (int i = 0; i < entry.Length; i++)
                if (!int.TryParse(entry[i].ToString(), out _))
                    return false;
            return true;
        }

        public static bool IsDiscordID(string strID) => strID.StartsWith("<@") && strID.EndsWith(">") && IsNumeric(strID.TrimStart("<@".ToCharArray()).TrimEnd('>'));
    }
}
