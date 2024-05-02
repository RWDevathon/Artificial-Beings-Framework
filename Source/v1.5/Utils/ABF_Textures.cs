using Verse;
using UnityEngine;

namespace ArtificialBeings
{
    [StaticConstructorOnStartup]
    public static class ABF_Textures
    {
        static ABF_Textures()
        {
        }
        public static readonly Texture2D VanillaBleedIcon = ContentFinder<Texture2D>.Get("UI/Icons/Medical/Bleeding");

        // Reprogramming Icons
        public static readonly Texture2D complexityIcon = ContentFinder<Texture2D>.Get("UI/Icons/Gizmos/ABF_RestrictionGizmo");
        public static readonly Texture2D complexityEffectIcon = ContentFinder<Texture2D>.Get("UI/Icons/Gizmos/ABF_RestrictionGizmo");

        // Settings
        public static readonly Texture2D DrawPocket = ContentFinder<Texture2D>.Get("UI/Icons/Settings/DrawPocket");

        // Medicine
        public static readonly Texture2D NoCare = ContentFinder<Texture2D>.Get("UI/Icons/Medical/NoCare");
        public static readonly Texture2D NoMed = ContentFinder<Texture2D>.Get("Things/Pawns/Hatchling_east");
    }
}