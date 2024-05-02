using Rhino;
using Rhino.Commands;
using Rhino.Geometry;
using Rhino.Input;
using Rhino.Input.Custom;
using System;
using System.Collections.Generic;
using System.Windows.Interop;
using UI_01_BASIC_MVVM.Examples._01_MVVM_WINDOW.View;

namespace UI_01_BASIC_MVVM.RhinoCommands
{
    public class UI_01_MVVM_WindowCommand : Command
    {
        public UI_01_MVVM_WindowCommand()
        {
            // Rhino only creates one instance of each command class defined in a
            // plug-in, so it is safe to store a refence in a static property.
            Instance = this;
        }

        ///<summary>The only instance of this command.</summary>
        public static UI_01_MVVM_WindowCommand Instance { get; private set; }

        ///<returns>The command name as it appears on the Rhino command line.</returns>
        public override string EnglishName => "UI_01_MVVM_WINDOW";

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            var window = new BrepOverviewView(doc);


            // Make sure that we are a child of the Rhino window!
            _ = new WindowInteropHelper(window)
            {
                Owner = RhinoApp.MainWindowHandle()
            };

            window.Show();

            return Result.Success;
        }
    }
}
