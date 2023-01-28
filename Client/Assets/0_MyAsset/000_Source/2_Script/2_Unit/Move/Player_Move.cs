using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Move : Move
{
    public float m_WalkSpeed    = 2f; //Walk�� ���Ǵ� �ӵ�
    public float m_RunSpeed     = 3f; //Run�� ���Ǵ� �ӵ�

    public  float   m_JumpPower = 4.6f;
    private float   m_fUseJumpPower;
    private float   m_fJumpTime = 0f;
    private float   m_fJumpPosY;
    private bool    m_bJump     = false;

    //���簢�����θ� �� �Ű� �⺻ Ÿ�� ������� 1�̶�� ��´�. �ᱹ MapTile Scale�� ���� �ش� ��.
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
                KLog.Log("�÷��̾� ���� ī�޶� Player_Move�� �־��ִ��� �̸��� Main Camera�� �ٲ��ִ��� ���ּ���.");
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

    //����Ű�� �Է����� �� �ҷ��� �Լ�
    public void Set_Jump(bool _bInput = true)
    {
        //���� ���¸� ���Ѵ�.
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
            //�߷°��ӵ� 9.8������ ��� ���� �ϱ� ���ؼ� 10
            float fG    = 10f;
            fHeight     = ((-fG * 0.5f) * m_fJumpTime * m_fJumpTime) + (m_fUseJumpPower * m_fJumpTime);

            v3Pos               = transform.position;
            v3Pos.y             = m_fJumpPosY + fHeight;
            transform.position  = v3Pos;
        }

        fHeight = Ground_Height(); //���� ���� ���̸� �����´�

        float fPlayerY = transform.position.y;

        if (fPlayerY == fHeight) //float ������ ==�� �Ǳ� ���� �� ������ ���� ������ ���� Ż �� ������ �����̶� ���� �ٿ����� 
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
        else if(fPlayerY > fHeight && !m_bJump) //�������� ������ �� Ÿ�� ����
        {
            Set_Jump(false);
        }
    }

    //�׻� ���� Ȯ���ϰ�, ���� ���� ��������� m_bJump�� False�� �ٲ۴�.
    //���� ������� �ʴٸ� m_bJump�� True�� �ٲ۴�. m_fUseJumpPower 0���� �ٲ۴�.

    //����ִ� ���� ���̸� ��ȯ���ش�.
    private float Ground_Height()
    {
        MapTile mapTile = StageMng.Instance.MapTile;
        float fHeight   = mapTile.Get_UnderTile_Height(transform.position);

        return fHeight;
    }

    //�׻� ���� Ȯ���ϰ�, ���� ���� ��������� m_bJump�� False�� �ٲ۴�.
    //���� ������� �ʴٸ� m_bJump�� True�� �ٲ۴�. m_fUseJumpPower 0���� �ٲ۴�.

    //����ִ� ���� ���̸� ��ȯ���ش�.
    private float Ground_Height_Capsule()
    {
        MapTile mapTile = StageMng.Instance.MapTile;

        int ix = 0, iy = 0;
        int iTileLocation = mapTile.Get_UnderTile_ReturnQuarter(transform.position, ref ix, ref iy); //<-�̰� �ٽ� ¥�� ��. ��ħ?

        //���� ��� �ִ� Ÿ���� High�� �ٷ� ����
        if (mapTile.Get_TilePos(ix, iy).y > 0.1f)
        { 
            return mapTile.Get_TilePos(ix, iy).y;
        }

        Vector3 v3TilePos = Vector3.zero;
        switch (iTileLocation)
        {
            //�� �߾� �� ���� ������ �ѰŶ� �����ϴ� ��� ���ص� ��
            case 0:
                break;
            //1��и� �� ��
            case 1:
                //������ Ÿ�ϰ� �浹�ϴ� �� Ȯ��
                if (Crash_RightTile(mapTile, ix, iy, ref v3TilePos))
                {      //0(Low y��)���� ������ High�̰�
                       //High�� �ϳ��� ������ Ground�� High ���̰� �ǹǷ�
                    if (v3TilePos.y > 0.1f)
                    {
                        //�ٷ� �����Ѵ�.
                        return v3TilePos.y;
                    }
                }
                //���� Ÿ�ϰ� �浹�ϴ� �� Ȯ��
                if(Crash_TopTile(mapTile,ix,iy,ref v3TilePos))
                {
                    if (v3TilePos.y > 0.1f)
                    {
                        return v3TilePos.y;
                    }
                }
                //������ �� �밢�� Ÿ�ϰ� �浹�ϴ� �� Ȯ��
                if(Crash_RightTopTile(mapTile,ix,iy,ref v3TilePos))
                {
                    if (v3TilePos.y > 0.1f)
                    {
                        return v3TilePos.y;
                    }
                }
                break;
            //2��и� �� ��
            case 2:
                //���� Ÿ�ϰ� �浹�ϴ� �� Ȯ��
                if (Crash_LeftTile(mapTile, ix, iy, ref v3TilePos))
                {
                    if (v3TilePos.y > 0.1f)
                    {
                        return v3TilePos.y;
                    }
                }
                //���� Ÿ�ϰ� �浹�ϴ� �� Ȯ��
                if (Crash_TopTile(mapTile, ix, iy, ref v3TilePos))
                {
                    if (v3TilePos.y > 0.1f)
                    {
                        return v3TilePos.y;
                    }
                }
                //���� �� �밢�� Ÿ�ϰ� �浹�ϴ� �� Ȯ��
                if (Crash_LeftTopTile(mapTile, ix, iy, ref v3TilePos))
                {
                    if (v3TilePos.y > 0.1f)
                    {
                        return v3TilePos.y;
                    }
                }
                break;
            //3��и� �� ��
            case 3:
                //���� Ÿ�ϰ� �浹�ϴ� �� Ȯ��
                if (Crash_LeftTile(mapTile, ix, iy, ref v3TilePos))
                {
                    if (v3TilePos.y > 0.1f)
                    {
                        return v3TilePos.y;
                    }
                }
                //�Ʒ��� Ÿ�ϰ� �浹�ϴ� �� Ȯ��
                if (Crash_BottomTile(mapTile, ix, iy, ref v3TilePos))
                {
                    if (v3TilePos.y > 0.1f)
                    {
                        return v3TilePos.y;
                    }
                }
                //���� �Ʒ� �밢�� Ÿ�ϰ� �浹�ϴ� �� Ȯ��
                if (Crash_LeftBottomTile(mapTile, ix, iy, ref v3TilePos))
                {
                    if (v3TilePos.y > 0.1f)
                    {
                        return v3TilePos.y;
                    }
                }
                break;
            //4��и� �� ��
            case 4:
                //������ Ÿ�ϰ� �浹�ϴ� �� Ȯ��
                if (Crash_RightTile(mapTile, ix, iy, ref v3TilePos))
                {
                    if (v3TilePos.y > 0.1f)
                    {
                        return v3TilePos.y;
                    }
                }
                //�Ʒ��� Ÿ�ϰ� �浹�ϴ� �� Ȯ��
                if (Crash_BottomTile(mapTile, ix, iy, ref v3TilePos))
                {
                    if (v3TilePos.y > 0.1f)
                    {
                        return v3TilePos.y;
                    }
                }
                //������ �Ʒ� �밢�� Ÿ�ϰ� �浹�ϴ� �� Ȯ��
                if (Crash_RightBottomTile(mapTile, ix, iy, ref v3TilePos))
                {
                    if (v3TilePos.y > 0.1f)
                    {
                        return v3TilePos.y;
                    }
                }
                break;
            case 5:
                //���� Ÿ�ϰ� �浹�ϴ� �� Ȯ��
                if (Crash_TopTile(mapTile, ix, iy, ref v3TilePos))
                {
                    if (v3TilePos.y > 0.1f)
                    {
                        return v3TilePos.y;
                    }
                }
                break;
            case 6:
                //���� Ÿ�ϰ� �浹�ϴ� �� Ȯ��
                if (Crash_LeftTile(mapTile, ix, iy, ref v3TilePos))
                {
                    if (v3TilePos.y > 0.1f)
                    {
                        return v3TilePos.y;
                    }
                }
                break;
            case 7:
                //�Ʒ��� Ÿ�ϰ� �浹�ϴ� �� Ȯ��
                if (Crash_BottomTile(mapTile, ix, iy, ref v3TilePos))
                {
                    if (v3TilePos.y > 0.1f)
                    {
                        return v3TilePos.y;
                    }
                }
                break;
            case 8:
                //������ Ÿ�ϰ� �浹�ϴ� �� Ȯ��
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

    //�浹 ������ True
    private bool Crash_TopTile(MapTile _mapTile, int _ix, int _iy, ref Vector3 _v3TilePos)
    {
        //���� Ÿ�ϰ� �浹�ϴ� �� Ȯ��
        if (_iy != (_mapTile.m_NumY - 1))
        {
            _v3TilePos = _mapTile.Get_TilePos(_ix, _iy + 1);

            //���� ģ���� ����� Ȯ��.
            float fDisTemp = _v3TilePos.y - transform.position.y;
            //���� �߽��� �簢�� �� ���ʿ� ���� �� �浹�ϴ���
            if (fDisTemp < (m_fTileHalfSize + m_fCapsuleR))
            {
                return true;
            }
        }
        return false;
    }
    private bool Crash_RightTile(MapTile _mapTile, int _ix, int _iy, ref Vector3 _v3TilePos)
    {
        //������ Ÿ�ϰ� �浹�ϴ� �� Ȯ��
        if (_ix != (_mapTile.m_NumX - 1))
        {
            _v3TilePos = _mapTile.Get_TilePos(_ix + 1, _iy);

            //���� ģ���� ����� Ȯ��.
            float fDisTemp = _v3TilePos.x - transform.position.x;
            //���� �߽��� �簢�� �� ���ʿ� ���� �� �浹�ϴ���
            if (fDisTemp < (m_fTileHalfSize + m_fCapsuleR))
            {
                return true;
            }
        }
        return false;
    }
    private bool Crash_BottomTile(MapTile _mapTile, int _ix, int _iy, ref Vector3 _v3TilePos)
    {
        //�Ʒ��� Ÿ�ϰ� �浹�ϴ� �� Ȯ��
        if (_iy != 0)
        {
            _v3TilePos = _mapTile.Get_TilePos(_ix, _iy - 1);

            //�Ʒ��� ģ���� ����� Ȯ��.
            float fDisTemp = transform.position.y - _v3TilePos.y;
            //���� �߽��� �簢�� �� ���ʿ� ���� �� �浹�ϴ���
            if (fDisTemp < (m_fTileHalfSize + m_fCapsuleR))
            {
                return true;
            }
        }
        return false;
    }
    private bool Crash_LeftTile(MapTile _mapTile, int _ix, int _iy, ref Vector3 _v3TilePos)
    {
        //���� Ÿ�ϰ� �浹�ϴ� �� Ȯ��
        if (_ix != 0)
        {
            _v3TilePos = _mapTile.Get_TilePos(_ix - 1, _iy);

            //���� ģ���� ����� Ȯ��.
            float fDisTemp = transform.position.x - _v3TilePos.x;
            //���� �߽��� �簢�� �� ���ʿ� ���� �� �浹�ϴ���
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
    //        //ī�޶��� y������ ȸ�� ���Ѿ� ��
    //        Quaternion CameraRotate         = m_MainCameraTransform.rotation;
    //        Vector3    v3CameraEulerAngles  = CameraRotate.eulerAngles;
    //        float      fReversCameraY       = (360.0f - v3CameraEulerAngles.y);
    //        float      fCosCameraY          = Mathf.Cos(fReversCameraY);
    //        float      fSinCameraY          = Mathf.Sin(fReversCameraY);
    //        float      fMoveDirX, fMoveDirZ;

    //        //y�� ȸ��
    //        fMoveDirX = (m_v3MoveDir.x *  fCosCameraY) + (m_v3MoveDir.z * fSinCameraY);
    //        fMoveDirZ = (m_v3MoveDir.x * -fSinCameraY) + (m_v3MoveDir.z * fCosCameraY);
    //        m_v3MoveDir.x = fMoveDirX;
    //        m_v3MoveDir.y = 0.0f;
    //        m_v3MoveDir.z = fMoveDirZ;

    //        //���� ���ͷ� �������ش�.
    //        m_v3MoveDir = v3MoveDir;
    //    }
}
