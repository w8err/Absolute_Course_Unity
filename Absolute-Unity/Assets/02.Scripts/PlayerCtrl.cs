using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;   // UI에 접근하려면 네임스페이스가 반드시 필요하다. 

public class PlayerCtrl : MonoBehaviour
{
  // 컴포넌트를 캐시 처리할 변수
    [SerializeField] private Transform tr;
  // Animation 컴포넌트를 저장할 변수
    private Animation anim;

  // 이동속도 변수(public으로 선언되어 인스펙터 뷰에 노출됨)
  public float moveSpeed = 3.0f;
  // 회전 속도 변수
  public float turnSpeed = 80.0f;

  // 초기 생명 값
  private readonly float initHp = 100.0f;
  // 현재 생명 값
  private float currHp;
  // Hpbar 연결할 변수
  private Image hpBar;

  // 델리게이트 선언
  public delegate void PlayerDieHandler();
  // 이벤트 선언
  public static event PlayerDieHandler OnPlayerDie;

  // 달리기 모드 onoff
  [SerializeField] private bool runMode = false;

    IEnumerator Start()
    {
      // HP바 연결
      hpBar = GameObject.FindGameObjectWithTag("HP_BAR")?.GetComponent<Image>();
      // 초기 체력 초기화
      currHp = initHp;

        // Transform 컴포넌트를 추출해 변수에 대입
        tr = GetComponent<Transform>();
        anim = GetComponent<Animation>();

        // 애니메이션 실행
        anim.Play("Idle");

        turnSpeed = 0.0f;
        yield return new WaitForSeconds(0.3f);
        turnSpeed = 80.0f;
    }

    void Update()
      {
      if(Input.GetKey(KeyCode.LeftShift)) {
      runMode = true; moveSpeed = 7.0f;
      }
      else { 
      runMode = false; moveSpeed = 3.0f; 
      }

      float h = Input.GetAxis("Horizontal");  // -1.0f ~ 0.0f ~ + 1.0f
      float v = Input.GetAxis("Vertical");    // -1.0f ~ 0.0f ~ + 1.0f
      float r = Input.GetAxis("Mouse X");

    // 전후좌우 이동 방향 벡터 계산
    Vector3 moveDir = (Vector3.forward * v) + (Vector3.right * h);

    // Translate(이동 방향 * 속력 * Time.deltaTime)
    tr.Translate(moveDir.normalized * moveSpeed * Time.deltaTime);

    // Vector3.up 축을 기준으로 turnSpeed만큼의 속도로 회전
    tr.Rotate(Vector3.up * turnSpeed * Time.deltaTime * r);

    PlayerAnim(h, v, runMode);
    }

  void PlayerAnim(float h, float v, bool runMode) {

    if(runMode == false) {
      if (v >= 0.1f)
      {
        anim.CrossFade("WalkF", 0.5f); // 전진 애니메이션 실행
                                       // ("변경할 애니메이션", 다른 애니메이션 클립으로 페이드아웃 되는 시간)
      }
      else if (v <= -0.1f)
      {
        anim.CrossFade("WalkB", 0.5f); // 후진 애니메이션 실행
      }
      else if (h >= 0.1f)
      {
        anim.CrossFade("WalkR", 0.5f); // 오른쪽 이동 애니메이션 실행
      }
      else if (h <= -0.1f)
      {
        anim.CrossFade("WalkL", 0.5f); // 왼쪽 이동 애니메이션 실행
      }
      else
      {
        anim.CrossFade("Idle", 0.35f); // 정지 시 Idle 애니메이션 실행
      }
    }
    else if(runMode == true) {
      if (v >= 0.1f)
      {
        anim.CrossFade("RunF", 0.35f); 
      }
      else if (v <= -0.1f)
      {
        anim.CrossFade("RunB", 0.35f); 
      }
      else if (h >= 0.1f)
      {
        anim.CrossFade("RunR", 0.35f); 
      }
      else if (h <= -0.1f)
      {
        anim.CrossFade("RunL", 0.35f);
      }
      else
      {
        anim.CrossFade("Idle", 0.35f);
      }
    }
    else {}
    }

    // 충돌한 Collider의 IsTrigger 옵션이 체크됐을 때 발생
    void OnTriggerEnter(Collider coll) 
    {
      // 충돌한 Collider가 몬스터의 PUNCH이면 Player의 HP 차감
      if(currHp >= 0.0f && coll.CompareTag("PUNCH"))
      {
        currHp -= 10.0f;
        DisplayHealth();

        Debug.Log($"Player hp = {currHp/initHp}");
        //Debug.Log($"Player hp : {currhp}/{initHp}={currHp/initHp}");

        // Player의 생명이 0 이하이면 사망 처리
        if (currHp <= 0.0f)
        {
          PlayerDie();
        }
      }
    }

    void PlayerDie()
    {
      Debug.Log("플레이어 죽음!");

    //   // 게임 오브젝트 중 MONSTER 태그를 가진 애들을 전부 모아서 배열에 담을 것이다.
    //   GameObject[] monsters = GameObject.FindGameObjectsWithTag("MONSTER");

    //   // 모아온 애들한테 하나씩 적용
    //   foreach(GameObject monster in monsters) 
    //   {
    //     monster.SendMessage("OnPlayerDie", SendMessageOptions.DontRequireReceiver);
    
    // 주인공 사망 이벤트 호출(발생)
    OnPlayerDie();

    //GameManager 스크립트의 IsGameOver 프로퍼티 값을 변경
    GameObject.Find("GameMgr").GetComponent<GameManager>().IsGameOver = true;

      }

      void DisplayHealth()
      {
        hpBar.fillAmount = currHp/initHp;
      }
    }
