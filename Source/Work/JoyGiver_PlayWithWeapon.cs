﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace HumanResources
{
    class JoyGiver_PlayWithWeapon : JoyGiver_InteractBuildingInteractionCell
    {
        public override bool CanBeGivenTo(Pawn pawn)
        {
            if (pawn.equipment != null && ((def == TechDefOf.Play_Shooting && pawn.equipment.Primary != null && pawn.equipment.Primary.def.IsRangedWeapon) || def == TechDefOf.Play_MartialArts))
            {
                return base.CanBeGivenTo(pawn);
            }
            else return false;
        }
    }
}