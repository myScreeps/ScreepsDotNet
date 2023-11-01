using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
//using System.Security.AccessControl;
using System.Text;
using System.Threading;
using System.Xml;
using System.Xml.Linq;
using Microsoft.VisualBasic;
using ScreepsDotNet.API;
using ScreepsDotNet.API.World;
using ScreepsDotNet.World.profiler;
using static ScreepsDotNet.World.RoomManager;
using Color = ScreepsDotNet.API.Color;
using System.Text.Json;
using System.Diagnostics.CodeAnalysis;


namespace ScreepsDotNet.World
{
    internal partial class RoomManager
    {
        [RequiresUnreferencedCode("Calls System.Text.Json.JsonSerializer.Serialize<TValue>(TValue, JsonSerializerOptions)")]
        public void Tick()
        {
            stopwatch.Reset();
            stopwatch.Start();
            Console.WriteLine(" ");
            Console.WriteLine("******************** !!Start of Tick Test World!! ********************");

            currentGameTick = game.Time;
            myDebug.debug = this.debug;
            // myDebug.debug = true;
            

            testSpawn();


            // Exit here so everything else is disabled.
            if (Spawn1.Memory.TryGetBool("disableEverything", out var disabled))
            {
                if (disabled)
                {
                    stopwatch.Stop();
                    tickTotalExecutionTime = stopwatch.ElapsedMilliseconds;
                    overlay();
                    return;
                }
            }

            var example1 = new CheckSumExample(1, 1);

            //   Object example1 = 1;

            //  Console.WriteLine($"start1");
            //Console.WriteLine($"start21");


            if (spawningManager == null)
            {
                Console.WriteLine($"spawningManager is null: {spawningManager}");

            }
            else
            {
                Console.WriteLine($"spawningManager.spawn.Name: {spawningManager.spawn.Name}");
            }

            //if (spawningManager.HasChanged && spawningManager != null)
            //{
            //    Console.WriteLine($"Spawn Memory has been updated");
            //}

            // var data = new { Name = "John", Age = 30 };
            //string jsonString = JsonSerializer.Serialize(data);
            // Console.WriteLine($"jsonString: {jsonString}");
            //var cksum = ChecksumGenerator.ObjectToByteArray(data);
            //Console.WriteLine($"cksum: {cksum}");
            Console.WriteLine($"end");


            //Spawn1.Memory.TryGetInt("targetMiner1Count",out targetMiner1Count);
            //object test1 = 1;
            //string myNewChecksumtest1 = ChecksumUtility.GenerateChecksum(test1);
            //Console.WriteLine($"myNewChecksumtest1: {myNewChecksumtest1}");
            //test1 = 2;
            //myNewChecksumtest1 = ChecksumUtility.GenerateChecksum(test1);
            //Console.WriteLine($"myNewChecksumtest2: {myNewChecksumtest1}");


            //Console.WriteLine($"targetMiner1Count: {targetMiner1Count}");
            //Console.WriteLine($"creepsCrc: {creepsCrc}");
            // IMemoryObject spw1;


            // var getstatus = this.room.Memory.TryGetObject("Spawn1", out spw1);



            //spw1 = game.Spawns.Values.ToArray()[0];
            //var spw1mem = spw1.Memory;

            //var stat = spw1.Memory.TryGetInt("targetMiner1Count", out var sout);

            //Console.WriteLine($"sout: {sout}");
            //var crcHasChanged = ChecksumGenerator.HasChecksumChanged(spw1mem, this.creepsCrc);
            //Console.WriteLine($"creepsCrc: {creepsCrc}");

            //////////// Console.WriteLine($"crcHasChanged: {crcHasChanged}");

            //if (crcHasChanged)
            //{
            //    this.creepsCrc = ChecksumGenerator.GenerateChecksum(this.spw1);
            //    Console.WriteLine($" ---------------------- creepsCrc: {creepsCrc}");
            //}


            //var example1 = new ChecksumGenerator.CheckSumExample(1, 1);

            //var example1CS = ChecksumGenerator.CalculateMD5Hash(example1);

            //Console.WriteLine($"example1CS: {example1CS}");


            //example1.x = 2;
            //example1CS = ChecksumGenerator.CalculateMD5Hash(example1);
            //Console.WriteLine($"example1CS: {example1CS}");




            //   var checkSumExample1 = ChecksumGenerator.GenerateChecksum(example1);
            // Console.WriteLine($"example1.x : {example1.x}");

            //  Console.WriteLine($"checkSumExample1: {checkSumExample1}");


            //var checkSumExample2 = ChecksumGenerator.GenerateChecksum(example1);
            //Console.WriteLine($"checkSumExample1: {checkSumExample2}");

            //var crcHasChanged = ChecksumGenerator.HasChecksumChanged(example1, checkSumExample1);
            //Console.WriteLine($"crcHasChanged: {crcHasChanged}");

            //Console.WriteLine($"example1.x : {example1.x}");
            //if (crcHasChanged)
            //{
            //    var crc = ChecksumGenerator.GenerateChecksum(this.spw1);
            //    Console.WriteLine($" ---------------------- crc: {crc}");
            //}



            //if (game.Time % 100 == 0)
            //{
            //    CleanMemory();
            //}

            //  var isInDanger = GridUtility.IsPointInSubsection(new Point(9, 24), new Point(45, 45), new Point(40, 42));

            //  Console.WriteLine($"isInDanger: {isInDanger}");
            //InitLinks();



            myDebug.WriteLine($"maxTotalCreeps: {maxTotalCreeps}");
            myDebug.WriteLine($"Source1: {this.Source1}");
            myDebug.WriteLine($"Source2: {this.Source2}");
            myDebug.WriteLine($"source2Link: {this.source2Link}");
            myDebug.WriteLine($"controllerLink: {this.controllerLink}");
            //GetSource1Container();
            myDebug.WriteLine($" xxx containersInRoom.Count: {this.containersInRoom.Count}");
            myDebug.WriteLine($" xxx allRoomFlags.Count: {allRoomFlags.Count}");
            if (Source1Container == null)
            {
                myDebug.WriteLine($"Source1Container is null: {Source1Container}");

            }
            if (Source2Container == null)
            {
                myDebug.WriteLine($"Source1Container is null: {Source2Container}");

            }
            myDebug.WriteLine($"Source1Container: {Source1Container}");
            myDebug.WriteLine($"Source2Container: {Source2Container}");

            //var spawn1Mem = Spawn1.Memory;
            //var spawnKeys = spawn1Mem.Keys;

            //foreach (var key in spawnKeys)
            //{
            //    status = spawn1Mem.TryGetString(key.ToString(), out var _value);
            //    Console.WriteLine($"key: {key.ToString()} value: {_value}");

            //}




            //foreach (var key in spawnKeys)
            //{

            //    Console.WriteLine($"key: {key.ToString()} value: {key.}");
            //}
            //  string myValue = GetSpawnMemory("bob", "a");

            // Console.WriteLine($"myValue: {myValue}");
            //Console.WriteLine($"currentGameTick.GetType(): {currentGameTick.GetType()}");
            //if (currentGameTick is long) 
            //{
            //    Console.WriteLine($"currentGameTick.GetType().BaseType: {currentGameTick.GetType()}");
            //    Console.WriteLine($"currentGameTick.GetType().BaseType: {currentGameTick.GetType().BaseType}");
            //}

            this.UpdateSpawn1Memory();
            myDebug.WriteLine($"this.UpdateSpawn1Memory() completed");

            spawningManager.GetSpawnMemory();
            myDebug.WriteLine($"spawningManager.GetSpawnMemory() completed");
            myDebug.WriteLine($"spawningManager.spawn: {spawningManager.spawn.Name}");
            spawningManager.ToConsole();

            //profiler.Profile("updateSpawn1Memory", () => this.updateSpawn1Memory(), true);




            // Console.WriteLine("reading memory: Start");
            // roomMemory = game.Memory.GetOrCreateObject(this.room.Name);
            // Console.WriteLine("reading memory: end");
            // var test1 = roomMemory.TryGetInt("respawnTick", out var respawnTickTest);
            // Console.WriteLine($"respawnTickTest: {respawnTickTest}");
            // roomMemory.SetValue("respawnTick", 12);
            //var getMemStatus = Spawn1.Memory.TryGetInt("miner2RespawnTick", out var respawnTickTest);
            //Console.WriteLine($"respawnTickTest: {respawnTickTest}");
            //var getMemStatus = Spawn1.Memory.TryGetInt("cat", out var cat);

            //spawn1Mem.SetValue("cat", "huckleberry");
            // spawn1Mem.TryGetString("cat", out var cat);

            //Console.WriteLine($"cat: {cat}");

            //Console.WriteLine($"getMemStatus: {getMemStatus}");
            ////if (!getMemStatus)
            ////{
            ////    Spawn1.Memory.GetOrCreateObject("cat");
            ////    Spawn1.Memory.SetValue("cat", "huckleberry" );
            ////}







            //  var spawn1Mem = Spawn1.Memory;





            // Spawn1.Memory.SetValue("foo2", 12);
            //int spawnMemoryFoo2 = 0;
            //var status = Spawn1.Memory.TryGetInt("foo2", out maxTotalCreeps); 
            //Console.WriteLine($"Spawn1.Memory.foo2: {spawnMemoryFoo2}");


            // gameMemory = game.Memory;

            //IMemoryObject roomMemory = gameMemory.GetOrCreateObject(this.room.Name);
            //IMemoryObject myFooTest = gameMemory.GetOrCreateObject("MyFooTest");
            //string fooTest1 = string.Empty;
            //roomMemory.TryGetString("fooTest1", out fooTest1);
            //Console.WriteLine($"fooTest1: {fooTest1}");
            //// roomMemory.SetValue("fooTest1", "test1"); 
            ////  myFooTest.SetValue("fooTest2", "test2");

            if (game.Time % 10 == 0)
            {
                // allRoomFlags = room.Find<IFlag>().ToHashSet();
                // allRoomStructures = room.Find<IStructure>().ToHashSet();
                // towersInRoom = allRoomStructures.OfType<IStructureTower>().ToHashSet();
                // allContainers = allRoomStructures.OfType<IStructureContainer>().ToHashSet();
                // containersInRoom = allRoomStructures.OfType<IStructureContainer>().ToHashSet();
                // allContructionSitesInRoom = allRoomStructures.OfType<IConstructionSite>().ToHashSet();
                // roadsInRoom = allRoomStructures.OfType<IStructureRoad>().ToHashSet();

                SyncScreepsGameObjects();
                //profiler.Profile("SyncScreepsGameObjects", () => SyncScreepsGameObjects(), true);

                myDebug.WriteLine($"allRoomStructures.Count: {allRoomStructures.Count()}");
            }

            // InitLinks();
            // Console.WriteLine($"500 allCreepsInRoom.Count: {allCreepsInRoom.Count}");

            TickLinks();
            //profiler.Profile("TickLinks", () => TickLinks(), true);

            CreepHandler();
            // profiler.Profile("CreepHandler()", () => CreepHandler(), true);



            //var allCreeps = room.Find<ICreep>().ToHashSet();

            //// Check for any creeps we're tracking that no longer exist


            //foreach (ICreep? creep in this.allCreepsInRoom.ToArray())
            //{

            //    var respawnTick = this.respawnTick;

            //    if (creep.Exists)
            //    {
            //        if (creep.My == false)
            //        {
            //            return;
            //        }

            //        if (creep.Exists)
            //        {

            //            var getMemStatus = creep.Memory.TryGetInt("respawnTick", out var creepRespawnTick);
            //            if (getMemStatus)
            //            {
            //                respawnTick = creepRespawnTick;
            //            }

            //            if (creep.TicksToLive == respawnTick)
            //            {
            //                var creepData = new myCreepData(creep);
            //                myDebug.WriteLine($"creepData.spawnName {creepData.spawnName}");
            //                myDebug.WriteLine($"creepData.role {creepData.role}");
            //                spawnQueue.Enqueue(creepData);
            //                //   }

            //            }
            //        }
            //        continue;
            //    }

            //    var removeStatus = allCreepsInRoom.Remove(creep);
            //    //Console.WriteLine("Creep name: " + creep.Name);

            //    OnCreepDied(creep);


            //}

            //invaderCreeps = allCreeps.Where(x => !x.My).ToHashSet();
            //var newCreepList = allCreeps.Where(x => x.My); //.OrderByDescending(x => x.TicksToLive).ToList();


            //foreach (var creep in newCreepList)
            //{
            //    TTLCountDown(creep, 100);
            //    if (!allCreepsInRoom.Add(creep)) { continue; }
            //    OnAssignRole(creep);
            //}

            //stopwatch.Stop();
            //long executeTimeUpToCreeps = stopwatch.ElapsedMilliseconds;

            //// Tick all tracked creeps
            //// var minerCount = 0;
            //stopwatch.Reset();
            //stopwatch.Start();
            //status = Spawn1.Memory.TryGetBool("pauseAllCreeps", out var pauseAllCreeps);
            //// Console.WriteLine($"status: {status}");
            //if (status == true)
            //{
            //    if (pauseAllCreeps == false)
            //    {
            //        profiler.Profile("RunCreeps", () => RunCreeps(), true);
            //    }
            //}


            //stopwatch.Stop();
            //long creepsExecuteTime = stopwatch.ElapsedMilliseconds;
            //stopwatch.Reset();
            //stopwatch.Start();

            //   containersInRoom = allRoomStructures.OfType<IStructureContainer>().ToHashSet();

            // Console.WriteLine($"           containers.Count: {containers.Count}");

            // allContructionSitesInRoom = allRoomStructures.OfType<IConstructionSite>().ToHashSet();
            // Console.WriteLine($"++++ allContructionSites.Count: {allContructionSites.Count}");
            //   roadsInRoom = allRoomStructures.OfType<IStructureRoad>().ToHashSet();

            // Console.WriteLine($"***1 roadsInRoom.Count: {roadsInRoom.Count}");
            // Console.WriteLine($"***1 roadsThatNeedRepair.Count: {roadsThatNeedRepair.Count}");


            // towersInRoom = allRoomStructures.OfType<IStructureTower>().ToHashSet();
            //  Console.WriteLine($"           xtowersInRoom.Count: {towersInRoom.Count}");

            TickGetStructureUpdates(1);
            //profiler.Profile("TickGetStructureUpdates", () => TickGetStructureUpdates(10));

            DisplayStats();

            TickAllSpawns();
            //Profiler.Profile("TickAllSpawns", () => TickAllSpawns());

            TickTowers();
            //profiler.Profile("TickTowers", () => TickTowers(), true);



            Console.WriteLine(" ");
            Console.WriteLine("******************** !!!- The End of tick -!!! ********************");


            // profiler.Profile("RoomOverlay", () => RoomOverlay(executeTimeUpToCreeps, creepsExecuteTime, toTheEndExecutionTime), true);

            //Console.WriteLine(" ");
            //profiler.DisplayProfileResults();
            //profiler.Reset();
            //Console.WriteLine(" ");

            stopwatch.Stop();
            tickTotalExecutionTime = stopwatch.ElapsedMilliseconds;
            RoomOverlay(true);
            //test

        }

        private void TickSource1Miner(ICreep creep, ISource source)
        {
            if (!creep.Exists)
            {
                return;
            }
            //    myDebug.WriteLine($"---- source: {source}");

            // myDebug.WriteLine($"---- IN PickupEnergy");
            PickupEnergy(creep);
            // Check energy storage
            //   myDebug.WriteLine($"---- Out PickupEnergy");


            // if miner has any any free storage space, get more energy until full
            if (creep.Store.GetFreeCapacity(ResourceType.Energy) > 0)
            {
                myDebug.WriteLine($"---- IN if (creep.Store.GetFreeCapacity(ResourceType.Energy) > 0)");

                //// There is space for more energy @@@@
                //if (!sources.Any()) { return; }


                //if (sources.Count == 0)
                //{
                //    return;
                //}

                ////  var source = sources.Best(x => -x.LocalPosition.LinearDistanceTo(creep.LocalPosition));
                //var source1Flag = allRoomFlags.Where(x => x.Name == "Source1_" + room.Name).First();
                //myDebug.WriteLine($"source1Flag {source1Flag.Name}");



                //var source = GetSourceAtFlag(source1Flag);
                //if (source == null) { return; }
                //ISource source = Source1;
                //if (source.Energy > 0)
                //{

                //}
                if (Source1Container != null)
                {
                    MoveToContainer(creep, this.Source1Container);

                }


                myDebug.WriteLine($"---- IN harvestResults = HarvestEnergy(creep, source)");

                var harvestResults = HarvestEnergy(creep, source);
                myDebug.WriteLine($"---- OUT harvestResults = HarvestEnergy(creep, source)");

                //Console.WriteLine($" ----- harvestResults: {harvestResults}");

                // exit here of Source is not in range
                if (harvestResults == CreepHarvestResult.NotInRange)
                {
                    return;
                }

                ////////////////////////////////
                // If not able to harvest, then withdraw energy from container
                if (harvestResults != CreepHarvestResult.Ok)
                {

                    myDebug.WriteLine($"IN --------- harvestResults != CreepHarvestResult.Ok, harvestResults: {harvestResults}");

                    myDebug.WriteLine("IN --------- EnergyWithDrawHandler(creep,this.Source1Container)");

                    EnergyWithDrawHandler(creep, this.Source1Container);
                    myDebug.WriteLine("OUT --------- EnergyWithDrawHandler(creep,this.Source1Container)");

                }

                //var harvestResult = creep.Harvest(source);
                //if (harvestResult == CreepHarvestResult.NotInRange)
                //{
                //    creep.MoveTo(source.RoomPosition);
                //}
                //else if (harvestResult != CreepHarvestResult.Ok)
                //{

                //    return;
                //    //if (creep)
                //    //{

                //    //}

                //    //  Console.WriteLine($"{this}: {creep} unexpected result when harvesting {source} ({harvestResult})");
                //}
            }
            else
            {
                // We're full, go to drop off

                //var termFree = room.Terminal.Store.GetFreeCapacity();

                myDebug.WriteLine($"if (creep.Store.GetFreeCapacity(ResourceType.Energy) > 0");
                if (!spawns.Any()) { return; }
                var spawn = spawns.Best(x => -x.LocalPosition.LinearDistanceTo(creep.LocalPosition));
                if (spawn == null) { return; }

                CreepTransferResult transferResult;

                myDebug.WriteLine($"if (source1Link != null)");

                if (source1Link != null)
                {
                    myDebug.WriteLine("--------- Transfering to Sourcelink1");
                    //if (creep.RoomPosition.Position.IsNextTo(this.source1Link.RoomPosition.Position))
                    //{
                    if (source1Link.Store.GetFreeCapacity(ResourceType.Energy) > 0)
                    {
                        myDebug.WriteLine("--------- IN TransferEnergy(creep, source1Link)");

                        TransferEnergy(creep, source1Link);
                        myDebug.WriteLine("--------- OUT TransferEnergy(creep, source1Link)");

                        //transferResult = creep.Transfer(source1Link, ResourceType.Energy);
                        //if (transferResult == CreepTransferResult.Ok)
                        //{
                        //    return;
                        //}

                        return;
                    }
                    // test
                    //}
                }
                //Console.WriteLine("--------- skipping transfering to Sourcelink1");




                //     var spawnFreeStorageSpace = spawn.Store.GetFreeCapacity(ResourceType.Energy);
                //  var creepUsedCapacity = creep.Store.GetUsedCapacity(ResourceType.Energy);
                myDebug.WriteLine($"IN if (spawn.Store.GetFreeCapacity(ResourceType.Energy) >= 50)");
                if (spawn.Store.GetFreeCapacity(ResourceType.Energy) >= 50)
                {
                    //               var spawnFreeEnergy = spawn.Store.GetFreeCapacity(ResourceType.Energy);
                    //               var creepUsedCapacity = creep.Store.GetUsedCapacity(ResourceType.Energy);
                    // Console.WriteLine($"{this}: creepUsedCapacity: {creepUsedCapacity})");
                    // Console.WriteLine($"{this}: spawnFreeEnergy: {spawnFreeStorageSpace}");
                    //test
                    myDebug.WriteLine("--------- transfering to Spawn1");
                    myDebug.WriteLine("--------- IN TransferEnergy(creep, spawn);");
                    TransferEnergy(creep, spawn);
                    myDebug.WriteLine("--------- OUT TransferEnergy(creep, spawn);");

                    //var result = TransferEnergy(creep, (ICreep)spawn);
                    //transferResult = creep.Transfer(spawn, ResourceType.Energy);
                    //if (transferResult == CreepTransferResult.NotInRange)
                    //{
                    //    creep.MoveTo(spawn.RoomPosition);
                    //    return;
                    //}
                    return;

                }
                myDebug.WriteLine($"OUT if (spawn.Store.GetFreeCapacity(ResourceType.Energy) >= 50)");

                //else
                // {
                // Console.WriteLine("--------- skipping transfering to Spawn1");

                // Only transfer energy to container when Source has energy
                if (this.Source1Container != null && this.Source1Container.Store.GetFreeCapacity() > 0 && source.Energy > 0)
                {
                    myDebug.WriteLine("--------- IN TransferEnergy(creep, this.Source1Container)");
                    TransferEnergy(creep, this.Source1Container);
                    myDebug.WriteLine("--------- OUT TransferEnergy(creep, this.Source1Container)");

                    return;
                }

                if (room.Storage != null && room.Storage.Store.GetFreeCapacity() > 0)
                {

                    myDebug.WriteLine("--------- IN status = TransferEnergy(creep, room.Storage)");
                    var status = TransferEnergy(creep, room.Storage);
                    myDebug.WriteLine("--------- OUT status = TransferEnergy(creep, room.Storage)");

                    // Console.WriteLine($"{this}: XXX creepUsedCapacity: {creepUsedCapacity})");
                    // Console.WriteLine($"{this}: XXX spawnFreeEnergy: {spawnFreeStorageSpace}");
                    // 
                    //transferResult = creep.Transfer(room.Storage, ResourceType.Energy);
                    //if (transferResult == CreepTransferResult.NotInRange)
                    //{
                    //    creep.MoveTo(room.Storage.RoomPosition);
                    //    return;


                    return;


                    //}
                    //  }
                    Console.WriteLine("--------- skipping transfering to Storage");
                }

                //   Console.WriteLine($"room.Terminal: {room.Terminal}");
                //     Console.WriteLine($"room.Terminal.Store.GetFreeCapacity(): {room.Terminal.Store.GetFreeCapacity()}");
                myDebug.WriteLine("--------- IN if (room.Terminal != null && room.Terminal.Store.GetFreeCapacity() > 0)");

                if (room.Terminal != null && room.Terminal.Store.GetFreeCapacity() > 0)
                {
                    myDebug.WriteLine($"entering room.Terminal transfer");

                    //transferResult = creep.Transfer(room.Terminal, ResourceType.Energy);
                    //if (transferResult == CreepTransferResult.NotInRange)
                    //{
                    //    creep.MoveTo(room.Terminal.RoomPosition);
                    //    // return transferResult;
                    //}
                    myDebug.WriteLine("IN --------- transfering to Terminal");

                    var status = TransferEnergy(creep, room.Terminal);

                    myDebug.WriteLine("OUT --------- transfering to Terminal");

                    // status = TransferEnergy(creep, this.source1Flag);

                    //if (status = 0)
                    //{
                    //    return;
                    //}
                }

                //}
            }
        }
      
        private void TickSource2Miner(ICreep creep, ISource source)
        {
            myDebug.debug = true;
            Console.WriteLine("Enter TickSource2Miner");
            if (!creep.Exists)
            {
                return;
            }

            creep.Say(creep.TicksToLive.ToString());


            CreepTransferResult transferResult;
            PickupEnergy(creep);
            if (source2Link != null)
            {
                myDebug.WriteLine($"xx  source2Link: {source2Link}");

            }


            // Check energy storage
            myDebug.WriteLine($" if (creep.Store.GetFreeCapacity(ResourceType.Energy) > 0)");

            if (creep.Store.GetFreeCapacity(ResourceType.Energy) > 0)
            {

                // var source = this.Source2;
                // var container = Source2Container;


                if (Source2Container != null)
                {
                    MoveToContainer(creep, this.Source2Container);
                }




                var harvestResults = HarvestEnergy(creep, source);

                myDebug.WriteLine($" ----- harvestResults: {harvestResults}");
                if (harvestResults == CreepHarvestResult.NotInRange)
                {
                    return;
                }

                if (harvestResults != CreepHarvestResult.Ok)
                {
                  //  myDebug.WriteLine("line 2099");


                    if (this.Source2Container != null)
                    {
                        EnergyWithDrawHandler(creep, this.Source2Container);

                    }


                    myDebug.WriteLine("line 2120 EnergyWithDrawHandler");

                    //if (container != null)
                    //{
                    //    // harverst if source has energy
                    //    if (container.Store.GetUsedCapacity() > 0)
                    //    {
                    //        // var results = WithdrawEnergy(creep, this.Source1Container);
                    //        if (container.Store.GetUsedCapacity() > 0)
                    //        {
                    //            if (WithdrawEnergy(creep, container) == CreepWithdrawResult.NotInRange)
                    //            { return; }
                    //        }
                    //    }

                    //    // find any container that creep is next to
                    //    var otherContainers = containersInRoom.Where(x => x.RoomPosition.Position.IsNextTo(creep.RoomPosition.Position) && x.Store.GetUsedCapacity() > 0);
                    //    if (otherContainers != null && otherContainers.Count() > 0)
                    //    {
                    //        var IsNextToContainer = otherContainers.First();
                    //        var results = WithdrawEnergy(creep, IsNextToContainer);

                    //    }

                    //}
                }


                myDebug.debug = false;
            }
            else
            {
                // *** Full, do something with energy ***

                myDebug.WriteLine($" *** Full, do something with energy ***");
                /// if there a construction site next to miner2, then build it
                var sites = constructionSites.Where(x => x.RoomPosition.Position.IsNextTo(creep.RoomPosition.Position));
                //if (sites != null && sites.Count() > 0)
                //{
                //    Console.WriteLine($" @@@@@@@@ constructionSites.Count: {constructionSites.Count}");
                //    Console.WriteLine($" @@@@@@@@ sites.Count(): {sites.Count()}");


                //    var site = sites.First();
                //    if (site != null)
                //    {
                //        Console.WriteLine($" @@@@@@@@ site: {site}");

                //        var buildStatus = creep.Build(site);
                //        //  Console.WriteLine($"Miner2 buildStatus: {buildStatus}");
                //        // Console.WriteLine($"Miner2 buildStatus: {buildStatus}");
                //        //   Console.WriteLine($"Miner2 buildStatus: {buildStatus}");

                //        return;
                //    }

                //}


            //    if (!spawns.Any()) { return; }
                var spawn = spawns.Best(x => -x.LocalPosition.LinearDistanceTo(creep.LocalPosition));
                //var source2Flag = room.Find<IFlag>().Where(x => x.Name == "")
                myDebug.WriteLine(" *** if (!spawns.Any()) { return; }");

              //  if (spawn == null) { return; }

          


                Console.WriteLine($"this.Spawn2 is: {this.Spawn2}");
                // if miner is next to spawn true, transfer 
                if (this.Spawn2 != null)
                {
                    if (creep.RoomPosition.Position.IsNextTo(this.Spawn2.RoomPosition.Position))
                    {
                        if (this.Spawn2.Store.GetFreeCapacity(ResourceType.Energy) > 0)
                        {
                            transferResult = creep.Transfer(this.Spawn2, ResourceType.Energy);
                            if (transferResult == CreepTransferResult.Ok)
                            {
                                return;
                            }
                        }
                    }
                }

                if (source2Link != null && source2Link.Store.GetFreeCapacity(ResourceType.Energy) > 0)
                {
                    if (creep.RoomPosition.Position.IsNextTo(this.source2Link.RoomPosition.Position))
                    {
                        if (source2Link.Store.GetFreeCapacity(ResourceType.Energy) > 0)
                        {
                            transferResult = creep.Transfer(source2Link, ResourceType.Energy);
                            if (transferResult == CreepTransferResult.Ok)
                            {
                                return;
                            }
                        }
                    }
                    else
                    {
                        creep.MoveTo(source2Link.RoomPosition.Position);
                        return;
                    }
                }

                // Only transfer energy to container when Source has energy
                if (this.Source2Container != null && this.Source2Container.Store.GetFreeCapacity() > 0 && source.Energy > 0)
                {
                    // myDebug.WriteLine("--------- IN TransferEnergy(creep, this.Source1Container)");
                    TransferEnergy(creep, this.Source2Container);
                    // myDebug.WriteLine("--------- OUT TransferEnergy(creep, this.Source1Container)");

                    return;
                }

                // Hack. By level 6 we just want miners to mine and not travel anywhere to deposit energy





                if (room.Controller.Level >= 6)
                {
                    return;
                }

                //if (extensionTargets.Count > 0)
                //{

                //    var extensionTarget = extensionTargets.Best(x => -x.LocalPosition.LinearDistanceTo(creep.LocalPosition));

                //    transferResult = creep.Transfer(extensionTarget, ResourceType.Energy);
                //    if (transferResult == CreepTransferResult.NotInRange)
                //    {
                //        creep.MoveTo(extensionTarget.RoomPosition);
                //    }
                //    else if (transferResult != CreepTransferResult.Ok)
                //    {
                //        //Console.WriteLine($"{this}: {creep} Miner has unexpected result when depositing to {extensionTarget} ({transferResult})");
                //    }

                //    return;

                //}
                // Console.WriteLine("line 2260");

                if (room.Storage != null && source2Link == null)
                {
                    transferResult = creep.Transfer(room.Storage, ResourceType.Energy);

                    if (transferResult == CreepTransferResult.NotInRange)
                    {
                        creep.MoveTo(room.Storage.RoomPosition);
                    }
                    else if (transferResult != CreepTransferResult.Ok)
                    {
                        //  Console.WriteLine($"{this}: {creep} 603 unexpected result when depositing to {room.Storage} ({transferResult})");
                    }

                    return;
                }

                //if (room.Controller.Level <= 4)
                // {
                //var towersThatNeedsEngery = towersInRoom.Where(x => x.Store.GetFreeCapacity() > 0);
                //if (towersThatNeedsEngery.Count() > 0)
                //{
                //    var target = towersThatNeedsEngery.First();
                //    transferResult = creep.Transfer(target, ResourceType.Energy);
                //    if (transferResult == CreepTransferResult.NotInRange)
                //    {
                //        creep.MoveTo(target.RoomPosition);
                //    }
                //    if (transferResult != CreepTransferResult.Ok)
                //    {
                //        return;
                //        //  Console.WriteLine($"{this}: {creep} 616 unexpected result when depositing to {spawn} ({transferResult})");
                //    }
                //}
                //return;
                //}
                myDebug.WriteLine("2297 if (spawn.Store.GetFreeCapacity() > 0)");
                myDebug.WriteLine($"2300 spawn.Store.GetFreeCapacity(): {spawn.Store.GetFreeCapacity(ResourceType.Energy)}");
                myDebug.WriteLine($"spawn: {spawn}");

                if (spawn.Store.GetFreeCapacity(ResourceType.Energy) > 0)
                {
                    transferResult = creep.Transfer(spawn, ResourceType.Energy);

                    myDebug.WriteLine($"transferResult: {transferResult}");
                    if (transferResult == CreepTransferResult.NotInRange)
                    {
                        Console.WriteLine("creep.MoveTo(spawn.RoomPosition)");

                        creep.MoveTo(spawn.RoomPosition.Position);
                        return;
                    }

                    if (transferResult == CreepTransferResult.Ok)
                    {
                        // return;
                        //  Console.WriteLine($"{this}: {creep} 616 unexpected result when depositing to {spawn} ({transferResult})");
                    }

                    if (transferResult != CreepTransferResult.Ok)
                    {
                        //  Console.WriteLine($"{this}: {creep} 616 unexpected result when depositing to {spawn} ({transferResult})");
                    }

                }




                //if (room.Controller.Level <= 3)
                //{
                //    TickWorker(creep);
                //}





            }


        }

        private void TickTowers()
        {
            foreach (var tower in towersInRoom)
            {
                // Console.WriteLine($"------- towersInRoom.Count: {towersInRoom.Count}");
                TickTowers(tower);
            }
        }
        private void TickLinks()
        {
            //if (this.source2Link == null && this.source2Link.Exists == false)
            //{
            //    //  Console.WriteLine("Source link is null. Link transfer is disabled");
            //    return;
            //}

            ///////////////////////////////////////////////
            // Transfer from Source2 Link to controller link
            if (this.controllerLink != null && this.source2Link != null && enableSource2Link == true)
            {
                if (this.source2Link.Store.GetUsedCapacity(ResourceType.Energy) >= 200)
                {
                    if (this.controllerLink.Store.GetFreeCapacity(ResourceType.Energy) >= 200)
                    {
                        var transferStatus = this.source2Link.TransferEnergy(this.controllerLink);
                        if (transferStatus != LinkTransferEnergyResult.Ok)
                        {
                            // Console.WriteLine($"Link transfered to controller link failed with the status of {transferStatus}");
                            //  return;
                        }
                    }
                }
            }

            ///////////////////////////////////////////////
            // Transfer from Source2 Link to storage link
            if (this.source1Link != null && this.source2Link != null && enableSource2Link == true)
            {
                if (this.source2Link.Store.GetUsedCapacity(ResourceType.Energy) >= 200)
                {
                    if (this.source1Link.Store.GetFreeCapacity(ResourceType.Energy) >= 200)
                    {
                        var transferStatus = this.source2Link.TransferEnergy(this.source1Link);
                        if (transferStatus != LinkTransferEnergyResult.Ok)
                        {
                            // Console.WriteLine($"Link transfered to controller link failed with the status of {transferStatus}");
                            //  return;
                        }
                    }
                }
            }

            if (this.storageLink != null && this.source2Link != null && enableSource2Link == true)
            {
                if (this.source2Link.Store.GetUsedCapacity(ResourceType.Energy) >= 200)
                {
                    if (this.storageLink.Store.GetFreeCapacity(ResourceType.Energy) >= 200)
                    {
                        var transferStatus = this.source2Link.TransferEnergy(this.storageLink);
                        if (transferStatus != LinkTransferEnergyResult.Ok)
                        {
                            // Console.WriteLine($"Link transfered to controller link failed with the status of {transferStatus}");
                            //  return;
                        }
                    }
                }
            }

            /////////////////////////////////////////////////
            //// Transfer from Source2 Link to Source1 link
            //if (source2Link != null && source1Link !=null)
            //{
            //    if (this.source2Link.Store.GetUsedCapacity(ResourceType.Energy) >= 200)
            //    {
            //        if (source1Link != null)
            //        {
            //            if (this.source1Link.Store.GetFreeCapacity(ResourceType.Energy) >= 200)
            //            {
            //                var transferStatus = this.source2Link.TransferEnergy(this.source1Link);
            //                if (transferStatus != LinkTransferEnergyResult.Ok)
            //                {
            //                    Console.WriteLine($"Link transfered to controller link failed with the status of {transferStatus}");
            //                    return;
            //                }
            //            }
            //        }
            //    }
            //}


            //Console.WriteLine($"this.source1Link != null: {this.source1Link != null}");
            //Console.WriteLine($"this.enableSource1Link != null: {this.enableSource1Link != null}");
            //Console.WriteLine($"this.controllerLink != null: {this.controllerLink != null}");


            ///////////////////////////////////////////////
            // Transfer from Source1 Link to source1 link
            if (this.source1Link != null && this.controllerLink != null && enableSource1Link == true)
            {
                if (this.source1Link.Store.GetUsedCapacity(ResourceType.Energy) >= 200)
                {
                    if (this.controllerLink.Store.GetFreeCapacity(ResourceType.Energy) >= 200)
                    {
                        var transferStatus = this.source1Link.TransferEnergy(this.controllerLink);
                        if (transferStatus != LinkTransferEnergyResult.Ok)
                        {
                            Console.WriteLine($"Link transfered to controller link failed with the status of {transferStatus}");
                            //  return;
                        }
                    }
                }
            }

            return;
        }
        private void TickWorker(ICreep creep)
        {

            var status = Spawn1.Memory.TryGetBool("pauseWorkers", out var pause);

            myDebug.WriteLine($"status: {status}");
            myDebug.WriteLine($"pause: {pause}");

            if (status)
            {
                if (pause == true)
                {
                    return;
                }
            }


            if (!creep.Exists)
            {
                return;
            }

            PickupEnergy(creep);

            //myDebug.WriteLine($"creep.Store[ResourceType.Energy]: {creep.Store[ResourceType.Energy]}");


            if (creep.Store[ResourceType.Energy] > 0)
            {
                // There is energy to drop off

                CreepMoveResult moveStatus;

                if (creep.RoomPosition.Position.IsNextTo(room.Controller.RoomPosition.Position) == false)
                {
                    moveStatus = creep.MoveTo(roomController.RoomPosition);
                }

                if (creep.RoomPosition.Position.IsNextTo(room.Controller.RoomPosition.Position) == true)
                {
                    if (game.Time % 100 == 0)
                    {
                        creep.SignController(room.Controller, "Screeps In C#");

                    }
                }
                var upgradeResult = creep.UpgradeController(roomController);
                return;
            }

            //var upgradeResult = creep.UpgradeController(roomController);

            //if (upgradeResult == CreepUpgradeControllerResult.NotInRange)
            //{
            //   var moveStatus = creep.MoveTo(roomController.RoomPosition);
            //}
            //else if (upgradeResult != CreepUpgradeControllerResult.Ok)
            //{
            //    //  Console.WriteLine($"{this}: {creep} unexpected result when upgrading {roomController} ({upgradeResult})");
            //}


            //var upgradeResult = creep.UpgradeController(roomController);

            //if (upgradeResult == CreepUpgradeControllerResult.NotInRange)
            //{
            //    moveStatus = creep.MoveTo(roomController.RoomPosition);
            //}
            //else if (upgradeResult != CreepUpgradeControllerResult.Ok)
            //{
            //  //  Console.WriteLine($"{this}: {creep} unexpected result when upgrading {roomController} ({upgradeResult})");
            //}
            //}
            //else
            //{


            // We're empty, go to pick up
            if (!spawns.Any()) { return; }
            var spawn = spawns.Best(x => -x.LocalPosition.LinearDistanceTo(creep.LocalPosition));// && spawns.Where(x => x.Store.GetUsedCapacity(ResourceType.Energy) >= 50));
            if (spawn == null) { return; }

            CreepWithdrawResult withdrawResult;

            if (this.controllerLink != null)
            {
                if (controllerLink.Store.GetUsedCapacity(ResourceType.Energy) >= 100)
                {
                    withdrawResult = creep.Withdraw(this.controllerLink, (ResourceType.Energy));
                    if (withdrawResult == CreepWithdrawResult.NotInRange)
                    {
                        creep.MoveTo(this.controllerLink.RoomPosition);
                        return;
                    }

                }
            }
            //myDebug.WriteLine($"creep.Store[ResourceType.Energy]: {creep.Store[ResourceType.Energy]}");
            //if (creep.Store[ResourceType.Energy] > 0)
            //{
            //    There is energy to drop off

            //    CreepMoveResult moveStatus;

            //    if (creep.RoomPosition.Position.IsNextTo(room.Controller.RoomPosition.Position) == false)
            //    {
            //        moveStatus = creep.MoveTo(roomController.RoomPosition);
            //    }

            //    if (creep.RoomPosition.Position.IsNextTo(room.Controller.RoomPosition.Position) == true)
            //    {
            //        if (game.Time % 100 == 0)
            //        {
            //            creep.SignController(room.Controller, "Screeps In C#");

            //        }
            //    }
            //    var upgradeResult = creep.UpgradeController(roomController);
            //}
            // pause worker from getting energy when creeps need to be spawned.

            if (room.Controller.Level <= 4 && this.spawnQueue.Count > 0)
            {
                return;
            }

            getEnergy(creep);
            //    if (room.Storage != null)
            //    {
            //        //    Console.WriteLine(" -------- Storeage exists in room");
            //        withdrawResult = creep.Withdraw(room.Storage, ResourceType.Energy);
            //        if (withdrawResult == CreepWithdrawResult.NotInRange)
            //        {
            //            creep.MoveTo(room.Storage.RoomPosition);
            //        }
            //        return;

            //    }

            //    withdrawResult = creep.Withdraw(spawn, ResourceType.Energy);
            //    if (withdrawResult == CreepWithdrawResult.NotInRange)
            //    {
            //        creep.MoveTo(spawn.RoomPosition);
            //    }
            //    else if (withdrawResult != CreepWithdrawResult.Ok)
            //    {
            //       // Console.WriteLine($"{this}: {creep} unexpected result when withdrawing from {spawn} ({withdrawResult})");
            //    }
            //}
        }

        private void TickBuilder(ICreep creep)
        {

            if (!creep.Exists)
            {
                return;
            }
            // Check energy storage
            creep.Say("bld");
            PickupEnergy(creep);


            // if there are no more construstrion sites then run as upgrader.
            if (constructionSites.Count == 0)
            {
                TickWorker(creep);
                return;
            }

            if (creep.Store[ResourceType.Energy] > 0)
            {
                // There is energy to drop off
                // var upgradeResult = creep.UpgradeController(roomController);




                IConstructionSite buildTarget = constructionSites.First();
                if (!buildTarget.Exists)
                {
                    return;
                }

                CreepMoveResult moveStatus;


                if (creep.RoomPosition.Position.IsNextTo(buildTarget.RoomPosition.Position) == false)
                {
                    moveStatus = creep.MoveTo(buildTarget.RoomPosition);
                }
                var buildResult = creep.Build(buildTarget);
                // var upgradeResult = creep.UpgradeController(roomController);



                //if (buildResult == CreepBuildResult.NotInRange)
                //{
                //    creep.MoveTo(buildTarget.RoomPosition);
                //}
                //else if (buildResult != CreepBuildResult.Ok)
                //{
                //    // Console.WriteLine($"{this}: {creep} unexpected result when upgrading {roomController} ({buildResult})");
                //}
            }
            else
            {


                // do not get energy while there are creeps in the queue.
                if (room.Controller.Level <= 6 && spawnQueue.Count > 0)
                {
                    return;
                }

                getEnergy(creep);

            }
        }

        private void TickRepairer(ICreep creep)
        {

            if (!creep.Exists)
            {
                return;
            }


            creep.Say("rpr");
            if (constructionSites.Count > 0)
            {
                Console.WriteLine($"switch repair creep to builder: {creep} constructionSites.Count: {constructionSites.Count}");
                TickBuilder(creep);
                return;
            }


            if (creep.Store[ResourceType.Energy] > 0)
            {


                // There is energy to drop off
                // var upgradeResult = creep.UpgradeController(roomController);
                // Console.WriteLine("repairTargets.Count: " + repairTargets.Count);

                // Console.WriteLine("        repairTargets.ToString(): " + repairTargets.ToString());
                // Console.WriteLine("        repairTargets.ToArray().ToString(): " + repairTargets.ToArray().ToString());


                if (repairTargets.Count == 0)
                {
                    // Console.WriteLine("   Nothing to repair repairTargets.Count: " + repairTargets.Count + "  Running as Tanker");
                    TickTankers(creep);
                    return;
                }

                IStructure repairTarget = repairTargets.First();
                //  Console.WriteLine("        repairTarget.ToString(): " + repairTarget.ToString());


                var repairResult = creep.Repair(repairTarget);

                //    Console.WriteLine("        repairResult: " + repairResult);

                if (repairResult == CreepRepairResult.NotInRange)
                {
                    creep.MoveTo(repairTarget.RoomPosition);
                }
                else if (repairResult != CreepRepairResult.Ok)
                {
                    //  Console.WriteLine($"{this}: {creep} unexpected result when repairing {roomController} ({repairResult})");
                }
            }
            else
            {

                // do not get energy while there are creeps in the queue.
                if (room.Controller.Level <= 6 && spawnQueue.Count > 0)
                {
                    return;
                }

                getEnergy(creep);

            }
        }

        private void TickTankers(ICreep creep)
        {
            // Console.WriteLine("tanker is now running");
            creep.Say("tnk");

            if (towerTargets.Count == 0 && extensionTargets.Count == 0)
            {
                TickBuilder(creep);
                return;
            }

            if (extensionTargets.Count != 0)
            {
                //  Console.WriteLine("tanker is filling Extensions");

                FillExtentions(creep);
                return;
            }

            if (towerTargets.Count != 0)
            {
                // Console.WriteLine("tanker is filling towers");
                FillTowers(creep);
                return;
            }





        }

        private void TickTowers(IStructureTower tower)
        {
            foreach (ICreep invader in invaderCreeps)
            {
                tower.Attack(invader);
                return;
            }

            //  Console.WriteLine("      ++++++++++------------  repairTargets.Count: " + repairTargets.Count);
            if (demagedCreepsInRoom.Count > 0)
            {
                foreach (var creep in this.demagedCreepsInRoom)
                {
                    tower.Heal(creep);
                    return;
                }
            }


            foreach (var damagedStructure in this.repairTargets)
            {

                if (damagedStructure.GetType().ToString() == "ScreepsDotNet.Native.World.NativeStructureController")
                {
                    //Console.WriteLine("      ++++++++++------------  Skipping amagedStructure: " + damagedStructure);

                    continue;
                }

                var repairStatus = tower.Repair(damagedStructure);

                // Console.WriteLine("      ++++++++++------------  damagedStructure: " + damagedStructure);
                // Console.WriteLine("      ++++++++++------------  damagedStructure.GetType(): " + damagedStructure.GetType().ToString());
                //  Console.WriteLine("      ++++++++++------------  repairStatus: " + repairStatus);

                return;
            }
        }
    }
}