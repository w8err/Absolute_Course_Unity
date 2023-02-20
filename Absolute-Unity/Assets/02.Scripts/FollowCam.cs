using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCam : MonoBehaviour
{
    // 따라가야 할 대상을 연결할 변수
    public Transform targetTr;
    
    // 메인 카메라 자신의 Transform 컴포넌트
    private Transform camTr;

    // 따라갈 대상으로부터 떨어질 거리
    [Range(2.0f, 20.0f)] public float distance = 10.0f;
    // [Range(min, max)] Attribute => 다음 라인에 선언한 변수의 입력 범위를 min~max로 제한시킴, 인스펙터 뷰에 슬라이드바 표시됨

    // Y축으로 이동할 높이
    [Range(0.0f, 10.0f)] public float height = 2.0f;

    // 반응 속도
    public float damping = 10.0f;

    // 카메라 LookAt의 Offset 값
    public float targetOffset = 2.0f;

    // Velocity에서 사용할 변수
    private Vector3 velocity = Vector3.zero;
    void Start() {
        // 메인 카메라 자신의 Transform 컴포넌트 추출
        camTr = GetComponent<Transform>();
    }

    // LateUpdate 함수는 모든 Update 함수가 실행되고 난 후에 호출되는 함수다.
    // 주인공 캐릭터 Update 함수에서 이동 로직을 완료한 후, 이동한 좌표로 카메라를 이동시키기 위해 LateUpdate 함수를 사용했다.
    // 전부 Update에서 처리할 수도 있지만, 호출 순서가 보장된 게 아니기 때문에 카메라 떨림이 발생할 수 있다.
    void LateUpdate() {

        // 추적해야 할 대상의 뒤쪽으로 distance만큼 이동
        // 높이를 height만큼 이동
        Vector3 pos = targetTr.position + (-targetTr.forward * distance) + (Vector3.up * height);

        // 구면 선형 보간 함수를 이용해 부드럽게 위치 변경
        // camTr.position = Vector3.Slerp(camTr.position,              // 시작 위치
        //                                 pos,                        // 목표 위치
        //                                 Time.deltaTime * damping);  // 시간 t

        // SmmothDamp를 이용한 위치 보간
        camTr.position = Vector3.SmoothDamp(camTr.position,
                                            pos,
                                            ref velocity,
                                            damping);

        // 카메라를 피벗 좌표를 향해 회전
        camTr.LookAt(targetTr.position + (targetTr.up * targetOffset));
    }

}
