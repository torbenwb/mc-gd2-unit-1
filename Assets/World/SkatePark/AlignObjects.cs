using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
/*
[CustomEditor(typeof(AlignObjects))]
public class AlignObjectsEditor : Editor{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        AlignObjects a = (AlignObjects)target;

        if (GUILayout.Button("Align")){
            for(int i = 0; i < a.transform.childCount; i++){
                Vector3 p = a.transform.GetChild(i).position;
                p.x = Mathf.RoundToInt(p.x);
                p.z = Mathf.RoundToInt(p.z);
                p.y = Mathf.RoundToInt(p.y);
                a.transform.GetChild(i).position = p;
            }
        }
    }
}

public class AlignObjects : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
*/