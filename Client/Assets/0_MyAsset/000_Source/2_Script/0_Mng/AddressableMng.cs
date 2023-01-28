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

    private enum EAddressableModeType
    {
        Wait,
        ClearCache,
        DownloadAsset,
        LoadAsset,
        EAddressableModeType_End
    }

    private EAddressableModeType m_eAddressableMode = EAddressableModeType.Wait;

    private List<Dictionary<string, object>> m_KeyList;
    private string  m_strKeyFileName        = "Addressable"; //Ű ���� csv �̸�
    private string  m_strSizeCheckType      = "Labels";      //üũ �� Ű Ÿ��
    private float   m_fServerDownloadSize   = 0f;            //�ٿ�ε� �ؾ� �� ������
    private float   m_fClientDownloadSize   = 0f;            //�ٿ�ε� �� �� ������
    private int     m_iDownloadDoneNumCheck = 0;             //�ٿ�ε� �� ����
    //private bool    m_bStopSizeCheck        = false;         //�ٿ�ε� ���� �� ������ ��� ����, ������ ���� ���۰� ���ÿ� ���ϹǷ� ������

    public float fServerDownloadSize
    {
        get
        {
            return m_fServerDownloadSize;
        }
    }

    public float fClientDownloadSize
    {
        get
        {
            return m_fClientDownloadSize;
        }
    }

    //public void Set_StopDownloadSizeCheck()
    //{
    //    m_bStopSizeCheck = true;
    //}    

    public void AddressableDownload()
    {
        //����� ���� �ٿ�ε� �ض�, ��¥�� ������ �ѹ� �ٿ��ϰ� ���̴ϱ� ���ҷ� �޸𸮿� ���&���� �� ��� �̷��� �ϸ� X
        if (m_eAddressableMode == EAddressableModeType.ClearCache)
        {
            m_eAddressableMode = EAddressableModeType.DownloadAsset;
        }
        else
        {
            return;
        }

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
                    //Tip : TEXT���� ���� ���ϵ��� �ٿ�ε� ���� �ʰ� LoadAssetAsync�� �ٷ� ����ص� �˾Ƽ� �ٿ� �޾Ƽ� �����Ѵ�.
                    Addressables.DownloadDependenciesAsync(strKey, true).Completed += (opDownload) =>
                     {
                         DownloadDoneNumCheck();
                         if (opDownload.Status != AsyncOperationStatus.Succeeded)
                         {
                             return;
                         }
                         m_fClientDownloadSize += opSize.Result;
                     };
                }
                else
                {
                    DownloadDoneNumCheck();
                }
            };
        }
    }

    //////Unity Behaviour///////////////////////////////////////////

    //private void Update()
    //{

    //    switch (m_eAddressableMode)
    //    {
    //        case EAddressableModeType.Wait:
    //            break;
    //        case EAddressableModeType.ClearCache:
    //            Caching.ClearCache();
    //            m_iDownloadNumCheck = 0;
    //            Debug.Log("Cache Clear - AddressableMng");
    //            break;
    //        case EAddressableModeType.DownloadAsset:
    //            break;
    //        case EAddressableModeType.LoadAsset:
    //            break;
    //        case EAddressableModeType.EAddressableModeType_End:
    //            break;
    //    }
    //}

    //////Private///////////////////////////////////////////////////

    private void Init()
    {
        //���� ���� ��巹���� �Լ����� �˾Ƽ� �ȵǸ� ȣ�������� Ȥ�� �𸣴�
        var async = Addressables.InitializeAsync();
        async.Completed += (op) =>
        {
            m_eAddressableMode = EAddressableModeType.ClearCache;
            Addressables.Release(async);
        };

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
                    m_fServerDownloadSize += opSize.Result;
                }
            };
        }
    }

    private void DownloadDoneNumCheck()
    {
        ++m_iDownloadDoneNumCheck;

        if(m_iDownloadDoneNumCheck == m_KeyList.Count)
        {
            m_eAddressableMode = EAddressableModeType.EAddressableModeType_End;
        }
    }
    private bool GetDownloadDone()
    {
        return m_iDownloadDoneNumCheck == m_KeyList.Count;
    }
}
