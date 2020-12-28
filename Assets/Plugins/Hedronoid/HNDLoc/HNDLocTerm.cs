using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hedronoid.HNDLoc
{
    public enum HNDLocTermType
    {
        TEXT = 0,
        BUNDLE_NAME = 1,
    }

    public class HNDLocTerm
    {
        public string Key;

        public HNDLocTermType TermType;

        public string SourceText;

        public string Description;

        public override bool Equals (object obj)
        {
            //
            // See the full list of guidelines at
            //   http://go.microsoft.com/fwlink/?LinkID=85237
            // and also the guidance for operator== at
            //   http://go.microsoft.com/fwlink/?LinkId=85238
            //
            
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            return Key.Equals(((HNDLocTerm)obj).Key);
        }
        
        public override int GetHashCode()
        {
            return Key.GetHashCode();
        }
    }
}