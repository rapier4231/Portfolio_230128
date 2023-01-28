using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpdateCheckMng : MonoBehaviour
{
    private static UpdateCheckMng m_Instance = null;
    public static UpdateCheckMng Instance
    {
        get
        {
            if (ReferenceEquals(m_Instance, null))
            {
                GameObject instance = new GameObject("UpdateCheck Manager");
                m_Instance          = instance.AddComponent<UpdateCheckMng>();
                m_Instance.Init();
                DontDestroyOnLoad(instance);
            }
            return m_Instance;
        }
    }
    private UpdateCheckMng() { } //생성자를 private로 하여 인스턴스의 생성을 막음

    //////////////////////////////////////////////////////////////////////////////

    public struct STUpdateCheck
    {
        //bool값이 false 혹인 true가 되었을 때 함수를 실행한다.
        public delegate void DelegateFinishFunc();
        public DelegateFinishFunc   delFinishFunc;
        public delegate bool DelegateJudgeFunc();
        public DelegateJudgeFunc    delJudgeFunc;
    }

    private List<STUpdateCheck> m_stUpdateCheckList;
    private int                 m_iListCount;

    private void Init()
    {
        m_stUpdateCheckList = new List<STUpdateCheck>();
    }

    private void Update()
    {
        m_iListCount = m_stUpdateCheckList.Count;
        for (int i = 0; i < m_iListCount; i++)
        {
            if(m_stUpdateCheckList[i].delJudgeFunc())
            {
                m_stUpdateCheckList[i].delFinishFunc();
                m_stUpdateCheckList.RemoveAt(i--);
                if(i <= 0)
                {
                    break;
                }
            }
        }
    }

    //JudgeFunc이 true가 되면 delFinishFunc를 실행시킨다
    public void Set_UpdateCheck(STUpdateCheck.DelegateJudgeFunc _delJudgeFunc, STUpdateCheck.DelegateFinishFunc _delFinishFunc)
    {
        STUpdateCheck stUpdateCheck = new STUpdateCheck();
        stUpdateCheck.delJudgeFunc  += _delJudgeFunc;
        stUpdateCheck.delFinishFunc += _delFinishFunc;
        m_stUpdateCheckList.Add(stUpdateCheck);
    }
}
