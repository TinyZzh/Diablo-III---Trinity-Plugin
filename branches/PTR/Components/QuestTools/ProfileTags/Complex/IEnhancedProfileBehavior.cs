
namespace QuestTools.ProfileTags.Complex
{    
    /// <summary>
    /// Interface to expose Internal/Protected members in ProfileBehavior
    /// </summary>
    public interface IEnhancedProfileBehavior
    {
        /// <summary>
        /// For ProfileBehavior to run correctly Behavior property has to contain the composite 
        /// Call UpdateBehavior() in implementation.    
        /// </summary>
        void Update();

        /// <summary>
        /// Many tags use OnStart for setup and default params
        /// Call OnStart() in implementation.
        /// </summary>
        void Start();

        /// <summary>
        /// Method that should end the ProfileBehavior
        /// Set _isDone = true in implementation.
        /// Call Done() on children if INodeContainer
        /// </summary>
        void Done();
    }
}
