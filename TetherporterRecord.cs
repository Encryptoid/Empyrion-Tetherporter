using CsvHelper;
using Eleon.Modding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tetherporter
{ 
    public class TetherporterRecord
    { // This class is used to pass generics to DatabaseManager and not Eleon classes

        public TetherporterRecord() { } //For CsvHelper
        public TetherporterRecord(string steamId, int entityId, string playfield, PVector3 pos, PVector3 rot)//Extend cnstr to remove Eleon from this class entirely
        {
            SteamId = steamId;
            EntityId = entityId;
            Playfield = playfield;
            PosX = pos.x;
            PosY = pos.y;
            PosZ = pos.z;
            RotX = rot.x;
            RotY = rot.y;
            RotZ = rot.z;
        }

        public string SteamId { get; set; }
        public int EntityId { get; set; }
        public string Playfield { get; set; }
        public float PosX { get; set; }
        public float PosY { get; set; }
        public float PosZ { get; set; }
        public float RotX { get; set; }
        public float RotY { get; set; }
        public float RotZ { get; set; }
    }
}
