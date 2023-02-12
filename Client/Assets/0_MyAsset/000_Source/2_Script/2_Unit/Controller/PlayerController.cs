using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//인풋을 받으면 그에따라 이동, 상태변화를 시켜주는 중추 클래스, FSM도 상속받고 있음.
public class PlayerController : UnitController
{
    //인풋을 받으면 그에따라 플레이어를 이동시켜주는 클래스
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

        //애니메이션 상태 변경만을 위함
        m_Player_Move.FSM   = this;

        //공격력 및 애니메이션 상태 변경
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
                KLog.Log("플레이어에 Player_Move 스크립트가 안 붙어 있습니다.");
            }
        }

        if (m_Player_Atk == null)
        {
            m_Player_Atk = GetComponent<Player_Atk>();
            if (m_Player_Atk == null)
            {
                m_Player_Atk = gameObject.AddComponent<Player_Atk>();
                KLog.Log("플레이어에 Player_Atk 스크립트가 안 붙어 있습니다.");
            }
        }
    }

    private void Update()
    {
        //아이들 상태로 변경
        StateChange_Idle();

        //상태이상에 걸리지 않았고 공격중이 아니라면
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

        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            m_Player_Atk.Set_Atk(CMyUnityBase.EMyTag.Enemy);
            //인풋 초기화!
            m_v3InputDir.x = 0f;
            m_v3InputDir.y = 0f;
            m_v3InputDir.z = 0f;
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
    }

    //아이들 상태로 변경
    private void StateChange_Idle()
    {
        //방향키 인풋이 있을 경우
        if(m_v3InputDir.magnitude != 0)
        {
            return;
        }

        //만약 지금 점프 상태라면
        if (eCurrentState == eState.Jump)
        {
            return;
        }

        //지금 공격 상태라면
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
        ////캐릭터 방향
        //transform.LookAt(transform.position + m_v3InputDir);
    }    
}
