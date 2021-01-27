using System;

namespace ThunderKit.Integrations.Thunderstore
{
    [Flags]
    public enum LogLevel
    {
        //Disables all log messages
        None = 0,
        //Errors which cannot be recovered from; the game cannot continue to run
        Fatal = 1,
        //Errors are recoverable from; the game can be run, albeit with further errors
        Error = 2,
        //Messages that signify an anomaly that is not an error
        Warning = 4,
        //Important messages that should be displayed
        Message = 8,
        //Messages of low importance
        Info = 16,
        //Messages intended for developers
        Debug = 32,

        //All = Fatal | Error | Warning | Message | Info | Debug
    }
}