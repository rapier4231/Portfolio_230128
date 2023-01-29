using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

//나중에는 쓰지 않을 임시 씬 매니저. 어드레서블로 바꿀 예정. 테스트 용

public enum ESceneType
{
    Luncher,
    Loading,
    Title,
    InGame_Base,
    Stage1,
    ESceneType_End
}

public class SceneMng : MonoBehaviour
{
    private static SceneMng m_Instance = null;
    public static SceneMng Instance
    {
        get
        {
            if (ReferenceEquals(m_Instance, null))
            {
                GameObject instance = new GameObject("Scene Manager");
                m_Instance = instance.AddComponent<SceneMng>();
                m_Instance.Init();
                DontDestroyOnLoad(instance);
            }
            return m_Instance;
        }
    }
    private SceneMng() { } //생성자를 private로 하여 인스턴스의 생성을 막음
    ///////////////////////////////////////////////////////////////////////

    private ESceneType      m_ePreSceneType      = ESceneType.ESceneType_End;
    public ESceneType ePreSceneType //전 씬으로 돌아가기 등등에 사용
    {
        get
        {
            return m_ePreSceneType;
        }
    }
    private ESceneType      m_eCurrentSceneType  = ESceneType.Luncher;
    private AsyncOperation  m_AsyncOp;            //allowSceneActivation을 이용하여 다음 Scene을 바로 보이기 위함
    private float           m_fProgress          = 0f;
    private float           m_fFakeTime          = 3f; //로딩 씬 너무 빨리 지나가지 않게 페이크 로딩 시간

    //씬을 넘어갈 때, 이벤트를 받아서 넘어갈 것인지 //로딩 씬을 통해 넘어 갈 것인지
    public void Set_LoadSceneAsync(ESceneType _eNextSceneType, bool _bAllowSceneActivation = true, bool _bPassThroughLoading = true)
    {
        if (_bPassThroughLoading)
        {
            //로딩씬을 경유한다 (로딩씬 다음 씬이 _eNextSceneType)
            StartCoroutine(IELoadLoadingSceneAsync(_eNextSceneType, _bAllowSceneActivation));
        }
        else
        {
            StartCoroutine(IELoadSceneAsync(_eNextSceneType, _bAllowSceneActivation));
        }
    }

    //다음 Scene으로 이동
    public void Set_AllowSceneActivation()
    {
        m_AsyncOp.allowSceneActivation = true;
    }

    public float Get_SceneLoadingProgress()
    {
        return m_fProgress < 0.9f ? m_fProgress * 111f : 100f;
    }

    private void Init()
    {

    }

    private IEnumerator IELoadSceneAsync(ESceneType _eNextSceneType, bool _bAllowSceneActivation = true)
    {
        m_AsyncOp = SceneManager.LoadSceneAsync(_eNextSceneType.ToString());

        //다음 씬 넘어갈 준비를 마친 후 이벤트를 기다릴지
        m_AsyncOp.allowSceneActivation = _bAllowSceneActivation;

        while (true) //SceneLoading
        {
            if (m_AsyncOp.isDone)
            {
                m_fProgress         = 0f;
                m_ePreSceneType     = m_eCurrentSceneType;
                m_eCurrentSceneType = _eNextSceneType;
                break;
            }
            else
            {
                //얼마나 진행되었는지 (바로 안넘어갈 시 0.9에서 대기)
                m_fProgress = m_AsyncOp.progress;
            }
            yield return null;
        }
        yield return null;
    }

    private IEnumerator IELoadLoadingSceneAsync(ESceneType _eNextSceneType, bool _bAllowSceneActivation = true)
    {
        m_AsyncOp = SceneManager.LoadSceneAsync(ESceneType.Loading.ToString());

        //다음 씬 넘어갈 준비를 마친 후 이벤트를 기다릴지
        m_AsyncOp.allowSceneActivation = true;

        while (true) //SceneLoading (LoadingScene)
        {
            if (m_AsyncOp.isDone)
            {
                break;
            }
            yield return null;
        }

        //LoadingScene은 도착, 다음 씬 출발
        m_AsyncOp = SceneManager.LoadSceneAsync(_eNextSceneType.ToString());

        //다음 씬 넘어갈 준비를 마친 후 이벤트를 기다릴지
        m_AsyncOp.allowSceneActivation = false;

        float fTimeTemp = 0f;
        while (true) //SceneLoading (_eNextSceneType)
        {
            if (m_AsyncOp.isDone)
            {
                m_fProgress         = 0f;
                m_ePreSceneType     = m_eCurrentSceneType;
                m_eCurrentSceneType = _eNextSceneType;
                break;
            }
            else
            {
                //얼마나 진행되었는지 (바로 안넘어갈 시 0.9에서 대기)
                m_fProgress = m_AsyncOp.progress;

                fTimeTemp += Time.deltaTime; //코루틴은 비동기가 아니라서 호출 횟수가 같아 괜찮다.
                if(fTimeTemp > m_fFakeTime)
                {
                    m_AsyncOp.allowSceneActivation = _bAllowSceneActivation;
                }
            }
            yield return null;
        }

        yield return null;
    }
}
