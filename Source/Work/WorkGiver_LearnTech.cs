﻿using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace HumanResources
{
    class WorkGiver_LearnTech : WorkGiver_Knowledge
    {
		public override bool ShouldSkip(Pawn pawn, bool forced = false)
        {
			if (!base.ShouldSkip(pawn, forced))
			{
				IEnumerable<ResearchProjectDef> available = DefDatabase<ResearchProjectDef>.AllDefsListForReading.Where(x => x.IsFinished);
				return !available.Any();
			}
			return true;
		}

		public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			//Log.Message(pawn + " is looking for a study job...");
			Building_WorkTable Desk = t as Building_WorkTable;
			if (Desk != null)
			{
				var relevantBills = RelevantBills(Desk, pawn);
				if (!CheckJobOnThing(pawn, t, forced) | relevantBills.EnumerableNullOrEmpty())
				{
					//Log.Message("...no job on desk. CheckJobOnThing is "+ CheckJobOnThing(pawn, t, forced)+", relevantbills is "+ relevantBills.EnumerableNullOrEmpty());
					return false;
				}
				return pawn.TryGetComp<CompKnowledge>().homework.Where(x => x.IsFinished && x.RequisitesKnownBy(pawn)).Any();
			}
            //Log.Message("case 4");
            return false;

		}

		public override Job JobOnThing(Pawn pawn, Thing thing, bool forced = false)
		{
			IBillGiver billGiver = thing as IBillGiver;
			if (billGiver != null && ThingIsUsableBillGiver(thing) && billGiver.BillStack.AnyShouldDoNow && billGiver.UsableForBillsAfterFueling())
			{
				LocalTargetInfo target = thing;
				if (pawn.CanReserve(target, 1, -1, null, forced) && !thing.IsBurning() && !thing.IsForbidden(pawn))
				{
					billGiver.BillStack.RemoveIncompletableBills();
					foreach (Bill bill in RelevantBills(thing, pawn))
					{
						if (bill.ShouldDoNow() && bill.PawnAllowedToStartAnew(pawn)/*&& bill.SelectedTech().Intersect(availableTechs).Any()*/)
						{
							return new Job(TechJobDefOf.LearnTech, target)
							{
								bill = bill
							};
						}
					}
				}
			}
			return null;
		}
	}
}
