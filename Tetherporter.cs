using Eleon;
using Eleon.Modding;
using EmpyrionModdingFramework;
using System;
using System.Configuration;
using System.IO;
using System.Threading.Tasks;

namespace Tetherporter
{
    public class Tetherporter : EmpyrionModdingFrameworkBase
    {
        private IDatabaseManager _dbManager;
        protected override void Initialize()
        {
            ModName = "Tetherporter";

            var chatPrefix = "!";
            CommandManager.CommandPrexix = chatPrefix;

            var dbPath = GetDatabasePath();
            if (string.IsNullOrEmpty(dbPath))
                return;

            _dbManager = new CsvManager(dbPath, ModAPI.Log);

            CommandManager.CommandList.Add(new ChatCommand($"tetherport", (I) => Tetherport(I)));
            CommandManager.CommandList.Add(new ChatCommand($"untether", (I) => Untether(I)));
        }

        private async Task Tetherport(MessageData messageData)
        {
            PlayerInfo player = (PlayerInfo)await RequestManager.SendGameRequest(
                CmdId.Request_Player_Info, new Id() { id = messageData.SenderEntityId }
                );

            if(!_dbManager.LoadPortal(out var portal))
            {
                Log($"Entity {player.entityId}/{player.playerName} requested Tetherport but no portal was found.");
                await MessagePlayer(player.entityId, $"No portal is currently active. Cannot Tetherport.", 10, MessagerPriority.Red);
                return;
            }

            var record = new TetherporterRecord(player.steamId, player.entityId, player.playfield, player.pos, player.rot);

            _dbManager.SaveTetherportRecord(record);

            await TeleportPlayer(player.entityId, portal.Playfield, portal.PosX, portal.PosY, portal.PosZ, portal.RotX, portal.RotY, portal.RotZ);
            Log($"Tetherported entity: {player.entityId}/{player.playerName} to portal.");

            await MessagePlayer(player.entityId, $"Created teleport tether! Welcome to {portal.EventName}!", 10);
        }

        private async Task Untether(MessageData messageData)
        {
            PlayerInfo player = (PlayerInfo)await RequestManager.SendGameRequest(
                CmdId.Request_Player_Info, new Id() { id = messageData.SenderEntityId }
                );

            if (!_dbManager.LoadPortal(out var portal))
            {
                Log($"Entity {player.entityId}/{player.playerName} requested Untether but no portal was found.");
                await MessagePlayer(player.entityId, $"No event portal is active. Cannot Untether.", 10, MessagerPriority.Red);
                return;
            }

            if (!_dbManager.LoadTetherporterRecord(player.steamId, out var record))
            {
                Log($"Entity {player.entityId}/{player.playerName} requester Untether but no tether was found.");
                await MessagePlayer(player.entityId, $"No tether was found.", 5, MessagerPriority.Red);
                return;
            }

            await TeleportPlayer(player.entityId, record.Playfield, record.PosX, record.PosY, record.PosZ, record.RotX, record.RotY, record.RotZ);
        }


        private string GetDatabasePath()
        {
            /*
             * Different hosting services run the Empyrion server executable from different locations
             * In order to get around this, Tetherporter looks for a file: tetherporter.dbpath
             * which should contain the path from the run directory to the Tetherport mod directory
             * and be placed in the folder that the server executable is being ran from
             * Examples I have come across:
             * Running from: "/Empyrion - Dedicated Server/", File should contain: "Content/Mods/Tetherporter/Database"
             * Running from: "/Empyrion - Dedicated Server/DedicatedServer", File should contain "../Content/Mods/Tetherporter/Database"
            */

            Log("TeleportTether is running from directory: " + Directory.GetCurrentDirectory());
            var dbPathFileName = @"tetherporter.dbpath";
            var dbPathFile = Path.Combine(Directory.GetCurrentDirectory(), dbPathFileName);

            if (!File.Exists(dbPathFile)) //Check for tetherporter.dbpath
            {
                Log($"Tetherporter cannot locate it's database path. Please place a {dbPathFileName} file in the folder you are running the server from: {Directory.GetCurrentDirectory()}");
                return null;
            }

            var dbPath = Path.GetFullPath(File.ReadAllText(dbPathFile).Trim()); //Trim spaces and parse relative paths

            if (!Directory.Exists(dbPath)) //Check for Mods/Tetherporter/Database directory
            {
                Log($"Tetherporter cannot location it's database path, the full location it checked was: {dbPath}");
                return null;
            }

            Log("Tetherporter will use the database path: " + dbPath);
            return dbPath;
        }
    }
}
