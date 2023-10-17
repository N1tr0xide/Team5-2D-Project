using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugCommandBase
{
    public string CommandId { get; }
    public string CommandDescription { get; }
    public string CommandFormat { get; }

    public DebugCommandBase(string id, string description, string format)
    {
        CommandId = id;
        CommandDescription = description;
        CommandFormat = format;
    }
}

public class DebugCommand : DebugCommandBase
{
    private Action _command;

    public DebugCommand(string id, string description, string format, Action command) : base(id, description, format)
    {
        this._command = command;
    }

    public void Invoke()
    {
        _command.Invoke();
    }
}

public class DebugCommand<TP> : DebugCommandBase
{
    private Action<TP> _command;

    public DebugCommand(string id, string description, string format, Action<TP> command) : base(id, description, format)
    {
        this._command = command;
    }

    public void Invoke(TP value)
    {
        _command.Invoke(value);
    }
}
