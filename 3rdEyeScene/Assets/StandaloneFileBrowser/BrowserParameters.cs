namespace SFB {
    /// <summary>
    /// Extension parameters for opening file browser windows. This reduces the arguments to the Open/Save function
    /// calls and allows for future extensions.
    /// </summary>
    public class BrowserParameters {
        public string Title = string.Empty;
        public string Directory = string.Empty;
        public string DefaultName = string.Empty;
        public string DefaultExt = string.Empty;
        public ExtensionFilter[] Extensions;
        public bool Multiselect = false;
        public bool AddExtension = true;
        public int FilterIndex = 0;
    }
}
