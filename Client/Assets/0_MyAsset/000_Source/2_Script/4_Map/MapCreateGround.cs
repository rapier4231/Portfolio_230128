using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

[CanEditMultipleObjects]
[CustomEditor(typeof(MapCreateGround))]

public class MapCreateGround_Editor : Editor
{
    private void OnSceneGUI()
    {
        MapCreateGround cMapCreateGround = (MapCreateGround)target;
        Transform TempTransform = cMapCreateGround.transform;

        Vector3 v3Temp = TempTransform.localPosition;
        v3Temp.x = (TempTransform.localScale.x * 0.5f) - 0.5f;
        v3Temp.z = (TempTransform.localScale.y * 0.5f) - 0.5f;
        TempTransform.localPosition = v3Temp;
    }
}
#endif 

public class MapCreateGround : CMyUnityBase
{

}
