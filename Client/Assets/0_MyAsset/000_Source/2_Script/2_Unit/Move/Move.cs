using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Move : CMyUnityBase
{
    public float m_Speed = 2f;

    public CapsuleCollider  m_CapsuleCollider; //유닛 캡슐 콜라이더

    protected int     m_iGroundLayerMask;      //땅 충돌을 막을 레이어 마스크 저장
    protected float   m_fCapsuleR;             //캡슐 반지름
    protected Vector3 m_v3CapsuleTopCenter;    //캡슐 위쪽 구 센터
    protected Vector3 m_v3CapsuleBottomCenter; //캡슐 아래 구 센터

    protected float   m_fSeparationDis = 0.001f; //이격거리, 충돌 감지시 딱 붙으면 감지되어 움직일 수 없으니

    //이동 제어
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
                KLog.Log(gameObject.name + "의 캡슐 콜라이더가 없습니다.");
            }
        }
    }

    protected override void Setting()
    {
        base.Setting();
        //이동시 충돌 관련하여 레이캐스트 시
        //땅은 검출되면 안되기 때문에 빼준다
        m_iGroundLayerMask = LayerMask.GetMask("Ground") + LayerMask.GetMask("AtkCollider");
        m_iGroundLayerMask = ~m_iGroundLayerMask;
    }

    public void MoveFunc(Vector3 _v3MoveDir, float _fSpeed)
    {
        //각 축마다 한번씩 계산해서 슬라이딩 벡터를 따로 구해주지 않고 같은 효과를 낸다.
        //CanMoveDis 함수 안에서 해주면 연산이나 메모리는 좀 덜 들겠지만
        //유지 보수 측면이나 협업 부분에서 이렇게 하는게 좀 더 효율성 있어 보임. 알아보기도 쉽고.
        Vector3 v3MoveDir;
        v3MoveDir       = Vector3.zero;
        v3MoveDir.x     = _v3MoveDir.x;
        _v3MoveDir.x    = CanMoveDis(v3MoveDir, _fSpeed).x;

        v3MoveDir       = Vector3.zero;
        v3MoveDir.z     = _v3MoveDir.z;
        _v3MoveDir.z    = CanMoveDis(v3MoveDir, _fSpeed).z;
        transform.Translate(_v3MoveDir, Space.World);
    }


    //반환값 : 이동시킬 벡터
    protected Vector3 CanMoveDis(Vector3 _v3MoveDir , float _fSpeed)
    {
        float fMoveDis;
        //붙어있는 상태에서 반대편으로 움직일 때, 충돌 판정 나기 때문에 이동 방향으로 살짝 옮겨준다.
        Vector3 v3UnitPos, v3Up;
        //이번 픽시드 프레임 때 이동거리, 밀리거나 하면 스피드가 늘어나야 한다.
        fMoveDis = Time.deltaTime * _fSpeed; 
        v3Up        = transform.up;             //유닛 위
        v3UnitPos   = transform.position;       //바닥에 있음
        m_fCapsuleR = m_CapsuleCollider.radius;
        m_v3CapsuleTopCenter        = v3UnitPos + (v3Up * (m_CapsuleCollider.height - m_fCapsuleR)); //캡슐 콜라이더 위쪽 원 센터
        m_v3CapsuleBottomCenter     = v3UnitPos + (v3Up * m_fCapsuleR); //캡슐 콜라이더 아래 원 센터
        m_v3CapsuleBottomCenter.y   += m_fSeparationDis; //땅 충돌 방지용 (살짝 위로 올려준다)

        //이번 프레임에 이동 할 만큼만 충돌 계산한다.
        RaycastHit[] RaycastHits = Physics.CapsuleCastAll(m_v3CapsuleBottomCenter, m_v3CapsuleTopCenter, m_fCapsuleR, _v3MoveDir, fMoveDis, m_iGroundLayerMask);
        //거리 순서대로 담기는게 아니라고 함. 가장 가까운 녀석을 직접 구해줘야 함.
        int iRaycastLength = RaycastHits.Length;
        for (int i = 0; i < iRaycastLength; ++i)
        {
            if (RaycastHits[i].distance < fMoveDis)
            {
                GameObject RayHitGameObj = RaycastHits[i].collider.gameObject;
                //나 자신이라면 Pass 한다.
                if (Object.ReferenceEquals(RayHitGameObj, this.gameObject))
                {
                    continue;
                }
                fMoveDis = RaycastHits[i].distance - m_fSeparationDis;
            }
        }
        //이동 시킬 거리 반환
        return (_v3MoveDir * fMoveDis);
    }

    protected virtual void LateUpdate()
    {
        CollisionCorrection();
    }

    //만약 겹쳐있을 경우 보정해서 빼준다.
    public virtual void CollisionCorrection()
    {
        //유닛 주변에 콜라이더가 충돌한 게 있는지
        Collider[] CorrectionCols = Physics.OverlapCapsule(m_v3CapsuleBottomCenter, m_v3CapsuleTopCenter, m_fCapsuleR, m_iGroundLayerMask);

        int OverlapColLength = CorrectionCols.Length;
        //충돌 한 물체가 있다면
        if (OverlapColLength > 0)
        {
            float fOutDis;
            Vector3 v3OutDir, v3Out;
            Transform HitsTransform; //충돌체 트랜스 폼

            for (int i = 0; i < OverlapColLength; ++i)
            {
                //자기 자신도 검출되므로
                GameObject RayHitGameObj = CorrectionCols[i].gameObject;
                if (Object.ReferenceEquals(RayHitGameObj, this.gameObject))
                {
                    continue; //자기 자신일 경우 Pass
                }

                HitsTransform = CorrectionCols[i].transform; //충돌체 트랜스 폼
                //겹친 친구들 빼내주기 위해서는 방향과 거리가 얼마나 빠져야 하는지 알려주는 함수
                //서로 빠지면 벌어질 것 같긴 허다..
                Physics.ComputePenetration(m_CapsuleCollider, transform.position, transform.rotation,
                                           CorrectionCols[i], HitsTransform.position, HitsTransform.rotation,
                                           out v3OutDir, out fOutDis);
                //보정 해줘야 한다면
                if (fOutDis > 0.0f)
                {
                    fOutDis += m_fSeparationDis;
                    v3Out = v3OutDir * fOutDis; //v3OutDir에 거리를 곱해줘서 이동해야 할 벡터를 만들어준다.
                    transform.Translate(v3Out, Space.World); //충돌에서 벗어나야 하는 방향과 거리만큼 이동시켜준다.
                }
            }
        }
    }
}
