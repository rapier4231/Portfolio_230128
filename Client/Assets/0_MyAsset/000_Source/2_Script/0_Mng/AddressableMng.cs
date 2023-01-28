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
    private AddressableMng() { } //�����ڸ� private�� �Ͽ� �ν��Ͻ��� ������ ����
    ///////////////////////////////////////////////////////////////////////

    private List<Dictionary<string, object>> m_KeyList;
    private string  m_strKeyFileName    = "Addressable"; //Ű ���� csv �̸�
    private string  m_strSizeCheckType  = "Labels";      //üũ �� Ű Ÿ��
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

            //�ٿ�ε� �� ũ�⸦ ���´�.
            Addressables.GetDownloadSizeAsync(strKey).Completed += (opSize) =>
            {
                //����� �ҷ��԰� �ٿ� ���� ũ�Ⱑ 0���� ũ�ٸ�
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

            //�ٿ�ε� �� ũ�⸦ ���´�.
            Addressables.GetDownloadSizeAsync(strKey).Completed += (opSize) =>
            {
                //����� �ҷ��԰� �ٿ� ���� ũ�Ⱑ 0���� ũ�ٸ�
                if (opSize.Status == AsyncOperationStatus.Succeeded && opSize.Result > 0)
                {
                    m_fDownloadSize += opSize.Result;
                }
            };
        }
    }
}
