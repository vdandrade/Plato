﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyModel;
using System.IO;
using Plato.FileSystem;
using System.Collections.Concurrent;
using System.Runtime.Loader;
using Plato.Modules.Abstractions;

namespace Plato.Modules
{
    public class ModuleLoader : IModuleLoader
    {

        #region "Private Variables"

        private static readonly ConcurrentDictionary<string, Lazy<Assembly>> _loadedAssemblies =
            new ConcurrentDictionary<string, Lazy<Assembly>>(StringComparer.OrdinalIgnoreCase);
        
        private static HashSet<string> ApplicationAssemblyNames =>
            _applicationAssemblyNames.Value;

        private static readonly Lazy<HashSet<string>> _applicationAssemblyNames =
            new Lazy<HashSet<string>>(GetApplicationAssemblyNames);

        private static string _assemblyExtension = ".dll";

        #endregion

        #region "Constructor"

        private IPlatoFileSystem _fileSystem;
        //private ILogger _logger;

        public ModuleLoader(
            IPlatoFileSystem fileSystem         
            )
        {
            _fileSystem = fileSystem;
            //_logger = logger;


            _loadedAssemblies.TryAdd("Plato.Abstractions", null);


        }

        #endregion

        #region "Implementation"
      
        public List<Assembly> LoadModule(IModuleDescriptor descriptor)
        {            
            return LoadAssembliesInFolder(descriptor.VirtualPathToBin, new List<Assembly>());
        }

        #endregion

        #region "Private Methods"

        private List<Assembly> LoadAssembliesInFolder(
            string path, List<Assembly> localList)
        {

            if (string.IsNullOrEmpty(path))
                return localList;

            // no bin folder within module
            if (!_fileSystem.DirectoryExists(path))
                return localList;

            var folder = _fileSystem.GetDirectoryInfo(path);
            foreach (var file in folder.GetFiles())
            {
                if ((file.Extension != null) && (file.Extension.ToLower() == _assemblyExtension))
                {
                    if (!IsAssemblyLoaded(Path.GetFileNameWithoutExtension(file.FullName)))
                    {
                        Assembly assembly = LoadFromAssemblyPath(file.FullName);
                        if (assembly != null)
                            localList.Add(assembly);
                    }
                  
                }

            }

            // recursive lookup
            string[] subFolders = Directory.GetDirectories(path);
            if (folder != null)
            {
                for (int i = 0; i <= subFolders.Length - 1; i++)                
                    LoadAssembliesInFolder(subFolders.GetValue(i).ToString(), localList);                
            }
            
            return localList;

        }
                
        private Assembly LoadFromAssemblyPath(string assemblyPath)
        {
                     
            return _loadedAssemblies.GetOrAdd(Path.GetFileNameWithoutExtension(assemblyPath),
                new Lazy<Assembly>(() =>
                {
                    return AssemblyLoadContext.Default.LoadFromAssemblyPath(assemblyPath);
                })).Value;
        }
        
        private static HashSet<string> GetApplicationAssemblyNames()
        {
            return new HashSet<string>(DependencyContext.Default.RuntimeLibraries
                .SelectMany(library => library.RuntimeAssemblyGroups)
                .SelectMany(assetGroup => assetGroup.AssetPaths)
                .Select(path => Path.GetFileNameWithoutExtension(path)),
                StringComparer.OrdinalIgnoreCase);
        }
        

        private bool IsAssemblyLoaded(string assemblyName)
        {
            return _loadedAssemblies.ContainsKey(assemblyName);
        }

        #endregion

    }
}
