using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleList
{
	/// PARTICLELIST_START

	public enum ParticleSystems {
		NONE = 0,
		BIGEXPLOSION_HNDPFX = 597627626,
		ENERGYEXPLOSION_HNDPFX = 719155896,
		PLAYERDASHSTART_HNDPFX = 1328606610,
		PLAYERDASHTRAIL_HNDPFX = 336653277,
	}

	public static string Get(ParticleSystems ps){
	string particleId = null;
	switch(ps){
		case ParticleSystems.BIGEXPLOSION_HNDPFX:
			particleId = "BigExplosion_HNDPFX";
			break;
		case ParticleSystems.ENERGYEXPLOSION_HNDPFX:
			particleId = "EnergyExplosion_HNDPFX";
			break;
		case ParticleSystems.PLAYERDASHSTART_HNDPFX:
			particleId = "PlayerDashStart_HNDPFX";
			break;
		case ParticleSystems.PLAYERDASHTRAIL_HNDPFX:
			particleId = "PlayerDashTrail_HNDPFX";
			break;
	}
	return particleId;
	}
/// PARTICLELIST_END
}
