using Rhino;
using Rhino.Commands;
using Rhino.Geometry;
using Rhino.Input;
using Rhino.Input.Custom;
using System;
using System.Collections.Generic;
using UI_01_BASIC_MVVM.Examples._01_MVVM_WINDOW.View;

namespace UI_01_BASIC_MVVM
{
    public class UI_01_BASIC_MVVM_Command : Command
    {
        public UI_01_BASIC_MVVM_Command()
        {
            // Rhino only creates one instance of each command class defined in a
            // plug-in, so it is safe to store a refence in a static property.
            Instance = this;
        }

        ///<summary>The only instance of this command.</summary>
        public static UI_01_BASIC_MVVM_Command Instance { get; private set; }

        ///<returns>The command name as it appears on the Rhino command line.</returns>
        public override string EnglishName => "UI_01_BASIC_MVVM_Command";

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {

            var window = new AddBrepView(doc);


            window.Show();

            var window2 = new Window1();
            window2.Show();

            return Result.Success;
        }
    }
}
