using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;
using System.Collections.Generic;


public class DungeonRoom : MonoBehaviour
{
    public Tilemap floorTilemap;  // Tilemap 연결
    public Tilemap backwallTilemap;
    public Tilemap Collision;
    public Tilemap objectTilemap;
    public Tile baseTile;    // base00 타일
    public Tile wallTile;    // cube00 타일
    public Tile CollisionTile;
    public Tile[] connectionTiles; // 연결 타일들 (02, 03, 04, 05)

    public Tile btn01Tile;

    private Tile connectionstartTile;

    public void CreateStartRoom(RoomData room)
    {
        Vector3Int roomPosition = room.roomPosition;
        int[] roomSize = room.roomSize;

        // 방 크기 확인 (각 축이 5 이상이어야 함)
        if (roomSize[0] < 5) roomSize[0] = 5;
        if (roomSize[1] < 5) roomSize[1] = 5;

        // 중앙 크기 설정 (3x3)
        int baseSizeX = roomSize[0] - 2;
        int baseSizeY = roomSize[1] - 2;

        // 1. 중앙을 base00으로 채우기
        for (int x = 0; x < roomSize[0]; x++)  // x는 0부터 roomSize[0]까지
        {
            for (int y = 0; y < roomSize[1]; y++)  // y는 0부터 roomSize[1]까지
            {
                // x나 y가 외곽에 있을 경우 바닥 타일을 설정하지 않음
                if (x == 0 || y == 0 || x == roomSize[0] - 1 || y == roomSize[1] - 1)
                {
                    continue;  // 외곽 부분은 건너뛰기
                }

                Vector3Int tilePosition = roomPosition + new Vector3Int(x, y, 0);
                floorTilemap.SetTile(tilePosition, baseTile);  // 내부만 바닥 타일로 설정
            }
        }

        // 2. 외곽을 wallTile로 채우기
        for (int x = 0; x < roomSize[0] + 1; x++)
        {
            for (int y = 0; y < roomSize[1] + 1; y++)
            {
                // 바닥을 제외한 외곽 부분만 큐브 타일을 설치
                if ((x >= (((roomSize[0] + 1) / 2) + 1) && x <= roomSize[0] && y == roomSize[1]) ||
                (y >= (((roomSize[1] + 1) / 2) + 1) && y <= roomSize[1] && x == roomSize[0]))
                {
                    Vector3Int tilePosition = roomPosition + new Vector3Int(x, y, 0);
                    Vector3Int CollisiontilePosition = roomPosition + new Vector3Int(x - 1, y - 1, 0);
                    //backwallTilemap.SetTile(tilePosition, wallTile);
                    Collision.SetTile(CollisiontilePosition, CollisionTile);
                }
            }
        }

        // 3. 복도의 시작점과 도착점 생성
        Tile startTile = GetTileByName(room.startTile); // Convert the string to a Tile
        Tile endTile = GetTileByName(room.endTile);     // Convert the string to a Tile

        floorTilemap.SetTile(room.startTilePosition, startTile);
        floorTilemap.SetTile(room.endTilePosition, endTile);


        // 4. 오브젝트 데이터 활용
        CreateroomObject(roomPosition, room.additionalTiles);

    }

    private void CreateroomObject(Vector3Int roomPosition, List<ObjectData> roomObjects)
    {
        // RoomData 내 추가 오브젝트 데이터를 순회
        foreach (var objectData in roomObjects)
        {
            foreach (var tileName in objectData.objectTiles)
            {
                Tile objectTile = GetTileByName(tileName); // 타일 이름으로 타일 객체 가져오기
                if (objectTile != null)
                {
                    // 타일을 해당 위치에 설치
                    Vector3Int tilePosition = roomPosition + objectData.objectPosition;
                    objectTilemap.SetTile(tilePosition, objectTile);
                }
                else
                {
                    Debug.LogWarning($"타일 '{tileName}'이(가) 정의되어 있지 않습니다!");
                }
            }
        }
    }

    private Tile GetTileByName(string tileName)
    {
        switch (tileName)
        {
            case "btn01":
                return btn01Tile;
            case "base02":
                return connectionTiles[0];
            case "base03":
                return connectionTiles[1];
            case "base04":
                return connectionTiles[2];
            case "base05":
                return connectionTiles[3];
            // 필요 시 다른 타일도 여기에 추가
            default:
                return null;
        }
    }


    public void HandleButtonPress(Vector3Int startPointPosition, string connectionstartTile) //이거 작동시키면 타일 바뀜. 굳
    {
        // 1. 시작점 타일을 지우고 Collision 타일로 변경
        floorTilemap.SetTile(startPointPosition, null);  // 시작점 타일 삭제
        Collision.SetTile(startPointPosition, CollisionTile);  // 충돌 타일로 변경

        // 2. 3초 후에 원래 상태로 복원
        StartCoroutine(RestoreStartPoint(startPointPosition, connectionstartTile));
    }

    private IEnumerator RestoreStartPoint(Vector3Int startPointPosition, string tileName)
    {
        yield return new WaitForSeconds(3);
        
        Tile connectionstartTile = GetTileByName(tileName);
        // Collision 타일을 제거하고 원래 시작점 타일 복원
        Collision.SetTile(startPointPosition, null);
        floorTilemap.SetTile(startPointPosition, connectionstartTile);  // 원래 시작점 타일로 복원
    }

    public void CreateStartRoom(List<Vector3Int> corridorPoints)
    {
         foreach (var tilePosition in corridorPoints)
        {
            floorTilemap.SetTile(tilePosition, baseTile); // 해당 좌표에 타일 배치
        }
    }

}