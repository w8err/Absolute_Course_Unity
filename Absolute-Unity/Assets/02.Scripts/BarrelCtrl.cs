// https://velog.io/@gkswh4860/%EB%B9%84%ED%8A%B8-%EC%97%B0%EC%82%B0%EC%9D%84-%EC%9D%B4%EC%9A%A9%ED%95%B4-Unity-Layer-%EC%82%AC%EC%9A%A9%EB%B2%95%EC%97%90-%EB%8C%80%ED%95%B4-%EC%95%8C%EC%95%84%EB%B3%B4%EC%9E%90 모르겠으면 참고. Physics.OverlapSphere 검출 대상 레이어 탐색 방법

using UnityEngine;

[RequireComponent(typeof(AudioSource))] // AudioSource가 삭제되는 것을 방지하기 위한 어트리뷰트

public class BarrelCtrl : MonoBehaviour
{

    // 폭발 효과 파티클을 연결할 변수
    public GameObject expEffect;
    // 무작위로 적용할 텍스처 배열
    public Texture[] textures;
    // 폭발 반경
    public float radius = 10.0f;
    // 하위에 있는 Mesh Renderer 컴포넌트를 저장할 변수
    private new MeshRenderer renderer;

    // 컴포넌트를 지정할 변수
    private Transform tr;
    private Rigidbody rb;
    // 총알 맞은 횟수를 누적시킬 변수
    private int hitCount = 0;

    // 폭발소리에 사용할 오디오 음원
    public AudioClip expSfx;
    // AudioSource 컴포넌트를 저장할 변수
    private new AudioSource audio;

    void Awake() {
        
        tr = GetComponent<Transform>();
        rb = GetComponent<Rigidbody>();
        // 하위에 있는 MeshRenderer 컴포넌트를 추출
        renderer = GetComponentInChildren<MeshRenderer>();
        // 오디오소스 컴포넌트 불러오기
        audio = GetComponent<AudioSource>();

        // 난수 발생
        int idx = Random.Range(0, textures.Length);
        // 텍스처 지정
        renderer.material.mainTexture = textures[idx];
    }

    // 충돌 시 발생하는 콜백 함수
    void OnCollisionEnter(Collision coll) 
    {
        if (coll.collider.CompareTag("BULLET")) {
            hitCount++;
            // 총알 맞은 횟수를 증가시키고 3회 이상이면 폭발 처리
            if (hitCount == 3) 
            {
                ExpBarrel();
            }
        }
    }

    // 드럼통을 폭발시킬 함수, 연쇄폭발 함수
    void ExpBarrel() {

        // 폭발 효과 파티클 생성
        GameObject exp = Instantiate(expEffect, tr.position, Quaternion.identity);
        // 폭발 효과 파티클 5초 후 제거
        Destroy(exp, 4.5f);

        //  Rigidbody 컴포넌트의 mass를 1.0으로 수정해 무게를 가볍게 함
        // rb.mass = 1.0f;
        //  위로 솟구치는 힘을 가함
        // rb.AddForce(Vector3.up * 1500.0f);

        // 간접 폭발력 전달
        IndirectDamage(tr.position);

        // 3초 후 드럼통 제거
        Destroy(gameObject, 3.0f);

        // 폭발소리 발생
        audio.PlayOneShot(expSfx, 4.0f);
    }

    // 연쇄폭발이 없는 함수
//     void secondExpBarrel() {
//     GameObject exp = Instantiate(expEffect, rb.position, Quaternion.identity);
//     Destroy(exp, 4.5f);
//     rb.mass = 1.0f;
//     rb.AddForce(Vector3.up * 1500.0f);
//     Destroy(gameObject, 3.0f);
//     audio.PlayOneShot(expSfx, 4.0f);

//   }

    // 폭발력을 주변에 전달하는 함수
    void IndirectDamage(Vector3 pos) {

        Collider[] colls = Physics.OverlapSphere(pos, radius, 1 << 3); // --> 가비지 컬렉션 발생.. 하지만.. 이게 오류가 안떠..
    //                                         ?원점, 반지름, 검출 대상 레이어(3번째 레이어를 대상으로 한다는 뜻임. 

    if(colls != null) { 
    foreach(var coll in colls) // Colls 배열 안에 들어온 모든 드럼통들에게 하나씩(foreach) 적용
            {
            // 폭발 범위에 포함된 드럼통의 Rigidbody 컴포넌트 추출
            rb = coll.GetComponent<Rigidbody>();

            // 드럼통의 무게를 가볍게 함
            rb.mass = 1.0f;

            // freezeRotation 제한값을 해제
            rb.constraints = RigidbodyConstraints.None;

            // 폭발력을 전달
            rb.AddExplosionForce(1500.0f, pos, radius, 1200.0f);
        }
    }
}
}
