using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PushMoveCoroutine : CMyUnityBase
{
    //마찰 계수!
    public float m_fFrictionalCoefficient = 0.1f;
    private Coroutine m_PushMoveCoroutine;

    private Move m_Move;
    public Move Move
    {
        set { m_Move = value; }
    }
    private Info m_Info;
    public Info Info
    {
        set { m_Info = value; }
    }

    protected override void LateNullCheck()
    {
        base.LateNullCheck();

        if (m_Move == null)
        {
            KLog.Log(gameObject.name + "에 PushMoveCoroutine 스크립트 m_Move 설정되어 있지 않습니다.");
        }

        if (m_Info == null)
        {
            KLog.Log(gameObject.name + "에 PushMoveCoroutine 스크립트 m_Info 설정되어 있지 않습니다.");
        }
    }

    public void Set_PushMove(Vector3 _v3PushDir , float _fPushPower )
    {
        if (m_PushMoveCoroutine != null)
        {
            StopCoroutine(m_PushMoveCoroutine);
        }

        //움직임을 멈춘다
        m_Move.bMoveCC = true;

        m_PushMoveCoroutine = StartCoroutine(PushMoveObject(_v3PushDir, _fPushPower));
    }

    private IEnumerator PushMoveObject(Vector3 _v3PushDir, float _fPushPower)
    {
        float fWeight = m_Info.m_fWeight;

        //(최대)정지 마찰력  = 정지 마찰 계수 * 무게
        //정지 마찰 계수는 마찰 계수 *2로 임의로 하자!
        float fStopFrictionalForce = m_fFrictionalCoefficient * 2f * fWeight;
        //f=ma
        //_fPushPower = fWeight * 가속도
        //가속도 = _fPushPower / fWeight
        //정지 마찰력을 빼야 함
        float fStartSpeed = (_fPushPower - fStopFrictionalForce) / fWeight;

        while (fStartSpeed > 0f)
        {
            m_Move.MoveFunc(_v3PushDir, fStartSpeed);
            yield return null;
            //운동 마찰력 =−µNv^
            //대충 f=μN / μ를 0.1로 두자
            fStartSpeed -= (fWeight * m_fFrictionalCoefficient * Time.deltaTime * m_Move.m_Speed);
        }

        //이동 제한 해제
        m_Move.bMoveCC = false;
    }
}
