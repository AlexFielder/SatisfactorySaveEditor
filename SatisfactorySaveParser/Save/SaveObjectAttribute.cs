using System;

namespace SatisfactorySaveParser.Save
{
    public class SaveObjectAttribute : Attribute
    {
        public string Type { get; set; }

        public SaveObjectAttribute(string type)
        {
            Type = type;
        }
    }
}
