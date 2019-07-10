using NUnit.Framework;
using System.Linq;
using Unity.Entities;

namespace HMH.ECS.IsoSorting
{
    public class ECSTestsFixture
    {
        protected World m_PreviousWorld;
        protected World World;
        protected EntityManager _entityManager;
        protected EntityManager.EntityManagerDebug _entityManagerDebug;

        protected int StressTestEntityCount = 1000;

        [SetUp]
        public virtual void Setup()
        {
            m_PreviousWorld = World.Active;
            World = World.Active = new World("Test World");

            _entityManager = World.EntityManager;
            _entityManagerDebug = new EntityManager.EntityManagerDebug(_entityManager);
            
#if !UNITY_DOTSPLAYER
#if !UNITY_2019_2_OR_NEWER
            // Not raising exceptions can easily bring unity down with massive logging when tests fail.
            // From Unity 2019.2 on this field is always implicitly true and therefore removed.

            UnityEngine.Assertions.Assert.raiseExceptions = true;
#endif  // #if !UNITY_2019_2_OR_NEWER
#endif  // #if !UNITY_DOTSPLAYER
        }

        [TearDown]
        public virtual void TearDown()
        {
            if (_entityManager != null && _entityManager.IsCreated)
            {
                // Clean up systems before calling CheckInternalConsistency because we might have filters etc
                // holding on SharedComponentData making checks fail
                while (World.Systems.ToArray().Length > 0)
                {
                    World.DestroySystem(World.Systems.ToArray()[0]);
                }

                _entityManagerDebug.CheckInternalConsistency();

                World.Dispose();
                World = null;

                World.Active = m_PreviousWorld;
                m_PreviousWorld = null;
                _entityManager = null;
            }
        }
    }
}
