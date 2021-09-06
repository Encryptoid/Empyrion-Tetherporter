using Eleon;
using Eleon.Modding;
using EmpyrionModdingFramework;
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
            Log("TeleportTether is running from folder: " + Directory.GetCurrentDirectory());

            var dbPath = "../Content/Mods/Tetherporter/Database";
            var chatPrefix = "!";
            CommandManager.CommandPrexix = chatPrefix;

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

            var record = _dbManager.LoadTetherporterRecord(player.steamId);

            await TeleportPlayer(player.entityId, record.Playfield, record.PosX, record.PosY, record.PosZ, record.RotX, record.RotY, record.RotZ);
        }
    }
}
