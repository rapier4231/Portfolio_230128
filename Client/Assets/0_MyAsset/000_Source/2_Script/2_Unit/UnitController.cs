using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitController : FSM
{
    //인풋을 받으면 그에따라 플레이어를 이동시켜주는 클래스
    public Move     m_Move;
    public Attack   m_Atk;

    //인포 클래스
    public Info m_Info;

    //회전 시켜주는 콜우틴th 객체
    public RotationCoroutine    m_RotationCoroutine;
    //밀려짐 시켜주는 콜우틴th 객체
    public PushMoveCoroutine    m_PushMoveCoroutine;

    protected override void NullCheck()
    {
        base.NullCheck();
        if (m_Move == null)
        {
            m_Move = GetComponent<Move>();
            if (m_Move == null)
            {
                m_Move = gameObject.AddComponent<Move>();
                KLog.Log(gameObject.name + "에 Move 스크립트가 안 붙어 있습니다.");
            }
        }

        if (m_Atk == null)
        {
            m_Atk = GetComponent<Attack>();
            if (m_Atk == null)
            {
                m_Atk = gameObject.AddComponent<Attack>();
                KLog.Log(gameObject.name + "에 Attack 스크립트가 안 붙어 있습니다.");
            }
        }

        if (m_Info == null)
        {
            m_Info = GetComponent<Info>();
            if (m_Info == null)
            {
                m_Info = gameObject.AddComponent<Info>();
                KLog.Log(gameObject.name + "에 Info 스크립트가 안 붙어 있습니다.");
            }
        }

        if (m_RotationCoroutine == null)
        {
            m_RotationCoroutine = GetComponent<RotationCoroutine>();
            if (m_RotationCoroutine == null)
            {
                m_RotationCoroutine = gameObject.AddComponent<RotationCoroutine>();
                KLog.Log(gameObject.name + "에 RotationCoroutine 스크립트가 안 붙어 있습니다.");
            }
        }

        if (m_PushMoveCoroutine == null)
        {
            m_PushMoveCoroutine = GetComponent<PushMoveCoroutine>();
            if (m_PushMoveCoroutine == null)
            {
                m_PushMoveCoroutine = gameObject.AddComponent<PushMoveCoroutine>();
                KLog.Log(gameObject.name + "에 PushMoveCoroutine 스크립트가 안 붙어 있습니다.");
            }
        }
    }

    protected override void Setting()
    {
        base.Setting();

        m_PushMoveCoroutine.Move = m_Move;
        m_PushMoveCoroutine.Info = m_Info;
    }

    public virtual void Set_Hit(float _fDmg, Vector3 _v3PushDir = default, float _fPushPower = default)
    {
        //밀려나는 힘이 있다면
        if(_fPushPower != 0 )
        {
            m_PushMoveCoroutine.Set_PushMove(_v3PushDir, _fPushPower);
        }


    }
}
