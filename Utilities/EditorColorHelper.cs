using System;
using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms.Integration;
using System.Windows.Media;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;

namespace Functions_for_Dynamics_Operations.Utilities
{
    /// <summary>
    /// Shared utility for applying colors to all editor/search WPF controls.
    /// Walks the logical tree so new labels or grids are picked up automatically.
    /// Supports both user-configured custom colors and automatic Visual Studio theme colors.
    /// </summary>
    internal static class EditorColorHelper
    {
        /// <summary>
        /// Apply colors derived from the current Visual Studio theme. All WPF labels and
        /// checkboxes get VS window-text foreground; DataGridViews get VS tool-window colors.
        /// The WPF root background is left null so the VS shell paints it correctly.
        /// </summary>
        public static void ApplyVsTheme(FrameworkElement root)
        {
            if (root == null) return;

            try
            {
                // WPF: use VS dynamic brush — updates automatically on theme change.
                System.Windows.Media.Brush foreBrush =
                    root.TryFindResource(VsBrushes.ToolWindowTextKey) as System.Windows.Media.Brush
                    ?? root.TryFindResource(VsBrushes.WindowTextKey) as System.Windows.Media.Brush;

                // WinForms grids need concrete colors read from the current theme now.
                System.Drawing.Color gridFore = VSColorTheme.GetThemedColor(EnvironmentColors.ToolWindowTextColorKey);
                System.Drawing.Color gridBack = VSColorTheme.GetThemedColor(EnvironmentColors.ToolWindowBackgroundColorKey);

                ApplyRecursive(root, foreBrush, gridFore, gridBack);

                // Leave WPF root Background null — VS shell handles the tool window background.
                if (root is System.Windows.Controls.Control ctrl)
                    ctrl.Background = null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("EditorColorHelper.ApplyVsTheme failed: " + ex);
            }
        }

        /// <summary>
        /// Apply explicit user-configured colors. Labels/checkboxes get <paramref name="foreColor"/>;
        /// the root background gets <paramref name="backColor"/> (transparent = VS theme paints it);
        /// DataGridViews get <paramref name="gridForeColor"/> / <paramref name="gridBackColor"/>.
        /// </summary>
        public static void Apply(
            FrameworkElement root,
            System.Drawing.Color foreColor,
            System.Drawing.Color backColor,
            System.Drawing.Color gridForeColor,
            System.Drawing.Color gridBackColor)
        {
            if (root == null) return;

            try
            {
                System.Windows.Media.SolidColorBrush foreBrush = new System.Windows.Media.SolidColorBrush(
                    Color.FromArgb(foreColor.A, foreColor.R, foreColor.G, foreColor.B));

                ApplyRecursive(root, foreBrush, gridForeColor, gridBackColor);

                if (root is System.Windows.Controls.Control rootControl)
                {
                    rootControl.Background = backColor.A > 0
                        ? new System.Windows.Media.SolidColorBrush(Color.FromArgb(backColor.A, backColor.R, backColor.G, backColor.B))
                        : (System.Windows.Media.Brush)null;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("EditorColorHelper.Apply failed: " + ex);
            }
        }

        private static void ApplyRecursive(
            DependencyObject node,
            System.Windows.Media.Brush foreBrush,
            System.Drawing.Color gridForeColor,
            System.Drawing.Color gridBackColor)
        {
            if (foreBrush != null)
            {
                if (node is System.Windows.Controls.Label lbl)
                    lbl.Foreground = foreBrush;
                else if (node is System.Windows.Controls.CheckBox cb)
                    cb.Foreground = foreBrush;
            }

            if (node is WindowsFormsHost host && host.Child is System.Windows.Forms.DataGridView grid)
            {
                ApplyGridColors(grid, gridForeColor, gridBackColor);
                return; // no children to recurse into from a WindowsFormsHost
            }

            IEnumerable children = LogicalTreeHelper.GetChildren(node);
            foreach (object child in children)
            {
                if (child is DependencyObject d)
                    ApplyRecursive(d, foreBrush, gridForeColor, gridBackColor);
            }
        }

        private static void ApplyGridColors(System.Windows.Forms.DataGridView grid, System.Drawing.Color fore, System.Drawing.Color back)
        {
            byte Clamp(int v) => (byte)Math.Max(0, Math.Min(255, v));

            grid.BackgroundColor = back;
            grid.DefaultCellStyle.BackColor = back;
            grid.DefaultCellStyle.ForeColor = fore;

            // Keep rows and alternating rows in sync with the VS theme foreground/background
            // so no hardcoded colour set elsewhere can make text unreadable.
            grid.RowsDefaultCellStyle.BackColor = back;
            grid.RowsDefaultCellStyle.ForeColor = fore;
            grid.AlternatingRowsDefaultCellStyle.BackColor = back;
            grid.AlternatingRowsDefaultCellStyle.ForeColor = fore;

            grid.ColumnHeadersDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(
                back.A, Clamp(back.R - 15), Clamp(back.G - 15), Clamp(back.B - 15));
            grid.ColumnHeadersDefaultCellStyle.ForeColor = fore;
            grid.GridColor = System.Drawing.Color.FromArgb(
                back.A, Clamp(back.R + 30), Clamp(back.G + 30), Clamp(back.B + 30));
            grid.EnableHeadersVisualStyles = false;
        }
    }
}
