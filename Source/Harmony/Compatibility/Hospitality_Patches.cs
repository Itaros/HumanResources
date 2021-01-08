﻿using HarmonyLib;
using RimWorld;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using Verse;

namespace HumanResources
{
    public static class Hospitality_Patches
    {
        private static NotImplementedException stubMsg = new NotImplementedException("Hospitality reverse patch");

        public static bool active = false;

        public static void Execute(Harmony instance)
        {
            instance.CreateReversePatcher(AccessTools.Method("Hospitality.GuestUtility:IsGuest", new[] { typeof(Pawn), typeof(bool) }),
                new HarmonyMethod(AccessTools.Method(typeof(Hospitality_Patches), nameof(IsGuestExternal)))).Patch();
            active = true;
        }

        public static bool IsGuestExternal(Pawn pawn) { throw stubMsg; }

    }
}


