using UnityEngine;
using UnityEngine.UI;

public class TestUI : MonoBehaviour
{

    GameObject original;
    GameObject openFileDialog;
    Dialog dialog;

    // Use this for initialization
    void Start()
    {
        GameObject.Find("Button").GetComponent<Button>().onClick.AddListener(() => { StartExperiment(); });
        StartExperiment();
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void  onButtonClick()
    {
        StartExperiment();
    }

    void StartExperiment()
    {
        GameObject textGameObject = GameObject.Find("TextResult");
        original = Resources.Load<GameObject>("FileOpenDialog/Prefab/OpenDialog");

        openFileDialog = Instantiate<GameObject>(original);
        dialog = openFileDialog.GetComponent<Dialog>();

        dialog.OpenFileDialog("Try Opening a txt file", @"C:\Users", ".txt", OnDialogComplete);
        textGameObject.GetComponent<Text>().text = "Press this button to try again.";
    }

    void OnDialogComplete(bool isSucessful, string path)
    {
        GameObject textGameObject = GameObject.Find("TextResult");

        if (isSucessful)
        {
            Debug.Log("Path : " + path);
            textGameObject.GetComponent<Text>().text = "Selected :" + "\"" + path + "\"";
        }
        else
        {
            Debug.Log("Not successful");
            textGameObject.GetComponent<Text>().text = "No File was selected. Press this button to try again.";
        }
    }
}
