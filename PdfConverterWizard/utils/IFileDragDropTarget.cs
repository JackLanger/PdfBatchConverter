namespace PdfConverterWizard.utils
{
    /// <summary>
    /// IFileDragDropTarget Interface
    /// </summary>
    public interface IFileDragDropTarget
    {
        void OnFileDrop(string[] filepaths);
    }
}
