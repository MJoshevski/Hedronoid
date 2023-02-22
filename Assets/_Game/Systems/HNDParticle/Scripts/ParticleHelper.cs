using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hedronoid;
using Hedronoid.Particle;
using Hedronoid.Events;

public class ParticleHelper
{
    /// <summary>
    /// Play a particle system from the global particle manager.
    /// </summary>
    /// <param name="particleSystem">Enum identifier for the particle system to play.</param>
    /// <param name="position">The world position of where to play the particle system.</param>
    /// <param name="normalVector">The normal vector for the particle system. This is the direction in which the particles will move (only applies to directional systems).</param>
    /// <param name="duration">How long the particle system should play for. If you leave this value at the default (-1) or any negative value, the particle system will play for how long it's duration is set to.</param>
    /// <param name="keepRunning">If this is true, duration will be ignored and the particle system will never be auto-returned. Use this for looping particle systems that you want to decide when to return.</param>
    /// <param name="followTransform">Follow a transform, position and normal are relative if this is not null</param>
    /// <param name="ignoreFollowRotation">This is only valid if a 'follow transform' has been given. If this value is true, rotation will be ignored when following the 'follow transform'. Only the position will be updated in this case.</param>
    /// <returns>Reference to the instance of the particle system that was spawned.</returns>
    public static HNDParticleSystem PlayParticleSystem(ParticleList.ParticleSystems particleSystem, Vector3 position, Vector3 normalVector, float duration = -1, bool keepRunning = false, Transform followTransform = null, bool ignoreFollowRotation = false)
    {
        if (particleSystem == ParticleList.ParticleSystems.NONE)
            return null;
        string name = ParticleList.Get(particleSystem);
        return PlayParticleSystem(name, position, normalVector, duration, keepRunning, followTransform, ignoreFollowRotation);
    }

    /// <summary>
    /// Play a particle system from the global particle manager.
    /// </summary>
    /// <param name="particleSystem">Enum identifier for the particle system to play.</param>
    /// <param name="position">The world position of where to play the particle system.</param>
    /// <param name="normalVector">The normal vector for the particle system. This is the direction in which the particles will move (only applies to directional systems).</param>
    /// <param name="scale">The local scale of the particle system when played.</param>
    /// <param name="duration">How long the particle system should play for. If you leave this value at the default (-1) or any negative value, the particle system will play for how long it's duration is set to.</param>
    /// <param name="keepRunning">If this is true, duration will be ignored and the particle system will never be auto-returned. Use this for looping particle systems that you want to decide when to return.</param>
    /// <param name="followTransform">Follow a transform, position and normal are relative if this is not null</param>
    /// <param name="ignoreFollowRotation">This is only valid if a 'follow transform' has been given. If this value is true, rotation will be ignored when following the 'follow transform'. Only the position will be updated in this case.</param>
    /// <returns>Reference to the instance of the particle system that was spawned.</returns>
    public static HNDParticleSystem PlayParticleSystem(ParticleList.ParticleSystems particleSystem, Vector3 position, Vector3 normalVector, Vector3 scale, float duration = -1, bool keepRunning = false, Transform followTransform = null, bool ignoreFollowRotation = false)
    {
        if (particleSystem == ParticleList.ParticleSystems.NONE)
            return null;
        string name = ParticleList.Get(particleSystem);
        return PlayParticleSystem(name, position, normalVector, scale, duration, keepRunning, followTransform, ignoreFollowRotation);
    }

    /// <summary>
    /// Play a particle system from the global particle manager.
    /// </summary>
    /// <param name="particleSystemName">String identifier for the particle system to play.</param>
    /// <param name="position">The world position of where to play the particle system.</param>
    /// <param name="normalVector">The normal vector for the particle system. This is the direction in which the particles will move (only applies to directional systems).</param>
    /// <param name="duration">How long the particle system should play for. If you leave this value at the default (-1) or any negative value, the particle system will not be auto-returned. In that case you have to make sure to return it yourself.</param>
    /// <param name="keepRunning">If this is true, duration will be ignored and the particle system will never be auto-returned. Use this for looping particle systems that you want to decide when to return.</param>
    /// <param name="followTransform">Follow a transform, position and normal are relative if this is not null</param>
    /// <param name="ignoreFollowRotation">This is only valid if a 'follow transform' has been given. If this value is true, rotation will be ignored when following the 'follow transform'. Only the position will be updated in this case.</param>
    /// <returns>Reference to the instance of the particle system that was spawned.</returns>
    public static HNDParticleSystem PlayParticleSystem(string particleSystemName, Vector3 position, Vector3 normalVector, float duration = -1, bool keepRunning = false, Transform followTransform = null, bool ignoreFollowRotation = false)
    {
        ParticleManager.PlayConfig config = new ParticleManager.PlayConfig();
        config.Name = particleSystemName;
        config.Position = position;
        config.NormalVector = normalVector;
        config.Duration = duration;
        config.KeepRunning = keepRunning;
        config.FollowTransform = followTransform;
        config.IgnoreFollowRotation = ignoreFollowRotation;
        return PlayParticleSystem(config);
    }

    /// <summary>
    /// Play a particle system from the global particle manager.
    /// </summary>
    /// <param name="particleSystemName">String identifier for the particle system to play.</param>
    /// <param name="position">The world position of where to play the particle system.</param>
    /// <param name="normalVector">The normal vector for the particle system. This is the direction in which the particles will move (only applies to directional systems).</param>
    /// <param name="scale">The local scale of the particle system when played.</param>
    /// <param name="duration">How long the particle system should play for. If you leave this value at the default (-1) or any negative value, the particle system will not be auto-returned. In that case you have to make sure to return it yourself.</param>
    /// <param name="keepRunning">If this is true, duration will be ignored and the particle system will never be auto-returned. Use this for looping particle systems that you want to decide when to return.</param>
    /// <param name="followTransform">Follow a transform, position and normal are relative if this is not null</param>
    /// <param name="ignoreFollowRotation">This is only valid if a 'follow transform' has been given. If this value is true, rotation will be ignored when following the 'follow transform'. Only the position will be updated in this case.</param>
    /// <returns>Reference to the instance of the particle system that was spawned.</returns>
    public static HNDParticleSystem PlayParticleSystem(string particleSystemName, Vector3 position, Vector3 normalVector, Vector3 scale, float duration = -1, bool keepRunning = false, Transform followTransform = null, bool ignoreFollowRotation = false)
    {
        ParticleManager.PlayConfig config = new ParticleManager.PlayConfig();
        config.Name = particleSystemName;
        config.Position = position;
        config.NormalVector = normalVector;
        config.Scale = scale;
        config.Duration = duration;
        config.KeepRunning = keepRunning;
        config.FollowTransform = followTransform;
        config.IgnoreFollowRotation = ignoreFollowRotation;
        return PlayParticleSystem(config);
    }

    /// <summary>
    /// Play a particle system from the global particle manager.
    /// </summary>
    /// <param name="c">Configuration for the particle system to play.</param>
    /// <returns>Reference to the instance of the particle system that was spawned.</returns>
    public static HNDParticleSystem PlayParticleSystem(ParticleManager.PlayConfig c)
    {
        PlayParticleSystem p = new PlayParticleSystem { Config = c };
        HNDEvents.Instance.Raise(p);
        return p.ParticleSystemInstance;
    }

    public static void StopParticleSystem(HNDParticleSystem particleSystemInstance)
    {
        if (particleSystemInstance == null)
        {
            return;
        }

        HNDEvents.Instance.Raise(new StopParticleSystem { ParticleSystemInstance = particleSystemInstance });
    }
}