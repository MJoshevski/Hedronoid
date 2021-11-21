using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleList
{
	/// PARTICLELIST_START

	public enum ParticleSystems {
		NONE = 0,
		HIT15 = 1191342186,
		HIT16 = 710524136,
		HIT17 = 776233840,
		HIT18 = 509621650,
		HIT19 = 1253416725,
		HIT20 = 770489384,
		HIT21 = 540995071,
		HIT22 = 1064138810,
		HIT23 = 1497775562,
		HIT24 = 285458243,
		HIT25 = 1174790548,
		HIT26 = 318633435,
		HIT27 = 2050828858,
		HIT1 = 1992848276,
		HIT2 = 531107233,
		HIT3 = 1644591277,
		HIT4 = 1912203826,
		HIT5 = 1150835575,
		HIT6 = 453155657,
		HIT7 = 1242592173,
		HIT8 = 1946591420,
		HIT9 = 1925383849,
		HIT10 = 2098469744,
		HIT11 = 886090313,
		HIT12 = 1972553506,
		HIT13 = 529406270,
		HIT14 = 1109173971,
		FLASH1 = 1940895310,
		FLASH2 = 275131673,
		FLASH3 = 2135857625,
		FLASH4 = 1489210072,
		FLASH5 = 1456493153,
		FLASH6 = 2013244276,
		FLASH7 = 2088004626,
		FLASH9 = 941999223,
		FLASH10 = 1815290767,
		FLASH11 = 1721608539,
		FLASH12 = 970368403,
		FLASH13 = 801558699,
		FLASH14 = 1511827858,
		FLASH15 = 1363156208,
		FLASH16 = 1563875683,
		FLASH17 = 1147929071,
		FLASH19 = 2063833154,
		FLASH22 = 720717015,
		FLASH23 = 1097037110,
		FLASH24 = 1705275283,
		FLASH25 = 42058108,
		FLASH26 = 38987292,
		FLASH27 = 1480774030,
		PROJECTILE1 = 517799401,
		PROJECTILE2 = 316308806,
		PROJECTILE3 = 128787250,
		PROJECTILE4 = 1227703537,
		PROJECTILE5 = 820499359,
		PROJECTILE6 = 1905688741,
		PROJECTILE7 = 533488101,
		PROJECTILE8 = 1013271746,
		PROJECTILE9 = 839914481,
		PROJECTILE10 = 2001086137,
		PROJECTILE11 = 192514766,
		PROJECTILE12 = 493669416,
		PROJECTILE13 = 793891820,
		PROJECTILE14 = 1776558884,
		PROJECTILE15 = 772596531,
		PROJECTILE16 = 1434062629,
		PROJECTILE17 = 1955938562,
		PROJECTILE18 = 1008769144,
		PROJECTILE19 = 1979513251,
		PROJECTILE20 = 1895492385,
		PROJECTILE21 = 775343993,
		PROJECTILE22 = 376248830,
		PROJECTILE23 = 80663422,
		PROJECTILE24 = 600480562,
		PROJECTILE25 = 991939874,
		PROJECTILE26 = 1162062203,
		PROJECTILE27 = 929357721,
	}

	public static string Get(ParticleSystems ps){
	string particleId = null;
	switch(ps){
		case ParticleSystems.HIT15:
			particleId = "Hit15";
			break;
		case ParticleSystems.HIT16:
			particleId = "Hit16";
			break;
		case ParticleSystems.HIT17:
			particleId = "Hit17";
			break;
		case ParticleSystems.HIT18:
			particleId = "Hit18";
			break;
		case ParticleSystems.HIT19:
			particleId = "Hit19";
			break;
		case ParticleSystems.HIT20:
			particleId = "Hit20";
			break;
		case ParticleSystems.HIT21:
			particleId = "Hit21";
			break;
		case ParticleSystems.HIT22:
			particleId = "Hit22";
			break;
		case ParticleSystems.HIT23:
			particleId = "Hit23";
			break;
		case ParticleSystems.HIT24:
			particleId = "Hit24";
			break;
		case ParticleSystems.HIT25:
			particleId = "Hit25";
			break;
		case ParticleSystems.HIT26:
			particleId = "Hit26";
			break;
		case ParticleSystems.HIT27:
			particleId = "Hit27";
			break;
		case ParticleSystems.HIT1:
			particleId = "Hit1";
			break;
		case ParticleSystems.HIT2:
			particleId = "Hit2";
			break;
		case ParticleSystems.HIT3:
			particleId = "Hit3";
			break;
		case ParticleSystems.HIT4:
			particleId = "Hit4";
			break;
		case ParticleSystems.HIT5:
			particleId = "Hit5";
			break;
		case ParticleSystems.HIT6:
			particleId = "Hit6";
			break;
		case ParticleSystems.HIT7:
			particleId = "Hit7";
			break;
		case ParticleSystems.HIT8:
			particleId = "Hit8";
			break;
		case ParticleSystems.HIT9:
			particleId = "Hit9";
			break;
		case ParticleSystems.HIT10:
			particleId = "Hit10";
			break;
		case ParticleSystems.HIT11:
			particleId = "Hit11";
			break;
		case ParticleSystems.HIT12:
			particleId = "Hit12";
			break;
		case ParticleSystems.HIT13:
			particleId = "Hit13";
			break;
		case ParticleSystems.HIT14:
			particleId = "Hit14";
			break;
		case ParticleSystems.FLASH1:
			particleId = "Flash1";
			break;
		case ParticleSystems.FLASH2:
			particleId = "Flash2";
			break;
		case ParticleSystems.FLASH3:
			particleId = "Flash3";
			break;
		case ParticleSystems.FLASH4:
			particleId = "Flash4";
			break;
		case ParticleSystems.FLASH5:
			particleId = "Flash5";
			break;
		case ParticleSystems.FLASH6:
			particleId = "Flash6";
			break;
		case ParticleSystems.FLASH7:
			particleId = "Flash7";
			break;
		case ParticleSystems.FLASH9:
			particleId = "Flash9";
			break;
		case ParticleSystems.FLASH10:
			particleId = "Flash10";
			break;
		case ParticleSystems.FLASH11:
			particleId = "Flash11";
			break;
		case ParticleSystems.FLASH12:
			particleId = "Flash12";
			break;
		case ParticleSystems.FLASH13:
			particleId = "Flash13";
			break;
		case ParticleSystems.FLASH14:
			particleId = "Flash14";
			break;
		case ParticleSystems.FLASH15:
			particleId = "Flash15";
			break;
		case ParticleSystems.FLASH16:
			particleId = "Flash16";
			break;
		case ParticleSystems.FLASH17:
			particleId = "Flash17";
			break;
		case ParticleSystems.FLASH19:
			particleId = "Flash19";
			break;
		case ParticleSystems.FLASH22:
			particleId = "Flash22";
			break;
		case ParticleSystems.FLASH23:
			particleId = "Flash23";
			break;
		case ParticleSystems.FLASH24:
			particleId = "Flash24";
			break;
		case ParticleSystems.FLASH25:
			particleId = "Flash25";
			break;
		case ParticleSystems.FLASH26:
			particleId = "Flash26";
			break;
		case ParticleSystems.FLASH27:
			particleId = "Flash27";
			break;
		case ParticleSystems.PROJECTILE1:
			particleId = "Projectile1";
			break;
		case ParticleSystems.PROJECTILE2:
			particleId = "Projectile2";
			break;
		case ParticleSystems.PROJECTILE3:
			particleId = "Projectile3";
			break;
		case ParticleSystems.PROJECTILE4:
			particleId = "Projectile4";
			break;
		case ParticleSystems.PROJECTILE5:
			particleId = "Projectile5";
			break;
		case ParticleSystems.PROJECTILE6:
			particleId = "Projectile6";
			break;
		case ParticleSystems.PROJECTILE7:
			particleId = "Projectile7";
			break;
		case ParticleSystems.PROJECTILE8:
			particleId = "Projectile8";
			break;
		case ParticleSystems.PROJECTILE9:
			particleId = "Projectile9";
			break;
		case ParticleSystems.PROJECTILE10:
			particleId = "Projectile10";
			break;
		case ParticleSystems.PROJECTILE11:
			particleId = "Projectile11";
			break;
		case ParticleSystems.PROJECTILE12:
			particleId = "Projectile12";
			break;
		case ParticleSystems.PROJECTILE13:
			particleId = "Projectile13";
			break;
		case ParticleSystems.PROJECTILE14:
			particleId = "Projectile14";
			break;
		case ParticleSystems.PROJECTILE15:
			particleId = "Projectile15";
			break;
		case ParticleSystems.PROJECTILE16:
			particleId = "Projectile16";
			break;
		case ParticleSystems.PROJECTILE17:
			particleId = "Projectile17";
			break;
		case ParticleSystems.PROJECTILE18:
			particleId = "Projectile18";
			break;
		case ParticleSystems.PROJECTILE19:
			particleId = "Projectile19";
			break;
		case ParticleSystems.PROJECTILE20:
			particleId = "Projectile20";
			break;
		case ParticleSystems.PROJECTILE21:
			particleId = "Projectile21";
			break;
		case ParticleSystems.PROJECTILE22:
			particleId = "Projectile22";
			break;
		case ParticleSystems.PROJECTILE23:
			particleId = "Projectile23";
			break;
		case ParticleSystems.PROJECTILE24:
			particleId = "Projectile24";
			break;
		case ParticleSystems.PROJECTILE25:
			particleId = "Projectile25";
			break;
		case ParticleSystems.PROJECTILE26:
			particleId = "Projectile26";
			break;
		case ParticleSystems.PROJECTILE27:
			particleId = "Projectile27";
			break;
	}
	return particleId;
	}
/// PARTICLELIST_END
}
