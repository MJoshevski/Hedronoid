using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Hedronoid
{
    public class BaseSceneContext : HNDGameObject
    {
        public virtual string LevelName { get { return "Undefined"; } }

        private byte m_LevelId = 255;
        public byte LevelId
        {
            get
            {
                return m_LevelId;
            }
        }
    }

    public interface IBaseSceneContextInjector
    {
        BaseSceneContext BaseSceneContext { get; set; }
    }

    public static class BaseSceneContextInjectorExtensions
    {
        public static void Inject(this IBaseSceneContextInjector context, GameObject gameObject)
        {
            context.BaseSceneContext = gameObject.GetComponentInParent<BaseSceneContext>(true);

            if (context.BaseSceneContext == null)
            {
                D.CoreWarning("Missing BaseSceneContext in scene!");
                return;
            }
        }
    }
}