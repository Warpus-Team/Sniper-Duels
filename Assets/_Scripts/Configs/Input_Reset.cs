using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ResetInput : MonoBehaviour
{
    public TMP_InputField input;

    void OnEnable()
    {
        input.text = "";
        input.DeactivateInputField();
    }
}