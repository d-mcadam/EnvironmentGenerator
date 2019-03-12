using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

public class ToolbarMenuv2 : EditorWindow {

    [MenuItem(StringConstants.CustomGeneratorTool_MenuTitle + "/" +
        StringConstants.GenerateEnvironment_ButtonText, false, 1)]
    static void DisplayGenerateEnvironmentOnTerrainWindow()
    {
        //the specific type parameter at the end docks the window next to the inspector
        GetWindow<GenerateEnvironmentOnTerrainWindow>(StringConstants.GenerateEnvironment_ButtonText, Type.GetType("UnityEditor.InspectorWindow,UnityEditor.dll"));
    }

}
