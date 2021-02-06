using Networking;
using System;
using System.Collections.Generic;

public class ConstiCharacterSpawnsPacket : Packet {
    public class CharacterSpawn {
        private readonly Guid clientId;
        private readonly int spawnIndex;

        public CharacterSpawn(Guid clientId, int spawnIndex) {
            this.clientId = clientId;
            this.spawnIndex = spawnIndex;
        }

        public Guid GetClientId() {
            return clientId;
        }

        public int GetSpawnIndex() {
            return spawnIndex;
        }
    }

    public CharacterSpawn[] spawns;

    public ConstiCharacterSpawnsPacket(byte[] bytes) : base(bytes) {
        int count = ReadInt();
        spawns = new CharacterSpawn[count];
        for (int index = 0; index < count; index++) {
            Guid clientId = ReadGuid();
            int spawnIndex = ReadInt();
            spawns[index] = new CharacterSpawn(clientId, spawnIndex);
        }
    }

    public ConstiCharacterSpawnsPacket(CharacterSpawn[] spawns) : base(SpawnsToBytes(spawns)) {
        this.spawns = spawns;
    }

    public override void Validate() { }

    public CharacterSpawn[] GetSpawns() {
        return spawns;
    }

    private static byte[] SpawnsToBytes(CharacterSpawn[] spawns) {
        List<byte[]> pack = new List<byte[]>();
        pack.Add(Bytes.Of(spawns.Length));
        foreach (var spawn in spawns) {
            pack.Add(Bytes.Of(spawn.GetClientId()));
            pack.Add(Bytes.Of(spawn.GetSpawnIndex()));
        }
        return Bytes.Pack(pack.ToArray());
    }
}
