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
    private string  m_strKeyFileName        = "Addressable"; //키 파일 csv 이름
    private string  m_strSizeCheckType      = "Labels";      //체크 할 키 타입
    private float   m_fServerDownloadSize   = 0f;            //다운로드 해야 할 사이즈
    private float   m_fClientDownloadSize   = 0f;            //다운로드 한 할 사이즈
    private int     m_iDownloadDoneNumCheck = 0;             //다운로드 한 갯수
    //private bool    m_bStopSizeCheck        = false;         //다운로드 시작 전 사이즈 계산 멈춤, 하지만 게임 시작과 동시에 구하므로 괜찮음

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
        //비웠을 때만 다운로드 해라, 어짜피 지금은 한번 다운하고 끝이니까 분할로 메모리에 등록&해제 할 경우 이렇게 하면 X
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

            //다운로드 할 크기를 얻어온다.
            Addressables.GetDownloadSizeAsync(strKey).Completed += (opSize) =>
            {
                //제대로 불러왔고 다운 받을 크기가 0보다 크다면
                if (opSize.Status == AsyncOperationStatus.Succeeded && opSize.Result > 0)
                {
                    //Tip : TEXT같이 작은 파일들은 다운로드 받지 않고 LoadAssetAsync를 바로 사용해도 알아서 다운 받아서 생성한다.
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
        //자주 쓰는 어드레서블 함수들은 알아서 안되면 호출하지만 혹시 모르니
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

            //다운로드 할 크기를 얻어온다.
            Addressables.GetDownloadSizeAsync(strKey).Completed += (opSize) =>
            {
                //제대로 불러왔고 다운 받을 크기가 0보다 크다면
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
