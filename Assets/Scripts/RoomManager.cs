using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class RoomManager : MonoBehaviour
{
    public static RoomManager instance;
    [SerializeField] private Transform roomListParent;
    [SerializeField] private GameObject roomButtonPrefab;
    public string selectedRoom ="None";
    private GameObject selectedRoomButton;

    public GameObject GameStartUI;
    public GameObject GamelobbyUI;
    public GameObject GameroomUI;
    public InputField RoomNameInputField;

    void Awake() {
        instance = this;
    }

 public void DisplayRooms(JoinLobby data)
{
    // 기존 버튼 초기화
    foreach (Transform child in roomListParent)
    {
        Destroy(child.gameObject);
    }

    // 방 목록 불러오기
    List<JoinLobby.RoomData> rooms = data.rooms;

    // 방 버튼 생성
   foreach (var room in rooms)
        {
            GameObject roomButton = Instantiate(roomButtonPrefab, roomListParent);
            roomButton.GetComponentInChildren<Text>().text = $"{room.roomName} ({room.currentPlayers}/{room.maxPlayers})";

            // 선택 이벤트 연결
            roomButton.GetComponent<Button>().onClick.AddListener(() => SelectRoom(roomButton, room.roomName));
        }

    if ((GameStartUI.activeSelf || GameroomUI.activeSelf) && !GamelobbyUI.activeSelf)
    {   
        GameroomUI.SetActive(false);
        GameStartUI.SetActive(false);
        GamelobbyUI.SetActive(true);
        //뭔가 여기 방 나갈때, 크리에이터 룸에서 나올때도 꺼줘야 할듯?
    }
    }

public void OnJoinRoomButtonPressed()
{
    if (selectedRoom != null)
    {   
        NetworkManager.instance.SendJoinRoomPayloadPacket();
    }
    else
    {
        Debug.LogError("No room selected!");
    }
}



void SelectRoom(GameObject roomButton, string roomName)
{
    // 기존 선택된 방 버튼 초기화
    if (selectedRoomButton != null)
    {
        selectedRoomButton.GetComponent<Image>().color = Color.white; // 기본 색상으로 복원
    }

    // 새로 선택된 방 설정
    selectedRoomButton = roomButton;
    selectedRoom = roomName;

    // 선택된 버튼 강조 (색상 변경 예시)
    selectedRoomButton.GetComponent<Image>().color = Color.yellow;
}
}

