using SatisfactorySaveEditor.Model;
using SatisfactorySaveEditor.ViewModel.Property;
using SatisfactorySaveParser;
using SatisfactorySaveParser.PropertyTypes;
using SatisfactorySaveParser.PropertyTypes.Structs;
using SatisfactorySaveParser.Structures;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Numerics;
using System.Security.Cryptography;
using System.Windows;
using System.Windows.Documents;

namespace SatisfactorySaveEditor.Cheats
{
    public class PlaceBeaconsCheat : ICheat
    {
        private List<SaveObject> dropPods;
        private List<SaveObject> visitedDropPods;
        private List<SaveObject> existingBeacons;

        public string Name => "Place Beacons";

        public bool Apply(SaveObjectModel rootItem)
        {
            dropPods = rootItem.FindChild("BP_DropPod.BP_DropPod_C", false).DescendantSelf;
            if (dropPods == null)
            {
                return false;
            }
            visitedDropPods = (from SaveObject pod in dropPods
                                  from SerializedProperty serializedProp in pod.DataFields
                                  where serializedProp.PropertyName == "mHasBeenOpened"
                                  select pod).ToList();
            if (visitedDropPods != null)
            {
                dropPods = dropPods.Except(visitedDropPods).ToList();
            }
            existingBeacons = rootItem.FindChild("BP_Beacon.BP_Beacon_C", false).DescendantSelf;
            if (existingBeacons == null) return false;
            foreach (SaveObject dropPod in dropPods)
            {
                //copy this?
                //SaveEntity doggo = new SaveEntity("/Game/FactoryGame/Character/Creature/Wildlife/SpaceRabbit/Char_SpaceRabbit.Char_SpaceRabbit_C", "Persistent_Level", $"Persistent_Level:PersistentLevel.Char_SpaceRabbit_C_{currentDoggoID}")
                //{
                //    NeedTransform = true,
                //    Rotation = ((SaveEntity)player.Model).Rotation,
                //    Position = new Vector3()
                //    {
                //        X = ((SaveEntity)player.Model).Position.X,
                //        Y = ((SaveEntity)player.Model).Position.Y + 100 + 10 * currentDoggoID, // so they don't glitch one into another like the tractors did
                //        Z = ((SaveEntity)player.Model).Position.Z + 10
                //    },
                //    Scale = new Vector3() { X = 1, Y = 1, Z = 1 },
                //    WasPlacedInLevel = false,
                //    ParentObjectName = "",
                //    ParentObjectRoot = ""
                //};

                //SaveObject newBeacon = new SaveObject("","","");
                //newBeacon.Position.X = "";
                //existingBeacons.Add(newBeacon);
                
                //if (dropPod.InstanceName.Contains("389")) Debugger.Break();
                //if (!dropbox.DataFields.Contains("AlreadyOpenedOrWhatever"))
                //{
                    
                //}
            }
            //var gameState = rootItem.FindChild("Persistent_Level:PersistentLevel.BP_GameState_C_0", false);
            //if (gameState == null)
            //{
            //    MessageBox.Show("This save does not contain a GameState.\nThis means that the loaded save is probably corrupt. Aborting.", "Cannot find GameState", MessageBoxButton.OK, MessageBoxImage.Error);
            //    return false;
            //}

            //var numAdditionalSlots = gameState.FindOrCreateField<BoolPropertyViewModel>("mCheatNoPower");
            //numAdditionalSlots.Value = !numAdditionalSlots.Value;
            //MessageBox.Show($"{(numAdditionalSlots.Value ? "Enabled" : "Disabled")} no power cheat", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

            return true;
        }
    }
}
