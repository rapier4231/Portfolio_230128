using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//인풋을 받으면 그에따라 이동, 상태변화를 시켜주는 중추 클래스, FSM도 상속받고 있음.
public class PlayerController : UnitController
{
    //인풋을 받으면 그에따라 플레이어를 이동시켜주는 클래스
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
                KLog.Log("플레이어에 Player_Move 스크립트가 안 붙어 있습니다.");
            }
        }
    }

    protected override void Setting()
    {
        base.Setting();
        //애니메이션 상태 변경만을 위함
        m_Player_Move.FSM   = this;
        m_v3InputDir        = Vector3.zero;
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
        }

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
            //캐릭터 바라보는 방향 잡아주기
            ChaLookAtFunc_Update();
        }
        else
        {
            //방향키 인풋이 없을 경우
            //만약 지금 점프 상태가 아니라면 아이들로 바꾼다.
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
        ////캐릭터 방향
        //transform.LookAt(transform.position + m_v3InputDir);
    }    
}
