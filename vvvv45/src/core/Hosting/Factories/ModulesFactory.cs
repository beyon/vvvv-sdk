﻿using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;

using VVVV.Core;
using VVVV.Core.Logging;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Hosting.Factories
{
    /// <summary>
    /// Effects factory, parses and watches the effect directory
    /// </summary>
    [Export(typeof(IAddonFactory))]
    public class ModulesFactory : AbstractFileFactory<IAddonHost>
    {
        private string FDTD = "";
        
        [Import]
    	protected ILogger Logger { get; set; }
        
        public ModulesFactory()
        	: base(Shell.CallerPath.ConcatPath(@"..\..\modules"), ".v4p")
        {
        }

        //create a node info from a filename
        protected override IEnumerable<INodeInfo> GetNodeInfos(string filename)
        {
            if (FDTD == "") LoadDTD();

            //check extension
            if (Path.GetExtension(filename) != FileExtension) yield break;
            
            //check filename structure
            string fn = Path.GetFileNameWithoutExtension(filename);
            if (!Regex.IsMatch(fn, @"^.+\s\(.+\)$")) yield break;
            
            var nodeInfo = new NodeInfo();
            
            //match the filename
            var match = Regex.Match(fn, @"(\S+) \((\S+)(?: ([^)]*))?\)");
            
            //read matches
            nodeInfo.Name = match.Groups[1].Value;
            nodeInfo.Category = match.Groups[2].Value;
            nodeInfo.Version = match.Groups[3].Value;
            
            nodeInfo.Filename = filename;
            nodeInfo.Type = NodeType.Module;
            nodeInfo.InitialBoxSize = new System.Drawing.Size(4800, 3600);
            nodeInfo.InitialWindowSize = new System.Drawing.Size(9000, 6000);
            
            try
            {
                // Create an instance of StreamReader to read from a file.
                // The using statement also closes the StreamReader.
                using (StreamReader sr = new StreamReader(filename))
                {
                    
                    //skip first line
                    var s = sr.ReadLine();
                    
                    var settings = new XmlReaderSettings();
                    settings.ProhibitDtd = false;
                    
                    var xmlReader = XmlReader.Create((new StringReader(FDTD + sr.ReadToEnd())), settings);
                    //xmlReader.Settings
                    if(xmlReader.ReadToFollowing("INFO"))
                    {
                        nodeInfo.Author = xmlReader.GetAttribute("author");
                        nodeInfo.Help = xmlReader.GetAttribute("description");
                        nodeInfo.Tags = xmlReader.GetAttribute("tags");
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Log(LogType.Error, "Could not extract module info of " + nodeInfo.Systemname);
                Logger.Log(e);
                yield break;
            }
            
            yield return nodeInfo;
        }
        
        protected override bool CreateNode(INodeInfo nodeInfo, IAddonHost nodeHost)
        {
        	// Will never get called.
        	return true;
        }
        
        protected override bool DeleteNode(INodeInfo nodeInfo, IAddonHost nodeHost)
        {
        	// Will never get called.
        	return true;
        }
        
        //get the dtd string
        private void LoadDTD()
        {
            var path = Path.GetFullPath(DirectoryToWatch + @"\..\bin");
            var files = Directory.GetFiles(path, "*.dtd");
            
            using (StreamReader sr = new StreamReader(files[0]))
            {
                //add the DOCTYPE definition to place the DTD inline
                FDTD = sr.ReadLine();
                FDTD += @"<!DOCTYPE PATCH [";
                FDTD += sr.ReadToEnd();
                FDTD += @"]>";
            }
        }
    }
}
