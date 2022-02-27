using Eleon;
using Eleon.Modding;
using EmpyrionModdingFramework;
using InventoryManagement;
using ModLocator;
using System.IO;
using System.Threading.Tasks;
using EmpyrionModdingFramework.Database;
using System.Linq;
using System.Collections.Generic;
using System;
using EmpyrionModdingFramework.Teleport;

namespace Tetherporter
{
    public class Tetherporter : EmpyrionModdingFrameworkBase
    {
        private IDatabaseManager _dbManager;
        private const string PortalFileName = "portal.tether";

        private const string PortalCreateCommand = "portal-create";
        private const string PortalDeleteCommand = "portal-delete-please";
        private const string PortalAdminCommand = "portal-admin";
        protected override void Initialize()
        {
            ModName = "Tetherporter";

            var modLocator = new FolderLocator(Log);
            _dbManager = new CsvManager(modLocator.GetDatabaseFolder(ModName));

            CommandManager.CommandPrexix = "!";

            //All Players
            CommandManager.CommandList.Add(new ChatCommand("tetherport", ListPortals));
            CommandManager.CommandList.Add(new ChatCommand("ttp", ListPortals)); //short hand
            CommandManager.CommandList.Add(new ChatCommand("untether", Untether));

            //Admin Only
            CommandManager.CommandList.Add(new ChatCommand(PortalCreateCommand, CreatePortal, PlayerPermission.Admin, 1));
            CommandManager.CommandList.Add(new ChatCommand(PortalAdminCommand, PortalToggleAdmin, PlayerPermission.Admin, 1));
            CommandManager.CommandList.Add(new ChatCommand(PortalDeleteCommand, DeletePortal, PlayerPermission.Admin, 1));
        }

        private async Task CreatePortal(MessageData messageData, object[] parameters)
        {
            var player = await QueryPlayerInfo(messageData.SenderEntityId);

            if (parameters == null || parameters.Length == 0 ||
                !ChatCommand.ParseStringParam(parameters, 0, out var portalRecordName))
            {
                await MessagePlayer(player.entityId, $"Could not parse Portal Name from passed arguments. {FormatCommand(PortalCreateCommand)}", 5, MessagerPriority.Red);
                return;
            }

            var newLocation = new LocationRecord()
            {
                Name = portalRecordName,
                AdminYN = 'Y', //Portals are admin only by default
                Playfield = player.playfield,
                PosX = player.pos.x,
                PosY = player.pos.y,
                PosZ = player.pos.z,
                RotX = player.rot.x,
                RotY = player.rot.y,
                RotZ = player.rot.z
            };

            _dbManager.SaveRecord(PortalFileName, newLocation, false);

            await MessagePlayer(player.entityId, $"Succesfully created portal: {portalRecordName}", 5);
        }

        private async Task DeletePortal(MessageData messageData, object[] parameters)
        {
            var player = await QueryPlayerInfo(messageData.SenderEntityId);

            if (parameters == null || parameters.Length == 0 ||
                !ChatCommand.ParseStringParam(parameters, 0, out var requestedPortalName))
            {
                await MessagePlayer(player.entityId, $"Could not parse Portal Name from passed arguments. {FormatCommand(PortalCreateCommand)}", 5, MessagerPriority.Red);
                return;
            }

            var success = false;
            var existingRecords = _dbManager.LoadRecords<LocationRecord>(PortalFileName);
            var newRecords = new List<LocationRecord>();

            foreach (var portal in existingRecords)
            {
                if (portal.Name == requestedPortalName)
                {
                    success = true;
                }
                else
                {
                    newRecords.Add(portal);
                }
            }

            if (success)
            {
                _dbManager.SaveRecords(PortalFileName, newRecords, true);
                await MessagePlayer(player.entityId, $"Succesfully deleted Portal: {requestedPortalName}", 5, MessagerPriority.Red);
            }
            else
            {
                await MessagePlayer(player.entityId, $"Could not located Portal to delete: {requestedPortalName}", 5, MessagerPriority.Red);
            }
        }

        private string FormatCommand(string command)
        {
            return $"Command (no quotes): !{command} \"PortalName\"";
        }

        private async Task ListPortals(MessageData messageData)
        {
            var player = await QueryPlayerInfo(messageData.SenderEntityId);
            var records = _dbManager.LoadRecords<LocationRecord>(PortalFileName);

            ShowLinkedDialog(player.entityId, FormatLocationList(records, IsAdmin(player.permission)), "Tetherport Infomation!", DialogTetherportPlayer);
        }

        private async void DialogTetherportPlayer(int buttonIdx, string linkId, string inputContent, int playerId, int customValue)
        {
            Log($"Entity Id: {playerId} began tetherport to {linkId}");
            if (string.IsNullOrWhiteSpace(linkId))
                return;

            var player = await QueryPlayerInfo(playerId);

            var existingRecord = _dbManager.LoadRecords<PlayerLocationRecord>(TetherporterHelper.FormatTetherportFileName(player.steamId))?.FirstOrDefault() != null;

            //Only save new tether record if one does not exist
            if (!existingRecord)
            {
                Log($"Saving new record tetherport record for EntityId: {playerId}");
                _dbManager.SaveRecord(TetherporterHelper.FormatTetherportFileName(player.steamId), player.ToPlayerLocationRecord(), true);
            }

            var portals = _dbManager.LoadRecords<LocationRecord>(PortalFileName);
            if (portals == null)
            {
                await MessagePlayer(player.entityId, $"Could not find Tetherport Locations.", 10, MessagerPriority.Red);
                return;
            }

            // Find matching record and teleport player
            foreach (var portal in portals)
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

            var existingRecord = _dbManager.LoadRecords<PlayerLocationRecord>(TetherporterHelper.FormatTetherportFileName(player.steamId))?.FirstOrDefault();
            if (existingRecord == null)
            {
                Log($"Entity {player.entityId}/{player.playerName} requested Untether but no tether was found @ " + TetherporterHelper.FormatTetherportFileName(player.steamId));
                await MessagePlayer(player.entityId, $"No untether was found.", 5, MessagerPriority.Red);
                return;
            }

            await TeleportPlayer(player.entityId, existingRecord.Playfield,
                existingRecord.PosX, existingRecord.PosY, existingRecord.PosZ,
                existingRecord.RotX, existingRecord.RotY, existingRecord.RotZ);

            _dbManager.DeleteRecord(TetherporterHelper.FormatTetherportFileName(player.steamId));
            Log($"Deleted Tetherport record for SteamId: {player.steamId}, CurrentEntityId: {player.entityId}");
        }

        private async Task PortalToggleAdmin(MessageData messageData, object[] parameters)
        {
            var player = await QueryPlayerInfo(messageData.SenderEntityId);

            if (parameters == null || parameters.Length == 0 ||
                !ChatCommand.ParseStringParam(parameters, 0, out var requestedPortalName))
            {
                await MessagePlayer(player.entityId, $"Could not parse Portal Name from passed arguments. {FormatCommand(PortalAdminCommand)}", 5, MessagerPriority.Red);
                return;
            }

            var success = false;
            var existingRecords = _dbManager.LoadRecords<LocationRecord>(PortalFileName);
            var newRecords = new List<LocationRecord>();
            var oldValue = ' ';

            foreach (var portal in existingRecords)
            {
                if (portal.Name == requestedPortalName)
                {
                    oldValue = portal.AdminYN;
                    portal.AdminYN = InvertYN(portal.AdminYN);
                    success = true;
                }
                newRecords.Add(portal);
            }

            if (success)
            {
                _dbManager.SaveRecords(PortalFileName, newRecords, true);
                await MessagePlayer(player.entityId, $"Succesfully set Admin privileges for portal \"{requestedPortalName}\" from \'{oldValue}\' to '{InvertYN(oldValue)}'", 5, MessagerPriority.Yellow);
            }
            else
            {
                await MessagePlayer(player.entityId, $"Failed to changed portal Admin status. Attempted Name Lookup: \"{requestedPortalName}\"", 5, MessagerPriority.Red);
            }
        }

        private string FormatLocationList(List<LocationRecord> locations, bool isAdmin)
        {
            var uiString = $"Click on one of the below locations to Tetherport there!\nThen you can type '!untether' to return back to your original location!\n\n";

            foreach(var location in locations)
            {
                if (!isAdmin && location.AdminYN == 'N')
                {
                    uiString += $"{FormatLocation(location)}\n";
                }

                if (isAdmin)
                {
                    uiString += $"{AdminFormatLocation(location)}\n";
                }
            }

            uiString += "\n\n";

            if (isAdmin)
            {
                uiString += $"The following commands are made available to admins:\n";
                uiString += $"***N.B Currently no spaces are allowed in a PortalName***\n";
                uiString += $"\n{FormatCommand(PortalCreateCommand)}\n{FormatCommand(PortalAdminCommand)}\n{FormatCommand(PortalDeleteCommand)}\n";
            }

            return uiString;
        }

        private string AdminFormatLocation(LocationRecord location)
        {
            return $"<link=\"{location.Name}\"><indent=15%><line-height=150%>{location.Name} | {location.Playfield} | AdminYN={location.AdminYN}</line-height></indent></link>";
        }

        private string FormatLocation(LocationRecord location)
        {
            return $"<link=\"{location.Name}\"><indent=15%><line-height=150%>{location.Name} | {location.Playfield}</line-height></indent></link>";
        }
    }
}
