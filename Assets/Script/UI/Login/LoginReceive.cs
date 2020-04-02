using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityFramework;

public class LoginReceive : BaseReceive
{
    public override Protocol Protocol
    {
        get
        {
            return Protocol.System_Login;
        }
    }
    public bool IsSuccess
    {
        get;
        set;
    }
    public string Name
    {
        get;
        set;
    }
    public int Level
    {
        get;
        set;
    }
}
