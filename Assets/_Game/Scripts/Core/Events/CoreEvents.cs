using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hedronoid;
using Hedronoid.Core;
using Hedronoid.Events;

//////////////////////////////
// GAME FLOW                //
//////////////////////////////
public class LevelUnloaded : HNDBaseEvent
{
}

public class StartLevel : HNDBaseEvent
{
}

public class PlayerCreatedAndInitialized : HNDBaseEvent
{
}


public class IntroStarted : HNDBaseEvent
{
}

public class IntroEnded : HNDBaseEvent
{
}

public class BattleStarted : HNDBaseEvent
{
}

public class BattleEnded : HNDBaseEvent
{
}

public class SkipIntro : HNDBaseEvent
{
}