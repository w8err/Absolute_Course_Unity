using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;      // unity-ui를 사용하기 위해 선언한 네임스페이스
using UnityEngine.Events;  // unityevent 관련된 api를 사용하기 위해 선언한 네임스페이스

/*  특정 이벤트가 발생하면 호출할 함수를 연결하기 위해 AddListener(UnityAction call) 함수를 사용한다.
    Button에는 onClick이라는 버튼 클릭 이벤트가 정의되어 있음. AddListener를 사용해 함수를 연결하는 방법은
    UnityAction을 사용하거나 델리게이트를 사용하는 것. 또한 델리게이트를 람다식으로 표현할 수 있다.

    델리게이트_타입 변수명 = (매개변수_1, 매개변수2_, ...) => 식;
    델리게이트_타입 변수명 = (매개변수_1, 매개변수2_, ...) => {로직_1; 로직_2; ...};

*/

public class UIManager : MonoBehaviour
{
    public Button startButton;
    public Button optionButton;
    public Button shopButton;

    private UnityAction action;

    void Start() 
    {
        // UnityAction을 사용한 이벤트 연결 방식
        action = () => OnButtonClick(startButton.name);
        startButton.onClick.AddListener(action);

        // 무명 메서드를 활용한 이벤트 연결 방식
        optionButton.onClick.AddListener(delegate {OnButtonClick(optionButton.name);});

        // 람다식을 활용한 이벤트 연결 방식
        shopButton.onClick.AddListener(()=> OnButtonClick(shopButton.name));
    }

    public void OnButtonClick(string msg) 
    {
        Debug.Log($"Click Button : {msg}");
    }
}
