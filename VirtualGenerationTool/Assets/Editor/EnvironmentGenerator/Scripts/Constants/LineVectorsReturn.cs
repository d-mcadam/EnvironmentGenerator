﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//used to store a list of vectors that represent a vectors in a straight line
public class LineVectorsReturn
{
    //vector list
    private List<Vector3> _vectors = new List<Vector3>();

    //custom add functions
    private bool Add(Vector3 vector)
    {
        _vectors.Add(vector);
        return _vectors.Contains(vector);
    }
    private bool Insert(Vector3 vector, int index)
    {
        _vectors.Insert(index, vector);
        return _vectors[index] == vector;
    }

    //constructors
    public LineVectorsReturn()
    {

    }
    public LineVectorsReturn(List<Vector3> vectors)
    {
        _vectors = vectors;
    }
    public LineVectorsReturn(Vector3[] vectors)
    {
        foreach (Vector3 v in vectors)
            _vectors.Add(v);
    }

    //get vector list for line object
    public List<Vector3> Vectors
    {
        get
        {
            return this._vectors;
        }
    }

    //list array custom modification methods
    //many implement checks that are simply precausions
    public bool AddVectorToLine(Vector3 vector)
    {
        return !_vectors.Contains(vector) && GlobalMethods.VectorsInLine(_vectors[0], vector) ? Add(vector) : false;
    }
    public bool RemoveVectorFromLine(Vector3 vector)
    {
        return _vectors.Contains(vector) ? _vectors.Remove(vector) : false;
    }
    public bool AddVectorToLineAtIndex(Vector3 vector, int index)
    {
        return !_vectors.Contains(vector) && GlobalMethods.VectorsInLine(_vectors[0], vector) ? Insert(vector, index) : false;
    }
    public bool RemoveVectorAtIndex(int index)
    {
        //check the index value is in range
        if (index < 0 || index >= _vectors.Count)
            return false;

        //get a copy of the element
        Vector3 vectorCopy = _vectors[index];

        //remove the element
        _vectors.RemoveAt(index);

        //check the element is NOT still in the list
        return !_vectors.Contains(vectorCopy);
    }
}
