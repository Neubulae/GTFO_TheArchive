﻿using TheArchive.Core.Attributes;
using TheArchive.Core.FeaturesAPI;
using static TheArchive.Utilities.Utils;

namespace TheArchive.Features.Dev
{
    [EnableFeatureByDefault, HideInModSettings]
    [RundownConstraint(RundownFlags.RundownOne, RundownFlags.RundownThree)]
    public class DramaManagerNoDebugLog : Feature
    {
        public override string Name => nameof(DramaManagerNoDebugLog);
        public override string Group => FeatureGroups.Dev;

#if MONO
        [ArchivePatch(typeof(DramaManager), nameof(DramaManager.FullDebugLog))]
        internal static class DramaManager_FullDebugLogPatch
        {
            public static bool Prefix() => ArchivePatch.SKIP_OG;
        }
#endif
    }
}
