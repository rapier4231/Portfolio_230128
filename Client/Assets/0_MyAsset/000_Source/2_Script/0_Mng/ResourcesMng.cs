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
    private ResourcesMng() { } //생성자를 private로 하여 인스턴스의 생성을 막음

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
            Debug.Log(_strPath + " 오디오 클립이 없습니다.");
        }

        return audioClip;
    }

    public Sprite Get_Sprite(string _strPath)
    {
        Sprite sprite;
        sprite = Resources.Load<Sprite>(_strPath);

        if(sprite == null)
        {
            Debug.Log(_strPath + " 스프라이트가 없습니다.");
        }

        return sprite;
    }

    public GameObject Get_GameObj(string _strPath)
    {
        GameObject GameObj;
        GameObj = Resources.Load<GameObject>(_strPath);

        if (GameObj == null)
        {
            Debug.Log(_strPath + " 게임 오브젝트가 없습니다.");
        }

        return GameObj;
    }

    public VideoClip Get_VideoClip(string _strPath)
    {
        VideoClip videoClip;
        videoClip = Resources.Load<VideoClip>(_strPath);

        if (videoClip == null)
        {
            Debug.Log(_strPath + " 비디오 클립이 없습니다.");
        }

        return videoClip;
    }

    public Object Get_Obj(string _strPath)
    {
        Object objectTemp;
        objectTemp = Resources.Load(_strPath);

        if (objectTemp == null)
        {
            Debug.Log(_strPath + " 오브젝트가 없습니다.");
        }

        return objectTemp;
    }

    public Material Get_Material(string _strPath)
    {
        Material materialTemp;
        materialTemp = Resources.Load<Material>(_strPath);

        if (materialTemp == null)
        {
            Debug.Log(_strPath + " 마테리얼이 없습니다.");
        }

        return materialTemp;
    }

    public Mesh Get_Mesh(string _strPath)
    {
        Mesh meshTemp;
        meshTemp = Resources.Load<Mesh>(_strPath);

        if (meshTemp == null)
        {
            Debug.Log(_strPath + " 메쉬가 없습니다.");
        }

        return meshTemp;
    }
}
