using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Logger : MonoBehaviour
{
    [SerializeField]
    public Text m_Text;
    public static Logger instance;

    void Awake()
    {
        instance = this;
    }

    public static void Log(string text)
    {
        if (instance != null)
            instance.m_Text.text = text + "\n" + instance.m_Text.text;
    }
}
