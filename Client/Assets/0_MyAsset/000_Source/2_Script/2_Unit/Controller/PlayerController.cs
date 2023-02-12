using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//��ǲ�� ������ �׿����� �̵�, ���º�ȭ�� �����ִ� ���� Ŭ����, FSM�� ��ӹް� ����.
public class PlayerController : UnitController
{
    //��ǲ�� ������ �׿����� �÷��̾ �̵������ִ� Ŭ����
    public  Player_Move         m_Player_Move;

    private Vector3     m_v3InputDir;
    private Vector3     m_v3PreInputDir;
    private bool        m_bMoveCC   = false;
    private bool        m_bInputCC  = false;

    protected override void NullCheck()
    {
        base.NullCheck();
        if(m_Player_Move == null)
        {
            m_Player_Move = GetComponent<Player_Move>();
            if (m_Player_Move == null)
            {
                KLog.Log("�÷��̾ Player_Move ��ũ��Ʈ�� �� �پ� �ֽ��ϴ�.");
            }
        }
    }

    protected override void Setting()
    {
        base.Setting();
        //�ִϸ��̼� ���� ���游�� ����
        m_Player_Move.FSM   = this;
        m_v3InputDir        = Vector3.zero;
    }

    private void Update()
    {
        //�����̻� �ɸ��� �ʾҴٸ�
        if (!m_bInputCC)
        {
            InputFunc_Update();
        }
        if (!m_bMoveCC)
        {
            MoveFunc_Update();
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
        else
        {
            //����Ű ��ǲ�� ���� ���
            //���� ���� ���� ���°� �ƴ϶�� ���̵�� �ٲ۴�.
            if (eCurrentState != eState.Jump)
            {
                CurrentState.Idle();
            }
        }
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
