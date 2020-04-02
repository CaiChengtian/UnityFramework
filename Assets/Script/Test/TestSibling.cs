/*
	Copyright (c) 20xx XXX. All rights reserved.
	Description:
*/
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace UnityFramework
{
    public class TestSibling : MonoBehaviour
    {
        void Update()
        {
            if (Input.GetKey(KeyCode.Space))
            {
                transform.SetSiblingIndex(1);
            }
        }
    }
}
