using System.Drawing;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using PdfConverterWizard.utils;

namespace PdfBatchConverterWizard.models
{
    public class FileModel
    {
        public string FullPath { get; set; }
        public string FileName { get; set; }
        public BitmapSource FileIcon { get; set; }
        public FileExtension Extension { get; set; }

        public FileModel(string fullpath)
        {
            this.FullPath = fullpath;
            this.FileName = System.IO.Path.GetFileName(fullpath);

            using (Icon ico = Icon.ExtractAssociatedIcon(fullpath))
            {
                FileIcon = Imaging.CreateBitmapSourceFromHIcon(ico.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            }
            var extension = System.IO.Path.GetExtension(fullpath);

            if (extension is not null)
            {
                Extension = (extension) switch
                {
                    ".txt" => FileExtension.txt,
                    ".docx" => FileExtension.docx,
                    ".doc" => FileExtension.doc,
                    _ => FileExtension.invalid,
                };
            }
        }
    }
}
