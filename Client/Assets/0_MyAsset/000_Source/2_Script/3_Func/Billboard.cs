using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Billboard : CMyUnityBase
{
    public Transform m_CameraTransform;
    public bool m_ReverseLR = true;

    protected override void NullCheck()
    {
        base.NullCheck();

        if(m_CameraTransform == null)
        {
            m_CameraTransform = GameObject.Find("Main Camera").transform;
            if(m_CameraTransform == null)
            {
                Debug.Log(gameObject.name + "의 빌보드 카메라가 없습니다.");
            }
        }
    }

    protected override void Setting()
    {
        base.Setting();

        if(m_ReverseLR)
        {
            Vector3 v3LocalScaleTemp = transform.localScale;
            v3LocalScaleTemp.x *= -1f;
            transform.localScale = v3LocalScaleTemp;
        }
    }

    private void Update()
    {
        transform.LookAt(m_CameraTransform);
    }
}
