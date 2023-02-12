using System.Collections;
using UnityEngine;

public class RotationCoroutine : CMyUnityBase
{
    //회전하는데 걸리는 시간 설정
    public  float m_RotationTime    = 0.2f;
    private float m_fDesiredAngle   = 90f;

    Coroutine m_RotCoroutine;

    public void Set_Rotation(float _fDesiredAngle)
    {
        if (m_RotCoroutine != null)
        {
            StopCoroutine(m_RotCoroutine);
        }

        m_fDesiredAngle = _fDesiredAngle;

        m_RotCoroutine = StartCoroutine(RotateObject());
    }

    private IEnumerator RotateObject()
    {
        float       fElapsedTime    = 0f;
        Quaternion  QStartRotation  = transform.rotation;
        Quaternion  QEndRotation    = Quaternion.Euler(transform.eulerAngles + Vector3.up * m_fDesiredAngle);

        while (fElapsedTime < m_RotationTime)
        {
            transform.rotation = Quaternion.Slerp(QStartRotation, QEndRotation, (fElapsedTime / m_RotationTime));
            yield return null;
            fElapsedTime += Time.deltaTime;
        }
        transform.rotation = QEndRotation;
    }

}
