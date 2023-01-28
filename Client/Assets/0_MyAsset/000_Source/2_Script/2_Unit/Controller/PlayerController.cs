using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//인풋을 받으면 그에따라 이동, 상태변화를 시켜주는 중추 클래스, FSM도 상속받고 있음.
public class PlayerController : UnitController
{
    //인풋을 받으면 그에따라 플레이어를 이동시켜주는 클래스
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
                KLog.Log("플레이어에 Player_Move 스크립트가 안 붙어 있습니다.");
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
        //상태이상에 걸리지 않았다면
        if (!m_bInputCC)
        {
            InputFunc_Update();
        }
        if (!m_bMoveCC)
        {
            MoveFunc_Update();
            //JumpFunc_Update(); -> 플레이어의 피직스 업뎃에서 계속 돌림.
        }

        //캐릭터 바라보는 방향 잡아주기
        ChaLookAtFunc_Update();
        //카메라 위치 잡아주기
        m_Player_Move.Setting_MainCameraPos();
    }

    //유저 방향키 인풋 관련 처리
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
            //이동
            m_Player_Move.Set_Move(m_v3InputDir.normalized);
        }
    }

    //private void JumpFunc_Update()
    //{
    //    //플레이어 점프
    //    m_Player_Move.Jump_Update();
    //}

    private void ChaLookAtFunc_Update()
    {
        //캐릭터 방향
        transform.LookAt(transform.position + m_v3InputDir);
    }    
}
