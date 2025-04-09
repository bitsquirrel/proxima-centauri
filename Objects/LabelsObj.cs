using System;
using System.Collections.Generic;

namespace Functions_for_Dynamics_Operations
{
    public class DLabelFileCollection
    {
        public string Id { get; set; }
        public List<DLabelFile> Files { get; set; }

        public SortedDictionary<string, Dictionary<string, Label>> Labels, Duplicates;

        public DLabelFileCollection(string id)
        {
            Id = id;
            Files = new List<DLabelFile>();
            Labels = new SortedDictionary<string, Dictionary<string, Label>>();
            Duplicates = new SortedDictionary<string, Dictionary<string, Label>>();
        }

        public bool IdExists(string id)
        {
            foreach (var label in Labels)
            {
                if (id.ToLower() == label.Key.ToLower())
                    return true;
            }

            return false;
        }
    }

    public class DLabelFile
    {
        public string Buff { get; set; }
        public string Name { get; set; }
        public string FilePath { get; set; }
        public string Language { get; set; }
        public string ResourceName { get; set; }
    }

    public class Label
    {
        public string Text { get; set; }
        public string Description { get; set; }
        public string DescriptionExisted { get; set; }

        public Label(string text, string description, string descriptionExisted)
        {
            Text = text;
            Description = description;
            DescriptionExisted = descriptionExisted;
        }
    }
    /*
    public class LabelId
    {
        public string Id { get; set; }
    }
    */

    public class DLabelSearch
    {
        public string FileId { get; set; }
        public string LabelId { get; set; }
        public string Text { get; set; }
        public string Description { get; set; }

        public DLabelSearch(string fileId, string labelId)
        {   // Set the id for ability to edit
            FileId = fileId;
            LabelId = labelId;
        }
    }

    public class LabelFilePath
    {
        public int Id { get; set; }
        public string FilePath { get; set; }
        public string Model { get; set; }
        public Exception Exception { get; set; }
    }
}
