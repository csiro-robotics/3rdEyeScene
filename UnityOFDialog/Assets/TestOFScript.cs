using UnityEngine;
using Dialogs;

public class TestOFScript : MonoBehaviour
{
  public FileDialogUI BrowseUI;
  public MessageBoxUI _messageUI;
  //private OpenFileDialog _ofDialog;
  private FileDialog _fileDialog;

  public void ShowSaveDialog()
  {
    if (_fileDialog == null && BrowseUI != null)
    {
      //_fileDialog = new OpenFileDialog(new TrueFileSystem(), BrowseUI);
      _fileDialog = new SaveFileDialog(new TrueFileSystem(), BrowseUI, _messageUI);
      _fileDialog.AddExtension = true;
      _fileDialog.DefaultExt = "txt";
      _fileDialog.ShowDialog(delegate (CommonDialog dialog, DialogResult result)
      {
        Debug.Log(string.Format("Closed: {0}", result));
        if (result == DialogResult.OK)
        {
          if (_fileDialog.FileNames != null)
          {
            System.Text.StringBuilder str = new System.Text.StringBuilder();
            str.Append("Files:\n");
            foreach (string filename in _fileDialog.FileNames)
            {
              str.Append(filename);
              str.Append("\n");
            }
            Debug.Log(str.ToString());
          }
        }
        _fileDialog = null;
      });
    }
  }

  public void ShowMessageBox()
  {
    MessageBox.Show(delegate (CommonDialog dialog, DialogResult result)
    {
      MessageBox.Show(null, "Hello again");
    },
    "Press OK to show another");
  }

  void Start()
  {
    //ShowSaveDialog();
  }

  void OnDisable()
  {
    _fileDialog = null;
  }
}
