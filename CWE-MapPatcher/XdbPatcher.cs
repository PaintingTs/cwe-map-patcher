using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace CWE_MapPatcher
{
    class XdbPatcher
    {
        private static readonly Dictionary<string, int> _playerIds = new Dictionary<string,int>()
        {
            { "PLAYER_NONE", 0 }, { "PLAYER_1", 1 }, { "PLAYER_2", 2 }, { "PLAYER_3", 3 }, { "PLAYER_4", 4 },
            { "PLAYER_5", 5 }, { "PLAYER_6", 6 }, { "PLAYER_7", 7 }, { "PLAYER_8", 8 },
        };

        public void Patch(string fileName, Clans playersClans)
        {
            var doc = new XmlDocument();
            doc.Load(fileName);

            XmlNode playersNode = doc.SelectSingleNode("/AdvMapDesc/players");
            var playersRaces = SmartClanGenerator(playersClans, playersNode);

            if (NeedRedKnights(playersClans, playersRaces))
            {
                XmlNode additionalHeroesNode = doc.SelectSingleNode("AdvMapDesc/AdditionallyRollableHeroes");
                additionalHeroesNode.InnerXml += Properties.Resources.RedKnightsList;

                HeavenClan2Patcher(playersClans, playersRaces, playersNode);
            }

            XmlNodeList townNodes = doc.SelectNodes("/AdvMapDesc/objects/Item/AdvMapTown");
            TownPatcher(playersClans, townNodes);

            XmlNode objectivesNode = doc.SelectSingleNode("/AdvMapDesc/Objectives/Secondary/PlayerSpecific"); //may do Secondary instead 
            ObjectivePatcher(playersClans, objectivesNode);

            XmlNode nameRefNode = doc.SelectSingleNode("AdvMapDesc/NameFileRef");
            MapNamePatcher(nameRefNode, fileName);

            EnableMapScripts(doc);

            doc.Save(fileName);
        }

        public void OnlyEnableMapScripts(string fileName)
        {
            var doc = new XmlDocument();
            doc.Load(fileName);

            EnableMapScripts(doc);

            doc.Save(fileName);
        }


        private string[] SmartClanGenerator(Clans playersClans, XmlNode playersNode)
        {
            var races = new string[9];
            races[0] = string.Empty;

            var rng = new Random();
            var raceFlags = new Dictionary<string, int>();

            playersClans[0] = Clans.Neutral;

            int playerId = 1;
            foreach (XmlNode playerNode in playersNode.ChildNodes)
            {
                string key = playerNode.SelectSingleNode("Race").InnerText;
                races[playerId] = key;

                if (playersClans[playerId] == Clans.NonSet) // use rng if not set
                {
                    if (raceFlags.ContainsKey(key))
                        raceFlags[key] = OtherClan(raceFlags[key]);
                    else
                    {
                        int clan = rng.Next(Clans.Clan_1, Clans.Clan_2 + 1);

                        raceFlags.Add(key, clan);
                    }

                    playersClans[playerId] = raceFlags[key];
                }

                Console.WriteLine("PLAYER_{0} -=- Race: {1} -=- Clan: {2}", playerId, key, playersClans[playerId]);

                playerId++;
            }

            return races;
        }


        private void TownPatcher(Clans playersClans, XmlNodeList townNodes)
        {
            var rng = new Random();

            foreach (XmlNode townNode in townNodes)
            {
                string playerStrId = townNode.SelectSingleNode("PlayerID").InnerText;

                var sharedHrefAttribute = townNode.SelectSingleNode("Shared").Attributes["href"];

                // v2.0 Orcs don't have 2 upgrades from the start
                /*if (sharedHrefAttribute.Value.Contains("Stronghold"))
                {
                    OrcTownPatcher(townNode, rng.Next(2));
                    continue;
                }*/

                int clan = playersClans[_playerIds[playerStrId]];

                if (clan == Clans.Neutral)
                    NeutralTownPatcher(townNode, sharedHrefAttribute, rng.Next(Clans.Clan_1, Clans.Clan_2 + 1));
                else if (clan == Clans.Clan_1)
                    ClanOnePatcher(townNode, sharedHrefAttribute);
                else if (clan == Clans.Clan_2)
                    ClanTwoPatcher(townNode, sharedHrefAttribute);
            } 
        }

        private void ObjectivePatcher(Clans playersClans, XmlNode objectivesNode)
        {
            int playerId = 1;
            foreach (XmlNode playerItemNode in objectivesNode.ChildNodes)
            {
                XmlNode playerObjectiveNode = playerItemNode.SelectSingleNode("Objectives");

                if (playersClans[playerId] == Clans.Clan_1)
                    playerObjectiveNode.InnerXml += Properties.Resources.ClanOneObjective;

                if (playersClans[playerId] == Clans.Clan_2)
                    playerObjectiveNode.InnerXml += Properties.Resources.ClanTwoObjective;

                playerObjectiveNode.InnerXml += Properties.Resources.TownRebuildObjective;

                playerId++;
            }
        }

        private void MapNamePatcher(XmlNode nameRefNode, string fileName)
        {
            string nameHref = nameRefNode.Attributes["href"].Value;
            string mapNamePath = System.IO.Path.Combine(new System.IO.FileInfo(fileName).DirectoryName, nameHref);
            string mapName = System.IO.File.ReadAllText(mapNamePath);
            System.IO.File.WriteAllText(mapNamePath, "<color=CCB000>CWE " + mapName, Encoding.Unicode);
        }


        private void HeavenClan2Patcher(Clans playersClans, string[] playersRaces, XmlNode playersNode)
        {
            int playerId = 1;
            foreach (XmlNode playerNode in playersNode.ChildNodes)
            {
                var bannedHeroesNode = playerNode.SelectSingleNode("TavernFilter/BannedHeroes");

                if (NeedRedKnights(playersClans[playerId], playersRaces[playerId]))
                    bannedHeroesNode.InnerXml += Properties.Resources.BaseKnightsList;
                else
                    bannedHeroesNode.InnerXml += Properties.Resources.RedKnightsList;

                playerId++;
            }
        }


        //-----------------------------------------------------------------------------

        private void ClanTwoPatcher(XmlNode townNode, XmlAttribute sharedHrefAttribute)
        {
            string oldHref = sharedHrefAttribute.Value;

            // v2.0 Removing this logic because of 'Rebuild' ability
            /*if (NeedMagicSchoolPatch(oldHref))
            {
                string sharedEnding = ".(AdvMapTownShared).xdb#xpointer(/AdvMapTownShared)";

                string newHref = oldHref.Substring(0, oldHref.Length - sharedEnding.Length) + "_2" + sharedEnding;

                sharedHrefAttribute.Value = newHref;
            }*/

            var upgradesFilterNode = townNode.SelectSingleNode("CreaturesUpgradesFilter/ForbiddenBasicUpgradeTiers");

            bool isInferno = oldHref.Contains("Inferno");

            for (int i = (isInferno ? 2 : 1); i <= 7; i++)
            {
                var itemNode = townNode.OwnerDocument.CreateElement("Item");
                itemNode.InnerText = i.ToString();

                upgradesFilterNode.AppendChild(itemNode);
            }
        }

        private void ClanOnePatcher(XmlNode townNode, XmlAttribute sharedHrefAttribute)
        {
            var upgradesFilterNode = townNode.SelectSingleNode("CreaturesUpgradesFilter/ForbiddenAlterUpgradeTiers");

            bool isInferno = sharedHrefAttribute.Value.Contains("Inferno");

            for (int i = (isInferno ? 2 : 1); i <= 7; i++)
            {
                var itemNode = townNode.OwnerDocument.CreateElement("Item");
                itemNode.InnerText = i.ToString();

                upgradesFilterNode.AppendChild(itemNode);
            }
        }

        private void OrcTownPatcher(XmlNode townNode, int dice)
        {
            var upgradesFilterNode = townNode.SelectSingleNode(dice != 0 ?
                "CreaturesUpgradesFilter/ForbiddenAlterUpgradeTiers" :
                "CreaturesUpgradesFilter/ForbiddenBasicUpgradeTiers");

            var itemNode = townNode.OwnerDocument.CreateElement("Item");

            int wyvernIndex = 6;
            itemNode.InnerText = wyvernIndex.ToString();

            upgradesFilterNode.AppendChild(itemNode);
        }

        private void NeutralTownPatcher(XmlNode townNode, XmlAttribute sharedHref, int generatedClan)
        {
            if (generatedClan == Clans.Clan_1)
                ClanOnePatcher(townNode, sharedHref);
            else if (generatedClan == Clans.Clan_2)
                ClanTwoPatcher(townNode, sharedHref);
        }

        //-----------------------------------------------------------------------------

        private void EnableMapScripts(XmlDocument doc)
        {
            XmlNode mapScriptNode = doc.SelectSingleNode("/AdvMapDesc/MapScript");
            if (mapScriptNode.Attributes["href"] == null)
            {
                var mapScriptHrefAttribute = doc.CreateAttribute("href");
                mapScriptHrefAttribute.Value = "MapScript.xdb#xpointer(/Script)";

                mapScriptNode.Attributes.Append(mapScriptHrefAttribute);
            }
        }


        private static int OtherClan(int clan)
        {
            if (clan == Clans.Clan_1)
                return Clans.Clan_2;

            if (clan == Clans.Clan_2)
                return Clans.Clan_1;

            return clan;
        }

        private static bool NeedRedKnights(Clans playersClans, string[] playersRaces)
        {
            return Enumerable.Range(1, 8).Any(playerId => NeedRedKnights(playersClans[playerId], playersRaces[playerId]));
        }

        private static bool NeedRedKnights(int playersClanId, string playersRace)
        {
            return (playersRace == "TOWN_HEAVEN" || playersRace == "TOWN_NO_TYPE") // Generator does not set race properly :(
                && playersClanId == Clans.Clan_2;
        }

        private static bool NeedMagicSchoolPatch(string sharedHref)
        {
            return 
                sharedHref.Contains("Dungeon") || sharedHref.Contains("Fortress") ||
                sharedHref.Contains("Inferno") || sharedHref.Contains("Preserve") ||
                sharedHref.Contains("Academy");
        }
    }
}
