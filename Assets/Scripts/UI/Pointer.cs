using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Pointer : MonoBehaviour
{
    [SerializeField] RectTransform[] options;
    RectTransform rectTransform;
    int currentPosition;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            ChangePosition(-1);
        }
        else if(Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            ChangePosition(1);
        }

        if(Input.GetKeyDown(KeyCode.Return))
        {
            Interact();
        }
    }

    void ChangePosition(int _change)
    {   
        currentPosition += _change;

        if(currentPosition < 0)
            currentPosition = options.Length - 1;
        else if(currentPosition > options.Length -1)
            currentPosition = 0;

        rectTransform.position = new Vector3(rectTransform.position.x, options[currentPosition].position.y, 0);
    } 

    private void Interact()
    {
        options[currentPosition].GetComponent<Button>().onClick.Invoke();
    }  
}
