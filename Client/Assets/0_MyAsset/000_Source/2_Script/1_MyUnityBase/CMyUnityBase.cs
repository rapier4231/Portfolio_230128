using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CMyUnityBase : MonoBehaviour
{
    public enum EMyTag
    {
        GoalTile,
        Enemy,
        EMyTag_End
    }

    List<EMyTag> m_eMyTagList = new List<EMyTag>();

    public virtual void Awake()
    {
        Init();
    }
    public virtual void Start()
    {
        LateInit();
    }
    //public virtual void OnDisable()
    //{
    //    Exit();
    //}
    public virtual void Init()
    {
        NullCheck();
        NewCreate();
        Setting();
    }
    protected virtual void NullCheck()
    {

    }
    protected virtual void NewCreate()
    {

    }
    protected virtual void Setting()
    {

    }
    public virtual void LateInit()
    {
        LateNullCheck();
        LateNewCreate();
        LateSetting();
    }
    protected virtual void LateNullCheck()
    {

    }
    protected virtual void LateNewCreate()
    {

    }
    protected virtual void LateSetting()
    {

    }
    public virtual void Exit()
    {

    }
    public void Set_AddMyTag(EMyTag _eMyTag)
    {
        int iListCount = m_eMyTagList.Count;
        for (int i = 0; i < iListCount; i++)
        {
            if (m_eMyTagList[i] == _eMyTag)
            {
                return;
            }
        }

        m_eMyTagList.Add(_eMyTag);
    }
    public bool Set_RemoveMyTag(EMyTag _eMyTag)
    {
        int iListCount = m_eMyTagList.Count;
        for (int i = 0; i < iListCount; i++)
        {
            if(m_eMyTagList[i] == _eMyTag)
            {
                m_eMyTagList.RemoveAt(i);
                return true;
            }
        }

        return false;
    }

    public bool Get_HaveMyTag(EMyTag _eMyTag)
    {
        int iListCount = m_eMyTagList.Count;
        for (int i = 0; i < iListCount; i++)
        {
            if (m_eMyTagList[i] == _eMyTag)
            {
                return true;
            }
        }

        return false;
    }
}
