/*
	Copyright (c) 2017 Tiantian. All rights reserved.
	Description:
*/
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityFramework;

public class LoginSend : BaseSend
{
    public override Protocol Protocol
    {
        get
        {
            return Protocol.System_Login;
        }
    }

    public string Account;
    public string Password;

}
