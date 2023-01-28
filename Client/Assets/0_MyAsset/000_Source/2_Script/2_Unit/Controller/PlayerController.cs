using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//��ǲ�� ������ �׿����� �̵�, ���º�ȭ�� �����ִ� ���� Ŭ����, FSM�� ��ӹް� ����.
public class PlayerController : UnitController
{
    //��ǲ�� ������ �׿����� �÷��̾ �̵������ִ� Ŭ����
    public  Player_Move m_Player_Move;

    private Vector3     m_v3InputDir;
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
        m_v3InputDir = Vector3.zero;
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
            //JumpFunc_Update(); -> �÷��̾��� ������ �������� ��� ����.
        }

        //ĳ���� �ٶ󺸴� ���� ����ֱ�
        ChaLookAtFunc_Update();
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
        }
    }

    //private void JumpFunc_Update()
    //{
    //    //�÷��̾� ����
    //    m_Player_Move.Jump_Update();
    //}

    private void ChaLookAtFunc_Update()
    {
        //ĳ���� ����
        transform.LookAt(transform.position + m_v3InputDir);
    }    
}
