﻿using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;
using RimWorld;
using System.Linq;

namespace HumanResources
{
    public class JobDriver_LearnTech : JobDriver_Knowledge
	{
		protected bool IsResearch;

		public override void ExposeData()
		{
			Scribe_Values.Look<bool>(ref IsResearch, "IsResearch");
			base.ExposeData();
		}

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			IsResearch = job.bill == null;
			var valid = techComp.homework?.Where(x => IsResearch ? (!x.IsFinished && !x.IsKnownBy(pawn) && x.RequisitesKnownBy(pawn)) : x.IsFinished).Reverse();
			var initiated = techComp.expertise.Where(x => valid.Contains(x.Key));
			if (initiated.Any()) project = initiated.Aggregate((l, r) => l.Value > r.Value ? l : r).Key;
			else if (valid.Any()) project = valid.FirstOrDefault();
			return base.TryMakePreToilReservations(errorOnFailed);
		}

		protected override IEnumerable<Toil> MakeNewToils()	
		{
            //Log.Message("Toil start:" + pawn + " is trying to learn " + project + ", globalFailConditions count:" + globalFailConditions.Count);
            Dictionary<ResearchProjectDef, float> expertise = pawn.TryGetComp<CompKnowledge>().expertise;
			AddEndCondition(delegate
			{
				if (!desk.Spawned)
				{
					return JobCondition.Incompletable;
				}
				return JobCondition.Ongoing;
			});
			this.FailOnBurningImmobile(TargetIndex.A);
			this.FailOn(delegate ()
			{
				IBillGiver billGiver = desk as IBillGiver;
				if (billGiver != null)
				{
					if (job.bill.DeletedOrDereferenced) return true;
					if (!billGiver.CurrentlyUsableForBills()) return true;
					if (project == null)
                    {
						Log.Warning("[HumanResources] " + pawn + " tried to learn a null project.");
						TryMakePreToilReservations(true);
						return true;
                    }
					bool flag = IsResearch ? project.IsKnownBy(pawn) : !techComp.homework.Contains(project);
					if (flag) return true;
				}
				return false;
			});
			Toil gotoBillGiver = Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);
			yield return gotoBillGiver;

			Toil acquireKnowledge = new Toil();
			acquireKnowledge.initAction = delegate ()
			{
				Pawn actor = acquireKnowledge.actor;
				if (!expertise.ContainsKey(project))
				{
					expertise.Add(project, 0f);
				}
			};
			acquireKnowledge.AddEndCondition(delegate
			{
				Pawn actor = acquireKnowledge.actor;
				if (expertise.ContainsKey(project) && expertise[project] > 1f)
				{
					techComp.LearnTech(project);
					if (!IsResearch)
					{
						Notify_IterationCompleted(actor, job.bill as Bill_Production);
						techComp.homework.Remove(project);
					}
					project = null;
					return JobCondition.Succeeded;
				}
				return JobCondition.Ongoing;
			});
			acquireKnowledge.tickAction = delegate ()
			{
				Pawn actor = acquireKnowledge.actor;
				float num = actor.GetStatValue(StatDefOf.ResearchSpeed, true);
				num *= TargetThingA.GetStatValue(StatDefOf.ResearchSpeedFactor, true);
				float workAmount = IsResearch ? 1f : job.bill.recipe.workAmount;
				project.Learned(num, workAmount, actor, job.bill == null);
				actor.skills.Learn(SkillDefOf.Intellectual, 0.1f, false);
				actor.GainComfortFromCellIfPossible(true);
			};
			acquireKnowledge.AddFinishAction(delegate
			{
				Pawn actor = acquireKnowledge.actor;
				if (job.bill != null && job.RecipeDef.workSkill != null)
				{
					float xp = ticksSpentDoingRecipeWork * 0.1f * job.RecipeDef.workSkillLearnFactor;
					actor.skills.GetSkill(job.RecipeDef.workSkill).Learn(xp, false);
				}
			});
			acquireKnowledge.FailOn(() => project == null);
			//research.FailOn(() => !this.Project.CanBeResearchedAt(this.ResearchBench, false)); //need rework
			acquireKnowledge.FailOnCannotTouch(TargetIndex.A, PathEndMode.InteractionCell);
			acquireKnowledge.FailOnDespawnedOrNull(TargetIndex.A);
			acquireKnowledge.WithEffect(EffecterDefOf.Research, TargetIndex.A);
			acquireKnowledge.WithProgressBar(TargetIndex.A, delegate
			{
				if (project == null) return 0f;
				return expertise[project];
			}, false, -0.5f);
			acquireKnowledge.defaultCompleteMode = ToilCompleteMode.Delay;
			acquireKnowledge.defaultDuration = 4000;
			acquireKnowledge.activeSkill = () => SkillDefOf.Intellectual;
			yield return acquireKnowledge;
			yield return Toils_General.Wait(2, TargetIndex.None);
			yield break;
		}

	}
}
