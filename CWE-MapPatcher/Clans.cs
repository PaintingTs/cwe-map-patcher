using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CWE_MapPatcher
{
    class Clans
    {
        public const int NonSet = -1;
        public const int Neutral = 0;
        public const int Clan_1 = 1;
        public const int Clan_2 = 2;

        private int[] _clanIds = new int[9];

        private Clans() { }

        public int this[int index]
        {
            get { return _clanIds[index]; }
            set { _clanIds[index] = value; }
        }

        public static Clans CreateDefaultPlayersClans()
        {
            var res = new Clans();
            res[0] = Neutral;
            for (int i = 1; i < 9; i++)
                res[i] = NonSet;

            return res;
        }

        public void SaveClanScript(string mapDirectory)
        {
            string content = "CLAN_OF_PLAYER = {}\n";

            for (int i = 1; i < 9; i++)
                content += string.Format("CLAN_OF_PLAYER[{0}] = {1}\n", i, _clanIds[i]);

            File.WriteAllLines(Path.Combine(mapDirectory, "clans.lua"), content.Split('\n'));
        }

        public static bool AlreadyPatched(string mapDirectory)
        {
            return File.Exists(Path.Combine(Path.Combine(mapDirectory, "scripts"), "clans.lua"));
        }
    }
}
