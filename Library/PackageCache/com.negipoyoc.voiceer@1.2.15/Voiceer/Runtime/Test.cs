using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class Test : MonoBehaviour
{
    [SerializeField] bool createError;
    [SerializeField] bool printMessage;

    private void Update()
    {
        if (createError)
        {
            createError = false;

            Debug.LogError("Error!", gameObject);
        }

        if (printMessage)
        {
            printMessage = false;

            Debug.Log("Here's a regular Debug.Log message", gameObject);
        }
    }
}
