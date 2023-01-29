using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Move : Move
{
    //���� ���������� �ƴ� Base���� ������ �� ���
    public bool  m_NoJump = false;

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

        if (!m_NoJump)
        {
            m_fTileHalfSize = StageMng.Instance.MapTile.transform.localScale.x * 0.5f;
        }
    }

    private void FixedUpdate()
    {
        if (!m_NoJump)
        {
            Jump_FixedUpdate();
        }
    }

    public void Set_Move(Vector3 _v3NInputDir)
    {
        if(m_NoJump)
        {
            MoveFunc(_v3NInputDir, m_RunSpeed);
        }
        else
        {
            if (Input.GetKey(KeyCode.LeftShift))
            {
                MoveFunc(_v3NInputDir, m_RunSpeed);
            }
            else
            {
                MoveFunc(_v3NInputDir, m_WalkSpeed);
            }
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
