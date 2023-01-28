using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageMng : MonoBehaviour
{
    private static StageMng m_Instance = null;
    public static StageMng Instance
    {
        get
        {
            if (ReferenceEquals(m_Instance, null))
            {
                GameObject instance = new GameObject("Stage Manager");
                m_Instance = instance.AddComponent<StageMng>();
                m_Instance.Init();
                DontDestroyOnLoad(instance);    
            }
            return m_Instance;
        }
    }
    private StageMng() { } //�����ڸ� private�� �Ͽ� �ν��Ͻ��� ������ ����
    ///////////////////////////////////////////////////////////////////////

    private MapTile m_NowStageMapTile;
    public MapTile MapTile
    {
        get { return m_NowStageMapTile; }
        set { m_NowStageMapTile = value; }
    }

    private void Init()
    {

    }
}
