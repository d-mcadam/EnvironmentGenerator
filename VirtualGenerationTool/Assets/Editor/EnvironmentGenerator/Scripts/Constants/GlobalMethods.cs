using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.IO;

public class GlobalMethods {

    private static int _loopFailCount = 0;
    //10 million times, gives acceptable 'lock-out' time
    private const int _maxLoopFail = 10000000;
    
    //should never need modifying
    public static void CreateTagIfNotPresent(string s)
    {

        //get tag manager
        SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty tagsProp = tagManager.FindProperty("tags");
        //may not needs layers
        //SerializedProperty layersProp = tagManager.FindProperty("layers");

        //check if tag exists
        bool found = false;
        for (int i = 0; i < tagsProp.arraySize; i++)
        {
            SerializedProperty t = tagsProp.GetArrayElementAtIndex(i);
            if (t.stringValue.Equals(s)) { found = true; break; }
        }

        //add it if does not exist
        if (!found)
        {
            tagsProp.InsertArrayElementAtIndex(0);
            SerializedProperty newTag = tagsProp.GetArrayElementAtIndex(0);
            newTag.stringValue = s;
        }

        tagManager.ApplyModifiedProperties();

    }

    private static void GenerateObject(Object obj, Vector3 vector)
    {

        //instantiate the prefab as a gameobject
        GameObject prefab = (GameObject)PrefabUtility.InstantiatePrefab(obj);

        //set its world position
        prefab.transform.position = vector;

    }
    
    public static void GenerateObjectsOnTerrain(Terrain terrain, int quantity, Vector3 start_point, Vector3 dimensions)
    {
        //perform a final check on vectors before generating
        start_point = EvaluateStartingPointAgainstTerrain(start_point, terrain);
        dimensions = EvaluateDimensionsAgainstTerrain(start_point, dimensions, terrain);

        for (int i = 0; i < quantity; i++)
        {
            //set loop count
            _loopFailCount = 0;

            //attempt to get a start vector
            VectorBoolReturn startVector = StartingVector(start_point, dimensions, terrain);

            //check if the operation failed
            if (!startVector.OperationSuccess)
            {
                //display a message box detailing to the user what happened
                EditorUtility.DisplayDialog(StringConstants.Error, startVector.Message + "\nFailed to find start vector on loop " + (i+1), "OK");
                return;
            }

            //get all prefans
            Object[] prefabs = GetPrefabs(StringConstants.BasicPrefabFilePath);

            //generate the object, start vector modified to adjust for terrain vector
            GenerateObject(prefabs[0], startVector.Vector);
            
        }
    }

    //this method is currently only used to generate objects identified by tag
    public static void GenerateObjectsOnTerrains(Terrain[] terrains, int quantity, Vector3 start_point, Vector3 dimensions, bool quantityPerTerrain)
    {

        if (quantityPerTerrain)
        {
            for (int i = 0; i < terrains.Length; i++)
            {
                GenerateObjectsOnTerrain(terrains[i], quantity, start_point, dimensions);
            }
        }
        else
        {
            for (int i = 0; i < quantity; i++)
            {
                //can possible add features where they are distributed evenly or more are distributed on one terrain than the other
                GenerateObjectsOnTerrain(terrains[Random.Range(0, terrains.Length)], 1, start_point, dimensions);
            }
        }

    }

    //will change the start point if it is not within terrain boundaries
    public static Vector3 EvaluateStartingPointAgainstTerrain(Vector3 start_point, Terrain terrain)
    {

        Vector3 terrainSize = terrain.terrainData.size;

        if (start_point.x >= terrainSize.x)
            start_point.x = terrainSize.x - 1;

        if (start_point.x < 0)
            start_point.x = 0;

        if (start_point.y >= terrainSize.y)
            start_point.y = terrainSize.y - 1;

        if (start_point.y < 0)
            start_point.y = 0;

        if (start_point.z >= terrainSize.z)
            start_point.z = terrainSize.z - 1;

        if (start_point.z < 0)
            start_point.z = 0;

        return start_point;

    }

    //will change dimensions if it is not within terrain boundaries
    public static Vector3 EvaluateDimensionsAgainstTerrain(Vector3 start_point, Vector3 dimensions, Terrain terrain)
    {

        Vector3 terrainSize = terrain.terrainData.size;

        if (start_point.x + dimensions.x > terrainSize.x)
            dimensions.x = terrainSize.x - start_point.x;

        if (dimensions.x < 1)
            dimensions.x = 1;

        if (start_point.y + dimensions.y > terrainSize.y)
            dimensions.y = terrainSize.y - start_point.y;

        if (dimensions.y < 1)
            dimensions.y = 1;
        
        if (start_point.z + dimensions.z > terrainSize.z)
            dimensions.z = terrainSize.z - start_point.z;

        if (dimensions.z < 1)
            dimensions.z = 1;
        
        return dimensions;

    }


    public static Object[] GetPrefabs(string path)
    {
        //get a list of all the asset file paths (using lists for easy add / remove methods)
        List<string> filePaths = new List<string>();
        foreach (string s in Directory.GetFiles(path))
        {
            filePaths.Add(s);
        }

        //get a collection of file paths to remove (cannot remove from 'filePaths' as Concurrent Modification Exeption thrown)
        List<string> stringsToRemove = new List<string>();
        foreach (string s in filePaths)
        {
            //remove any meta files
            if (s.Contains(".meta"))
                stringsToRemove.Add(s);

            //remove anything that isn't a prefab
            if (!s.Contains(".prefab"))
                stringsToRemove.Add(s);
        }

        //remove the unwanted file paths
        foreach (string s in stringsToRemove)
        {
            filePaths.Remove(s);
        }

        //get all assets as Object's
        Object[] prefabs = new Object[filePaths.Count];
        for (int j = 0; j < prefabs.Length; j++)
        {
            prefabs[j] = AssetDatabase.LoadAssetAtPath(filePaths[j], typeof(GameObject));
        }
        
        //return assets
        return prefabs;
    }

    public static VectorBoolReturn StartingVector(Vector3 start_point, Vector3 dimensions, Terrain terrain)
    {
        //generate a random X and Z coordinate between specified boundaries
        float x = Random.Range(start_point.x, start_point.x + dimensions.x);
        float z = Random.Range(start_point.z, start_point.z + dimensions.z);

        //get the height (Y) of the terrain at the X and Z coordinate
        float y = terrain.SampleHeight(new Vector3(x, 0, z));

        //check if the Y coordinate is outside specified boundaries
        while (y < start_point.y || y > start_point.y + dimensions.y)
        {
            //continue to generate values until the Y coordinate is within boundaries
            x = Random.Range(start_point.x, start_point.x + dimensions.x);
            z = Random.Range(start_point.z, start_point.z + dimensions.z);

            y = terrain.SampleHeight(new Vector3(x, 0, z));

            //exit method if loop limit reached
            if (++_loopFailCount >= _maxLoopFail)
            {
                //return that the operation failed
                return new VectorBoolReturn(false, StringConstants.Error_ContinousLoopError);
            }
        }

        //return the generated starting vector
        return new VectorBoolReturn(new Vector3(x, y, z) + terrain.transform.position);
    }

    //vectors only required to have exactly 2 coordinates identical
    //if the 3rd coordinate was identical, it'd be the same vector
    public static bool VectorsInLine(Vector3 v1, Vector3 v2)
    {
        return (v1.x == v2.x && v1.y == v2.y) || (v1.x == v2.x && v1.z == v2.z) || (v1.y == v2.y && v1.z == v2.z);
    }

    public static List<LineVectorsReturn> SortMeshVerticesToLineArrays(GameObject model)
    {
        //get the models mesh (always getchild at 0) and then its vertices
        Vector3[] vertices = model.transform.GetChild(0).transform.GetComponent<MeshFilter>().sharedMesh.vertices;

        return SortMeshVerticesToLineArrays(vertices);
    }

    public static List<LineVectorsReturn> SortMeshVerticesToLineArrays(Vector3[] vertices)
    {
        //declare a 'master list'
        List<LineVectorsReturn> lineCollection = new List<LineVectorsReturn>();

        foreach (Vector3 vertex in vertices)
        {
            //
            //get all the vectors in line with vertex
            //=======================================

            //declare a list to save any vectors that appear in line with this current one
            List<Vector3> vectorsInLine = new List<Vector3>();

            //get a list of all vectors in line on a single axis with current one
            foreach (Vector3 v in vertices)
            {
                //if its the same vertex, skip it
                if (vertex == v)
                    continue;

                //if vector appears in line, save it
                if (VectorsInLine(vertex, v))
                    vectorsInLine.Add(v);
            }//end of foreach v in vertices

            //
            //sort the vectors in to line arrays
            //==================================

            //declare a list to save filtered line objects
            List<LineVectorsReturn> filteredList = new List<LineVectorsReturn>();

            //declare an initial, default line array to start
            LineVectorsReturn defaultLineArray = new LineVectorsReturn();
            //save the current vertex in this default array
            defaultLineArray.AddVectorToLine(vertex);

            //add the initial default array to filtered list
            filteredList.Add(defaultLineArray);

            //go through all vectors that were found to be in line with current vector
            foreach (Vector3 vector in vectorsInLine)
            {
                //boolean to mark the 'vector' has matched in the 'line'
                bool matchSuccess = false;

                //check the current vector against every array in the filtered list
                foreach (LineVectorsReturn line in filteredList)
                {
                    //find which pair of coordinates match with the default search vertex
                    if (vertex.x == vector.x && vertex.y == vector.y)
                        matchSuccess = VectorMatchesElementsInLine(vector, line, VectorLineCompareType.XY);
                    else if (vertex.y == vector.y && vertex.z == vector.z)
                        matchSuccess = VectorMatchesElementsInLine(vector, line, VectorLineCompareType.YZ);
                    else if (vertex.x == vector.x && vertex.z == vector.z)
                        matchSuccess = VectorMatchesElementsInLine(vector, line, VectorLineCompareType.XZ);
                    else
                    {
                        DisplayError("Should not have reached this part of 'if' statement. Vector should not have been assigned to this list \"vectorsInLine\".");
                        return null;
                    }

                    //if the vector matched with a line array, add it to the same array
                    if (matchSuccess) { line.AddVectorToLine(vector); break; }

                }//end of foreach line in filteredList

                //if, after looping through all of the line arrays in the filtered list, there is still no match,
                //create a new line array object and save it to the filtered list
                if (!matchSuccess) filteredList.Add(new LineVectorsReturn(new Vector3[] { vertex, vector }));

            }//end of foreach vector in vectorsInLine

            //
            //add sorted line arrays to master collection
            //===========================================

            foreach (LineVectorsReturn array in filteredList)
            {
                //create a copy of the array (removes memory pointer)
                LineVectorsReturn copy = new LineVectorsReturn();
                foreach (Vector3 vector in array.Vectors)
                {
                    copy.AddVectorToLine(vector);
                }

                //save the copy
                lineCollection.Add(copy);
            }

        }//end of foreach vertex in vertices

        //
        //sort all line arrays in master collection
        //=========================================
        
        foreach (LineVectorsReturn line in lineCollection)
            line.Sort();

        //
        //remove duplicate line arrays
        //============================

        //collect list of duplicates
        List<LineVectorsReturn> duplicates = new List<LineVectorsReturn>();
        for (int i = 0; i < lineCollection.Count; i++)
            if (!duplicates.Contains(lineCollection[i]))
                for (int j = i + 1; j < lineCollection.Count; j++)
                    if (DuplicateArraysDetected(lineCollection[i], lineCollection[j]))
                        duplicates.Add(lineCollection[j]);

        //remove the duplicates from master list (done this way to avoid concurrent modification exceptions)
        foreach (LineVectorsReturn duplicate in duplicates)
            lineCollection.Remove(duplicate);

        //return the master list
        return lineCollection;
    }

    public static List<Vector3> FindEdgeCorners(List<LineVectorsReturn> edges)
    {
        //find a list of all vectors that appear at the end of lines / edges
        List<Vector3> cornersList = new List<Vector3>();
        foreach (LineVectorsReturn edge in edges)
        {
            if (!cornersList.Contains(edge.Vectors[0]))
                cornersList.Add(edge.Vectors[0]);
            if (!cornersList.Contains(edge.Vectors[edge.Vectors.Count - 1]))
                cornersList.Add(edge.Vectors[edge.Vectors.Count - 1]);
        }

        //find all the identified corners that also appear in the center of a vector array
        List<Vector3> falseVectors = new List<Vector3>();
        foreach (Vector3 vector in cornersList)
            if (!VectorIsACorner(vector, edges))
                falseVectors.Add(vector);

        //remove all the identified corners that also appear in the center of a vector array
        foreach (Vector3 vector in falseVectors)
            cornersList.Remove(vector);

        return cornersList;
    }

    public static List<Vector3> FindModelFaceInAxisDirection(List<Vector3> corners, AxisDirection axis, bool positiveDirection)
    {
        //need to create groups of corners where the Axis Direction are the same
        List<List<Vector3>> vectorCollection = new List<List<Vector3>>();

        //go through each corner vector and group vectors on
        //the same face of the specified axis direction
        foreach (Vector3 v in corners)
        {
            //immediately start the list if its empty
            if (vectorCollection.Count < 1)
            {
                List<Vector3> newList = new List<Vector3>();
                newList.Add(v);
                vectorCollection.Add(newList);
            }
            else
            {
                //go through each list to find where the correct coordinates match
                foreach (List<Vector3> list in vectorCollection)
                {

                    if (list.Count < 1)
                    {
                        DisplayError("A list was detected to be empty, an error has occured with vectorCollection variable");
                        return null;
                    }

                    if (VectorMatchedToList(list[0], v, axis))
                    {
                        //if matched, add the new vector and jump to next loop
                        list.Add(v);
                        goto foundmatch;
                    }

                }//vectorCollection loop

                //only reached if vector was not matched to existing list

                List<Vector3> newList = new List<Vector3>();
                newList.Add(v);
                vectorCollection.Add(newList);

                //=======================================================

            foundmatch:;

            }

        }//corners loop
        
        //sort in decending order (most positive facing side first)
        vectorCollection.Sort(CustomCompareListFirstVector3XCoordinate);
        
        return positiveDirection ? vectorCollection[0] : vectorCollection[vectorCollection.Count - 1];
    }

    public static Vector3 FaceCenterAtGroundLevel(List<Vector3> faceCorners)
    {
        //sort vectors by Y coordinate, to determine which is the lowest point (may not always be 0)
        faceCorners.Sort(CustomCompareVector3YCoordinate);

        //identify and remove the vectors not in line with lowest point
        List<Vector3> vectorsToRemove = new List<Vector3>();
        foreach (Vector3 v in faceCorners)
        {
            if (v.y != faceCorners[0].y)
                vectorsToRemove.Add(v);
        }
        foreach (Vector3 v in vectorsToRemove)
            faceCorners.Remove(v);
        
        float lowerValue = 0.0f, upperValue = 0.0f;
        if (faceCorners[0].x != faceCorners[1].x)
        {
            if (faceCorners[0].x < faceCorners[1].x)
            {
                lowerValue = faceCorners[0].x;
                upperValue = faceCorners[1].x;
            }
            else
            {
                lowerValue = faceCorners[1].x;
                upperValue = faceCorners[0].x;
            }

            float X = ((upperValue - lowerValue) / 2) + lowerValue;
            float Y = faceCorners[0].y;
            float Z = faceCorners[0].z;
            return new Vector3(X, Y, Z);
        }
        else
        {
            if (faceCorners[0].z < faceCorners[1].z)
            {
                lowerValue = faceCorners[0].z;
                upperValue = faceCorners[1].z;
            }
            else
            {
                lowerValue = faceCorners[1].z;
                upperValue = faceCorners[0].z;
            }

            float X = faceCorners[0].x;
            float Y = faceCorners[0].y;
            float Z = ((upperValue - lowerValue) / 2) + lowerValue;
            return new Vector3(X, Y, Z);
        }
    }


    private static bool VectorMatchesElementsInLine(Vector3 vector, LineVectorsReturn line, VectorLineCompareType type)
    {
        foreach (Vector3 v in line.Vectors)
        {
            switch (type)
            {
                case VectorLineCompareType.XY:

                    if (vector.x != v.x || vector.y != v.y)
                        return false;

                    break;
                case VectorLineCompareType.YZ:

                    if (vector.z != v.z || vector.y != v.y)
                        return false;

                    break;
                case VectorLineCompareType.XZ:

                    if (vector.x != v.x || vector.z != v.z)
                        return false;

                    break;
                default:
                    DisplayError("Switch statement hit default in CheckElementsInList");
                    return false;
            }
        }

        return true;
    }

    private static bool DuplicateArraysDetected(LineVectorsReturn l1, LineVectorsReturn l2)
    {
        if (l1.Vectors.Count != l2.Vectors.Count)
            return false;

        for (int i = 0; i < l1.Vectors.Count; i++)
        {
            Vector3 v1 = l1.Vectors[i], v2 = l2.Vectors[i];

            if (v1.x != v2.x || v1.y != v2.y || v1.z != v2.z)
                return false;
        }

        return true;
    }

    private static bool VectorIsACorner(Vector3 vector, List<LineVectorsReturn> edges)
    {
        foreach (LineVectorsReturn edge in edges)
            for (int i = 1; i < edge.Vectors.Count - 1; i++)
                if (EqualVectors(vector, edge.Vectors[i]))
                    return false;

        return true;
    }

    private static bool EqualVectors(Vector3 v1, Vector3 v2)
    {
        return v1.x == v2.x && v1.y == v2.y && v1.z == v2.z;
    }

    private static bool VectorMatchedToList(Vector3 listVector, Vector3 newVector, AxisDirection axis)
    {
        switch (axis)
        {
            case AxisDirection.X:
                return listVector.x == newVector.x;
            case AxisDirection.Y:
                return listVector.y == newVector.y;
            case AxisDirection.Z:
                return listVector.z == newVector.z;
            default:
                return false;
        }
    }


    //sorts in decending order
    private static int CustomCompareListFirstVector3XCoordinate(List<Vector3> l1, List<Vector3> l2)
    {
        if (l1[0].x < l2[0].x)
            return 1;
        else if (l1[0].x == l2[0].x)
            return 0;
        else
            return -1;
    }

    //sorts in ascending order
    private static int CustomCompareVector3YCoordinate(Vector3 v1, Vector3 v2)
    {
        if (v1.y < v2.y)
            return -1;
        else if (v1.y == v2.y)
            return 0;
        else
            return 1;
    }


    private static float HalfOfObjectLengthInXAxis(string objectName)
    {
        switch (objectName)
        {
            case StringConstants.MedIndustrial:
                return 4.73f;
            case StringConstants.MedOffice:
                return 6.54f;
            case StringConstants.XSmallOffice:
                return 8.25f;
            case StringConstants.SmallOffice:
                return 10.7f;
            case StringConstants.MedOfficeTwo:
                return 6.25f;
            case StringConstants.MedApartment:
                return 9.06f;
            case StringConstants.LargeIndustrial:
                return 32.11f;
            case StringConstants.XLargeOffice://not currently being used
                //break;
            default:
                return 0;
        }
    }

    public static Vector3 VectorToMoveObjectInLocalAxis(string previousName, string newName)
    {
        const float largeIndustrialFixedZDistance = 27.52f;
        float x = HalfOfObjectLengthInXAxis(previousName) + HalfOfObjectLengthInXAxis(newName);
        float z = 0.0f;

        switch (newName)
        {
            case StringConstants.MedIndustrial:
                switch (previousName)
                {
                    case StringConstants.MedApartment:
                        z = 0.54f;
                        break;
                    case StringConstants.MedOffice:
                        z = 1.4f;
                        break;
                    case StringConstants.XSmallOffice:
                        z = 0.36f;
                        break;
                    case StringConstants.SmallOffice:
                        z = -2.2f;
                        break;
                    case StringConstants.MedOfficeTwo:
                        z = 0.84f;
                        break;
                    case StringConstants.LargeIndustrial:
                        x = 16.43f;
                        z = largeIndustrialFixedZDistance + HalfOfObjectLengthInXAxis(newName);
                        break;
                    default:
                        break;
                }
                break;
            case StringConstants.MedOffice:
                switch (previousName)
                {
                    case StringConstants.MedApartment:
                        z = -0.88f;
                        break;
                    case StringConstants.MedIndustrial:
                        z = -1.4f;
                        break;
                    case StringConstants.XSmallOffice:
                        z = -1.04f;
                        break;
                    case StringConstants.SmallOffice:
                        z = -3.6f;
                        break;
                    case StringConstants.MedOfficeTwo:
                        z = -0.55f;
                        break;
                    case StringConstants.LargeIndustrial:
                        x = 17.83f;
                        z = largeIndustrialFixedZDistance + HalfOfObjectLengthInXAxis(newName);
                        break;
                    default:
                        break;
                }
                break;
            case StringConstants.XSmallOffice:
                switch (previousName)
                {
                    case StringConstants.MedApartment:
                        z = 0.21f;
                        break;
                    case StringConstants.MedIndustrial:
                        z = -0.36f;
                        break;
                    case StringConstants.SmallOffice:
                        z = -2.56f;
                        break;
                    case StringConstants.MedOfficeTwo:
                        z = 0.48f;
                        break;
                    case StringConstants.MedOffice:
                        z = 1.04f;
                        break;
                    case StringConstants.LargeIndustrial:
                        x = 16.79f;
                        z = largeIndustrialFixedZDistance + HalfOfObjectLengthInXAxis(newName);
                        break;
                    default:
                        break;
                }
                break;
            case StringConstants.SmallOffice:
                switch (previousName)
                {
                    case StringConstants.MedApartment:
                        z = 2.67f;
                        break;
                    case StringConstants.MedIndustrial:
                        z = 2.2f;
                        break;
                    case StringConstants.XSmallOffice:
                        z = 2.56f;
                        break;
                    case StringConstants.MedOfficeTwo:
                        z = 3.04f;
                        break;
                    case StringConstants.MedOffice:
                        z = 3.6f;
                        break;
                    case StringConstants.LargeIndustrial:
                        x = 14.22f;
                        z = largeIndustrialFixedZDistance + HalfOfObjectLengthInXAxis(newName);
                        break;
                    default:
                        break;
                }
                break;
            case StringConstants.MedOfficeTwo:
                switch (previousName)
                {
                    case StringConstants.MedApartment:
                        z = -0.35f;
                        break;
                    case StringConstants.MedIndustrial:
                        z = -0.84f;
                        break;
                    case StringConstants.XSmallOffice:
                        z = -0.48f;
                        break;
                    case StringConstants.SmallOffice:
                        z = -3.04f;
                        break;
                    case StringConstants.MedOffice:
                        z = 0.55f;
                        break;
                    case StringConstants.LargeIndustrial:
                        x = 17.27f;
                        z = largeIndustrialFixedZDistance + HalfOfObjectLengthInXAxis(newName);
                        break;
                    default:
                        break;
                }
                break;
            case StringConstants.MedApartment:
                switch (previousName)
                {
                    case StringConstants.MedIndustrial:
                        z = -0.54f;
                        break;
                    case StringConstants.MedOffice:
                        z = 0.88f;
                        break;
                    case StringConstants.XSmallOffice:
                        z = -0.21f;
                        break;
                    case StringConstants.SmallOffice:
                        z = -2.67f;
                        break;
                    case StringConstants.MedOfficeTwo:
                        z = 0.35f;
                        break;
                    case StringConstants.LargeIndustrial:
                        x = 16.99f;
                        z = largeIndustrialFixedZDistance + HalfOfObjectLengthInXAxis(newName);
                        break;
                    default:
                        break;
                }
                break;
            case StringConstants.LargeIndustrial:
                switch (previousName)
                {
                    case StringConstants.MedIndustrial:
                        z = 9.17f;
                        break;
                    case StringConstants.MedOffice:
                        z = 10.63f;
                        break;
                    case StringConstants.XSmallOffice:
                        z = 9.47f;
                        break;
                    case StringConstants.SmallOffice:
                        z = 6.95f;
                        break;
                    case StringConstants.MedOfficeTwo:
                        z = 10.08f;
                        break;
                    case StringConstants.MedApartment:
                        z = 9.73f;
                        break;
                    case StringConstants.LargeIndustrial:
                        x = 7.27f;
                        z = largeIndustrialFixedZDistance + HalfOfObjectLengthInXAxis(newName);
                        break;
                    default:
                        break;
                }
                break;
            default:
                break;
        }

        return new Vector3(x, 0.0f, z);
    }


    public static void DisplayError(string msg)
    {
        EditorUtility.DisplayDialog(StringConstants.Error, msg, "OK");
    }

    public static Vector3[] TestData
    {
        get
        {
            return new Vector3[] {
           /*A*/new Vector3(-67.3f, 14.2f, 0f), 
           /*B*/new Vector3(-53.1f, 14.2f, 0f), 
           /*C*/new Vector3(-22.3f, 14.2f, 0f),
           /*D*/new Vector3(-67.3f, 0f, 0f), 
           /*E*/new Vector3(-53.1f, 0f, 0f), 
           /*F*/new Vector3(-22.3f, 0f, 0f),
           /*G*/new Vector3(-53.1f, 14.2f, 14.2f), 
           /*H*/new Vector3(-22.3f, 14.2f, 14.2f), 
           /*I*/new Vector3(-53.1f, 0f, 14.2f),
           /*J*/new Vector3(-22.3f, 0f, 14.2f), 
           /*K*/new Vector3(-67.3f, 14.2f, 30.8f), 
           /*L*/new Vector3(-53.1f, 14.2f, 30.8f),
           /*M*/new Vector3(-67.3f, 0f, 30.8f), 
           /*N*/new Vector3(-53.1f, 0f, 30.8f),
            };
        }
    }

}
