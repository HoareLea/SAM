using SAM.Core.Attributes;
using System.ComponentModel;

namespace SAM.Core
{
    [Description("Command Operator")]
    public enum CommandOperator
    {
        [Operator(null), Description("Undefined")] Undefined,
        [Operator("\'"), Description("Text")] Text,
        [Operator("("), Description("Opening Bracket")] OpeningBracket,
        [Operator(")"), Description("Closing Bracket")] ClosingBracket,
        [Operator("$"), Description("Object")] Object,
        [Operator("//"), Description("Comment")] Comment,
        [Operator("using"), Description("Directive")] Directive,
        [Operator("."), Description("Member Separator")] MemberSeparator,
    }
}