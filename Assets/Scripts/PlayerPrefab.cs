using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerPrefab : MonoBehaviour
{
    private Rigidbody2D rb;
    [SerializeField] private float speed = 3f;

    private string role;
    private string id;

    private SpriteRenderer spriter;

    [SerializeField] private Sprite knightSprite; // 기사 스프라이트
    [SerializeField] private Sprite princessSprite; // 공주 스프라이트

    private bool isCarryingObject = false; // 기사가 공주를 들고 있는지 여부
    private GameObject carriedObject = null;

    private bool isCarried = false; // 공주가 들려 있는 상태
    private GameObject carrier = null; // 공주를 들고 있는 기사

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriter = GetComponent<SpriteRenderer>();
    }
    
    public void Init(string role, string id)
    {
        this.role = role;
        this.id = id;
        

        // 스프라이트 설정
        if (role == "Knight")
        {
            spriter.sprite = knightSprite;
            gameObject.tag = "Knight";
        }
        else if (role == "Princess")
        {
            spriter.sprite = princessSprite;
            gameObject.tag = "Princess";
        }
    }

        void Start()
    {    
        if (this.role == "Knight")
        {
            speed = 3f; // 기사 속도
        }
        else if (this.role == "Princess")
        {
            speed = 1.5f; // 공주 속도
        }
    }

    
     public void UpdatePosition(float x, float y)
    {
      transform.position = new Vector3(x, y);
    }



    void Update()
    {
        // 이동 처리
        if( id == GameManager.instance.deviceId && !isCarried )
        {
            HandleMovesend();

            if (Input.GetKeyDown(KeyCode.E) && role == "Knight") // E키로 공주 들기/내려놓기
        {
            if (isCarryingObject)
            {
                TryDropPrincess();
            }
            else
            {
                TryPickUpPrincess();
            }
        }
        }

         if (isCarried && carrier != null)
        {
            FollowCarrier();
        }
        
    }

    private void HandleMovesend()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveY = Input.GetAxis("Vertical");
        NetworkManager.instance.SendLocationUpdatePacket(moveX, moveY);
    }

     public void HandleMovement(float x, float y)
    {
        
        Vector2 movement = new Vector2(x, y);

          if (movement.magnitude > 1)
        {
            movement.Normalize();
        }

        rb.velocity = movement* speed;

          if (x != 0)
        {
            transform.localScale = new Vector3(Mathf.Sign(x), 1, 1);
        }
    }

     public void TryPickUpPrincess()
    {
        // 반경 1f 탐지하여 공주 확인
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 1f);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Princess")) // "Princess" 태그 확인
            {
                carriedObject = hit.gameObject;
                string princessId = carriedObject.GetComponent<PlayerPrefab>().GetId();
                //carriedObject.GetComponent<PlayerPrefab>().SetCarriedState(true, gameObject); // 공주 상태 업데이트
                NetworkManager.instance.SendCarryUpdatePayloadPacket(id, princessId, true); // 서버로 상태 전송
                Debug.Log("Princess picked up!");
                return;
            }
        }
    }

    public void PickUpPrincess(bool CarryingObject, GameObject carrierObj)
    {
        isCarryingObject = CarryingObject;
        carriedObject = carrierObj;
    }

    public void TryDropPrincess()
    {
        if (isCarryingObject && carriedObject != null)
        {
            isCarryingObject = false;
            //carriedObject.GetComponent<PlayerPrefab>().SetCarriedState(false, null); // 공주 상태 업데이트
            NetworkManager.instance.SendCarryUpdatePayloadPacket(id, carriedObject.GetComponent<PlayerPrefab>().GetId(), false);  // 서버로 상태 전송
            Debug.Log("Princess dropped!");
        }
    }


    public void DropPrincess(bool CarryingObject, GameObject carrierObj)
    {
        isCarryingObject = CarryingObject;
        carriedObject = null;
    }


 public void SetCarriedState(bool carried, GameObject carrierObj)
    {
        isCarried = carried;
        carrier = carrierObj;

        // 들려 있는 동안 스프라이트 투명도 조정
        spriter.color = isCarried ? new Color(1f, 1f, 1f, 0.7f) : Color.white;      
        // 공주가 들려 있는 동안 비활성화 이거 싹 비활성임;; 빼두기
        //enabled = !isCarried;
    }
    private void FollowCarrier()
    {
        // 기사의 위치에 맞춰 공주 위치 업데이트
        if (carrier != null)
        {
            Vector3 carrierPosition = carrier.transform.position;
            transform.position = new Vector3(carrierPosition.x, carrierPosition.y + 0.5f, carrierPosition.z); // 공주를 기사 위로 이동
        }
    }

    public string GetId()
{
    return id;
}
}
