using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

//TODO : Credit the following from Noun Project
//Folder Icon Folder_Arthur_Shlain
//File Icon File_Joe_Harrison
//Arrow Up http://www.kenney.nl/ Kenny Game Assets (Steam)

public class Dialog : MonoBehaviour
{
    //Default Values
    const float deselectedAlpha = 0.5f;
    const float selectedAlpha = 1f;
    const string defaultTitle = "Open File...";
    const string defaultExtension = "*";
    const string iconResourcePath = "FileOpenDialog/Icons/";

    //Child & Component Objects
    RectTransform rectTransform;
    Text TextTitle;
    Button ButtonBack;
    InputField InputFieldPath;
    GameObject PanelContent;
    Button ButtonOK;
    Button ButtonCancel;

    //List to store file/directory buttons
    List<GameObject> items;
    string selectedItem;
    int selectedIndex;

    DirectoryInfo currDir;

    //To check if it's a directory or a file to be selected
    bool isOpenFileDialog;

    string extension;

    public delegate void OnDialogComplete(bool isSuccessful, string itemPath);
    OnDialogComplete eventHandler;

    //Because Start didn't work
    bool isSetupComplete = false;

    void Setup()
    {
        if (!isSetupComplete)
        {
            isSetupComplete = true;

            //Get Child Objects
            rectTransform = GetComponent<RectTransform>();
            TextTitle = transform.GetChild(0).gameObject.GetComponentInChildren<Text>();
            ButtonBack = transform.GetChild(1).gameObject.GetComponent<Button>();
            InputFieldPath = transform.GetChild(2).gameObject.GetComponent<InputField>();
            PanelContent = transform.GetChild(3).gameObject.transform.GetChild(0).gameObject;
            ButtonOK = transform.GetChild(4).gameObject.GetComponent<Button>();
            ButtonCancel = transform.GetChild(5).gameObject.GetComponent<Button>();

            //Default parameters
            isOpenFileDialog = true;
            extension = "*";
            items = new List<GameObject>();

            //Set event handlers
            InputFieldPath.onEndEdit.AddListener((newPath) => { OnInputTextChanged(newPath); });
            ButtonOK.onClick.AddListener(() => { ButtonOkHandler(); });
            ButtonCancel.onClick.AddListener(() => { ButtonCancelHandler(); });
            ButtonBack.onClick.AddListener(() => { GoUpOneDirectory(); });

            //Start at root by default
            selectedItem = "";
            selectedIndex = -1;
            ButtonOK.interactable = false;
            currDir = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.Desktop));

            //Set Transform and parenting
            transform.SetParent(GameObject.Find("Canvas").transform);
            transform.SetAsLastSibling();
            rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 0, 0);
            rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 0, 0);
            rectTransform.localScale = Vector3.one;
            rectTransform.anchorMin = new Vector2(0f, 0f);
            rectTransform.anchorMax = new Vector2(1f, 1f);
        }
    }

    public void OpenFileDialog(string title, string startLocation, string extensionWithDot, OnDialogComplete OnDialogCompleteHandler)
    {
        Setup();
        eventHandler = OnDialogCompleteHandler;
        
        TextTitle.text = (title != null && title.Length > 0) ? title : defaultTitle;
        isOpenFileDialog = true;
        //TODO : Clean extension inputs
        extension = (extensionWithDot != null && extensionWithDot.Length > 0) ? extensionWithDot : defaultExtension;

        gameObject.SetActive(true);
        StartCoroutine(OpenDirectory(startLocation));
    }

    public void OpenFolderDialog(string title, string startLocation, OnDialogComplete OnDialogCompleteHandler)
    {
        Setup();
        eventHandler = OnDialogCompleteHandler;
        
        TextTitle.text = (title != null && title.Length > 0) ? title : defaultTitle;
        isOpenFileDialog = false;
        extension = defaultExtension;

        gameObject.SetActive(true);
        StartCoroutine(OpenDirectory(startLocation));
    }

    void GoUpOneDirectory()
    {
        if (currDir.Parent != null)
        {
            StartCoroutine(OpenDirectory(currDir.Parent.FullName));
        }
    }

    IEnumerator OpenDirectory(string path)
    {
        DirectoryInfo[] dirs;
        FileInfo[] files;
        GameObject tempDirGameObject;
        GameObject tempFileGameObject;
        int index;

        //Open Location
        if (Directory.Exists(path))
        {
            currDir = new DirectoryInfo(path);
        }
        else
        {
            InputFieldPath.text = currDir.FullName;
            yield break;
        }

        InputFieldPath.text = currDir.FullName;

        //Cleanup
        foreach (GameObject item in items)
        {
            Destroy(item);
        }
        items.Clear();

        yield return 0;


        //Add Directories to list
        dirs = currDir.GetDirectories();
        index = 0;
        foreach (DirectoryInfo dir in dirs)
        {
            if (hasAccessPermission(dir.FullName))
            {
                tempDirGameObject = CreateButton(dir, index);
                tempDirGameObject.transform.SetParent(PanelContent.transform);
                tempDirGameObject.GetComponent<RectTransform>().localScale = Vector3.one;
                items.Add(tempDirGameObject);

                index++;
                if ((index + 1) % 100 == 0)
                {
                    yield return 0;
                }

                //Not too many directories!
                if (index > 1000)
                {
                    break;
                }
            }
        }

        //Add Files to list
        files = currDir.GetFiles("*" + extension);
        foreach (FileInfo file in files)
        {
            tempFileGameObject = CreateButton(file, index);
            tempFileGameObject.transform.SetParent(PanelContent.transform);
            tempFileGameObject.GetComponent<RectTransform>().localScale = Vector3.one;
            items.Add(tempFileGameObject);

            index++;
            if ((index + 1) % 100 == 0)
            {
                yield return 0;
            }

            //Not too many files!
            if (index > 2000)
            {
                break;
            }
        }

        //Reset Selected
        OnItemDeselectHandler();

        yield return null;
    }

    bool hasAccessPermission(string itemFullName)
    {
        DirectoryInfo itemInfo = null;

        try
        {
            if (Directory.Exists(itemFullName))
            {
                itemInfo = new DirectoryInfo(itemFullName);
                itemInfo.GetDirectories();
            }

            return true;
        }
        catch (Exception)
        {
            itemInfo = null;
        }

        return (itemInfo != null);
    }

    Sprite LoadIcon(String fullName)
    {
        //TODO : Get Icons Dynamically
        return File.Exists(fullName) ? Resources.Load<Sprite>(iconResourcePath + "FileIcon") : Resources.Load<Sprite>(iconResourcePath + "FolderIcon");
    }

    GameObject CreateButton(FileSystemInfo info, int index)
    {
        GameObject item;
        GameObject icon;
        GameObject label;

        RectTransform rectTransform;
        Button button;
        Color color;

        //Create GameObjects
        item = new GameObject(File.Exists(info.FullName) ? "File" : "Directory");
        icon = new GameObject("Icon");
        label = new GameObject("Label");

        //Add Components
        item.AddComponent<RectTransform>();
        item.AddComponent<Button>();

        icon.AddComponent<RectTransform>();
        icon.AddComponent<Image>();

        label.AddComponent<RectTransform>();
        label.AddComponent<Text>();

        //Setup Component Values
        rectTransform = item.GetComponent<RectTransform>();
        button = item.GetComponent<Button>();
        button.image = icon.GetComponent<Image>();
        button.onClick.AddListener(() => { OnItemClickEventHandler(info.FullName, index); });

        rectTransform = icon.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 1f);
        rectTransform.anchorMax = new Vector2(0.5f, 1f);
        rectTransform.sizeDelta = new Vector2(70f, 70f);
        rectTransform.anchoredPosition = new Vector2(0f, 20f);
        icon.GetComponent<Image>().sprite = LoadIcon(info.FullName);
        color = icon.GetComponent<Image>().color;
        color.a = deselectedAlpha;
        icon.GetComponent<Image>().color = color;

        rectTransform = label.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0f);
        rectTransform.anchorMax = new Vector2(0.5f, 0f);
        rectTransform.sizeDelta = new Vector2(90f, 20f);
        rectTransform.anchoredPosition = new Vector2(0f, -40f);
        label.GetComponent<Text>().text = info.Name.Length > 26 ? info.Name.Substring(0, 23) + "..." : info.Name;
        label.GetComponent<Text>().font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        label.GetComponent<Text>().fontSize = 10;
        label.GetComponent<Text>().horizontalOverflow = HorizontalWrapMode.Wrap;
        label.GetComponent<Text>().verticalOverflow = VerticalWrapMode.Overflow;
        label.GetComponent<Text>().color = Color.black;

        //Setup Parenting
        icon.transform.SetParent(item.transform);
        label.transform.SetParent(item.transform);
        return item;
    }

    void OnInputTextChanged(String newPath)
    {
        if (!newPath.Equals(currDir.FullName))
        {
            Debug.Log("New Value : " + newPath);
            StartCoroutine(OpenDirectory(newPath));
        }
    }

    int clicks = 0;
    float clickTime = 0f;
    float clickDelay = 1f;
    bool isDoubleClick()
    {
        clicks++;

        if (clicks == 1)
        {
            clickTime = Time.time;
        }

        if (clicks > 1 && Time.time - clickTime < clickDelay)
        {
            clicks = 0;
            clickTime = 0f;
            return true;
        }
        else if (clicks > 2 || Time.time - clickTime > clickDelay)
        {
            clicks = 0;
        }

        return false;
    }

    void OnItemClickEventHandler(string path, int index)
    {
        if (isDoubleClick() == true)
        {
            //Double Click
            OnItemOpenHandler(path, index);
        }
        else
        {
            //Single Click
            OnItemSelectHandler(path, index);
        }
    }

    void ButtonOkHandler()
    {
        OnItemAcceptHandler();
    }

    void ButtonCancelHandler()
    {
        OnCancelledHandler();
    }

    void OnItemSelectHandler(string path, int index)
    {
        Color color;
        if (selectedIndex != index && selectedIndex != -1 && selectedIndex < items.Count)
        {
            color = items[selectedIndex].GetComponentInChildren<Image>().color;
            color.a = deselectedAlpha;
            items[selectedIndex].GetComponentInChildren<Image>().color = color;
        }

        selectedItem = path;
        selectedIndex = index;
        color = items[selectedIndex].GetComponentInChildren<Image>().color;
        color.a = selectedAlpha;
        items[selectedIndex].GetComponentInChildren<Image>().color = color;

        
        if (File.Exists(path))
        {
            ButtonOK.interactable = isOpenFileDialog;
        }
        else
        {
            ButtonOK.interactable = !isOpenFileDialog;
        }
    }

    void OnItemDeselectHandler()
    {
        selectedIndex = -1;
        selectedItem = "";
        ButtonOK.interactable = false;
    }

    void OnItemOpenHandler(string path, int index)
    {
        if (File.Exists(path))
        {
            if (isOpenFileDialog)
            {
                OnItemAcceptHandler();
            }
        }
        else
        {
            //Open this directory
            StartCoroutine(OpenDirectory(path));
        }
    }

    void OnItemAcceptHandler()
    {
        gameObject.SetActive(false);
        eventHandler(true, selectedItem);
        Destroy(gameObject);
    }

    void OnCancelledHandler()
    {
        gameObject.SetActive(false);
        eventHandler(false, null);
        Destroy(gameObject);
    }
}
