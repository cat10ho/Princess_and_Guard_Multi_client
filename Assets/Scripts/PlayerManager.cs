using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq; // 추가: Count 메서드를 사용하기 위해 필요

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager instance;
    [SerializeField] private Transform playerListParent;
    [SerializeField] private GameObject playerItemPrefab;
    // Start is called before the first frame update
    public GameObject GameStartUI;
    public GameObject GamelobbyUI;
    public GameObject CreatroomUI;
    public GameObject GameroomUI;

    public string selectedrole ="None" ;
    private Coroutine updateCoroutine;

       void Awake() {
        instance = this;
    }
    

  public void UpdatePlayerList(JoinRoom data)
{
    // 기존 UI 초기화
    foreach (Transform child in playerListParent)
    {
        Destroy(child.gameObject);
    }
    GameManager.instance.SetPlayerList(data.players);

    string currentDeviceId = GameManager.instance.deviceId;
    RoomManager.instance.selectedRoom = string.IsNullOrEmpty(data.roomName)? "None":data.roomName;
    List<JoinRoom.PlayerData> players = data.players;

    // 플레이어 리스트 UI 생성
    foreach (var player in players)
    {
        GameObject playerItem = Instantiate(playerItemPrefab, playerListParent);
        playerItem.GetComponentInChildren<Text>().text = player.deviceId;

        Dropdown roleDropdown = playerItem.GetComponentInChildren<Dropdown>();
        roleDropdown.ClearOptions();
        roleDropdown.AddOptions(new List<string> { "None", "Knight", "Princess" });

        // 선택된 직업 동기화
        roleDropdown.value = player.role == "Knight" ? 1 : player.role == "Princess" ? 2 : 0;
        
        if (player.deviceId == currentDeviceId)
            {
                roleDropdown.onValueChanged.AddListener((value) => OnRoleSelected(player, value));
                roleDropdown.interactable = true; // 드롭다운 활성화
            }
            else
            {
                roleDropdown.interactable = false; // 드롭다운 비활성화
            }
    }
    
        GamelobbyUI.SetActive(false);
        CreatroomUI.SetActive(false);
        GameroomUI.SetActive(true);

         if (updateCoroutine == null)
        {
            updateCoroutine = StartCoroutine(SendJoinRoomUpdates());
        }
}

void OnRoleSelected(JoinRoom.PlayerData player, int value)
{
    string[] roles = { "None", "Knight", "Princess" };
    player.role = roles[value];
    selectedrole = player.role;
    Debug.Log($"Role selected: {selectedrole} for player {player.deviceId}");
    NetworkManager.instance.SendChangeRolePayloadPacket();
    Debug.Log("Change role payload sent.");
}

 IEnumerator SendJoinRoomUpdates()
    {
        while (GameroomUI.activeSelf) // GameroomUI가 활성화된 동안
        {
            NetworkManager.instance.SendRoomDataPayloadPacket(); // 패킷 전송
            yield return new WaitForSeconds(5f); // 5초 대기
        }

        updateCoroutine = null; // 코루틴 종료 시 null로 초기화
    }

void ValidateRoleSelection(List<JoinRoom.PlayerData> players)
{
     int knightCount = players.Count(p => p.role == "Knight");
        int princessCount = players.Count(p => p.role == "Princess");
        int totalPlayers = players.Count;

        // 1. 최소 기사 1명
        if (knightCount < 1)
        {
            Debug.LogError("Error: At least one Knight is required!");
            return;
        }

        // 2. 유저가 2명 이상이라면 공주 1명, 기사 1명 조건 체크
        if (totalPlayers >= 2)
        {
            if (knightCount < 1 || princessCount < 1)
            {
                Debug.LogError("Error: At least one Knight and one Princess are required for more than 2 players!");
                return;
            }
        }

        Debug.Log("Role selection is valid.");
}

public void OnReadyButtonPressed() //여기서 레디를 눌렀을때. 값이 맞는걸 확인한 뒤. 넣어주기.
{

        List<JoinRoom.PlayerData> players = GameManager.instance.GetPlayerList();

        // 직업 선택 검사
        ValidateRoleSelection(players);

        // 유효성 검사를 통과하지 못하면 반환
        if (players.Count(p => p.role == "Knight") < 1 || (players.Count >= 2 && players.Count(p => p.role == "Princess") < 1))
        {
            Debug.LogError("Invalid role configuration! Please adjust roles.");
            return;
        }

        // 유효성 검사를 통과하면 게임 시작
        Debug.Log("Ready button pressed. Starting the game...");
        NetworkManager.instance.SendGameReadyPayloadPacket();

}
}
