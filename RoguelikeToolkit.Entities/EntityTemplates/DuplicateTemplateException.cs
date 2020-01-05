using System;
using System.Runtime.Serialization;

namespace RoguelikeToolkit.Entities
{
    internal class DuplicateTemplateException : Exception
    {
        public DuplicateTemplateException(string templateId, string filename) : 
            base($"Failed to load template (id = '{templateId}', filename = '{filename}') because another template with the same Id is already loaded. Please make sure you have unique template Ids in the json files")
        {
        }
    }
}