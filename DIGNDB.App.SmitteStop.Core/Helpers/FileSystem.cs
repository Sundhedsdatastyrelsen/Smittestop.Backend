﻿using DIGNDB.App.SmitteStop.Core.Contracts;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

namespace DIGNDB.App.SmitteStop.Core.Helpers
{
    public class FileSystem : IFileSystem
    {
        public void CreateDirectory(string directoryName)
        {
            Directory.CreateDirectory(directoryName);
        }

        public DateTime GetCreationDateUTC(string fileName)
        {
            var fileInfo = new FileInfo(fileName);
            return fileInfo.CreationTimeUtc;
        }

        public string[] GetFileNamesFromDirectory(string directoryPath)
        {
            return Directory.GetFiles(directoryPath);
        }

        public string JoinPaths(params string[] paths)
        {
            return Path.Join(paths);
        }

        public void DeleteFile(string path)
        {
            var fileInfo = new FileInfo(path);
            fileInfo.Delete();
        }

        public void WriteAllBytes(string path, byte[] fileContent)
        {
            File.WriteAllBytes(path, fileContent);
        }

        public bool FileExists(string path)
        {
            return (File.Exists(path));
        }

        public byte[] ReadFile(string path)
        {
            return (File.ReadAllBytes(path));
        }

        public string[] GetAllTemporaryFilesFromFolder(string currentZipFilesFolder)
        {
            List<string> temporaryZipFiles = new List<string>();
            var allZipFiles = GetFileNamesFromDirectory(currentZipFilesFolder);
            foreach (var file in allZipFiles)
            {
                var fileName = GetFileNameFromPath(file);
                if (fileName[0] == '.')
                {
                    temporaryZipFiles.Add(file);
                }
            }

            return temporaryZipFiles.ToArray();
        }

        public void DeleteFiles(string[] temporaryFileList)
        {
            foreach (string file in temporaryFileList)
            {
                DeleteFile(file);
            }
        }

        public void Rename(string oldFilePath, string newFilePath)
        {
            System.IO.File.Move(oldFilePath, newFilePath);
        }

        public string GetDirectoryNameFromPath(string path)
        {
            return Path.GetDirectoryName(path);
        }

        public string GetFileNameFromPath(string path)
        {
            return Path.GetFileName(path);
        }

        public bool DirectoryExists(string directoryName)
        {
            return Directory.Exists(directoryName);
        }

        public async Task SaveFormFileAsync(IFormFile sourceFile, string destinationPath)
        {
            await using Stream fileStream = new FileStream(destinationPath, FileMode.Create);
            await sourceFile.CopyToAsync(fileStream);
        }

        public ZipArchive OpenZip(string filename)
        {
            return ZipFile.OpenRead(filename);
        }
    }
}
