using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class ModelPreviewEditor : Editor
{

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
    }

    public void Init(Object model)
    {
        Debug.Log(model.name);
    }
    
}
