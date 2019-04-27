using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class GenerateEnvironmentOnTerrainWindow : EditorWindow
{
    //editor to modify game objects
    Editor _objectEditor;

    //target terrain to generate on
    private Terrain _terrainTarget;

    //possibly make this an editable list
    private GenerateWorldTheme _generatorTheme = GenerateWorldTheme.ModernCities;

    private int _maximumNumberOfObjects = 20;
    private int _maximumNumberInSeriesOrCluster = 5;
    private int _maximumNumberOfGroups = 5;

    private int _loopFailCount = 0;
    //1 million times, gives unacceptable 'lock-out' time (will need modifying)
    private const int _maxLoopFail = 10;

    private bool _showLandRestrictionTools = false;
    private bool _landRestrictionDrawingEnabled = false;
    public Rect[] _restrictedAreas;
    private List<GameObject> _restrictedAreaVisualObjects = new List<GameObject>();
    

    public void OnGUI()
    {
        //title
        GUILayout.Label("Virtual Environment Generator Tool ©", EditorStyles.centeredGreyMiniLabel);

        CreateTerrainField();
        CreateDropdownFields();
        CreateMaximumValueFields();

        CreateSpace(5);
        
        CreateLandRestrictionFields();
        
        CreateSpace(4);

        CreateGeneratorButtons();

        CreateSpace(14);
        
        if (GUILayout.Button("test preview window creator"))
        {

            GameObject gameObject = null;

            EditorGUIUtility.ShowObjectPicker<GameObject>(gameObject, false, "", StringConstants.ModelPreviewControlID);
        }

        if (Event.current.commandName == "ObjectSelectorClosed" && 
            EditorGUIUtility.GetObjectPickerObject() != null && 
            EditorGUIUtility.GetObjectPickerControlID() == StringConstants.ModelPreviewControlID)
        {
            if (_objectEditor == null)
                _objectEditor = Editor.CreateEditor(EditorGUIUtility.GetObjectPickerObject());
            
            _objectEditor.OnPreviewGUI(GUILayoutUtility.GetRect(200, 200), EditorStyles.whiteBoldLabel);
        }

    }
    private void CreateSpace(int space)
    {
        for (int i = 0; i < space; i++)
            EditorGUILayout.Space();
    }
    private void CreateTerrainField()
    {

        //terrain object
        GUILayout.Label("Terrain object to target", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;
        _terrainTarget = (Terrain)EditorGUILayout.ObjectField("Terrain Object:", _terrainTarget, typeof(Terrain), true);
        EditorGUI.indentLevel--;

    }
    private void CreateDropdownFields()
    {

        //generation type
        GUILayout.Label("Generation Type", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;
        _generatorTheme = (GenerateWorldTheme)EditorGUILayout.EnumPopup("Select Type:", _generatorTheme);
        EditorGUI.indentLevel--;

    }
    private void CreateMaximumValueFields()
    {

        //limited generator values
        GUILayout.Label("Maximum Generation Values", EditorStyles.boldLabel);

        EditorGUI.indentLevel++;

        _maximumNumberOfObjects = EditorGUILayout.IntField("Maximum Total", _maximumNumberOfObjects);

        _maximumNumberInSeriesOrCluster =
            EditorGUILayout.IntField("Total in " +
                (_generatorTheme == GenerateWorldTheme.ModernCitiesWithStreets ||
                _generatorTheme == GenerateWorldTheme.CityStreets ? "Series" : "Cluster"),
                _maximumNumberInSeriesOrCluster);

        _maximumNumberOfGroups = EditorGUILayout.IntField("Maximum groups", _maximumNumberOfGroups);

        EditorGUI.indentLevel--;

    }
    private void CreateLandRestrictionFields()
    {

        //display foldable section
        _showLandRestrictionTools = EditorGUILayout.Foldout(_showLandRestrictionTools, "Land-restriction drawing tools", true);
        if (_showLandRestrictionTools)
        {
            EditorGUI.indentLevel++;

            EditorGUI.BeginDisabledGroup(!_terrainTarget);

            if (GUILayout.Button((_landRestrictionDrawingEnabled ? "Disable" : "Enable") + " land-restriction drawing tools"))
            {
                _landRestrictionDrawingEnabled = !_landRestrictionDrawingEnabled;

                if (_landRestrictionDrawingEnabled)
                {

                    SceneView view = SceneView.lastActiveSceneView;

                    view.LookAt(_terrainTarget.transform.position, Quaternion.Euler(90.0f, 0.0f, 0.0f));
                    
                    Selection.activeGameObject = _terrainTarget.gameObject;
                    view.FrameSelected();

                    CheckAndDisplayLandRestrictionObjects();

                }
                else
                {
                    DestroyAllLandRestrictionObjects();
                }
            }

            GUILayout.Label("(Consider the Y coordinate to be the Z coordinate)", EditorStyles.boldLabel);
            GUILayout.Label("(Consider the Height value to be the Length value)", EditorStyles.boldLabel);

            EditorGUI.BeginDisabledGroup(!_landRestrictionDrawingEnabled);

            if (GUILayout.Button("Update drawing area"))
            {
                CheckAndDisplayLandRestrictionObjects();
            }

            ScriptableObject target = this;
            SerializedObject so = new SerializedObject(target);
            SerializedProperty vectorProperty = so.FindProperty("_restrictedAreas");

            if (vectorProperty != null)
            {
                EditorGUILayout.PropertyField(vectorProperty, true);
                so.ApplyModifiedProperties();
            }

            EditorGUI.EndDisabledGroup();

            EditorGUI.EndDisabledGroup();

            EditorGUI.indentLevel--;

            if (_landRestrictionDrawingEnabled && !_terrainTarget)
                _landRestrictionDrawingEnabled = false;
            
        }
        else
        {

            _landRestrictionDrawingEnabled = false;
            //could this be called only once when section folds?
            if (_restrictedAreas.Length > 0)
                DestroyAllLandRestrictionObjects();

        }
        //end of foldable section
        
    }
    private void CreateGeneratorButtons()
    {

        //generator buttons
        EditorGUI.BeginDisabledGroup(!_terrainTarget);
        EditorGUI.BeginDisabledGroup(_landRestrictionDrawingEnabled);//unknown reason, would not work with && in duplicate line above
        if (GUILayout.Button(StringConstants.GenerateEnvironment_ButtonText))
        {
            //BasicPrefabGenerationAlgorithm();
            //CalculateModelFaceArea();
        }
        CreateSpace(1);
        if (GUILayout.Button("Generate using models"))
        {
            ModelPrefabGenerationAlgorithm();
        }
        EditorGUI.EndDisabledGroup();
        EditorGUI.EndDisabledGroup();

    }


    private void CheckAndDisplayLandRestrictionObjects()
    {

        if (_restrictedAreaVisualObjects.Count > 0)
            DestroyAllLandRestrictionObjects();

        foreach (Rect area in _restrictedAreas)
        {

            GameObject parent = new GameObject(StringConstants.RestrictedAreas);
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.parent = parent.transform;
            cube.transform.localPosition += new Vector3(0.5f, 0.0f, 0.5f);
            _restrictedAreaVisualObjects.Add(parent);
            
            //change the colour so its easy to see
            cube.GetComponent<Renderer>().sharedMaterial.color = new Color(1f, 0f, 0f);

            //adjust the position relative to specified dimensions and targeted terrain object
            parent.transform.position = _terrainTarget.transform.position + 
                new Vector3(area.x, 0.0f, area.y);

            //adjust the scale accordingly
            parent.transform.localScale = new Vector3(area.width, 1.0f, area.height);

        }

    }
    private void DestroyAllLandRestrictionObjects()
    {

        foreach (GameObject obj in _restrictedAreaVisualObjects)
            DestroyImmediate(obj);
        
    }

    private void CalculateModelFaceArea(int[,] points)
    {
        //int[,] points = { { 1, 1 }, { 1, 3 }, { 6, 3 }, { 6, 1 } };
        
        int n = points.GetLength(0) - 1;
        int sum = 0;

        for (int i = 0; i <= n - 1; i++)
        {
            int val = points[i + 1, 0] * points[i, 1];
            sum += val;
        }

        sum += points[0, 0] * points[n, 1];
        
        for (int i = 0; i <= n - 1; i++)
        {
            int val = points[i, 0] * points[i + 1, 1];
            sum -= val;
        }

        sum -= points[n, 0] * points[0, 1];

        sum /= 2;

        if (sum < 0)
            sum = -sum;

        Debug.Log(sum);
    }
    

    private Vector3 NewModelRelativePosition(GameObject previousModel, GameObject newModel)
    {
        //rotate the object to account for its new position
        newModel.transform.rotation = previousModel.transform.rotation;
        
        //adjust rotation if the previous object was a Large Industrial building 
        if (previousModel.name.Contains(StringConstants.LargeIndustrial))
            newModel.transform.Rotate(0.0f, -90.0f, 0.0f);

        //return the movement vector for the Translate function
        return GlobalMethods.VectorToMoveObjectInLocalAxis(previousModel.name, newModel.name);
    }


    private void ModelPrefabGenerationAlgorithm()
    {
        //get all models
        Object[] models = GlobalMethods.GetPrefabs(StringConstants.ModelPrefabFilePath);

        int groupCount = 0;
        
        //full generation loop
        for (int currentTotal = 0; currentTotal < /*-1*/_maximumNumberOfObjects; currentTotal += 0)
        {
            //select a random model (generator 'seed')
            Object obj = models[Random.Range(0, models.Length)];
            
            //give it a starting position
            VectorBoolReturn startVector = GlobalMethods.StartingVector(new Vector3(), _terrainTarget.terrainData.size, _terrainTarget);
            if (!startVector.OperationSuccess)
            {
                GlobalMethods.DisplayError("Failed to seed environment with initial start vector\n\nLikely an issue with input vector dimensions compared with the Terrain's world vertex coordinates");
                return;
            }

            //instantiate the 'seed' model and initiate the generator
            GameObject newModel = PrefabUtility.InstantiatePrefab(obj) as GameObject;
            newModel.transform.position = startVector.Vector;
            newModel.transform.rotation = Quaternion.Euler(new Vector3(0, Random.Range(0, 360), 0));

            if (!ModelWithinParameters(newModel))
            {
                DestroyImmediate(newModel);
                goto cancelledseries;
            }

            if (newModel.name != StringConstants.LargeIndustrial)
                DuplicateNewModel(obj, newModel);

            //save a reference of the previously generated object
            //groupTotal starts at 1
            GameObject previousModel = newModel;
            //null so it isnt instantiated as an empty game object
            GameObject previousDuplicate = null;

            //series generation loop
            for (int seriesQuantity = 1; seriesQuantity < _maximumNumberInSeriesOrCluster; seriesQuantity++)
            {
                //check if we have reached the maximum, exit full generation loop if so
                if (seriesQuantity + currentTotal >= _maximumNumberOfObjects)
                    goto finishedgeneration;

                //continue selecting models to generate in series
                Object newObject = models[Random.Range(0, models.Length - 1)];

                //check if new model fits parameters
                _loopFailCount = 0;
                bool loopSuccess = false;
                do
                {
                    //exit method if loop limit reached, an error has occurred
                    if (++_loopFailCount > _maxLoopFail)
                    {
                        Debug.Log("model loop parameter failure");
                        currentTotal += seriesQuantity;
                        groupCount++;
                        goto cancelledseries;
                    }

                    //select and instantiate model
                    newObject = models[Random.Range(0, models.Length - 1)];
                    newModel = PrefabUtility.InstantiatePrefab(newObject) as GameObject;

                    //initialise the position to match the previous model
                    newModel.transform.position = previousModel.transform.position;

                    //move the object to the new relative position
                    newModel.transform.Translate(NewModelRelativePosition(previousModel, newModel), previousModel.transform);
                    
                    //check whether parameters matched
                    if (ModelWithinParameters(previousModel, newModel))
                    {
                        loopSuccess = true;
                        //save reference to this model
                        previousModel = newModel;

                        //used to set previousDuplicate to null if required
                        GameObject duplicate = null;

                        //check whether we can generate a duplicate
                        if (newModel.name != StringConstants.LargeIndustrial)
                        {
                            //generate a duplicate
                            duplicate = DuplicateNewModel(newObject, newModel);
                            
                            //check if the duplicate satisfies parameters
                            if (previousDuplicate == null)
                            {
                                if (!ModelWithinParameters(duplicate))
                                {
                                    DestroyImmediate(duplicate);
                                }
                            }
                            else
                            {
                                if (!ModelWithinParameters(previousDuplicate, duplicate))
                                {
                                    DestroyImmediate(duplicate);
                                }
                            }

                        }

                        previousDuplicate = duplicate;

                        //currently doesnt work, left code as is to try in future
                        //RelativeMoveDuplicate(); 
                    }
                    else
                    {
                        DestroyImmediate(newModel);
                    }

                } while (!loopSuccess);
                
            }//end of _maximumNumberInSeriesOrCluster loop - series generation loop

            currentTotal += _maximumNumberInSeriesOrCluster;
            groupCount++;

        cancelledseries:;
            
            if (groupCount >= _maximumNumberOfGroups)
                goto finishedgeneration;

        }//end of  _maximumNumberOfObjects loop - full generation loop

    finishedgeneration:;

        //kill any invisible models
        foreach (GameObject o in Resources.FindObjectsOfTypeAll(typeof(GameObject)))
        {
            if (o.hideFlags == HideFlags.HideInHierarchy)
            {
                Debug.Log("found '" + o.name + "'. Destroying...");
                DestroyImmediate(o);
            }
        }

    }
    private GameObject DuplicateNewModel(Object newObject, GameObject newModel)
    {

        //duplicate model
        GameObject duplicate = (GameObject)PrefabUtility.InstantiatePrefab(newObject);

        //intersecting duplicates is okay (for now), this will identify them
        duplicate.name += StringConstants.DuplicateText;

        //adjust position of model
        duplicate.transform.position = newModel.transform.position;
        duplicate.transform.rotation = newModel.transform.rotation;

        //translate to opposing sides
        duplicate.transform.Translate(new Vector3(0.0f, 0.0f, 35.0f));//will need adjusting as testing goes on
        duplicate.transform.Rotate(new Vector3(0.0f, 180.0f, 0.0f));
        
        return duplicate;

    }

    private bool InitialCheckIntersectionForSeriesStart(GameObject newObj)
    {
        foreach (GameObject sceneObj in GetAllSceneModels())
        {
            if (sceneObj == newObj)
                continue;

            MeshRenderer sceneObjMesh = sceneObj.transform.GetChild(0).GetComponent<MeshRenderer>();
            MeshRenderer newObjMesh = newObj.transform.GetChild(0).GetComponent<MeshRenderer>();

            if (newObjMesh.bounds.Intersects(sceneObjMesh.bounds))
                return false;

        }

        return true;
    }

    private bool CheckIntersectionForNewObjectInSeries(GameObject prevObj, GameObject newObj)
    {
        foreach (GameObject sceneObj in GetAllSceneModels())
        {
            if (sceneObj == newObj || sceneObj == prevObj)
                continue;
            
            MeshRenderer sceneObjMesh = sceneObj.transform.GetChild(0).GetComponent<MeshRenderer>();
            MeshRenderer newObjMesh = newObj.transform.GetChild(0).GetComponent<MeshRenderer>();

            if (newObjMesh.bounds.Intersects(sceneObjMesh.bounds))
                return false;
            
        }
        
        return true;
    }
    
    private bool ModelWithinParameters(GameObject newObject)
    {
        return WithinParameters(newObject) && InitialCheckIntersectionForSeriesStart(newObject);
    }

    private bool ModelWithinParameters(GameObject previousObject, GameObject newObject)
    {
        return WithinParameters(newObject) && CheckIntersectionForNewObjectInSeries(previousObject, newObject);
    }

    private bool WithinParameters(GameObject newObject)
    {
        //get the models mesh (always getchild at 0) and then its vertices
        Vector3[] vertices = newObject.transform.GetChild(0).transform.GetComponent<MeshFilter>().sharedMesh.vertices;

        //check object is within the X and Z bounds of the terrain object
        foreach (Vector3 vertex in vertices)
        {
            if (!VertexWithinTerrainBounds(newObject.transform.TransformPoint(vertex)) || !VertexWithinRectBounds(newObject.transform.TransformPoint(vertex)))
            {
                return false;
            }
        }

        return true;
    }

    private bool VertexWithinTerrainBounds(Vector3 vertex)
    {
        //the Vertex X and Z coordinates need to be within the bounds of the terrain
        //
        //ie. greater than the starting X and Z position of the terrain but less than
        //the starting point plus the dimension along its respective axis
        //
        return vertex.x > _terrainTarget.transform.position.x && vertex.x < _terrainTarget.transform.position.x + _terrainTarget.terrainData.size.x &&
            vertex.z > _terrainTarget.transform.position.z && vertex.z < _terrainTarget.transform.position.z + _terrainTarget.terrainData.size.z;
    }

    private bool VertexWithinRectBounds(Vector3 vertex)
    {
        if (_restrictedAreas.Length < 1)
            return true;

        foreach (Rect rect in _restrictedAreas)
        {
            if (vertex.x > _terrainTarget.transform.position.x + rect.x && vertex.x < _terrainTarget.transform.position.x + rect.x + rect.width &&
                vertex.z > _terrainTarget.transform.position.z + rect.y && vertex.z < _terrainTarget.transform.position.z + rect.y + rect.height)
            {
                return true;
            }
            
        }
        
        return false;
    }
    

    private List<GameObject> GetAllSceneModels()
    {

        List<GameObject> objectsInScene = new List<GameObject>();

        foreach (GameObject o in Resources.FindObjectsOfTypeAll(typeof(GameObject)))
        {
            if (o.hideFlags == HideFlags.NotEditable || o.hideFlags == HideFlags.HideAndDontSave)
                continue;

            if (EditorUtility.IsPersistent(o.transform.root.gameObject))
                continue;

            if (o.layer == LayerMask.NameToLayer(StringConstants.BaseObjectTag))
                objectsInScene.Add(o);
        }

        return objectsInScene;

    }


    private Vector3 BasicPrefabNewRelativeObjectPosition(GameObject newObj, GameObject oldObj)
    {
        float newObjectLength = 0;
        float oldObjectLength = 0;

        try
        {
            newObjectLength = newObj.transform.GetChild(0).transform.lossyScale.x / 2;
        }
        catch (UnityException e)
        {
            newObjectLength = newObj.transform.lossyScale.x / 2;
        }

        try
        {
            oldObjectLength = oldObj.transform.GetChild(0).transform.lossyScale.x / 2;
        }
        catch (UnityException e)
        {
            oldObjectLength = oldObj.transform.lossyScale.x / 2;
        }

        return new Vector3(newObjectLength + oldObjectLength, 0, 0);
    }


    private void BasicPrefabGenerationAlgorithm()
    {
        //get an array of all the prefabs
        Object[] prefabs = GlobalMethods.GetPrefabs(StringConstants.BasicPrefabFilePath);

        switch (_generatorTheme)
        {
            case GenerateWorldTheme.CityStreets:
                BasicPrefabGenerateCityStreets(prefabs);
                break;
            default:
                GlobalMethods.DisplayError("Switch statement 'default' hit");
                break;
        }
        
    }
    private void BasicPrefabGenerateCityStreets(Object[] prefabs)
    {
        for (int currentTotalObjects = 0; currentTotalObjects < _maximumNumberOfObjects; currentTotalObjects += 0)
        {
            //choose object (this is effectively a 'seed' for the generator)
            Object obj = prefabs[Random.Range(0, prefabs.Length - 1)];
            VectorBoolReturn startVector = GlobalMethods.StartingVector(new Vector3(), _terrainTarget.terrainData.size, _terrainTarget);

            if (!startVector.OperationSuccess)
            {
                GlobalMethods.DisplayError("Failed to seed environment with initial start vector\n\nLikely an issue with input vector dimensions compared with the Terrain's vertice coordinates");
                return;
            }

            //initialise a game object variable and instantiate first object
            GameObject prefab = (GameObject)PrefabUtility.InstantiatePrefab(obj);
            prefab.transform.position = startVector.Vector;
            prefab.transform.rotation = Quaternion.Euler(new Vector3(0, Random.Range(0, 360), 0));
            if (prefab.tag == "Cylinder")
            {
                prefab.transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, 90f));
                prefab.transform.Translate(new Vector3(prefab.transform.lossyScale.x / 2, 0f, 0f));
            }

            //save reference to first generated object
            GameObject previousPrefab = prefab;

            //starts at 'one' as the 'seed' is the initial object
            for (int currentSeriesTotal = 1; currentSeriesTotal < _maximumNumberInSeriesOrCluster; currentSeriesTotal++)
            {
                if (currentTotalObjects + currentSeriesTotal >= _maximumNumberOfObjects)
                    goto outerloop;

                _loopFailCount = 0;

                //choose new object
                Object newObject = prefabs[Random.Range(0, prefabs.Length - 1)];

                //check if it satisfies restrictions
                while (!BasicPrefabObjectWithinParameters(previousPrefab, newObject))
                {
                    //if it does NOT satisfy restrictions, choose new object again
                    newObject = prefabs[Random.Range(0, prefabs.Length - 1)];

                    //exit method if loop limit reached, an error has occurred
                    if (++_loopFailCount >= _maxLoopFail)
                    {
                        GlobalMethods.DisplayError(StringConstants.Error_ContinousLoopError);
                        return;
                    }
                }

                //instantiate the new object
                prefab = (GameObject)PrefabUtility.InstantiatePrefab(newObject);
                //set the original position
                prefab.transform.position = previousPrefab.transform.position;
                //translate the position relative to the previous objects rotation
                prefab.transform.Translate(BasicPrefabNewRelativeObjectPosition(prefab, previousPrefab), previousPrefab.transform);
                //rotate the new object to line up with others
                prefab.transform.rotation = previousPrefab.transform.rotation;

                //save a copy
                previousPrefab = prefab;

            }//end of _maxSeriesQuantity

            currentTotalObjects += _maximumNumberInSeriesOrCluster;

        }//end of _maxObjectQuantity

        outerloop:;//used to break from nested loop if maximum quantity reached

    }


    private bool BasicPrefabObjectWithinParameters(GameObject previousObject, Object obj)
    {
        GameObject newObject = (GameObject)obj;

        bool colourCondition = BasicPrefabWithinColourParameters(previousObject, newObject);

        bool sizeCondition = BasicPrefabWithinSizeParameters(previousObject, newObject);

        return colourCondition && sizeCondition;
    }
    private bool BasicPrefabWithinColourParameters(GameObject previousObject, GameObject newObject)
    {
        Color previousObjectColour = Color.white;
        Color newObjectColour = Color.white;

        try
        {
            previousObjectColour = previousObject.GetComponent<MeshRenderer>().sharedMaterial.color;
        }
        catch (MissingComponentException e)
        {
            previousObjectColour = previousObject.transform.GetChild(0).GetComponent<MeshRenderer>().sharedMaterial.color;
        }

        try
        {
            newObjectColour = newObject.GetComponent<MeshRenderer>().sharedMaterial.color;
        }
        catch (MissingComponentException e)
        {
            newObjectColour = newObject.transform.GetChild(0).GetComponent<MeshRenderer>().sharedMaterial.color;
        }

        return  //the colour for yellow is slightly different to the custom defined material colour for yellow
            (previousObjectColour == Color.red && newObjectColour != Color.blue) ||
            (previousObjectColour == Color.blue && newObjectColour != Color.red) ||
            (previousObjectColour == Color.green && newObjectColour != new Color(1.0f, 1.0f, 0.0f)) ||
            (previousObjectColour == new Color(1.0f, 1.0f, 0.0f) && newObjectColour != Color.green);
    }
    private bool BasicPrefabWithinSizeParameters(GameObject previousObject, GameObject newObject)
    {



        return true;
    }




    private void RelativeMoveDuplicate()
    {

        //GameObject previousDuplicate = null;

        ////used to check how many times large industrial building was generated
        //int uniqueConditionCount = 0;

        ////general rule #1
        ////if generated large industrial, skip opposing building this turn,
        ////but be sure to adjust the next opposing model accordingly
        //if (newModel.name != StringConstants.LargeIndustrial)
        //{
        //    GameObject duplicate = DuplicateNewModel(newObject, newModel);

        //    //general rule #2
        //    //if the previous model was a large industrial building (so an opposing model wasnt generated last loop),
        //    //adjust this models position accordingly (must account for consecutive large industrial buildings 
        //    //and any previous duplicate size)

        //    if (previousModel.name == StringConstants.LargeIndustrial)
        //    {
        //        if (previousDuplicate == null)
        //            goto generatenormally;

        //        //get all the model corners for the previous duplicate
        //        List<Vector3> modelCorners = GlobalMethods.FindEdgeCorners(GlobalMethods.SortMeshVerticesToLineArrays(previousDuplicate));

        //        //determine facing direction
        //        AxisDirection direction = AxisDirection.X;
        //        bool positiveDirection = false;
        //        if (uniqueConditionCount % 4 == 0)
        //        {

        //            positiveDirection = false;
        //            direction = AxisDirection.X;

        //        }
        //        else if (uniqueConditionCount % 3 == 0)
        //        {

        //            positiveDirection = false;
        //            direction = AxisDirection.Z;

        //        }
        //        else if (uniqueConditionCount % 2 == 0)
        //        {

        //            positiveDirection = true;
        //            direction = AxisDirection.X;

        //        }
        //        else //assume: uniqueConditionCount % 1 == 0
        //        {

        //            positiveDirection = true;
        //            direction = AxisDirection.Z;

        //        }

        //        List<Vector3> modelFace = GlobalMethods.FindModelFaceInAxisDirection(modelCorners, direction, positiveDirection);

        //        switch (direction)
        //        {
        //            case AxisDirection.X:
        //                duplicate.transform.position =
        //                    new Vector3(
        //                        previousDuplicate.transform.TransformPoint(modelFace[0]).x,
        //                        duplicate.transform.position.y,
        //                        duplicate.transform.position.z);
        //                break;
        //            case AxisDirection.Z:
        //                duplicate.transform.position =
        //                    new Vector3(
        //                        duplicate.transform.position.x,
        //                        duplicate.transform.position.y,
        //                        previousDuplicate.transform.TransformPoint(modelFace[0]).z);
        //                break;
        //            default:
        //                break;
        //        }

        //    }

        //generatenormally:;

        //    previousDuplicate = duplicate;

        //    uniqueConditionCount = 0;

        //}
        //else
        //{
        //    uniqueConditionCount++;
        //}
    }

}
