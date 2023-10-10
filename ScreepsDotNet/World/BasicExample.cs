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

// W27S55 reached level 7 morning of Wed Sept 19th, 2023
// N1W8 reached level 6 afternoon of Mon Oct 9th, 2023
namespace ScreepsDotNet.World
{
    public class BasicExample : ITutorialScript
    {
        private readonly IGame game;

        private readonly IDictionary<IRoom, RoomManager> roomManagers = new Dictionary<IRoom, RoomManager>();

        public BasicExample(IGame game)
        {
            this.game = game;
            CleanMemory();
        }

        public void Loop()
        {
            // Check for any rooms that are no longer visible and remove their manager
            var trackedRooms = roomManagers.Keys.ToArray();
            foreach (var room in trackedRooms)
            {
                if (room.Exists) { continue; }
                Console.WriteLine($"Removing room manager for {room} as it is no longer visible");
                roomManagers.Remove(room);
            }

            // Iterate over all visible rooms, create their manager if needed, and tick them
            foreach (var room in game.Rooms.Values)
            {
                if (!room.Controller?.My ?? false) { continue; }
                if (!roomManagers.TryGetValue(room, out var roomManager))
                {
                    roomManager = new RoomManager(game, room);
                    roomManagers.Add(room, roomManager);
                    Console.WriteLine($"Adding room manager for {room} as it is now visible and controlled by us");
                    roomManager.Init();
                }
                roomManager.Tick();
            }

        }

        private void CleanMemory()
        {
            if (!game.Memory.TryGetObject("creeps", out var creepsObj)) { return; }
            int clearCnt = 0;
            foreach (var creepName in creepsObj.Keys)
            {
                if (!game.Creeps.ContainsKey(creepName))
                {
                    creepsObj.ClearValue(creepName);
                    ++clearCnt;
                }
            }
            if (clearCnt > 0) { Console.WriteLine($"Cleared {clearCnt} dead creeps from memory"); }
        }

    }



    internal class RoomManager
    {
        private readonly IGame game;
        private readonly IRoom room;
        private long currentGameTick;
        private readonly IStructureController roomController;
        private IStructureSpawn Spawn1;
        private IStructureSpawn Spawn2;
        private IStructureSpawn Spawn3;
        private ISource Source1;
        private ISource Source2;
        // private IStructureSpawn Spawn11;
        //   private IStructureSpawn Spawn12;
        private readonly ISet<IStructureSpawn> spawns = new HashSet<IStructureSpawn>();
        private readonly ISet<ISource> sources = new HashSet<ISource>();
        private readonly ISet<IConstructionSite> constructionSites = new HashSet<IConstructionSite>();
        private readonly ISet<ICreep> demagedCreepsInRoom = new HashSet<ICreep>();      
        private readonly ISet<IStructure> repairTargets = new HashSet<IStructure>();
        private readonly ISet<IStructureExtension> extensionTargets = new HashSet<IStructureExtension>();
        //  private readonly ISet<IStructureTower> towersInRoom = new HashSet<IStructureTower>();
        private ISet<IStructureTower> towersInRoom = new HashSet<IStructureTower>();
        private ISet<IStructure> structuresThatNeedRepair = new HashSet<IStructure>();
        private readonly ISet<IStructureTower> towerTargets = new HashSet<IStructureTower>();
        private ISet<IFlag> allRoomFlags = new HashSet<IFlag>();
        private ISet<IStructureContainer> allContainers = new HashSet<IStructureContainer>();
        //    private ISet<IConstructionSite> allContructionSitesInRoom = new HashSet<IConstructionSite>();
        private IEnumerable<IConstructionSite> allConstructionSitesInRoom;
        // private ISet<IStructureExtension> extensionsInRoom = new HashSet<IStructureExtension>();


        // that need energy
        //private readonly IStructureStorage storage.;



        private readonly ISet<ICreep> allCreepsInRoom = new HashSet<ICreep>();
        private HashSet<ICreep> invaderCreeps = new HashSet<ICreep>();
        //private IEnumerable invaderCreeps;

        private readonly ISet<ICreep> minerCreepsSource1 = new HashSet<ICreep>();
        private readonly ISet<ICreep> minerCreepsSource2 = new HashSet<ICreep>();
        private readonly ISet<ICreep> builderCreeps = new HashSet<ICreep>();
        private readonly ISet<ICreep> workerCreeps = new HashSet<ICreep>();
        private readonly ISet<ICreep> repairCreeps = new HashSet<ICreep>();
        private readonly ISet<ICreep> tankerCreeps = new HashSet<ICreep>();
        //private ISet<IStructure> allRoomStructures = new HashSet<IStructure>();
        private IEnumerable<IStructure> allRoomStructures;
        private ISet<IRoomObject> allRoomObjects = new HashSet<IRoomObject>();
        private Queue<myCreepData> spawnQueue = new Queue<myCreepData>();
        private CpuUseageHistory<double> cpuUseageHistory = new CpuUseageHistory<double>(1500);


        //private IEnumerable allRoomStructures;

        private readonly Random rng = new();

        private int targetMiner1Count = 3;
        private int targetMiner2Count = 3;
        // private int targetUpgraderCount = 7;
        private int targetTankerCount = 1;
        private int targetBuilderCount = 2;

        private int targetRepairerCount = 0;
        private int targetWorkerCount = 3;
        
        // private int targetUpgraderCount1 = 0;
        // private int targetMinerCountTest = 0;

        // flags
        private IFlag source1Flag;
        private IFlag source2Flag;
        private IFlag source1LinkFlag;
        private IFlag source2LinkFlag;
        private IFlag storageLinkFlag;
        private IFlag storageFlag;
        private IFlag controllerLinkFlag;

        // Links
        private IStructureLink? source1Link;
        private IStructureLink? source2Link;
        private IStructureLink? controllerLink;
        private IStructureLink? storageLink;
        private IStructureContainer Source1Container;
        private IStructureContainer Source2Container;

        //private IStructureLink? source1Link;
        //private IStructureLink? source2Link;
        //private IStructureLink? ControllerLink;

        private HashSet<IStructureRoad> roadsInRoom;
        private HashSet<IStructureContainer> containersInRoom;
        private HashSet<IStructureRampart> rampartsInRoom;
        private HashSet<IStructureWall> wallsInRoom;
        private HashSet<IStructureRoad> roadsThatNeedRepair;
        private HashSet<IStructureContainer> containersThatNeedRepair;

        private static readonly BodyType<BodyPartType> simpleWorkerBodyType = new(stackalloc (BodyPartType, int)[] { (BodyPartType.Move, 1), (BodyPartType.Carry, 1), (BodyPartType.Work, 1) });
        private static readonly BodyType<BodyPartType> WorkerLevel5BodyType = new(stackalloc (BodyPartType, int)[] { (BodyPartType.Move, 6), (BodyPartType.Carry, 2), (BodyPartType.Work, 4) });
        private static readonly BodyType<BodyPartType> workerLevel4BodyType = new(stackalloc (BodyPartType, int)[] { (BodyPartType.Move, 4), (BodyPartType.Carry, 2), (BodyPartType.Work, 2) });
        private static readonly BodyType<BodyPartType> Miner1Level4BodyType = new(stackalloc (BodyPartType, int)[] { (BodyPartType.Move, 1), (BodyPartType.Carry, 1), (BodyPartType.Work, 4) });
        private static readonly BodyType<BodyPartType> medWorkerBodyType = new(stackalloc (BodyPartType, int)[] { (BodyPartType.Move, 4), (BodyPartType.Carry, 2), (BodyPartType.Work, 2) });
        private static readonly BodyType<BodyPartType> miner1BodyType = new(stackalloc (BodyPartType, int)[] { (BodyPartType.Move, 5), (BodyPartType.Carry, 2), (BodyPartType.Work, 5) });
        private static readonly BodyType<BodyPartType> miner1LargeBodyType = new(stackalloc (BodyPartType, int)[] { (BodyPartType.Move, 7), (BodyPartType.Carry, 4), (BodyPartType.Work, 7) });
        private static readonly BodyType<BodyPartType> miner2BodyType = new(stackalloc (BodyPartType, int)[] { (BodyPartType.Move, 1), (BodyPartType.Carry, 1), (BodyPartType.Work, 7) });
        private static readonly BodyType<BodyPartType> miner2Level5TravelBodyType = new(stackalloc (BodyPartType, int)[] { (BodyPartType.Move, 5), (BodyPartType.Carry, 1), (BodyPartType.Work, 7) });

        private static IMemoryObject gameMemory;
        private IMemoryObject roomMemory;
        private IMemoryObject creepMemory;
        private SpawnCreepOptions spawnCreepOptions;
        private IRoomVisual roomVisual;

        /// <summary>
        /// Room  Settings
        /// </summary>
        private Stopwatch stopwatch = new Stopwatch();
        private SpawningManager spawningManager;
        private long executeTimeUpToCreeps;
        private long creepsExecuteTime;
        private ScreepsProfiler profiler = new ScreepsProfiler();
        private bool debug = true;
        private bool displayStats = true;
        private bool softReset = false;
        private bool hardReset = false;
        private bool enableSource1Link = true;
        private bool enableSource2Link = true;
        private long tickTotalExecutionTime; 
        private long disableEverything;
        private int maxTotalCreeps = 13;
        private bool pauseAllCreeps = false;
        private int maxRampartHits = 5000;
        private int maxWallHits = 5000;
        private int respawnTick = 20;


        public RoomManager(IGame game, IRoom room)
        {
            this.game = game;
            this.room = room;

            var roomController = room.Controller;
            if (roomController == null) { throw new InvalidOperationException($"Room {room} has no controller!"); }
            this.roomController = roomController;
            gameMemory = game.Memory;
            // roomMemory = gameMemory.GetOrCreateObject(this.room.Name);
            roomMemory = game.Memory.GetOrCreateObject(this.room.Name);
            //   var firstKey = game.Spawns.Cast<DictionaryEntry>().First().Key.ToString();
            // // Spawn1 = game.Spawns[firstKey];

            // Spawn1 = game.Spawns["Spawn1"];
            // Spawn2 = game.Spawns["Spawn2"];

            this.Spawn1 = game.Spawns.Values.ToArray()[0];
            if (game.Spawns.Values.ToArray().Length == 2)
            {
                this.Spawn2 = game.Spawns.Values.ToArray()[1];
            }

            if (game.Spawns.Values.ToArray().Length == 3)
            {
                this.Spawn3 = game.Spawns.Values.ToArray()[2];
            }

        }

        public void Init()
        {
            CleanMemory();
            currentGameTick = game.Time;
            SyncScreepsGameObjects();
            roomVisual = game.CreateRoomVisual(this.room.Name);
   
            GetRoomSpawns();
            GetRoomSources();
            spawningManager = new SpawningManager(Spawn1);

            this.Source1 = GetSource1();
            this.Source2 = GetSource2();
            this.Source1Container = GetSource1Container();
            this.Source2Container = GetSource2Container();

            if (this.room.Name == "W27S55")
            {
                if (!Spawn1.Memory.TryGetInt("targetMiner1Count", out this.targetMiner1Count))
                {
                    Spawn1.Memory.SetValue("targetMiner1Count", 4);
                }

                if (!Spawn1.Memory.TryGetInt("targetMiner2Count", out this.targetMiner2Count))
                {
                    Spawn1.Memory.SetValue("targetMiner2Count", 2);
                }

                if (!Spawn1.Memory.TryGetInt("targetTankerCount", out this.targetTankerCount))
                {
                    Spawn1.Memory.SetValue("targetTankerCount", 1);
                }

                if (!Spawn1.Memory.TryGetInt("respawnTick", out this.respawnTick))
                {
                    Spawn1.Memory.SetValue("respawnTick", 20);
                }


                if (!Spawn1.Memory.TryGetInt("maxTotalCreeps", out this.maxTotalCreeps))
                {
                    Spawn1.Memory.SetValue("maxTotalCreeps", 9);
                }




                //roomMemory.SetValue("targetMiner2Count", 2);
                //roomMemory.SetValue("targetTankerCount", 1);
                //roomMemory.SetValue("maxTotalCreeps", 16);

            }
            else if (this.room.Name == "W1N7")
            {

                if (!Spawn1.Memory.TryGetInt("targetMiner1Count", out this.targetMiner1Count))
                {
                    Spawn1.Memory.SetValue("targetMiner1Count", 3);
                }

                if (!Spawn1.Memory.TryGetInt("targetMiner2Count", out this.targetMiner2Count))
                {
                    Spawn1.Memory.SetValue("targetMiner2Count", 3);
                }

                if (!Spawn1.Memory.TryGetInt("targetTankerCount", out this.targetTankerCount))
                {
                    Spawn1.Memory.SetValue("targetTankerCount", 1);
                }

                if (!Spawn1.Memory.TryGetInt("targetBuilderCount", out this.targetBuilderCount))
                {
                    Spawn1.Memory.SetValue("targetBuilderCount", 2);
                }

                if (!Spawn1.Memory.TryGetInt("targetWorkerCount", out this.targetWorkerCount))
                {
                    Spawn1.Memory.SetValue("targetWorkerCount", 2);
                }
                if (!Spawn1.Memory.TryGetInt("targetRepairerCount", out this.targetRepairerCount))
                {
                    Spawn1.Memory.SetValue("targetRepairerCount", 1);
                }



                if (!Spawn1.Memory.TryGetInt("respawnTick", out this.respawnTick))
                {
                    Spawn1.Memory.SetValue("respawnTick", 20);

                }

                if (!Spawn1.Memory.TryGetInt("maxTotalCreeps", out this.maxTotalCreeps))
                {
                    Spawn1.Memory.SetValue("maxTotalCreeps", 13);
                }

                //roomMemory.SetValue("footest", 1);
                //roomMemory.SetValue("targetMiner1Count", 3);
                //roomMemory.SetValue("targetMiner2Count", 3);
                //roomMemory.SetValue("numberOfTankers", 1);
                //roomMemory.SetValue("maxTotalCreeps", 13);

            }
            else
            {
                if (!Spawn1.Memory.TryGetInt("targetMiner1Count", out this.targetMiner1Count))
                {
                    Spawn1.Memory.SetValue("targetMiner1Count", 2);
                }

                if (!Spawn1.Memory.TryGetInt("targetMiner2Count", out this.targetMiner2Count))
                {
                    Spawn1.Memory.SetValue("targetMiner2Count", 2);
                }

                if (!Spawn1.Memory.TryGetInt("targetWorkerCount", out this.targetWorkerCount))
                {
                    Spawn1.Memory.SetValue("targetWorkerCount", 2);
                }

                

                if (!Spawn1.Memory.TryGetInt("targetTankerCount", out this.targetTankerCount))
                {
                    Spawn1.Memory.SetValue("targetTankerCount", 1);
                }
                if (!Spawn1.Memory.TryGetInt("targetBuilderCount", out this.targetBuilderCount))
                {
                    Spawn1.Memory.SetValue("targetBuilderCount", 2);
                }

                if (!Spawn1.Memory.TryGetInt("maxTotalCreeps", out this.maxTotalCreeps))
                {
                    Spawn1.Memory.SetValue("maxTotalCreeps", 6);
                }

                if (!Spawn1.Memory.TryGetInt("targetBuilderCount", out this.targetBuilderCount))
                {
                    Spawn1.Memory.SetValue("targetBuilderCount", 2);
                }

                if (!Spawn1.Memory.TryGetBool("pauseAllCreeps", out this.pauseAllCreeps))
                {
                    // this.pauseAllCreeps = false;
                    Spawn1.Memory.SetValue("pauseAllCreeps", this.pauseAllCreeps);
                }



                //roomMemory.SetValue("footest", 1);
                //roomMemory.SetValue("targetMiner1Count", 1);
                //roomMemory.SetValue("targetMiner2Count", 1);
                //roomMemory.SetValue("targetTankerCount", 1);
                //roomMemory.SetValue("maxTotalCreeps", 5);
            }

         


            this.UpdateSpawn1Memory();
            InitLinks();
            Console.WriteLine($"{this}: INIT success (tracking {spawns.Count} spawns and {sources.Count} sources)");
            Console.WriteLine($"{this}: room exits = {game.Map.DescribeExits(room.Name)}");
        }

        private void GetRoomSpawns()
        {
            spawns.Clear();
            // foreach (var spawn in room.Find<IStructureSpawn>())
            foreach (var spawn in allRoomStructures.OfType<IStructureSpawn>())
            {
                spawns.Add(spawn);
            }
        }

        private void GetRoomSources()
        {
            sources.Clear();
            foreach (var source in room.Find<ISource>())
            //   foreach (var source in  allRoomStructures.OfType<ISource>())
            {
                sources.Add(source);
            }
        }

        private void SyncScreepsGameObjects()
        {
            allRoomStructures = room.Find<IStructure>();
            allRoomFlags = room.Find<IFlag>().ToHashSet();
            containersInRoom = allRoomStructures.OfType<IStructureContainer>().ToHashSet();
            rampartsInRoom = allRoomStructures.OfType<IStructureRampart>().ToHashSet();
            wallsInRoom = room.Find<IStructureWall>().OfType<IStructureWall>().ToHashSet();

            //allContainers = allRoomStructures.OfType<IStructureContainer>().ToHashSet();
            towersInRoom = allRoomStructures.OfType<IStructureTower>().ToHashSet();
            // allContructionSitesInRoom = allRoomStructures.OfType<IConstructionSite>().ToHashSet();
            allConstructionSitesInRoom = room.Find<IConstructionSite>();
            myDebug.WriteLine($"allContructionSitesInRoom.Count: {allConstructionSitesInRoom.Count()}");

            allContainers = allRoomStructures.OfType<IStructureContainer>().ToHashSet();
            roadsInRoom = allRoomStructures.OfType<IStructureRoad>().ToHashSet();
        }


        public void Tick()
        {
            stopwatch.Reset();
            stopwatch.Start();
            Console.WriteLine(" ");
            Console.WriteLine("******************** !!Start of Tick Test World!! ********************");

            currentGameTick = game.Time;
            myDebug.debug = this.debug;


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



            if (game.Time % 100 == 0)
            {
                CleanMemory();
            }

            //  var isInDanger = GridUtility.IsPointInSubsection(new Point(9, 24), new Point(45, 45), new Point(40, 42));

            //  Console.WriteLine($"isInDanger: {isInDanger}");
            InitLinks();
            myDebug.WriteLine($"maxTotalCreeps: {maxTotalCreeps}");
            myDebug.WriteLine($"Source1: {Source1}");
            myDebug.WriteLine($"Source2: {Source2}");
            myDebug.WriteLine($"source2Link: {source2Link}");
            myDebug.WriteLine($"controllerLink: {controllerLink}");
            //GetSource1Container();
            myDebug.WriteLine($" xxx containersInRoom.Count: {containersInRoom.Count}");
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
            spawningManager.GetSpawnMemory();
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

        private void InitLinks()
        {
            if (source1Link == null)
            {
                source1Link = GetSource1Link(allRoomStructures);
                myDebug.WriteLine($"***1 source1Link: {source1Link}");

            }

            if (source2Link == null)
            {
                source2Link = GetSource2Link(allRoomStructures);
                myDebug.WriteLine($"***1 source2Link: {source2Link}");

            }

            if (controllerLink == null)
            {
                controllerLink = GetControllerLink(allRoomStructures);
                myDebug.WriteLine($"***1 controllerLink: {controllerLink}");
            }

            if (storageLink == null)
            {
                storageLink = GetStorageLink(allRoomStructures);
                myDebug.WriteLine($"***1 controllerLink: {controllerLink}");
            }
        }

        private void TickAllSpawns()
        {
            // Tick all spawns
            foreach (var spawn in spawns)
            {
                if (!spawn.Exists) { continue; }
                TickSpawn(spawn);
            }
        }

        private void TickGetStructureUpdates(int updateTick)
        {
            if (currentGameTick % updateTick != 0)
            {
                return;
            }
            myDebug.WriteLine("Start UpdateTowersEngeryStatus(10)");
            UpdateTowersEngeryStatus(10);
            myDebug.WriteLine("completed UpdateTowersEngeryStatus(10)");

          
            // profiler.Profile("CheckTowersEngeryStatus", () => UpdateTowersEngeryStatus(10), false);
            myDebug.WriteLine("Start UpdateExentions(10)");
            UpdateExentions(10);
            myDebug.WriteLine("completed UpdateExentions(10)");
          
            myDebug.WriteLine("Start UpdateConstructionSites(10);");
            UpdateConstructionSites(10);
            myDebug.WriteLine("completed UpdateConstructionSites(10);");
            

            myDebug.WriteLine("Start UpdateDamagedCreeps(1);");
            UpdateDamagedCreeps(1);
            myDebug.WriteLine("completed UpdateDamagedCreeps(1);");

            myDebug.WriteLine("Start UpdateDamagedCreeps(10);");
            UpateRepartTargets(1);
            myDebug.WriteLine("completed UpateRepartTargets(10);");



            // Console.WriteLine(" ");
            //  Console.WriteLine("Adding constructionSite to constructionSites");

            // Console.WriteLine(" ");



            // Console.WriteLine(" ");
            // Console.WriteLine("Adding extension that need energy to to extensionTargets");
            //extensionTargets.Clear();
            //foreach (var extension in extensionsInRoom.Where(x => x.Store.GetUsedCapacity(ResourceType.Energy) < x.Store.GetCapacity(ResourceType.Energy)))
            //{
            //    extensionTargets.Add(extension);
            //}

            //  Console.WriteLine("  extension that need energy: " + extensionTargets.Count);
            //  Console.WriteLine(" ");

        }

        private void UpdateExentions(int updateTick)
        {

            if (currentGameTick % updateTick != 0)
            {
                return;
            }

            var extensionsInRoom = allRoomStructures.OfType<IStructureExtension>();
            var extensionsThatNeedEnergy = extensionsInRoom.Where(x => x.Store.GetFreeCapacity(ResourceType.Energy) > 0);
            //var extentionsThatNeedEnergy = extensionsInRoom.Where(x => x.Store.GetUsedCapacity(ResourceType.Energy) < 50);

            extensionTargets.Clear();

            foreach (var extensionsTarget in extensionsThatNeedEnergy)
            {
                extensionTargets.Add(extensionsTarget);
            }

            myDebug.WriteLine("extensions that need energy: " + extensionTargets.Count);
        }

        private void UpdateConstructionSites(int updateTick)
        {
            if (currentGameTick % updateTick != 0)
            {
                return;
            }

            constructionSites.Clear();
            //foreach (var constructionSite in allRoomStructures.OfType<IConstructionSite>())
            foreach (var constructionSite in allConstructionSitesInRoom)
            {
                constructionSites.Add(constructionSite);
            }
            myDebug.WriteLine("   constructionSites.Count: " + constructionSites.Count);
        }

        private void UpdateDamagedCreeps(int updateTick)
        {
            if (currentGameTick % updateTick != 0)
            {
                return;
            }

            var damagedCreeps = allCreepsInRoom.Where(x => x.Hits < x.HitsMax);
            demagedCreepsInRoom.Clear();
            foreach (var damagedCreep in damagedCreeps)
            {
                demagedCreepsInRoom.Add(damagedCreep);
            }
            myDebug.WriteLine("   demagedCreepsInRoom.Count: " + demagedCreepsInRoom.Count);
        }



        private void UpateRepartTargets(int updateTick)
        {
            if (currentGameTick % updateTick != 0)
            {
                return;
            }

            repairTargets.Clear();

            var roadsThatNeedRepair = roadsInRoom.Where(x => x.Hits < x.HitsMax);

            foreach (var road in roadsThatNeedRepair)
            {
                repairTargets.Add(road);

            }

            var containersThatNeedRepair = containersInRoom.Where(x => x.Hits < x.HitsMax);
            foreach (var container in containersThatNeedRepair)
            {
                repairTargets.Add(container);
            }

            var rampartsThatNeedRepair = rampartsInRoom.Where(x => x.Hits < maxRampartHits);
            foreach (var rampart in rampartsThatNeedRepair)
            {
                repairTargets.Add(rampart);
            }



            //var wallsThatNeedRepair = wallsInRoom.Where(x => x.Hits < maxWallHits);
            //foreach (var wall in wallsThatNeedRepair)
            //{
            //    repairTargets.Add(wall);
            //}

            foreach (var repairTarget in repairTargets)
            {

                Console.WriteLine($"repairTarget: {repairTarget} {repairTarget.Hits}");
            }



            myDebug.WriteLine("structures that need be repaired: " + repairTargets.Count);
        }



        private void UpdateTowersEngeryStatus(int updateTick)
        {
            if (currentGameTick % updateTick != 0)
            {
                return;
            }

            towerTargets.Clear();

            var towersThatNeedEnergy = towersInRoom.Where(x => x.Store.GetUsedCapacity(ResourceType.Energy) <= 900);

            // adding towers to items that need additional energy
            if (towersThatNeedEnergy.Count() > 0)
            {
                foreach (var Tower in towersThatNeedEnergy)
                {
                    towerTargets.Add(Tower);
                }
                myDebug.WriteLine("   Towers that need energy: " + towerTargets.Count);
                myDebug.WriteLine("Adding extensions to extensionTargets");
            }
        }

        private void RunCreeps()
        {
            foreach (var creep in minerCreepsSource1)
            {
                // creep.Say("m");
                //  if (creep.Exists && minerCount < 4)
                // {
                TickSource1Miner(creep, this.Source1);
                // Console.WriteLine("right after profile results!");
                //   minerCount++;
                //}

                //else
                //{
                //    TickMinerSource2(creep);

                //    //Console.WriteLine($"{this}: {creep} assigned is a dead miner");

                //}
            }
            foreach (var creep in minerCreepsSource2)
            {
                TickSource2Miner(creep,this.Source2);
            }
            foreach (var creep in builderCreeps)
            {
                if (creep.Exists)
                {
                    TickBuilder(creep);
                }
                else
                {
                    //  Console.WriteLine($"{this}: {creep} assigned is a dead upgrader");

                }
            }
            foreach (var creep in workerCreeps)
            {

                if (creep.Exists)
                {
                    TickWorker(creep);
                }
                else
                {
                    //  Console.WriteLine($"{this}: {creep} assigned is a dead upgrader");

                }
            }
            foreach (var creep in tankerCreeps)
            {
                if (creep.Exists == false)
                {
                    continue;
                }

                if (extensionTargets.Count > 0 || towerTargets.Count > 0)
                {
                    TickTankers(creep);
                    continue;
                }

                if (creep.Exists)
                {
                    //  Console.WriteLine($"{this}: {creep} tanker switching to upgrader");

                    TickWorker(creep);
                }
                else
                {
                    //   Console.WriteLine($"{this}: {creep} Tanker creeps are doing nothing");

                }
            }
            foreach (var creep in repairCreeps)
            {
                if (creep.Exists == false)
                {
                    continue;
                }

                if (extensionTargets.Count > 0)
                {
                    TickBuilder(creep);
                    continue;
                }

                if (creep.Exists)
                {
                    TickRepairer(creep);
                }
                else
                {
                    Console.WriteLine($"{this}: {creep} assigned is a dead repairer");

                }
            }

            // return minerCount;
        }

        private void TickTowers()
        {
            foreach (var tower in towersInRoom)
            {
                // Console.WriteLine($"------- towersInRoom.Count: {towersInRoom.Count}");
                TickTowers(tower);
            }
        }

        private void RoomOverlay(bool enable)
        {
            if (enable == false)
            {
                return;
            }
            var bucket = game.Cpu.Bucket;
            //  var getUsed = game.Cpu.GetUsed;
            // Console.WriteLine($"getUsed: {getUsed.ToString()}");
            var x = 3; var y = 10;


            var cpuUsed = game.Cpu.GetUsed();
            if (cpuUsed == 0 || cpuUsed >= 20)
            {
                return;
            }

            cpuUseageHistory.Enqueue(cpuUsed);
            var cpuAverage = cpuUseageHistory.CalculateAverage();

            // roomText($"       executeTimeUpToCreeps: {executeTimeUpToCreeps} ms", x, y);
            // roomText($"Execution Time of all Creeps: {creepsExecuteTime} ms", x, ++y);
            // roomText($"Execution From Creeps To End: {toTheEndExecutionTime} ms", x, ++y);tickTotalExecutionTime
            if (room.Storage != null)
          //      if (room.Storage.Exists && room.Storage != null)
            {
                roomText($"                room.Storage: {room.Storage.Store.GetUsedCapacity(ResourceType.Energy)}", x, ++y);

            }
            roomText($"         Tick Execution Time: {tickTotalExecutionTime} ms", x, ++y);
            roomText($"          game.Cpu.GetUsed(): {Math.Round(cpuUsed, 2)}", x, ++y);

            roomText($"                  cpuAverage: {Math.Round(cpuAverage, 2)}", x, ++y);
            roomText($"                      bucket: {bucket}", x, ++y);
        }

        private void UpdateSpawn1Memory()
        {

            this.targetMiner1Count = GetSpawnMemory("targetMiner1Count", this.targetMiner1Count);

            // Spawn1.Memory.TryGetInt("targetMiner1Count", out this.targetMiner1Count);
            //  myDebug.WriteLine($"targetMiner1Count: {this.targetMiner1Count}");

            this.targetMiner2Count = GetSpawnMemory("targetMiner2Count", this.targetMiner2Count);

            // Spawn1.Memory.TryGetInt("targetMiner2Count", out this.targetMiner2Count);
            // myDebug.WriteLine($"targetMiner2Count: {this.targetMiner2Count}");

            this.targetTankerCount = GetSpawnMemory("targetTankerCount", this.targetTankerCount);
            // Spawn1.Memory.TryGetInt("targetTankerCount", out this.targetTankerCount);
            // myDebug.WriteLine($"targetTankerCount: {this.targetTankerCount}");


            this.respawnTick = GetSpawnMemory("respawnTick", this.respawnTick);

            // Spawn1.Memory.TryGetInt("respawnTick", out this.respawnTick);
            // myDebug.WriteLine($"respawnTick: {this.respawnTick}");


            this.maxTotalCreeps = GetSpawnMemory("maxTotalCreeps", this.maxTotalCreeps);

            // Spawn1.Memory.TryGetInt("maxTotalCreeps", out this.maxTotalCreeps);
            // myDebug.WriteLine($"maxTotalCreeps: {this.maxTotalCreeps}");

            this.debug = GetSpawnMemory("debug", this.debug);

            // myDebug.WriteLine($"cat: {cat}");

            this.displayStats = GetSpawnMemory("displayStats", this.displayStats);

            //Spawn1.Memory.TryGetBool("displayStats", out this.displayStats);
            //{
            //    Spawn1.Memory.SetValue("displayStats", false);
            //}

            //  myDebug.WriteLine($"displayStats: {this.displayStats}"); //test

            //SetSpawnMemory(string name,<T>);

            //myDebug.WriteLine($"displayStats: {this.softReset}"); //test
    
          //  this.pauseAllCreeps = GetSpawnMemory("pauseAllCreeps", this.pauseAllCreeps);
           // this.disableEverything = GetSpawnMemory("disableEverything", this.disableEverything);//ss
            this.softReset = GetSpawnMemory("softReset", this.softReset);
            this.hardReset = GetSpawnMemory("hardReset", this.hardReset);
            this.enableSource1Link = GetSpawnMemory("enableSource1Link", this.enableSource1Link);
            this.enableSource2Link = GetSpawnMemory("enableSource2Link", this.enableSource2Link);
            this.maxRampartHits = GetSpawnMemory("maxRampartHits", this.maxRampartHits);
            spawningManager.GetSpawnMemory();
           
            //var status = Spawn1.Memory.TryGetBool("softReset", out this.softReset);
            //if (status == false)
            //{
            //    Spawn1.Memory.SetValue("softReset", false);
            //}

            //status = Spawn1.Memory.TryGetBool("hardReset", out this.hardReset);
            //if (status == false)
            //{
            //    Spawn1.Memory.SetValue("hardReset", false);
            //}

            //var status = Spawn1.Memory.TryGetBool("enableSource1Link", out this.enableSource1Link);
            //if (status == false)
            //{
            //    Spawn1.Memory.SetValue("enableSource1Link", this.enableSource1Link);
            //}

            //status = SetSpawnMemory ();

            //bool SetSpawnMemory <T>(IStructureSpawn spawn, string keyName, T memValue)
            //{

            //    if (memValue is bool)
            //    {
            //        if (memValue == null)
            //            bool status = Spawn.Memory.TryGetBool("enableSource2Link", out T memValue) where ;
            //            if (status == false)
            //            {
            //                Spawn1.Memory.SetValue("enableSource2Link", this.enableSource2Link);
            //            }

            //    }

            //    return status;
            //}



            //this.softReset = GetSpawnMemory("softReset", false);
            //var bob = GetSpawnMemory("bob", "bob1");
            //myDebug.WriteLine($"bob: {bob}"); //test

            //spawningManager.targetMiner1Count = this.targetMiner1Count;
            //spawningManager.targetMiner2Count = this.targetMiner2Count;
            //spawningManager.targetBuilderCount = this.targetBuilderCount;
            //spawningManager.targetTankerCount = this.targetTankerCount;
            //spawningManager.targetWorkerCount = this.targetWorkerCount;

        }

        public T? GetSpawnMemory<T>(string name, T defaultValue)
        {

            if (defaultValue is bool)
            {
                var _default = Convert.ToBoolean(defaultValue);
                bool outVar;

                var status = Spawn1.Memory.TryGetBool(name, out outVar);
                if (status == false)
                {
                    Spawn1.Memory.SetValue(name, _default);
                }


                return (T)(Object)outVar;
            }

            if (defaultValue is string)
            {

                string _default = Convert.ToString(defaultValue);
                ///string outVar = Convert.ToString(outVar);
                string outVar;
                // bool _value = Convert.ToBoolean(value);

                var status = Spawn1.Memory.TryGetString(name, out outVar);
                if (status == false)
                {
                    Spawn1.Memory.SetValue(name, _default);
                }
                return (T)(Object)outVar;
            }
            if (defaultValue is int)
            {

                var _default = Convert.ToInt32(defaultValue);
                int outVar;
                // bool _value = Convert.ToBoolean(value);

                var status = Spawn1.Memory.TryGetInt(name, out outVar);
                if (status == false)
                {
                    Spawn1.Memory.SetValue(name, _default);
                }
                return (T)(Object)outVar;
            }

            return (T)(Object)"";



        }
        public T? GetSpawnMemory<T>(string name)
        {
            var outVar = GetSpawnMemory(name, " ");
            return (T)(Object)outVar;
        }

        private void CreepHandler()
        {
            var allCreeps = room.Find<ICreep>().ToHashSet();

            // Check for any creeps we're tracking that no longer exist


            foreach (ICreep? creep in this.allCreepsInRoom.ToArray())
            {
                var respawnTick = this.respawnTick;

                if (creep.Exists)
                {
                    if (creep.My == false)
                    {
                        return;
                    }

                    if (creep.Exists)
                    {

                        var getMemStatus = creep.Memory.TryGetInt("respawnTick", out var creepRespawnTick);
                        if (getMemStatus)
                        {
                            respawnTick = creepRespawnTick;
                        }

                        if (creep.TicksToLive == respawnTick)
                        {
                            var creepData = new myCreepData(creep);


                            //if (creepData.role == "miner1")
                            //{
                            //    if (minerCreepsSource1.Count > this.targetMiner1Count)
                            //    {
                            //        return;
                            //    }
                            //}


                            if (creepData.role == "miner2")
                            {
 //                               if (minerCreepsSource2.Count > this.targetMiner2Count && this.targetMiner2Count > 1)
                                if (minerCreepsSource2.Count > this.targetMiner2Count)
                                {
                                    return;
                                }
                            }

                            //if (creepData.role == "worker")
                            //{
                            //    if (workerCreeps.Count > this.targetWorkerCount)
                            //    {
                            //        return;
                            //    }
                            //}

                            //if (creepData.role == "buider")
                            //{
                            //    if (builderCreeps.Count > this.targetBuilderCount)
                            //    {
                            //        return;
                            //    }
                            //}

                            //if (creepData.role == "tanker")
                            //{
                            //    if (tankerCreeps.Count > this.targetTankerCount)
                            //    {
                            //        return;
                            //    }
                            //}



                            myDebug.WriteLine($"creepData.spawnName {creepData.spawnName}");
                            myDebug.WriteLine($"creepData.role {creepData.role}");
                            
                         
                            
                            spawnQueue.Enqueue(creepData);
                            //   }

                        }
                    }
                    continue;
                }

                var removeStatus = allCreepsInRoom.Remove(creep);
                //Console.WriteLine("Creep name: " + creep.Name);

                OnCreepDied(creep);


            }

            invaderCreeps = allCreeps.Where(x => !x.My).ToHashSet();
            var newCreepList = allCreeps.Where(x => x.My); //.OrderByDescending(x => x.TicksToLive).ToList();


            foreach (var creep in newCreepList)
            {
                TTLCountDown(creep, 100);
                if (!allCreepsInRoom.Add(creep)) { continue; }
                OnAssignRole(creep);
            }

            //  stopwatch.Stop();
            //   long executeTimeUpToCreeps = stopwatch.ElapsedMilliseconds;

            // Tick all tracked creeps
            // var minerCount = 0;
            // stopwatch.Reset();
            // stopwatch.Start();
            var status = Spawn1.Memory.TryGetBool("pauseAllCreeps", out var pauseAllCreeps);
            // Console.WriteLine($"status: {status}");
            if (status == true)
            {
                if (pauseAllCreeps == false)
                {
                    RunCreeps();
                    //profiler.Profile("RunCreeps", () => RunCreeps(), true);
                }
            }


            //stopwatch.Stop();
            //creepsExecuteTime = stopwatch.ElapsedMilliseconds;
            //stopwatch.Reset();
            //stopwatch.Start();

        }

        private void DisplayStats()
        {
            //bool displayStats;
            //var status = Spawn1.Memory.TryGetBool("displayStats", out displayStats);

            if (this.displayStats == false)
            {
                return;
            }

            Console.WriteLine(" ");
            Console.WriteLine("        spawnQueue.Count.Count: " + spawnQueue.Count);
            Console.WriteLine("         AllCreepsInRoom.Count: " + allCreepsInRoom.Count);
            Console.WriteLine("      minerCreepsSource1.Count: " + minerCreepsSource1.Count);
            Console.WriteLine("      minerCreepsSource2.Count: " + minerCreepsSource2.Count);
            Console.WriteLine("           builderCreeps.Count: " + builderCreeps.Count);
            Console.WriteLine("       constructionSites.Count: " + constructionSites.Count);
            //Console.WriteLine(" constructionSitesInRoom.Count: " + constructionSitesInRoom.Count());
            Console.WriteLine("          upgraderCreeps.Count: " + workerCreeps.Count);
            Console.WriteLine("           invaderCreeps.Count: " + invaderCreeps.Count);
            Console.WriteLine("          repairerCreeps.Count: " + repairCreeps.Count);
            Console.WriteLine("            tankerCreeps.Count: " + tankerCreeps.Count);
            Console.WriteLine("            towerTargets.Count: " + towerTargets.Count);
            Console.WriteLine($"            towersInRoom.Count: {towersInRoom.Count}");
            Console.WriteLine("           invaderCreeps.Count: " + invaderCreeps.Count);
            Console.WriteLine("           repairTargets.Count: " + repairTargets.Count);

            //  Console.WriteLine("extensionsThatNeedEnergy.Count: " + extensionsThatNeedEnergy.Count);
            Console.WriteLine("          room.EnergyAvailable: " + room.EnergyAvailable);
            Console.WriteLine(" ");
        }

        /// <summary>
        /// assign creep to a role
        /// </summary>
        /// <param name="creep"></param>
        private void OnAssignRole(ICreep creep)
        {
            if (!creep.Exists)
            {
                return;
            }

            // Check the body type and assign the creep a role by putting it in the right tracking list s


            //if (creep.BodyType == minerBodyType && room.EnergyAvailable >= 800)
            //{
            //    minerCreepsSource2.Add(creep);

            //}

            string role;
            string spawnName;
            int respawnTick;

            var status = creep.Memory.TryGetString("role", out role);
            if (status == false)
            {
                assignNewRole(creep);
                return;
                // var status = creep.Memory.TryGetString("role", out role);
                //Console.WriteLine($" !!!!!!!!! getting role returning: {status}");
                //Console.WriteLine($" !!!!!!!!! getting role returning: {status}");
                //Console.WriteLine($" !!!!!!!!! getting role returning: {status}");
                //Console.WriteLine($" !!!!!!!!! getting role returning: {status}");
                //Console.WriteLine($" !!!!!!!!! getting role returning: {status}");


                //  creep.Memory.SetValue("role", "worker");

            }

            status = creep.Memory.TryGetString("Spawn", out spawnName);
            status = creep.Memory.TryGetInt("respawnTick", out respawnTick);

            if (role != null || role != string.Empty || role != "" && role.Length >= 1)
            {
                //Console.WriteLine($" !!!!!!!!! role.length: {role.Length} role: {role}");
                //Console.WriteLine($" !!!!!!!!! role.length: {role.Length} role: {role}");
                //Console.WriteLine($" !!!!!!!!! role.length: {role.Length} role: {role}");
                //Console.WriteLine($" !!!!!!!!! role.length: {role.Length} role: {role}");
                //Console.WriteLine($" !!!!!!!!! role.length: {role.Length} role: {role}");
                //Console.WriteLine($" !!!!!!!!! role.length: {role.Length} role: {role}");
                //Console.WriteLine($" !!!!!!!!! role.length: {role.Length} role: {role}");

            }
            else
            {
                Console.WriteLine($"-----!: {role}");
                Console.WriteLine($"-----!: {role}");
                Console.WriteLine($"-----!: {role}");
                Console.WriteLine($"-----!: {role}");
                Console.WriteLine($"-----!: {role}");
                Console.WriteLine($"-----!: {role}");
                Console.WriteLine($"-----!: {role}");
            }

            //if (creep.BodyType == simpleWorkerBodyType
            //    || creep.BodyType == medWorkerBodyType
            //    || creep.BodyType == miner1BodyType
            //    || creep.BodyType == miner1LargeBodyType
            //    || creep.BodyType == miner2BodyType) {

            if (role != null || role != string.Empty || role != "" && role.Length >= 1 && role != "worker")
            {

                Console.WriteLine($"Swtich: {role}");

                switch (role)
                {
                    case "miner1":
                        Console.WriteLine($"role: {role}");
                        creep.Memory.SetValue("Spawn", "Spawn1");
                        creep.Memory.SetValue("role", "miner1");
                        creep.Memory.SetValue("respawnTick", this.respawnTick);
                        minerCreepsSource1.Add(creep);
                        break;

                    case "miner2":
                        Console.WriteLine($"role: {role}");

                        if (spawns.Count > 1)
                        {
                            creep.Memory.SetValue("Spawn", "Spawn2");

                        }
                        else
                        {
                            creep.Memory.SetValue("Spawn", "Spawn1");

                        }
                        creep.Memory.SetValue("role", "miner2");


                        status = Spawn1.Memory.TryGetInt("miner2RespawnTick", out var miner2RespawnTick);
                        if (status)
                        {
                            creep.Memory.SetValue("respawnTick", miner2RespawnTick);

                        }
                        else
                        {
                            creep.Memory.SetValue("respawnTick", this.respawnTick);

                        }

                        //if (room.Name == "W27NS55")
                        //{
                        //    creep.Memory.SetValue("respawnTick", 94);

                        //}

                        minerCreepsSource2.Add(creep);
                        break;

                    case "tanker":
                        Console.WriteLine($"role: {role}");
                        creep.Memory.SetValue("Spawn", "Spawn1");
                        creep.Memory.SetValue("role", "tanker");
                        creep.Memory.SetValue("respawnTick", this.respawnTick);
                        tankerCreeps.Add(creep);

                        break;

                    case "builder":
                        Console.WriteLine($"role: {role}");
                        creep.Memory.SetValue("Spawn", "Spawn1");
                        creep.Memory.SetValue("role", "builder");
                        creep.Memory.SetValue("respawnTick", this.respawnTick);
                        builderCreeps.Add(creep);
                        break;

                    case "repairer":
                        Console.WriteLine($"role: {role}");
                        creep.Memory.SetValue("Spawn", "Spawn1");
                        creep.Memory.SetValue("role", "repairer");
                        creep.Memory.SetValue("respawnTick", this.respawnTick);
                        builderCreeps.Add(creep);
                        break;


                    case "defender":
                        Console.WriteLine($"role: {role}");
                        creep.Memory.SetValue("Spawn", "Spawn1");
                        creep.Memory.SetValue("role", "defender");
                        creep.Memory.SetValue("respawnTick", this.respawnTick);
                        builderCreeps.Add(creep);
                        break;

                    case "worker":
                        Console.WriteLine($"role: {role}");
                        creep.Memory.SetValue("Spawn", "Spawn1");
                        creep.Memory.SetValue("role", "worker");
                        creep.Memory.SetValue("respawnTick", this.respawnTick);
                        workerCreeps.Add(creep);
                        break;

                    default:

                        assignNewRole(creep);

                        //Console.WriteLine($"role: {role}");
                        //creep.Memory.SetValue("Spawn", "Spawn1");
                        //creep.Memory.SetValue("role", "worker");
                        //creep.Memory.SetValue("respawnTick", this.respawnTick);
                        //workerCreeps.Add(creep);
                        break;
                }
                return;
                //}
            }

            assignNewRole(creep);

            void assignNewRole(ICreep creep)
            {
                if (room.Storage != null)
                {
                    if (minerCreepsSource1.Count >= 2)
                    {
                        if (tankerCreeps.Count < 1)
                        {
                            creep.Memory.SetValue("Spawn", "Spawn1");
                            creep.Memory.SetValue("respawnTick", this.respawnTick);
                            creep.Memory.SetValue("role", "tanker");

                            tankerCreeps.Add(creep);

                            return;
                        }
                    }
                }

                if (minerCreepsSource1.Count < targetMiner1Count)
                {
                    Console.WriteLine($"{this}: {creep} assigned as miner1");

                    creep.Memory.SetValue("Spawn", "Spawn1");
                    creep.Memory.SetValue("role", "miner1");
                    creep.Memory.SetValue("respawnTick", this.respawnTick);

                    minerCreepsSource1.Add(creep);
                    return;
                }

                if (minerCreepsSource2.Count < this.targetMiner2Count)
                {
                    Console.WriteLine($"{this}: {creep} assigned as miner2");
                    if (Spawn2 != null)
                    {
                        creep.Memory.SetValue("Spawn", "Spawn2");
                    }
                    else
                    {
                        creep.Memory.SetValue("Spawn", "Spawn1");

                    }


                    creep.Memory.SetValue("role", "miner2");
                    creep.Memory.SetValue("respawnTick", this.respawnTick);

                    minerCreepsSource2.Add(creep);
                    return;
                }

                if (tankerCreeps.Count < targetTankerCount)
                {
                    Console.WriteLine($"{this}: {creep} assigned as tanker");
                    creep.Memory.SetValue("Spawn", "Spawn1");
                    creep.Memory.SetValue("role", "tanker");
                    creep.Memory.SetValue("respawnTick", this.respawnTick);

                    tankerCreeps.Add(creep);
                    return;
                }

                if (builderCreeps.Count < targetBuilderCount)
                {
                    Console.WriteLine($"{this}: {creep} assigned as builder");
                    creep.Memory.SetValue("Spawn", "Spawn1");
                    creep.Memory.SetValue("role", "worker");
                    creep.Memory.SetValue("respawnTick", this.respawnTick);

                    builderCreeps.Add(creep);
                    return;
                }

                if (repairCreeps.Count < this.targetRepairerCount)
                {
                    Console.WriteLine($"{this}: {creep} assigned as repairer");

                    creep.Memory.SetValue("Spawn", "Spawn1");
                    creep.Memory.SetValue("role", "repairer");
                    creep.Memory.SetValue("respawnTick", this.respawnTick);

                    repairCreeps.Add(creep);
                    return;
                }



                // whatever is left is assigned to worker

                Console.WriteLine($"{this}: {creep} assigned as worker");
                creep.Memory.SetValue("Spawn", "Spawn1");
                creep.Memory.SetValue("role", "worker");
                creep.Memory.SetValue("respawnTick", this.respawnTick);

                workerCreeps.Add(creep);
                return;

            }
        }


        private void OnCreepDied(ICreep creep)
        {
            // Check the body type and remove it from all tracking lists
            //if (creep.BodyType == workerBodyType)
            //{


            minerCreepsSource1.Remove(creep);
            minerCreepsSource2.Remove(creep);
            workerCreeps.Remove(creep);
            tankerCreeps.Remove(creep);
            repairCreeps.Remove(creep);
            builderCreeps.Remove(creep);






            Console.WriteLine($"{this}: {creep} died");
            //
        }

        /// <summary>
        /// spawn new creeps
        /// </summary>
        /// <param name="spawn"></param>
        private void TickSpawn(IStructureSpawn spawn)
        {
            spawn = Spawn1;
            this.maxTotalCreeps = spawningManager.MaxCreepsCount;
            //if (spawn.Exists == false || spawn == null)
            //{
            //    return;
            //}
      



            // Check if we're able to spawn something, and spawn until we've filled our target role counts
            if (spawn.Spawning != null) { return; }
            BodyType<BodyPartType> creepBodyType;

            creepBodyType = simpleWorkerBodyType;


            if (room.Controller.Level >= 4 && room.EnergyCapacityAvailable >= 500)
            {
                creepBodyType = workerLevel4BodyType;
            }



            

            if (room.Controller.Level >= 6 && room.EnergyCapacityAvailable >= 550)
            {
                creepBodyType = medWorkerBodyType;


                if (minerCreepsSource2.Count < 1)
                {
                    creepBodyType = miner2BodyType;
                }
            }
            //else
            //{
            //    creepBodyType = simpleWorkerBodyType;

            //}





            myDebug.WriteLine($"minerCreepsSource2.Count: {minerCreepsSource2.Count}");

            if (Spawn2 != null)
            {
                if (minerCreepsSource2.Count < targetMiner2Count)
                {
                    creepBodyType = miner2BodyType;
                    var newCreep = new myCreepData();
                    newCreep.spawnName = FindUniqueCreepName();
                    newCreep.role = "miner2";
                    newCreep.spawnName = Spawn2.Name;
                    newCreep.bodyType = miner2BodyType;
                    //  spawnQueue.Enqueue(newCreep);

                    if (Spawn2.Exists)
                    {
                        spawn = Spawn2;

                    }
                }
            }



            // Console.WriteLine($"Spawn1 name is {Spawn1.Name}");
            //   Console.WriteLine($"Spawn2 name is {Spawn2.Name}");

            myDebug.WriteLine($"Active Spawn is {spawn.Name}");


            myDebug.WriteLine("right before if (spawnQueue.Count > 0)");
            if (spawnQueue.Count > 0)
            {
                myDebug.WriteLine("right after if (spawnQueue.Count > 0)");

                var screepData = spawnQueue.Peek();
                myDebug.WriteLine("var screepData = spawnQueue.Peek();");

                var name = FindUniqueCreepName();
                myDebug.WriteLine($"var name = FindUniqueCreepName() is {name}");

                //creepBodyType = screepData.bodyType;
                //myDebug.WriteLine($"screepData.bodyType is {creepBodyType}");

                var role = screepData.role;
                myDebug.WriteLine($"role: {role}");

                var spawnName = screepData.spawnName;
                Console.WriteLine($"spawnName: {spawnName}");



                if (role == "miner2")
                {
                    //  Console.WriteLine($"role is miner2");

                    if (spawns.Count > 1)
                    {
                        spawn = this.Spawn2;
                    }
                    else
                    {
                        spawn = this.Spawn1;

                    }
                    // var Spawn2x = game.Spawns.Values.ToArray()[1];
                    // Console.WriteLine($"Spawn2x.Name: {Spawn2x.Name}");
                    //  spawn = Spawn2x;
                    //var testSpawn = this.Spawn2;
                    // Console.WriteLine($"testSpawn: {testSpawn.Name}");
                }


                if (role == "miner1")

                {
                    //Miner1Level4BodyType
                    if (room.Controller.Level >= 4 && room.Storage !=null)
                    {
                        screepData.bodyType = Miner1Level4BodyType;
                        creepBodyType = Miner1Level4BodyType;
                    }



                    if (room.Controller.Level >= 7 && room.EnergyAvailable >= 700)
                    {
                        screepData.bodyType = miner1LargeBodyType;
                        creepBodyType = miner1LargeBodyType;
                    }
                    myDebug.WriteLine($"role is miner1");
                    // var testSpawn = this.Spawn1;
                    // Console.WriteLine($"testSpawn: {testSpawn.Name}");
                }


                if (role == "miner2")

                {


                    if (room.Controller.Level == 4 && room.EnergyAvailable >= 500)
                    {
                        creepBodyType = workerLevel4BodyType;
                    }


                    if (room.Controller.Level >=5 && room.EnergyAvailable >= 700 && room.Storage != null && source2Link !=null && controllerLink != null )
                    {
                        creepBodyType = miner2Level5TravelBodyType;
                    }


                    myDebug.WriteLine($"role is miner2");
                    if (room.Controller.Level >= 7 && room.EnergyAvailable >= 1000)
                    {
                        screepData.bodyType = miner2BodyType;
                        creepBodyType = miner2BodyType;
                        spawn = game.Spawns.Values.ToArray()[1];
                    }

                    // var testSpawn = this.Spawn1;
                    // Console.WriteLine($"testSpawn: {testSpawn.Name}");
                }

                //if (spawnName == "Spawn3")
                //{
                //    if (this.Spawn3.Exists && Spawn3 != null)
                //    {
                //         spawn = game.Spawns.Values.ToArray()[2];
                //    }
                //}

                myDebug.WriteLine($"right before spawning creep");
                myDebug.WriteLine($"1389 Active Spawn is {spawn.Name}");
                myDebug.WriteLine($"1390 creepBodyType: {creepBodyType}");
               
                if (spawn.SpawnCreep(creepBodyType, name, new(dryRun: true)) == SpawnCreepResult.Ok)
                {
                    myDebug.WriteLine($"1393 spawn.Name: {spawn.Name}");

                    myCreepData creepdata;
                    if (spawnQueue.Count > 0)
                    {
                        creepdata = spawnQueue.Dequeue();

                        myDebug.WriteLine($"1400 creepdata: {creepdata}");
                        var creepOptions = game.CreateMemoryObject();
                        //  var gameMemoryObject = game.CreateMemoryObject();
                        if (creepOptions != null)
                        {

                            creepOptions.SetValue("role", creepdata.role);
                            creepOptions.SetValue("spawn", creepdata.spawnName);
                            creepOptions.SetValue("respawnTick", creepdata.respawnTick);
                            SpawnCreepOptions spawnCreepOptions = new SpawnCreepOptions(memory: creepOptions);
                            spawn.SpawnCreep(creepBodyType, name, spawnCreepOptions);
                            myDebug.WriteLine($"spawning creep started");
                            creepdata = null;

                        }
                    }
                    else
                    {
                        
                        myDebug.WriteLine($"what is the creepBodyType: {creepBodyType}");
                        // spawn = Spawn1;
                        //  SpawnScreepOptions optionos = new
                        myDebug.WriteLine($"{this}: spawning a {creepBodyType} from {spawn}...");
                        spawn.SpawnCreep(creepBodyType, name);
                        myDebug.WriteLine($"spawning creep started");

                    }

                }

            }
            else if (allCreepsInRoom.Count < this.maxTotalCreeps)
            {
                
                var name = FindUniqueCreepName();
               // creepBodyType = miner1LargeBodyType;
                if (spawn.SpawnCreep(creepBodyType, name, new(dryRun: true)) == SpawnCreepResult.Ok)
                {
                    myDebug.WriteLine($"{this}: spawning a {creepBodyType} from {spawn}...");
                    //SpawnCreepOptions options = new SpawnCreepOptions();
                    //options.Memory.SetValue("fooTester", "FooTester1");

                    spawn.SpawnCreep(creepBodyType, name);
                }
            }
            // }
            //
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

                    EnergyWithDrawHandler(creep,this.Source1Container);
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
                if (this.Source1Container != null && this.Source1Container.Store.GetFreeCapacity () > 0 && source.Energy > 0)
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
                    myDebug.WriteLine("line 2099");


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


                if (!spawns.Any()) { return; }
                var spawn = spawns.Best(x => -x.LocalPosition.LinearDistanceTo(creep.LocalPosition));
                //var source2Flag = room.Find<IFlag>().Where(x => x.Name == "")
                myDebug.WriteLine(" *** if (!spawns.Any()) { return; }");

                if (spawn == null) { return; }





                // if miner is next to spawn true, transfer 
                if (Spawn2 != null)
                {
                    if (creep.RoomPosition.Position.IsNextTo(Spawn2.RoomPosition.Position))
                    {
                        if (Spawn2.Store.GetFreeCapacity(ResourceType.Energy) > 0)
                        {
                            transferResult = creep.Transfer(Spawn2, ResourceType.Energy);
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

        private void MoveToContainer(ICreep creep, IStructureContainer container) 
        {
            if (creep == null || container == null)
            {
                return;
            }

            // is creep already there?
            if (creep.RoomPosition.Position.Equals(container.RoomPosition.Position) == true)
            {
                return;  
            }

            // Is there another creep already there
            if (isThereACreepAtPosition(container.RoomPosition) == false)
            {
                creep.MoveTo(container.RoomPosition.Position);
                return;
            }

            
        }

        private bool isThereACreepAtPosition(RoomPosition roomPosition) 
        { 
        
            var pos = roomPosition.Position;
            foreach (var creep in allCreepsInRoom)
            {
                if (creep.RoomPosition.Position.Equals(pos))
                {
                    return true;
                }
            }
            return false;
        }


        private CreepTransferResult TransferEnergy<T>(ICreep creep, T targetStore) where T : IWithStore, IOwnedStructure, IWithId
        {
            var result = creep.Transfer(targetStore, ResourceType.Energy);
            if (result == CreepTransferResult.NotInRange)
            {
                creep.MoveTo(targetStore.RoomPosition);
            }
            return result;
        }

        
        private ISource? GetSource1()
        {
            if (this.Source1 != null)
            {
                return this.Source1;
            }


            //var source2Flag = allRoomFlags.Where(x => x.Name == "Source1_" + room.Name).First();
            //   myDebug.WriteLine($"xx source2Flag.Name: {source1Flag.Name}");
            //   //     myDebug.WriteLine($"xx source2Flag.Name: {source2Flag.Name}");
            //   this.Source1 = GetSourceByFlag(source1Flag);
            ////   this.Source1 = GetSourceAtFlag(source1Flag);
            var flagName = "Source1_" + room.Name;
            myDebug.WriteLine($"xx flagName Source1: {flagName}");

            this.Source1 = GetSource(flagName);
            return this.Source1;
        }
        private ISource? GetSource2()
        {
            if (this.Source2 != null)
            {
                return this.Source2;
            }

            //var source2Flag = allRoomFlags.Where(x => x.Name == "Source2_" + room.Name).First();
            //myDebug.WriteLine($"xx source2Flag.Name: {source2Flag.Name}");
            ////  myDebug.WriteLine($"xx source2Flag.Name: {source2Flag.Name}");

            //  var source = GetSourceAtFlag(source2Flag, sources);
            var flagName = "Source2_" + room.Name;
            myDebug.WriteLine($"xx flagName Source2: {flagName}");

            this.Source2 = GetSource(flagName);

            myDebug.WriteLine($"Source2: {Source2}");
            return this.Source2;
        }
        private ISource? GetSource(string FlagName)
        {
            var Flag = allRoomFlags.Where(x => x.Name == FlagName).First();
            return GetSource(Flag);

        }
        private ISource? GetSource(IFlag flag)
        {
            // var source2Flag = allRoomFlags.Where(x => x.Name == "Source2_" + room.Name).First();

            var source = this.sources.Where(x => x.RoomPosition.Position.Equals(flag.RoomPosition.Position)).First();
            //var source = GetSourceAtFlag(flag);
            return source;
        }

        private IStructureContainer? GetSource1Container()
        {
            if (this.Source1Container != null)
            {
                return this.Source1Container;
            }

            var flagName = "Source1_Container_" + room.Name;
            myDebug.WriteLine($" xxx Source1_Container_: {flagName}");

            this.Source1Container = GetContainer(flagName);

            myDebug.WriteLine($" xxx SourceContainer: {this.Source1Container}");
            return Source1Container;
        }
        private IStructureContainer? GetSource2Container()
        {
            if (this.Source2Container != null)
            {
                return this.Source2Container;
            }

            var flagName = "Source2_Container_" + room.Name;
            myDebug.WriteLine($"xx Source2_Container_: {flagName}");

            this.Source2Container = GetContainer(flagName);

            myDebug.WriteLine($"Source2Container: {this.Source2Container}");
            return Source2Container;
        }

        private IStructureContainer? GetContainer(string FlagName)
        {

            Console.WriteLine($" xxx allRoomFlags.Count: {allRoomFlags.Count}");
            //foreach (var xflag in allRoomFlags)
            //{

            //    Console.WriteLine($" xxx flag.Name: {xflag.Name}");
            //}
            //   var flags = allRoomFlags.ToHashSet();
            //   Console.WriteLine($" xxx flags.Count: {allRoomFlags.Count}");

            var flag = allRoomFlags.Where(x => x.Name == FlagName).First();
            //Console.WriteLine($" xxx Flag.Name: {flag.Name}"); 
            if (flag == null)
            {
                return null;
            }
            return GetContainer(flag);

        }
        private IStructureContainer? GetContainer(IFlag flag)
        {
            if (flag == null)
            {
                return null;
            }
            // var source2Flag = allRoomFlags.Where(x => x.Name == "Source2_" + room.Name).First();
            myDebug.WriteLine($" xxx flag.Name: {flag.Name}");
            myDebug.WriteLine($" xxx allRoomFlags.Count: {allRoomFlags.Count}");
            var container = this.containersInRoom.Where(x => x.RoomPosition.Position.Equals(flag.RoomPosition.Position)).First();
            myDebug.WriteLine($" xxx container: {container}");

            //var source = GetSourceAtFlag(flag);
            return container;
        }


        private CreepWithdrawResult WithdrawEnergy<T>(ICreep creep, T targetStore) where T : IWithStore, IOwnedStructure, IWithId
        { 

            var result = creep.Withdraw(targetStore, ResourceType.Energy);
            if (result == CreepWithdrawResult.NotInRange)
            {
                creep.MoveTo(targetStore.RoomPosition);
            }
            return result;
        }
        private void EnergyWithDrawHandler(ICreep creep,IStructureContainer container)
        {
            myDebug.WriteLine("START --------- EnergyWithDrawHandler()");
            if (container == null)
            {
                myDebug.WriteLine("container is null. Exiting EnergyWithDrawHandler()");
                return;
            }

            if (container == null) 
            {
                return;
            }

           // if (container != null)
            //{
                // harverst if source has energy
                if (container.Store.GetUsedCapacity() > 0)
                {
                    // var results = WithdrawEnergy(creep, this.Source1Container);
                    if (container.Store.GetUsedCapacity() > 0)
                    {
                        myDebug.WriteLine(" 2417 IN --------- EnergyWithDrawHandler()");

                        if (WithdrawEnergy(creep, container) == CreepWithdrawResult.NotInRange)
                        { return; }

                    }
                }
            //}

            // find any container that creep is next to creep
            myDebug.WriteLine(" 2427 IN --------- find any container that creep is next to creep");
            var otherContainers = containersInRoom.Where(x => x.RoomPosition.Position.IsNextTo(creep.RoomPosition.Position) && x.Store.GetUsedCapacity() > 0);
            myDebug.WriteLine(" 2429 OUT --------- find any container that creep is next to creep");

            if (otherContainers != null && otherContainers.Count() > 0)
            {
                myDebug.WriteLine($" 2433 --------- IsNextToContainer = otherContainers.First();  otherContainers.Count(): { otherContainers.Count()}");

                var IsNextToContainer = otherContainers.First();
                var results = WithdrawEnergy(creep, IsNextToContainer);
            }

            myDebug.WriteLine(" EXIT --------- EnergyWithDrawHandler");
        }
        private CreepHarvestResult HarvestEnergy(ICreep creep, ISource source)
        {
            //ISource source = Source1;
            if (source.Energy == 0)
            {
                creep.MoveTo(source.RoomPosition);
                return CreepHarvestResult.NotEnoughResources;
            }


            var harvestResult = creep.Harvest(source);
            if (harvestResult == CreepHarvestResult.NotInRange)
            {
                creep.MoveTo(source.RoomPosition);
            }
            else if (harvestResult != CreepHarvestResult.Ok)
            {

                return harvestResult;

            }
            return harvestResult;
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


            Console.WriteLine($"this.source1Link != null: {this.source1Link != null}");
            Console.WriteLine($"this.enableSource1Link != null: {this.enableSource1Link != null}");
            Console.WriteLine($"this.controllerLink != null: {this.controllerLink != null}");


            ///////////////////////////////////////////////
            // Transfer from Source1 Link to controller link
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

        private IStructureLink? GetSource1Link(IEnumerable<IStructure> allStructures)
        {


            //Source2_Link_W1N7
            var flagName = $"Source1_Link_{room.Name}";
            myDebug.WriteLine($" xxxx flagName is {flagName}");

            var source1LinkFlags = allRoomFlags.Where(x => x.Name == flagName);
            myDebug.WriteLine($" xxxx source1LinkFlags.Count is {source1LinkFlags.Count()}");




            if (source1LinkFlags.Count() > 0)
            {
                Console.WriteLine($" xxxx source1LinkFlags is {source1LinkFlags}");

                source1LinkFlag = source1LinkFlags.First();

                    Console.WriteLine($" xxxx source1LinkFlag is {source1LinkFlag}");
                    Console.WriteLine($" xxxx source1LinkFlag.RoomPosition.Position: {source1LinkFlag.RoomPosition.Position}");
                //   IStructure? link = game.GetObjectById <IStructure>("88bcc550827613c");
                //  Console.WriteLine($" *** link.RoomPosition.Position: {link.RoomPosition.Position}");

                var link = GetStructureAtFlag(source1LinkFlag, allStructures);
                Console.WriteLine($" *** link is {link}");
                if (link != null)
                {
                    return (IStructureLink)link;

                }
            }

            //        (IStructureLink)link;
            //    var source2Links = room.Find<IStructureLink>().Where(x => x.RoomPosition.Position.Equals(source2LinkFlag.RoomPosition.Position));
            //    Console.WriteLine($" *** source2Links.Count is {source2Links.Count()}");

            //    if (source2Links.Count() > 0)
            //    {
            //        var source2Link = source2Links.First();
            //        Console.WriteLine($" *** source2Link is {source2Link}");

            //        if (source2Link != null)
            //        {
            //            Console.WriteLine($" *** returning source2Link {source2Link}");
            //            return source2Link;
            //        }

            //    }
            //}



            return null;

        }

        private IStructureLink? xGetSource1Link(IEnumerable<IStructure> allStructures)
        {

            var flagName = $"Source1_Link_{room.Name}";
            myDebug.WriteLine($" *** flagName is {flagName}");

            var source1LinkFlags = allRoomFlags.Where(x => x.Name == flagName);
            myDebug.WriteLine($" *** source1LinkFlags.Count is {source1LinkFlags.Count()}");


            if (source1LinkFlags.Count() > 0)
            {
                source1LinkFlag = source1LinkFlags.First();
                //   source1LinkFlag = source1LinkFlags.Where(x => x.Name == "Source1_Link_" + room.Name).First();

                myDebug.WriteLine($" *** source1LinkFlag is {source1LinkFlag}");
                myDebug.WriteLine($" *** source1LinkFlag.RoomPosition.Position: {source1LinkFlag.RoomPosition.Position}");
                //   IStructure? link = game.GetObjectById <IStructure>("88bcc550827613c");
                //  Console.WriteLine($" *** link.RoomPosition.Position: {link.RoomPosition.Position}");

                var link = GetStructureAtFlag(source1LinkFlag, allStructures);
                myDebug.WriteLine($" *** link is {link}");
                myDebug.WriteLine($" *** link is {link}");
                myDebug.WriteLine($" *** link is {link}");
                myDebug.WriteLine($" *** link is {link}");
                myDebug.WriteLine($" *** link is {link}");
                myDebug.WriteLine($" *** link is {link}");
                myDebug.WriteLine($" *** link is {link}");
                if (link != null)
                {
                    return (IStructureLink)link;

                }
            }

            //        (IStructureLink)link;
            //    var source2Links = room.Find<IStructureLink>().Where(x => x.RoomPosition.Position.Equals(source2LinkFlag.RoomPosition.Position));
            //    Console.WriteLine($" *** source2Links.Count is {source2Links.Count()}");

            //    if (source2Links.Count() > 0)
            //    {
            //        var source2Link = source2Links.First();
            //        Console.WriteLine($" *** source2Link is {source2Link}");

            //        if (source2Link != null)
            //        {
            //            Console.WriteLine($" *** returning source2Link {source2Link}");
            //            return source2Link;
            //        }

            //    }
            //}

            return null;

        }

        private IStructureLink? GetSource2Link(IEnumerable<IStructure> allStructures)
        {



            var flagName = $"Source2_Link_{room.Name}";
            myDebug.WriteLine($" *** flagName is {flagName}");

            var source2LinkFlags = allRoomFlags.Where(x => x.Name == flagName);
            myDebug.WriteLine($" *** source2LinkFlags.Count is {source2LinkFlags.Count()}");


            if (source2LinkFlags.Count() > 0)
            {
                source2LinkFlag = source2LinkFlags.First();

                //    Console.WriteLine($" *** source2LinkFlag is {source2LinkFlag}");
                //      Console.WriteLine($" *** source2LinkFlag.RoomPosition.Position: {source2LinkFlag.RoomPosition.Position}");
                //   IStructure? link = game.GetObjectById <IStructure>("88bcc550827613c");
                //  Console.WriteLine($" *** link.RoomPosition.Position: {link.RoomPosition.Position}");

                var link = GetStructureAtFlag(source2LinkFlag, allStructures);
                Console.WriteLine($" *** link is {link}");
                if (link != null)
                {
                    return (IStructureLink)link;

                }
            }

            //        (IStructureLink)link;fghfg
            //    var source2Links = room.Find<IStructureLink>().Where(x => x.RoomPosition.Position.Equals(source2LinkFlag.RoomPosition.Position));
            //    Console.WriteLine($" *** source2Links.Count is {source2Links.Count()}");

            //    if (source2Links.Count() > 0)
            //    {
            //        var source2Link = source2Links.First();
            //        Console.WriteLine($" *** source2Link is {source2Link}");

            //        if (source2Link != null)
            //        {
            //            Console.WriteLine($" *** returning source2Link {source2Link}");
            //            return source2Link;
            //        }

            //    }
            //}



            return null;

        }

        private IStructureLink? GetControllerLink(IEnumerable<IStructure> allStructures)

        {

            var flagName = $"Controller_Link_{room.Name}";
            Console.WriteLine($" *** flagName is {flagName}");

            var controllerLinkFlags = allRoomFlags.Where(x => x.Name == flagName);
            Console.WriteLine($" *** controllerLinkFlags.Count is {controllerLinkFlags.Count()}");
            if (controllerLinkFlags.Count() == 0)
            {
                return null;
            }



            if (controllerLinkFlags.Count() > 0)
            {
                controllerLinkFlag = controllerLinkFlags.First();
                if (controllerLinkFlag == null)
                {
                    return null;
                }

                Console.WriteLine($" *** controllerLinkFlags is {controllerLinkFlags}");
                Console.WriteLine($" *** controllerLinkFlag.RoomPosition.Position: {controllerLinkFlag.RoomPosition.Position}");
                //   IStructure? link = game.GetObjectById <IStructure>("88bcc550827613c");
                //  Console.WriteLine($" *** link.RoomPosition.Position: {link.RoomPosition.Position}");

                var link = GetStructureAtFlag(controllerLinkFlag, allStructures);
                Console.WriteLine($" *** link is {link}");
                try
                {
                    if (link != null)
                    {
                        return (IStructureLink)link;

                    }
                }
                catch (Exception)
                {

                    return null;
                }

            }

            //        (IStructureLink)link;
            //    var source2Links = room.Find<IStructureLink>().Where(x => x.RoomPosition.Position.Equals(source2LinkFlag.RoomPosition.Position));
            //    Console.WriteLine($" *** source2Links.Count is {source2Links.Count()}");

            //    if (source2Links.Count() > 0)
            //    {
            //        var source2Link = source2Links.First();
            //        Console.WriteLine($" *** source2Link is {source2Link}");

            //        if (source2Link != null)
            //        {
            //            Console.WriteLine($" *** returning source2Link {source2Link}");
            //            return source2Link;
            //        }

            //    }
            //}



            return null;

        }

        private IStructureLink? GetStorageLink(IEnumerable<IStructure> allStructures)

        {


            //Source2_Link_W1N7
            var flagName = $"Storage_Link_{room.Name}";
            //  Console.WriteLine($" *** flagName is {flagName}");

            var storageLinkFlags = allRoomFlags.Where(x => x.Name == flagName);
            Console.WriteLine($" *** storageLinkFlags.Count is {storageLinkFlags.Count()}");
            if (storageLinkFlags.Count() == 0)
            {
                return null;
            }



            if (storageLinkFlags.Count() > 0)
            {
                storageLinkFlag = storageLinkFlags.First();
                if (storageLinkFlag == null)
                {
                    return null;
                }

                Console.WriteLine($" *** storageLinkFlag is {storageLinkFlag}");
                Console.WriteLine($" *** storageLinkFlag.RoomPosition.Position: {storageLinkFlag.RoomPosition.Position}");

                //  IStructure? link = game.GetObjectById <IStructure>("88bcc550827613c");
                //  Console.WriteLine($" *** link.RoomPosition.Position: {link.RoomPosition.Position}");

                var link = GetStructureAtFlag(storageLinkFlag, allStructures);
                Console.WriteLine($" *** link is {link}");
                if (link != null)
                {
                    return (IStructureLink)link;

                }
            }

            //        (IStructureLink)link;
            //    var source2Links = room.Find<IStructureLink>().Where(x => x.RoomPosition.Position.Equals(source2LinkFlag.RoomPosition.Position));
            //    Console.WriteLine($" *** source2Links.Count is {source2Links.Count()}");

            //    if (source2Links.Count() > 0)
            //    {
            //        var source2Link = source2Links.First();
            //        Console.WriteLine($" *** source2Link is {source2Link}");

            //        if (source2Link != null)
            //        {
            //            Console.WriteLine($" *** returning source2Link {source2Link}");
            //            return source2Link;
            //        }

            //    }
            //}



            return null;

        }


      //  private ISource? GetSourceAtFlag(IFlag flag, ISet<ISource> allSourcesInRoom)
        private ISource? GetSourceAtFlag(IFlag flag)
        {
            //  source2LinkFlag = room.Find<IFlag>().Where(x => x.Name == $"Source1_Link_{room.Name}");
            myDebug.WriteLine($"flag.Name: {flag.Name}");

            if (flag != null)
            {


              //  Console.WriteLine($"sources.Count(): {sources.Count()}");

               //var source = allSourcesInRoom.Where(x => x.RoomPosition.Position.Equals(flag.RoomPosition.Position)).ToHashSet().First();
                var source = this.sources.Where(x => x.RoomPosition.Position.Equals(flag.RoomPosition.Position)).First();
                
                if (source.Exists)
                {
                    return source;
                }
                ///Console.WriteLine($" ***! No source found");
               
                //Console.WriteLine($" ***! structures.Count is {structures.Count()}");

                //if (sources.Count() > 0)
                //{
                //    var source = sources.First();
                //    if (source != null)
                //    {
                //        //    Console.WriteLine($" ***! returning structure {structure}");

                //        return source;
                //    }

                //}
            }

            return null;

        }


        private IStructure? GetStructureAtFlag(IFlag flag, IEnumerable<IStructure> allStructures)
        {
            //  source2LinkFlag = room.Find<IFlag>().Where(x => x.Name == $"Source1_Link_{room.Name}");

            if (flag != null)
            {
                var structures = allStructures.Where(x => x.RoomPosition.Position.Equals(flag.RoomPosition.Position));
                //Console.WriteLine($" ***! structures.Count is {structures.Count()}");

                if (structures.Count() > 0)
                {
                    var structure = structures.First();
                    if (structure != null)
                    {
                        //    Console.WriteLine($" ***! returning structure {structure}");

                        return structure;
                    }

                }
            }

            return null;

        }

        private void PickupEnergy(ICreep creep)
        {


            if (!creep.Exists)
            {
                return;
            }


            bool disablePickupEnergy;
            var status = Spawn1.Memory.TryGetBool("disablePickupEnergy", out disablePickupEnergy);

            if (!status || disablePickupEnergy == true)
            {
                return;
            }



            if (creep.Store.GetFreeCapacity(ResourceType.Energy) > 0)
            {

                // IResource resource;
                // var resources = creep.Room.Find<IResource>().Where(X => X.RoomPosition.Equals(creep.RoomPosition));
                // var tombstones = creep.Room.Find<ITombstone>().Where(X => X.RoomPosition.Equals(creep.RoomPosition));
                var resources = creep.Room.Find<IResource>().Where(X => X.RoomPosition.Position.IsNextTo(creep.RoomPosition.Position));
                var tombstones = creep.Room.Find<ITombstone>().Where(X => X.RoomPosition.Position.IsNextTo(creep.RoomPosition.Position));



                if (resources.Count() > 0)
                {
                    var resource = resources.First();
                    creep.Pickup(resource);



                }
                else if (tombstones.Count() > 0)
                {
                    var tombstone = tombstones.First();
                    creep.Withdraw(tombstone, ResourceType.Energy);
                }

            }
        }

     

        private void TickWorker(ICreep creep)
        {

            var status = Spawn1.Memory.TryGetBool("pauseWorkers", out var pause);

            myDebug.WriteLine($"status: {status}");
            myDebug.WriteLine($"pause: {pause}");

            if (status)
            {
                if (pause ==true)
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
                if (room.Controller.Level <=6 && spawnQueue.Count > 0)
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
            if (demagedCreepsInRoom.Count >0)
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

        private void FillTowers(ICreep creep)
        {
            if (!creep.Exists)
            {
                return;
            }

            // Check energy storage
            if (creep.Store[ResourceType.Energy] > 0)
            {
                // There is energy to drop off
                //Console.WriteLine("Starting extensionTargets(ICreep creep)");
                //Console.WriteLine("extensionTargets.Count: " + extensionTargets.Count);

                // Console.WriteLine("        extensionTargets.ToString(): " + extensionTargets.ToString());
                // Console.WriteLine("        extensionTargets.ToArray().ToString(): " + extensionTargets.ToArray().ToString());


                if (towerTargets.Count == 0)
                {
                    Console.WriteLine("   All towers are full");
                    return;
                }


                IStructureTower towerTarget = towerTargets.Best(x => -x.LocalPosition.LinearDistanceTo(creep.LocalPosition));
                // IStructureExtension extensionTarget = extensionTargets.First();


                //IStructureExtension extensionTarget = extensionTargets.First();
                //Console.WriteLine("        extensionTargets.ToString(): " + extensionTargets.ToString());


                var transferResults = creep.Transfer(towerTarget, ResourceType.Energy);

                // Console.WriteLine("        transferResults: " + transferResults);

                if (transferResults == CreepTransferResult.NotInRange)
                {
                    creep.MoveTo(towerTarget.RoomPosition);
                }
                else if (transferResults != CreepTransferResult.Ok)
                {
                    // Console.WriteLine($"{this}: {creep} unexpected result when transfering energy to {towerTarget} ({transferResults})");
                }
            }
            else
            {

                getEnergy(creep);
            }
        }

        private void FillExtentions(ICreep creep)
        {
            if (!creep.Exists)
            {
                return;
            }

            // Check energy storage
            if (creep.Store[ResourceType.Energy] > 0)
            {
                // There is energy to drop off
                //Console.WriteLine("Starting extensionTargets(ICreep creep)");
                //Console.WriteLine("extensionTargets.Count: " + extensionTargets.Count);

                // Console.WriteLine("        extensionTargets.ToString(): " + extensionTargets.ToString());
                // Console.WriteLine("        extensionTargets.ToArray().ToString(): " + extensionTargets.ToArray().ToString());

                //  IStructureExtension extensionTarget = extensionTargets.Best(x => -x.LocalPosition.LinearDistanceTo(creep.LocalPosition));
                
                IStructureExtension extensionTarget = extensionTargets.Best(x => -x.LocalPosition.LinearDistanceTo(creep.LocalPosition));
                if (extensionTarget.Exists == false && extensionTarget != null)
                {
                    return;
                }

                //  if (extensionTarget.Store.GetUsedCapacity(ResourceType.Energy) == extensionTarget.Store.GetCapacity(ResourceType.Energy))
                if (extensionTarget.Store.GetFreeCapacity(ResourceType.Energy) == 0)
                {
                    extensionTargets.Remove(extensionTarget);
                    Console.WriteLine("extensionTarget is full ");
                    return;
                }

                // Console.WriteLine($"{this}: {creep} Target {extensionTarget} energy ({extensionTarget.Store.GetUsedCapacity(ResourceType.Energy)})");

                // IStructureExtension extensionTarget = extensionTargets.First();


                //IStructureExtension extensionTarget = extensionTargets.First();
                //Console.WriteLine("        extensionTargets.ToString(): " + extensionTargets.ToString());


                var transferResults = creep.Transfer(extensionTarget, ResourceType.Energy);

               //  Console.WriteLine("        transferResults: " + transferResults);

                if (transferResults == CreepTransferResult.NotInRange)
                {
                    creep.MoveTo(extensionTarget.RoomPosition);
                }
                else if (transferResults != CreepTransferResult.Ok)
                {
                    //  Console.WriteLine($"{this}: {creep} 1057 unexpected result when transfering energy to {extensionTarget} ({transferResults})");
                    //   Console.WriteLine($"{this}: {creep} 1057 unexpected result when transfering energy to {extensionTarget} ({transferResults})");
                }
            }
            else
            {

                getEnergy(creep);

                //// We're empty, go to pick up
                //if (!spawns.Any()) { return; }
                //var spawn = spawns.Best(x => -x.LocalPosition.LinearDistanceTo(creep.LocalPosition));
                //if (spawn == null) { return; }

                //CreepWithdrawResult withdrawResult;

                //if (room.Storage.Exists)
                //{

                //    withdrawResult = creep.Withdraw(room.Storage, ResourceType.Energy);
                //    if (withdrawResult == CreepWithdrawResult.NotInRange)
                //    {
                //        creep.MoveTo(room.Storage.RoomPosition);
                //    }
                //    else if (withdrawResult != CreepWithdrawResult.Ok)
                //    {
                //        Console.WriteLine($"{this}: {creep} unexpected result when withdrawing from {room.Storage} ({withdrawResult})");
                //    }

                //}
                //else
                //{


                //    withdrawResult = creep.Withdraw(spawn, ResourceType.Energy);
                //    if (withdrawResult == CreepWithdrawResult.NotInRange)
                //    {
                //        creep.MoveTo(spawn.RoomPosition);
                //    }
                //    else if (withdrawResult != CreepWithdrawResult.Ok)
                //    {
                //        Console.WriteLine($"{this}: {creep} unexpected result when withdrawing from {spawn} ({withdrawResult})");
                //    }
                //}
            }
        }

        private void getEnergy(ICreep creep)
        {

            // Console.WriteLine("getting energy");

            // We're empty, go to pick up
            if (!creep.Exists)
            {
                return;
            }

            //   PickupEnergy(creep);

            //  TickBuilder(creep);
            // We're empty, go to pick up
            if (!spawns.Any()) { return; }
            var spawn = spawns.Best(x => -x.LocalPosition.LinearDistanceTo(creep.LocalPosition));
            if (spawn == null) { return; }

            CreepWithdrawResult withdrawResult;

            if (this.source2Link != null)
            {
                if (creep.RoomPosition.Position.LinearDistanceTo(source2Link.RoomPosition.Position) <= 7)
                {
                    if (source2Link.Store.GetUsedCapacity(ResourceType.Energy) > 0 )
                    {
                        withdrawResult = creep.Withdraw(source2Link, ResourceType.Energy);
                        if (withdrawResult == CreepWithdrawResult.NotInRange)
                        {
                           var moveStatus = creep.MoveTo(source2Link.RoomPosition);
                        }
                        else if (withdrawResult != CreepWithdrawResult.Ok)
                        {
                            //  Console.WriteLine($"{this}: {creep} unexpected result when withdrawing from {room.Storage} ({withdrawResult})");
                        }
                    }
                }
            }


            if (this.source1Link != null)
            {
                if (this.source1Link.Store.GetUsedCapacity(ResourceType.Energy) >= 100)
                {
                    withdrawResult = creep.Withdraw(this.source1Link, ResourceType.Energy);
                    if (withdrawResult == CreepWithdrawResult.NotInRange)
                    {
                        creep.MoveTo(this.source1Link.RoomPosition);
                    }
                    else if (withdrawResult != CreepWithdrawResult.Ok)
                    {
                        //  Console.WriteLine($"{this}: {creep} unexpected result when withdrawing from {room.Storage} ({withdrawResult})");
                    }
                    return;
                }
            }


            if (room.Storage != null)
            {
                withdrawResult = creep.Withdraw(room.Storage, ResourceType.Energy);
                if (withdrawResult == CreepWithdrawResult.NotInRange)
                {
                    creep.MoveTo(room.Storage.RoomPosition);
                }
                else if (withdrawResult != CreepWithdrawResult.Ok)
                {
                    //  Console.WriteLine($"{this}: {creep} unexpected result when withdrawing from {room.Storage} ({withdrawResult})");
                }
            }
            else
            {
                withdrawResult = creep.Withdraw(spawn, ResourceType.Energy);
                if (withdrawResult == CreepWithdrawResult.NotInRange)
                {
                    creep.MoveTo(spawn.RoomPosition);
                }
                else if (withdrawResult != CreepWithdrawResult.Ok)
                {
                    //  Console.WriteLine($"{this}: {creep} unexpected result when withdrawing from {spawn} ({withdrawResult})");
                }
            }
        }

        private void roomText(string text, int x,int y)
        {
            // var roomPos = new RoomPosition()
            
            //  var myColor = new Color(255, 255, 255);
            var mycolor = Color.FromNorm(0, 255, 255);
            TextVisualStyle textVisualStyle = new(align: TextAlign.Left,color: Color.Green, stroke: Color.Green, strokeWidth: 0.0, opacity: 0.5, font: "0.85 Courier New");
        // TextVisualStyle textVisualStyle = new TextVisualStyle(align: TextAlign.Left, color: mycolor, font: "0.85 Courier New"); 
            //TextVisualStyle textVisualStyle = new TextVisualStyle(align: TextAlign.Left,backgroundColor: Color.Green, color: Color.White);

            FractionalPosition fractionalPosition = new FractionalPosition(x, y);
            // var outText = $"<font color = \"yellow\">{text}</>"; 
            var outText = text;
            //   roomVisual.Text("Test", fractionalPosition);
            roomVisual.Text(outText, fractionalPosition, textVisualStyle);

            //   roomVisual.Text($"Bucket2: {game.Cpu.Bucket}", new FractionalPosition(3, 11));
            
        }

        private string FindUniqueCreepName()
            => $"{room.Name}_{rng.Next()}";


        //private void HarvestEnergy(API.World.ICreep creep)
        //{
        //    // Find adjacent energy sources (dropped energy and tombstones)
        //    var adjacentSources = creep.RoomPosition.Find (Find.DROPPED_RESOURCES, 1)
        //        .Concat(creep.RoomPosition.FindInRange(Find.TOMBSTONES, 1, tombstone => tombstone.Store[ResourceType.Energy] > 0));

        //    if (adjacentSources.Any())
        //    {
        //        // Choose the first adjacent source
        //        var source = adjacentSources.First();

        //        // If it's a dropped resource, pick it up
        //        if (source is IResource droppedResource)
        //        {
        //            if (creep.Pickup(droppedResource) == CreepHarvestResult.NotInRange)
        //            {
        //                creep.MoveTo(droppedResource);
        //            }
        //        }
        //        // If it's a tombstone with energy, withdraw from it
        //        else if (source is Tombstone tombstone)
        //        {
        //            if (creep.Withdraw(tombstone, ResourceType.Energy) == ResponseCode.ERR_NOT_IN_RANGE)
        //            {
        //                creep.MoveTo(tombstone);
        //            }
        //        }
        //    }
        //}



        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SyncObjects<T>(IEnumerable<T> incomingObjectList, HashSet<T> trackedObjectSet, Action<T> onNowExistsEvent, Action<T> onNoLongerExistsEvent, ref long lastSyncTime)
        {
            var incomingObjectSet = new HashSet<T>(incomingObjectList);
            List<T>? pendingRemoveList = null;
            foreach (var roomObject in trackedObjectSet)
            {
                if (!incomingObjectSet.Contains(roomObject))
                {
                    (pendingRemoveList ??= new List<T>()).Add(roomObject);
                }
            }
            if (pendingRemoveList != null)
            {
                foreach (var roomObject in pendingRemoveList)
                {
                    trackedObjectSet.Remove(roomObject);
                    onNoLongerExistsEvent(roomObject);
                }
            }
            foreach (var roomObject in incomingObjectSet)
            {
                if (trackedObjectSet.Add(roomObject))
                {
                    onNowExistsEvent(roomObject);
                }
            }
            lastSyncTime = game.Time;
        }


        public override string ToString()
                => $"RoomManager[{room.Name}]";

        private void TTLCountDown(ICreep creep, int TTL)
        {
            if (creep.TicksToLive < TTL)
            {
                creep.Say(creep.TicksToLive.ToString());
            }


        }

        private void CleanMemory()
        {
            if (!game.Memory.TryGetObject("creeps", out var creepsObj)) { return; }
            int clearCnt = 0;
            foreach (var creepName in creepsObj.Keys)
            {
                if (!game.Creeps.ContainsKey(creepName))
                {
                    creepsObj.ClearValue(creepName);
                    ++clearCnt;
                }
            }
            if (clearCnt > 0) { Console.WriteLine($"Cleared {clearCnt} dead creeps from memory"); }
        }
       
        public void overlay()
        {
           
        }

        public static class myDebug
        {
            public static bool debug { get; set; }
            // private const string yellow = $"<font color = \"yellow\">DEBUG: {text}</>";
            private const string color = "yellow";
            public static void WriteLine(string msg) 
            { 
              if (debug)
                {
                    Console.WriteLine($"<font color = \"{color}\">DEBUG: {msg}</>");
                } 
            } 



            public static void WriteLine(bool on, string msg)
            {
                if (on)
                {
                    Console.WriteLine(msg);
                }
            }
        }

        public class SpawningManager
        {
            public int targetMiner1Count { get; set; }
            public int targetMiner2Count { get; set; }
            public int targetBuilderCount { get; set; }
            public int targetWorkerCount { get; set; }
            public int targetTankerCount { get; set; }
            public int targetRepairerCount { get; set; }
            private IStructureSpawn spawn;

            public SpawningManager(IStructureSpawn _spawn)
            {
                spawn = _spawn;
                GetSpawnMemory();
            }

            public void SetSpawnMemory()
            {

                spawn.Memory.SetValue("targetMiner1Count", targetMiner1Count);
                spawn.Memory.SetValue("targetMiner2Count", targetMiner2Count);
                spawn.Memory.SetValue("targetBuilderCount", targetBuilderCount);
                spawn.Memory.SetValue("targetWorkerCount", targetWorkerCount);
                spawn.Memory.SetValue("targetTankerCount", targetTankerCount);
                spawn.Memory.SetValue("targetRepairerCount", targetRepairerCount);
            }

            public void GetSpawnMemory()
            {
                bool status;
                
                status = spawn.Memory.TryGetInt("targetMiner1Count", out var targetCount);
                if (status)
                {
                    this.targetMiner1Count = targetCount;
                }
            
                spawn.Memory.TryGetInt("targetMiner2Count", out var _targetMiner2Count);
                if (status)
                {
                    this.targetMiner2Count = _targetMiner2Count;
                }

                spawn.Memory.TryGetInt("targetBuilderCount", out var _targetBuilderCount);
                if (status)
                {
                    this.targetBuilderCount = _targetBuilderCount;
                }


                spawn.Memory.TryGetInt("targetWorkerCount", out var _targetWorkerCount);
                if (status)
                {
                    this.targetWorkerCount = _targetWorkerCount;
                }

                spawn.Memory.TryGetInt("targetTankerCount", out var _targetTankerCount);
                if (status)
                {
                    targetTankerCount = _targetTankerCount;
                }

                spawn.Memory.TryGetInt("targetRepairerCount", out var _targetRepairerCount);
                if (status)
                {
                    this.targetRepairerCount = _targetRepairerCount;
                }

                //spawn.Memory.SetValue("targetBuilderCount", builderCount);
                //spawn.Memory.SetValue("targetWorkerCount", workerCount);
                //spawn.Memory.SetValue("targetTankerCount", tankerCount);
                //spawn.Memory.SetValue("targetRepairerCount", repairerCount);
            }



            public int MaxCreepsCount
            {
                get
                {
                    return CalculateTotal();
                }
            }

            private int CalculateTotal()
            {
                    return   targetMiner1Count + targetMiner2Count + targetBuilderCount + targetTankerCount + targetWorkerCount + targetRepairerCount;
            }

            public  void ToConsole()
            {

                Console.WriteLine($"   miner1Count: {targetMiner1Count}");
                Console.WriteLine($"   miner2Count: {targetMiner2Count}");
                Console.WriteLine($"  builderCount: {targetBuilderCount}");
                Console.WriteLine($"   tankerCount: {targetTankerCount}");
                Console.WriteLine($"   workerCount: {targetWorkerCount}");
                Console.WriteLine($" repairerCount: {targetRepairerCount}");
                Console.WriteLine($"MaxCreepsCount: {MaxCreepsCount}");
            }



        }
      



    }

    public class myCreepData
    {
        public string name;
        public string spawnName;
        public string respawnTick;
        public BodyType<BodyPartType> bodyType { get; set; }
        public string role;

        public myCreepData() { }

        public myCreepData(ICreep? creep)
        {
            if(creep != null)
            {
                var status = creep.Memory.TryGetString("role", out this.role);
                var status2 = creep.Memory.TryGetString("Spawn", out this.spawnName);
                //var status3 = creep.Memory.TryGetString("respawnTick", out this.respawnTick);
                bodyType = creep.BodyType;
            }
        }

    }



    //function overlayInfo(spawn)
    //{
    //    new RoomVisual(spawn.room.name).text(spawn.room.storage.store[RESOURCE_ENERGY], 2, 2, { color: 'green', font: 0.8 });
    //    new RoomVisual(spawn.room.name).text(spawn.room.name, 2, 3, { color: 'green', font: 0.8 });
    //}


    // public class CustomList<T> : List<T>;
    public interface IMyCreep : ICreep
    {

        int fooInt { get; set; }

        public void pickupEnergy()
        {
            fooInt = 0;
        }

    };


    public class CpuUseageHistory<T>
    {
        private Queue<T> queue = new Queue<T>();
        private int limit;

        public CpuUseageHistory(int limit)
        {
            if (limit <= 0)
            {
                throw new ArgumentException("Limit must be a positive number.");
            }
            this.limit = limit;
        }

        public int Count
        {
            get { return queue.Count; }
        }

        public void Enqueue(T item)
        {
            if (queue.Count >= limit)
            {
                // Remove the oldest item if the limit is reached
                queue.Dequeue();
            }
            queue.Enqueue(item);
        }

        public T Dequeue()
        {
            if (queue.Count == 0)
            {
                throw new InvalidOperationException("Queue is empty.");
            }
            return queue.Dequeue();
        }

        public double CalculateAverage()
        {
            if (queue.Count == 0)
            {
                return 0; // Avoid division by zero
            }

            return queue.Average(item => Convert.ToDouble(item));
        }
    }



  


public class GridUtility
    {
        public static bool IsPointInSubsection(Point startPoint, Point endPoint, Point checkPoint)
        {

            var grid = new int [50,50];

            // Check for valid input coordinates
            if (startPoint.X < 0 || startPoint.Y < 0 || endPoint.X >= grid.GetLength(0) || endPoint.Y >= grid.GetLength(1))
            {
                throw new ArgumentException("Invalid starting coordinates");
            }

            // Calculate width and height
            int width = endPoint.X - startPoint.X + 1;
            int height = endPoint.Y - startPoint.Y + 1;

            // Check if the point is within the subsection
            bool isWithinSubsection = checkPoint.X >= startPoint.X && checkPoint.X <= endPoint.X &&
                                      checkPoint.Y >= startPoint.Y && checkPoint.Y <= endPoint.Y;

            return isWithinSubsection;
        }
    }



    //class Program
    //{
    //    static void Main()
    //    {
    //        // Create a limited queue with a limit of 100
    //        LimitedQueue<int> queue = new LimitedQueue<int>(100);

    //        // Enqueue elements into the limited queue
    //        for (int i = 1; i <= 150; i++)
    //        {
    //            queue.Enqueue(i);
    //        }

    //        Console.WriteLine("Count of elements in the limited queue: " + queue.Count);
    //        Console.WriteLine("Average of elements in the limited queue: " + queue.CalculateAverage());
    //    }
    //}



    //public class MiningSideCounter
    //    {
    //        public int CountMiningSides(ISource source)
    //        {


    //            Position sourcePos = source.RoomPosition.Position;
    //            int miningSides = 0;

    //            // Define adjacent positions
    //            Position[] adjacentPositions = new Position[]
    //            {
    //            new Position(0, -1),
    //            new Position(1, 0),
    //            new Position(0, 1),
    //            new Position(-1, 0),
    //            };

    //            // Check each adjacent position
    //            foreach (var posOffset in adjacentPositions)
    //            {
    //                int posX = sourcePos.X + posOffset.X;
    //                int posY = sourcePos.Y + posOffset.Y;
    //                Position position = new(posX, posY);

    //                // Check if the position is unblocked (no structures) and exists in the room
    //                if (position.Equals(sourcePos) ||
    //                    (position.X >= 0 &&
    //                     position.Y >= 0 &&
    //                     position.X <= 49 &&
    //                     position.Y <= 49 &&
    //                     !position.LookFor(LOOK_STRUCTURES).Any()))
    //                {
    //                    miningSides++;
    //                }
    //            }

    //            return miningSides;
    //        }
    //    }

}