using System;
using System.IO;
using System.Collections.Generic;

namespace PolarisServer.Resource
{
    public class ResourceManager
    {
        private static readonly ResourceManager instance = new ResourceManager();
        private List<Resource> resources;

        public static ResourceManager Instance
        {
            get
            {
                return instance;
            }
        }

        private ResourceManager()
        {
            this.resources = new List<Resource>();
            LoadResources(Path.Combine(Directory.GetCurrentDirectory(), "resources"));
        }

        private void LoadResources(String resourcedir)
        {
            Logger.WriteInternal("[RES] Loading resources from {0}", resourcedir);
            /* TODO:
             * Iterate through resources folder
             * JSON Seralize/Deseralize
             * Add resource to resources list
             */

            Logger.WriteInternal("[RES] Loaded {0} resources off disk.", resources.Count);
        }
    }
}

