using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class ToolbarMenuBase : ScriptableWizard {
    
    [MenuItem(StringConstants.CustomGeneratorTool_MenuTitle + "/" +
        StringConstants.GenerateObject_BaseButtonText + "/" +
        StringConstants.GenerateObject_OnTerrain_SubButton + "/" +
        StringConstants.GenerateObject_LinkByTag_SubButton, false, 11)]
    static void GenerateObjectOnTerrainByTagWizard()
    {
        DisplayWizard<WindowGenerateObjectByTerrainTag>
            (StringConstants.GenerateObject_OnTerrain_WindowTitle, StringConstants.Create);
    }

    [MenuItem(StringConstants.CustomGeneratorTool_MenuTitle + "/" +
        StringConstants.GenerateObject_BaseButtonText + "/" +
        StringConstants.GenerateObject_OnTerrain_SubButton + "/" +
        StringConstants.GenerateObject_LinkByName_SubButton, false, 12)]
    static void GenerateObjectOnTerrainByNameWizard()
    {
        DisplayWizard<WindowGenerateObjectByTerrainName>
            (StringConstants.GenerateObject_OnTerrain_WindowTitle, StringConstants.Create);
    }

    [MenuItem(StringConstants.CustomGeneratorTool_MenuTitle + "/" +
        StringConstants.GenerateObject_BaseButtonText + "/" +
        StringConstants.GenerateObject_OnTerrain_SubButton + "/" +
        StringConstants.GenerateObject_LinkByObject_SubButton, false, 13)]
    static void GenerateObjectOnTerrainByObjectWizard()
    {
        DisplayWizard<WindowGenerateObjectByTerrainObject>
            (StringConstants.GenerateObject_OnTerrain_WindowTitle, StringConstants.Create);
    }
}
