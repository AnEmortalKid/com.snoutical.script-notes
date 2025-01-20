using System;

namespace Snoutical.ScriptNotes
{
    /// <summary>
    /// Attaches a note to this script, which will be visible in an editor window
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class ScriptNoteAttribute : Attribute
    {
        public string Note { get; }

        /// <summary>
        /// Attaches a note to this script
        /// </summary>
        /// <param name="note">the text note to attach</param>
        public ScriptNoteAttribute(string note) => Note = note;
    }
}