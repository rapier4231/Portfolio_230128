using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Move : Move
{
    //오직 애니메이션 변경만을 위함
    protected FSM m_FSM;
    public FSM FSM
    {
        set { m_FSM = value; }
    }

    //게임 스테이지가 아닌 Base에서 움직일 때 사용
    public bool  m_TypeisNoJump = false;

    public float m_WalkSpeed    = 2f; //Walk에 사용되는 속도
    public float m_RunSpeed     = 3f; //Run에 사용되는 속도

    public  float   m_JumpPower = 4.6f;
    private float   m_fUseJumpPower;
    private float   m_fJumpTime = 0f;
    private float   m_fJumpPosY;
    private bool    m_bJump     = false;
    public bool bJump
    {
        get { return m_bJump; }
    }

    //정사각형으로만 할 거고 기본 타일 사이즈는 1이라고 잡는다. 결국 MapTile Scale의 반이 해당 값.
    private float   m_fTileHalfSize;

    public  Transform   m_MainCameraTransform;
    private Vector3     m_v3AddToCameraPos;

    protected override void NullCheck()
    {
        base.NullCheck();
        if(m_MainCameraTransform == null)
        {
            m_MainCameraTransform = GameObject.Find("Main Camera").transform;
            if(m_MainCameraTransform == null)
            {
                KLog.Log("플레이어 메인 카메라 Player_Move에 넣어주던지 이름을 Main Camera로 바꿔주던지 해주세욥.");
            }
        }
    }

    protected override void Setting()
    {
        base.Setting();

        m_v3AddToCameraPos = m_MainCameraTransform.position - transform.position;
    }

    protected override void LateSetting()
    {
        base.LateSetting();

        if (!m_TypeisNoJump)
        {
            m_fTileHalfSize = StageMng.Instance.MapTile.transform.localScale.x * 0.5f;
        }
    }

    private void FixedUpdate()
    {
        if (!m_TypeisNoJump)
        {
            Jump_FixedUpdate();
        }
    }

    public void Set_Move(Vector3 _v3NInputDir)
    {
        if(m_TypeisNoJump)
        {
            //점프 안되는 베이스 플레이어라면 항상 달리는 속도로 한다.
            MoveFunc(_v3NInputDir, m_RunSpeed);
            m_FSM.CurrentState.Run();
        }
        else
        {
            if (Input.GetKey(KeyCode.LeftShift))
            {
                MoveFunc(_v3NInputDir, m_RunSpeed);
                m_FSM.CurrentState.Run();
            }
            else
            {
                MoveFunc(_v3NInputDir, m_WalkSpeed);
                m_FSM.CurrentState.Walk();
            }
        }

        //걷는 상태로 변경
    }

    public void Setting_MainCameraPos()
    {
        m_MainCameraTransform.position = transform.position + m_v3AddToCameraPos;
    }

    //점프키를 입력했을 때 불러줄 함수
    public void Set_Jump(bool _bInput = true)
    {
        //점프가 안되는 타입이거나 점프 상태면 안한다.
        if (m_TypeisNoJump || m_bJump)
        {
            return;
        }

        m_fJumpPosY = transform.position.y;
        m_fJumpTime = 0f;
        m_bJump = true;
        if (_bInput)
        {
            m_fUseJumpPower = m_JumpPower;
        }
        else
        {
            m_fUseJumpPower = 0f;
        }

        //현재 상태를 점프로 바꾼다.
        m_FSM.CurrentState.Jump();
    }

    public void Jump_FixedUpdate()
    {
        float fHeight;
        Vector3 v3Pos;

        if (m_bJump)
        {
            m_fJumpTime += Time.fixedDeltaTime;
            //중력가속도 9.8이지만 계산 쉽게 하기 위해서 10
            float fG    = 10f;
            fHeight     = ((-fG * 0.5f) * m_fJumpTime * m_fJumpTime) + (m_fUseJumpPower * m_fJumpTime);

            v3Pos               = transform.position;
            v3Pos.y             = m_fJumpPosY + fHeight;
            transform.position  = v3Pos;
        }

        fHeight = Ground_Height(); //현재 땅의 높이를 가져온다

        float fPlayerY = transform.position.y;

        if (fPlayerY == fHeight) //float 끼리라 ==이 되긴 힘들 수 있지만 대입 때문에 많이 탈 것 같으니 조금이라도 연산 줄여보자 
        {
            JumpFalse();
        }
        else if (fPlayerY < fHeight)
        {
            v3Pos               = transform.position;
            v3Pos.y             = fHeight;
            transform.position  = v3Pos;
            JumpFalse();
        }
        else if(fPlayerY > fHeight && !m_bJump) //절벽에서 떨어질 때 타는 로직
        {
            Set_Jump(false);
        }
    }

    //m_bJump를 false 시켜준다. & 상태가 점프 상태면 점프도 끝낸다.
    private void JumpFalse()
    {
        if (m_bJump)
        {
            m_FSM.CurrentState.Idle();
            m_bJump = false;
        }
    }

    //항상 땅을 확인하고, 내가 땅과 닿아있으면 m_bJump를 False로 바꾼다.
    //땅과 닿아있지 않다면 m_bJump를 True로 바꾼다. m_fUseJumpPower 0으로 바꾼다.

    //밟고있는 땅의 높이를 반환해준다.
    private float Ground_Height()
    {
        MapTile mapTile = StageMng.Instance.MapTile;
        float fHeight   = mapTile.Get_UnderTile_Height(transform.position);

        return fHeight;
    }

    //    private void MoveDir_Sync()
    //    {
    //        //카메라의 y축으로 회전 시켜야 함
    //        Quaternion CameraRotate         = m_MainCameraTransform.rotation;
    //        Vector3    v3CameraEulerAngles  = CameraRotate.eulerAngles;
    //        float      fReversCameraY       = (360.0f - v3CameraEulerAngles.y);
    //        float      fCosCameraY          = Mathf.Cos(fReversCameraY);
    //        float      fSinCameraY          = Mathf.Sin(fReversCameraY);
    //        float      fMoveDirX, fMoveDirZ;

    //        //y축 회전
    //        fMoveDirX = (m_v3MoveDir.x *  fCosCameraY) + (m_v3MoveDir.z * fSinCameraY);
    //        fMoveDirZ = (m_v3MoveDir.x * -fSinCameraY) + (m_v3MoveDir.z * fCosCameraY);
    //        m_v3MoveDir.x = fMoveDirX;
    //        m_v3MoveDir.y = 0.0f;
    //        m_v3MoveDir.z = fMoveDirZ;

    //        //단위 벡터로 전달해준다.
    //        m_v3MoveDir = v3MoveDir;
    //    }
}
