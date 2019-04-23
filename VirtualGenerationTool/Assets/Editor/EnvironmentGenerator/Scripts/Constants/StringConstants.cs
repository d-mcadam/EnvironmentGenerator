using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.ComponentModel;

public class StringConstants {

    //constant tags
    public const string BaseTerrainTag = "Base Terrain";

    //constant layers
    public const string ObjectModelTag = "Model";
    public const string BaseObjectTag = "BaseObject";

    //constant strings
    public const string Create = "Create";
    public const string Error = "Error";
    public const string BasicPrefabFilePath = "Assets/Editor/EnvironmentGenerator/Prefabs/BasicPrefabs";
    public const string ModelPrefabFilePath = "Assets/Editor/EnvironmentGenerator/Prefabs/ModelPrefabs";

    //base menu title
    public const string CustomGeneratorTool_MenuTitle = "Custom Generator Tool";

    //generator algorithm text
    public const string GenerateEnvironment_ButtonText = "Generate Environment";
    public const string GenerateEnvironment_WindowTitle = "Select a terrain...";
    
    //object generator text
    public const string GenerateObject_BaseButtonText = "Generate Objects...";
    public const string GenerateObject_OnTerrain_SubButton = "On Terrain...";
    public const string GenerateObject_OnTerrain_WindowTitle = "Generate Objects On A Specified Terrain";
    public const string GenerateObject_LinkByTag_SubButton = "Search by Tag";
    public const string GenerateObject_LinkByName_SubButton = "Search by Name";
    public const string GenerateObject_LinkByObject_SubButton = "Select from Hierarchy";

    //error messages
    public const string Error_ContinousLoopError = "Continous Loop Error Occurred (this is a custom message)";

    //existing object name
    public const string SceneCameraName = "SceneCamera";

    //Model prefab names
    public const string LargeIndustrial = "LargeIndustrial"; //is an L shape building
    public const string MedApartment = "MedApartment";
    public const string MedIndustrial = "MedIndustrial";
    public const string MedOffice = "MedOffice";
    public const string MedOfficeTwo = "MedOffice2";
    public const string SmallOffice = "SmallOffice";
    public const string XLargeOffice = "XLargeOffice"; //is triangular and excessively large compared to other models (currently removed)
    public const string XSmallOffice = "XSmallOffice";


}
