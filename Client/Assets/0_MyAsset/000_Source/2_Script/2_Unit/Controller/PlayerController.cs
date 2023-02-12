using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//��ǲ�� ������ �׿����� �̵�, ���º�ȭ�� �����ִ� ���� Ŭ����, FSM�� ��ӹް� ����.
public class PlayerController : UnitController
{
    //��ǲ�� ������ �׿����� �÷��̾ �̵������ִ� Ŭ����
    private Player_Move m_Player_Move;
    private Player_Atk  m_Player_Atk;

    private Vector3     m_v3InputDir;
    private Vector3     m_v3PreInputDir;
    private bool        m_bInputCC  = false;

    protected override void Setting()
    {
        base.Setting();

        m_Player_Move   = (Player_Move) m_Move;
        m_Player_Atk    = (Player_Atk)  m_Atk;

        m_v3InputDir = Vector3.zero;

        //�ִϸ��̼� ���� ���游�� ����
        m_Player_Move.FSM   = this;

        //���ݷ� �� �ִϸ��̼� ���� ����
        m_Player_Atk.UnitController = this;
    }

    protected override void LateNullCheck()
    {
        base.LateNullCheck();

        if (m_Player_Move == null)
        {
            m_Player_Move = GetComponent<Player_Move>();
            if (m_Player_Move == null)
            {
                m_Player_Move = gameObject.AddComponent<Player_Move>();
                KLog.Log("�÷��̾ Player_Move ��ũ��Ʈ�� �� �پ� �ֽ��ϴ�.");
            }
        }

        if (m_Player_Atk == null)
        {
            m_Player_Atk = GetComponent<Player_Atk>();
            if (m_Player_Atk == null)
            {
                m_Player_Atk = gameObject.AddComponent<Player_Atk>();
                KLog.Log("�÷��̾ Player_Atk ��ũ��Ʈ�� �� �پ� �ֽ��ϴ�.");
            }
        }
    }

    private void Update()
    {
        //���̵� ���·� ����
        StateChange_Idle();

        //�����̻� �ɸ��� �ʾҰ� �������� �ƴ϶��
        if (eCurrentState != eState.Atk)
        {
            if (!m_bInputCC)
            {
                InputFunc_Update();
            }
            if (!m_Player_Move.bMoveCC)
            {
                MoveFunc_Update();
            }
        }

        //ī�޶� ��ġ ����ֱ�
        m_Player_Move.Setting_MainCameraPos();
    }

    //���� ����Ű ��ǲ ���� ó��
    private void InputFunc_Update()
    {
        m_v3InputDir.x = Input.GetAxisRaw("Horizontal");
        m_v3InputDir.z = Input.GetAxisRaw("Vertical");

        if(Input.GetKeyDown(KeyCode.Space))
        {
            m_Player_Move.Set_Jump();
        }

        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            m_Player_Atk.Set_Atk(CMyUnityBase.EMyTag.Enemy);
            //��ǲ �ʱ�ȭ!
            m_v3InputDir.x = 0f;
            m_v3InputDir.y = 0f;
            m_v3InputDir.z = 0f;
        }
    }

    private void MoveFunc_Update()
    {
        if (m_v3InputDir.magnitude != 0)
        {
            //�̵�
            m_Player_Move.Set_Move(m_v3InputDir.normalized);
            //ĳ���� �ٶ󺸴� ���� ����ֱ�
            ChaLookAtFunc_Update();
        }
    }

    //���̵� ���·� ����
    private void StateChange_Idle()
    {
        //����Ű ��ǲ�� ���� ���
        if(m_v3InputDir.magnitude != 0)
        {
            return;
        }

        //���� ���� ���� ���¶��
        if (eCurrentState == eState.Jump)
        {
            return;
        }

        //���� ���� ���¶��
        if (eCurrentState == eState.Atk)
        {
            return;
        }

        CurrentState.Idle();
    }

    private void ChaLookAtFunc_Update()
    {
        if(m_v3PreInputDir != m_v3InputDir)
        {
            //Vector3 v3Right = Vector3.Cross(Vector3.up, transform.forward);
            float fAngle    = Vector3.Angle(m_v3InputDir, transform.forward);
            float fSign     = Mathf.Sign(Vector3.Dot(m_v3InputDir, transform.right/*v3Right*/));
            fAngle          *= fSign;

            m_RotationCoroutine.Set_Rotation(fAngle);
            m_v3PreInputDir = m_v3InputDir;
        }
        ////ĳ���� ����
        //transform.LookAt(transform.position + m_v3InputDir);
    }    
}
