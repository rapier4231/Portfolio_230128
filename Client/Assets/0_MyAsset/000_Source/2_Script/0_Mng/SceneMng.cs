using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

//���߿��� ���� ���� �ӽ� �� �Ŵ���. ��巹����� �ٲ� ����. �׽�Ʈ ��

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
    private SceneMng() { } //�����ڸ� private�� �Ͽ� �ν��Ͻ��� ������ ����
    ///////////////////////////////////////////////////////////////////////

    private ESceneType      m_ePreSceneType      = ESceneType.ESceneType_End;
    public ESceneType ePreSceneType //�� ������ ���ư��� �� ���
    {
        get
        {
            return m_ePreSceneType;
        }
    }
    private ESceneType      m_eCurrentSceneType  = ESceneType.Luncher;
    private AsyncOperation  m_AsyncOp;            //allowSceneActivation�� �̿��Ͽ� ���� Scene�� �ٷ� ���̱� ����
    private float           m_fProgress          = 0f;
    private float           m_fFakeTime          = 3f; //�ε� �� �ʹ� ���� �������� �ʰ� ����ũ �ε� �ð�

    //���� �Ѿ ��, �̺�Ʈ�� �޾Ƽ� �Ѿ ������ //�ε� ���� ���� �Ѿ� �� ������
    public void Set_LoadSceneAsync(ESceneType _eNextSceneType, bool _bAllowSceneActivation = true, bool _bPassThroughLoading = true)
    {
        if (_bPassThroughLoading)
        {
            //�ε����� �����Ѵ� (�ε��� ���� ���� _eNextSceneType)
            StartCoroutine(IELoadLoadingSceneAsync(_eNextSceneType, _bAllowSceneActivation));
        }
        else
        {
            StartCoroutine(IELoadSceneAsync(_eNextSceneType, _bAllowSceneActivation));
        }
    }

    //���� Scene���� �̵�
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

        //���� �� �Ѿ �غ� ��ģ �� �̺�Ʈ�� ��ٸ���
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
                //�󸶳� ����Ǿ����� (�ٷ� �ȳѾ �� 0.9���� ���)
                m_fProgress = m_AsyncOp.progress;
            }
            yield return null;
        }
        yield return null;
    }

    private IEnumerator IELoadLoadingSceneAsync(ESceneType _eNextSceneType, bool _bAllowSceneActivation = true)
    {
        m_AsyncOp = SceneManager.LoadSceneAsync(ESceneType.Loading.ToString());

        //���� �� �Ѿ �غ� ��ģ �� �̺�Ʈ�� ��ٸ���
        m_AsyncOp.allowSceneActivation = true;

        while (true) //SceneLoading (LoadingScene)
        {
            if (m_AsyncOp.isDone)
            {
                break;
            }
            yield return null;
        }

        //LoadingScene�� ����, ���� �� ���
        m_AsyncOp = SceneManager.LoadSceneAsync(_eNextSceneType.ToString());

        //���� �� �Ѿ �غ� ��ģ �� �̺�Ʈ�� ��ٸ���
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
                //�󸶳� ����Ǿ����� (�ٷ� �ȳѾ �� 0.9���� ���)
                m_fProgress = m_AsyncOp.progress;

                fTimeTemp += Time.deltaTime; //�ڷ�ƾ�� �񵿱Ⱑ �ƴ϶� ȣ�� Ƚ���� ���� ������.
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
