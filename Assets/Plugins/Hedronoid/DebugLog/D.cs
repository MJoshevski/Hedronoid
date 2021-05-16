using UnityEngine;
using System;
using UnityDBG;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class D
{

    public delegate void DebugLogger(object message, string tag = "");
    public delegate void DebugFormatLogger(string tag, string format, object[] formatParams);
    public delegate void DebugContextLogger(object message, UnityEngine.Object context, string tag = "");

    public const string CATEGORY_GAME = "Game";
    public const string CATEGORY_CORE = "Core";
    public const string CATEGORY_NETWORK = "Network";
    public const string CATEGORY_AUDIO = "Audio";
    public const string CATEGORY_HNDLoc = "HNDLoc";
    public const string CATEGORY_UI = "UI";
    public const string CATEGORY_AI = "AI";
    public const string CATEGORY_GRAVITY = "Gravity";
    public const string CATEGORY_PROFILE = "Profile";

#if UNITY_EDITOR
    private string m_AssetPath = "Assets/Resources/DBGTags.asset";
#endif

    public static bool _initialized = false;

    private static D s_instance;
    private static D sInstance
    {
        get
        {
            if (s_instance == null) s_instance = new D();
            return s_instance;
        }
    }

    private static bool m_LoggingEnabled = true;
    public static bool LoggingEnabled
    {
        get { return m_LoggingEnabled; }
        set
        {
            m_LoggingEnabled = value;
            if (_initialized)
            {
                DBG.LoggingEnabled = value;
            }
        }
    }

    private static bool m_PrintTime = true;
    public static bool PrintTime
    {
        get { return m_PrintTime; }
        set
        {
            m_PrintTime = value;
            if (_initialized)
            {
                DBG.PrintTime = value;
            }
        }
    }

    public static void Init()
    {
        Debug.Log("Initializing DBG-logging");

        if (!DBG.DbgTags) SetDBGTagReference();
        if (DBG.DbgTags) DBG.UpdateTags();

        _initialized = true;
        LoggingEnabled = m_LoggingEnabled;
        PrintTime = m_PrintTime;
    }

    private static void SetDBGTagReference()
    {
        // DBGTags dbgTags = AssetDatabase.LoadAssetAtPath<DBGTags>(sInstance.assetPath);
        var dbgTags = Resources.Load<DBGTags>("DBGTags");
#if UNITY_EDITOR
        if (dbgTags == null)
        {
            dbgTags = ScriptableObject.CreateInstance<DBGTags>();
            AssetDatabase.CreateAsset(dbgTags, sInstance.m_AssetPath);
        }
#endif
        UnityDBG.DBG.DbgTags = dbgTags;
    }



    public static DebugLogger Log
    {
        get
        {
            if (!_initialized) Init();
            return DBG.Log;
        }
    }

    public static DebugContextLogger LogContext
    {
        get
        {
            if (!_initialized) Init();
            return DBG.Log;
        }
    }

    public static DebugLogger LogWarning
    {
        get
        {
            if (!_initialized) Init();
            return DBG.LogWarning;
        }
    }
    public static DebugContextLogger LogContextWarning
    {
        get
        {
            if (!_initialized) Init();
            return DBG.LogWarning;
        }
    }

    public static DebugLogger LogError
    {
        get
        {
            if (!_initialized) Init();
            return DBG.LogError;
        }
    }
    public static DebugContextLogger LogContextError
    {
        get
        {
            if (!_initialized) Init();
            return DBG.LogError;
        }
    }


    public static DBG.TaggedLogger NetLog { get { if (!_initialized) Init(); return DBG.GetTaggedLog(CATEGORY_NETWORK); } }
    public static DBG.TaggedFormatLogger NetLogFormat { get { if (!_initialized) Init(); return DBG.GetTaggedLogFormat(CATEGORY_NETWORK); } }
    public static DBG.TaggedLogger NetWarning { get { if (!_initialized) Init(); return DBG.GetTaggedWarning(CATEGORY_NETWORK); } }
    public static DBG.TaggedFormatLogger NetWarningFormat { get { if (!_initialized) Init(); return DBG.GetTaggedWarningFormat(CATEGORY_NETWORK); } }
    public static DBG.TaggedLogger NetError { get { if (!_initialized) Init(); return DBG.GetTaggedError(CATEGORY_NETWORK); } }
    public static DBG.TaggedFormatLogger NetErrorFormat { get { if (!_initialized) Init(); return DBG.GetTaggedErrorFormat(CATEGORY_NETWORK); } }


    public static DBG.TaggedLogger GameLog { get { if (!_initialized) Init(); return DBG.GetTaggedLog(CATEGORY_GAME); } }
    public static DBG.TaggedFormatLogger GameLogFormat { get { if (!_initialized) Init(); return DBG.GetTaggedLogFormat(CATEGORY_GAME); } }
    public static DBG.TaggedLogger GameWarning { get { if (!_initialized) Init(); return DBG.GetTaggedWarning(CATEGORY_GAME); } }
    public static DBG.TaggedFormatLogger GameWarningFormat { get { if (!_initialized) Init(); return DBG.GetTaggedWarningFormat(CATEGORY_GAME); } }
    public static DBG.TaggedLogger GameError { get { if (!_initialized) Init(); return DBG.GetTaggedError(CATEGORY_GAME); } }
    public static DBG.TaggedFormatLogger GameErrorFormat { get { if (!_initialized) Init(); return DBG.GetTaggedErrorFormat(CATEGORY_GAME); } }



    public static DBG.TaggedLogger CoreLog { get { if (!_initialized) Init(); return DBG.GetTaggedLog(CATEGORY_CORE); } }
    public static DBG.TaggedFormatLogger CoreLogFormat { get { if (!_initialized) Init(); return DBG.GetTaggedLogFormat(CATEGORY_CORE); } }
    public static DBG.TaggedLogger CoreWarning { get { if (!_initialized) Init(); return DBG.GetTaggedWarning(CATEGORY_CORE); } }
    public static DBG.TaggedFormatLogger CoreWarningFormat { get { if (!_initialized) Init(); return DBG.GetTaggedWarningFormat(CATEGORY_CORE); } }
    public static DBG.TaggedLogger CoreError { get { if (!_initialized) Init(); return DBG.GetTaggedError(CATEGORY_CORE); } }
    public static DBG.TaggedFormatLogger CoreErrorFormat { get { if (!_initialized) Init(); return DBG.GetTaggedErrorFormat(CATEGORY_CORE); } }


    public static DBG.TaggedLogger AudioLog { get { if (!_initialized) Init(); return DBG.GetTaggedLog(CATEGORY_AUDIO); } }
    public static DBG.TaggedFormatLogger AudioLogFormat { get { if (!_initialized) Init(); return DBG.GetTaggedLogFormat(CATEGORY_AUDIO); } }
    public static DBG.TaggedLogger AudioWarning { get { if (!_initialized) Init(); return DBG.GetTaggedWarning(CATEGORY_AUDIO); } }
    public static DBG.TaggedFormatLogger AudioWarningFormat { get { if (!_initialized) Init(); return DBG.GetTaggedWarningFormat(CATEGORY_AUDIO); } }
    public static DBG.TaggedLogger AudioError { get { if (!_initialized) Init(); return DBG.GetTaggedError(CATEGORY_AUDIO); } }
    public static DBG.TaggedFormatLogger AudioErrorFormat { get { if (!_initialized) Init(); return DBG.GetTaggedErrorFormat(CATEGORY_AUDIO); } }


    public static DBG.TaggedLogger LocLog { get { if (!_initialized) Init(); return DBG.GetTaggedLog(CATEGORY_HNDLoc); } }
    public static DBG.TaggedFormatLogger LocLogFormat { get { if (!_initialized) Init(); return DBG.GetTaggedLogFormat(CATEGORY_HNDLoc); } }
    public static DBG.TaggedLogger LocWarning { get { if (!_initialized) Init(); return DBG.GetTaggedWarning(CATEGORY_HNDLoc); } }
    public static DBG.TaggedFormatLogger LocWarningFormat { get { if (!_initialized) Init(); return DBG.GetTaggedWarningFormat(CATEGORY_HNDLoc); } }
    public static DBG.TaggedLogger LocError { get { if (!_initialized) Init(); return DBG.GetTaggedError(CATEGORY_HNDLoc); } }
    public static DBG.TaggedFormatLogger LocErrorFormat { get { if (!_initialized) Init(); return DBG.GetTaggedErrorFormat(CATEGORY_HNDLoc); } }



    public static DBG.TaggedLogger UILog { get { if (!_initialized) Init(); return DBG.GetTaggedLog(CATEGORY_UI); } }
    public static DBG.TaggedFormatLogger UILogFormat { get { if (!_initialized) Init(); return DBG.GetTaggedLogFormat(CATEGORY_UI); } }
    public static DBG.TaggedLogger UIWarning { get { if (!_initialized) Init(); return DBG.GetTaggedWarning(CATEGORY_UI); } }
    public static DBG.TaggedFormatLogger UIWarningFormat { get { if (!_initialized) Init(); return DBG.GetTaggedWarningFormat(CATEGORY_UI); } }
    public static DBG.TaggedLogger UIError { get { if (!_initialized) Init(); return DBG.GetTaggedError(CATEGORY_UI); } }
    public static DBG.TaggedFormatLogger UIErrorFormat { get { if (!_initialized) Init(); return DBG.GetTaggedErrorFormat(CATEGORY_UI); } }


    public static DBG.TaggedLogger ProfileLog { get { if (!_initialized) Init(); return DBG.GetTaggedLog(CATEGORY_PROFILE); } }
    public static DBG.TaggedFormatLogger ProfileLogFormat { get { if (!_initialized) Init(); return DBG.GetTaggedLogFormat(CATEGORY_PROFILE); } }
    public static DBG.TaggedLogger ProfileWarning { get { if (!_initialized) Init(); return DBG.GetTaggedWarning(CATEGORY_PROFILE); } }
    public static DBG.TaggedFormatLogger ProfileWarningFormat { get { if (!_initialized) Init(); return DBG.GetTaggedWarningFormat(CATEGORY_PROFILE); } }
    public static DBG.TaggedLogger ProfileError { get { if (!_initialized) Init(); return DBG.GetTaggedError(CATEGORY_PROFILE); } }
    public static DBG.TaggedFormatLogger ProfileErrorFormat { get { if (!_initialized) Init(); return DBG.GetTaggedErrorFormat(CATEGORY_PROFILE); } }
    

    public static DBG.TaggedLogger GravLog { get { if (!_initialized) Init(); return DBG.GetTaggedLog(CATEGORY_GRAVITY); } }
    public static DBG.TaggedFormatLogger GravLogFormat { get { if (!_initialized) Init(); return DBG.GetTaggedLogFormat(CATEGORY_GRAVITY); } }
    public static DBG.TaggedLogger GravWarning { get { if (!_initialized) Init(); return DBG.GetTaggedWarning(CATEGORY_GRAVITY); } }
    public static DBG.TaggedFormatLogger GravWarningFormat { get { if (!_initialized) Init(); return DBG.GetTaggedWarningFormat(CATEGORY_GRAVITY); } }
    public static DBG.TaggedLogger GravError { get { if (!_initialized) Init(); return DBG.GetTaggedError(CATEGORY_GRAVITY); } }
    public static DBG.TaggedFormatLogger GravErrorFormat { get { if (!_initialized) Init(); return DBG.GetTaggedErrorFormat(CATEGORY_GRAVITY); } }


    public static DBG.TaggedLogger AILog { get { if (!_initialized) Init(); return DBG.GetTaggedLog(CATEGORY_AI); } }
    public static DBG.TaggedFormatLogger AILogFormat { get { if (!_initialized) Init(); return DBG.GetTaggedLogFormat(CATEGORY_AI); } }
    public static DBG.TaggedLogger AIWarning { get { if (!_initialized) Init(); return DBG.GetTaggedWarning(CATEGORY_AI); } }
    public static DBG.TaggedFormatLogger AIWarningFormat { get { if (!_initialized) Init(); return DBG.GetTaggedWarningFormat(CATEGORY_AI); } }
    public static DBG.TaggedLogger AIError { get { if (!_initialized) Init(); return DBG.GetTaggedError(CATEGORY_AI); } }
    public static DBG.TaggedFormatLogger AIErrorFormat { get { if (!_initialized) Init(); return DBG.GetTaggedErrorFormat(CATEGORY_AI); } }

}
