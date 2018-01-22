# TempFileExplorer

A Choose File/Folder Dialog Component for unity. This was made originally for Unity 5 and now it's ported over to Unity 2017 (Without much hassle). 

![Refer the screenshot to see how it looks like](screenshot.png?raw=true "Screenshot")

## Contents

The Project contains a simple demo (Demo Windows.zip) using this component where it asks you to choose a .txt file and then it shows you the complete path to the txt file. 

In addition to the demo there's also a unity package that you can use in your own unity projects as follows:

```
// Make an instance of the OpenDialog Prefab
GameObject original = Resources.Load<GameObject>("FileOpenDialog/Prefab/OpenDialog");
GameObject openFileDialog = Instantiate<GameObject>(original);
Dialog dialog = openFileDialog.GetComponent<Dialog>();

// Choose a file
dialog.OpenFileDialog("Try Choosing a txt file", @"C:\StartingLocationHere", ".txt", OnDialogComplete);
// Choose a folder
dialog.OpenFileDialog("Try Choosing a folder", @"C:\StartingLocationHere", OnDialogComplete);
```

You'll need to pass a function as the handler for when the file/folder is chosen. A sample handler is shown below:

```
void OnDialogComplete(bool isSucessful, string path)
{
    if (isSucessful)
    {
        Debug.Log("Path : " + path);
    }
    else
    {
        Debug.Log("No File/Folder Chosen, Cancel was pressed or something else happened.");
    }
}
```

Finally, there's the source code of the component. Kindly note that the images in this project were taken from the following sources:

Folder Icon by Arthur Shlain (https://thenounproject.com/ArtZ91/)

File Icon by Joe Harrison (https://thenounproject.com/joe_harrison/)

All other Icons From Kenny Game Assets (http://www.kenney.nl/) 