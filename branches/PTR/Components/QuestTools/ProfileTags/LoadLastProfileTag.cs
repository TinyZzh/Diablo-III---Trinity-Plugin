using QuestTools.Helpers;
using QuestTools.ProfileTags.Complex;
using System.IO;
using Zeta.Bot;
using Zeta.Bot.Profile;
using Zeta.TreeSharp;
using Zeta.XmlEngine;

namespace QuestTools.ProfileTags
{
    [XmlElement("LoadLastProfile")]
    public class LoadLastProfileTag : ProfileBehavior, IEnhancedProfileBehavior
    {
        private bool _isDone;
        public override bool IsDone
        {
            get { return _isDone; }
        }

        [XmlAttribute("fallbackFile")]
        public string FallbackFile { get; set; }

        protected override Composite CreateBehavior()
        {
            return new Action(ret =>
            {
                var lastProfile = ProfileHistory.LastProfile;

                var currentProfileDirectory = Path.GetDirectoryName(ProfileManager.CurrentProfile.Path);

                if (string.IsNullOrEmpty(currentProfileDirectory))
                    currentProfileDirectory = string.Empty;

                var fallbackProfilePath = string.Empty;
                if(!string.IsNullOrEmpty(FallbackFile))
                    fallbackProfilePath = Path.Combine(currentProfileDirectory, FallbackFile);                

                if (lastProfile != null && File.Exists(lastProfile.Path))
                {
                    Logger.Debug("Loading last profile: {0}", lastProfile.Name);
                    ProfileManager.Load(lastProfile.Path);
                }
                else if (File.Exists(fallbackProfilePath))
                {
                    Logger.Debug("Loading fallback profile: {0}", FallbackFile);
                    ProfileManager.Load(fallbackProfilePath);
                }
                else
                {
                    Logger.Log("Failed to load profile! file doesnt exist");
                }

                _isDone = true;
                return RunStatus.Failure;
            });
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
        }

        #endregion

    }

}



