using CsvHelper;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Tetherporter
{
    interface IDatabaseManager
    {
        void SaveTetherportRecord(TetherporterRecord record);
        bool LoadTetherporterRecord(string steamId, out TetherporterRecord record);
        bool LoadPortal(out PortalRecord portal);
    }

    class CsvManager: IDatabaseManager
    {
        private string _databasePath;
        private Action<string> _log;
        public CsvManager(string databasePath, Action<string> logFunc)
        {
            _databasePath = databasePath;
            _log = logFunc;
        }

        public void SaveTetherportRecord(TetherporterRecord record)
        {
            var path = FormatFilePath(record.SteamId);

            _log($"Logging record [[{record}]] to path {path}");

            using(var stream = new StreamWriter(path))
                using(var csv = new CsvWriter(stream, CultureInfo.InvariantCulture))
                    csv.WriteRecords(new List<TetherporterRecord> { record });
            
        }

        public bool LoadPortal(out PortalRecord portal)
        {
            portal = null;
            var path = FormatPortalPath();

            if (!File.Exists(path))
                return false;

            using (var stream = new StreamReader(path))
            using (var csv = new CsvReader(stream, CultureInfo.InvariantCulture))
                portal = csv.GetRecords<PortalRecord>().SingleOrDefault();

            return portal != null;
        }

        public bool LoadTetherporterRecord(string steamId, out TetherporterRecord record)
        {
            record = null;
            var path = FormatFilePath(steamId);

            _log($"Loading record from: {path}");

            using (var stream = new StreamReader(path))
                using (var csv = new CsvReader(stream, CultureInfo.InvariantCulture))
                    record =  csv.GetRecords<TetherporterRecord>().Single();

            return record != null;
        }

        private string FormatFilePath(string steamId)
        {
            var fileName = $"{steamId}.tether";
            return Path.Combine(_databasePath, fileName);
        }

        private string FormatPortalPath()
        {
            return Path.Combine(_databasePath, "portal.tether");
        }
    }
}
