using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class AddressableMng : MonoBehaviour
{
    private static AddressableMng m_Instance = null;
    public static AddressableMng Instance
    {
        get
        {
            if (ReferenceEquals(m_Instance, null))
            {
                GameObject instance = new GameObject("Addressable Manager");
                m_Instance = instance.AddComponent<AddressableMng>();
                m_Instance.Init();
                DontDestroyOnLoad(instance);
            }
            return m_Instance;
        }
    }
    private AddressableMng() { } //생성자를 private로 하여 인스턴스의 생성을 막음
    ///////////////////////////////////////////////////////////////////////

    private List<Dictionary<string, object>> m_KeyList;
    private string  m_strKeyFileName    = "Addressable"; //키 파일 csv 이름
    private string  m_strSizeCheckType  = "Labels";      //체크 할 키 타입
    private float   m_fDownloadSize     = 0f;
    public float fDownloadSize
    {
        get
        {
            return m_fDownloadSize;
        }
    }

    public void AddressableDownload()
    {
        int iMaxResouceCount = m_KeyList.Count;
        for (int i = 0; i < iMaxResouceCount; ++i)
        {
            string strKey = (string)m_KeyList[i][m_strSizeCheckType];

            //다운로드 할 크기를 얻어온다.
            Addressables.GetDownloadSizeAsync(strKey).Completed += (opSize) =>
            {
                //제대로 불러왔고 다운 받을 크기가 0보다 크다면
                if (opSize.Status == AsyncOperationStatus.Succeeded && opSize.Result > 0)
                {
                    Addressables.DownloadDependenciesAsync(strKey)
                }
            };
        }
    }

    private void Init()
    {
        AddressableKeyListSetting();
        AddressableDownloadSize();
    }
    private void AddressableKeyListSetting()
    {
        m_KeyList = CSVReader.Read(m_strKeyFileName);
    }

    private void AddressableDownloadSize()
    {
        int iMaxResouceCount = m_KeyList.Count;
        for (int i = 0; i < iMaxResouceCount; ++i)
        {
            string strKey = (string)m_KeyList[i][m_strSizeCheckType];

            //다운로드 할 크기를 얻어온다.
            Addressables.GetDownloadSizeAsync(strKey).Completed += (opSize) =>
            {
                //제대로 불러왔고 다운 받을 크기가 0보다 크다면
                if (opSize.Status == AsyncOperationStatus.Succeeded && opSize.Result > 0)
                {
                    m_fDownloadSize += opSize.Result;
                }
            };
        }
    }
}
