using Zeta.Game.Internals.Actors;

namespace QuestTools.Helpers
{
    public static class ActorExtensions
    {
        /// <summary>
        /// Returns if a DiaObject is not null, is valid, and it's ACD is not null, and is valid
        /// </summary>
        /// <param name="diaObject"></param>
        /// <returns></returns>
        public static bool IsFullyValid(this DiaObject diaObject)
        {
            return diaObject != null && diaObject.IsValid && diaObject.ACDId != 0 && diaObject.CommonData != null && diaObject.CommonData.IsValid;
        }

        /// <summary>
        /// Determines whether the actor is fully valid, with a special check for ACDItems
        /// </summary>
        /// <param name="actor">The actor.</param>
        /// <returns><c>true</c> if [is fully valid] [the specified actor]; otherwise, <c>false</c>.</returns>
        public static bool IsFullyValid(this Actor actor)
        {
            if (actor is ACDItem)
                return actor.IsValid && (int)((ACDItem)actor).GameBalanceType != -1; 
            return actor != null && actor.IsValid;
        }
    }
}
