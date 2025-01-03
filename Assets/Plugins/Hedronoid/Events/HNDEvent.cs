﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hedronoid.Events
{
    /// <summary>
    /// Base event all other events must inherit from.
    /// sender doesn't need to be set, but it's a good practice
    /// </summary>
    [Serializable]
    public class HNDBaseEvent
    {
        public GameObject sender;
    }
}