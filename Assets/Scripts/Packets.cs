using UnityEngine;
using ProtoBuf;
using System.IO;
using System.Buffers;
using System.Collections.Generic;
using System;

public class Packets : MonoBehaviour
{
    public enum PacketType { Ping, Normal, JOINLOBBY, Location, JOINROOM, GAME_START, CARRYUPDATE }
    public enum HandlerIds {
        Init = 0,
        JoinLobby = 1,
        CreateRoom= 2,
        JoinRoom= 3,
        GameReady = 4,
        LocationUpdate = 5,
        CarryUpdate = 6,
        RoomData = 7,
        ChangeRole = 8,
    }

    public static void Serialize<T>(IBufferWriter<byte> writer, T data)
    {
        Serializer.Serialize(writer, data);
    }

    public static T Deserialize<T>(byte[] data) {
        try {
            using (var stream = new MemoryStream(data)) {
                return ProtoBuf.Serializer.Deserialize<T>(stream);
            }
        } catch (Exception ex) {
            Debug.LogError($"Deserialize: Failed to deserialize data. Exception: {ex}");
            throw;
        }
    }
}

[ProtoContract]
public class CommonPacket
{
    [ProtoMember(1)]
    public uint handlerId { get; set; }

    [ProtoMember(2)]
    public string userId { get; set; }

    [ProtoMember(3)]
    public string version { get; set; }

    [ProtoMember(4)]
    public byte[] payload { get; set; }
}

[ProtoContract]
public class InitialPayload
{
    [ProtoMember(1, IsRequired = true)]
    public string deviceId { get; set; }
    
     [ProtoMember(2, IsRequired = true)]
    public float latency { get; set; }
} //여기서 방을 받는다. 뭐 유저 입장등등 하겠지만. 이건 게임 참가.


[ProtoContract]
public class JoinLobbyPayload
{
    [ProtoMember(1, IsRequired = true)]
    public string deviceId { get; set; }
} //이러고 받을때 지금 연결이 되있는지 확인해야 겠지.


[ProtoContract]
public class JoinLobby
{
    [ProtoMember(1)]
    public List<RoomData> rooms { get; set; }

    [ProtoContract]
    public class RoomData
    {
        [ProtoMember(1)]
        public string roomName { get; set; }

        [ProtoMember(2)]
        public uint maxPlayers { get; set; }

        [ProtoMember(3)]
        public uint currentPlayers { get; set; }
    }
}

[ProtoContract]
public class CreateRoomPayload
{
    [ProtoMember(1, IsRequired = true)]
    public string deviceId { get; set; }

    [ProtoMember(2, IsRequired = true)]
    public string roomName { get; set; }
}

[ProtoContract]
public class JoinRoomPayload
{
    [ProtoMember(1, IsRequired = true)]
    public string deviceId { get; set; }

    [ProtoMember(2, IsRequired = true)]
    public string roomName { get; set; }
}

[ProtoContract]
public class JoinRoom
{
    [ProtoMember(1)]
    public List<PlayerData> players { get; set; }

    [ProtoMember(2, IsRequired = true)]
    public string roomName { get; set; }

    [ProtoContract]
    public class PlayerData
    {
        [ProtoMember(1)]
        public string deviceId { get; set; }

        [ProtoMember(2)]
        public string role { get; set; }
    }
}



[ProtoContract]
public class  GameReadyPayload //직업, 아이디, 방 뭐 그런거 받아서 레디가 가능한지. 확인. 이제 직업이 두개가 겹치면 안됨.
{
    [ProtoMember(1, IsRequired = true)]
    public string deviceId { get; set; }

     [ProtoMember(2, IsRequired = true)]
    public string role { get; set; }
    [ProtoMember(3, IsRequired = true)]
    public string roomName { get; set; }
}

[ProtoContract]
public class  ChangeRolePayload //직업, 아이디, 방 뭐 그런거 받아서 레디가 가능한지. 확인. 이제 직업이 두개가 겹치면 안됨.
{
    [ProtoMember(1, IsRequired = true)]
    public string deviceId { get; set; }

     [ProtoMember(2, IsRequired = true)]
    public string role { get; set; }
    [ProtoMember(3, IsRequired = true)]
    public string roomName { get; set; }
}


[ProtoContract]
public class LocationUpdatePayload {
    [ProtoMember(1, IsRequired = true)]
    public string roomName { get; set; }
    [ProtoMember(2, IsRequired = true)]
    public float x { get; set; }
    [ProtoMember(3, IsRequired = true)]
    public float y { get; set; }
}

[ProtoContract]
public class LocationUpdate
{
    [ProtoMember(1)]
    public List<UserLocation> users { get; set; }

    [ProtoContract]
    public class UserLocation
    {
        [ProtoMember(1)]
        public string id { get; set; }

        [ProtoMember(2)]
        public string role { get; set; }

        [ProtoMember(3)]
        public float x { get; set; }

        [ProtoMember(4)]
        public float y { get; set; }
    }
}

[ProtoContract]
public class Response {
    [ProtoMember(1)]
    public uint handlerId { get; set; }

    [ProtoMember(2)]
    public uint responseCode { get; set; }

    [ProtoMember(3)]
    public long timestamp { get; set; }

    [ProtoMember(4)]
    public byte[] data { get; set; }
}


[ProtoContract]
public class CarryUpdatePayload
{   
    [ProtoMember(1, IsRequired = true)]
    public string knightId { get; set; }

    [ProtoMember(2, IsRequired = true)]
    public string princessId { get; set; }

    [ProtoMember(3, IsRequired = true)]
    public bool isCarried { get; set; }
}

[ProtoContract]
public class CarryUpdate
{
    [ProtoMember(1, IsRequired = true)]
    public string princessId { get; set; }

    [ProtoMember(2, IsRequired = true)]
    public bool isCarried { get; set; }

    [ProtoMember(3, IsRequired = true)]
    public string carrierId { get; set; }
}

[ProtoContract]
public class  Start {

    [ProtoMember(1)]
    public List<UserStartLocation> users { get; set; }

    [ProtoMember(2)]
    public string roomName { get; set; }

    [ProtoMember(3)]
    public long timestamp { get; set; }

    [ProtoContract]
    public class UserStartLocation
    {
        [ProtoMember(1)]
        public string id { get; set; }

        [ProtoMember(2)]
        public string role { get; set; }

        [ProtoMember(3)]
        public float x { get; set; }

        [ProtoMember(4)]
        public float y { get; set; }
    }

}



[ProtoContract]
public class RoomDataPayload
{
    [ProtoMember(1, IsRequired = true)]
    public string deviceId { get; set; }

    [ProtoMember(2, IsRequired = true)]
    public string roomName { get; set; }
}
