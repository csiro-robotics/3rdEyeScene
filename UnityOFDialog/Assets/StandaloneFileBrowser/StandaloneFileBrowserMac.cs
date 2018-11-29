#if UNITY_STANDALONE_OSX

using System;
using System.Runtime.InteropServices;

namespace SFB {
    public class StandaloneFileBrowserMac : IStandaloneFileBrowser {
        private static Action<string[]> _openFileCb;
        private static Action<string[]> _openFolderCb;
        private static Action<string> _saveFileCb;

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void AsyncCallback(string path);

        [DllImport("StandaloneFileBrowser")]
        private static extern IntPtr DialogOpenFilePanel(string title, string directory, string extension, bool multiselect);
        [DllImport("StandaloneFileBrowser")]
        private static extern void DialogOpenFilePanelAsync(string title, string directory, string extension, bool multiselect, AsyncCallback callback);
        [DllImport("StandaloneFileBrowser")]
        private static extern IntPtr DialogOpenFolderPanel(string title, string directory, bool multiselect);
        [DllImport("StandaloneFileBrowser")]
        private static extern void DialogOpenFolderPanelAsync(string title, string directory, bool multiselect, AsyncCallback callback);
        [DllImport("StandaloneFileBrowser")]
        private static extern IntPtr DialogSaveFilePanel(string title, string directory, string defaultName, string extension);
        [DllImport("StandaloneFileBrowser")]
        private static extern void DialogSaveFilePanelAsync(string title, string directory, string defaultName, string extension, AsyncCallback callback);

        public string[] OpenFilePanel(string title, string directory, ExtensionFilter[] extensions, bool multiselect) {
            return OpenFilePanel(new BrowserParameters() {
                Title = title,
                Directory = directory,
                Extensions = extensions,
                Multiselect = multiselect
            });
        }

        public string[] OpenFilePanel(BrowserParameters args) {
            var paths = Marshal.PtrToStringAnsi(DialogOpenFilePanel(
                args.Title,
                args.Directory,
                GetFilterFromFileExtensionList(args.Extensions),
                args.Multiselect));
            return paths.Split((char)28);
        }

        public void OpenFilePanelAsync(string title, string directory, ExtensionFilter[] extensions, bool multiselect, Action<string[]> cb) {
            OpenFilePanelAsync(new BrowserParameters() {
                Title = title,
                Directory = directory,
                Extensions = extensions,
                Multiselect = multiselect
            }, cb);
        }

        public void OpenFilePanelAsync(BrowserParameters args, Action<string[]> cb) {
            _openFileCb = cb;
            DialogOpenFilePanelAsync(
                args.Title,
                args.Directory,
                GetFilterFromFileExtensionList(args.Extensions),
                args.Multiselect,
                (string result) => { _openFileCb.Invoke(result.Split((char)28)); });
        }

        public string[] OpenFolderPanel(string title, string directory, bool multiselect) {
            return OpenFolderPanel(new BrowserParameters() {
                Title = title,
                Directory = directory,
                Multiselect = multiselect
            });
        }

        public string[] OpenFolderPanel(BrowserParameters args) {
            var paths = Marshal.PtrToStringAnsi(DialogOpenFolderPanel(
                args.Title,
                args.Directory,
                args.Multiselect));
            return paths.Split((char)28);
        }

        public void OpenFolderPanelAsync(string title, string directory, bool multiselect, Action<string[]> cb) {
            OpenFolderPanelAsync(new BrowserParameters() {
                Title = title,
                Directory =directory,
                Multiselect = multiselect
            }, cb);
        }

        public void OpenFolderPanelAsync(BrowserParameters args, Action<string[]> cb) {
            _openFolderCb = cb;
            DialogOpenFolderPanelAsync(
                args.Title,
                args.Directory,
                args.Multiselect,
                (string result) => { _openFolderCb.Invoke(result.Split((char)28)); });
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
            return Marshal.PtrToStringAnsi(DialogSaveFilePanel(
                args.Title,
                args.Directory,
                args.DefaultName,
                GetFilterFromFileExtensionList(args.Extensions)));
        }

        public void SaveFilePanelAsync(string title, string directory, string defaultName, ExtensionFilter[] extensions, Action<string> cb) {
            SaveFilePanelAsync(new BrowserParameters() {
                Title = title,
                Directory = directory,
                DefaultName = defaultName,
                Extensions = extensions
            }, cb);
        }

        public void SaveFilePanelAsync(BrowserParameters args, Action<string> cb) {
            _saveFileCb = cb;
            DialogSaveFilePanelAsync(
                args.Title,
                args.Directory,
                args.DefaultName,
                GetFilterFromFileExtensionList(args.Extensions),
                (string result) => { _saveFileCb.Invoke(result); });
        }

        private static string GetFilterFromFileExtensionList(ExtensionFilter[] extensions) {
            if (extensions == null) {
                return "";
            }

            var filterString = "";
            foreach (var filter in extensions) {
                filterString += filter.Name + ";";

                foreach (var ext in filter.Extensions) {
                    filterString += ext + ",";
                }

                filterString = filterString.Remove(filterString.Length - 1);
                filterString += "|";
            }
            filterString = filterString.Remove(filterString.Length - 1);
            return filterString;
        }
    }
}

#endif