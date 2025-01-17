﻿using System;
using BenchmarkDotNet.Attributes;
using DefaultEcs.System;
using DefaultEcs.Threading;
using Entitas;
using Leopotam.Ecs;
using Leopotam.Ecs.Threads;
using Microsoft.Xna.Framework;
using MonoGame.Extended.Entities;
using MonoGame.Extended.Entities.Systems;
using Svelto.ECS;
using Svelto.ECS.Schedulers;
using DefaultEntity = DefaultEcs.Entity;
using DefaultEntitySet = DefaultEcs.EntitySet;
using DefaultWorld = DefaultEcs.World;
using EntitasEntity = Entitas.Entity;
using EntitasWorld = Entitas.IContext<Entitas.Entity>;
using LeoEntity = Leopotam.Ecs.EcsEntity;
using LeoSystems = Leopotam.Ecs.EcsSystems;
using LeoWorld = Leopotam.Ecs.EcsWorld;
using MonoEntity = MonoGame.Extended.Entities.Entity;
using MonoWorld = MonoGame.Extended.Entities.World;

namespace DefaultEcs.Benchmark.Performance
{
    [MemoryDiagnoser]
    public partial class SingleComponentEntityEnumeration
    {
        private struct DefaultComponent
        {
            public int Value;
        }

        private sealed class DefaultEcsSystem : AEntitySetSystem<int>
        {
            public DefaultEcsSystem(DefaultWorld world, IParallelRunner runner)
                : base(world.GetEntities().With<DefaultComponent>().AsSet(), runner)
            { }

            public DefaultEcsSystem(DefaultWorld world)
                : this(world, null)
            { }

            protected override void Update(int state, ReadOnlySpan<DefaultEntity> entities)
            {
                foreach (ref readonly DefaultEntity entity in entities)
                {
                    ++entity.Get<DefaultComponent>().Value;
                }
            }
        }

        private sealed class DefaultEcsEntityComponentSystem : AEntitySetSystem<int>
        {
            private readonly DefaultWorld _world;

            public DefaultEcsEntityComponentSystem(DefaultWorld world, IParallelRunner runner)
                : base(world.GetEntities().With<DefaultComponent>().AsSet(), runner)
            {
                _world = world;
            }

            public DefaultEcsEntityComponentSystem(DefaultWorld world)
                : this(world, null)
            { }

            protected override void Update(int state, ReadOnlySpan<DefaultEntity> entities)
            {
                Components<DefaultComponent> components = _world.GetComponents<DefaultComponent>();

                foreach (ref readonly DefaultEntity entity in entities)
                {
                    ++components[entity].Value;
                }
            }
        }

        private sealed class DefaultEcsComponentSystem : AComponentSystem<int, DefaultComponent>
        {
            public DefaultEcsComponentSystem(DefaultWorld world, IParallelRunner runner)
                : base(world, runner)
            { }

            public DefaultEcsComponentSystem(DefaultWorld world)
                : this(world, null)
            { }

            protected override void Update(int state, Span<DefaultComponent> components)
            {
                foreach (ref DefaultComponent component in components)
                {
                    ++component.Value;
                }
            }
        }

        private sealed partial class DefaultEcsGeneratorSystem : AEntitySetSystem<int>
        {
            [Update]
            private static void Update(ref DefaultComponent component)
            {
                ++component.Value;
            }
        }

        private class EntitasComponent : IComponent
        {
            public int Value;
        }

        public class EntitasSystem : JobSystem<EntitasEntity>
        {
            public EntitasSystem(EntitasWorld world, int threadCount) : base(world.GetGroup(Matcher<EntitasEntity>.AllOf(0)), threadCount)
            { }

            public EntitasSystem(EntitasWorld world) : this(world, 1)
            { }

            protected override void Execute(EntitasEntity entity)
            {
                EntitasComponent component = (EntitasComponent)entity.GetComponent(0);
                ++component.Value;
            }
        }

        private class MonoComponent
        {
            public int Value;
        }

        public class MonoSystem : EntityUpdateSystem
        {
            private ComponentMapper<MonoComponent> _components;

            public MonoSystem()
                : base(Aspect.All(typeof(MonoComponent)))
            { }

            public override void Initialize(IComponentMapperService mapperService)
            {
                _components = mapperService.GetMapper<MonoComponent>();
            }

            public override void Update(GameTime gameTime)
            {
                foreach (int entityId in ActiveEntities)
                {
                    ++_components.Get(entityId).Value;
                }
            }
        }

        private struct LeoComponent
        {
            public int Value;
        }

        private sealed class LeoSystem : IEcsRunSystem
        {
            private readonly EcsFilter<LeoComponent> _filter = null;

            public void Run()
            {
                for (int i = 0, iMax = _filter.GetEntitiesCount(); i < iMax; i++)
                {
                    ++_filter.Get1(i).Value;
                }
            }
        }

        private sealed class LeoMultiSystem : EcsMultiThreadSystem<EcsFilter<LeoComponent>>
        {
            private readonly EcsFilter<LeoComponent> _filter = null;

            protected override EcsFilter<LeoComponent> GetFilter() => _filter;

            protected override int GetMinJobSize() => 0;

            protected override int GetThreadsCount() => Environment.ProcessorCount - 1;

            protected override EcsMultiThreadWorker GetWorker() => Worker;

            private static void Worker(EcsMultiThreadWorkerDesc workerDesc)
            {
                foreach (var i in workerDesc)
                {
                    ++workerDesc.Filter.Get1(i).Value;
                }
            }
        }

        private struct SveltoComponent : IEntityComponent
        {
            public int Value;
        }

        private sealed class SveltoEntity : GenericEntityDescriptor<SveltoComponent>
        { }

        private sealed class SveltoSystem : IQueryingEntitiesEngine
        {
            public EntitiesDB entitiesDB { get; set; }

            public void Ready()
            { }

            public void Update()
            {
                var (entityViews, count) = entitiesDB.QueryEntities<SveltoComponent>(_sveltoGroup);

                for (var i = 0; i < count; i++)
                {
                    ++entityViews[i].Value;
                }
            }
        }

        private static readonly ExclusiveGroup _sveltoGroup = new();

        private DefaultWorld _defaultWorld;
        private DefaultParallelRunner _defaultRunner;
        private DefaultEntitySet _defaultEntitySet;
        private DefaultEcsSystem _defaultSystem;
        private DefaultEcsSystem _defaultMultiSystem;
        private DefaultEcsEntityComponentSystem _defaultEntityComponentSystem;
        private DefaultEcsEntityComponentSystem _defaultMultiEntityComponentSystem;
        private DefaultEcsComponentSystem _defaultComponentSystem;
        private DefaultEcsComponentSystem _defaultComponentMultiSystem;
        private DefaultEcsGeneratorSystem _defaultGeneratorSystem;
        private DefaultEcsGeneratorSystem _defaultGeneratorMultiSystem;

        private EntitasWorld _entitasWorld;
        private EntitasSystem _entitasSystem;
        private EntitasSystem _entitasMultiSystem;

        private MonoWorld _monoWorld;
        private GameTime _time;

        private LeoWorld _leoWorld;
        private LeoSystems _leoSystems;
        private LeoSystem _leoSystem;
        private IEcsRunSystem _leoMultiSystem;

        private EnginesRoot _sveltoWorld;
        private SveltoSystem _sveltoSystem;

        [Params(100000)]
        public int EntityCount { get; set; }

        [GlobalSetup]
        public void Setup()
        {
            _defaultWorld = new DefaultWorld(EntityCount);
            _defaultEntitySet = _defaultWorld.GetEntities().With<DefaultComponent>().AsSet();
            _defaultRunner = new DefaultParallelRunner(Environment.ProcessorCount);
            _defaultSystem = new DefaultEcsSystem(_defaultWorld);
            _defaultMultiSystem = new DefaultEcsSystem(_defaultWorld, _defaultRunner);
            _defaultEntityComponentSystem = new DefaultEcsEntityComponentSystem(_defaultWorld);
            _defaultMultiEntityComponentSystem = new DefaultEcsEntityComponentSystem(_defaultWorld, _defaultRunner);
            _defaultComponentSystem = new DefaultEcsComponentSystem(_defaultWorld);
            _defaultComponentMultiSystem = new DefaultEcsComponentSystem(_defaultWorld, _defaultRunner);
            _defaultGeneratorSystem = new DefaultEcsGeneratorSystem(_defaultWorld);
            _defaultGeneratorMultiSystem = new DefaultEcsGeneratorSystem(_defaultWorld, _defaultRunner);

            _entitasWorld = new Context<EntitasEntity>(1, () => new EntitasEntity());
            _entitasSystem = new EntitasSystem(_entitasWorld);
            _entitasMultiSystem = new EntitasSystem(_entitasWorld, Environment.ProcessorCount);

            _monoWorld = new WorldBuilder().AddSystem(new MonoSystem()).Build();
            _time = new GameTime();

            _leoWorld = new LeoWorld();
            _leoSystem = new LeoSystem();
            _leoMultiSystem = new LeoMultiSystem();
            _leoSystems = new LeoSystems(_leoWorld).Add(_leoSystem).Add(_leoMultiSystem);
            _leoSystems.ProcessInjects().Init();

            SimpleEntitiesSubmissionScheduler sveltoScheduler = new();
            _sveltoWorld = new EnginesRoot(sveltoScheduler);
            _sveltoSystem = new SveltoSystem();
            _sveltoWorld.AddEngine(_sveltoSystem);
            IEntityFactory sveltoFactory = _sveltoWorld.GenerateEntityFactory();

            for (int i = 0; i < EntityCount; ++i)
            {
                DefaultEntity defaultEntity = _defaultWorld.CreateEntity();
                defaultEntity.Set<DefaultComponent>();

                EntitasEntity entitasEntity = _entitasWorld.CreateEntity();
                entitasEntity.AddComponent(0, new EntitasComponent());

                MonoEntity monoEntity = _monoWorld.CreateEntity();
                monoEntity.Attach(new MonoComponent());

                LeoEntity leoEntity = _leoWorld
                    .NewEntity()
                    .Replace(new LeoComponent());

                sveltoFactory.BuildEntity<SveltoEntity>((uint)i, _sveltoGroup);
            }

            sveltoScheduler.SubmitEntities();
        }

        [GlobalCleanup]
        public void Cleanup()
        {
            _defaultRunner.Dispose();
            _defaultWorld.Dispose();
        }

        [Benchmark]
        public void DefaultEcs_EntitySet()
        {
            foreach (ref readonly DefaultEntity entity in _defaultEntitySet.GetEntities())
            {
                ++entity.Get<DefaultComponent>().Value;
            }
        }

        [Benchmark]
        public void DefaultEcs_System() => _defaultSystem.Update(42);

        [Benchmark]
        public void DefaultEcs_MultiSystem() => _defaultMultiSystem.Update(42);

        [Benchmark]
        public void DefaultEcs_EntityComponentSystem() => _defaultEntityComponentSystem.Update(42);

        [Benchmark]
        public void DefaultEcs_MultiEntityComponentSystem() => _defaultMultiEntityComponentSystem.Update(42);

        [Benchmark]
        public void DefaultEcs_Component()
        {
            foreach (ref DefaultComponent component in _defaultWorld.Get<DefaultComponent>())
            {
                ++component.Value;
            }
        }

        [Benchmark]
        public void DefaultEcs_ComponentSystem() => _defaultComponentSystem.Update(42);

        [Benchmark]
        public void DefaultEcs_ComponentMultiSystem() => _defaultComponentMultiSystem.Update(42);

        [Benchmark]
        public void DefaultEcs_GeneratorSystem() => _defaultGeneratorSystem.Update(42);

        [Benchmark]
        public void DefaultEcs_GeneratorMultiSystem() => _defaultGeneratorMultiSystem.Update(42);

        [Benchmark]
        public void Entitas_System() => _entitasSystem.Execute();

        [Benchmark]
        public void Entitas_MultiSystem() => _entitasMultiSystem.Execute();

        [Benchmark]
        public void MonoGameExtendedEntities_System() => _monoWorld.Update(_time);

        [Benchmark]
        public void Leo_System() => _leoSystem.Run();

        [Benchmark]
        public void Leo_MultiSystem() => _leoMultiSystem.Run();

        [Benchmark]
        public void Svelto_System() => _sveltoSystem.Update();
    }
}
