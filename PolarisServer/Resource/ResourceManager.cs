using System;
using System.IO;
using System.Collections.Generic;

namespace PolarisServer.Resource
{
    public class ResourceManager
    {
        private static ResourceManager instance = null;
        private List<Resource> resources;

        private ResourceManager()
        {
            this.resources = new List<Resource>();
            loadResources(Path.Combine(Directory.GetCurrentDirectory(), "resources"));
        }

        private void loadResources(String resourcedir)
        {
            Logger.WriteInternal("[RES] Loading resources from {0}", resourcedir);
            /* TODO:
             * Iterate through resources folder
             * JSON Seralize/Deseralize
             * Add resource to resources list
             */

            Logger.WriteInternal("[RES] Loaded {0} resources off disk.", resources.Count);

        }


        public static ResourceManager GetInstance()
        {
            if (instance == null)
            {
                instance = new ResourceManager();
            }

            return instance;
        }

        public static void Init()
        {
            instance = new ResourceManager();
        }




    }
}

