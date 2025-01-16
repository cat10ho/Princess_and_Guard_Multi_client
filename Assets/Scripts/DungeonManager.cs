using System.Collections;
using System.Collections.Generic; // List<>를 위해 추가
using UnityEngine;
using UnityEngine.Tilemaps;       // Tile 관련 클래스를 위해 추가

public class DungeonManager : MonoBehaviour
{
    public DungeonRoom dungeonRoom; // DungeonRoom 스크립트를 연결
    private List<RoomData> roomData = new List<RoomData>(); // 방 데이터를 저장하는 리스트
    
    //public PlayerInteraction playerInteraction; // PlayerInteraction을 위한 참조 변수 추가
    public Tile[] connectionTiles; // 연결 타일 배열 (03, 05 등) //지울 예정.

    private Tile connectionstartTile; // 시작 타일
    private Vector3Int startPointPosition; // 시작 타일 위치
    private Tile connectionendTile; // 끝 타일
    private Vector3Int endPointPosition; // 끝 타일 위치

    private List<Vector3Int> allCorridorPoints = new List<Vector3Int>(); // 최종 복도 배열

    private HashSet<Vector3Int> roomAreas = new HashSet<Vector3Int>(); // 모든 방의 좌표를 저장하는 해시셋

    void Start()
    {
        DefineRooms(); //던전 데이터 생성

        // if (playerInteraction != null)
        // {
        //     // roomData를 PlayerInteraction의 roomDataList에 할당
        //     playerInteraction.SetRoomDataList(roomData);
        // }
        foreach (var room in roomData)
        {
            CreateRoomConnections(room); //연결 타일 추가.

            if (dungeonRoom != null)
            {
                // DungeonRoom 스크립트의 CreateStartRoom 메서드를 호출
                dungeonRoom.CreateStartRoom(room);
            }
            else
            {
                Debug.LogError("DungeonRoom 스크립트를 연결하세요!");
            }

        }
        CreateCorridors(roomData);

        dungeonRoom.CreateStartRoom(allCorridorPoints);
        
    }
    void DefineRooms()
    {
        // Room 1 데이터
        RoomData room1 = new RoomData
        {
            roomType = "StartRoom",
            roomPosition = new Vector3Int(0, 0, 0),
            roomSize = new int[] { 11, 11 },
            additionalTiles = new List<ObjectData>()
        };

        ObjectData object1 = new ObjectData
        {
            objectPosition = new Vector3Int(5, 5, 0),
            objectTiles = new List<string> {"btn01"}
        };

        room1.additionalTiles.Add(object1); // Room 1에 Object 추가
        roomData.Add(room1); // Room 1 추가

         // Room 1 데이터
        RoomData room2 = new RoomData
        {
            roomType = "NomalRoom",
            roomPosition = new Vector3Int(-20, -20, 0),
            roomSize = new int[] { 11, 11 },
            additionalTiles = new List<ObjectData>()
        };

        ObjectData object2 = new ObjectData
        {
            objectPosition = new Vector3Int(5, 5, 0),
            objectTiles = new List<string> {"btn01"}
        };

        room2.additionalTiles.Add(object2); // Room 1에 Object 추가
        roomData.Add(room2); // Room 1 추가
    }

    private void CreateRoomConnections(RoomData room)
    {
        // 방향 랜덤 선택 (0: 위, 1: 오른쪽, 2: 아래, 3: 왼쪽)
        int startDirection = Random.Range(0, 4);
        int endDirection;
        do
        {
            endDirection = Random.Range(0, 4); // 시작점과 다른 방향
        } while (endDirection == startDirection);

        // 방의 크기
        int startX = 0;
        int startY = 0;
        int endX = room.roomSize[0];
        int endY = room.roomSize[1];

        // 시작점과 끝점을 생성
        CreateConnections(room.roomPosition, startDirection, endDirection, startX, startY, endX, endY);

        // RoomData에 시작점과 끝점 정보를 저장
        room.startTile = connectionstartTile.name; // connectionstartTile은 SetConnectionTile 함수에서 설정
        room.startTilePosition = startPointPosition;
        room.endTile = connectionendTile.name; // connectionendTile은 SetConnectionTile 함수에서 설정
        room.endTilePosition = endPointPosition;
    }
    private void CreateConnections(Vector3Int roomPosition, int startDirection, int endDirection, int startX, int startY, int endX, int endY)
    {
        // 시작점 설정
        SetConnectionTile(roomPosition, startDirection, startX, startY, endX, endY, true);

        // 도착점 설정
        SetConnectionTile(roomPosition, endDirection, startX, startY, endX, endY, false);
    }

    private void SetConnectionTile(Vector3Int roomPosition, int direction, int startX, int startY, int endX, int endY, bool isStartPoint)
    {
        Vector3Int tilePosition = Vector3Int.zero;
        Tile connectionTile = null;

        // 방향에 따라 타일 위치와 종류 결정
        switch (direction)
        {
            case 0: // 위쪽
                tilePosition = roomPosition + new Vector3Int(((startX + endX) / 2), endY - 1, 0);
                connectionTile = isStartPoint ? connectionTiles[0] : connectionTiles[2]; // 03 또는 05
                break;
            case 1: // 오른쪽
                tilePosition = roomPosition + new Vector3Int(endX - 1, ((startY + endY) / 2), 0);
                connectionTile = isStartPoint ? connectionTiles[1] : connectionTiles[3]; // 04 또는 02
                break;
            case 2: // 아래쪽
                tilePosition = roomPosition + new Vector3Int(((startX + endX) / 2), startY, 0);
                connectionTile = isStartPoint ? connectionTiles[2] : connectionTiles[0]; // 05 또는 03
                break;
            case 3: // 왼쪽
                tilePosition = roomPosition + new Vector3Int(startX, ((startY + endY) / 2), 0);
                connectionTile = isStartPoint ? connectionTiles[3] : connectionTiles[1]; // 02 또는 04
                break;
        }
        if (isStartPoint)
        {
            startPointPosition = tilePosition;
            connectionstartTile = connectionTile;
        }
        else
        {
            endPointPosition = tilePosition;
            connectionendTile = connectionTile;
        }

    }



    public void CreateCorridors(List<RoomData> roomDataList)
    {
        // 방의 영역을 계산하여 roomAreas에 추가
        foreach (var room in roomDataList)
        {
            AddRoomArea(room);
        }

        // 방의 시작점과 도착점 보정
        Dictionary<int, Vector3Int> correctedStartPoints = new Dictionary<int, Vector3Int>();
        Dictionary<int, Vector3Int> correctedEndPoints = new Dictionary<int, Vector3Int>();

        for (int i = 0; i < roomDataList.Count; i++)
        {
            var room = roomDataList[i];

            // 시작점 보정
            correctedStartPoints[i] = GetStartCorrectedPosition(room.startTile, room.startTilePosition);

            // 도착점 보정
            correctedEndPoints[i] = GetEndCorrectedPosition(room.endTile, room.endTilePosition);
        }

        // 복도 연결
        HashSet<string> visitedConnections = new HashSet<string>(); // 중복 제거용

        for (int i = 0; i < roomDataList.Count ; i++)
        {
            int randomRoomIndex = Random.Range(0, roomDataList.Count); // 랜덤으로 방 선택
            while (randomRoomIndex == i || visitedConnections.Contains($"{i}-{randomRoomIndex}"))
            {
                randomRoomIndex = Random.Range(0, roomDataList.Count);
            }

            // 연결된 시작점과 도착점을 복도로 추가
            Vector3Int startPoint = correctedStartPoints[i];
            Vector3Int endPoint = correctedEndPoints[randomRoomIndex];

            List<Vector3Int> corridor = GenerateCorridor(startPoint, endPoint);

            // 복도를 최종 배열에 추가 (중복 제거)
            foreach (var point in corridor)
            {
                if (!allCorridorPoints.Contains(point))
                {
                    allCorridorPoints.Add(point);
                }
            }

            visitedConnections.Add($"{i}-{randomRoomIndex}");
        }

    }

    private Vector3Int GetStartCorrectedPosition(string tileType, Vector3Int originalPosition)
    {
        switch (tileType)
        {
            case "base02":
                return originalPosition + new Vector3Int(1, 1, 0);
            case "base03":
                return originalPosition + new Vector3Int(1, 1, 0);
            case "base04":
                return originalPosition + new Vector3Int(0, 0, 0);
            case "base05":
                return originalPosition + new Vector3Int(-1, 1, 0);
            default:
                return originalPosition; // 기본값
        }
    }

    private Vector3Int GetEndCorrectedPosition(string tileType, Vector3Int originalPosition)
    {
        switch (tileType)
        {
            case "base02":
                return originalPosition + new Vector3Int(0, -1, 0);
            case "base03":
                return originalPosition + new Vector3Int(-1, 0, 0);
            case "base04":
                return originalPosition + new Vector3Int(0, 1, 0);
            case "base05":
                return originalPosition + new Vector3Int(1, 0, 0);
            default:
                return originalPosition; // 기본값
        }
    }

    private void AddRoomArea(RoomData room)
    {
        Vector3Int roomPosition = room.roomPosition;
        int width = room.roomSize[0]; //혹시 모르니 보정
        int height = room.roomSize[1]; 

        // 방의 모든 좌표를 roomAreas에 추가
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                roomAreas.Add(roomPosition + new Vector3Int(x, y, 0));
            }
        }
    }

    private List<Vector3Int> GenerateCorridor(Vector3Int startPoint, Vector3Int endPoint)
    {
        List<Vector3Int> corridorPoints = new List<Vector3Int>();
        HashSet<Vector3Int> closedSet = new HashSet<Vector3Int>(); // 방문한 노드
        PriorityQueue<Vector3Int, float> openSet = new PriorityQueue<Vector3Int, float>(); // 탐색 노드

        Dictionary<Vector3Int, Vector3Int> cameFrom = new Dictionary<Vector3Int, Vector3Int>(); // 경로 추적
        Dictionary<Vector3Int, float> gScore = new Dictionary<Vector3Int, float>(); // 시작점에서 특정 노드까지의 비용
        Dictionary<Vector3Int, float> fScore = new Dictionary<Vector3Int, float>(); // 예측 비용 (gScore + 휴리스틱)

        gScore[startPoint] = 0;
        fScore[startPoint] = HeuristicCost(startPoint, endPoint);
        openSet.Enqueue(startPoint, fScore[startPoint]);

        while (openSet.Count > 0)
        {
            Vector3Int current = openSet.Dequeue();

            if (current == endPoint)
            {
                // 경로 재구성
                while (cameFrom.ContainsKey(current))
                {
                    corridorPoints.Add(current);
                    current = cameFrom[current];
                }
                corridorPoints.Reverse();
                return corridorPoints;
            }

            closedSet.Add(current);

            foreach (Vector3Int neighbor in GetNeighbors(current))
            {
                if (closedSet.Contains(neighbor) || roomAreas.Contains(neighbor)) continue;

                float tentativeGScore = gScore[current] + 1; // 현재 비용 + 이동 비용
                if (!gScore.ContainsKey(neighbor) || tentativeGScore < gScore[neighbor])
                {
                    cameFrom[neighbor] = current;
                    gScore[neighbor] = tentativeGScore;
                    fScore[neighbor] = tentativeGScore + HeuristicCost(neighbor, endPoint);

                    if (!openSet.Contains(neighbor))
                    {
                        openSet.Enqueue(neighbor, fScore[neighbor]);
                    }
                }
            }
        }

        return corridorPoints; // 실패 시 빈 리스트 반환
    }

    private float HeuristicCost(Vector3Int a, Vector3Int b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y); // 맨해튼 거리
    }

    private List<Vector3Int> GetNeighbors(Vector3Int position)
    {
        List<Vector3Int> neighbors = new List<Vector3Int>
        {
            position + Vector3Int.up,
            position + Vector3Int.down,
            position + Vector3Int.left,
            position + Vector3Int.right
        };

        return neighbors;
    }
}





[System.Serializable]
public class RoomData
{
    public string roomType; // 방 타입
    public Vector3Int roomPosition; // 방 위치
    public int[] roomSize; // 방 크기
    public string startTile; // 시작 타일 타입
    public Vector3Int startTilePosition; // 시작 타일 위치
    public string endTile; // 도착 타일 타입
    public Vector3Int endTilePosition; // 도착 타일 위치
    public List<ObjectData> additionalTiles; // 방 안의 추가 타일들
}

// 방 안의 추가 객체 데이터 클래스
[System.Serializable]
public class ObjectData
{
    public Vector3Int objectPosition; // 객체의 위치
    public List<string> objectTiles; // 객체에 포함된 타일들
}