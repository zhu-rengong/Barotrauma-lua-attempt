using Steamworks.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Barotrauma.Steam
{
    static partial class SteamManager
    {
        public const int STEAMP2P_OWNER_PORT = 30000;

        public const uint AppID = 602960;

        private static readonly Dictionary<string, int> tagCommonness = new Dictionary<string, int>()
        {
            { "submarine", 10 },
            { "item", 10 },
            { "monster", 8 },
            { "art", 8 },
            { "mission", 8 },
            { "event set", 8 },
            { "total conversion", 5 },
            { "environment", 5 },
            { "item assembly", 5 },
            { "language", 5 }
        };

        public static bool IsInitialized { get; private set; }

        private static readonly List<string> popularTags = new List<string>();
        public static IEnumerable<string> PopularTags
        {
            get
            {
                if (!IsInitialized) { return Enumerable.Empty<string>(); }
                return popularTags;
            }
        }

        public static void Initialize()
        {
            InitializeProjectSpecific();
        }

        public static ulong GetSteamID()
        {
            if (!IsInitialized || !Steamworks.SteamClient.IsValid)
            {
                return 0;
            }

            return Steamworks.SteamClient.SteamId;
        }

        public static bool IsFamilyShared()
        {
            if (!IsInitialized || !Steamworks.SteamClient.IsValid) { return false; }

            return Steamworks.SteamApps.IsSubscribedFromFamilySharing;
        }

        public static bool IsFreeWeekend()
        {
            if (!IsInitialized || !Steamworks.SteamClient.IsValid) { return false; }

            return Steamworks.SteamApps.IsSubscribedFromFamilySharing;
        }

        public static string GetUsername()
        {
            if (!IsInitialized || !Steamworks.SteamClient.IsValid)
            {
                return "";
            }
            return Steamworks.SteamClient.Name;
        }

        public static uint GetNumSubscribedItems()
        {
            if (!IsInitialized || !Steamworks.SteamClient.IsValid)
            {
                return 0;
            }
            return Steamworks.SteamUGC.NumSubscribedItems;
        }

        public static PublishedFileId[] GetSubscribedItems()
        {
            if (!IsInitialized || !Steamworks.SteamClient.IsValid)
            {
                return new PublishedFileId[0];
            }
            return Steamworks.SteamUGC.GetSubscribedItems();
        }

        public static bool UnlockAchievement(string achievementIdentifier) =>
            UnlockAchievement(achievementIdentifier.ToIdentifier());

        public static bool UnlockAchievement(Identifier achievementIdentifier)
        {
            if (!IsInitialized || !Steamworks.SteamClient.IsValid)
            {
                return false;
            }

            DebugConsole.Log("Unlocked achievement \"" + achievementIdentifier + "\"");

            var achievements = Steamworks.SteamUserStats.Achievements.ToList();
            int achIndex = achievements.FindIndex(ach => ach.Identifier == achievementIdentifier);
            bool unlocked = achIndex >= 0 ? achievements[achIndex].Trigger() : false;
            if (!unlocked)
            {
                //can be caused by an incorrect identifier, but also happens during normal gameplay:
                //SteamAchievementManager tries to unlock achievements that may or may not exist 
                //(discovered[whateverbiomewasentered], kill[withwhateveritem], kill[somemonster] etc) so that we can add
                //some types of new achievements without the need for client-side changes.
                DebugConsole.Log($"Failed to unlock achievement \"{achievementIdentifier}\".");
            }

            return unlocked;
        }

        public static bool IncrementStat(Identifier statName, int increment)
        {
            if (!IsInitialized || !Steamworks.SteamClient.IsValid) { return false; }
            DebugConsole.Log($"Incremented stat \"{statName}\" by " + increment);
            bool success = Steamworks.SteamUserStats.AddStat(statName.Value.ToLowerInvariant(), increment);
            if (!success)
            {
                DebugConsole.Log("Failed to increment stat \"" + statName + "\".");
            }
            else
            {
                StoreStats();
            }
            return success;
        }

        public static bool IncrementStat(Identifier statName, float increment)
        {
            if (!IsInitialized || !Steamworks.SteamClient.IsValid) { return false; }
            DebugConsole.Log($"Incremented stat \"{statName}\" by " + increment);
            bool success = Steamworks.SteamUserStats.AddStat(statName.Value.ToLowerInvariant(), increment);
            if (!success)
            {
                DebugConsole.Log("Failed to increment stat \"" + statName + "\".");
            }
            else
            {
                StoreStats();
            }
            return success;
        }

        public static int GetStatInt(Identifier statName)
        {
            if (!IsInitialized || !Steamworks.SteamClient.IsValid) { return 0; }
            return  Steamworks.SteamUserStats.GetStatInt(statName.Value.ToLowerInvariant());
        }

        public static bool StoreStats()
        {
            if (!IsInitialized || !Steamworks.SteamClient.IsValid) { return false; }
            DebugConsole.Log("Storing Steam stats...");
            bool success = Steamworks.SteamUserStats.StoreStats();
            if (!success)
            {
                DebugConsole.Log("Failed to store Steam stats.");
            }
            return success;
        }

        public static bool TryGetUnlockedAchievements(out List<Steamworks.Data.Achievement> achievements)
        {
            if (!IsInitialized || !Steamworks.SteamClient.IsValid) 
            {
                achievements = null;
                return false; 
            }
            achievements = Steamworks.SteamUserStats.Achievements.Where(a => a.State).ToList();
            return true;
        }

        public static void Update(float deltaTime)
        {
            if (!IsInitialized) { return; }

            if (Steamworks.SteamClient.IsValid) { Steamworks.SteamClient.RunCallbacks(); }
            if (Steamworks.SteamServer.IsValid) { Steamworks.SteamServer.RunCallbacks(); }

            SteamAchievementManager.Update(deltaTime);
        }

        public static void ShutDown()
        {
            if (!IsInitialized) { return; }

            if (Steamworks.SteamClient.IsValid) { Steamworks.SteamClient.Shutdown(); }
            if (Steamworks.SteamServer.IsValid) { Steamworks.SteamServer.Shutdown(); }
            IsInitialized = false;
        }

        public static IEnumerable<ulong> ParseWorkshopIds(string workshopIdData)
        {
            string[] workshopIds = workshopIdData.Split(',');
            foreach (string id in workshopIds)
            {
                if (ulong.TryParse(id, out ulong idCast))
                {
                    yield return idCast;
                }
                else
                {
                    yield return 0;
                }
            }
        }

        public static IEnumerable<ulong> WorkshopUrlsToIds(IEnumerable<string> urls)
        {
            return urls.Select((u) =>
            {
                if (string.IsNullOrEmpty(u))
                {
                    return (ulong)0;
                }
                else
                {
                    return GetWorkshopItemIDFromUrl(u);
                }
            });
        }

        public static ulong GetWorkshopItemIDFromUrl(string url)
        {
            try
            {
                Uri uri = new Uri(url);
                string idStr = HttpUtility.ParseQueryString(uri.Query)["id".ToIdentifier()];
                if (ulong.TryParse(idStr, out ulong id))
                {
                    return id;
                }
            }
            catch (Exception e)
            {
                DebugConsole.ThrowError("Failed to get Workshop item ID from the url \"" + url + "\"!", e);
            }

            return 0;
        }

        public static UInt64 SteamIDStringToUInt64(string str)
        {
            if (string.IsNullOrWhiteSpace(str)) { return 0; }
            UInt64 retVal;
            if (str.StartsWith("STEAM64_", StringComparison.InvariantCultureIgnoreCase)) { str = str.Substring(8); }
            if (UInt64.TryParse(str, out retVal) && retVal > (1 << 52)) { return retVal; }
            if (!str.StartsWith("STEAM_", StringComparison.InvariantCultureIgnoreCase)) { return 0; }
            string[] split = str.Substring(6).Split(':');
            if (split.Length != 3) { return 0; }

            if (!UInt64.TryParse(split[0], out UInt64 universe)) { return 0; }
            if (!UInt64.TryParse(split[1], out UInt64 y)) { return 0; }
            if (!UInt64.TryParse(split[2], out UInt64 accountNumber)) { return 0; }

            UInt64 accountInstance = 1; UInt64 accountType = 1;

            return (universe << 56) | (accountType << 52) | (accountInstance << 32) | (accountNumber << 1) | y;
        }

        public static string SteamIDUInt64ToString(UInt64 uint64)
        {
            UInt64 y = uint64 & 0x1;
            UInt64 accountNumber = (uint64 >> 1) & 0x7fffffff;
            UInt64 universe = (uint64 >> 56) & 0xff;

            string retVal = "STEAM_" + universe.ToString() + ":" + y.ToString() + ":" + accountNumber.ToString();

            if (SteamIDStringToUInt64(retVal) != uint64) { return "STEAM64_" + uint64.ToString(); }

            return retVal;
        }
    }
}
