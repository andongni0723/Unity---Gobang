using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Gobang_ErrorManager : MonoBehaviour
{
    public static Gobang_ErrorManager instance;

    public GameObject errorPanel_Obj;
    public TextMeshProUGUI errorText_T;

    private void Awake()
    {
        instance = this;
    }

    public void SentErrorMessage(string errorMessage)
    {
        errorPanel_Obj.SetActive(true);
        errorText_T.text = errorMessage;
    }
}
