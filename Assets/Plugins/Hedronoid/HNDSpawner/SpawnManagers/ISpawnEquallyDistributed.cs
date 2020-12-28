namespace Hedronoid.Spawners
{
    /// <summary>
    /// Ensures that a SpawnManager can equally distribute a number of items.
    /// These spawners need to know beforehand how many, and will be told using this interface.
    /// </summary>
    public interface ISpawnEquallyDistributed
    {
        /// <summary>
        /// Tells the SpawnManager the amount of items to allow them to equally distribute
        /// </summary>
        /// <param name="numberOfItems"></param>
        void SetAmountOfEquallyDistributedItems(int numberOfItems);
        /// <summary>
        /// Resets the SpawnManager progress in equal distribution
        /// </summary>
        void ResetEqualDistribution();
    }
}