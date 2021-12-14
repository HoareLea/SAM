using System.ComponentModel;

namespace SAM.Core
{
    /// <summary>
    /// Coordinated Universal Time (UTC) Time Zones
    /// </summary>
    public enum UTC
    {
        [Description("Undefined")] Undefined,
        [Description("UTC−12:00")] Minus1200,
        [Description("UTC−11:00")] Minus1100,
        [Description("UTC−10:00")] Minus1000,
        [Description("UTC−09:30")] Minus0930,
        [Description("UTC−09:00")] Minus0900,
        [Description("UTC−08:00")] Minus0800,
        [Description("UTC−07:00")] Minus0700,
        [Description("UTC−06:00")] Minus0600,
        [Description("UTC−05:00")] Minus0500,
        [Description("UTC−04:00")] Minus0400,
        [Description("UTC−03:30")] Minus0330,
        [Description("UTC−03:00")] Minus0300,
        [Description("UTC−02:00")] Minus0200,
        [Description("UTC−01:00")] Minus0100,
        [Description("UTC±00:00")] PlusMinus0000,
        [Description("UTC+01:00")] Plus0100,
        [Description("UTC+02:00")] Plus0200,
        [Description("UTC+03:00")] Plus0300,
        [Description("UTC+03:30")] Plus0330,
        [Description("UTC+04:00")] Plus0400,
        [Description("UTC+04:30")] Plus0430,
        [Description("UTC+05:00")] Plus0500,
        [Description("UTC+05:30")] Plus0530,
        [Description("UTC+05:45")] Plus0545,
        [Description("UTC+06:00")] Plus0600,
        [Description("UTC+06:30")] Plus0630,
        [Description("UTC+07:00")] Plus0700,
        [Description("UTC+08:00")] Plus0800,
        [Description("UTC+08:45")] Plus0845,
        [Description("UTC+09:00")] Plus0900,
        [Description("UTC+09:30")] Plus0930,
        [Description("UTC+10:00")] Plus1000,
        [Description("UTC+10:30")] Plus1030,
        [Description("UTC+11:00")] Plus1100,
        [Description("UTC+12:00")] Plus1200,
        [Description("UTC+12:45")] Plus1245,
        [Description("UTC+13:00")] Plus1300,
        [Description("UTC+14:00")] Plus1400
    }
}
