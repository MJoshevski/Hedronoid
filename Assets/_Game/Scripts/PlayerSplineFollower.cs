using Dreamteck;
using Dreamteck.Splines;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hedronoid.Player
{    
    public class PlayerSplineFollower : SplineFollower
    {
        public new event SplineReachHandler onEndReached;
        public new event SplineReachHandler onBeginningReached;

        protected override void Start()
        {
        }

        protected override void LateRun()
        {
 
#if UNITY_EDITOR
            if (!Application.isPlaying) return;
#endif
        }

        protected override void PostBuild()
        {
            base.PostBuild();
            if (sampleCount == 0) return;
            Evaluate(_result.percent, _result);
            if (follow && !autoStartPosition) ApplyMotion();
        }

        public override void Follow()
        {
            if (!follow) return; 

            if (!followStarted)
            {
                if (autoStartPosition)
                {
                    Project(GetTransform().position, evalResult);
                    SetPercent(evalResult.percent);
                }
                else SetPercent(_startPosition);
            }

            followStarted = true;

            switch (followMode)
            {
                case FollowMode.Uniform: Move(Time.deltaTime * _followSpeed); break;
                case FollowMode.Time:
                    if (_followDuration == 0.0) Move(0.0);
                    else Move((double)Time.deltaTime / _followDuration);
                    break;
            }
        }

        public override void Restart(double startPosition = 0.0)
        {
            followStarted = false;
            SetPercent(startPosition);
        }

        public override void SetPercent(double percent, bool checkTriggers = false, bool handleJuncitons = false)
        {
            base.SetPercent(percent, checkTriggers, handleJuncitons);
            lastClippedPercent = percent;
        }

        public override void SetDistance(float distance, bool checkTriggers = false, bool handleJuncitons = false)
        {
            base.SetDistance(distance, checkTriggers, handleJuncitons);
            lastClippedPercent = ClipPercent(_result.percent);
            if (samplesAreLooped && clipFrom == clipTo && distance > 0f && lastClippedPercent == 0.0) lastClippedPercent = 1.0;
        }

        public override void Move(double percent)
        {
            if (percent == 0.0) return;
            if (sampleCount <= 1)
            {
                if (sampleCount == 1)
                {
                    _result.CopyFrom(GetSampleRaw(0));
                    ApplyMotion();
                }
                return;
            }
            Evaluate(_result.percent, _result);
            double startPercent = _result.percent;
            if (wrapMode == Wrap.Default && lastClippedPercent >= 1.0 && startPercent == 0.0) startPercent = 1.0;
            double p = startPercent + (_direction == Spline.Direction.Forward ? percent : -percent);
            bool callOnEndReached = false, callOnBeginningReached = false;
            lastClippedPercent = p;
            if (_direction == Spline.Direction.Forward && p >= 1.0)
            {
                if (onEndReached != null && startPercent < 1.0) callOnEndReached = true;
                switch (wrapMode)
                {
                    case Wrap.Default:
                        p = 1.0;
                        break;
                    case Wrap.Loop:
                        CheckTriggers(startPercent, 1.0);
                        CheckNodes(startPercent, 1.0);
                        while (p > 1.0) p -= 1.0;
                        startPercent = 0.0;
                        break;
                    case Wrap.PingPong:
                        p = DMath.Clamp01(1.0 - (p - 1.0));
                        startPercent = 1.0;
                        _direction = Spline.Direction.Backward;
                        break;
                }
            }
            else if (_direction == Spline.Direction.Backward && p <= 0.0)
            {
                if (onBeginningReached != null && startPercent > 0.0) callOnBeginningReached = true;
                switch (wrapMode)
                {
                    case Wrap.Default:
                        p = 0.0;
                        break;
                    case Wrap.Loop:
                        CheckTriggers(startPercent, 0.0);
                        CheckNodes(startPercent, 0.0);
                        while (p < 0.0) p += 1.0;
                        startPercent = 1.0;
                        break;
                    case Wrap.PingPong:
                        p = DMath.Clamp01(-p);
                        startPercent = 0.0;
                        _direction = Spline.Direction.Forward;
                        break;
                }
            }

            CheckTriggers(startPercent, p);
            CheckNodes(startPercent, p);
            Evaluate(p, _result);
            RotateOnAxis();
            ApplyMotion();
            if (callOnEndReached) onEndReached();
            else if (callOnBeginningReached) onBeginningReached();
            InvokeTriggers();
            InvokeNodes();
        }

        public override void Move(float distance)
        {
            bool endReached = false, beginningReached = false;
            float moved = 0f;
            double startPercent = _result.percent;
            _result.percent = Travel(_result.percent, distance, _direction, out moved);
            if (startPercent != _result.percent)
            {
                CheckTriggers(startPercent, _result.percent);
                CheckNodes(startPercent, _result.percent);
            }
            if (moved < distance)
            {
                if (direction == Spline.Direction.Forward)
                {
                    if (_result.percent >= 1.0)
                    {

                        switch (wrapMode)
                        {
                            case Wrap.Loop:
                                _result.percent = Travel(0.0, distance - moved, _direction, out moved);
                                CheckTriggers(0.0, _result.percent);
                                CheckNodes(0.0, _result.percent);
                                break;
                            case Wrap.PingPong:
                                _direction = Spline.Direction.Backward;
                                _result.percent = Travel(1.0, distance - moved, _direction, out moved);
                                CheckTriggers(1.0, _result.percent);
                                CheckNodes(1.0, _result.percent);
                                break;
                        }
                        if (startPercent < _result.percent) endReached = true;

                    }
                }
                else
                {
                    if (_result.percent <= 0.0)
                    {
                        switch (wrapMode)
                        {
                            case Wrap.Loop:
                                _result.percent = Travel(1.0, distance - moved, _direction, out moved);
                                CheckTriggers(1.0, _result.percent);
                                CheckNodes(1.0, _result.percent);
                                break;
                            case Wrap.PingPong:
                                _direction = Spline.Direction.Forward;
                                _result.percent = Travel(0.0, distance - moved, _direction, out moved);
                                CheckTriggers(0.0, _result.percent);
                                CheckNodes(0.0, _result.percent);
                                break;
                        }
                        if (startPercent > _result.percent) beginningReached = true;
                    }
                }
            }
            Evaluate(_result.percent, _result);
            RotateOnAxis();
            ApplyMotion();
            if (endReached && onEndReached != null) onEndReached();
            else if (beginningReached && onBeginningReached != null) onBeginningReached();
            InvokeTriggers();
            InvokeNodes();
        }
    }
}

