using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;

public class VectorBoolReturn {

    private Vector3 _startVector = new Vector3();

    private bool _operationSuccess = true;

    private string _message = "";

    public VectorBoolReturn(Vector3 start_vector)
    {
        _startVector = start_vector;
    }

    public VectorBoolReturn(bool op_success, string message)
    {
        _operationSuccess = op_success;
        _message = message;
    }

    public Vector3 Vector
    {
        get
        {
            return this._startVector;
        }
    }

    public bool OperationSuccess
    {
        get
        {
            return this._operationSuccess;
        }
    }

    public string Message
    {
        get
        {
            return this._message;
        }
    }
    
}
