using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Move : CMyUnityBase
{
    public float m_Speed = 2f;

    public CapsuleCollider  m_CapsuleCollider; //���� ĸ�� �ݶ��̴�

    protected int     m_iGroundLayerMask;      //�� �浹�� ���� ���̾� ����ũ ����
    protected float   m_fCapsuleR;             //ĸ�� ������
    protected Vector3 m_v3CapsuleTopCenter;    //ĸ�� ���� �� ����
    protected Vector3 m_v3CapsuleBottomCenter; //ĸ�� �Ʒ� �� ����

    protected float   m_fSeparationDis = 0.001f; //�̰ݰŸ�, �浹 ������ �� ������ �����Ǿ� ������ �� ������

    //�̵� ����
    protected bool m_bMoveCC = false;
    public virtual bool bMoveCC
    {
        set { m_bMoveCC = value; }
        get { return m_bMoveCC; }
    }

    protected override void NullCheck()
    {
        base.NullCheck();

        if (m_CapsuleCollider == null)
        {
            m_CapsuleCollider = GetComponent<CapsuleCollider>();
            if (m_CapsuleCollider == null)
            {
                KLog.Log(gameObject.name + "�� ĸ�� �ݶ��̴��� �����ϴ�.");
            }
        }
    }

    protected override void Setting()
    {
        base.Setting();
        //�̵��� �浹 �����Ͽ� ����ĳ��Ʈ ��
        //���� ����Ǹ� �ȵǱ� ������ ���ش�
        m_iGroundLayerMask = LayerMask.GetMask("Ground") + LayerMask.GetMask("AtkCollider");
        m_iGroundLayerMask = ~m_iGroundLayerMask;
    }

    public void MoveFunc(Vector3 _v3MoveDir, float _fSpeed)
    {
        //�� �ึ�� �ѹ��� ����ؼ� �����̵� ���͸� ���� �������� �ʰ� ���� ȿ���� ����.
        //CanMoveDis �Լ� �ȿ��� ���ָ� �����̳� �޸𸮴� �� �� �������
        //���� ���� �����̳� ���� �κп��� �̷��� �ϴ°� �� �� ȿ���� �־� ����. �˾ƺ��⵵ ����.
        Vector3 v3MoveDir;
        v3MoveDir       = Vector3.zero;
        v3MoveDir.x     = _v3MoveDir.x;
        _v3MoveDir.x    = CanMoveDis(v3MoveDir, _fSpeed).x;

        v3MoveDir       = Vector3.zero;
        v3MoveDir.z     = _v3MoveDir.z;
        _v3MoveDir.z    = CanMoveDis(v3MoveDir, _fSpeed).z;
        transform.Translate(_v3MoveDir, Space.World);
    }


    //��ȯ�� : �̵���ų ����
    protected Vector3 CanMoveDis(Vector3 _v3MoveDir , float _fSpeed)
    {
        float fMoveDis;
        //�پ��ִ� ���¿��� �ݴ������� ������ ��, �浹 ���� ���� ������ �̵� �������� ��¦ �Ű��ش�.
        Vector3 v3UnitPos, v3Up;
        //�̹� �Ƚõ� ������ �� �̵��Ÿ�, �и��ų� �ϸ� ���ǵ尡 �þ�� �Ѵ�.
        fMoveDis = Time.deltaTime * _fSpeed; 
        v3Up        = transform.up;             //���� ��
        v3UnitPos   = transform.position;       //�ٴڿ� ����
        m_fCapsuleR = m_CapsuleCollider.radius;
        m_v3CapsuleTopCenter        = v3UnitPos + (v3Up * (m_CapsuleCollider.height - m_fCapsuleR)); //ĸ�� �ݶ��̴� ���� �� ����
        m_v3CapsuleBottomCenter     = v3UnitPos + (v3Up * m_fCapsuleR); //ĸ�� �ݶ��̴� �Ʒ� �� ����
        m_v3CapsuleBottomCenter.y   += m_fSeparationDis; //�� �浹 ������ (��¦ ���� �÷��ش�)

        //�̹� �����ӿ� �̵� �� ��ŭ�� �浹 ����Ѵ�.
        RaycastHit[] RaycastHits = Physics.CapsuleCastAll(m_v3CapsuleBottomCenter, m_v3CapsuleTopCenter, m_fCapsuleR, _v3MoveDir, fMoveDis, m_iGroundLayerMask);
        //�Ÿ� ������� ���°� �ƴ϶�� ��. ���� ����� �༮�� ���� ������� ��.
        int iRaycastLength = RaycastHits.Length;
        for (int i = 0; i < iRaycastLength; ++i)
        {
            if (RaycastHits[i].distance < fMoveDis)
            {
                GameObject RayHitGameObj = RaycastHits[i].collider.gameObject;
                //�� �ڽ��̶�� Pass �Ѵ�.
                if (Object.ReferenceEquals(RayHitGameObj, this.gameObject))
                {
                    continue;
                }
                fMoveDis = RaycastHits[i].distance - m_fSeparationDis;
            }
        }
        //�̵� ��ų �Ÿ� ��ȯ
        return (_v3MoveDir * fMoveDis);
    }

    protected virtual void LateUpdate()
    {
        CollisionCorrection();
    }

    //���� �������� ��� �����ؼ� ���ش�.
    public virtual void CollisionCorrection()
    {
        //���� �ֺ��� �ݶ��̴��� �浹�� �� �ִ���
        Collider[] CorrectionCols = Physics.OverlapCapsule(m_v3CapsuleBottomCenter, m_v3CapsuleTopCenter, m_fCapsuleR, m_iGroundLayerMask);

        int OverlapColLength = CorrectionCols.Length;
        //�浹 �� ��ü�� �ִٸ�
        if (OverlapColLength > 0)
        {
            float fOutDis;
            Vector3 v3OutDir, v3Out;
            Transform HitsTransform; //�浹ü Ʈ���� ��

            for (int i = 0; i < OverlapColLength; ++i)
            {
                //�ڱ� �ڽŵ� ����ǹǷ�
                GameObject RayHitGameObj = CorrectionCols[i].gameObject;
                if (Object.ReferenceEquals(RayHitGameObj, this.gameObject))
                {
                    continue; //�ڱ� �ڽ��� ��� Pass
                }

                HitsTransform = CorrectionCols[i].transform; //�浹ü Ʈ���� ��
                //��ģ ģ���� �����ֱ� ���ؼ��� ����� �Ÿ��� �󸶳� ������ �ϴ��� �˷��ִ� �Լ�
                //���� ������ ������ �� ���� ���..
                Physics.ComputePenetration(m_CapsuleCollider, transform.position, transform.rotation,
                                           CorrectionCols[i], HitsTransform.position, HitsTransform.rotation,
                                           out v3OutDir, out fOutDis);
                //���� ����� �Ѵٸ�
                if (fOutDis > 0.0f)
                {
                    fOutDis += m_fSeparationDis;
                    v3Out = v3OutDir * fOutDis; //v3OutDir�� �Ÿ��� �����༭ �̵��ؾ� �� ���͸� ������ش�.
                    transform.Translate(v3Out, Space.World); //�浹���� ����� �ϴ� ����� �Ÿ���ŭ �̵������ش�.
                }
            }
        }
    }
}
