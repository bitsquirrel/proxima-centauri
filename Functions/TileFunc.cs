using Microsoft.Dynamics.AX.Metadata.MetaModel;

namespace Functions_for_Dynamics_Operations
{
    internal class TileFunc : ClassFunc
    {
        public static void GenLabelsForTile(LabelEditorControl labelEditor, AxTile axTile)
        {
            string labelId = $"@{labelEditor.LabelFileCollectionSelected.Id}:";

            if (IsNotLabelOrEmpty(axTile.Label, labelId))
            {
                axTile.Label = labelEditor.AddLabelFromTextInCode($"{axTile.Name}~{axTile.Label}", "", true);
            }

            if (IsNotLabelOrEmpty(axTile.HelpText, labelId))
            {
                axTile.HelpText = labelEditor.AddLabelFromTextInCode($"{axTile.Name}Help~{axTile.HelpText}", "", true);
            }
        }
    }
}
