using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attack : CMyUnityBase
{
    [Header("�Ϲ� ���� ���� �ݶ��̴���, �ݶ��̴� �߽ɿ��� �Ϲ� ������ ��� �ִ� ������ ũ��")]
    [Tooltip("���� �ݶ��̴��� ���� ������Ʈ���� ����� �ڽ����� ������ ���� ��.")]
    public Collider m_AtkCollider;
    [Tooltip("�ش� �������� ����Ͽ� 1�� �浹ü �����")]
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
            KLog.Log(gameObject.name + "�� AtkCollider �ݶ��̴��� �����Ǿ� ���� �ʽ��ϴ�.");
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
            KLog.Log(gameObject.name + "�� Attack ��ũ��Ʈ UnitController�� �����Ǿ� ���� �ʽ��ϴ�.");
        }
    }

    //����, ���� �ִϸ��̼��� �������� Ȯ�� true�� ����
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

    //�⺻ ���� (������ ���� �� �±�!)
    public virtual void Set_Atk(CMyUnityBase.EMyTag _eMyTag)
    {
        //1�� �˻�
        Collider[] CorrectionCols = Physics.OverlapSphere(m_AtkCollider.bounds.center, m_AtkOverlapRadius);

        //�迭 ����
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
            //CMyUnityBase�� ���� �ݶ��̴���� Pass
            if (TempUnitController == null)
            {
                continue;
            }
            //���ϴ� �ְ� �ƴϸ� Pass
            else if (!TempUnitController.Get_HaveMyTag(_eMyTag))
            {
                continue;
            }
            TempTransform = TempCollider.gameObject.transform;

            //bounds �Ⱦ��� �����ǰ� �����̼� ���� ���� : �� �ݶ��̴� ���� ���� ��ü�� ���� ������ ���� �����̶�
            if (Physics.ComputePenetration(TempCollider, TempTransform.position, TempTransform.rotation,
                                        m_AtkCollider, transform.position, transform.rotation,
                                        out v3OutDirection, out fOutDistance))
            {
                //Atk�� �¾Ҵٸ�
                //���Ǵ�!
                UnitAtk(TempUnitController);
            }
        }
    }

    //���� ������ �ؼ� ���� ��
    public virtual void UnitAtk(UnitController _unitController)
    {
        //_unitController.Set_Hit();
    }
}
