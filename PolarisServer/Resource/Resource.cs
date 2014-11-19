using System;

namespace PolarisServer.Resource
{
    public enum ResourceType
    {
        Area,
        NPC,
        Object,
        Setting,
        Other
    }
    public abstract class Resource
    {
        public String Name;
        public ResourceType Type;
        public Type ClassType;

        /// <summary>
        /// Initializes a new instance of the <see cref="PolarisServer.Resource.Resource"/> class.
        /// </summary>
        /// <param name="name">Name of your resource bundle.</param>
        /// <param name="type">ResourceType of your resource bundle</param>
        /// <param name="classType">C# Class type of your resource bundle</param>
        public Resource(String name, ResourceType type, Type classType)
        {
            this.Name = name;
            this.Type = type;
            this.ClassType = classType;
        }

    }
}

