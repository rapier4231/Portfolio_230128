using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Move : Move
{
    public float m_WalkSpeed    = 2f; //Walk에 사용되는 속도
    public float m_RunSpeed     = 3f; //Run에 사용되는 속도

    public  float   m_JumpPower = 4.6f;
    private float   m_fUseJumpPower;
    private float   m_fJumpTime = 0f;
    private float   m_fJumpPosY;
    private bool    m_bJump     = false;

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
        m_fTileHalfSize = StageMng.Instance.MapTile.transform.localScale.x * 0.5f;
    }

    private void FixedUpdate()
    {
        Jump_FixedUpdate();
    }

    public void Set_Move(Vector3 _v3NInputDir)
    {
        if(Input.GetKey(KeyCode.LeftShift))
        {
            MoveFunc(_v3NInputDir, m_RunSpeed);
        }
        else
        {
            MoveFunc(_v3NInputDir, m_WalkSpeed);
        }
    }

    public void Setting_MainCameraPos()
    {
        m_MainCameraTransform.position = transform.position + m_v3AddToCameraPos;
    }

    //점프키를 입력했을 때 불러줄 함수
    public void Set_Jump(bool _bInput = true)
    {
        //점프 상태면 안한다.
        if(m_bJump)
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
            m_bJump = false;
        }
        else if (fPlayerY < fHeight)
        {
            v3Pos               = transform.position;
            v3Pos.y             = fHeight;
            transform.position  = v3Pos;
            m_bJump             = false;
        }
        else if(fPlayerY > fHeight && !m_bJump) //절벽에서 떨어질 때 타는 로직
        {
            Set_Jump(false);
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

    //항상 땅을 확인하고, 내가 땅과 닿아있으면 m_bJump를 False로 바꾼다.
    //땅과 닿아있지 않다면 m_bJump를 True로 바꾼다. m_fUseJumpPower 0으로 바꾼다.

    //밟고있는 땅의 높이를 반환해준다.
    private float Ground_Height_Capsule()
    {
        MapTile mapTile = StageMng.Instance.MapTile;

        int ix = 0, iy = 0;
        int iTileLocation = mapTile.Get_UnderTile_ReturnQuarter(transform.position, ref ix, ref iy); //<-이거 다시 짜야 함. 미침?

        //내가 밟고 있는 타일이 High면 바로 리턴
        if (mapTile.Get_TilePos(ix, iy).y > 0.1f)
        { 
            return mapTile.Get_TilePos(ix, iy).y;
        }

        Vector3 v3TilePos = Vector3.zero;
        switch (iTileLocation)
        {
            //정 중앙 일 때는 위에서 한거랑 동일하니 사실 안해도 됨
            case 0:
                break;
            //1사분면 일 때
            case 1:
                //오른쪽 타일과 충돌하는 지 확인
                if (Crash_RightTile(mapTile, ix, iy, ref v3TilePos))
                {      //0(Low y값)보다 높으면 High이고
                       //High가 하나라도 있으면 Ground는 High 높이가 되므로
                    if (v3TilePos.y > 0.1f)
                    {
                        //바로 리턴한다.
                        return v3TilePos.y;
                    }
                }
                //위쪽 타일과 충돌하는 지 확인
                if(Crash_TopTile(mapTile,ix,iy,ref v3TilePos))
                {
                    if (v3TilePos.y > 0.1f)
                    {
                        return v3TilePos.y;
                    }
                }
                //오른쪽 위 대각선 타일과 충돌하는 지 확인
                if(Crash_RightTopTile(mapTile,ix,iy,ref v3TilePos))
                {
                    if (v3TilePos.y > 0.1f)
                    {
                        return v3TilePos.y;
                    }
                }
                break;
            //2사분면 일 때
            case 2:
                //왼쪽 타일과 충돌하는 지 확인
                if (Crash_LeftTile(mapTile, ix, iy, ref v3TilePos))
                {
                    if (v3TilePos.y > 0.1f)
                    {
                        return v3TilePos.y;
                    }
                }
                //위쪽 타일과 충돌하는 지 확인
                if (Crash_TopTile(mapTile, ix, iy, ref v3TilePos))
                {
                    if (v3TilePos.y > 0.1f)
                    {
                        return v3TilePos.y;
                    }
                }
                //왼쪽 위 대각선 타일과 충돌하는 지 확인
                if (Crash_LeftTopTile(mapTile, ix, iy, ref v3TilePos))
                {
                    if (v3TilePos.y > 0.1f)
                    {
                        return v3TilePos.y;
                    }
                }
                break;
            //3사분면 일 때
            case 3:
                //왼쪽 타일과 충돌하는 지 확인
                if (Crash_LeftTile(mapTile, ix, iy, ref v3TilePos))
                {
                    if (v3TilePos.y > 0.1f)
                    {
                        return v3TilePos.y;
                    }
                }
                //아래쪽 타일과 충돌하는 지 확인
                if (Crash_BottomTile(mapTile, ix, iy, ref v3TilePos))
                {
                    if (v3TilePos.y > 0.1f)
                    {
                        return v3TilePos.y;
                    }
                }
                //왼쪽 아래 대각선 타일과 충돌하는 지 확인
                if (Crash_LeftBottomTile(mapTile, ix, iy, ref v3TilePos))
                {
                    if (v3TilePos.y > 0.1f)
                    {
                        return v3TilePos.y;
                    }
                }
                break;
            //4사분면 일 때
            case 4:
                //오른쪽 타일과 충돌하는 지 확인
                if (Crash_RightTile(mapTile, ix, iy, ref v3TilePos))
                {
                    if (v3TilePos.y > 0.1f)
                    {
                        return v3TilePos.y;
                    }
                }
                //아래쪽 타일과 충돌하는 지 확인
                if (Crash_BottomTile(mapTile, ix, iy, ref v3TilePos))
                {
                    if (v3TilePos.y > 0.1f)
                    {
                        return v3TilePos.y;
                    }
                }
                //오른쪽 아래 대각선 타일과 충돌하는 지 확인
                if (Crash_RightBottomTile(mapTile, ix, iy, ref v3TilePos))
                {
                    if (v3TilePos.y > 0.1f)
                    {
                        return v3TilePos.y;
                    }
                }
                break;
            case 5:
                //위쪽 타일과 충돌하는 지 확인
                if (Crash_TopTile(mapTile, ix, iy, ref v3TilePos))
                {
                    if (v3TilePos.y > 0.1f)
                    {
                        return v3TilePos.y;
                    }
                }
                break;
            case 6:
                //왼쪽 타일과 충돌하는 지 확인
                if (Crash_LeftTile(mapTile, ix, iy, ref v3TilePos))
                {
                    if (v3TilePos.y > 0.1f)
                    {
                        return v3TilePos.y;
                    }
                }
                break;
            case 7:
                //아래쪽 타일과 충돌하는 지 확인
                if (Crash_BottomTile(mapTile, ix, iy, ref v3TilePos))
                {
                    if (v3TilePos.y > 0.1f)
                    {
                        return v3TilePos.y;
                    }
                }
                break;
            case 8:
                //오른쪽 타일과 충돌하는 지 확인
                if (Crash_RightTile(mapTile, ix, iy, ref v3TilePos))
                {
                    if (v3TilePos.y > 0.1f)
                    {
                        return v3TilePos.y;
                    }
                }
                break;
            default:
                break;
        }
        return 0f;
    }

    //충돌 했으면 True
    private bool Crash_TopTile(MapTile _mapTile, int _ix, int _iy, ref Vector3 _v3TilePos)
    {
        //위쪽 타일과 충돌하는 지 확인
        if (_iy != (_mapTile.m_NumY - 1))
        {
            _v3TilePos = _mapTile.Get_TilePos(_ix, _iy + 1);

            //위쪽 친구와 닿는지 확인.
            float fDisTemp = _v3TilePos.y - transform.position.y;
            //원의 중심이 사각형 변 앞쪽에 있을 때 충돌하는지
            if (fDisTemp < (m_fTileHalfSize + m_fCapsuleR))
            {
                return true;
            }
        }
        return false;
    }
    private bool Crash_RightTile(MapTile _mapTile, int _ix, int _iy, ref Vector3 _v3TilePos)
    {
        //오른쪽 타일과 충돌하는 지 확인
        if (_ix != (_mapTile.m_NumX - 1))
        {
            _v3TilePos = _mapTile.Get_TilePos(_ix + 1, _iy);

            //위쪽 친구와 닿는지 확인.
            float fDisTemp = _v3TilePos.x - transform.position.x;
            //원의 중심이 사각형 변 앞쪽에 있을 때 충돌하는지
            if (fDisTemp < (m_fTileHalfSize + m_fCapsuleR))
            {
                return true;
            }
        }
        return false;
    }
    private bool Crash_BottomTile(MapTile _mapTile, int _ix, int _iy, ref Vector3 _v3TilePos)
    {
        //아래쪽 타일과 충돌하는 지 확인
        if (_iy != 0)
        {
            _v3TilePos = _mapTile.Get_TilePos(_ix, _iy - 1);

            //아래쪽 친구와 닿는지 확인.
            float fDisTemp = transform.position.y - _v3TilePos.y;
            //원의 중심이 사각형 변 앞쪽에 있을 때 충돌하는지
            if (fDisTemp < (m_fTileHalfSize + m_fCapsuleR))
            {
                return true;
            }
        }
        return false;
    }
    private bool Crash_LeftTile(MapTile _mapTile, int _ix, int _iy, ref Vector3 _v3TilePos)
    {
        //왼쪽 타일과 충돌하는 지 확인
        if (_ix != 0)
        {
            _v3TilePos = _mapTile.Get_TilePos(_ix - 1, _iy);

            //왼쪽 친구와 닿는지 확인.
            float fDisTemp = transform.position.x - _v3TilePos.x;
            //원의 중심이 사각형 변 앞쪽에 있을 때 충돌하는지
            if (fDisTemp < (m_fTileHalfSize + m_fCapsuleR))
            {
                return true;
            }
        }
        return false;
    }
    private bool Crash_RightTopTile(MapTile _mapTile, int _ix, int _iy, ref Vector3 _v3TilePos)
    {
        if (_ix != (_mapTile.m_NumX - 1) && _iy != (_mapTile.m_NumY - 1))
        {
            _v3TilePos = _mapTile.Get_TilePos(_ix + 1, _iy + 1);
            Vector2 v2TileEdge, v2Player;
            v2TileEdge.x    = _v3TilePos.x + m_fTileHalfSize;
            v2TileEdge.y    = _v3TilePos.z + m_fTileHalfSize;

            Vector3 v3PlayerPos = transform.position;
            v2Player.x      = v3PlayerPos.x;
            v2Player.y      = v3PlayerPos.z;
            if (Vector2.Distance(v2TileEdge, v2Player) < m_fCapsuleR)
            {
                return true;
            }
        }
        return false;
    }
    private bool Crash_LeftTopTile(MapTile _mapTile, int _ix, int _iy, ref Vector3 _v3TilePos)
    {
        if (_ix != 0 && _iy != (_mapTile.m_NumY - 1))
        {
            _v3TilePos = _mapTile.Get_TilePos(_ix - 1, _iy + 1);
            Vector2 v2TileEdge, v2Player;
            v2TileEdge.x = _v3TilePos.x - m_fTileHalfSize;
            v2TileEdge.y = _v3TilePos.z + m_fTileHalfSize;

            Vector3 v3PlayerPos = transform.position;
            v2Player.x = v3PlayerPos.x;
            v2Player.y = v3PlayerPos.z;
            if (Vector2.Distance(v2TileEdge, v2Player) < m_fCapsuleR)
            {
                return true;
            }
        }
        return false;
    }
    private bool Crash_LeftBottomTile(MapTile _mapTile, int _ix, int _iy, ref Vector3 _v3TilePos)
    {
        if (_ix != 0 && _iy != 0)
        {
            _v3TilePos = _mapTile.Get_TilePos(_ix - 1, _iy - 1);
            Vector2 v2TileEdge, v2Player;
            v2TileEdge.x = _v3TilePos.x - m_fTileHalfSize;
            v2TileEdge.y = _v3TilePos.z - m_fTileHalfSize;

            Vector3 v3PlayerPos = transform.position;
            v2Player.x = v3PlayerPos.x;
            v2Player.y = v3PlayerPos.z;
            if (Vector2.Distance(v2TileEdge, v2Player) < m_fCapsuleR)
            {
                return true;
            }
        }
        return false;
    }
    private bool Crash_RightBottomTile(MapTile _mapTile, int _ix, int _iy, ref Vector3 _v3TilePos)
    {
        if (_ix != (_mapTile.m_NumX - 1) && _iy != 0)
        {
            _v3TilePos = _mapTile.Get_TilePos(_ix + 1, _iy - 1);
            Vector2 v2TileEdge, v2Player;
            v2TileEdge.x = _v3TilePos.x + m_fTileHalfSize;
            v2TileEdge.y = _v3TilePos.z - m_fTileHalfSize;

            Vector3 v3PlayerPos = transform.position;
            v2Player.x = v3PlayerPos.x;
            v2Player.y = v3PlayerPos.z;
            if (Vector2.Distance(v2TileEdge, v2Player) < m_fCapsuleR)
            {
                return true;
            }
        }
        return false;
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
