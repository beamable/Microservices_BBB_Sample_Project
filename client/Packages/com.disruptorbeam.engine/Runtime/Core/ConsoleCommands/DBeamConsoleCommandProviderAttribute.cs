using System;
using UnityEngine.Scripting;

namespace Core.ConsoleCommands
{
    /// <summary>
    /// Used on a class to annotate the class as having console commands.
    /// The class must have an empty constructor, or no console commands will be loaded.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class DBeamConsoleCommandProviderAttribute : PreserveAttribute
    {

    }
}
