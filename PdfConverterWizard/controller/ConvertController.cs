using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using JetBrains.Annotations;
using Microsoft.Win32;
using PdfBatchConverterWizard.models;
using PdfBatchConverterWizzard.controls;
using PdfConverterWizard.controls;
using PdfConverterWizard.exceptions;
using PdfConverterWizard.utils;
using SautinSoft.Document;

namespace PdfConverterWizard.controller;

public class ConvertController : INotifyPropertyChanged, IFileDragDropTarget
{
    private string LogFilePath { get; set; } = string.Empty;
    /// <summary>
    /// List of file paths from selection.
    /// </summary>
    private readonly ObservableCollection<FileModel> _filePaths = new();

    /// <summary>
    /// UX string to give feedback to user.
    /// </summary>
    private string _actionString = "Convert";

    /// <summary>
    /// Value for the progress bar.
    /// </summary>
    private double _progress;

    /// <summary>
    /// Public value for progress bar.
    /// </summary>
    public double Progress
    {
        get => _progress;
        set
        {
            _progress = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Action string used for giving ux feedback.
    /// </summary>
    public string ActionString
    {
        get => _actionString;
        set
        {
            _actionString = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Public accessor for filepath. This are the pats from selection.
    /// </summary>
    public ObservableCollection<FileModel> FilePaths
    {
        get => _filePaths;
    }

    private ICommand _removeCommand;
    private ICommand _convertCommand;
    private ICommand _selectFileCommnad;
    private ICommand _openFileCommnad;
    private ICommand _clearLogCommand;
    private ICommand _saveLogCommnad;

    /// <summary>
    /// Command to clear the log.
    /// </summary>
    public ICommand ClearLogCommand => _clearLogCommand ??= new RelayCommand(() => ErrorLogs.Clear());

    /// <summary>
    /// Command to save the log.
    /// </summary>
    public ICommand SaveLogCommand => _saveLogCommnad ??= new RelayCommand(() => SaveLog());

    private void SaveLog()
    {
        // open save dialog
        if (LogFilePath.Equals(string.Empty))
        {
            SetLogFileName();
            if (LogFilePath.Equals(string.Empty))
                return;

            SaveLogToFile();
        }
        else
        {
            SaveLogToFile();
        }
    }

    private void SetLogFileName()
    {
        SaveFileDialog dlg = new SaveFileDialog();
        dlg.Filter = "text file (*.txt)|*.txt";
        dlg.Title = "Save Log to file";
        dlg.ShowDialog();
        LogFilePath = dlg.FileName;
    }

    private void SaveLogToFile()
    {
        var file = File.Create(LogFilePath);
        StringBuilder sb = new StringBuilder();
        foreach (var sLog in ErrorLogs)
        {
            sb.AppendLine(sLog);
        }
        var log = sb.ToString();
        var buffer = Encoding.UTF8.GetBytes(log);

        file.WriteAsync(buffer, 0, buffer.Length);
        file.Close();
        ErrorLogs.Clear();
    }

    /// <summary>
    /// Command to open the file dialog.
    /// </summary>
    public ICommand SelectFileCommand => _selectFileCommnad ??= new RelayCommand(() => SelectFiles());
    /// <summary>
    /// Command to open the selected file.
    /// </summary>
    public ICommand OpenFileCommand => _openFileCommnad ??= new ParamCommand<FileModel>(file => OpenFile(file));
    /// <summary>
    /// Command to start conversion.
    /// </summary>
    public ICommand ConvertCommand => _convertCommand ??= new RelayCommand( () => ConvertAllFiles());

    /// <summary>
    /// Converts the paths provided.
    /// </summary>
    private async void ConvertAllFiles()
    {
        bool finished = false;
        List<Task> tasks = new();
        foreach (var filePath in _filePaths)
        {
            try
            {

                Task task = ConvertAsync(filePath);
                tasks.Add(task);

            }
            catch (Exception ex)
            {
                ErrorLogs.Add(ex.Message);
            }
        }
        
        while (!finished)
        {
            var progress = 0;
            foreach (var tsk in tasks)
            {
                var task = (Task<FileModel>) tsk;
                var file = await task;
                finished = task.IsCompleted || task.IsCanceled;
                if (finished)
                {
                    progress++;
                }
                FilePaths.Remove(file);
            }
            Progress = 100.0 * progress / tasks.Count;
            ActionString = $"Converting files to PDF. Progress: {Progress}%";
        }

        ActionString = $"Converting files to PDF. Progress: {Progress}%";
        FilePaths.Clear();
    }

    /// <summary>
    /// Runs an async task to generate the pdf file.
    /// </summary>
    /// <param name="file">the file to convert</param>
    /// <returns>a task converting the file to pdf</returns>
    private async Task<FileModel> ConvertAsync(FileModel file)
    {
        return await Task.Run(() =>
        {
            // todo: Implement excel files.

            var fileName = file.FullPath;
            var nameArr = fileName.Split('.');
            nameArr[1] = "pdf";
            if (file.Extension is not FileExtension.invalid)
                DocumentCore.Load(file.FullPath).Save($"{nameArr[0]}.{nameArr[1]}");

            return file;
        });
    }

    /// <summary>
    /// Command to remove unwanted files from the filepaths list. 
    /// </summary>
    public ICommand RemoveCommand => _removeCommand ??= new ParamCommand<Object>((i) => Remove(i));

    public ObservableCollection<string> ErrorLogs { get; } = new();

    /// <summary>
    /// Removes all entries from the List of filePaths.
    /// </summary>
    /// <param name="data">the objects to remove</param>
    private void Remove(Object data)
    {
        System.Collections.IList items = (System.Collections.IList)data;
        var collection = items.Cast<FileModel>();
        List<int> indexes = new List<int>();
        foreach (var item in collection)
        {
            indexes.Add(FilePaths.IndexOf(item));
        }
        for (int i = FilePaths.Count - 1; i >= 0; i--)
        {
            if (indexes.Contains(i))
                FilePaths.RemoveAt(i);
        }
    }

    /// <summary>
    /// Opens a file dialog where user can select multiple files to parse.
    /// </summary>
    private void SelectFiles()
    {
        OpenFileDialog ofd = new OpenFileDialog();
        ofd.Multiselect = true;
        ofd.Filter = "docx file (*.docx)|*.docx| doc file (*.doc)|*.doc|txt files (*.txt)|*.txt|All files (*.*)|*.*";
        ofd.ShowDialog();
        foreach (var fileName in ofd.FileNames)
        {
            FileModel fileMod = new FileModel(fileName);
            // prevent doubling of files.
            try
            {
                if (!_filePaths.Contains(fileMod) && canConvert(fileMod))
                    _filePaths.Add(fileMod);
            }
            catch (InvalidFileTypeException ex)
            {
                ErrorLogs.Add(ex.Message);
            }
        }
        ActionString = "Convert";
    }

    private bool canConvert(FileModel fileMod)
    {
        if (fileMod.Extension is FileExtension.invalid)
            throw new InvalidFileTypeException($"Can not convert {fileMod.FileName}, Filetype is not supported.");
        return true;
    }

    /// <summary>
    /// Runs a Process that opens the provided file.
    /// </summary>
    /// <param name="file">Path to file</param>
    private void OpenFile(FileModel file)
    {
        var p = new Process();
        p.StartInfo = new ProcessStartInfo(file.FullPath)
        {
            UseShellExecute = true
        };
        p.Start();
    }

    /// <summary>
    /// Property changed event. Is fired upon change of property.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    [NotifyPropertyChangedInvocator]
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    /// <summary>
    /// File drop method this method is beeing fired on a file drop on the Files list.
    /// </summary>
    /// <param name="filepaths">the file paths from the files dropped on the list</param>
    public void OnFileDrop(string[] filepaths)
    {
        foreach (var file in filepaths)
        {
            FileModel fileMod = new FileModel(file);
            // prevent doubling of files.
            try
            {
                if (!_filePaths.Contains(fileMod) && canConvert(fileMod))
                    _filePaths.Add(fileMod);
            }
            catch (InvalidFileTypeException ex)
            {
                ErrorLogs.Add(ex.Message);
            }
        }
        ActionString = "Convert";
    }
}