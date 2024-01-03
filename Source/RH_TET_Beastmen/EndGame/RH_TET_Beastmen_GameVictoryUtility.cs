using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace TheEndTimes_Beastmen
{
    public static class RH_TET_Beastmen_GameVictoryUtility
    {
        private static void ShowCredits(string victoryText)
        {
            Screen_Credits screen_Credits = new Screen_Credits(victoryText);
            screen_Credits.wonGame = true;
            Find.WindowStack.Add(screen_Credits);
            Find.MusicManagerPlay.ForceSilenceFor(999f);
            ScreenFader.StartFade(Color.clear, 3f);
        }

        private static void InitiateVictory()
        {
            BeastmenDefOf.RH_TET_Beastmen_Victory.PlayOneShotOnCamera(null);
            ScreenFader.StartFade(Color.red, 5.0f);
        }

        public static void FinalDestructionVictory()
        {
            InitiateVictory();
            string victoryText = "RH_TET_FinalDestructionVictory".Translate();
            ShowCredits(victoryText);
        }
    }
}
