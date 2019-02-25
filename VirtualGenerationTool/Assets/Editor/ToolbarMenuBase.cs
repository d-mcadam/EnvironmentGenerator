using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class ToolbarMenuBase : ScriptableWizard {

	[MenuItem(StringConstants.CustomGeneratorTool_MenuTitle + "/" + StringConstants.GenerateTerrain_ButtonText, false, 2)]
    static void GenerateTerrainWizard()
    {
        ScriptableWizard.DisplayWizard<WindowGenerateTerrain>(StringConstants.GenerateTerrain_WindowTitle, StringConstants.Create);
    }

    [MenuItem(StringConstants.CustomGeneratorTool_MenuTitle + "/" +
        StringConstants.GenerateObject_BaseButtonText + "/" +
        StringConstants.GenerateObject_OnTerrain_SubButton + "/" +
        StringConstants.GenerateObject_LinkByTag_SubButton)]
    static void GenerateObjectOnTerrainByTagWizard()
    {

    }

    [MenuItem(StringConstants.CustomGeneratorTool_MenuTitle + "/" +
        StringConstants.GenerateObject_BaseButtonText + "/" +
        StringConstants.GenerateObject_OnTerrain_SubButton + "/" +
        StringConstants.GenerateObject_LinkByName_SubButton)]
    static void GenerateObjectOnTerrainByNameWizard()
    {

    }

    [MenuItem(StringConstants.CustomGeneratorTool_MenuTitle + "/" +
        StringConstants.GenerateObject_BaseButtonText + "/" +
        StringConstants.GenerateObject_OnTerrain_SubButton + "/" +
        StringConstants.GenerateObject_LinkByObject_SubButton)]
    static void GenerateObjectOnTerrainByObjectWizard()
    {

    }
}
