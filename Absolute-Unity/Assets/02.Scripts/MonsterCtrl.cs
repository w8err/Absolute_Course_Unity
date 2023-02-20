using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 네비게이션 기능을 사용하기 위해 추가해야 하는 네임스페이스
using UnityEngine.AI;

public class MonsterCtrl : MonoBehaviour
{
    
    // 몬스터의 상태 정보
    public enum State { 
        IDLE,
        TRACE,
        ATTACK,
        DIE
    } 
    
    // 몬스터의 현재 상태
    public State state = State.IDLE;
    // 추적 사정거리
    public float traceDist = 10.0f;
    // 공격 사정거리
    public float attackDist = 2.0f;
    // 몬스터의 사망 여부
    public bool isDie = false;

    // 컴포넌트의 캐시를 처리할 변수
    private Transform monsterTr;
    private Transform playerTr;
    private NavMeshAgent agent;
    private Animator anim;

    // Animator 파라미터의 해시값 추출. 경제적인 측면에서 이롭다..... 근데 난 그냥 스트링으로 뽑을래.... 
    private readonly int hashTrace = Animator.StringToHash("IsTrace");
    private readonly int hashAttack = Animator.StringToHash("IsAttack");
    private readonly int hashHit = Animator.StringToHash("Hit");
    private readonly int hashPlayerDie = Animator.StringToHash("PlayerDie");
    private readonly int hashSpeed = Animator.StringToHash("Speed");
    
    // 혈흔 효과 프리팹 변수 선언
    private GameObject bloodEffect;

    void Start() {

        // 몬스터의 Transform 할당
        monsterTr = GetComponent<Transform>();

        // 추적 대상인 Player의 Transform 할당
        playerTr = GameObject.FindWithTag("PLAYER").GetComponent<Transform>();

        // NavMeshAgent 컴포넌트 할당
        agent = GetComponent<NavMeshAgent>();

        // 애니메이터 컴포넌트 할당
        anim = GetComponent<Animator>();

        // BloodSprayEffect 프리팹 Resources 폴더에서 Load해오기
        bloodEffect = Resources.Load<GameObject>("BloodSprayEffect");
        // 여러개를 불러올 때는 bloodEffect = Resources.LoadAll<T>() 함수를 사용해 로드한다.
        
        // 추적 대상의 위치를 설정하면 바로 추적 시작
        // agent.destination = playerTr.position;

        // 몬스터의 상태를 체크하는 코루틴 함수 호출
        StartCoroutine(CheckMonsterState());

        // 몬스터의 상태에 따라 액션을 취하게 해주는 함수 호출
        StartCoroutine(MonsterAction());
    }

    // 코루틴 함수. 몬스터의 상태를 0.3초씩 체크함.
    IEnumerator CheckMonsterState() {

        while(!isDie) {
            // 0.3초 동안 중지(대기)하는 동안 제어권을 메시지 루프에 양보
            yield return new WaitForSeconds(0.3f);

            // 몬스터와 주인공 캐릭터 사이의 거리 측정
            float distance = Vector3.Distance(playerTr.position, monsterTr.position);

            // 공격 사정거리로 들어왔는지 확인
            if(distance <= attackDist) 
            {
                state = State.ATTACK;
            }
            // 추적 사정거리 범위로 들어왔는지 확인
            else if(distance <= traceDist) 
            {
                state = State.TRACE;
            }
            else 
            {
                state = State.IDLE; 
            }
        }
    }

    // 코루틴 함수. 이제 몬스터가 움직이고, 공격하고, 죽는다. 
    IEnumerator MonsterAction() 
    {
        while (!isDie)
        {
            switch (state)
            {
                // IDLE 상태
                case State.IDLE:
                // 추적 중지
                    agent.isStopped = true;
                    // Animator의 isTrace 변수를 false로 설정
                    anim.SetBool(hashTrace, false);
                    break;
                
                // 추적 상태
                case State.TRACE:
                // 추적 대상의 좌표로 이동 시작
                    agent.SetDestination(playerTr.position);
                    agent.isStopped = false;

                    // Animator의 IsTrace 변수를 true로 설정
                    anim.SetBool(hashTrace, true);

                    // Animator의 IsAttack 변수를 false로 설정
                    anim.SetBool(hashAttack, false);
                    break;
                
                // 공격 상태
                case State.ATTACK:

                    // Animator의 IsAttack 변수를 true로 설정
                    anim.SetBool(hashAttack, true);
                break;

                case State.DIE:
                break;
            }
            yield return new WaitForSeconds(0.3f);
        }
    }

    // 콜라이더 관련 함수. 총알을 삭제하고 피격 리액션 애니메이션을 실행시켜줌
    void OnCollisionEnter(Collision coll)
    {
        if(coll.collider.CompareTag("BULLET"))
        {
            // 충돌한 총알 삭제
            Destroy(coll.gameObject);
            // 피격 리액션 애니메이션 실행
            anim.SetTrigger(hashHit);
            
            // 총알의 충돌 지점
            Vector3 pos = coll.GetContact(0).point;
            // 총알의 충돌 지점의 법선 벡터(90도 계산)
            Quaternion rot = Quaternion.LookRotation(-coll.GetContact(0).normal);
            // 혈흔 효과를 생성하는 함수 호출
            ShowBloodEffect(pos, rot);
        }
    }

    // 피튀기게 하는 함수
    void ShowBloodEffect(Vector3 pos, Quaternion rot) 
    {
        // 혈흔 효과 생성
        GameObject blood = Instantiate<GameObject>(bloodEffect, pos, rot, monsterTr);
        Destroy(blood, 1.0f);
    }

    // 플레이어 죽었을때 호출될 함수
    void OnPlayerDie()
    {
        // 몬스터의 상태를 체크하는 코루틴함수를 모두 정지시킴
        StopAllCoroutines();

        // 추적을 정지하고 애니메이션을 수행
        agent.isStopped = true;
        anim.SetFloat(hashSpeed, Random.Range(0.8f, 1.2f));
        anim.SetTrigger(hashPlayerDie);
    }

    // 스크립트가 활성화될 때마다 호출되는 함수
    void OnEnable()
    {
        // 이벤트 발생 시 수행할 함수 연결
        PlayerCtrl.OnPlayerDie += this.OnPlayerDie;
    }

    void OnDisable()
    {
        // 기존에 연결된 함수 해제
        PlayerCtrl.OnPlayerDie -= this.OnPlayerDie;
    }

    // 기즈모를 그려줌. 궤적 확인용
    void OnDrawGizmos()
    {
        // 추적 사정거리 표시
        if (state == State.TRACE)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, traceDist);
        }
        // 공격 사정거리 표시
        if (state == State.ATTACK)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackDist);
        }
    }
}


