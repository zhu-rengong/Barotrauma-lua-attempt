﻿extern alias BarotraumaClient;
extern alias BarotraumaServer;

namespace LuaDocsGenerator
{

    internal class Program
    {
        private static string basePath = "";
        private static string generatedDir = "";
        private static string baseLuaDir = "";

        private static void GenerateDocs<T>(string file, string? categoryName = null, string realm = "shared")
        {
            DocsGenerator.GenerateDocs(typeof(T), $"{baseLuaDir}/{file}", $"{generatedDir}/{file}", categoryName, realm);
        }

        private static void GenerateDocs<T1, T2>(string file, string? categoryName = null)
        {
            DocsGenerator.GenerateDocs(typeof(T1), typeof(T2), $"{baseLuaDir}/{file}", $"{generatedDir}/{file}", categoryName);
        }

        private static void GenerateDocs(Type clientType, Type serverType, string file, string? categoryName = null)
        {
            DocsGenerator.GenerateDocs(clientType, serverType, $"{baseLuaDir}/{file}", $"{generatedDir}/{file}", categoryName);
        }
        private static void GenerateEnum<T>(string file, string realm = "shared")
        {
            DocsGenerator.GenerateEnum(typeof(T), $"{generatedDir}/{file}", realm);
        }


        static void Main(string[] args)
        {
            var gitDir = (new Func<String>(() =>
            {
                var (success, gitDir, error) = DocsGenerator.TryRunGitCommand("rev-parse --show-toplevel");
                if (!success)
                {
                    throw new InvalidDataException($"Failed to determine the root of the git repo: {error}");
                }

                return gitDir;
            }))();

            basePath = $"{gitDir}/luacs-docs/lua";
            generatedDir = $"{basePath}/lua/generated";
            baseLuaDir = $"{basePath}/baseluadocs";

            try
            {
                Directory.Delete(generatedDir, true);
            }
            catch (DirectoryNotFoundException) { }

            Directory.CreateDirectory(generatedDir);
            Directory.CreateDirectory(baseLuaDir);

            GenerateDocs<BarotraumaClient::Barotrauma.Character, BarotraumaServer::Barotrauma.Character>("Character.lua");
            GenerateDocs<BarotraumaClient::Barotrauma.CharacterInfo, BarotraumaServer::Barotrauma.CharacterInfo>("CharacterInfo.lua");
            GenerateDocs<BarotraumaClient::Barotrauma.CharacterHealth, BarotraumaServer::Barotrauma.CharacterHealth>("CharacterHealth.lua");
            GenerateDocs<BarotraumaClient::Barotrauma.AnimController, BarotraumaServer::Barotrauma.AnimController>("AnimController.lua");
            GenerateDocs<BarotraumaClient::Barotrauma.Networking.Client, BarotraumaServer::Barotrauma.Networking.Client>("Client.lua");
            GenerateDocs<BarotraumaClient::Barotrauma.Entity, BarotraumaServer::Barotrauma.Entity>("Entity.lua");
            GenerateDocs<BarotraumaClient::Barotrauma.EntitySpawner, BarotraumaServer::Barotrauma.EntitySpawner>("Entity.Spawner.lua", "Entity.Spawner");
            GenerateDocs<BarotraumaClient::Barotrauma.Item, BarotraumaServer::Barotrauma.Item>("Item.lua");
            GenerateDocs<BarotraumaClient::Barotrauma.ItemPrefab, BarotraumaServer::Barotrauma.ItemPrefab>("ItemPrefab.lua");
            GenerateDocs<BarotraumaClient::Barotrauma.Submarine, BarotraumaServer::Barotrauma.Submarine>("Submarine.lua");
            GenerateDocs<BarotraumaClient::Barotrauma.SubmarineInfo, BarotraumaServer::Barotrauma.SubmarineInfo>("SubmarineInfo.lua");
            GenerateDocs<BarotraumaClient::Barotrauma.Job, BarotraumaServer::Barotrauma.Job>("Job.lua");
            GenerateDocs<BarotraumaClient::Barotrauma.JobPrefab, BarotraumaServer::Barotrauma.JobPrefab>("JobPrefab.lua");
            GenerateDocs<BarotraumaClient::Barotrauma.GameSession, BarotraumaServer::Barotrauma.GameSession>("GameSession.lua");
            GenerateDocs<BarotraumaClient::Barotrauma.NetLobbyScreen, BarotraumaServer::Barotrauma.NetLobbyScreen>("NetLobbyScreen.lua");
            GenerateDocs<BarotraumaClient::Barotrauma.GameScreen, BarotraumaServer::Barotrauma.GameScreen>("GameScreen.lua");
            GenerateDocs<FarseerPhysics.Dynamics.World>("World.lua", "Game.World");
            GenerateDocs<BarotraumaClient::Barotrauma.Inventory, BarotraumaServer::Barotrauma.Inventory>("Inventory.lua");
            GenerateDocs<BarotraumaClient::Barotrauma.ItemInventory, BarotraumaServer::Barotrauma.ItemInventory>("ItemInventory.lua");
            GenerateDocs<BarotraumaClient::Barotrauma.CharacterInventory, BarotraumaServer::Barotrauma.CharacterInventory>("CharacterInventory.lua");
            GenerateDocs<BarotraumaClient::Barotrauma.Hull, BarotraumaServer::Barotrauma.Hull>("Hull.lua");
            GenerateDocs<BarotraumaClient::Barotrauma.Level, BarotraumaServer::Barotrauma.Level>("Level.lua");
            GenerateDocs<BarotraumaClient::Barotrauma.Affliction, BarotraumaServer::Barotrauma.Affliction>("Affliction.lua");
            GenerateDocs<BarotraumaClient::Barotrauma.AfflictionPrefab, BarotraumaServer::Barotrauma.AfflictionPrefab>("AfflictionPrefab.lua");
            GenerateDocs<BarotraumaClient::Barotrauma.WayPoint, BarotraumaServer::Barotrauma.WayPoint>("WayPoint.lua");
            GenerateDocs<BarotraumaClient::Barotrauma.Networking.ServerSettings, BarotraumaServer::Barotrauma.Networking.ServerSettings>("ServerSettings.lua", "Game.ServerSettings");
            GenerateDocs(typeof(BarotraumaClient::Barotrauma.GameSettings), typeof(BarotraumaServer::Barotrauma.GameSettings), "GameSettings.lua", "Game.Settings");
            GenerateDocs<BarotraumaClient::Barotrauma.Structure, BarotraumaServer::Barotrauma.Structure>("Structure.lua");
            GenerateDocs<BarotraumaClient::Barotrauma.StructurePrefab, BarotraumaServer::Barotrauma.StructurePrefab>("StructurePrefab.lua");
            GenerateDocs<BarotraumaClient::Barotrauma.PhysicsBody, BarotraumaServer::Barotrauma.PhysicsBody>("PhysicsBody.lua");
            GenerateDocs<BarotraumaClient::Barotrauma.Limb, BarotraumaServer::Barotrauma.Limb>("Limb.lua");
            GenerateDocs<BarotraumaClient::Barotrauma.PlayerInput>("PlayerInput.lua", "PlayerInput", "client");

            GenerateEnum<BarotraumaClient.Barotrauma.CauseOfDeathType>("CauseOfDeathType.lua");
            GenerateEnum<BarotraumaClient.Barotrauma.CharacterTeamType>("CharacterTeamType.lua");
            GenerateEnum<BarotraumaClient.Barotrauma.Networking.ClientPermissions>("ClientPermissions.lua");
            GenerateEnum<BarotraumaClient.Barotrauma.Networking.ChatMessageType>("ChatMessageType.lua");
            GenerateEnum<BarotraumaClient.Barotrauma.InvSlotType>("InvSlotType.lua");
            GenerateEnum<BarotraumaClient.Barotrauma.LimbType>("LimbType.lua");
            GenerateEnum<BarotraumaClient.Barotrauma.Rand.RandSync>("RandSync.lua");
            GenerateEnum<BarotraumaClient.Barotrauma.Networking.ServerLog.MessageType>("ServerLogMessageType.lua");
            GenerateEnum<BarotraumaClient.Barotrauma.SpawnType>("SpawnType.lua");
            GenerateEnum<BarotraumaClient.Barotrauma.Networking.DisconnectReason>("DisconnectReason.lua");

        }
    }
}
