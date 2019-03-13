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
            VectorBoolReturn startVector = GenerateStartingVector(start_point, dimensions, terrain);

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

    public static VectorBoolReturn GenerateStartingVector(Vector3 start_point, Vector3 dimensions, Terrain terrain)
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

        List<LineVectorsReturn> duplicates = new List<LineVectorsReturn>();
        for (int i = 0; i < lineCollection.Count; i++)
            if (!duplicates.Contains(lineCollection[i]))
                for (int j = i + 1; j < lineCollection.Count; j++)
                    if (DuplicateArraysDetected(lineCollection[i], lineCollection[j]))
                        duplicates.Add(lineCollection[j]);

        //return the master list
        return lineCollection;
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


    public static void DisplayError(string msg)
    {
        EditorUtility.DisplayDialog(StringConstants.Error, msg, "OK");
    }

}
