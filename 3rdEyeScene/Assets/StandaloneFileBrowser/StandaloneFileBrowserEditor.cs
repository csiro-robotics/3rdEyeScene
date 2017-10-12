#if UNITY_EDITOR

using System;
using UnityEditor;

namespace SFB {
    public class StandaloneFileBrowserEditor : IStandaloneFileBrowser  {
        public string[] OpenFilePanel(string title, string directory, ExtensionFilter[] extensions, bool multiselect) {
            return OpenFilePanel(new BrowserParameters() {
                Title = title,
                Directory = directory,
                Extensions = extensions,
                Multiselect = multiselect
            });
        }

        public string[] OpenFilePanel(BrowserParameters args) {
            string path = "";

            if (args.Extensions == null) {
                path = EditorUtility.OpenFilePanel(args.Title, args.Directory, "");
            }
            else {
                path = EditorUtility.OpenFilePanelWithFilters(args.Title, args.Directory, GetFilterFromFileExtensionList(args.Extensions));
            }

            return string.IsNullOrEmpty(path) ? new string[0] : new[] { path };
        }

        public void OpenFilePanelAsync(string title, string directory, ExtensionFilter[] extensions, bool multiselect, Action<string[]> cb) {
            cb.Invoke(OpenFilePanel(title, directory, extensions, multiselect));
        }

        public void OpenFilePanelAsync(BrowserParameters args, Action<string[]> cb) {
            cb.Invoke(OpenFilePanel(args));
        }

        public string[] OpenFolderPanel(string title, string directory, bool multiselect) {
            return OpenFolderPanel(new BrowserParameters() {
                Title = title,
                Directory = directory,
                Multiselect = multiselect
            });
        }

        public string[] OpenFolderPanel(BrowserParameters args) {
            var path = EditorUtility.OpenFolderPanel(args.Title, args.Directory, "");
            return string.IsNullOrEmpty(path) ? new string[0] : new[] {path};
        }

        public void OpenFolderPanelAsync(string title, string directory, bool multiselect, Action<string[]> cb) {
            cb.Invoke(OpenFolderPanel(title, directory, multiselect));
        }

        public void OpenFolderPanelAsync(BrowserParameters args, Action<string[]> cb) {
            cb.Invoke(OpenFolderPanel(args.Title, args.Directory, args.Multiselect));
        }

        public string SaveFilePanel(string title, string directory, string defaultName, ExtensionFilter[] extensions) {
            return SaveFilePanel(new BrowserParameters() {
                Title = title,
                Directory = directory,
                DefaultName = defaultName,
                Extensions = extensions
            });
        }

        public string SaveFilePanel(BrowserParameters args) {
            var ext = args.Extensions != null ? args.Extensions[0].Extensions[0] : "";
            var name = string.IsNullOrEmpty(ext) ? args.DefaultName : args.DefaultName + "." + ext;
            return EditorUtility.SaveFilePanel(args.Title, args.Directory, name, ext);
        }

        public void SaveFilePanelAsync(string title, string directory, string defaultName, ExtensionFilter[] extensions, Action<string> cb) {
            cb.Invoke(SaveFilePanel(title, directory, defaultName, extensions));
        }

        public void SaveFilePanelAsync(BrowserParameters args, Action<string> cb) {
            cb.Invoke(SaveFilePanel(args));
        }

        // EditorUtility.OpenFilePanelWithFilters extension filter format
        private static string[] GetFilterFromFileExtensionList(ExtensionFilter[] extensions) {
            var filters = new string[extensions.Length * 2];
            for (int i = 0; i < extensions.Length; i++) {
                filters[(i * 2)] = extensions[i].Name;
                filters[(i * 2) + 1] = string.Join(",", extensions[i].Extensions);
            }
            return filters;
        }
    }
}

#endif
