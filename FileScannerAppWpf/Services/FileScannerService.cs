using FileScannerApp.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class FileScannerService
{
    public static FileInfo[] Scan(string path)
    {
        DirectoryInfo dir = new DirectoryInfo(path);
        return dir.GetFiles();
    }

    public static List<FileData> Map(FileInfo[] files)
    {
        return files.Select(f => new FileData
        {
            Name = f.Name,
            Extension = f.Extension,
            Path = f.FullName,
            Size = f.Length,
            CreatedDate = f.CreationTime,
            ModifiedDate = f.LastWriteTime
        }).ToList();
    }





}