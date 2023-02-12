using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitController : FSM
{
    //��ǲ�� ������ �׿����� �÷��̾ �̵������ִ� Ŭ����
    public Move     m_Move;
    public Attack   m_Atk;

    //���� Ŭ����
    public Info m_Info;

    //ȸ�� �����ִ� �ݿ�ƾth ��ü
    public RotationCoroutine    m_RotationCoroutine;
    //�з��� �����ִ� �ݿ�ƾth ��ü
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
                KLog.Log(gameObject.name + "�� Move ��ũ��Ʈ�� �� �پ� �ֽ��ϴ�.");
            }
        }

        if (m_Atk == null)
        {
            m_Atk = GetComponent<Attack>();
            if (m_Atk == null)
            {
                m_Atk = gameObject.AddComponent<Attack>();
                KLog.Log(gameObject.name + "�� Attack ��ũ��Ʈ�� �� �پ� �ֽ��ϴ�.");
            }
        }

        if (m_Info == null)
        {
            m_Info = GetComponent<Info>();
            if (m_Info == null)
            {
                m_Info = gameObject.AddComponent<Info>();
                KLog.Log(gameObject.name + "�� Info ��ũ��Ʈ�� �� �پ� �ֽ��ϴ�.");
            }
        }

        if (m_RotationCoroutine == null)
        {
            m_RotationCoroutine = GetComponent<RotationCoroutine>();
            if (m_RotationCoroutine == null)
            {
                m_RotationCoroutine = gameObject.AddComponent<RotationCoroutine>();
                KLog.Log(gameObject.name + "�� RotationCoroutine ��ũ��Ʈ�� �� �پ� �ֽ��ϴ�.");
            }
        }

        if (m_PushMoveCoroutine == null)
        {
            m_PushMoveCoroutine = GetComponent<PushMoveCoroutine>();
            if (m_PushMoveCoroutine == null)
            {
                m_PushMoveCoroutine = gameObject.AddComponent<PushMoveCoroutine>();
                KLog.Log(gameObject.name + "�� PushMoveCoroutine ��ũ��Ʈ�� �� �پ� �ֽ��ϴ�.");
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
        //�з����� ���� �ִٸ�
        if(_fPushPower != 0 )
        {
            m_PushMoveCoroutine.Set_PushMove(_v3PushDir, _fPushPower);
        }


    }
}
