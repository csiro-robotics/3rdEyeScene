using System;

namespace SFB {
    public interface IStandaloneFileBrowser {
        string[] OpenFilePanel(string title, string directory, ExtensionFilter[] extensions, bool multiselect);
        string[] OpenFilePanel(BrowserParameters args);
        string[] OpenFolderPanel(string title, string directory, bool multiselect);
        string[] OpenFolderPanel(BrowserParameters args);
        string SaveFilePanel(string title, string directory, string defaultName, ExtensionFilter[] extensions);
        string SaveFilePanel(BrowserParameters args);

        void OpenFilePanelAsync(string title, string directory, ExtensionFilter[] extensions, bool multiselect, Action<string[]> cb);
        void OpenFilePanelAsync(BrowserParameters args, Action<string[]> cb);
        void OpenFolderPanelAsync(string title, string directory, bool multiselect, Action<string[]> cb);
        void OpenFolderPanelAsync(BrowserParameters args, Action<string[]> cb);
        void SaveFilePanelAsync(string title, string directory, string defaultName, ExtensionFilter[] extensions, Action<string> cb);
        void SaveFilePanelAsync(BrowserParameters args, Action<string> cb);
    }
}
