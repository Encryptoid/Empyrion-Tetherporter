using Eleon;
using Eleon.Modding;
using EmpyrionModdingFramework;
using InventoryManagement;
using ModLocator;
using System.IO;
using System.Threading.Tasks;
using EmpyrionModdingFramework.Database;
using EmpyrionModdingFramework.Teleport;
using System.Linq;
using System.Collections.Generic;
using System;

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

            CommandManager.CommandList.Add(new ChatCommand("tetherport", ListPortals));
            CommandManager.CommandList.Add(new ChatCommand("ttp", ListPortals));

            CommandManager.CommandList.Add(new ChatCommand("create-portal", CreatePortal, PlayerPermission.Admin, 1));

            CommandManager.CommandList.Add(new ChatCommand("untether", Untether));
        }

        private async Task CreatePortal(MessageData messageData, object[] parameters)
        {
            var player = await QueryPlayerInfo(messageData.SenderEntityId);

            if (parameters == null || parameters.Length == 0 ||
                !ChatCommand.ParseStringParam(parameters, 0, out var portalRecordName))
            {
                await MessagePlayer(player.entityId, "Could not parse Portal Name from passed arguments, command should be '!create-portal <PortalName>'", 5);
                return;
            }

            var newLocation = new LocationRecord()
            {
                Name = portalRecordName,
                EnabledYN = 'N', //Portals are disabled & Admin only by default
                Permission = "Admin",
                Playfield = player.playfield,
                PosX = player.pos.x,
                PosY = player.pos.y,
                PosZ = player.pos.z,
                RotX = player.rot.x,
                RotY = player.rot.y,
                RotZ = player.rot.z
            };

            _dbManager.SaveRecord<LocationRecord>(PortalFileName, newLocation, false);
        }

        private async Task ListPortals(MessageData messageData)
        {
            var player = await QueryPlayerInfo(messageData.SenderEntityId);

            Log($"Existis: {_dbManager._databasePath} {PortalFileName} " + File.Exists(Path.Combine(_dbManager._databasePath, PortalFileName)));

            var records = _dbManager.LoadRecords<LocationRecord>(PortalFileName);
            
            
            // TODO Exclude Admin and disabled portals
            Log((records?.Count ?? 100) + "rec count");



            ShowLinkedDialog(player.entityId, FormatLocationList(records, IsAdmin(player.permission)), "Tetherport Infomation!", DialogTetherportPlayer);
        }

        private async void DialogTetherportPlayer(int buttonIdx, string linkId, string inputContent, int playerId, int customValue)
        {
            Log("Link id was " + linkId + " & button was " + buttonIdx);
            if (string.IsNullOrWhiteSpace(linkId))
                return;

            var player = await QueryPlayerInfo(playerId);

            var existingRecord = _dbManager.LoadRecords<PlayerLocationRecord>(FormatTethersFileName(player.steamId))?.FirstOrDefault();
   
            //Only save new tether record if one does not exist
            if (existingRecord == null)
            { 
                _dbManager.SaveRecord(FormatTethersFileName(player.steamId), existingRecord, false);
            }

            Log("HEre");
            var portals = _dbManager.LoadRecords<LocationRecord>(PortalFileName);
            if (portals == null)
            {
                await MessagePlayer(player.entityId, $"Could not find Tetherport Locations.", 10, MessagerPriority.Red);
                return;
            }

            // Find matching record and teleport player
            foreach(var portal in portals)
            {
                if (string.Equals(portal.Name, linkId))
                {
                    //Teleport and inform player
                    await TeleportPlayer(player.entityId, portal.Playfield, portal.PosX, portal.PosY, portal.PosZ, portal.RotX, portal.RotY, portal.RotZ);
                    await MessagePlayer(player.entityId, $"Created Tetherporter tether! Welcome to {portal.Name}!", 10);
                    return;
                }
            }
        }

        private async Task Untether(MessageData messageData)
        {
            var player = await QueryPlayerInfo(messageData.SenderEntityId);

            var existingRecord = _dbManager.LoadRecords<PlayerLocationRecord>(FormatTethersFileName(player.steamId))?.FirstOrDefault();
            if (existingRecord == null)
            {
                Log($"Entity {player.entityId}/{player.playerName} requester Untether but no tether was found.");
                await MessagePlayer(player.entityId, $"No untether was found.", 5, MessagerPriority.Red);
                return;
            }

            await TeleportPlayer(player.entityId, existingRecord.Playfield,
                existingRecord.PosX, existingRecord.PosY, existingRecord.PosZ,
                existingRecord.RotX, existingRecord.RotY, existingRecord.RotZ);
        }
        

        private string FormatLocationList(List<LocationRecord> locations, bool isAdmin)
        {
            var uiString = $"Click on one of the below locations to Tetherport there!\nThen you can type '!untether' to return back to your original location!\n\n" +
                        $"{string.Join("\n", locations.Select(l => isAdmin ? AdminFormatLocation(l) : FormatLocation(l)))}\n\n";

            if (isAdmin)
            {
                uiString += "The following commands are made available to admins:\n!portal create <PortalName>\n!portal toggle permission\n!portal toggle active";
            }

            return uiString;
        }

        private string AdminFormatLocation(LocationRecord location)
        {
            return $"<link=\"{location.Name}\"><indent=15%><line-height=150%>{location.Name} | {location.Playfield} | Permission={location.Permission} | Enabled={location.EnabledYN}</line-height></indent></link>";
        }

        private string FormatLocation(LocationRecord location)
        {
            return $"<link=\"{location.Name}\"><indent=15%><line-height=150%>{location.Name} | {location.Playfield}</line-height></indent></link>";
        }

        private string FormatTethersFileName(string steamId)
        {
            return Path.Combine("Tethers", $"{steamId}.tether");
        }

    }
}
