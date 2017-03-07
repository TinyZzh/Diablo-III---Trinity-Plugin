using System;
using System.Collections.Generic;
using System.Linq;
using Org.BouncyCastle.Asn1.Cms;
using QuestTools.Helpers;
using Zeta.Bot.Profile;
using Zeta.Bot.Profile.Composites;
using Zeta.XmlEngine;

namespace QuestTools.ProfileTags.Complex
{
    
    /// <summary>
    /// Reorders child tags, useful for split-farming profiles with multiple bots 
    /// For example bounties or keys - each bot would start a different bounty
    /// </summary>
    [XmlElement("Shuffle")]
    public class ShuffleTag : ComplexNodeTag, IEnhancedProfileBehavior
    {
        public ShuffleTag()
        {
            QuestId = QuestId <= 0 ? 1 : QuestId;
            Timeout = Timeout <= 0 ? 5 : Timeout;
        }

        [XmlAttribute("order")]
        public OrderType Order { get; set; }
        public enum OrderType
        {
            Random = 0,
            Reverse
        }

        /// <summary>
        /// This is the longest time this behavior can run for. Default is 30 seconds.
        /// </summary>
        [XmlAttribute("timeout")]
        public int Timeout { get; set; }

        private bool _shuffled;
        private bool _isDone;
        private DateTime _startTime = DateTime.MaxValue;

        public override bool IsDone
        {
            get
            {
                var done = _isDone || !IsActiveQuestStep || Body.All(p => p.IsDone);

                if (done)
                    return true;

                CheckTimeout();

                if(!_shuffled)
                    Shuffle();

                return _isDone;
            }
        }

        public void CheckTimeout()
        {
            if (DateTime.UtcNow.Subtract(_startTime).TotalSeconds <= Timeout)
                return;

            Logger.Log("timed out ({0} seconds)", Timeout);
            _isDone = true;
        }

        public override void OnStart()
        {
            _startTime = DateTime.UtcNow;
            base.OnStart();
        }

        public void Shuffle()
        {
            Logger.Log("{0} Shuffling {1} tags", Order, Body.Count);

            switch (Order)
            {
                case OrderType.Reverse:

                    Body.Reverse();
                    break;

                default:

                    ProfileUtils.RandomShuffle(Body);
                    break;
            }

            _shuffled = true;
        }
            


        public override void ResetCachedDone()
        {
            _shuffled = false;
            _isDone = false;
            _startTime = DateTime.MaxValue;
            base.ResetCachedDone();
        }

        #region IEnhancedProfileBehavior

        public void Update()
        {
            UpdateBehavior();
        }

        public void Start()
        {
            OnStart();
        }

        public void Done()
        {
            _isDone = true;
            this.SetChildrenDone();
        }

        #endregion
    }
}

