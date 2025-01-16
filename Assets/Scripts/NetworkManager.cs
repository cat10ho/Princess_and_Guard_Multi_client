using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Tracing;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager instance;

    public InputField ipInputField;
    public InputField portInputField;
    public InputField deviceIdInputField;
    public InputField creatroomInputField;
    private TcpClient tcpClient;
    private NetworkStream stream;

    public GameObject GameroomUI;
    
    WaitForSecondsRealtime wait;

    private byte[] receiveBuffer = new byte[4096];
    private List<byte> incompleteData = new List<byte>();
   void Awake() {        
        instance = this;
        wait = new WaitForSecondsRealtime(5);
    }

 public void OnStartButtonClicked() {
        string ip = ipInputField.text;
        string port = portInputField.text;

        if (IsValidPort(port)) {
            int portNumber = int.Parse(port); //숫자로 바꿔주는건거 같은데?

            if (deviceIdInputField.text != "") {
                GameManager.instance.deviceId = deviceIdInputField.text; //게임 메니저에 넣어주고.
            } else {
                if (GameManager.instance.deviceId == "") {
                    GameManager.instance.deviceId = GenerateUniqueID(); //이건 뭐지? 일단 받아온다고 생각해.
                }
            }
  
            if (ConnectToServer(ip, portNumber)) { //일단 이게 연결 시도인듯?
                StartGame(); //연결되면 이렇게 시작
            } else {
                Debug.LogError("접속실패");
            }
            
        } else {
            Debug.LogError("IP와 포트 확인 실패");
        }
    }
    
     bool IsValidIP(string ip) //이거 유효성 검사를 안썼네 ㅋㅋ
    {
        // 간단한 IP 유효성 검사
        return System.Net.IPAddress.TryParse(ip, out _);
    }

    bool IsValidPort(string port)
    {
        // 간단한 포트 유효성 검사 (0 - 65535)
        if (int.TryParse(port, out int portNumber))
        {
            return portNumber > 0 && portNumber <= 65535;
        }
        return false;
    }

    public string GenerateUniqueID() {
        return System.Guid.NewGuid().ToString();
    }

  bool ConnectToServer(string ip, int port) {
        try {
            tcpClient = new TcpClient(ip, port);
            stream = tcpClient.GetStream(); // 뭔가 연결 시도같은 느낌?
            Debug.Log($"Connected to {ip}:{port}");

            return true;
        } catch (SocketException e) {
            Debug.LogError($"SocketException: {e}");
            return false;
        }
    }

    void StartGame()
    {
        // 게임 시작 코드 작성
        Debug.Log("Game Started");
        StartReceiving(); // Start receiving data
        SendInitialPacket();

    }
       void StartReceiving() {
        _ = ReceivePacketsAsync(); //이건 뭐야.
    }

     public static byte[] ToBigEndian(byte[] bytes) {
        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(bytes);
        }
        return bytes;
    }

    byte[] CreatePacketHeader(int dataLength, Packets.PacketType packetType) { 
        int packetLength = 4 + 1 + dataLength; // 전체 패킷 길이 (헤더 포함)
        byte[] header = new byte[5]; // 4바이트 길이 + 1바이트 타입

        // 첫 4바이트: 패킷 전체 길이
        byte[] lengthBytes = BitConverter.GetBytes(packetLength);
        lengthBytes = ToBigEndian(lengthBytes); //거꾸로 해주기.
        Array.Copy(lengthBytes, 0, header, 0, 4);

        // 다음 1바이트: 패킷 타입
        header[4] = (byte)packetType;

        return header;
    }

     async void SendPacket<T>(T payload, uint handlerId)
    {
        // ArrayBufferWriter<byte>를 사용하여 직렬화
        var payloadWriter = new ArrayBufferWriter<byte>();
        Packets.Serialize(payloadWriter, payload);
        byte[] payloadData = payloadWriter.WrittenSpan.ToArray();

        CommonPacket commonPacket = new CommonPacket
        {
            handlerId = handlerId,
            userId = GameManager.instance.deviceId,
            version = GameManager.instance.version,
            payload = payloadData,
        };

        // ArrayBufferWriter<byte>를 사용하여 직렬화
        var commonPacketWriter = new ArrayBufferWriter<byte>();
        Packets.Serialize(commonPacketWriter, commonPacket);
        byte[] data = commonPacketWriter.WrittenSpan.ToArray();

        // 헤더 생성
        byte[] header = CreatePacketHeader(data.Length, Packets.PacketType.Normal);

        // 패킷 생성
        byte[] packet = new byte[header.Length + data.Length];
        Array.Copy(header, 0, packet, 0, header.Length);
        Array.Copy(data, 0, packet, header.Length, data.Length);

        await Task.Delay(GameManager.instance.latency);
        
        // 패킷 전송
        stream.Write(packet, 0, packet.Length);
    }
  async System.Threading.Tasks.Task ReceivePacketsAsync() {
        while (tcpClient.Connected) { //연결되면 계속 시도하는듯?
            try {
                int bytesRead = await stream.ReadAsync(receiveBuffer, 0, receiveBuffer.Length); //길이? 라는데.
                if (bytesRead > 0) {
                    ProcessReceivedData(receiveBuffer, bytesRead);
                }
            } catch (Exception e) {
                Debug.LogError($"Receive error: {e.Message}");
                break;
            }
        }
    }

 void ProcessReceivedData(byte[] data, int length) {
         incompleteData.AddRange(data.AsSpan(0, length).ToArray());

        while (incompleteData.Count >= 5)
        {
            // 패킷 길이와 타입 읽기
            byte[] lengthBytes = incompleteData.GetRange(0, 4).ToArray();
            int packetLength = BitConverter.ToInt32(ToBigEndian(lengthBytes), 0);
            Packets.PacketType packetType = (Packets.PacketType)incompleteData[4];

            if (incompleteData.Count < packetLength)
            {
                // 데이터가 충분하지 않으면 반환
                return;
            }

            // 패킷 데이터 추출
            byte[] packetData = incompleteData.GetRange(5, packetLength - 5).ToArray(); //이건 자른거?.
            incompleteData.RemoveRange(0, packetLength); //어 이게 자르고, 남은건 두는듯.

            // Debug.Log($"Received packet: Length = {packetLength}, Type = {packetType}");

            switch (packetType)
            {
                case Packets.PacketType.Normal:
                    HandleJoinLobbyPacket(packetData); //일반처리
                    break;
                case Packets.PacketType.JOINLOBBY:
                    HandleJoinLobbyPacket(packetData); //로비 입장 처리.
                    break;
                case Packets.PacketType.Location:
                    HandleLocationPacket(packetData); //위치처리.
                    break;
                case Packets.PacketType.JOINROOM:
                    HandleJoinRoomPacket(packetData); //방 입장 처리.
                    break;
                case Packets.PacketType.GAME_START:
                    HandleGameReadyPacket(packetData); //게임 시작 처리.
                    break;
                case Packets.PacketType.CARRYUPDATE:
                    HandleCarryUpdatePacket(packetData); //게임 내 이벤트 중 공주 들기,놓기 처리.
                    break;
            }
        }
    }

    void HandleNormalPacket(byte[] packetData) {
        // 패킷 데이터 처리
        var response = Packets.Deserialize<Response>(packetData);
        // Debug.Log($"HandlerId: {response.handlerId}, responseCode: {response.responseCode}, timestamp: {response.timestamp}");
        
        if (response.responseCode != 0) {
            Debug.LogError("패킷 교환 에러"); //패킷  교환 에러
            return;
        }

        if (response.data != null && response.data.Length > 0) {
            if (response.handlerId == 0) {
                 HandleJoinLobbyPacket(response.data); //이거 아닐수도 있음. 나중에 확인 ㄱ
            }
            if (response.handlerId == 4) {
                HandleGameReadyPacket(response.data); //애는 게임 시작 입니다. 이제 시작하는 거임.
            }
             if (response.handlerId == 6) {
                HandleCarryUpdatePacket(response.data); 
            }
            ProcessResponseData(response.data); 
        }
    }
      void ProcessResponseData(byte[] data) {
        try {
            // var specificData = Packets.Deserialize<SpecificDataType>(data);
            string jsonString = Encoding.UTF8.GetString(data);
            Debug.Log($"Processed SpecificDataType: {jsonString}");
        } catch (Exception e) {
            Debug.LogError($"Error processing response data: {e.Message}");
        }
    }
    
        void HandleLocationPacket(byte[] data) {
        try {
           LocationUpdate response;

            if (data.Length > 0) {
                // 패킷 데이터 처리
                response = Packets.Deserialize<LocationUpdate>(data);
        
            } else {
                // data가 비어있을 경우 빈 배열을 전달
                response = new LocationUpdate { users = new List<LocationUpdate.UserLocation>() };
            }
            
            GameManager.instance.Spawn(response);
        } catch (Exception e) {
            Debug.LogError($"Error HandleLocationPacket: {e.Message}");
        }
    }



    void SendInitialPacket() {
        InitialPayload initialPayload = new InitialPayload
        {
            deviceId = GameManager.instance.deviceId,
            latency = GameManager.instance.latency,
        };

        // handlerId는 0으로 가정
        SendPacket(initialPayload, (uint)Packets.HandlerIds.Init);
    }

    public void SendJoinLobbyPayloadPacket() {
        JoinLobbyPayload JoinLobbyPayload = new JoinLobbyPayload
        {
            deviceId = GameManager.instance.deviceId,
        };

        // handlerId는 0으로 가정
        SendPacket(JoinLobbyPayload, (uint)Packets.HandlerIds.JoinLobby);
    }

     void HandleJoinLobbyPacket(byte[] data) {
        try {
            JoinLobby response;

            if (data.Length > 0) {
                // 패킷 데이터 처리
                response = Packets.Deserialize<JoinLobby>(data);
            } else {
                // data가 비어있을 경우 빈 배열을 전달
                response = new JoinLobby { rooms = new List<JoinLobby.RoomData>() };
            }
            
                RoomManager.instance.DisplayRooms(response);
        
        } catch (Exception e) {
            Debug.LogError($"Error HandleLocationPacket: {e.Message}");
        }
    }

   public void SendCreateRoomPayloadPacket() {
        CreateRoomPayload CreateRoomPayload = new CreateRoomPayload
        {
            deviceId = GameManager.instance.deviceId,
            roomName = creatroomInputField.text,
        };
        // handlerId는 2으로 가정
        SendPacket(CreateRoomPayload, (uint)Packets.HandlerIds.CreateRoom);
    }

   public void SendJoinRoomPayloadPacket() {
        JoinRoomPayload JoinRoomPayload = new JoinRoomPayload
        {
            deviceId = GameManager.instance.deviceId,
            roomName = RoomManager.instance.selectedRoom,
        };
        // handlerId는 2으로 가정
        SendPacket(JoinRoomPayload, (uint)Packets.HandlerIds.JoinRoom);
       
    }

      public void SendRoomDataPayloadPacket() { //애는 방에 추가 안되게. 그냥 데이터만 다시.
        RoomDataPayload RoomDataPayload = new RoomDataPayload
        {
            deviceId = GameManager.instance.deviceId,
            roomName = RoomManager.instance.selectedRoom,
        };

        // handlerId는 2으로 가정
        SendPacket(RoomDataPayload, (uint)Packets.HandlerIds.RoomData);
    }


    void HandleJoinRoomPacket(byte[] data) {
        try {
            JoinRoom response;

            if (data.Length > 0) {
                // 패킷 데이터 처리
                response = Packets.Deserialize<JoinRoom>(data);
            } else {
                // data가 비어있을 경우 빈 배열을 전달
                response = new JoinRoom { players = new List<JoinRoom.PlayerData>() };
            }

            PlayerManager.instance.UpdatePlayerList(response);

        } catch (Exception e) {
            Debug.LogError($"Error HandleLocationPacket: {e.Message}");
        }
    }

 public void SendGameReadyPayloadPacket() {
        GameReadyPayload GameReadyPayload = new GameReadyPayload
        {
            deviceId = GameManager.instance.deviceId,
            role = PlayerManager.instance.selectedrole,
            roomName = RoomManager.instance.selectedRoom,
        };

        // handlerId는 2으로 가정
        SendPacket(GameReadyPayload, (uint)Packets.HandlerIds.GameReady);
    }

 public void SendChangeRolePayloadPacket() {
        ChangeRolePayload ChangeRolePayload = new ChangeRolePayload
        {
            deviceId = GameManager.instance.deviceId,
            role = PlayerManager.instance.selectedrole,
            roomName = RoomManager.instance.selectedRoom,
        };

        SendPacket(ChangeRolePayload, (uint)Packets.HandlerIds.ChangeRole);
    }




     void HandleGameReadyPacket(byte[] data) {
        try {
          
           Start response;

            if (data.Length > 0) {
                // 패킷 데이터 처리
                response = Packets.Deserialize<Start>(data);
            } else {
                // data가 비어있을 경우 빈 배열을 전달
                response = new Start {};
            }
          GameManager.instance.GameStart(response);

        } catch (Exception e) {
            Debug.LogError($"Error HandleLocationPacket: {e.Message}");
        }
    }


  public void SendLocationUpdatePacket(float x, float y) {
        LocationUpdatePayload locationUpdatePayload = new LocationUpdatePayload
        {
            roomName = RoomManager.instance.selectedRoom,
            x = x,
            y = y,
        };

        SendPacket(locationUpdatePayload, (uint)Packets.HandlerIds.LocationUpdate);
    }

    public void SendCarryUpdatePayloadPacket(string knightId , string princessId, bool isCarried)
{
    // 공주 들기/내리기 상태를 서버로 전송
    CarryUpdatePayload CarryUpdatePayload = new CarryUpdatePayload
    {   
        knightId = knightId,
        princessId = princessId,
        isCarried = isCarried
    };

    SendPacket(CarryUpdatePayload, (uint)Packets.HandlerIds.CarryUpdate);
}

     void HandleCarryUpdatePacket(byte[] data) {
        try {
            CarryUpdate response;

            if (data.Length > 0) {
                // 패킷 데이터 처리
                response = Packets.Deserialize<CarryUpdate>(data);
            } else {
                // data가 비어있을 경우 빈 배열을 전달
                response = new CarryUpdate {};
            }
            GameManager.instance.UpdateCarryState(response);
        } catch (Exception e) {
            Debug.LogError($"Error HandleLocationPacket: {e.Message}");
        }
    }


}

