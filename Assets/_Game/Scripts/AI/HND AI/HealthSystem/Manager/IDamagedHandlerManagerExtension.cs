using System;
using Hedronoid.Health;

public interface IDamagedHandlerManagerExtension
{
    bool MeetCondition(DamageHandler damagedHandler, DamageInfo damagedInfo, HealthBase health, HealthInfo healthInfo);
    void Handle(DamageHandler damagedHandler, DamageInfo damagedInfo, HealthBase health, HealthInfo healthInfo);
}