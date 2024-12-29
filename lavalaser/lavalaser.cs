using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MCGalaxy;
using MCGalaxy.Blocks.Physics;
using MCGalaxy.Events.LevelEvents;
using MCGalaxy.Events.PlayerEvents;
using MCGalaxy.Levels.IO;
using MCGalaxy.Maths;
using MCGalaxy.Tasks;
using BlockID = System.UInt16;

namespace lavalaser
{
    public sealed class LavaLaser : Plugin
    {
        public override string name { get { return "lavalaser"; } }
        public override string MCGalaxy_Version { get { return "1.9.5.1"; } }
        //This name is used to determine who to send debug text to
        static string author = "Bruceja";
        public override string creator { get { return author; } }

        //The level we want to add a custom physics block to.
        static string physicsLevelName = "bruceja5";
        //Block that sets off the laser
        static BlockID igniteBlock = 13;
        //Block that the laser is made out of
        static BlockID lavaLaserBlock = 11;
        static ushort maxLaserLength = 8;       



        //You can find the server-side block ID in a custom command with:
        //Vec3S32 pos = p.Pos.FeetBlockCoords;
        //p.Message("Server-side BlockID at this location is {0}", p.level.GetBlock((ushort)pos.X, (ushort)pos.Y, (ushort)pos.Z));

        public override void Load(bool startup)
        {
            OnBlockChangedEvent.Register(OnBlockPlaced, Priority.Low);
        }
        public override void Unload(bool shutdown)
        {
            OnBlockChangedEvent.Unregister(OnBlockPlaced);
        }
             
        static void OnBlockPlaced(Player p, ushort x, ushort y, ushort z, ChangeResult result)
        {
            if (result == ChangeResult.Modified)
            {
                BlockID newBlock = p.level.GetBlock(x, y, z);
                if (newBlock == igniteBlock)
                {
                    int index = p.level.PosToInt(x, y, z);
                    Vec3U16 pos = new Vec3U16();
                    pos.X = x; pos.Y = y; pos.Z = z;
                    List<int> laserBlockIndexes = new List<int>();
                    string direction = GetPlayerDirection(p, p.Rot.RotY);

                    //Remove gravel
                    p.level.AddUpdate(index, Block.Air);

                    //Place line of lava blocks
                    for (int i = 0; i < maxLaserLength; i++)
                    {                      
                        switch (direction)
                        {
                            case "North":
                                pos.Z--;
                                break;
                            case "East":
                                pos.X++;
                                break;
                            case "South":
                                pos.Z++;
                                break;

                            default:
                                pos.X--;
                                break;
                        }                       
                        index = p.level.PosToInt(pos.X, pos.Y, pos.Z);

                        if (p.level.GetBlock(pos.X, pos.Y, pos.Z) != Block.Air)
                        {
                            break;
                        }

                        p.level.AddUpdate(index, lavaLaserBlock);
                        laserBlockIndexes.Add(index);
                    }

                    //Remove laser
                    SchedulerTask task = Server.MainScheduler.QueueOnce(_ =>
                    {
                        foreach (int laserBlockIndex in laserBlockIndexes)
                        {
                            p.level.AddUpdate(laserBlockIndex, Block.Air);
                        }
                    }, null, TimeSpan.FromMilliseconds(300));

                    if (newBlock != Block.Air)
                    {
                        p.Message("{4} placed block ID {0} at ({1}, {2}, {3})", newBlock, x, y, z, p.ColoredName);
                        Logger.Log(LogType.UserActivity, $"{p.ColoredName} placed block {newBlock} at {x}, {y}, {z}");
                    }                  
                    
                }

            }
        }

        static string GetPlayerDirection(Player p, int yaw)
        {
            string direction;
                    
            if (yaw >= 223 || yaw <= 31)
            {
                direction = "North";
            }

            else if (yaw >= 32 && yaw <= 95)
            {
                direction = "East";
            }

            else if (yaw >= 95 && yaw <= 158)
            {
                direction = "South";
            }

            else
            {
                direction = "West";
            }           

            p.Message($"Player Direction: {direction}");
            return direction;
        }


        static void MsgDebugger(string message, params object[] args)
        {
            Player debugger = PlayerInfo.FindExact(LavaLaser.author); if (debugger == null) { return; }
            debugger.Message(message, args);
        }
    }
}

