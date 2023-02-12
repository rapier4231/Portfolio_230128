using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attack : CMyUnityBase
{
    [Header("일반 공격 범위 콜라이더와, 콜라이더 중심에서 일반 공격이 닿는 최대 반지름 크기")]
    [Tooltip("따로 콜라이더만 가진 오브젝트들을 만들어 자식으로 가지고 있을 것.")]
    public Collider m_AtkCollider;
    [Tooltip("해당 반지름을 사용하여 1차 충돌체 검출용")]
    public float m_AtkOverlapRadius = 5f;

    protected UnitController m_UnitController;
    public UnitController UnitController
    {
        set { m_UnitController = value; }
    }

    protected override void NullCheck()
    {
        base.NullCheck();

        if (m_AtkCollider == null)
        {
            KLog.Log(gameObject.name + "에 AtkCollider 콜라이더가 설정되어 있지 않습니다.");
        }
    }

    protected override void Setting()
    {
        base.Setting();

        m_AtkCollider.isTrigger = true;
    }

    protected override void LateNullCheck()
    {
        base.LateNullCheck();

        if (m_UnitController == null)
        {
            KLog.Log(gameObject.name + "에 Attack 스크립트 UnitController가 설정되어 있지 않습니다.");
        }
    }

    //방어용, 현재 애니메이션이 무엇인지 확인 true면 가능
    protected bool Check_Animation(string _strAnimationName)
    {
        if (m_UnitController.m_Animator.GetCurrentAnimatorStateInfo(0).IsName(_strAnimationName))
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    //기본 공격 (때리고 싶은 놈 태그!)
    public virtual void Set_Atk(CMyUnityBase.EMyTag _eMyTag)
    {
        //1차 검사
        Collider[] CorrectionCols = Physics.OverlapSphere(m_AtkCollider.bounds.center, m_AtkOverlapRadius);

        //배열 길이
        int iLength = CorrectionCols.Length;
        Collider TempCollider;
        Transform TempTransform;
        Vector3 v3OutDirection;
        float fOutDistance;

        UnitController TempUnitController;
        for (int i = 0; i < iLength; ++i)
        {
            TempCollider = CorrectionCols[i];
            TempUnitController = TempCollider.gameObject.GetComponent<UnitController>();
            //CMyUnityBase가 없는 콜라이더라면 Pass
            if (TempUnitController == null)
            {
                continue;
            }
            //원하는 애가 아니면 Pass
            else if (!TempUnitController.Get_HaveMyTag(_eMyTag))
            {
                continue;
            }
            TempTransform = TempCollider.gameObject.transform;

            //bounds 안쓰고 포지션과 로테이션 쓰는 이유 : 각 콜라이더 별로 따로 객체를 만들어서 가지고 있을 예정이라서
            if (Physics.ComputePenetration(TempCollider, TempTransform.position, TempTransform.rotation,
                                        m_AtkCollider, transform.position, transform.rotation,
                                        out v3OutDirection, out fOutDistance))
            {
                //Atk을 맞았다면
                //때렷다!
                UnitAtk(TempUnitController);
            }
        }
    }

    //각자 재정의 해서 만들 것
    public virtual void UnitAtk(UnitController _unitController)
    {
        //_unitController.Set_Hit();
    }
}
