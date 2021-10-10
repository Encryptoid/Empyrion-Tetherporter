using Eleon;
using Eleon.Modding;
using EmpyrionModdingFramework;
using InventoryManagement;
using ModLocator;
using System.IO;
using System.Threading.Tasks;
using EmpyrionModdingFramework.Database;

namespace Tetherporter
{
    public class Tetherporter : EmpyrionModdingFrameworkBase
    {
        private IDatabaseManager _dbManager;
        private const string PortalFileName = "portal.tether";
        protected override void Initialize()
        {
            ModName = "Tetherporter";

            var modLocator = new FolderLocator(Log);
            _dbManager = new CsvManager(modLocator.GetDatabaseFolder(ModName));

            CommandManager.CommandPrexix = "!";
            CommandManager.CommandList.Add(new ChatCommand("tetherport", Tetherport));
            CommandManager.CommandList.Add(new ChatCommand("untether", Untether));
        }

        private async Task Tetherport(MessageData messageData)
        {
            PlayerInfo player = (PlayerInfo)await RequestManager.SendGameRequest(
                CmdId.Request_Player_Info, new Id() { id = messageData.SenderEntityId }
                );

            if(!_dbManager.LoadRecord<PortalRecord>(PortalFileName, out var portal))
            {
                Log($"Entity {player.entityId}/{player.playerName} requested Tetherport but no portal was found.");
                await MessagePlayer(player.entityId, $"No portal is currently active. Cannot Tetherport.", 10, MessagerPriority.Red);
                return;
            }

            var record = new TetherporterRecord(player.steamId, player.entityId, player.playfield, player.pos, player.rot);

            _dbManager.SaveRecord(FormatRecordId(player.steamId), record);

            await TeleportPlayer(player.entityId, portal.Playfield, portal.PosX, portal.PosY, portal.PosZ, portal.RotX, portal.RotY, portal.RotZ);
            Log($"Tetherported entity: {player.entityId}/{player.playerName} to portal.");

            await MessagePlayer(player.entityId, $"Created Tetherporter tether! Welcome to {portal.EventName}!", 10);
        }

        private async Task Untether(MessageData messageData)
        {
            PlayerInfo player = (PlayerInfo)await RequestManager.SendGameRequest(
                CmdId.Request_Player_Info, new Id { id = messageData.SenderEntityId }
                );

            if (!_dbManager.LoadRecord<PortalRecord>(PortalFileName, out _))
            {
                Log($"Entity {player.entityId}/{player.playerName} requested Untether but no portal was found.");
                await MessagePlayer(player.entityId, $"No event portal is active. Cannot Untether.", 10, MessagerPriority.Red);
                return;
            }

            if (!_dbManager.LoadRecord<TetherporterRecord>(FormatRecordId(player.steamId), out var tetherporterRecord))
            {
                Log($"Entity {player.entityId}/{player.playerName} requester Untether but no tether was found.");
                await MessagePlayer(player.entityId, $"No tether was found.", 5, MessagerPriority.Red);
                return;
            }

            await TeleportPlayer(player.entityId, tetherporterRecord.Playfield, 
                tetherporterRecord.PosX, tetherporterRecord.PosY, tetherporterRecord.PosZ,
                tetherporterRecord.RotX, tetherporterRecord.RotY, tetherporterRecord.RotZ);
        }

        private string FormatRecordId(string steamId)
        {
            return $"{steamId}.tether";
        }

    }
}
