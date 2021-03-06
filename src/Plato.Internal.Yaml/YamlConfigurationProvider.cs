﻿using System;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace Plato.Internal.Yaml
{
    public class YamlConfigurationProvider : FileConfigurationProvider
    {
        public const char _separator = ':';
        public const string _emptyValue = "null";
        public const char _themesSeparator = ';';

        public YamlConfigurationProvider(FileConfigurationSource source) : base(source)
        {
        }

        public override void Load(Stream stream)
        {
            YamlConfigurationFileParser parser = new YamlConfigurationFileParser();
            try
            {
                Data = parser.Parse(stream);
            }
            catch (InvalidCastException e)
            {
                throw new FormatException("FormatError_YAMLparseError", e);
            }
        }

        public virtual void Commit()
        {
            if (File.Exists(Source.Path))
            {
                File.Delete(Source.Path);
            }

            var fileInfo = new FileInfo(Source.Path);

            if (!fileInfo.Directory.Exists)
            {
                Directory.CreateDirectory(fileInfo.Directory.FullName);
            }
            
                 
            var newConfigFileStream = new FileStream(Source.Path, FileMode.CreateNew);

            try
            {
                // Generate contents and write it to the newly created config file
                GenerateNewConfig(newConfigFileStream);
            }
            catch
            {
                newConfigFileStream.Dispose();

                // The operation should be atomic because we don't want a corrupted config file
                // So we roll back if the operation fails
                if (File.Exists(Source.Path))
                {
                    File.Delete(Source.Path);
                }

                // Rethrow the exception
                throw;
            }
            finally
            {
                newConfigFileStream.Dispose();
            }

            return;
        }


        internal void GenerateNewConfig(Stream outputStream)
        {
            var outputWriter = new StreamWriter(outputStream);

            foreach (var entry in Data)
            {
                outputWriter.WriteLine("{0}: {1}", entry.Key, (entry.Value ?? _emptyValue));
            }

            outputWriter.Flush();
        }
    }

}
