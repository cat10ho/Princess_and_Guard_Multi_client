using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Cinemachine; //카메라 함수.

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public bool isLive;
    public int targetFrameRate;
    public string version = "1.0.0";
    public int latency = 2;
    public string deviceId;

    public PoolManager pool;
    public PlayerManager PlayerManager;
    public GameObject GameStartUI;
    public GameObject GameroomUI;
    public GameObject DungeonManager;

    private List<JoinRoom.PlayerData> players;
    private Dictionary<string, string> playerRoles;
    private HashSet<string> currentUsers = new HashSet<string>();

    public CinemachineVirtualCamera virtualCamera; //카메라 참조.
  void Awake() {
        instance = this;
        Application.targetFrameRate = targetFrameRate;
    }
    public void SetPlayerList(List<JoinRoom.PlayerData> newPlayers)
    {
        players = newPlayers;
    }

    public List<JoinRoom.PlayerData> GetPlayerList()
    {
        return players;
    }

    public void SetPlayerRole(string deviceId, string role)
    {
        if (playerRoles.ContainsKey(deviceId))
        {
            playerRoles[deviceId] = role;
        }
        else
        {
            playerRoles.Add(deviceId, role);
        }
    }

     public Dictionary<string, string> GetAllPlayerRoles()
    {
        return new Dictionary<string, string>(playerRoles);
    }

    public void GameStart(Start data) {
        Debug.Log("게임 시작");
        isLive = true;
        HashSet<string> newUsers = new HashSet<string>();
        foreach(Start.UserStartLocation user in data.users){
        newUsers.Add(user.id);
        GameObject player = pool.Assign(user);
        PlayerPrefab playerScript = player.GetComponent<PlayerPrefab>();
        playerScript.UpdatePosition(user.x, user.y); //여기는 위치를 밑은 속도를 따로 해야겠다. 이거 중요.
        }
        currentUsers = newUsers;
        GameroomUI.SetActive(false);
        GameObject controlplayer = pool.GetById(deviceId);
        virtualCamera.Follow = controlplayer.transform;
        DungeonManager.SetActive(true);
    }

     public void Spawn(LocationUpdate data) {
        if (!isLive) {
            return;
        }
        
        HashSet<string> newUsers = new HashSet<string>(); //새로 만들기.
        foreach(LocationUpdate.UserLocation user in data.users) {
            newUsers.Add(user.id); //뉴 유저라고 하지만. 그냥 처음부터 다시 넣어주는거.

            GameObject player = pool.Get(user);// 오브젝트를 받음.
            PlayerPrefab playerScript = player.GetComponent<PlayerPrefab>();
            playerScript.HandleMovement(user.x, user.y);// 위치이동.
        }

        foreach (string userId in currentUsers) { //여기서 나간. 사라진 유저 있는지. 확인하는듯.
            if (!newUsers.Contains(userId)) {
                pool.Remove(userId);
            }
        }
        
        currentUsers = newUsers;
    }

    public void UpdateCarryState(CarryUpdate data)
    {   
        GameObject princess = pool.GetById(data.princessId); // 공주 오브젝트 가져오기
        GameObject carrier = pool.GetById(data.carrierId);// 기사 오브젝트 가져오기

        if (carrier != null && data.isCarried)
        {
            carrier.GetComponent<PlayerPrefab>().PickUpPrincess(data.isCarried, princess); // 상태 업데이트
        } else if (carrier != null && !data.isCarried){
            carrier.GetComponent<PlayerPrefab>().DropPrincess(data.isCarried, princess);
        }

        if (princess != null && data.isCarried)
        {
            princess.GetComponent<PlayerPrefab>().SetCarriedState(data.isCarried, carrier); // 상태 업데이트
        } else if (princess != null && !data.isCarried) {
            princess.GetComponent<PlayerPrefab>().SetCarriedState(data.isCarried, null);
        }
    }
    
}
