using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PlayerInteraction : MonoBehaviour
{
    public DungeonRoom dungeonRoom; // DungeonRoom 스크립트 참조
    public Tilemap objectTilemap;   // 버튼이 있는 오브젝트 타일맵
    public Tile btn01Tile;          // 버튼 타일
    public Transform playerTransform; // 플레이어 Transform
    private List<RoomData> roomDataList;


    // 여기서 폴의 getbyid로 오브젝트를 가져와서 트렌스 폴만 추출하면 가능하다.
    // 그리고 서버와 연동해서 상대쪽도 보여주면 됨.
    private void Update()
    {   
        
        // 플레이어의 현재 타일맵 좌표 가져오기
        Vector3 playerPosition = playerTransform.position;
        Vector3Int tilePosition = objectTilemap.WorldToCell(playerPosition);

        // 플레이어가 버튼 타일 위에 있는지 확인
        TileBase currentTile = objectTilemap.GetTile(tilePosition);

         if (currentTile == btn01Tile && Input.GetKeyDown(KeyCode.Space))
        {
            // 플레이어가 현재 있는 방 찾기
            RoomData currentRoom = FindCurrentRoom(tilePosition);
            if (currentRoom != null)
            {
                // HandleButtonPress 호출, 해당 방의 시작점 전달
                dungeonRoom.HandleButtonPress(currentRoom.startTilePosition, currentRoom.startTile);
            }
        }
    }

      public void SetRoomDataList(List<RoomData> roomData)
    {
        roomDataList = roomData;
    }

     private RoomData FindCurrentRoom(Vector3Int playerTilePosition)
    {
        foreach (var room in roomDataList)
        {
            Vector3Int roomPosition = room.roomPosition;
            int[] roomSize = room.roomSize;

            // 플레이어가 방 안에 있는지 확인
            if (playerTilePosition.x >= roomPosition.x &&
                playerTilePosition.x < roomPosition.x + roomSize[0] &&
                playerTilePosition.y >= roomPosition.y &&
                playerTilePosition.y < roomPosition.y + roomSize[1])
            {
                return room; // 플레이어가 현재 있는 방 반환
            }
        }

        return null; // 방을 찾지 못함
    }
}