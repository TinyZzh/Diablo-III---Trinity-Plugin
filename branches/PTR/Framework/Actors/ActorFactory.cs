using System;
using Trinity.Framework.Actors.ActorTypes;
using Trinity.Framework.Objects.Memory;
using Trinity.Framework.Objects.Memory.Misc;
using Zeta.Common;
using Zeta.Game;
using Zeta.Game.Internals.Actors;
using Zeta.Game.Internals.SNO;

namespace Trinity.Framework.Actors
{
    public static class ActorFactory
    {
        /// <summary>
        /// Object to hold the minimum requirements for creating an actor object, 
        /// to be able to switch based on ActorType without reading memory multiple times.
        /// Centralizes varying source data - ACD only, RActor only and ACD + RActor
        /// </summary>
        public class ActorSeed
        {
            public SNORecordActor ActorInfo;
            public int ActorSnoId;
            public int RActorId;
            public ActorType ActorType;
            public DiaObject RActor;
            public int AcdId;
            public string InternalName;
            public Vector3 Position;
            public ACD CommonData;
            public bool IsAcdBased;
            public bool IsRActorBased;
            public int FastAttributeGroupId;
            public int MonsterSnoId;
            public SNORecordMonster MonsterInfo;
            public int AnnId;
        }

        public static T CreateActor<T>(ACD commonData) where T : TrinityActor
        {
            return (T)CreateActor(GetActorSeed(commonData));
        }

        public static TrinityActor CreateActor(ACD commonData)
        {
            return CreateActor(GetActorSeed(commonData));
        }

        public static T CreateActor<T>(DiaObject diaObject) where T : TrinityActor
        {
            return (T)CreateActor(GetActorSeed(diaObject));
        }

        public static TrinityActor CreateActor(DiaObject diaObject)
        {
            return CreateActor(GetActorSeed(diaObject));
        }

        public static ActorSeed GetActorSeed(ACD commonData)
        {
            if (commonData == null || !commonData.IsValid)
                return null;

            return new ActorSeed
            {
                IsAcdBased = true,
                IsRActorBased = false,
                ActorSnoId = commonData.ActorSnoId,
                CommonData = commonData,
                AcdId = commonData.ACDId,
                AnnId = commonData.AnnId,
                ActorType = commonData.ActorType,
                InternalName = commonData.Name,
                Position = commonData.Position,
                FastAttributeGroupId = commonData.FastAttribGroupId,
            };
        }

        public static ActorSeed GetActorSeed(DiaObject rActor)
        {
            if (rActor == null || !rActor.IsValid)
                return null;

            var acdId = rActor.ACDId;
            var isAcdBased = acdId != -1;
            var commonData = isAcdBased ? rActor.CommonData : null;
            var actorInfo = rActor.ActorInfo;

            return new ActorSeed
            {
                RActor = rActor,
                RActorId = rActor.RActorId,
                ActorSnoId = rActor.ActorSnoId,
                ActorType = actorInfo.Type,
                AcdId = acdId,
                AnnId = isAcdBased ? rActor.CommonData.AnnId : -1,
                ActorInfo = actorInfo,
                IsAcdBased = isAcdBased,
                IsRActorBased = true,
                InternalName = rActor.Name,
                Position = rActor.Position,
                CommonData = commonData,
                FastAttributeGroupId = isAcdBased ? commonData.FastAttribGroupId : -1,
                MonsterInfo = commonData?.MonsterInfo,
                MonsterSnoId = actorInfo.MonsterSnoId
            };
        }

        public static TrinityActor CreateActor(ActorSeed seed)
        {
            if (seed == null)
                return null;

            switch (seed.ActorType)
            {
                case ActorType.Item:
                    return CreateActor<TrinityItem>(seed);
                case ActorType.Player:
                    return CreateActor<TrinityPlayer>(seed);
            }

            return CreateActor<TrinityActor>(seed);
        }

        public static T CreateActor<T>(ActorSeed actorSeed) where T : ActorBase, new()
        {
            var actor = new T
            {
                RActor = actorSeed.RActor,
                RActorId = actorSeed.RActorId,
                AcdId = actorSeed.AcdId,
                AnnId = actorSeed.AnnId,
                ActorSnoId = actorSeed.ActorSnoId,
                ActorInfo = actorSeed.ActorInfo,
                ActorType = actorSeed.ActorType,
                InternalName = actorSeed.InternalName,
                Position = actorSeed.Position,
                CommonData = actorSeed.CommonData,
                IsAcdBased = actorSeed.IsAcdBased,
                IsRActorBased = actorSeed.IsRActorBased,
                FastAttributeGroupId = actorSeed.FastAttributeGroupId,
                MonsterInfo = actorSeed.MonsterInfo,
                MonsterSnoId = actorSeed.MonsterSnoId
            };

            actor.OnCreated();
            return actor;
        }



        //public static RActor CreateRActor(IntPtr ptr)
        //{
        //    return MemoryWrapper.Create<RActor>(ptr);
        //}

        //public static ActorCommonData CreateCommonData(IntPtr ptr)
        //{
        //    return MemoryWrapper.Create<ActorCommonData>(ptr);
        //}

        //public static T CreateFromRActorPtr<T>(IntPtr ptr) where T : ActorBase, new()
        //{
        //    return CreateFromRActor<T>(CreateRActor(ptr));
        //}

        //public static T CreateFromRActor<T>(RActor rActor) where T : ActorBase, new()
        //{
        //    return CreateActor<T>(GetActorSeed(rActor));
        //}

        //public static T CreateFromAcdPtr<T>(IntPtr ptr) where T : ActorBase, new()
        //{
        //    return CreateFromAcd<T>(CreateCommonData(ptr));
        //}

        //public static T CreateFromAcd<T>(ActorCommonData commonData) where T : ActorBase, new()
        //{
        //    return CreateActor<T>(GetActorSeed(commonData));
        //}

        //public static TrinityItem CreateActor(ACDItem item)
        //{
        //    return CreateFromRActorPtr<TrinityItem>(item.BaseAddress);
        //}

        //public static TrinityActor CreateActor(DiaObject diaObject)
        //{
        //    return CreateActor(GetActorSeed(diaObject));
        //    //return CreateFromRActorPtr<TrinityActor>(diaObject.BaseAddress);
        //}


    }
}
