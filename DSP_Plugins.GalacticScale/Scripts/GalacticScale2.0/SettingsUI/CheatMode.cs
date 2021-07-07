﻿using System.Linq;

namespace GalacticScale
{
    public static partial class GS2
    {
        public static void CheatModeOptionCallback(Val o)
        {
            CheatMode = o;
            if (GSSettings.Instance.imported && CheatMode) UnlockTech(true); //Unlock tech if you enable and a save has been loaded
            //Warn($"Cheatmode set to {CheatMode}");
        }
        public static void CheatModeOptionPostfix()
        {
            CheatModeOption.Set(CheatMode);
            
        }
        public static GSUI CheatModeOption;
        // All credit to Windows10CE
        public static void UnlockTech(Val o)
        {
            Log("Unlocking Tech");
            foreach (TechProto tech in LDB.techs.dataArray.Where(x => x.Published))
            {
                if (!GameMain.history.TechUnlocked(tech.ID))
                {
                    UnlockTechRecursive(tech.ID, GameMain.history);
                }
            }
            ResearchUnlocked = true;
        }

        private static void UnlockTechRecursive(int techId, GameHistoryData history)
        {
            //GS2.Warn($"UnlockTechRecursive {techId} {history != null}");
            TechState state = history.TechState(techId);
            TechProto proto = LDB.techs.Select(techId);
            try
            {
                foreach (var techReq in proto.PreTechs)
                {
                    if (!history.TechState(techReq).unlocked)
                    {
                        UnlockTechRecursive(techReq, history);
                    }
                }
                foreach (var techReq in proto.PreTechsImplicit)
                {
                    if (!history.TechState(techReq).unlocked)
                    {
                        UnlockTechRecursive(techReq, history);
                    }
                }
                foreach (var itemReq in proto.itemArray)
                {
                    if (itemReq.preTech != null && !history.TechState(itemReq.preTech.ID).unlocked)
                    {
                        UnlockTechRecursive(itemReq.preTech.ID, history);
                    }
                }

                int current = state.curLevel;
                for (; current < state.maxLevel; current++)
                {
                    for (int j = 0; j < proto.UnlockFunctions.Length; j++)
                    {
                        history.UnlockTechFunction(proto.UnlockFunctions[j], proto.UnlockValues[j], current);
                    }
                }

                history.UnlockTech(techId);
            } catch (System.Exception e)
            {
                Log("Techunlock exception caught: " + e.Message);
            }
        }
    }
}