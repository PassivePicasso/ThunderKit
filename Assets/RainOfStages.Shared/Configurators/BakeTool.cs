using RoR2;
using System;
using System.Diagnostics;
using System.Linq;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Jobs;
using Debug = UnityEngine.Debug;

namespace RainOfStages.Bake
{
    public class BakeTool
    {
        private static NativeArray<float3> tpCheckOffsets;

        private static Func<RaycastHit, HitData> RaycastHitToHitData = rh => new HitData { hit = rh.collider != null ? 1 : 0, distance = rh.distance };

        private static HullData humanData = new HullData
        {
            hullMask = (int)HullMask.Human,
            height = 2f,
            radius = 0.5f
        };

        private static HullData golemData = new HullData
        {
            hullMask = (int)HullMask.Golem,
            height = 8f,
            radius = 1.8f
        };

        private static HullData queenData = new HullData
        {
            hullMask = (int)HullMask.BeetleQueen,
            height = 20f,
            radius = 5f
        };

        public static (Node[] nodes, Link[] links) Bake(Node[] inputNodes, string graphType)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            NativeArray<Node> nativeNodes = new NativeArray<Node>(inputNodes.Length, Allocator.TempJob);
            NativeArray<Link> nativeLinks = new NativeArray<Link>(inputNodes.Length * inputNodes.Length, Allocator.TempJob);

            GenerateTPOffsets();

            for (int i = 0; i < nativeNodes.Length; i++)
                nativeNodes[i] = inputNodes[i];

            Debug.Log($"{graphType} Nodes: {nativeNodes.Length}");

            BakeNodes(stopwatch, nativeNodes, nativeLinks, $"{graphType}: ");

            var results = new { nodes = nativeNodes.ToArray(), links = nativeLinks.ToArray() };

            nativeNodes.Dispose();
            nativeLinks.Dispose();

            tpCheckOffsets.Dispose();

            stopwatch.Stop();

            return (results.nodes, results.links);
        }

        private static void BakeNodes(Stopwatch stopwatch, NativeArray<Node> _nodes, NativeArray<Link> _links, String logPrefix = "")
        {
            int loopSize = 10;

            long timer = stopwatch.ElapsedMilliseconds;
            LogWatch(stopwatch, ref timer, logPrefix + "Start: ");

            var linkGen = new GenerateLinks
            {
                count = _nodes.Length,
                Links = _links
            };
            var linkGenHandle = linkGen.Schedule(_links.Length, loopSize);
            //linkGenHandle.Complete();
            //LogWatch(stopwatch, ref timer, logPrefix + "Node link gen: ");


            NativeArray<RaycastCommand> _losRays = new NativeArray<RaycastCommand>(_links.Length, Allocator.TempJob);
            var losRayGen = new LOSRayGen
            {
                count = _nodes.Length,
                _links = _links,
                _nodes = _nodes,
                _rays = _losRays,
                mask = LayerMask.GetMask("World")
            };
            var losGenHandle = losRayGen.Schedule(_links.Length, loopSize, linkGenHandle);
            //losGenHandle.Complete();
            //LogWatch(stopwatch, ref timer, logPrefix + "LOS rays gen: ");


            NativeArray<RaycastHit> _losCastHits = new NativeArray<RaycastHit>(_links.Length, Allocator.TempJob);
            var losCastHandle = RaycastCommand.ScheduleBatch(_losRays, _losCastHits, loopSize, losGenHandle);
            losCastHandle.Complete();
            //LogWatch(stopwatch, ref timer, logPrefix + "LOS rays cast: ");

            NativeArray<HitData> _losCastHitData = new NativeArray<HitData>(_losCastHits.Select(RaycastHitToHitData).ToArray(), Allocator.TempJob);

            var losRead = new LOSRayRead
            {
                _hitRes = _losCastHitData,
                _links = _links,
            };
            var losReadHandle = losRead.Schedule(_links.Length, loopSize, losCastHandle);
            losReadHandle.Complete();
            LogWatch(stopwatch, ref timer, logPrefix + "LOS rays read: ");


            NativeArray<RaycastCommand> _skyRays = new NativeArray<RaycastCommand>(_nodes.Length, Allocator.TempJob);
            var skyRayGen = new SkyRayGen
            {
                _nodes = _nodes,
                _rays = _skyRays,
                layerMask = LayerMask.GetMask("World")
            };
            var skyGenHandle = skyRayGen.Schedule(_nodes.Length, loopSize);
            //skyGenHandle.Complete();
            //LogWatch(stopwatch, ref timer, logPrefix + "Sky rays gen: ");


            NativeArray<RaycastHit> _skyHits = new NativeArray<RaycastHit>(_nodes.Length, Allocator.TempJob);
            var skyCastHandle = RaycastCommand.ScheduleBatch(_skyRays, _skyHits, loopSize, skyGenHandle);
            skyCastHandle.Complete();
            //LogWatch(stopwatch, ref timer, logPrefix + "Sky rays cast: ");

            NativeArray<HitData> _skyHitData = new NativeArray<HitData>(_skyHits.Select(RaycastHitToHitData).ToArray(), Allocator.TempJob);


            var skyRead = new SkyRayRead
            {
                _hits = _skyHitData,
                _nodes = _nodes
            };
            var skyReadHandle = skyRead.Schedule(_nodes.Length, loopSize, skyCastHandle);
            skyReadHandle.Complete();
            LogWatch(stopwatch, ref timer, logPrefix + "Sky rays read: ");


            NativeArray<RaycastCommand> _tpRays = new NativeArray<RaycastCommand>(_nodes.Length * tpCheckOffsets.Length, Allocator.TempJob);
            var toRayGen = new TPRayGen
            {
                _offsets = tpCheckOffsets,
                range = 10f,
                _nodes = _nodes,
                _rays = _tpRays,
                layerMask = LayerMask.GetMask("World")
            };
            var tpGenHandle = toRayGen.Schedule(_nodes.Length, loopSize);
            //tpGenHandle.Complete();
            //LogWatch(stopwatch, ref timer, logPrefix + "TP rays gen: ");


            NativeArray<RaycastHit> _tpHits = new NativeArray<RaycastHit>(_nodes.Length * tpCheckOffsets.Length, Allocator.TempJob);
            var tpCastHandle = RaycastCommand.ScheduleBatch(_tpRays, _tpHits, loopSize, tpGenHandle);
            tpCastHandle.Complete();
            //LogWatch(stopwatch, ref timer, logPrefix + "TP rays cast: ");
            NativeArray<HitData> _tpHitData = new NativeArray<HitData>(_tpHits.Select(RaycastHitToHitData).ToArray(), Allocator.TempJob);


            var tpRead = new TPRayRead
            {
                offCount = tpCheckOffsets.Length,
                _hits = _tpHitData,
                _nodes = _nodes
            };
            var tpReadHandle = tpRead.Schedule(_nodes.Length, loopSize, tpCastHandle);
            tpReadHandle.Complete();
            LogWatch(stopwatch, ref timer, logPrefix + "TP rays read: ");


            var distChecks = new LinkDistScore
            {
                rangeFactor = (15f * 2f) * (15f * 2f),
                _links = _links,
                _nodes = _nodes
            };
            var distCheckHandle = distChecks.Schedule(_links.Length, loopSize, linkGenHandle);
            distCheckHandle.Complete();
            LogWatch(stopwatch, ref timer, logPrefix + "Link distance scoring: ");


            NativeArray<CapsulecastCommand> _groundCapsules = new NativeArray<CapsulecastCommand>(2 * _links.Length, Allocator.TempJob);
            var groundHumanCastGen = new GroundCastsGen
            {
                _links = _links,
                _nodes = _nodes,
                _caps = _groundCapsules,
                fudge = new float3(0, 1, 0) * 0.01f,
                hull = humanData
            };
            var groundHumanGenHandle = groundHumanCastGen.Schedule(_links.Length, 100, linkGenHandle);
            //groundHumanGenHandle.Complete();
            //LogWatch(stopwatch, ref timer, logPrefix + "Human ground position gen: ");


            NativeArray<RaycastHit> _groundCapsuleHits = new NativeArray<RaycastHit>(2 * _links.Length, Allocator.TempJob);
            var groundHumanCastHandle = CapsulecastCommand.ScheduleBatch(_groundCapsules, _groundCapsuleHits, loopSize, groundHumanGenHandle);
            //groundHumanCastHandle.Complete();
            //LogWatch(stopwatch, ref timer, logPrefix + "Human ground position cast: ");

            NativeArray<HitData> _groundCapsuleHitData = new NativeArray<HitData>(_groundCapsuleHits.Select(RaycastHitToHitData).ToArray(), Allocator.TempJob);
            NativeArray<CapsulecastCommand> _groundCapsules2 = new NativeArray<CapsulecastCommand>(2 * _links.Length, Allocator.TempJob);
            NativeArray<float3> _groundVecs1 = new NativeArray<float3>(2 * _links.Length, Allocator.TempJob);
            var groundHumanCastRead = new GroundCastsReadGen
            {
                _links = _links,
                _nodes = _nodes,
                _hitRes = _groundCapsuleHitData,
                _caps = _groundCapsules2,
                _groundPos = _groundVecs1,
                layerMask = LayerMask.GetMask("World"),
                hull = humanData,
                fudge = new float3(0, 1, 0) * 0.01f
            };
            var groundHumanCastReadHandle = groundHumanCastRead.Schedule(_links.Length, loopSize, groundHumanCastHandle);
            //groundHumanCastReadHandle.Complete();
            //LogWatch(stopwatch, ref timer, logPrefix + "Human ground position read: ");


            NativeArray<RaycastHit> _groundCapsuleHits2 = new NativeArray<RaycastHit>(2 * _links.Length, Allocator.TempJob);
            var groundHumanCastHandle2 = CapsulecastCommand.ScheduleBatch(_groundCapsules2, _groundCapsuleHits2, loopSize, groundHumanCastReadHandle);
            //groundHumanCastHandle2.Complete();
            //LogWatch(stopwatch, ref timer, logPrefix + "Human ground position casts2: ");


            var groundHumanCastRead2 = new GroundCastsReadGen2
            {
                _links = _links,
                _nodes = _nodes,
                _hitRes = _groundCapsuleHits2,
                hull = humanData,
            };
            var groundHumanCastRead2Handle = groundHumanCastRead2.Schedule(_links.Length, loopSize, groundHumanCastHandle2);
            groundHumanCastRead2Handle.Complete();
            LogWatch(stopwatch, ref timer, logPrefix + "Human ground: ");

            NativeArray<CapsulecastCommand> _groundGolemCapsules = new NativeArray<CapsulecastCommand>(2 * _links.Length, Allocator.TempJob);
            var groundGolemCastGen = new GroundCastsGen
            {
                _links = _links,
                _nodes = _nodes,
                _caps = _groundGolemCapsules,
                fudge = new float3(0, 1, 0) * 0.01f,
                hull = golemData
            };
            var groundGolemGenHandle = groundGolemCastGen.Schedule(_links.Length, loopSize, groundHumanCastHandle2);
            //groundGolemGenHandle.Complete();
            //LogWatch(stopwatch, ref timer, logPrefix + "Golem ground position gen: ");


            NativeArray<RaycastHit> _groundGolemCapsuleHits = new NativeArray<RaycastHit>(2 * _links.Length, Allocator.TempJob);
            var groundGolemCastHandle = CapsulecastCommand.ScheduleBatch(_groundGolemCapsules, _groundGolemCapsuleHits, loopSize, groundGolemGenHandle);
            //groundGolemCastHandle.Complete();
            //LogWatch(stopwatch, ref timer, logPrefix + "Golem ground position cast: ");


            NativeArray<HitData> _groundGolemCapsuleHitData = new NativeArray<HitData>(_groundCapsuleHits.Select(RaycastHitToHitData).ToArray(), Allocator.TempJob);
            NativeArray<CapsulecastCommand> _groundGolemCapsules2 = new NativeArray<CapsulecastCommand>(2 * _links.Length, Allocator.TempJob);
            NativeArray<float3> _groundGolemVecs1 = new NativeArray<float3>(2 * _links.Length, Allocator.TempJob);
            var groundGolemCastRead = new GroundCastsReadGen
            {
                _links = _links,
                _nodes = _nodes,
                _hitRes = _groundGolemCapsuleHitData,
                _caps = _groundGolemCapsules2,
                _groundPos = _groundGolemVecs1,
                layerMask = LayerMask.GetMask("World"),
                hull = golemData,
                fudge = new float3(0, 1, 0) * 0.01f
            };
            var groundGolemCastReadHandle = groundGolemCastRead.Schedule(_links.Length, loopSize, groundGolemCastHandle);
            //groundGolemCastReadHandle.Complete();
            //LogWatch(stopwatch, ref timer, logPrefix + "Golem ground position read: ");


            NativeArray<RaycastHit> _groundGolemCapsuleHits2 = new NativeArray<RaycastHit>(2 * _links.Length, Allocator.TempJob);
            var groundGolemCastHandle2 = CapsulecastCommand.ScheduleBatch(_groundGolemCapsules2, _groundGolemCapsuleHits2, loopSize, groundGolemCastReadHandle);
            //groundGolemCastHandle2.Complete();
            //LogWatch(stopwatch, ref timer, logPrefix + "Golem ground position casts2: ");


            var groundGolemCastRead2 = new GroundCastsReadGen2
            {
                _links = _links,
                _nodes = _nodes,
                _hitRes = _groundGolemCapsuleHits2,
                hull = golemData,
            };
            var groundGolemCastRead2Handle = groundGolemCastRead2.Schedule(_links.Length, loopSize, groundGolemCastHandle2);
            groundGolemCastRead2Handle.Complete();
            LogWatch(stopwatch, ref timer, logPrefix + "Golem ground: ");

            NativeArray<CapsulecastCommand> _groundQueenCapsules = new NativeArray<CapsulecastCommand>(2 * _links.Length, Allocator.TempJob);
            var groundQueenCastGen = new GroundCastsGen
            {
                _links = _links,
                _nodes = _nodes,
                _caps = _groundQueenCapsules,
                fudge = new float3(0, 1, 0) * 0.01f,
                hull = queenData
            };
            var groundQueenGenHandle = groundQueenCastGen.Schedule(_links.Length, loopSize, groundHumanCastHandle2);
            //groundQueenGenHandle.Complete();
            //LogWatch(stopwatch, ref timer, logPrefix + "Queen ground position gen: ");


            NativeArray<RaycastHit> _groundQueenCapsuleHits = new NativeArray<RaycastHit>(2 * _links.Length, Allocator.TempJob);
            var groundQueenCastHandle = CapsulecastCommand.ScheduleBatch(_groundQueenCapsules, _groundQueenCapsuleHits, loopSize, groundQueenGenHandle);
            //groundQueenCastHandle.Complete();
            //LogWatch(stopwatch, ref timer, logPrefix + "Queen ground position cast: ");


            NativeArray<HitData> _groundQueenCapsuleHitData = new NativeArray<HitData>(_groundCapsuleHits.Select(RaycastHitToHitData).ToArray(), Allocator.TempJob);
            NativeArray<CapsulecastCommand> _groundQueenCapsules2 = new NativeArray<CapsulecastCommand>(2 * _links.Length, Allocator.TempJob);
            NativeArray<float3> _groundQueenVecs1 = new NativeArray<float3>(2 * _links.Length, Allocator.TempJob);
            var groundQueenCastRead = new GroundCastsReadGen
            {
                _links = _links,
                _nodes = _nodes,
                _hitRes = _groundQueenCapsuleHitData,
                _caps = _groundQueenCapsules2,
                _groundPos = _groundQueenVecs1,
                layerMask = LayerMask.GetMask("World"),
                hull = queenData,
                fudge = new float3(0, 1, 0) * 0.01f
            };
            var groundQueenCastReadHandle = groundQueenCastRead.Schedule(_links.Length, loopSize, groundQueenCastHandle);
            //groundQueenCastReadHandle.Complete();
            //LogWatch(stopwatch, ref timer, logPrefix + "Queen ground position read: ");


            NativeArray<RaycastHit> _groundQueenCapsuleHits2 = new NativeArray<RaycastHit>(2 * _links.Length, Allocator.TempJob);
            var groundQueenCastHandle2 = CapsulecastCommand.ScheduleBatch(_groundQueenCapsules2, _groundQueenCapsuleHits2, loopSize, groundQueenCastReadHandle);
            //groundQueenCastHandle2.Complete();
            //LogWatch(stopwatch, ref timer, logPrefix + "Queen ground position casts2: ");


            var groundQueenCastRead2 = new GroundCastsReadGen2
            {
                _links = _links,
                _nodes = _nodes,
                _hitRes = _groundQueenCapsuleHits2,
                hull = queenData,
            };
            var groundQueenCastRead2Handle = groundQueenCastRead2.Schedule(_links.Length, loopSize, groundQueenCastHandle2);
            groundQueenCastRead2Handle.Complete();
            LogWatch(stopwatch, ref timer, logPrefix + "Queen ground: ");



            // TODO: Slope
            // TODO: Hull

            _losRays.Dispose();
            _losCastHits.Dispose();
            _skyRays.Dispose();
            _skyHits.Dispose();
            _tpRays.Dispose();
            _tpHits.Dispose();
            _groundCapsules.Dispose();
            _groundCapsuleHits.Dispose();
            _groundCapsules2.Dispose();
            _groundCapsuleHits2.Dispose();
            _groundVecs1.Dispose();
            _groundGolemCapsules.Dispose();
            _groundGolemCapsuleHits.Dispose();
            _groundGolemCapsules2.Dispose();
            _groundGolemCapsuleHits2.Dispose();
            _groundGolemVecs1.Dispose();
            _groundQueenCapsules.Dispose();
            _groundQueenCapsuleHits.Dispose();
            _groundQueenCapsules2.Dispose();
            _groundQueenCapsuleHits2.Dispose();
            _groundQueenVecs1.Dispose();
        }

        private static void LogWatch(Stopwatch stopwatch, ref Int64 timer, String prefix = "")
        {
            stopwatch.Stop();
            Debug.Log(prefix + (stopwatch.ElapsedMilliseconds - timer));
            timer = stopwatch.ElapsedMilliseconds;
            stopwatch.Start();
        }

        private static void GenerateTPOffsets()
        {
            int numberOfChecks = 20;

            float radius = 15f;
            float verticalOffset = 7f;
            float angleStep = 360f / (float)numberOfChecks;

            tpCheckOffsets = new NativeArray<float3>(numberOfChecks, Allocator.Persistent);

            for (int i = 0; i < numberOfChecks; i++)
            {
                float3 b = Quaternion.AngleAxis(angleStep * (float)i, new float3(0, 1, 0)) * (new float3(0, 0, 1) * radius);
                float3 origin = b + new float3(0, 1, 0) * verticalOffset;
                tpCheckOffsets[i] = origin;
            }
        }

    }
}
