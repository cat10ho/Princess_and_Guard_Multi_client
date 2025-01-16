using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolManager : MonoBehaviour
{
    // 프리펩을 보관할 변수
    public GameObject[] prefabs;

    // 생성된 오브젝트를 관리하는 리스트인듯?
    List<GameObject> pool;

     // 유저 ID를 키로, 그 유저가 사용할 오브젝트를 벨류로 가지는 리스트.
    Dictionary<string, GameObject> userDictionary = new Dictionary<string, GameObject>();

    void Awake() {
        pool = new List<GameObject>();
    }

    public GameObject Get(LocationUpdate.UserLocation data) {
        if (userDictionary.TryGetValue(data.id, out GameObject existingUser)) {
            return existingUser; //기존 사용중인 오브젝트 반환.
        }
        
        GameObject select = null; //없으면 일단 만들기.

        // ... 선택한 풀의 놀고 있는(비활성화) 게임 오브젝트 접근
        foreach (GameObject item in pool) {
            if (!item.activeSelf) {
                // 발견하면 select에 할당
                select = item;
                select.GetComponent<PlayerPrefab>().Init(data.role, data.id); //id는 유저 아이디. 플레이어 아이디는 스킨인듯.
                select.SetActive(true);
                userDictionary[data.id] = select; //할당해주기.
                break;
            }
        }
        // ... 못 찾으면
        if (select == null) {
            // 새롭게 생성하고 select 변수에 할당
            select = Instantiate(prefabs[0], transform);
            pool.Add(select);
            select.GetComponent<PlayerPrefab>().Init(data.role, data.id);
            userDictionary[data.id] = select;
            Debug.Log($"[PoolManager] Returned object position: {select.transform.position}");
        }

        return select; 
    }

   public GameObject Assign(Start.UserStartLocation data){

      GameObject select = null;

      foreach (GameObject item in pool) {
         if (!item.activeSelf) {
                // 발견하면 select에 할당
              select = item;
              select.GetComponent<PlayerPrefab>().Init(data.role, data.id); //id는 유저 아이디. 플레이어 아이디는 스킨인듯.
               select.SetActive(true);
               userDictionary[data.id] = select; //할당해주기.
               break;
           }
       }
        // ... 못 찾으면
      if (select == null) {
            // 새롭게 생성하고 select 변수에 할당
          select = Instantiate(prefabs[0], transform);
           pool.Add(select);
         select.GetComponent<PlayerPrefab>().Init(data.role, data.id);
          userDictionary[data.id] = select;
           Debug.Log($"[PoolManager] Returned object position: {select.transform.position}");
        }

        return select; 
     }


    

    public void Remove(string userId) {
        if (userDictionary.TryGetValue(userId, out GameObject userObject)) {
            Debug.Log($"Removing user: {userId}");
            userObject.SetActive(false);
            userDictionary.Remove(userId);
        } else {
            Debug.Log($"User {userId} not found in dictionary");
        }
    }

    public GameObject GetById(string id)
{
    userDictionary.TryGetValue(id, out GameObject userObject);
    return userObject; // 유저 오브젝트 반환
}
}
