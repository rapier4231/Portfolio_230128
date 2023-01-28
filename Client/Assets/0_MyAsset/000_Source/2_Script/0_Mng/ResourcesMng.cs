using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class ResourcesMng : MonoBehaviour
{
    private static ResourcesMng m_Instance = null;
    public static ResourcesMng Instance
    {
        get
        {
            if (ReferenceEquals(m_Instance, null))
            {
                GameObject instance = new GameObject("Resources Manager");
                m_Instance          = instance.AddComponent<ResourcesMng>();
                m_Instance.Init();
                DontDestroyOnLoad(instance);
            }
            return m_Instance;
        }
    }
    private ResourcesMng() { } //�����ڸ� private�� �Ͽ� �ν��Ͻ��� ������ ����

    //////////////// //Singleton// ////////////////

    //private string m_strPath = "";

    private void Init()
    {
        //m_strPath = "";
    }

    public AudioClip Get_AudioClip(string _strPath)
    {
        AudioClip audioClip;
        audioClip = Resources.Load<AudioClip>(_strPath);

        if(audioClip == null)
        {
            Debug.Log(_strPath + " ����� Ŭ���� �����ϴ�.");
        }

        return audioClip;
    }

    public Sprite Get_Sprite(string _strPath)
    {
        Sprite sprite;
        sprite = Resources.Load<Sprite>(_strPath);

        if(sprite == null)
        {
            Debug.Log(_strPath + " ��������Ʈ�� �����ϴ�.");
        }

        return sprite;
    }

    public GameObject Get_GameObj(string _strPath)
    {
        GameObject GameObj;
        GameObj = Resources.Load<GameObject>(_strPath);

        if (GameObj == null)
        {
            Debug.Log(_strPath + " ���� ������Ʈ�� �����ϴ�.");
        }

        return GameObj;
    }

    public VideoClip Get_VideoClip(string _strPath)
    {
        VideoClip videoClip;
        videoClip = Resources.Load<VideoClip>(_strPath);

        if (videoClip == null)
        {
            Debug.Log(_strPath + " ���� Ŭ���� �����ϴ�.");
        }

        return videoClip;
    }

    public Object Get_Obj(string _strPath)
    {
        Object objectTemp;
        objectTemp = Resources.Load(_strPath);

        if (objectTemp == null)
        {
            Debug.Log(_strPath + " ������Ʈ�� �����ϴ�.");
        }

        return objectTemp;
    }

    public Material Get_Material(string _strPath)
    {
        Material materialTemp;
        materialTemp = Resources.Load<Material>(_strPath);

        if (materialTemp == null)
        {
            Debug.Log(_strPath + " ���׸����� �����ϴ�.");
        }

        return materialTemp;
    }

    public Mesh Get_Mesh(string _strPath)
    {
        Mesh meshTemp;
        meshTemp = Resources.Load<Mesh>(_strPath);

        if (meshTemp == null)
        {
            Debug.Log(_strPath + " �޽��� �����ϴ�.");
        }

        return meshTemp;
    }
}
